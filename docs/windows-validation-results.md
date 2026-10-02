# Windows validation results

## Environment and revision

Tested **2026-10-02** on Windows 11 build **26200**, through the dedicated `builder` SSH account. Revision **`44e37ba74c98fbf7f070eb77c051a3caa1fdbba2`**, branch `codex/build-tests-and-gameplay-fixes`, cloned from a verified Git bundle. The baseline working tree was clean. Toolchain: Godot **4.2.2 .NET / 15073afe3**, .NET SDK **6.0.428 x64**, Python **3.14.3**. No GitHub push or remote CI run was needed for these checks.

## Automated results

The first native run of `python scripts/validate.py --export-windows` passed compilation (14 existing warnings, zero errors), strict import, **309/309 isolated regression cases**, and source startup smoke. All six validator unit tests at that revision passed. **The overall command failed at export** because `rcedit` was absent:

```text
ERROR: Could not create child process: rcedit ... --set-icon ...
```

The failed attempt and its output were preserved before further work. Installing verified [RCEdit 2.0.0](https://github.com/electron/rcedit/releases/tag/v2.0.0) and adding its directory to the export process's PATH made a separate strict export pass. The resulting Windows executable passed startup smoke using the repository's external `Tests/Smoke.gd` driver. This executes the packaged game; test resources remain excluded from the package. Pack inspection found **1,103 entries and no test resources**. This is a successful regression run plus repaired export/startup checks across separate commands, **not a passing original aggregate**.

The installer now supplies pinned RCEdit alongside Godot when installing Windows templates, and the validator makes that directory available to child processes. Nine Python tests pass locally, including download integrity and offline cache checks. A fresh full Windows run using the committed tooling correction is still pending.

Godot's [4.2 documentation](https://docs.godotengine.org/en/4.2/tutorials/export/changing_application_icon_for_windows.html) requires RCEdit for resource modification and notes embedded-pack limitations; both pack readability and packaged startup were therefore checked. No exporter/gameplay errors were suppressed to obtain the repaired result.

## Gameplay acceptance

| Scenario | Source | Export | Evidence / next step |
| --- | --- | --- | --- |
| Automated regression suite | PASS, 309 cases | Not shipped | Isolated headless engine processes |
| Startup and clean scripted exit | PASS | PASS | External smoke driver for the packaged executable |
| Physical mouse/keyboard, audio, window close | NOT TESTED | NOT TESTED | Desktop agent/Craig acceptance |
| Save compatibility and normal campaign progression | NOT TESTED manually | NOT TESTED | Follow the Windows agent brief |
| Newer AMA cargo correction | NOT TESTED at this revision | NOT TESTED | Separate local follow-up; do not attribute later changes to this baseline |

The logged-in desktop belongs to a different account from the SSH worker. No desktop input, audible output, GPU rendering or campaign acceptance is claimed from these headless checks. None of the 48 task rows is yet declared fully accepted.

## Evidence and next steps

Raw logs and environment/export hashes were collected under ignored `artifacts/windows-handoff/baseline-evidence/`; the archive also preserves the original missing-RCEdit failure. Remote evidence is retained in the isolated validation checkout. Generated executables, caches and logs are not committed.

Repeat the canonical Windows command with the corrected installer/validator, then merge independently observed desktop results into this report. Preserve commit identities when adding newer fixes. Team PR readiness still requires the feature acceptance checks in [the Windows brief](windows-agent-brief.md).
