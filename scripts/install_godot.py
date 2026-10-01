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

VERSION = "4.2.1-stable"
BASE = "https://github.com/godotengine/godot/releases/download/" + VERSION + "/"
# Published SHA512-SUMS.txt for the official release; pins downloads independently.
HASHES = {
    "mono_linux_x86_64.zip": "1a2bfd681e1691bb6e39a8d1b8d0863cae1e25bf72fefb5f37572354aaddafde0043d83c070a580a0c5f3db7ac7e9bbe448dcd5f1206b74382edd4e96a916158",
    "mono_win64.zip": "9701e26c6e1b7d7cb944bb3bde3c8e73a3826aa605ea8b5f73d13b6ea7ec6d70069fe810ce5e5164e9b2970cf5cd9a3d76b40abc790382c211993bf50f18da93",
    "mono_macos.universal.zip": "eb4b602cef663507c3e1965c2ac72d4ded009098936c1b98eb8e4c21365b9c0b979f982548838bed152f2138298b833b46f891d204ce93fbe0af56fc82be582a",
    "mono_export_templates.tpz": "cb8354e733a8cbde169eb99ce36ea2e51d795ff5ab6e4df444e482fe96b02abb55a92a983bfb6722c2baeb6b40a8ec525d638ceb986717b40d9e07c5a3ea42e5",
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


def template_directory():
    if sys.platform == "darwin":
        base = Path.home() / "Library/Application Support"
    elif sys.platform == "win32":
        base = Path(os.environ["APPDATA"])
    else:
        base = Path(os.environ.get("XDG_DATA_HOME", str(Path.home() / ".local/share")))
    return base / "Godot/export_templates/4.2.1.stable.mono" if sys.platform in ("win32", "darwin") else base / "godot/export_templates/4.2.1.stable.mono"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--directory", type=Path, default=Path(".tools/godot-4.2.1"))
    parser.add_argument("--templates", action="store_true", help="Also install .NET export templates (about 857 MB download)")
    args = parser.parse_args()
    target = args.directory.resolve()
    target.mkdir(parents=True, exist_ok=True)
    choices = {"darwin": ("mono_macos.universal.zip", "Godot_mono.app/Contents/MacOS/Godot"),
               "win32": ("mono_win64.zip", "Godot_v4.2.1-stable_mono_win64/Godot_v4.2.1-stable_mono_win64_console.exe"),
               "linux": ("mono_linux_x86_64.zip", "Godot_v4.2.1-stable_mono_linux_x86_64/Godot_v4.2.1-stable_mono_linux.x86_64")}
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
    print("Godot: " + str(executable))
    if os.environ.get("GITHUB_ENV"):
        with open(os.environ["GITHUB_ENV"], "a", encoding="utf-8") as env:
            env.write("GODOT=" + str(executable) + "\n")


if __name__ == "__main__":
    main()
