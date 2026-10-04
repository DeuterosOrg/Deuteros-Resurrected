# Windows desktop validation results

**Coordination permission update, 2026-10-03:** Craig subsequently authorized Asana progress comments to keep the team informed and avoid duplicate work. Four evidence-based comments were posted after the second pass; see the communication record at the end. Earlier no-write statements describe the scope/results at that time. No task statuses or fields were changed.

**Latest update: 2026-10-03.** The first-pass record below is preserved as historical evidence. The [second pass](#second-desktop-pass-and-scg-pointer-fix--2026-10-0203) adds source checks and a focused SCG button fix; it supersedes the earlier statements that no gameplay fix or clean desktop exit exists. Export acceptance remains NOT TESTED.

## Environment and revision

Tested on 2026-10-02 in Craig's interactive Windows session. This is a focused **source-game desktop pass**, not a replacement for the builder account's automated run and not complete Windows acceptance.

- Tested branch: `codex/build-tests-and-gameplay-fixes`.
- Tested HEAD: **`44e37ba74c98fbf7f070eb77c051a3caa1fdbba2`**, verified immediately after cloning the supplied `deuteros-44e37ba.bundle` with `GIT_LFS_SKIP_SMUDGE=1`.
- Isolated checkout: `C:\Users\CraigFletcher\Developer\GitHub\tonyoddsphere\Deuteros-Resurrected\desktop-validation-44e37ba`.
- Windows 11 Business, `10.0.26200`, build 26200. NVIDIA GeForce RTX 5070 Ti, driver `32.0.16.1692`; Parsec Virtual Display Adapter `0.45.0.0` also installed. Game logs identify the NVIDIA device, Vulkan API 1.4.351, Forward+.
- Godot `4.2.2.stable.mono.official.15073afe3`; .NET SDK `6.0.428` x64, hostfxr/runtime `6.0.36`; Python `3.14.3`. Godot archive passed the repository installer's pinned SHA-512 check. SDK came from Craig's copied ZIP.
- Initial working tree was clean. No gameplay, test-suite, project, or `AGENTS.md` changes were made. Import sidecars appear dirty from line endings; `git -c core.safecrlf=false diff --exit-code -- AGENTS.md Godot scripts` passes with no normalized changes. Report/ledger changes are separate from the tested game revision.
- Mouse clicks and Escape were delivered to the actual Windows game window through Computer Use, not through the regression harness. This does not constitute physical controller-hardware testing.

Evidence root, abbreviated **E** below: `artifacts/windows-validation/desktop-01/`. It contains raw process/engine logs, screenshots, fixture-generation scripts, saved before/after states and `evidence-summary.json`. Generated caches and binaries remain ignored.

The builder checkout and its processes were not modified or stopped. Newer bundles and handoff reports copied by Craig were preserved, but their presence was not treated as an instruction to change the explicitly pinned revision. No push, PR or Asana write was performed.

## Automated results and preparation

**No regression cases were rerun here (0).** No startup-smoke or export suite was duplicated. The supplied `deuteros-windows-validation-8cdd458.md` describes the earlier 44e37ba run as 309/309 regressions, build/import/source smoke and six Python tests passing, followed by an initial missing-RCEdit export failure and a separately repaired export/startup check. Those are supplied results, not observations made in this desktop pass, and not a passing original aggregate.

| Local preparation/check | Result | Evidence |
| --- | --- | --- |
| Isolated bundle clone and exact HEAD | PASS | SHA above; origin remains the local bundle |
| Source build | PASS: 14 warnings, 0 errors | E/build-authorized.log |
| Bootstrap/strict import inside sandbox | FAIL: environment restrictions and runtime selection | E/import-bootstrap.log, E/import.log |
| Authorized import with explicit .NET 6 selection | PASS: exit 0, `IMPORT OK`; documented `_EDITOR_GET` editor teardown diagnostic retained | E/import-authorized.log |
| Recorded-save conservation/backup checks | PASS | E/inspect-evidence.py, E/evidence-summary.json |
| Fresh export supplied to this session | NOT TESTED / unavailable | Requested through Craig; no matching complete export folder was received |

Preparation commands (from this checkout):

```powershell
$env:GIT_LFS_SKIP_SMUDGE = '1' # set before the clone
git rev-parse HEAD
& C:/Python314/python.exe scripts/install_godot.py
& .tools/dotnet/dotnet.exe build Godot/Deuteros.csproj
```

The build used `DOTNET_ROOT=.tools/dotnet`, `DOTNET_CLI_HOME=.tools`, and `NUGET_PACKAGES=.tools/nuget`, resolved to absolute paths. Initial sandbox build failure is retained in E/build.log; it could not create NuGet's normal migrations directory. The approved build restored dependencies successfully. Initial SDK extraction hit Windows' path-length limit; Python ZIP extraction with the absolute `\\?\` path succeeded without changing account permissions or OS settings.

For import/launch, `DOTNET_ROOT_X64` also points to the local SDK, `DOTNET_MULTILEVEL_LOOKUP=0`, and the SDK directory is first on PATH. The sandbox import initially selected the system .NET 8 host and could not write editor settings/cache or read the certificate store. These are preparation failures, not gameplay regressions. The authorized import selected hostfxr 6.0.36.

```powershell
# Asset preparation only; prepare.py calls the existing validator's import stages.
& C:/Python314/python.exe artifacts/windows-validation/desktop-01/prepare.py
# Authorized follow-up: DEUTEROS_IMPORT_ONLY=1, pinned environment above.
& .tools/godot-4.2.2/Godot_v4.2.2-stable_mono_win64/Godot_v4.2.2-stable_mono_win64_console.exe --headless --editor --path Godot
# Separate interactive attempts; selectors are cleared by launch.py.
& C:/Python314/python.exe artifacts/windows-validation/desktop-01/launch.py source-01
& C:/Python314/python.exe artifacts/windows-validation/desktop-01/launch.py source-02
& C:/Python314/python.exe artifacts/windows-validation/desktop-01/launch.py source-03
& C:/Python314/python.exe artifacts/windows-validation/desktop-01/inspect-evidence.py
```

Do not reuse attempt names: the launcher opens the named log for writing. The three completed attempts and their PID/exit files are preserved.

## Gameplay acceptance

PASS below applies only to the specific observed behavior. It does not override the strict process-log failures or accept a whole Asana task. **Every export cell is NOT TESTED** because the matching export was unavailable.

| Asana task / specific scenario | Progression | Source | Export | Steps and evidence |
| --- | --- | --- | --- | --- |
| Title, Earth, Training, overview, deposit analysis, Save, settings pointer navigation; Escape open/dismiss | Normal/new game | PASS | NOT TESTED | Three launches; readable 960x600 game area. E/01-startup.png, 05-training-settings.png, 07-volume-restart.png, 28-final-close-settings.png. Craig started recruitment during run 1; subsequent observations refreshed the screen before input. |
| Time toggle and return to stopped time | Normal | PASS | NOT TESTED | Saved day 1, advanced to **day 25**, toggled off, saved slot 2. An intermediate chat update misread the date as 26; the save and screenshots establish 25. |
| 1215685674676235: save, load confirmation/cancel, restored day and recruitment screen | Normal | PASS | NOT TESTED | Slot 1 preserves Craig's day-1 recruitment; slot 2 preserves day 25. Cancel Load left day 25, confirm restored day 1; training showed 5759 available trainees and closed training doors again. E/02-normal-save.png through 05-training-settings.png. Slots remained visible after process restart. |
| 1215685674676235: overwrite cancel, overwrite and Windows backup | Staged | PASS | NOT TESTED | Cancel overwrite of slot 4 preserved bytes exactly. Confirm saved repaired IOS and produced `.bak` identical to the damaged input fixture. E/damaged-fixture-initial.json, ios-repaired.json, profile/.../saves/slot-4.json.bak; assertions in inspect-evidence.py. |
| 1215685674676237: volume control, save, restart persistence, restore defaults | Normal | PASS | NOT TESTED | Changed 100% to 45%, saw Settings saved, closed process, reopened and saw 45%. Restored On/100%/960x600/windowed defaults. E/06-volume-setting.png, 07-volume-restart.png, 28-final-close-settings.png. |
| 1215683087492491: audible menu/ambience quality, interruption, mute and controller sounds | Both | NOT TESTED | NOT TESTED | Sound enabled; WASAPI initialized. Tool does not capture audio; listening confirmation was requested from Craig but not received. Settings/logs alone do not prove audibility. |
| Physical title-bar X, sound enabled, settings open/closed | Both | FAIL | NOT TESTED | All three processes exited 0, but their logs contain errors. See attempt table below. This is not three clean closes. |
| 1215683087492485: staged MTX installation/access from orbital Stores | Staged preset | PASS | NOT TESTED | Applied Settings > Cheats > Enable MTX, confirmed, waited for discovery bulletin. Opened Earth station > Stores > lower-left view switch; MTX panel rendered. E/08-mtx-fixture-applied.png, 09-orbital-mtx-access.png. |
| 1216065854613319: right-click actual MTX control returns to overview | Staged | PASS | NOT TESTED | Right-click Clear control returned to Master Control. E/10-mtx-rightclick.png. |
| Equipment-modal right-click precedence in ship bay | Staged | PASS | NOT TESTED | After DFCC selection, right-click dismissed equipment list and retained bay, rather than navigating away. |
| IOS assembly and exact local part deductions | Staged inventory/research | PASS | NOT TESTED | Built one IOS, fitted its drive and one tool pod/DFCC. Save comparison: i_chassis 2->1, i_drive 2->1, automatically fitted ACC 2->1, tool_pod 3->2, DFCC 2->1. Exactly one IOS saved. E/assembly-fixture-initial.json, ios-assembled.json, 11-empty-bay.png through 15-dfcc-conversion.png. No claim about duplicate-click rejection or SCG assembly. |
| 1215716464570901: IOS DFCC conversion and manual fuel ratio, zero drones | Staged | PASS | NOT TESTED | Ordinary +: 1000 stock/0 gauge ->999/1. Fit DFCC: ->1000/0 (old-rate refund). DFCC +: ->990/1; minus: ->1000/0. E/14-normal-fuel-one.png through 17-dfcc-unload-one.png. |
| 1215716464570901: nine-unit DFCC boundary | Staged | PASS | NOT TESTED | Loaded fixture with 9 MeH and gauge 0; + left 9/0 unchanged. E/19-dfcc-nine-blocked.png and saved repaired state. Exact-ten-only fixture was not exercised. |
| 1216073204565506: loaded damage display and one-spare replacement | Staged damage | PASS | NOT TESTED | Loaded `EngineDamaged=true`; interior displayed `Damaged !`. Bay engine fitting consumed the remaining spare (1->0), saved damage=false. E/18-damaged-loaded.png, 20-replace-damaged-drive.png, ios-repaired.json. Actual escape damage rolls and travel timing were not tested. |
| 1215683087492480: physical Engage, scan-triggered grapple-only ACC stop, retained fuel/cargo/filter | Staged ship, real daily scanning | PASS | NOT TESTED | Loaded piloted IOS at asteroids with grapple, supply pod/12 iron, fuel100, Earth->asteroids ACC filters iron/titanium, no AMA. Clicked red ACC lamp then Engage; saw Engaged before starting time, Disengaged by observed day3. It stayed orbiting with engine off, fuel100 and cargo12 through day88. Saved filters remained [iron]/[titanium]. Initial find type was not captured; do not infer it was selected/large. E/22-acc-engage.png, 23-acc-disengaged-at-find.png, acc-result.json. |
| 1215683087492480: manual capture and save/reload after disengagement | Staged | PASS | NOT TESTED | At day88 scanner showed unselected class3/250-ton silver asteroid. Grab captured it and cleared scan. Saved/reloaded; ACC remained disengaged. Saved held asteroid mass250/type12, orbit500/state700, fuel100/cargo12. E/24-grapple-scan-after-acc.png, 26-manual-grab-result.png, 27-acc-after-reload.png, acc-result.json. |
| Grapple dialog right-click dismissal over Grab | Staged | FAIL | NOT TESTED | Right-click on Grab left the dialog open; right-click outside it closed it. This narrow behavior needs controlled reproduction on the agreed current revision before a code change. It is separate from the passing bay equipment-modal check. |

### Preserved process failures

| Attempt | Sequence | Exit | Strict result |
| --- | --- | --- | --- |
| source-01, PID55224 | Craig's recruitment; Save/load; navigation; Earth->Training->Escape; set volume45%; title-bar X with settings open | 0 | FAIL: two allocator errors at exit |
| source-02, PID68472 | Title->Earth->Training->Escape; verify45%; restore defaults; Escape dismiss; title-bar X | 0 | FAIL: 60 shader-cache initialization errors; allocator errors did not recur |
| source-03, PID68300 | Title->Earth; staged MTX, IOS assembly, DFCC, repair and ACC/capture/save-load; Earth->Training->Escape; title-bar X with settings open | 0 | FAIL: three shader-cache write errors; allocator errors did not recur |

Exact diagnostics:

```text
source-01.log:156: ERROR: Pages in use exist at exit in PagedAllocator: 6Thread
source-01.log:158: ERROR: Pages in use exist at exit in PagedAllocator: N16WorkerThreadPool4TaskE
  at: ~PagedAllocator (./core/templates/paged_allocator.h:170)

source-02.log: ERROR: Condition "err != OK" is true.
  at: _initialize_cache (servers/rendering/renderer_rd/shader_rd.cpp:768)

source-03.log:124/126/128: ERROR: Condition "f.is_null()" is true.
  at: _save_to_cache (servers/rendering/renderer_rd/shader_rd.cpp:471)
```

Runs 1/2 used a long workspace-local APPDATA profile. Windows PowerShell also could not enumerate some of its deeply nested shader-cache paths, making path length a concrete setup concern. Run3 used `GetShortPathNameW` for the same profile; initialization errors disappeared, but three writes still failed. **The cache problem is not resolved, and its relationship to the first allocator errors is unproven.** No sleeps, log exclusions, rendering-backend changes or gameplay patches were used to obtain a passing label. Other retained warnings include Vulkan layer-manifest lookup, controls unable to grab focus and the game's lock/unlock diagnostics. There was no observed hang or native crash.

### Fixture and remaining-check boundaries

The normal game was never modified by fixture scripts: its slots1/2 remain separate. Settings Enable MTX supplied the initial staged world, including prerequisite station/shuttle/staff. E/stage-fixture.py then supplied researched hulls/drives/pods, a qualified orbital pilot and inventory to slot4. E/stage-damage.py derived a damaged IOS/nine-MeH fixture from the actually assembled save. E/stage-acc.py moved the repaired IOS/pilot to the asteroids, supplied grapple/cargo/fuel and route filters, and left Engage/scanning/capture to desktop controls. Every initial/observed state is retained. These scripts are preparation artifacts, not additions to the regression suite.

These remaining brief checks are **NOT TESTED in source and export**; no broader task acceptance is implied:

- Full normal research/production/mining/SCG/MTX campaign progression; active research and production roundtrip quantities, crew ranks/news fidelity, corrupt-save feedback, write-denied save directory, old-save compatibility and stale-world day subscriptions.
- Research/Production screen acceptance and production-to-Stores selection; inactive/hidden controls; complete right-click/hover matrix; controller input; fullscreen/resizing persistence; mute/volume listening and cue timing.
- SCG assembly and all five mounts, sixth-mount exclusion, hull research gates on bay entry, repeated creation, full/exact-capacity dismantling and repeat assembly cycles; shuttle fitting and fuel changes; salvage unloading through normal discovery.
- MTX paid manual/AOC manufacture, duplicate/repeat prevention, stock transfer/balance, capacity limits, station capture/loss/recapture, stale selector buttons and discovery through normal capture. Ground/unequipped/enemy MTX rejection was not exercised.
- ACC equal/unequal/odd stock balancing, overflow, shuttle ground/orbit routes, selected large asteroid and AMA mining variants, Complete Cycle, fuel waits and all route invalidation/cancellation cases.
- Engine-damage occurrence during war/escape, DFCC damage immunity, doubled travel/return ETA, repair without spare, repeated fitting and damaged-versus-healthy dismantle returns.
- DFCC exact-ten-only payment, 250 capacity, populated drones, SCG/HeD, fitting with full fuel storage/retry, combined cargo+tank return, ACC thresholds/remainders, gauge drain and legacy tanks.
- Supply-pod ditch across hulls/states, rename/tooltip edge cases, dynamic menus in transit, station status changes, construction frames, damaged-base menu restoration, Methanoid artwork/trade decisions, gifts, comms progression, News cancellation and OF deployment.

## Remaining work and review readiness

**Not ready to claim complete native Windows acceptance or any fully accepted Asana task.** This report provides focused source-control observations and reproductions, while all export checks, listening and the remaining matrix stay explicit.

Next actions through Craig:

1. Confirm whether continuation should stay on 44e37ba or use a newer supplied handoff. Preserve these observations under 44e37ba regardless. Newer code already contains shutdown follow-ups; no concurrent gameplay edit was attempted.
2. Copy the complete matching `artifacts/windows` directory into Craig's accessible workspace, with revision/hash evidence, then repeat applicable desktop checks on that export. Do not access or alter the builder checkout to obtain it without Craig's copy.
3. Isolate cache errors using a genuinely shorter fresh validation profile while preserving these saves/logs; repeat the minimal three-close sequence. Independently reproduce the allocator errors and grapple-control right-click behavior before changing code.
4. Obtain audible-quality confirmation and complete the untested progression/edge-case matrix. No automated headless result substitutes for those observations.

No gameplay fix or task closure is proposed from this partial pass. The local validation artifacts demonstrate successes and preserve failures for the next agreed revision.

## Second desktop pass and SCG pointer fix — 2026-10-02/03

Craig authorized continued desktop use while away. Baseline gameplay remained `44e37ba74c98fbf7f070eb77c051a3caa1fdbba2`; checkout HEAD was the first report-only commit `433877e4acba9dd5e848b6deb00e65f3f4f8e8b0`. Runs `source-04` and `source-05` used unchanged gameplay. Run `source-06-fixed` used the one-line scene fix and enhanced existing regression described below. `AGENTS.md` remains unchanged. Same pinned engine/SDK and GPU as the first pass.

Evidence **E2**: `artifacts/windows-validation/desktop-02/`. A fresh profile at `../wv02` (passed to Godot using its short Windows path) separates these staged saves from Craig's recruitment saves and the first pass. Automated case 263 used a separate `../wv-reg` profile. No builder checkout/process was changed. No export has arrived in `../export-inbox` and no Mac Pro reply was present in the shared coordination file at completion.

### Actual desktop observations

All scenarios below use **staged fixtures**, not normal campaign research/manufacture. Source observations came from mouse/Escape actions on the Windows game window. The fixture preparation scripts and before/after saves are retained in E2.

| Task / scenario | Source | Export | Observation and evidence |
| --- | --- | --- | --- |
| 1215685674676241 / SCG build button hit area | FAIL baseline; PASS focused fix | NOT TESTED | In an empty orbital bay with two SCG chassis, centre click `(350,100)` at 960x600 did nothing; left-edge `(325,100)` built the SCG. Screenshots 01–03. With the scene fix, the same centre click builds it; screenshots 19–20. |
| SCG assembly / stock conservation | PASS, narrow scope | NOT TESTED | Physically built one hull, fitted its star drive and all five mounts: supply/supply/tool/cryo/supply. Saved `scg-assembled.json` contains exactly one SCG, five modules and an installed engine. Deductions from `slot-1-initial.json`: chassis 2→1, star drive 2→1, ACC 2→1, supply pods 5→2, tool pods 3→2, cryo pods 2→1. No sixth mount appears. |
| SCG reload and five-row layout | PASS | NOT TESTED | Saved slot 4, loaded it through confirmation, re-entered bay and interior. All five rows render in interior/Cargo dialog without clipping. Empty supply/tool/cryo discard buttons are disabled. Escape closes dialog. Screenshots 04–07. |
| 1215716464570901 / DFCC exact-ten payment and restart | PASS, IOS/no drones | NOT TESTED | Fixture derives from the previously assembled IOS with tank zero and exactly ten MeH. Plus gives stock 0/tank 1; another Plus preserves 0/1. Saved slot 5 (`dfcc-paid.json`), closed the game, restarted and loaded it: still 0/1. Minus returns exactly ten MeH and leaves tank 0. Screenshots 08–12. |
| 1215685674676219 / fifth SCG supply-pod discard | PASS, docked staged cargo and save | NOT TESTED | Derived the physically built SCG with 11/22/55 iron in mounts 1/2/5. Ditch on mount 5 removes only its 55 iron; the supply pod remains fitted. Repeated click on now-empty mount and clicks on tool/cryo Ditch controls do nothing. Close button works. Saved `scg-cargo-result.json` preserves both neighbouring cargo amounts and exact ground/orbital stores compared with `scg-cargo-initial.json`. Screenshots 13–14. Post-discard reload and other hull/state combinations remain NOT TESTED. |
| Grapple dialog right-click follow-up | PASS in two controlled checks; earlier failure not reproduced | NOT TESTED | At the asteroids, right-click over Grab closes the dialog with no find. Repeated using the previously captured class-three/250-ton silver asteroid staged as a scan result: closes again. No time advancement or natural scan is claimed. Screenshots 15–18 and `grapple-find-initial.json`. Preserve the earlier observation; these checks do not prove its cause or justify a grapple-code change. |
| Child navigation after SCG fix | PASS | NOT TESTED | Pod-mount 5 and engine controls still respond to desktop pointer input. Screenshots 21–22. |

### Focused fix and red/green evidence

`Buttons/ShipNav` is an `HBoxContainer` drawn above the SCG creation button. Its rectangle starts at x=32 while the SCG button spans x=24..48, so the empty container intercepted clicks over most of that button even when its children were hidden. `Godot/Screens/ShipBay.tscn` now sets only that container's `mouse_filter = 2` (Ignore); child buttons retain their own input handling.

The existing case **263**, `ScgModuleMounts`, previously emitted the creation signal directly. It now unlocks the staged chassis and sends pointer motion/press/release at the button centre, asserting that a hull was created before testing the five mounts. It reuses the suite's input-event lifetime pattern; no new case or framework was added.

Commands, from the isolated checkout:

```powershell
& C:/Python314/python.exe artifacts/windows-validation/desktop-02/check-scg.py before
& C:/Python314/python.exe artifacts/windows-validation/desktop-02/check-scg.py before-authorized
& C:/Python314/python.exe artifacts/windows-validation/desktop-02/check-scg.py after
& C:/Python314/python.exe artifacts/windows-validation/desktop-02/launch.py source-06-fixed
& C:/Python314/python.exe artifacts/windows-validation/desktop-02/check-evidence.py
```

One distinct regression case, three recorded attempts; **no full automated suite, export or startup-smoke rerun**:

- `scg-before.log`: expected pointer assertion failure (`expected 1, got 0`) plus sandbox certificate-store diagnostic. Build: 14 warnings, 0 errors.
- `scg-before-authorized.log`: same assertion failure outside the sandbox, without that certificate error. Confirms the regression independently of the sandbox diagnostic. Incremental build: 0 warnings/errors.
- `scg-after.log`: **1 passed, 0 failed**, clean under the existing strict validator. Incremental build: 0 warnings/errors.
- Fresh desktop `source-06-fixed`: centre click succeeds; child navigation remains functional. This is source-only validation of the patch.

### Runtime findings and remaining scope

| Run | Gameplay | Exit | Strict runtime result |
| --- | --- | --- | --- |
| source-04 | Baseline 44e37ba, fresh shorter profile | 0 | FAIL: the two PagedAllocator errors recur on title-bar close after Save screen; no shader-cache errors. |
| source-05 | Baseline 44e37ba, same shorter profile | 0 | PASS: no strict engine errors on close from IOS interior after grapple checks. |
| source-06-fixed | SCG scene patch plus case-263 enhancement | 0 | PASS: no strict engine errors on close from SCG engine bay. |

All three retained Vulkan layer-manifest warnings. No hang/native crash was observed. These runs produced no shader-cache initialization/write errors with the shorter profile. This supports using that path for further tests; it does not repair an engine cache defect or explain the independent allocator recurrence. The patch does not change shutdown code, and a clean baseline exit already occurred before it.

The patch and these observations are ready for focused review through Craig/the shared local handoff. **Overall Windows acceptance and full task acceptance remain incomplete.** All export checks and audible-quality/controller checks remain NOT TESTED. The first-pass remaining matrix still applies except for the narrow source checks explicitly added here: SCG fitting/layout, exact-ten DFCC payment/restart and docked fifth-pod discard. Full/atomic dismantling limits, DFCC capacity/drain/SCG variants, MTX transfer/production, wider ACC/AMA, actual war damage and normal progression still need their own evidence. No push, PR, task closure or Asana update was performed.

### Communication after the second pass — 2026-10-03

At Craig's explicit request, the Windows desktop agent posted progress comments to these existing tasks at 10:36 UTC. They distinguish source/staged observations from export/normal progression, name local SCG fix commit `5a9b1ef`, and identify remaining work and the shared coordination file. They do not mark tasks accepted or alter fields. Created comment IDs:

- SCGs `1215685674676241`: `1219128556289884`.
- DFCC fuel `1215716464570901`: `1219128692386084`.
- Supply-pod discard `1215685674676219`: `1219128556304832`.
- ACC `1215683087492480`: `1219128692396037`.

## Fresh normal-progression playtest in progress — 2026-10-03

Launched the source game at `cae9fb988ec68f1c1ad2b382be67c53ef2b88583` (44e37ba plus SCG pointer fix and report commits) using a new empty `../wv03` profile and the same pinned toolchain. No cheats, progression presets, staged saves or game-state edits were supplied. Title-screen startup PASS; further agent-verified progression/save/shutdown outcomes remain NOT TESTED in this session. Craig took desktop input ownership at 11:17 UTC. Session record and logs: `artifacts/windows-validation/desktop-03/`.

Craig reports two missing sounds during normal play: **doors closing when training personnel**, and **production machinery while an item is in build**. These are human listening observations; the cause has not been independently reproduced or diagnosed. At Craig's explicit direction, audio investigation is deferred and gameplay/progression/controls take priority. Sound-inventory task `1215683087492491` has progress comment `1219128903362522`. This does not certify audio acceptance or completed normal-progression acceptance.

### Stocktaker time control and preserved normal save — 2026-10-03

Craig also reports that Advance Time uses the same cue as the bottom date bar, differing from the original game. Original-cue correspondence remains NOT TESTED by the agent; investigation is deferred. Asana sound comment: `1219128955625746`.

Craig left the normal-progression game on Stocktaker at day 219 and requested a save. Agent physical Save to empty slot 1 succeeded. A separate original copy is retained as `artifacts/windows-validation/desktop-03/craig-day219-slot1.json`; SHA-256 `5f7d8b39c913a5ffa3c43ae69be410fddb0ea357e5e5f60eb1ad843eae6136c7`. The active profile is `../wv03/Godot/app_userdata/Deuteros/saves/slot-1.json`. Its hash remained unchanged after testing and reloading. No fixtures or save edits were used.

| Scenario | Source | Export | Evidence / limits |
| --- | --- | --- | --- |
| Advance Time on Stocktaker, normal day 219 | FAIL before; PASS with local fix | NOT TESTED | Physical centre click left day 219 unchanged before fix. Invisible `Store.tscn/TradStore` Control occupies 40x40 at the origin and intercepts the time control. One-line `mouse_filter = 2` lets clicks reach the menu. After restart, physical clicks started/stopped time (219 to 241), while Stocktaker category switching still worked. Screenshots 01, 03–05. |
| Save and restart/load Craig's normal game | PASS | NOT TESTED | Slot 1 saved day 219; loaded after process restart, then reloaded unchanged after clock testing. Returned to Earth City Stocktaker at day 219 with time stopped. Screenshots 02, 06. |
| Original playtest close | FAIL strict logs; process exit 0 | NOT TESTED | `playtest-01` closed by Alt+F4 after saving. Both known PagedAllocator exit errors recur; no shutdown fix claimed. Fixed process remains open, so its shutdown is NOT TESTED. |

Existing time case 112 now loads the real Store scene and performs pointer hit testing with the menu and screen in the same viewport. It fails before the scene fix (`expected True, got False`) and passes afterward with strict clean logs. Adjacent cases 108–111 also pass with pointer input; those retain their existing menu-context-only setup and do not certify full underlying scene interaction. Five distinct cases, no full suite rerun. Build before: 14 warnings/0 errors; after and adjacent checks: 0 warnings/0 errors. Raw logs and runner: `artifacts/windows-validation/desktop-03/stocktaker-*.log` and `check-stocktaker.py`.

Fix is local and uncommitted over `cae9fb988ec68f1c1ad2b382be67c53ef2b88583`, in `Godot/Screens/Store.tscn` and `Godot/Tests/RegressionRunner.TimeAnimation.cs`; review patch is `artifacts/windows-validation/desktop-03/stocktaker-time-fix.patch`. Asana Stores task `1215685674676245` progress comment `1219129123049970`. No push, PR or task field/status change. Fixed-source run `playtest-02-stocktaker-fixed`, PID 67588, is left open for Craig. Export remains unavailable; overall acceptance remains incomplete.

### Shuttle service artwork had no action — 2026-10-03

Craig continued normal progression to day 439 and identified the grille-shaped service control at the top-left of the interior panel, just right of the main menu (window pointer approximately 188,99 at 3x scale). The first exploratory bottom Dock click was a different control; Craig clarified the exact target with his cursor. No Dock behavior was changed.

| Scenario | Source | Export | Evidence / limits |
| --- | --- | --- | --- |
| Service control in ground shuttle interior | FAIL before; PASS with local fix | NOT TESTED | The indicated artwork had no Button node or callback. Added a flat `Service` button over the existing art at game coordinates 56,16–72,32 and a handler opening the correct ShipBay. Physical click at the reported position opens the ground service bay after restart. Normal progression; screenshots 07, 10–12. |
| Preserve and resume Craig's shuttle position | PASS | NOT TESTED | Saved day 439 into empty slot 2; slot 1 day 219 untouched. Exact backup `desktop-03/craig-day439-slot2.json`, SHA-256 `8efaf0e0b9c4789ea89b1b5871c48aadc5f5747d8fcef1184014ff8837ca18ff`. Restart/load and successful service navigation leave day 439 unchanged; save hash still matches backup. Screenshot 08. |
| Service routing and availability | PASS, staged regression | NOT TESTED | New case 310 covers pointer navigation for ground/orbital shuttle, IOS and SCG, selected ship/berth/inventory context, unchanged day/fuel/state, input blocking, and rejection of stale signals while undocked, in transit, docking, on asteroids, at enemy or absent stations. Other-hull physical acceptance remains NOT TESTED. Original-game availability beyond Craig's requested ground-shuttle behavior is unverified. |
| Existing interior menu transitions | PASS, staged regressions | NOT TESTED | Cases 102–104 pass for entry context, shuttle takeoff/landing and interplanetary departure/arrival. These are existing tests, separate from physical normal progression. |
| Stocktaker-fixed playtest shutdown | PASS this run | NOT TESTED | `playtest-02-stocktaker-fixed` exited 0 via Alt+F4 after day-439 save; no strict engine-error patterns. Earlier allocator failures remain unresolved. |

Only four distinct focused cases were run: 310 red before (`expected ShipBay, got ShipInterior`), then green after; 102–104 green. Both before/after builds: 14 existing warnings, 0 errors; incremental context build: 0 warnings/errors. Strict logs and runner: `desktop-03/service-*.log` and `check-service.py`. No full suite or export rerun.

Service fix changes `ShipInterior.cs`, `ShipInterior.tscn`, `RegressionRunner.InteriorBacklog.cs` and the final registration in `RegressionRunner.cs`. Standalone patch: `artifacts/windows-validation/desktop-03/service-button-fix.patch`, separate from the Stocktaker patch. Craig subsequently confirmed the service button works and authorized its local commit: `878d74dc69b48e2144a1ab3ce7dd38e6a82cb165`. Stocktaker remains uncommitted. Asana interior-navigation task `1215691951441136`, original progress comment `1219129630407301`, confirmation/commit comment `1219129517825462`; no task fields changed. Source run `playtest-03-service-fixed`, PID 60504, was returned to Craig in the ground service bay at day 439; Craig has continued playing since. Its eventual shutdown is NOT TESTED. Game push/PR remains on hold.

### Unavailable supply pod and loading animation — 2026-10-03

Craig reports selecting Supply Pod in the shuttle's middle module when none is available should show an error, as in the original. He also reports cargo-pod fitting appears immediately without a loading animation. These are normal-progression user observations; the agent did not interrupt his ongoing game to reproduce them physically.

| Scenario | Source | Export | Evidence / limits |
| --- | --- | --- | --- |
| Unavailable pod fitting feedback | FAIL before; PASS staged after local fix | NOT TESTED | Shared `ShipBay_ModuleChanged` silently rejected missing/locked supply, tool and cryo pods. Reuses `GameCore.ShowError` with `Pod Not Available`, before any mutation. Wording is not claimed as an original-game transcription. |
| Rejection/dismissal and successful fitting/removal | PASS staged case 311 | NOT TESTED | All three pod types, ground/orbital shuttle stores, absent stock and locked technology; stock/module unchanged on rejection, error button dismisses, available pod consumes one, removal returns one. Scene button signals, not physical pointer acceptance. |
| Normal-progression popup display after restart | NOT TESTED | NOT TESTED | Craig retains `playtest-03-service-fixed`, which does not load this new code until restarted. No save edits or restart during this check. |
| Cargo-pod loading animation | FAIL against Craig's reported expectation; unfinished | NOT TESTED | User sees instantaneous fitting; code inspection confirms the fitting path updates its sprite immediately. No loading-animation implementation or intentional temporary-mode switch was found in this path. Original timing/movement remains NOT TESTED; animation work deferred while gameplay is prioritized. |

Case 311 was appended after 310 without renumbering existing tests. Red before: `unavailable pod explains rejection: expected True, got False`; green after: 1 passed, 0 failed, strict clean log. Before/after builds each have 14 existing warnings and 0 errors. Logs/runner: `artifacts/windows-validation/desktop-03/pods-*.log`, `check-pods.py`. No full suite rerun. Three-file patch `unavailable-pod-feedback.patch` applies over `878d74d`; source and regression changes remain local/uncommitted, separate from Stocktaker. Shared coordination and a related supply-pod task comment record the follow-up without completing Ditch/AMA acceptance. Physical popup/layout and matching export remain outstanding.

## 2026-10-03 - Craig resumed slot 3

Craig exited playtest-03-service-fixed himself (exit 0). No ERROR/SCRIPT ERROR/Exception/PagedAllocator matches; Vulkan registry layer-manifest warning remains in its log. Latest normal save is slot 3, day 981. Preserved byte-for-byte backup desktop-03/craig-resume-slot3-20261003.json, SHA-256 8e15177ca802a14f67f4dd9776ca70f2ea27ed5a2d9bee3c1c717940e1b3f67c; original slot hash still matches after load. Slots 1/2 remain day 219/439.

Launched source playtest-04-pods-fixed, PID 63760, HEAD 878d74d plus local Stocktaker/missing-pod changes, same wv03 profile. Physical Save/Load -> slot 3 Load -> Yes restored day 981 (PASS source normal progression; export NOT TESTED). Initial post-load Master Control is blank; selecting Earth opens Earth City with populated controls. This visual behavior is recorded, not diagnosed. Screenshots 13-15 under desktop-03. Left in Earth City at day 981 for Craig; no time advancement or save writes. New popup is now in the loaded build but physical popup verification remains NOT TESTED. Craig owns input; do not stop or compete with PID 63760. No game push/PR.

## 2026-10-03 - Popup text overflow fixed

Craig confirmed the missing-pod popup works, but its box size is wrong. Physical source observation: the single-line text overflows the fixed 128x57 box (screenshot 17; screenshot 16 is occluded). Shared Error.tscn now uses native PanelContainer/MarginContainer minimum sizing and centred anchors, retaining border/background and existing node paths/dismissal. No new layout script.

| Check | Source | Export | Evidence / limits |
| --- | --- | --- | --- |
| Popup contains text with padding and stays centred | FAIL before; PASS staged after | NOT TESTED | Case 311 strengthened; before fails containment. After passes for pod text and longer multiline storage/production errors within viewport, plus all existing unavailable/fitting checks. |
| Normal-progression missing Tool Pod popup | PASS after layout fix | NOT TESTED | Physical pointer at day 981 opens compact centred popup, all text enclosed; screenshot 18-popup-layout-fixed.png. Original-game typography/geometry fidelity unverified. |
| Save/restart/load | PASS normal progression | NOT TESTED | Saved into previously empty slot 4 day 981; restored after restart. Backup craig-day981-slot4-popup-layout.json and active slot hash 3759e31f26179f5b695232f37b205b50436b3f657cbea1c5947671a0c1c7688d match. Earlier slots preserved. |
| Previous source close | PASS this run | NOT TESTED | playtest-04-pods-fixed exited 0 via Alt+F4; no ERROR/SCRIPT ERROR/Exception/PagedAllocator matches. |

Builds before/after: 14 existing warnings, 0 errors. One focused case, strict clean after; no full suite. Logs popup-layout-before/after*.log, runner check-popup-layout.py in artifacts/windows-validation/desktop-03. Four-file unavailable-pod-feedback-v2.patch supersedes v1; based on 878d74d, still uncommitted, includes pod feedback, shared error scene and case 311. Separate Stocktaker patch remains local. Updated Asana related supply-pod task; no fields changed or game push/PR.

Current source playtest-05-popup-layout PID 30940 (exec session 51653) is left at day 981 with the corrected popup visible for Craig. New process includes service, Stocktaker, missing-pod and popup-layout fixes. Craig owns input; do not stop or run competing UI checks. Current shutdown NOT TESTED. Mac agent: review v2 rather than applying both patches; export remains pending.

## 2026-10-03 - Background return from all shuttle service sections

Craig accepted the corrected missing-pod box and current wording for now; he confirms the text differs from the original. Exact original message is deferred, not a blocker for this local feedback fix. Asana supply-pod task updated with his acceptance.

New report: empty-space click in module servicing should return to main cockpit. Root cause: Torso.tscn/OpenShipInterior is full-rect anchored but offset (85,101), making most of the section unclickable. Removed its four offsets to match cockpit/engine service layouts. Existing shared callback and control stacking reused.

| Check | Source | Export | Evidence / limits |
| --- | --- | --- | --- |
| Background return across module mounts | FAIL before; PASS staged after | NOT TESTED | New case 312 pointer tests shuttle, IOS and all five SCG mounts; same ship/day/fuel, pod fitting/cargo controls, input blocker and cargo-panel lock. Before: expected ShipInterior, got ShipBay. |
| Shuttle cockpit-section background return | PASS normal progression | NOT TESTED | Physical blank-area click at window 500,235 opens main cockpit; screenshots 23-24. |
| Shuttle mid-section background return | PASS normal progression | NOT TESTED | Physical reported blank-area click at 343,277 opens main cockpit; screenshots 21-22. |
| Shuttle engine-section background return | PASS normal progression | NOT TESTED | Physical blank-area click at 343,277 opens main cockpit; screenshots 19-20. |
| Save/restart/load landed shuttle | PASS normal progression | NOT TESTED | Craig landed at day 1002. Saved empty slot 5, exact backup craig-landed-slot5-background.json; SHA-256 798ad254482c2d5b24702cfa599f7d98646e145067abffdf97ac4e7ee22d860f. Active slot still matches after checks. Earlier slots preserved. Day 1002/fuel 191 unchanged through navigation. |
| Previous source close | PASS this run | NOT TESTED | playtest-05-popup-layout exited 0; no strict engine-error matches. |

Focused case 312 only; before build 14 existing warnings/0 errors, after incremental build 0 warnings/errors; strict after log clean. Logs/runner: desktop-03/bay-background-before/after*.log, check-bay-background.py. No full suite duplicated. Other-hull physical checks remain NOT TESTED.

Source playtest-06-bay-background PID 8340 (exec session 35088), HEAD 878d74d plus local Stocktaker, pod feedback/layout and background fix, is left in main cockpit for Craig at day 1002. Current shutdown NOT TESTED. Craig owns input; do not stop or compete. All five save slots now occupied; preserve originals/backups before any later overwrite.

Mac handoff: six-file pod-and-bay-feedback-v3.patch applies over 878d74d and SUPERSEDES unavailable-pod-feedback v1/v2. Includes ShipBay.cs, Error.tscn, Torso.tscn and case 311/312 test changes. Do not apply both patch generations. Stocktaker remains separately patched. Latest fixes uncommitted; service fix remains commit 878d74d. Asana navigation and pod tasks updated; no task fields, game push or PR. Export still pending.

Craig confirmed all three shuttle service sections verified: cockpit, mid-section and engine empty-space clicks return to the main cockpit. Human acceptance recorded for the current source build; export remains NOT TESTED. No further change requested.


## 2026-10-04 - Shuttle storm-door pixelation

Craig requested original blast-door graphics after spotting excessive pixelation during shuttle departure. The expanded 192x112 location view was loading the 60x30 thumbnail. One-line ShipInterior.cs fix selects the existing 192x112 BigLocation_StormDoors.png; small preview unchanged. Focused staged source case 313 FAIL before, PASS after, strict clean log. Builds each 14 existing warnings/0 errors. Normal-progression visual check after restart and export NOT TESTED; no live game restart/save changes in this investigation.

Recovered original door record 3 from Disk 2 bank 0xBB000 in archived local deuteros-parallel commit faf3a30beb07868e06e01ebb306f63b2b42affed. All 21,312 pixels of the 192x111 recovered image match the existing large PNG under a consistent palette, and all eight used colours match original Disk 1 palette 0x79D24. The existing PNG's extra final row is outside that comparison. Original frame: desktop-03/original-stormdoors-04/original-stormdoors.png; comparison-palette.json and docs/original-stormdoors-evidence.md record hashes, offsets, decoding limits and failed exploratory attempts. No invented/repainted art or animation changes.

Source acceptance: PASS staged texture selection and original-pixel comparison; NOT TESTED normal-progression appearance/export. Existing live playtest, if still running, retains old code until restart. No full suite duplicated. New code/test remains uncommitted over 878d74d. desktop-03/desktop-gameplay-v4.patch contains eight code/scene/test files for pod feedback/layout, bay-background and storm-door fixes; it supersedes pod-and-bay-feedback-v3.patch and earlier pod patches. Stocktaker remains separate. Original-evidence documentation is a separate new file to collect. No game push/PR or builder-checkout modifications. Export inbox still empty.

## 2026-10-04 - Craig playtest resumed after storm-door fix

PASS source / normal progression: launched playtest-07-stormdoors-fixed (PID 62688, exec session 19735), HEAD 878d74dc69b48e2144a1ab3ce7dd38e6a82cb165 plus documented local fixes. Physically loaded slot 5, day 1002, selected Earth City then shuttle interior. Left at Earth ground bay, fuel 191 T, pilot Adamson, crew 41, engine disengaged, cargo empty. Slot 5 SHA-256 remains 798ad254482c2d5b24702cfa599f7d98646e145067abffdf97ac4e7ee22d860f; no saves overwritten, no time advanced. Known partial/blank Master Control immediately after load recurred; selecting Earth restored navigation. Storm-door normal-departure visual acceptance and export remain NOT TESTED. Craig owns input for continued play. No existing Godot processes were running before launch; protected builder checkout untouched.

## 2026-10-04 - Completion text and orbital view follow-ups

Craig confirmed blast doors fixed in the live source playtest (normal progression, playtest-07-stormdoors-fixed). Source human acceptance PASS; export NOT TESTED.

Factory completion report: Craig heard typing but could not see the final operational line. Root cause in shared ModuleTextFrame/Line.GetText: normalized float colour channels were cast to integers and emitted without HTML # syntax (green became 000000). One-line fix uses Color.ToHtml(false). Focused staged source case 314 FAIL before, PASS after, including factory/base completion playback, parsed text, window content height and alien-colour serialization. Initial sandbox run failed certificate-store access; unrestricted reproduction and fix logs retained as deployment-text-before-unrestricted / after. Fixed build 14 existing warnings, 0 errors; strict log clean. Source normal-progression acceptance after restart and export NOT TESTED.

Orbital-view report: Craig reports both screens missing the planet after station departure. When observed, he had moved to Research, day 1104; no input was taken. Source inspection found expanded orbital view explicitly null and mini-screen refreshed only with expanded view closed. Fix wires the included Ship_View_Planet.png through BigLocation_Planet.tres (192x112 atlas, cropping only black right/bottom margins from 200x120; no nonblack pixels discarded), and updates the mini-screen in both modes. Existing small planet/station and neutral fallback icons retained. Staged case 315 FAIL before-corrected, PASS after-fit for shuttle/IOS/SCG departure, view toggles and return docking. Initial test selected Dock instead of TakeOff; corrected fixture before product edit. Initial after run caught 200x120 texture dimensions; atlas corrected sizing without stretching. All failed attempts preserved. Final build 14 existing warnings / 0 errors, strict log clean. The large blue artwork is existing supplied art, not proof of original per-planet colour/station fidelity. Source normal progression and export NOT TESTED.

Live playtest 07 remains untouched, with old loaded code; save/restart needed before human acceptance of these two changes. New ACC flashing report under investigation; no ACC change claimed yet. All source changes remain local over 878d74d; no push/PR, full automated run or protected builder activity.

ACC cockpit flashing: Craig explicitly defers implementation pending original-game comparison; suspected bug / INVESTIGATION REQUIRED, not a confirmed original mismatch. Current button is static red. Determine fitted-idle, engaged and Complete Cycle colours/cadence in original before changing it. Asana ACC task 1215683087492480 comment 1219141211426959. No ACC changes made.

Focused neighbours 104 (travel/arrival icon) and 313 (storm doors) PASS after orbital-view change, strict logs orbit-neighbour-104/313. No full suite run.

Coordination hub message received from Mac session 6e597818-7fc4-59d8-968d-c90a40e8ba52, thread 217443da35594e5eb12be2b2f6c061a0. Mac cannot read this protected directory via SSH; use hub plus an evidence package in builder Downloads, without changing permissions. Latest message reports Mac runtime f99b462 / 608 cases, not independently Windows-tested here. Preserve existing saves; newer-build save compatibility warning means use a separate profile for any future bundle validation. Do not replace live source playtest or claim newer source tested.
