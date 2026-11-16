# Security Policy

## Reporting a vulnerability

Please **don't** open a public issue. Use GitHub's private reporting:
**Security → Report a vulnerability** on this repository. If that isn't available,
contact a maintainer privately on the project Discord and we'll arrange a private
channel.

Include the affected release or commit, what an attacker could do, steps to
reproduce, and a minimal example (redact any personal data). We aim to acknowledge
reports within a week.

## Scope

Deuteros is an offline single-player game, so the realistic risks are:

- crafted save, settings or data files that crash the game or run code when loaded;
- the build and release pipeline — GitHub Actions workflows, pinned actions,
  NuGet dependencies, and release binaries;
- credentials or tokens accidentally committed to the repository.

## Supported versions

The latest release and the `develop` branch. Older releases are not patched.
