# Windows validation results

## Environment and revision

Tested **2026-10-02** on Windows 11 build **26200**, through the dedicated `builder` SSH account. Latest tested revision **`b3936c2668c430af5eec421d5e170dde2b4c464b`**, branch `codex/build-tests-and-gameplay-fixes`, cloned from a verified Git bundle. The baseline working tree was clean. Toolchain: Godot **4.2.2 .NET / 15073afe3**, .NET SDK **6.0.428 x64**, Python **3.14.3**. No GitHub push or remote CI run was needed for these checks.

## Automated results

**Fresh native Windows validation passed in one run:** compilation, strict import, **408/408 isolated regression cases**, source startup smoke and Windows release export. All **nine Python tests** passed. This used the committed installer, including checksum verification of the cached RCEdit binary, and the corrected validator without manually adding RCEdit to PATH. Compilation retains 14 existing warnings and zero errors.

The resulting executable also passed an external headless startup smoke check, including actual loads of the canonical Bandaid and alien-artifact Research images and SCG Production illustration. All 64 illustration resources are present in the pack. Its embedded pack has **1,218 entries and no test resources**. Windows executable SHA-256:

```text
00d9f6243e79d3e27036575650a4c1561db2544193369c8608ad69613f1d2616
```

The checkout was clean before import. Afterwards, Git status flagged 385 `.import` sidecars with LF/CRLF notices; `git diff --exit-code` confirmed **no normalized content differences**. Those worktree files and logs were preserved, not bulk-committed or reset.

### Earlier passing baselines

Revision `7c7db5e` passed 406 cases, source smoke, export and packaged startup after the original illustration recovery. Its executable SHA-256 was `26551d145cddb7c60843e0cbe04eb5c63eea4b901246452f25edbdd9fcdf58d5`; evidence remains in `artifacts/windows-handoff/7c7db5e-evidence/`.

Revision `d0d15f7` passed 405 cases, source smoke, export and the strengthened Bandaid package smoke before the full illustration recovery. Its executable SHA-256 was `b823a475c49a3a59e9f4667beefa0a175f056cf68859ff1e050315e717190d9d`; evidence remains in `artifacts/windows-handoff/d0d15f7-evidence/`.

Revision `5053982` passed 403 cases, source smoke, export and the earlier packaged startup check. Its executable SHA-256 was `58538b21645386451f2d12a08c6ea7da411080af4e0ff5019c078ced82656d63`; evidence remains in `artifacts/windows-handoff/5053982-evidence/`. A later targeted probe exposed the asset-path defect described below.

Revision `ca54582` passed all 385 cases, source smoke, export and packaged startup before the construction-art recovery. Its executable SHA-256 was `edde06de1c9bb4d05f144518214edcc7d5901286a939bcd2c6f2ae4f9ae50127`; evidence remains in `artifacts/windows-handoff/ca54582-evidence/`.

Revision `3871a6f` passed all 381 cases, source smoke, export and packaged startup before the production-rod correction. Its executable SHA-256 was `88754f1bc93d214aa39167e5c5fc640b2a88eb841ab8e19b9a25771d29e66fa2`; evidence remains in `artifacts/windows-handoff/3871a6f-evidence/`.

Revision `6ac9301` passed all 379 cases, source smoke, export and packaged startup before the orbital-view correction. Its executable SHA-256 was `28eb66c617f25fd33951412acf1e7fd6699221c430ef5ecd61f99a2ef779f062`; evidence remains in `artifacts/windows-handoff/6ac9301-evidence/`.

Revision `853986c` passed all 375 cases, source smoke, export and packaged startup before the alarm addition. Its executable SHA-256 was `07ebde47221f7c85b43ad879d113b88ebd4fbbfeec3f1c3f3ff594ce559e0352`; evidence remains in `artifacts/windows-handoff/853986c-evidence/`.

Revision `862e345` passed all 374 cases, source smoke, export and packaged startup before the colony ground-crew correction. Its executable SHA-256 was `467539e789238c79acb46d4f9cf5a46915330c6276d39bc1cd21f880031f2a44`; evidence remains in `artifacts/windows-handoff/862e345-evidence/`.

Revision `55316b6` passed all 359 cases, source smoke, export and packaged startup. Its executable SHA-256 was `ed7ffde6954cbb62d77bbe21787206e0e50fa7bf38c6adfa3174ca1f3d6a3063`; evidence remains in `artifacts/windows-handoff/55316b6-evidence/`.

Revision `8cdd458` passed all 349 cases, source smoke, export and packaged startup. Its executable SHA-256 was `92106ed3bc9242ddad9bc19d068969838bcda7cf3a8cec9847b66d709c777b33`; evidence remains in `artifacts/windows-handoff/8cdd458-evidence/`. Preserve any desktop acceptance already underway on that revision.

Revision `d11ff3b` separately passed all 337 cases, source smoke, export and packaged startup. Its executable SHA-256 was `9bc8710d289b79f1d56df03a22eef8ea539449e014d170850541608de24d01f4`; evidence remains in `artifacts/windows-handoff/d11ff3b-evidence/`. Its desktop handoff and checkout remain available for work already in progress.

Revision `d057b9d` separately passed all 329 cases, source smoke, export and packaged startup. Its executable SHA-256 was `13a279e225f5e9f8847e0ac526de573ee99c2f3f90cd5a0acd35e0f295c8f7cd`; evidence remains in `artifacts/windows-handoff/d057b9d-evidence/`.

Revision `3004d9b` separately passed all 313 cases, source smoke, export and packaged startup, including a fresh verified RCEdit download. Its executable SHA-256 was `1bb04c4f480b562f3caa726248482d849e9855a23ade2de4f7166c8d7433a415`; evidence remains in `artifacts/windows-handoff/3004d9b-evidence/`. Both passing revisions retain nine Python tests.

### Earlier missing-RCEdit failure

At earlier revision `44e37ba`, the first native run of `python scripts/validate.py --export-windows` passed compilation (14 existing warnings, zero errors), strict import, **309/309 isolated regression cases**, and source startup smoke. All six validator unit tests at that revision passed. **The overall command failed at export** because `rcedit` was absent:

```text
ERROR: Could not create child process: rcedit ... --set-icon ...
```

The failed attempt and its output were preserved before further work. Installing verified [RCEdit 2.0.0](https://github.com/electron/rcedit/releases/tag/v2.0.0) and adding its directory to the export process's PATH made a separate strict export pass. The resulting Windows executable passed startup smoke using the repository's external `Tests/Smoke.gd` driver. This executes the packaged game; test resources remain excluded from the package. Pack inspection found **1,105 entries and no test resources**. This is a successful regression run plus repaired export/startup checks across separate commands, **not a passing original aggregate**.

The installer now supplies pinned RCEdit alongside Godot when installing Windows templates, and the validator makes that directory available to child processes. Nine Python tests pass locally, including download integrity and offline cache checks. The fresh 408-case run above verifies the committed tooling correction on Windows.

Godot's [4.2 documentation](https://docs.godotengine.org/en/4.2/tutorials/export/changing_application_icon_for_windows.html) requires RCEdit for resource modification and notes embedded-pack limitations; both pack readability and packaged startup were therefore checked. No exporter/gameplay errors were suppressed to obtain the repaired result.

### SDM validation corrections

Earlier full attempts at `d61ef77` and `dce19e1` stopped at old whole-save UI comparisons (preset case 119, then pilot-warning case 129). The new saved SDM timer advances during rendered frames. Targeted fixture corrections freeze GameCore processing around those comparisons while retaining every saved-state assertion. The fresh `862e345` aggregate passed all 374 cases; failed checkouts and logs remain preserved. The separate intermittent Mac editor timer diagnostic is recorded in [SDM implementation](self-destruct-implementation.md#verification), without a claimed cause or suppression.

## Gameplay acceptance

| Scenario | Source | Export | Evidence / next step |
| --- | --- | --- | --- |
| Automated regression suite | PASS, 408 cases | Not shipped | Isolated headless engine processes |
| Startup and clean scripted exit | PASS | PASS | External smoke driver for the packaged executable |
| Physical mouse/keyboard, audio, window close | NOT TESTED | NOT TESTED | Desktop agent/Craig acceptance |
| Save compatibility and normal campaign progression | NOT TESTED manually | NOT TESTED | Follow the Windows agent brief |
| Complete Cycle | PASS, automated cases 314–329 | NOT TESTED interactively | Fuel waits, save/load and unload-and-stop behavior; physical controls/close pending |
| Marine ranks and research-only recipes | PASS, cases 330–337 | NOT TESTED interactively | Includes legacy Hyperlight selection and automatic fuel refining |
| Research completion and restored selection | PASS, cases 404–405 | Bandaid image load PASS; interaction NOT TESTED | Hyperlight null recipe, return to physical recipes and recovered illustrations; native Mac screenshots separately checked |
| Staff attrition | PASS, cases 350–359 | NOT TESTED interactively | 100-day gates, cryopod suspension, depleted teams and old/new saves |
| SDM controls, capture and expiry | PASS, cases 360–375 | NOT TESTED interactively | Switches, discovery/interlock, both timers, ship/station loss and save compatibility; colony ground-crew loss corrected; original listening/timing observations pending |
| SCG item names | PASS, cases 407–408 | NOT TESTED interactively | New and legacy saves show chassis/drive names in Production, Station, Stores and MTX; existing names and stock preserved |
| Original item illustrations | PASS, case 406 | Resource loads PASS; desktop NOT TESTED | All 32 source hashes, opaque Research versus masked Production, original bounds/placement; 113,952 native Mac pixels separately verified |
| Recovered construction artwork | PASS, cases 386–403 | NOT TESTED interactively | Seven genuine frame sets, blank SDM/MTX stages, five missing research images; paid manual/AOC production and installation; native Mac render separately verified |
| Production rod | PASS, cases 382–385 | NOT TESTED interactively | Original frame hashes/cadence, manual/AOC, ground/orbit, pause/idle/navigation and normal frame processing; native Mac pixels separately verified |
| Orbital location view | PASS, cases 380–381 | NOT TESTED interactively | Planet/station artwork, nine palettes, moon inheritance, local station presence, cached images and all hull transitions; native Mac render separately verified |
| SDM alarm | PASS, cases 376–379 | NOT TESTED interactively | Exact PCM/loop parameters, sound priority, navigation/lifetime and stereo mixer onset 0.3202 seconds; desktop/original listening remains pending |
| SDM local installation | PASS, cases 338–347 | NOT TESTED interactively | Paid installation, no duplicate/repeat, interruption and save/load; gameplay checked separately below |
| Retained-input shutdown | PASS, cases 348–349 | Startup smoke only | Debug input-cache correction; physical window closing remains pending |
| Accepted trade fuel gift | PASS, strengthened cases 220–235 | NOT TESTED interactively | Accept fills gauge; refusal/stale offers do not |
| AMA cargo correction | PASS, automated cases 310–313 | NOT TESTED interactively | Included in the latest native run; manual gameplay remains pending |

The historical native Mac case-315 shutdown failure is preserved. A controlled retained-input reproduction now has a [tested correction](shutdown-input-evidence.md), included in this revision: cases 348–349 passed on Windows, and planned native Mac checks include three clean case-315 exits. This does not replace physical Windows close/audio acceptance or establish the precise object in the historical crash.

The logged-in desktop belongs to a different account from the SSH worker. No desktop input, audible output, GPU rendering or campaign acceptance is claimed from these headless checks. None of the 48 task rows is yet declared fully accepted.

## Packaged Bandaid artwork correction

The `5053982` executable lacked the requested `res://Sprites/Items/Research/bandaid.png`, while tracked `Bandaid.png` existed. Case-insensitive source filesystems hid the mismatch; Production and Research both construct the lowercase path. The confirmed probe is retained in `artifacts/windows-handoff/probe-bandaid-5053982-v2.log`. Its initial wrapper failure checked a GUI process too early and is not a game crash.

Revision `d0d15f7` renames the PNG and import sidecar to lowercase while retaining the image bytes and resource UID. The smoke driver now loads that exact resource. The strengthened driver **failed against the old executable with game exit 1**, then **passed against the new Windows export with exit 0**. See `smoke-resource-red-5053982.log` and the current collected `export-smoke.log`. This verifies the resource-load correction; desktop rendering remains pending.

## Evidence and next steps

Latest raw logs and environment/export hashes were collected under ignored `artifacts/windows-handoff/b3936c2-evidence/`. The earlier `baseline-evidence/` archive preserves the missing-RCEdit failure and its separate repair. Remote evidence is retained in the isolated validation checkout. Generated executables, caches and logs are not committed.

Merge independently observed desktop results into this report. The canonical Windows command now passes with the corrected installer/validator. Preserve commit identities when adding newer fixes. Team PR readiness still requires the feature acceptance checks in [the Windows brief](windows-agent-brief.md).
