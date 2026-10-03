# Windows validation results

## 595-case source checkpoint — 2026-10-03

The isolated Windows checkout at `dbac18325e92bb214d5524732157d3eb963d22ab` is runtime-identical to `bcaeb1cc9da5ae956059857f822296a1d86988d8`. All **595/595 regressions**, compilation (14 existing warnings), strict import and source startup passed. All 600 collected source-evidence files were independently audited for case discovery, fresh timestamps, individual success markers and engine errors. Evidence: `artifacts/windows-handoff/bcaeb1c-source-evidence/`; archive SHA-256 `55402ad335b83ed3b9c70eda59856bb2ed525413f7133c8a249942e39466b333`.

The runner reports 19 Python checks with one original-disk roundtrip skip; the original disk was not copied to Windows. **Separate direct export and packaged startup now pass**, with the audit below. The original full command timed out during console-wrapper exit; it is not recorded as a passing aggregate. These headless results do not establish physical Windows controls, rendering, audio or normal campaign acceptance. Task-specific local acceptance is recorded in [the ledger](backlog-progress.md#requirements-accepted--2026-10-03).

## Direct export and package audit — 2026-10-03

After all 595 cases and source startup passed, the console-wrapper export timed out at 600 seconds. Its retained log contains completed .NET publishing, `savepack: end` and editor shutdown. Only the wrapper and a background `dotnet` process remained from that export; both disappeared at timeout. The unrelated desktop game was preserved.

Godot 4.2.2's [console wrapper](https://github.com/godotengine/godot/blob/4.2.2-stable/platform/windows/console_wrapper_windows.cpp) waits for its entire child job, including background workers. Changing only the invoked executable to the main Godot binary made a separate identical export finish in **33.26 seconds**, with strict logs and exit zero. The installer now selects that binary for Windows automation; Python already waits for it and captures output. The updated installer was checked against the cached pinned Windows distribution and its returned binary's version; all 19 Python tests pass on Mac with the original ending disk. No timeout was extended or error suppressed.

The resulting package passed headless startup and clean exit on Windows. Independent collection audit verifies **1,230 pack entries**, **64 illustration imports**, **zero test resources**, the alien font, ending JSON and music import/sample. Packed payload MD5 values match. Windows checkout converted the JSON's final LF to CRLF; this is the only byte difference from the committed Mac copy, and the decoded JSON is identical. Windows normalized source diff remains empty.

- Executable: **163,833,616 bytes**, SHA-256 `d2cb76c98769773d6a11a0179146a36ac8a6139ab580366b50b7759c8b1fbda5`.
- Collected archive SHA-256: `3c8af1cff599f02734e905e2dd510127c11ccb9232e2953cf336a5d6c6130336`.
- Evidence: `artifacts/windows-handoff/bcaeb1c-evidence/`, including `collection-audit.json`, `export-windows-direct.log` and `export-smoke.log`. The original timeout/log is preserved separately in `bcaeb1c-source-evidence/failed-export/`.

This establishes source regressions plus a separately verified export/package at runtime `bcaeb1c`. Physical Windows input, rendering, audio and normal campaign checks remain separate.

## Historical environment and revision

Tested **2026-10-02** on Windows 11 build **26200**, through the dedicated `builder` SSH account. Latest tested revision **`a54e899d4c6796e6f406733112d39a7ba424e7d0`**, branch `codex/build-tests-and-gameplay-fixes`, cloned from a verified Git bundle. The baseline working tree was clean. Toolchain: Godot **4.2.2 .NET / 15073afe3**, .NET SDK **6.0.428 x64**, Python **3.14.3**. No GitHub push or remote CI run was needed for these checks.

## Automated results

**Newer revision, evidence collected 2026-10-03:** the Windows runner at `f5a9cbb994b2f904426c0744856405328f243a16` exited zero after **421/421 cases**, nine Python tests, import, source startup and release export. SSH access was restored using the existing login agent/Keychain; all 421 individual logs now pass audit, with no gameplay errors. Import/export contain only the existing exact Godot 4.2.2 editor-teardown exemption. The executable has 1,219 pack entries, all 64 illustration imports, no tests, and SHA-256 `28a5899195edeee1e31fb8adbe00e46a9c81a56e3595b129bfb6fb9f9f74a8f1` (149,260,848 bytes). The 385 imported sidecars have no normalized content differences. Evidence is in `artifacts/windows-handoff/f5a9cbb-evidence/`. **Packaged startup remains untested at this revision**; the complete source-plus-package checkpoint below remains `a54e899`. This collection did not run a newer build or establish interactive acceptance.

**Fresh native Windows validation passed in one run:** compilation, strict import, **409/409 isolated regression cases**, source startup smoke and Windows release export. All **nine Python tests** passed. This used the committed installer, including checksum verification of the cached RCEdit binary, and the corrected validator without manually adding RCEdit to PATH. Compilation retains 14 existing warnings and zero errors.

The resulting executable also passed an external headless startup smoke check, including actual loads of the canonical Bandaid and alien-artifact Research images and SCG Production illustration. All 64 illustration resources are present in the pack. Its embedded pack has **1,218 entries and no test resources**. Windows executable SHA-256:

```text
6bc8b3c31a15dd881bb757b52b675dcc08e8101b9ebc37943773cfbed0619da3
```

The checkout was clean before import. Afterwards, Git status flagged 385 `.import` sidecars with LF/CRLF notices; `git diff --exit-code` confirmed **no normalized content differences**. Those worktree files and logs were preserved, not bulk-committed or reset.

### Earlier passing baselines

Revision `b3936c2` passed 408 cases, source smoke, export and packaged startup after the SCG name repair. Its executable SHA-256 was `00d9f6243e79d3e27036575650a4c1561db2544193369c8608ad69613f1d2616`; evidence remains in `artifacts/windows-handoff/b3936c2-evidence/`.

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

The installer now supplies pinned RCEdit alongside Godot when installing Windows templates, and the validator makes that directory available to child processes. Nine Python tests pass locally, including download integrity and offline cache checks. The fresh 409-case run above verifies the committed tooling correction on Windows.

Godot's [4.2 documentation](https://docs.godotengine.org/en/4.2/tutorials/export/changing_application_icon_for_windows.html) requires RCEdit for resource modification and notes embedded-pack limitations; both pack readability and packaged startup were therefore checked. No exporter/gameplay errors were suppressed to obtain the repaired result.

### SDM validation corrections

Earlier full attempts at `d61ef77` and `dce19e1` stopped at old whole-save UI comparisons (preset case 119, then pilot-warning case 129). The new saved SDM timer advances during rendered frames. Targeted fixture corrections freeze GameCore processing around those comparisons while retaining every saved-state assertion. The fresh `862e345` aggregate passed all 374 cases; failed checkouts and logs remain preserved. The separate intermittent Mac editor timer diagnostic is recorded in [SDM implementation](self-destruct-implementation.md#verification), without a claimed cause or suppression.

## Gameplay acceptance

| Scenario | Source | Export | Evidence / next step |
| --- | --- | --- | --- |
| Automated regression suite | PASS, 409 cases | Not shipped | Isolated headless engine processes |
| Startup and clean scripted exit | PASS | PASS | External smoke driver for the packaged executable |
| Physical mouse/keyboard, audio, window close | NOT TESTED | NOT TESTED | Desktop agent/Craig acceptance |
| Save compatibility and normal campaign progression | NOT TESTED manually | NOT TESTED | Follow the Windows agent brief |
| Complete Cycle | PASS, automated cases 314–329 | NOT TESTED interactively | Fuel waits, save/load and unload-and-stop behavior; physical controls/close pending |
| Marine ranks and research-only recipes | PASS, cases 330–337 | NOT TESTED interactively | Includes legacy Hyperlight selection and automatic fuel refining |
| Research completion and restored selection | PASS, cases 404–405 | Bandaid image load PASS; interaction NOT TESTED | Hyperlight null recipe, return to physical recipes and recovered illustrations; native Mac screenshots separately checked |
| Staff attrition | PASS, cases 350–359 | NOT TESTED interactively | 100-day gates, cryopod suspension, depleted teams and old/new saves |
| SDM controls, capture and expiry | PASS, cases 360–375 | NOT TESTED interactively | Switches, discovery/interlock, both timers, ship/station loss and save compatibility; colony ground-crew loss corrected; original listening/timing observations pending |
| Module dialogue colours | PASS, case 409 | NOT TESTED interactively | Normalized colour conversion, real text-window output and dynamic substitutions; native Mac screenshot separately reviewed |
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

Latest raw logs and environment/export hashes were collected under ignored `artifacts/windows-handoff/a54e899-evidence/`. The earlier `baseline-evidence/` archive preserves the missing-RCEdit failure and its separate repair. Remote evidence is retained in the isolated validation checkout. Generated executables, caches and logs are not committed.

Merge independently observed desktop results into this report. The canonical Windows command now passes with the corrected installer/validator. Preserve commit identities when adding newer fixes. Team PR readiness still requires the feature acceptance checks in [the Windows brief](windows-agent-brief.md).


## Desktop agent reports retrieved 2026-10-03

The Mac agent retrieved these reports from Asana comments at 13:06 UTC. They are **reported Windows source checks**, not independently audited artifacts or matching-export acceptance. The desktop checkout starts at `44e37ba74c98fbf7f070eb77c051a3caa1fdbba2`; subsequent changes are identified below. Its shared `DEUTEROS-COORDINATION.md` and patch bundle are not yet accessible to SSH user `builder`. The running desktop Godot process was left untouched.

| Area | Reported result | Remaining work |
| --- | --- | --- |
| [SCG](https://app.asana.com/0/1214891399253076/1215685674676241) | Staged hull/drive fitting, five pod mounts, deductions, save/reload and cargo layout passed. Commit `5a9b1ef` fixes a navigation container blocking the build-button centre; existing case 263 failed before and passed after. | Collect/reconcile patch; matching export, current six-mount implementation and normal progression. |
| [Supply discard](https://app.asana.com/0/1214891399253076/1215685674676219) | Docked staged SCG discarded only selected cargo, retained the pod and preserved other modules/stores. Empty/non-supply controls did nothing. | Reload, other hulls, transit, ACC/AMA and normal progression. |
| [ACC](https://app.asana.com/0/1214891399253076/1215683087492480) | Qualified grapple/supply IOS engaged, found and disengaged at asteroids; manual small-asteroid capture and reload passed. Earlier right-click failure was not reproduced twice. | Large asteroids, AMA, Complete Cycle, balancing and export. |
| [DFCC](https://app.asana.com/0/1214891399253076/1215716464570901) | Staged zero-drone IOS required ten MeH per tank; insufficient stock did nothing, save/restart retained fuel, removal refunded ten. | SCG/HeD, drone/cargo capacity, ACC, legacy saves and export. |
| [Stocktaker](https://app.asana.com/0/1214891399253076/1215685674676245) | Day-219 normal play reproduced Advance Time blocked by `TradStore`. Local one-line mouse-filter fix passed extended case 112 and physical time/category clicks. | Collect uncommitted patch and audit against current suite/export. |
| [Service](https://app.asana.com/0/1214891399253076/1215691951441136) | Commit `878d74dc69b48e2144a1ab3ce7dd38e6a82cb165` fixed the reported day-439 control. Staged eligibility/lock checks and physical click/save/restart passed; Craig confirmed it. | Reconcile with Mac Service fix `50ea4af`; matching export still untested. |
| Pod fitting feedback | Same Supply task reports a local shared-handler popup for unavailable supply/tool/cryo pods; new Windows case 311 failed before and passed after. | Collect patch; normal pointer check and export. Fitting animation remains unresolved. |
| [Audio](https://app.asana.com/0/1214891399253076/1215683087492491) | Craig reported missing training-door/production sounds and an incorrect Advance Time cue. | No audio fix or listening acceptance; agent reports gameplay was prioritized. |

Windows case numbers refer to its older checkout and must not be copied over current case numbers. Detailed retrieved comments are retained locally at `artifacts/research/windows-agent-asana-2026-10-03.json`; Windows screenshots/logs remain under its `artifacts/windows-validation/desktop-01` through `desktop-03`. All 48 tasks remain open; no acceptance count or Asana status was changed.

### Desktop popup follow-up retrieved 2026-10-03 15:52 UTC

The latest Supply task comment reports an additional physical failure: missing-pod text overflowed the fixed 128×57 error box. The Windows agent replaced its geometry with `PanelContainer`/`MarginContainer`, strengthened its existing case 311, and reports a normal day-981 source click with enclosed text after restart. Player saves were preserved. The running desktop process **30940** belongs to this playtest and was not disturbed by SSH validation.

`unavailable-pod-feedback-v2.patch` supersedes the earlier patch and includes four files over `878d74d`. It remains uncommitted in the desktop checkout and has not been collected/integrated here; original box fidelity and matching export remain untested. Retrieved source: `artifacts/research/windows-agent-asana-2026-10-03-1552.json`. This report adds no accepted task and no Asana changes.
