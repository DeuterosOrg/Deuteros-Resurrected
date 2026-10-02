"""Guard the executable dependency used by native Windows export."""
import hashlib
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import install_godot


class RceditInstallationTests(unittest.TestCase):
    def test_verified_binary_is_usable_from_godot_directory_and_cached_offline(self):
        payload = b"verified test executable"
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            def download(url, destination):
                Path(destination).write_bytes(payload)
            with patch.object(install_godot, "RCEDIT_SHA512", hashlib.sha512(payload).hexdigest(), create=True):
                with patch.object(install_godot.urllib.request, "urlretrieve", side_effect=download):
                    installed = install_godot.install_rcedit(root)
                self.assertEqual(root / "rcedit.exe", installed)
                self.assertEqual(payload, installed.read_bytes())
                with patch.object(install_godot.urllib.request, "urlretrieve", side_effect=OSError("offline")):
                    self.assertEqual(installed, install_godot.install_rcedit(root))

    def test_bad_download_never_becomes_an_executable(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            def download(url, destination):
                Path(destination).write_bytes(b"corrupted download")
            with patch.object(install_godot.urllib.request, "urlretrieve", side_effect=download):
                with self.assertRaisesRegex(RuntimeError, "Checksum mismatch"):
                    install_godot.install_rcedit(root)
            self.assertFalse((root / "rcedit.exe").exists())

    def test_modified_cached_executable_is_rejected(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "rcedit.exe").write_bytes(b"modified executable")
            with self.assertRaisesRegex(RuntimeError, "Checksum mismatch"):
                install_godot.install_rcedit(root)


if __name__ == "__main__":
    unittest.main()
