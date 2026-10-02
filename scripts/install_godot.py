#!/usr/bin/env python3
"""Install pinned, checksum-verified Godot .NET binaries without admin rights."""
import argparse
import hashlib
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import urllib.request
import zipfile

VERSION = "4.2.2-stable"
BASE = "https://github.com/godotengine/godot/releases/download/" + VERSION + "/"
# Published SHA512-SUMS.txt for the official release; pins downloads independently.
HASHES = {
    "mono_linux_x86_64.zip": "9e38f41785c92c27ce08d5f197be66e167fc5b402d345c6bd06968deecce276b2817d1eabb96c17d3bd21759403f7c29f1cf6acf6f52062786ad84fa507b30b5",
    "mono_win64.zip": "d6bf8ae60c4e83e89db5a8da74ce56464f8890736e40a18a3c3e593aefb8c755f2fa4992ee24be1074ccaed7b7d12a22342d9a7d576648255bf0c489227ecd5f",
    "mono_macos.universal.zip": "44d3826eee79906677ac008b04309e5fbf7e0d0217dfbbbd39be512538b4e825cc0833ffc2ca881c6f696812b5f96d59bd6c7b2bdb5c4bfe45ac2e5a08fa85b1",
    "mono_export_templates.tpz": "b1639a80409be8a07d02d8f1aacfcf4ee1da0cb9b2cb6707383cca68aece028d465bad7915c9e4d0d143c78ed6f90208d5cddb65be5ce5922721870e8f79ffd4",
}


def download(cache, suffix):
    name = "Godot_v" + VERSION + "_" + suffix
    archive = cache / name
    if not archive.exists():
        print("Downloading " + name, flush=True)
        temporary = archive.with_suffix(archive.suffix + ".partial")
        urllib.request.urlretrieve(BASE + name, temporary)
        temporary.replace(archive)
    digest = hashlib.sha512()
    with archive.open("rb") as source:
        for chunk in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(chunk)
    if digest.hexdigest() != HASHES[suffix]:
        raise RuntimeError("Checksum mismatch: " + str(archive))
    return archive



RCEDIT_URL = "https://github.com/electron/rcedit/releases/download/v2.0.0/rcedit-x64.exe"
RCEDIT_SHA512 = "c13e7ffd60169c348e16a3ea59a171c1777acdb241f950c11a6e9b69c955a3a4eb3432182aee7f489a87a555d0bd51fde3b597826f7c1e6488f1f5097359ab4d"


def install_rcedit(directory):
    """Keep the pinned Windows resource editor beside Godot, without admin rights."""
    executable = directory / "rcedit.exe"
    candidate = executable
    if not executable.exists():
        candidate = executable.with_suffix(".partial")
        urllib.request.urlretrieve(RCEDIT_URL, candidate)
    if hashlib.sha512(candidate.read_bytes()).hexdigest() != RCEDIT_SHA512:
        raise RuntimeError("Checksum mismatch: " + str(candidate))
    if candidate != executable:
        candidate.replace(executable)
    return executable


def template_directory():
    if sys.platform == "darwin":
        base = Path.home() / "Library/Application Support"
    elif sys.platform == "win32":
        base = Path(os.environ["APPDATA"])
    else:
        base = Path(os.environ.get("XDG_DATA_HOME", str(Path.home() / ".local/share")))
    return base / "Godot/export_templates/4.2.2.stable.mono" if sys.platform in ("win32", "darwin") else base / "godot/export_templates/4.2.2.stable.mono"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--directory", type=Path, default=Path(".tools/godot-4.2.2"))
    parser.add_argument("--templates", action="store_true", help="Also install .NET export templates (about 857 MB download)")
    args = parser.parse_args()
    target = args.directory.resolve()
    target.mkdir(parents=True, exist_ok=True)
    choices = {"darwin": ("mono_macos.universal.zip", "Godot_mono.app/Contents/MacOS/Godot"),
               "win32": ("mono_win64.zip", "Godot_v4.2.2-stable_mono_win64/Godot_v4.2.2-stable_mono_win64_console.exe"),
               "linux": ("mono_linux_x86_64.zip", "Godot_v4.2.2-stable_mono_linux_x86_64/Godot_v4.2.2-stable_mono_linux.x86_64")}
    if sys.platform not in choices or (sys.platform != "darwin" and platform.machine().lower() not in ("x86_64", "amd64")):
        parser.error("Supported: macOS universal, Linux x86_64, Windows x64")
    suffix, relative = choices[sys.platform]
    archive = download(target, suffix)
    executable = target / relative
    if not executable.exists():
        if sys.platform == "darwin":
            subprocess.run(["ditto", "-x", "-k", str(archive), str(target)], check=True)
        else:
            with zipfile.ZipFile(archive) as source:
                source.extractall(target)
        if sys.platform != "win32":
            executable.chmod(executable.stat().st_mode | 0o111)
    if args.templates:
        archive = download(target, "mono_export_templates.tpz")
        destination = template_directory()
        destination.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(archive) as source:
            for member in source.infolist():
                relative = Path(member.filename)
                if relative.parts[0] != "templates" or member.is_dir():
                    continue
                output = destination.joinpath(*relative.parts[1:])
                output.parent.mkdir(parents=True, exist_ok=True)
                with source.open(member) as data, output.open("wb") as output_file:
                    shutil.copyfileobj(data, output_file)
        print("Export templates: " + str(destination))
        if sys.platform == "win32":
            print("Windows resource editor: " + str(install_rcedit(executable.parent)))
    print("Godot: " + str(executable))
    if os.environ.get("GITHUB_ENV"):
        with open(os.environ["GITHUB_ENV"], "a", encoding="utf-8") as env:
            env.write("GODOT=" + str(executable) + "\n")


if __name__ == "__main__":
    main()
