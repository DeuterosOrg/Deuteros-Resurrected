#!/usr/bin/env python3
"""Build and exercise the real Godot/C# game; fail on logged engine errors too."""
import argparse
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
ANSI = re.compile(r"\x1b\[[0-9;]*m")
EDITOR_TEARDOWN = 'ERROR: Condition "!EditorSettings::get_singleton() || !EditorSettings::get_singleton()->has_setting(p_setting)" is true. Returning: Variant()'


def check_output(text, marker=None, editor=False):
    """Godot can log errors and still exit zero. A success marker alone is insufficient."""
    text = ANSI.sub("", text)
    errors = []
    for line in text.splitlines():
        if editor and line.strip() == EDITOR_TEARDOWN:
            continue
        if re.match(r"\s*(ERROR:|SCRIPT ERROR:|FAIL:|FATAL:|handle_crash: Program crashed|Unhandled exception)", line):
            errors.append(line)
    if marker and marker not in text:
        errors.append("Missing completion marker: " + marker)
    if errors:
        raise RuntimeError("\n".join(errors))
    return text


def run(name, command, logs, marker=None, importing=False, timeout=180, bootstrap=False, editor=False, test_case=None):
    print(f"[{name}] {' '.join(map(str, command))}", flush=True)
    env = os.environ.copy()
    env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
    if importing:
        env["DEUTEROS_IMPORT_ONLY"] = "1"
    else:
        env.pop("DEUTEROS_IMPORT_ONLY", None)
    if test_case is None:
        env.pop("DEUTEROS_TEST_CASE", None)
    else:
        env["DEUTEROS_TEST_CASE"] = str(test_case)
    log = logs / (name + ".log")
    with log.open("w", encoding="utf-8") as output:
        try:
            result = subprocess.run(command, cwd=ROOT, env=env, stdout=output,
                                    stderr=subprocess.STDOUT, timeout=timeout)
        except subprocess.TimeoutExpired as error:
            raise RuntimeError(f"{name} timed out after {timeout}s; see {log}") from error
    text = log.read_text(encoding="utf-8", errors="replace")
    # A fresh cache cannot load the global font/theme before its first import.
    # Preserve that pass's diagnostics, then require a separate strict import.
    try:
        clean = check_output(text, marker, importing or editor) if not bootstrap else ANSI.sub("", text)
    except RuntimeError as error:
        raise RuntimeError(f"{name} failed; see {log}\n{error}") from error
    if bootstrap and marker not in clean:
        raise RuntimeError(f"{name} did not finish; see {log}")
    if result.returncode:
        raise RuntimeError(f"{name} exited {result.returncode}; see {log}\n" + "\n".join(clean.splitlines()[-20:]))
    if (importing or editor) and EDITOR_TEARDOWN in clean:
        print("  Known Godot 4.2.2 editor teardown diagnostic recorded; gameplay errors are not exempt.")
    for line in clean.splitlines():
        if any(value in line for value in ("Warning(s)", "Error(s)", "REGRESSION RESULT:", "SMOKE OK", "IMPORT OK")):
            print("  " + line.strip())
    return clean


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--godot", default=os.environ.get("GODOT", "godot"), help="Godot 4.2.2 .NET executable")
    parser.add_argument("--skip-import", action="store_true", help="Use an already imported project during development")
    parser.add_argument("--export-windows", action="store_true", help="Also build a Windows release (matching .NET export templates required)")
    args = parser.parse_args()
    godot = shutil.which(args.godot)
    dotnet = shutil.which("dotnet")
    if not godot or not dotnet:
        parser.error("Install Godot 4.2.2 .NET and .NET SDK 6.0.428; put both on PATH or supply --godot. See README.md.")
    version = subprocess.check_output([godot, "--version"], text=True).strip()
    if not version.startswith("4.2.2.stable.mono"):
        parser.error("Expected Godot 4.2.2 .NET, found " + version)
    # The Windows installer puts rcedit beside Godot. Child export processes
    # must find it even when Godot was supplied by absolute path or GODOT.
    if sys.platform == "win32":
        os.environ["PATH"] = str(Path(godot).parent) + os.pathsep + os.environ.get("PATH", "")
    logs = ROOT / "artifacts" / "validation"
    logs.mkdir(parents=True, exist_ok=True)
    run("build", [dotnet, "build", "Godot/Deuteros.csproj", "--nologo"], logs)
    if not args.skip_import:
        if not (ROOT / "Godot" / ".godot" / "editor" / "filesystem_cache8").exists():
            run("import-bootstrap", [godot, "--headless", "--editor", "--path", "Godot"], logs,
                marker="IMPORT OK", importing=True, bootstrap=True)
            print("  First-import diagnostics saved; checking the populated cache strictly next.")
        run("import", [godot, "--headless", "--editor", "--path", "Godot"], logs,
            marker="IMPORT OK", importing=True)
    command = [godot, "--headless", "--path", "Godot", "res://Tests/Regression.tscn"]
    summary = logs / "regression-summary.log"
    summary.unlink(missing_ok=True)
    discovery = run("regression-list", command, logs, marker="TEST CASE:", test_case="list")
    cases = re.findall(r"^TEST CASE: (\d+): (.+)$", discovery, re.MULTILINE)
    count = re.search(r"^TEST CASE COUNT: (\d+)$", discovery, re.MULTILINE)
    if not cases or not count or int(count.group(1)) != len(cases) or len({case_id for case_id, _ in cases}) != len(cases):
        raise RuntimeError("Regression discovery returned missing or duplicate cases")
    for case_id, name in cases:
        print("  Testing: " + name, flush=True)
        run("regression-" + case_id.zfill(3), command, logs,
            marker="REGRESSION RESULT: 1 passed, 0 failed", test_case=case_id)
    result = f"REGRESSION RESULT: {len(cases)} passed, 0 failed (fresh process per case)"
    summary.write_text(result + "\n", encoding="utf-8")
    print(result, flush=True)
    run("smoke", [godot, "--headless", "--path", "Godot", "--script", "res://Tests/Smoke.gd"], logs,
        marker="SMOKE OK")
    if args.export_windows:
        target = ROOT / "artifacts" / "windows" / "Deuteros.exe"
        target.parent.mkdir(parents=True, exist_ok=True)
        target.unlink(missing_ok=True)
        run("export-windows", [godot, "--headless", "--path", "Godot", "--export-release",
                               "Windows Desktop", str(target)], logs, marker="savepack: end", timeout=600, editor=True)
        if not target.is_file() or target.stat().st_size == 0:
            raise RuntimeError("Export produced no executable: " + str(target))
        print("Windows export: " + str(target))
    print("Validation passed. Logs: " + str(logs))


if __name__ == "__main__":
    try:
        main()
    except (RuntimeError, OSError, subprocess.CalledProcessError) as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
