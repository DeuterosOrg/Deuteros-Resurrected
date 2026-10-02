# Windows agent brief

## Windows Codex starting brief

**Updated 2026-10-02; latest follow-up corrects ACC Complete Cycle.** Work on the latest pushed `codex/build-tests-and-gameplay-fixes`, recording its actual full SHA and any local changes. Preserve root `AGENTS.md`; its original toolchain/testing notes are superseded by the current README and commands below.

**Objective:** establish native Windows confidence in the existing contribution, then continue toward all 48 Asana tasks. The committed Mac record reports **329/329 headless regressions**, startup smoke and Windows cross-export passing, with **31/48 task-level implementations**. None has completed desktop Windows acceptance. Native Windows automation now passes all 329 cases at `d057b9d`; physical desktop checks and the separate native Mac shutdown failure remain outstanding. See [latest validation](validation-results.md#acc-complete-cycle--2026-10-02).

1. **Pick up the branch safely.** Inspect local changes and remotes, fetch from the remote Craig pushed to, and check out the contribution without discarding work. Do not substitute `develop` if it is missing. Read [backlog progress](backlog-progress.md) and [validation results](validation-results.md).
2. **Build and validate.** Follow [the PowerShell setup](#windows-setup-and-automated-validation): Godot **4.2.2 .NET**, .NET SDK **6.0.428 x64**, Python **3.9+**. Run validator unit tests and full validation with Windows export. Preserve each attempt; investigate failures before retesting.
3. **Test actual Windows gameplay.** Follow [interactive acceptance](#interactive-acceptance-pass) and the feature checks below in both source and fresh export. Prioritize physical navigation/audio/window close, save/load, MTX, ship assembly, supply-pod discard, engine damage and DFCC fuel. Label staged fixtures separately from normal progression.
4. **Return evidence, then continue fixes.** Create `docs/windows-validation-results.md` using [the report template](#deliverable), update the local task ledger, and continue unresolved tasks in focused commits. Record unavailable desktop/hardware checks as **NOT TESTED**, then continue independent work.

Complete Windows testing before recommending a team PR. Keep publication and Asana changes for Craig's instruction. Ignored Mac artifacts do not travel with Git; regenerate logs and builds locally.

**Launch instruction:** “Read `docs/windows-agent-brief.md` and execute its Windows validation and backlog workflow.” Pushing makes this file available; it does not launch Codex. The Windows session must be started with this instruction.

## Feature acceptance details

### Complete Cycle and shutdown follow-up

On a revision containing cases **314–329**, verify Cycle from each endpoint and during transit/fuel waits. It should finish one leg, unload, retain overflow, and stop without refuelling or loading return cargo. Engage should cancel the finish request; save/reload should preserve it. Check the Finishing and Refueling labels. Repeat with shuttle and IOS, then close the source and exported game normally.

A separate **native Mac case 315 failed during shutdown** with `!rc_owner` after its assertions passed; case 314 exited cleanly and the full 329-case headless run passed. Keep this failure visible in your report. Windows now passes all 329 cases at `d057b9d`, including case 315, but this does not establish physical close behavior or resolve the native Mac failure. Preserve every attempt and investigate any crash/hang before retesting.

### Supply-pod discard acceptance

The latest gameplay follow-up implements task **1215685674676219** through **Cargo...** in the ship interior. Open it, then use **Ditch** beside the chosen supply pod. The [original control and mutation](original-supply-pod-evidence.md) establish discard with no store credit; docked access through this modern dialog follows the task request and is not proof of original screen availability.

Repeat with shuttle, IOS and all five SCG slots; include docking, transit, active ACC and AMA mining. Verify only the selected cargo disappears, its supply pod stays fitted, both ground/orbital stores and neighboring pods are unchanged, and tools/cryo cannot be ditched. Check empty pods, Close/Escape, save/reload, input locks and leaving the screen. Keep normal mining progression distinct from staged fixtures. Cases **265–275** and native Mac captures cover the local implementation; record Windows source/export outcomes separately.

### Ship assembly acceptance

The current follow-up adds passing cases **249–263** for stock conservation and SCG assembly; native Mac cases 250/252/259/260/263 also passed. The [latest validation result](validation-results.md#ship-assembly-and-inventory-conservation--2026-10-02) records the separate case-61 failure and the 262/263 continuation result. On Windows, verify bay entry leaves research locked, the SCG selector follows its own chassis technology, each hull/drive fitting consumes one local part, and repeat creation cannot put two ships in one berth. Fit all five SCG pods and confirm there is no sixth mount. Dismantle powered and unpowered hulls with full and exactly sufficient part storage; repeat a build/fit/dismantle cycle and save/reload between steps.

Treat inventories from pre-fix saves separately: historical part deductions cannot be reconstructed. Normal SCG discovery and manufacture remain acceptance gaps; the parallel port's Sol-cleared unlock trigger is documented as a reconstruction, not verified original behavior.

### Menu sound acceptance

Menu-sound tests 212–219 now accompany the implementation: one persistent player uses the supplied cue for top/side menus, time toggle and hold. All eight focused cases pass; native Mac cases 212/216–219 also pass. The full 219-case run, startup smoke and fresh Windows cross-export also pass; use the actual pushed SHA for Windows validation.

On Windows, listen while activating those controls with the physical mouse, keyboard and a controller where available. Verify scene changes do not truncate the cue; disabled/empty/hidden controls, cancelled presses and right-clicks stay silent. Holding time should sound once on press, with no tick/release cues. Check rapid navigation, mute/volume and window close. Automated `ui_accept` events cover the action route, not physical controller hardware or listening quality. Original-game cue correspondence remains unverified.

## Mission and handoff

Craig's goal is to resolve **all 48 open Asana tasks**, including missing features and original-game research questions. The first contribution supplies build tooling, regression tests and a batch of fixes. It does not resolve the entire backlog.

After Craig pushes `codex/build-tests-and-gameplay-fixes`, work from that branch. First establish the native Windows baseline, verify the existing fixes in the exported game, then continue the backlog in focused, tested changes. Inspect the working tree before switching branches; preserve existing work. Coordinate file ownership if another agent is still working on this branch.

**Windows Codex starting instruction:** “Read `docs/windows-agent-brief.md`, record the checked-out commit, and complete its Windows validation and backlog workflow.” This file supplies the handoff; pushing it does not itself launch an agent. Complete native Windows testing before recommending a team-facing PR.

Read these files first:

- Root `AGENTS.md` — preserve it. Its original engine pin and testing/export description predate this contribution; use Godot 4.2.2 and the current commands below and in the README.
- [README](../README.md), [CONTRIBUTING](../CONTRIBUTING.md), and [testing details](testing.md).
- [Validation results](validation-results.md) — verified work and unresolved runtime failures.
- [Asana reconciliation](asana-triage.md) — all 48 task IDs, source locations and acceptance questions.
- [Backlog progress](backlog-progress.md) — current implementation and verification status; coordinate active items before editing.

Keep work local until Craig authorizes publication. Do not push, open/merge PRs, or change/comment on Asana tasks merely because a test passes.

## Start here on Windows

Use the latest pushed head of **`codex/build-tests-and-gameplay-fixes`** and record the actual checked-out SHA. The station-display batch was `c7b3968`; MTX route safety was `77d296f`, installation/Stores was `78c6c48`, followed by the Godot 4.2.2 script-lifetime fix. Reconcile subsequent changes with the progress ledger before using the counts below.

1. Inspect local changes and `git remote -v`, then fetch the contribution branch from the remote where Craig pushed it. Do not assume `origin` is Craig's fork: the Mac checkout currently points to `tonyoddspherecom/Deuteros-Resurrected`. Check out the latest pushed state without discarding local work. If the branch is unavailable, report the missing remote/branch rather than substituting `develop`. Preserve root `AGENTS.md`.
2. Run the pinned setup and full validation below. Investigate any stall or strict engine failure; retain failure evidence before any diagnostic rerun.
3. Complete the source-game and exported-game acceptance passes. Record each scenario as passed, failed or not tested, with the commit and reproduction steps.
4. Write `docs/windows-validation-results.md`, update task evidence in the local ledger, then continue unresolved backlog items. Keep Windows validation and subsequent gameplay changes in separate commits.

Task **1215683087492485 — Add MTX module** now includes local installation on production completion, duplicate/repeat prevention, Stores access checks and save/load regressions, supported by [original instructions](original-behavior-evidence.md#1215683087492485--mtx-installation). Prioritize its Windows acceptance below alongside the known runtime failures, then continue unresolved rows in the ledger. Coordinate ownership before editing if Mac work has resumed.

Pushing this branch makes the brief available to the Windows agent; it does not start Codex or automatically run the validation workflow. The current CI push trigger targets `develop`; PR and manual workflow triggers are separate.

## Current evidence

The latest Complete Cycle batch passes **329/329 headless Mac cases**, import, startup and cross-export, plus nine Python tests. A separate native Mac teardown failed at case 315; [details and limits](validation-results.md#acc-complete-cycle--2026-10-02) remain part of the current result. Windows automation now passes all 329 cases at `d057b9d`, including release export and packaged startup.

The preceding AMA Mac suite had **313 cases**, including four AMA cargo regressions. **313/313, strict import, startup smoke and cross-export passed**; the later tooling update has nine passing Python tests. See [the result](validation-results.md#ama-compatible-cargo--2026-10-02). The local task count remains **31/48** because wider AMA research is incomplete. Windows results below cover their explicitly named revisions.

Native Windows now has a [passing full baseline](windows-validation-results.md) at **`d057b9d`**: **329/329 regressions**, nine Python tests, strict import, source smoke and release export passed in one run. The packaged executable also passed an external headless smoke check. The earlier missing-RCEdit export failure is preserved; the committed installer/validator correction is verified. Continue desktop acceptance on this revision or a separately validated newer one.

The preceding Mac suite had **309 cases**, including five grapple-only ACC regressions. **Full validation passed 309/309 in one run**, strict import, startup smoke, Windows cross-export and all six Python validator tests. Native Mac cases 306/308/309 pass. There are **31 task-level implementations** requiring Windows acceptance. See [the latest result](validation-results.md#grapple-only-asteroid-acc--2026-10-02) and [original control trace](original-asteroid-acc-evidence.md).

The preceding suite had **304 cases**, with eleven DFCC fuel regressions. **Full validation passed 304/304 in one run**, strict import, startup smoke, Windows cross-export and all six Python validator tests. Native Mac cases 294/296/298/300/301/303 also pass. Consult [the latest aggregate/export status](validation-results.md#dfcc-fuel-cost-and-conservation--2026-10-02). There are 30 task-level implementations requiring Windows acceptance.

The preceding suite had **293 cases**, including 17 engine-damage/recovery regressions. **Full validation passed 293/293 in one run**, strict import, startup smoke, Windows cross-export and all six Python validator tests. Native Mac cases 277/280/285/287/291/292/293 pass; consult [the latest aggregate/export status](validation-results.md#engine-damage-and-recovery--2026-10-02). At that checkpoint, 29 task-level implementations required Windows acceptance.

The preceding suite had **276 cases**. A controlled reproduction connects pending resource finalizers to skipped weak-reference cleanup during engine shutdown. The fix collects and drains finalizers after scene/audio release, before quitting. Cases 183/184/276 each pass four focused repetitions; native Mac cases 23/150/183/184/205/264/276 pass. **Fresh full validation passed 276/276 isolated cases in one run**, strict import, startup smoke and Windows cross-export; all six Python validator tests pass. See [the latest aggregate/export result](validation-results.md#shutdown-finalizer-drain--2026-10-02). Windows physical window close and exported-game acceptance remain required.

The preceding suite had **275 cases**, including eleven supply-pod cases. Its full attempt and continuation failed strict teardown at **cases 183 and 184** after passing MTX assertions, with `!rc_owner`, texture/scene reference leaks and leaked RIDs. Cases 1–182 and 185–275, startup smoke and a fresh Windows cross-export passed: **273/275 across three runs, not a green aggregate**. All eleven new cases pass. See [the current supply-pod result](validation-results.md#supply-pod-discard--2026-10-02) for evidence and limits. Native Mac checks and screenshots support the discard implementation; all 28 task-level implementations still require native Windows acceptance.

The preceding suite had **264 cases**, including a passing regression for prematurely disposed injected mouse events. Its full attempt failed at **case 23** with a native teardown crash. Cases 1–22 and continuation 24–264, startup smoke and a fresh Windows cross-export passed: **263/264 across two runs, not a green aggregate**. Native Mac cases 61/65/212/232/264 passed. See the [input-lifetime diagnosis and current run status](validation-results.md#queued-test-input-lifetime--2026-10-02). The validator has six passing unit tests and now explicitly rejects native crash/fatal headers. Keep physical Windows mouse/navigation/window-close acceptance separate from the injected-input fix.

The preceding **263-case** assembly-batch aggregate failed at case 61 with three GC-handle errors. Cases 1–60 and continuation 62–263 passed, as did startup smoke and a fresh Windows cross-export: **262/263 across two runs, not a green aggregate**. See the [current assembly result](validation-results.md#ship-assembly-and-inventory-conservation--2026-10-02) for the continuation and acceptance limits. Do not report a green aggregate based on the earlier run.

The preceding suite had **248 cases** and **27 task-level fixes with local regression evidence**, plus partial trading and SCG feature work. The hull-travel batch passed **248/248 in one full run**, strict import, startup smoke and fresh Windows export; five Python validator tests pass. Native Mac cases 241/243/246/247 passed and the IOS rejection message was visually inspected. Cases 236–248 cover IOS/local/SCG routing, metadata, ACC inventory conservation and elapsed arrival. See [hull validation](validation-results.md#hull-travel-restrictions-and-elapsed-arrival--2026-10-02); original interstellar duration and Windows gameplay remain unverified.

The preceding trading-decision batch passed **235/235 in one full run**, strict import, startup smoke and a fresh Windows export; five Python validator tests pass. Six reproduced defects include automatic acceptance, invalid cargo counting, war gating, duplicate gifts and interrupted text locks. Native Mac decision/input/cleanup checks passed and the corrected dialog was visually inspected. The first aggregate stopped at a timing-sensitive menu-cue assertion; a controlled slow-navigation probe proved the assertion could fail after successful cue completion, and the test was corrected before this fresh run. See [trade and validation evidence](validation-results.md#methanoid-trading-decisions-and-interrupted-text--2026-10-02). Original trade timing and Windows acceptance remain incomplete.

The preceding menu-click batch had **219 cases** and passed **219/219 in one full run**, startup smoke, a fresh Windows cross-export and five Python validator tests. Native Mac cases 212/216–219 passed. The exported pack contains the menu scene, cue import and sample. This does not certify audible quality or Windows gameplay. See [menu-sound evidence](validation-results.md#persistent-menu-click-feedback--2026-10-02).

The preceding construction-frame batch had **211 cases**. Nine supplied construction frames now render for IOS chassis/drive and resource-station frame. Cases 206–211 cover manual/AOC production; native Mac cases 206/208/210 passed all 32,256 frame-pixel comparisons. **211/211 passed in one full run**, along with startup smoke, fresh Windows cross-export and five Python validator tests. All 17 new resource entries were verified in the export's pack directory. The last recompilation retains 14 existing warnings. Windows rendering/gameplay and original timing remain pending. See [frame-recovery evidence](validation-results.md#supplied-construction-frame-recovery--2026-10-02).

The preceding ambience batch wired Store/Ship Bay audio and changed window close to release scenes before a bounded audio-mixer drain. Its first full attempt exposed a case-10 playback leak; after the production shutdown fix, **205/205 passed in one full run**, with startup smoke and Windows cross-export. Cases 200–205 cover those audio paths; native Mac cases 10/150/205 also exited cleanly. Native Windows listening/gameplay and physical window-close acceptance remain pending. See [audio/shutdown evidence](validation-results.md#store-and-ship-bay-ambience-and-audio-shutdown--2026-10-02).

The preceding menu-artwork batch restored the exact Methanoid face and damaged ground-service icons, disabled damaged services, and restored them after normal shuttle repair. Cases 192–199 passed focused checks; native Mac captures were inspected. Its full attempt passed 1–149, then case 150 passed gameplay assertions but hit `FATAL: Condition "!rc_owner" is true` during shutdown and timed out after 180 seconds. Continuation cases 151–199, startup smoke and Windows cross-export passed: **198/199 across two runs, not a passing aggregate**. Retain that historical failure; the new mixer-drain result does not establish its root cause. See [menu batch evidence](validation-results.md#methanoid-and-damaged-base-menu-artwork--2026-10-02).

The engine/SDK pin remains **Godot 4.2.2**. Its script-lifetime batch had **191 cases**. The patch contains the upstream fix for the captured script-registration deadlock. New stress case 191 timed out on 4.2.1 and passed on 4.2.2; focused cases 150, 153 and 65 also passed. **Clean-cache validation passed 191/191 isolated cases**, startup smoke and a fresh Windows cross-export. Five Python validator tests passed; compilation has 14 existing warnings and zero errors. Native Mac cases 187/190 passed and their captures were inspected; native Windows gameplay remains untested. See [engine evidence](validation-results.md#godot-422-script-lifetime-fix--2026-10-02). That runtime correction did not increase the then-current 25 Asana-linked implementation count.

The source baseline was `9817216`. The initial macOS arm64 contribution passed compilation, asset import, **26 isolated engine regression cases**, startup smoke and Windows cross-export from a fresh source copy. The suite is growing as backlog work continues; consult the progress ledger and latest validation results for subsequent batches. Five Python validator tests passed. A clean compile still reports 14 pre-existing warnings. Linux/Windows CI is configured but has not run remotely at this handoff.

Before the engine patch, the last fully passing local run on **2026-10-02 covered 153 isolated cases**, startup smoke and Windows cross-export. The MTX installation batch had **190 cases** and **25 Asana-linked fixes with local regression evidence**. Its final 4.2.1 full attempt passed cases 1–149, then case 150 timed out after 180 seconds while Research entered the tree. A continuation passed 151–152 but case 153 timed out at its 45-second focused limit. Without retrying either failure, a second continuation passed 154–190, startup smoke and a fresh Windows cross-export. Five Python validator tests passed. This is **188/190 across three runs, not a passing aggregate**. The preceding 162- and 173-case attempts also stalled at case 150. Windows gameplay and visual acceptance remain pending.

Commit `4cac288` previously contained 123 cases and 18 Asana-linked fixes with local regression evidence. That batch's full run failed at case 65 on invalid GC-handle errors despite passing assertions; its other 122 cases passed across the attempt and continuation. Case 65 passed in subsequent full runs, but the same stack signatures recurred at case 61 in the assembly batch; no proven fix exists for this intermittent failure. Preserve both results when assessing readiness.

### Salvage and bulletin batch — 2026-10-02

The News/bulletin, OF pilot-warning and unknown-object batch expanded the runner to **153 cases** and the ledger to **21 Asana-linked fixes with local regression evidence** at that point. See [batch evidence](validation-results.md#news-of-pilot-warning-and-alien-technology-batch--2026-10-02) for the latest aggregate result and exact limits; none of the 48 tasks is declared fully accepted.

- Cases 124–128 cover News history/replay and bulletin input-lock ownership. Broader News report coverage remains unspecified.
- Cases 129–134 cover OF pilot warnings, state preservation and cleanup. Compare the native layout and pointer behavior with the Asana reference.
- Cases 135–149 and 151 cover unknown-object capture, research discovery, cargo/artifact conservation, crew/weight limits, gift handling and idempotent unlocks.
- Case 150 follows the comms gift through normal research, production, fitting and trade. It passed targeted validation but has also intermittently stalled while Research enters the tree; the later 4.2.2 patch addresses its captured script-registration deadlock. Removing Research audio did not eliminate the old-engine stall. Case 152 verifies removal of a detached Research placeholder button; this is a separate confirmed leak fix.
- Case 153 follows the actual war-warning Fusion Laser gift through analysis and normal research to drone technology. Prior trades, travel, staff and equipped ships are staged fixtures; narrative text is shortened. Repeat the full gameplay path natively.

Earlier typing-audio leaks were reproduced in a standalone engine probe. The test helper now waits for stopped playback to drain on the mixer before immediate process exit, with a bounded wait and strict error checks. This does not certify native audio/navigation shutdown. Rebuild before investigating any case, never use a stale assembly after compilation fails, and preserve failed logs even when a diagnostic rerun passes.

The initial batch addressed twelve gameplay/UI defects, including AMA equipment duplication, reversed initial IOS ACC endpoints and consecutive grapple unloading. The latter three correspond to open Asana reports. Use the linked results for the full list; do not report all 48 as fixed.

Raw Mac logs and binaries are under ignored `artifacts/` and will not arrive with a Git checkout. Regenerate evidence on Windows.

## Windows setup and automated validation

Use **Godot 4.2.2 .NET**, **.NET SDK 6.0.428 x64**, and **Python 3.9+**. Install the SDK from Microsoft's official .NET 6 download page if it is missing. `global.json` pins the SDK; the standard, non-.NET Godot build is unsuitable.

Run in PowerShell from the repository root:

```powershell
$ErrorActionPreference = "Stop"
git status --short
git branch --show-current
git rev-parse HEAD
dotnet --list-sdks
dotnet --version
python --version
python scripts/install_godot.py --templates
if ($LASTEXITCODE -ne 0) { throw "Godot installation failed" }
$env:GODOT = (Resolve-Path ".tools/godot-4.2.2/Godot_v4.2.2-stable_mono_win64/Godot_v4.2.2-stable_mono_win64_console.exe").Path
& $env:GODOT --version
if ($LASTEXITCODE -ne 0) { throw "Godot version check failed" }
python -m unittest discover -s scripts -p "test_*.py"
if ($LASTEXITCODE -ne 0) { throw "Validator tests failed" }

# Preserve previous evidence and exports before starting this attempt.
$evidenceDir = Join-Path "artifacts/windows-validation" ([guid]::NewGuid().ToString())
New-Item -ItemType Directory -Path $evidenceDir -Force | Out-Null
if (Test-Path "artifacts/validation") {
    Move-Item "artifacts/validation" (Join-Path $evidenceDir "previous-validation")
}
if (Test-Path "artifacts/windows") {
    Move-Item "artifacts/windows" (Join-Path $evidenceDir "previous-export")
}
python scripts/validate.py --export-windows
if ($LASTEXITCODE -ne 0) { throw "Game validation failed; preserve and inspect logs" }
```

Check each command's exit status and stop to investigate failures. The installer verifies official Godot release checksums and a pinned RCEdit checksum. On Windows, `--templates` also places RCEdit beside Godot; the validator adds that directory to PATH for export. If .NET is installed outside its standard location, set `DOTNET_ROOT` and add that directory to `PATH` before launching Godot.

The validator discovers cases from the C# runner and starts a fresh Godot process for each. Record the discovered count and compare it with the latest validation results; do not hard-code the initial 26-case count. Require a passing aggregate, clean startup and successful export. Read `artifacts/validation/`, including warnings; a process exit of zero alone is insufficient. Test resources are excluded from the exported game.

Archive each attempt before another run: the validator overwrites logs for stages it reaches, but leaves later-stage logs from older runs. An old executable or smoke log must not be attributed to a failed new run. Run interactive export acceptance only after a successful export of the recorded commit.

## Interactive acceptance pass

Run both the source game and the Windows export:

```powershell
# Clear selectors left by focused diagnostics before interactive acceptance.
Remove-Item Env:DEUTEROS_TEST_CASE -ErrorAction SilentlyContinue
Remove-Item Env:DEUTEROS_SCREENSHOT_DIR -ErrorAction SilentlyContinue
Remove-Item Env:DEUTEROS_IMPORT_ONLY -ErrorAction SilentlyContinue
& $env:GODOT --path Godot
& .\artifacts\windows\Deuteros.exe
```

1. Check title, Earth, training, research, production, stores, ship bay, overview and Escape/settings screens. Exercise actual pointer hit areas, right-click navigation, font rendering and sound.
2. Exercise the existing fixes: depleted/replenished AOC production; ground/orbit output; MTX stock conservation; new/replacement ship ACC ownership; unavailable menu entries; course cancellation; SCG drone transfers and attacked icon; AMA removal; IOS ACC labels/loading; both grapple pods unloading without reopening the bay.
3. Verify bay entry at the cockpit and when returning to saved pod/engine positions. Dismantle equipped ships with exactly sufficient capacity; verify every returned quantity. Repeat with full staff/store capacity, confirm nothing is lost, clear space and retry. Unload held grapple salvage through its normal discovery path before dismantling.
4. Check roster and cryopod colours at native scale. Exercise station/ship overview hover text, travel/location updates and DFCC counts at zero and a populated fleet; check updates after arrival, transfer and removal.
5. Repeat **start → Earth ground → training → Escape/settings → close** at least three times with sound enabled. Include Store → Ship Bay → Training, ground/orbit contexts and MTX toggles; listen for one correctly interrupted loop and check mute/volume. Close through the actual Windows window control with settings both open and closed. Confirm the process exits and inspect the game log, not just the disappearing window. Case 205 covers a programmatic close notification; physical controls still need acceptance. Record hangs, exceptions and retained-resource messages.
6. Development shortcuts can establish a targeted fixture, but do not use them as evidence that normal research/unlock progression works. Record exactly which shortcuts were used.

The startup smoke only exercises the entry scene. It does not cover the above audio/navigation shutdown path. Save/load now has local regression coverage; verify it natively using the steps below and [save-file notes](save-files.md).

## Additional acceptance for the save/navigation batch

- Save a game with active training/research/production, equipped ships, cargo and ACC routes. Advance and change it, then load it and check the original quantities, progress, crew ranks and news. Advance again; old worlds must not keep running. Reopen the exported game and load the same slot.
- Cancel overwrite/load, then confirm each. Check the previous save's `.bak`, corrupt-file feedback, and a write-denied directory without losing the existing slot. Record Windows filesystem results.
- Select a production item on ground and in orbit, then open stores through the menu. Check matching highlight, local recipe capacity and return from equipment/MTX views.
- Right-click actual bay/store controls, then modal windows and timed grapple unloading. Verify dismissal/locks take precedence. Check bay hover labels through the physical mouse, including repeating fuel buttons and roster rows.
- Select HeD fuel in both ACC endpoints, including depleted inventories. Verify cycling finishes, loads available HeD and survives saving/loading.

## DFCC fuel acceptance

For **1215716464570901**, verify IOS/MeH and SCG/HeD with zero and populated drone fleets. Manual fuel loading/unloading must exchange ten stock for one gauge unit; capacity remains 250. Check nine versus ten available stock, exact payment, full/exact return capacity and unchanged 1:1 costs without DFCC. Gauge drain must not multiply again.

Fit DFCC with fuel already aboard: return that tank at the old rate before conversion and update both readouts immediately. Insufficient fuel-store space must preserve the entire fitting and permit retry after space is cleared. Exercise ACC with stock just below the configured threshold, exactly sufficient stock and a remainder below ten; the remainder stays in stores. Dismantle with both tank fuel and fuel cargo, checking their combined return atomically. Test the physical controls and a full save/reload cycle in both source and export.

Already-loaded DFCC tanks in old saves retain their range; subsequent refunds use the new ratio and can exceed their historical purchase cost. Record the save's originating revision and review this compatibility choice separately. The [original evidence](original-dfcc-fuel-evidence.md) supports the manual ratio; original ACC/complete conversion fidelity and clock cadence remain separate research gaps.

## Grapple-only ACC acceptance

For **1215683087492480**, the [original code](original-asteroid-acc-evidence.md) confirms deliberate disengagement after a scan without AMA. Engage ACC on an IOS at the asteroids with a qualified pilot, grapple and supply pod. Before a find it should remain engaged; after a find it must disengage without capturing cargo, docking, departing or clearing filters. Check both selected large asteroids and small/unselected ones. Capture an eligible small asteroid manually afterward, then save/reload. Repeat with AMA fitted: a selected class-six-or-larger unmined asteroid should still start docking for mining. Cases **305–309** cover local behavior; use physical Windows controls in source and export.

Keep Complete Cycle separate: its modern flag combination and full lifecycle still require investigation. Do not treat this single Engage correction as proof of all AMA/ACC fidelity or original scan timing.

## AMA cargo acceptance

For **1215685674676221**, mine a selected mineral while other pods contain different ores, including partial and full quantities. Only a compatible partial or empty pod may receive ore, capped at 250. With no compatible space, mining must launch without a crash or cargo overwrite; engaged ACC must return rather than approach a selected asteroid it cannot store. Repeat after save/load and with an emptied pod. Cases **310–313** support the cargo correction. Original yields/timing remain research work; see [the trace](original-ama-mining-evidence.md).

## Additional acceptance for ACC, settings and interiors

- Select the same mineral on both ACC endpoints. Check equal/unequal stocks, multiple pods, odd totals and several round trips. Fill stores near 50,000 and confirm surplus cargo remains aboard without loss. Repeat on shuttle ground/orbit and IOS routes. Activation already passes baseline tests; reproduce the original activation report before closing it.
- Keep the interior open through takeoff, landing, transit and arrival. Check menu destinations, ship-name header and locks. Open an SCG with five occupied pods; all cargo rows must render. Moon/unsupported-colour icons currently use existing neutral artwork.
- Rename each hull through the name label. Check blank/long names, Enter, Cancel, Escape, clipping/full tooltip and save/load. Verify actual pointer hit areas and dialog layout.
- Check time animation while Earth, Overview, News, Save and Store are selected. Exercise toggle, hold/release, pointer exit and automatic stops without resetting the selected-screen indicator.
- Change sound/volume, window scale and fullscreen; reopen settings and restart the executable to check persistence. Restore defaults. Confirm/cancel progression presets and repeat them without duplicate world state; verify the resumed view updates. These shortcuts are not normal progression evidence. Original-bug compatibility toggles are not specified or implemented.
- Use [media inventory](media-gap-inventory.md) for missing sound/animation sourcing and wiring; original audiovisual fidelity remains unverified.

## Additional acceptance for station displays

- Station overview: compare local orbital production with ground production, check completion without reopening, and follow shuttle takeoff/landing/removal. Check zero and nonzero deployed derricks; rigs in stores must not count. Labels describe the current simulation; only idle wording is confirmed by the original screenshot.
- Production: verify the full AOC plaque replaces staff labels and the removal control during idle and active automation, including the day an AOC finishes. Check manual production remains intact. The plaque uses the exact task reference; its static light does not certify original animation timing.
- Deposit analysis: select planets, return to system view, select moons and change systems. A body's own built/incomplete station should appear below the selection, disappear after loss and remain hidden in course selection. Compare placement with the linked [visual references](visual-reference-notes.md).

Cases 154–162 cover these transitions. Native Mac captures passed and corrected one clipped shuttle label. See [batch results](validation-results.md#station-status-and-missing-graphics-batch--2026-10-02) for screenshot reproduction commands and fixture limits. Repeat in the Windows source game and export.

## Additional acceptance for construction graphics

Manufacture an IOS chassis, interplanetary drive and resource-station frame manually and under AOC. Check all three stage images, completion/idle, and switching to ordinary PNG artwork. There must be no magenta rectangle, clipped art or source-sheet labels. Cases **206/208/210**, run without `--headless`, compare rendered pixels with the supplied sheets and can save captures via `DEUTEROS_SCREENSHOT_DIR`. Repeat in the exported game visually. The [other eleven candidate sheets](media-gap-inventory.md#a3-recovered-construction-frames-and-remaining-source-gaps) contain placeholders or unverified art; they are not ready for blind export. The production rod and original stage timing remain separate evidence gaps.

## Runtime checks and historical failures

- The latest shutdown correction collects unreachable wrappers and waits for their finalizers while the engine is still alive. Keep it after scene/audio release and before `Quit`. Case **276** deterministically queues 64 resources behind a bounded finalizer gate; the old code leaked all 64, while the correction exits cleanly. This is separate from premature injected-input disposal. Repeat 183/184/205/264/276 on Windows and test physical window close in source/export; preserve any recurrence instead of adding sleeps or excluding errors. See [controlled evidence](shutdown-finalizer-evidence.md).

- The menu-artwork **4.2.2** attempt hit a **case-150 shutdown failure** after its completed trade and passing assertions: `FATAL: Condition "!rc_owner" is true` at `_instance_binding_reference_callback` (`csharp_script.cpp:1379`), followed by a 180-second timeout. The native sample shows exception dispatch after engine disposal; the managed diagnostic tool returned no frames. This is distinct from the earlier Research-entry stall. Case 150 now passes focused headless/native and full-suite checks using the production shutdown path. Preserve the old failure, investigate shutdown on Windows, and do not treat passing assertions as a clean exit. Its original root cause and relationship to the earlier audio failures are unproven.

- The case-150 Research-entry deadlock was traced to opposing native/managed script locks and is addressed by the pinned 4.2.2 patch ([upstream fix](https://github.com/godotengine/godot/pull/87669)). On 4.2.1, instrumentation stopped inside the **prefab load**, before the separate script load or `SetScript`. Do not rewrite the generic buttons or remove audio to work around that old-engine failure. Repeat normal comms progression and case 191 on Windows. If a stall recurs on 4.2.2, preserve the engine log and capture a managed stack with [Microsoft's dotnet-stack tool](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-stack): `dotnet-stack report --process-id <pid>`.

- On Mac, the former direct-exit audio/navigation path leaked Ogg resources or hung with `!rc_owner`. An isolated Godot 4.3 comparison also failed. The new scene-release/mixer-drain sequence resolves the observed immediate-exit leak in strict checks; it is not a demonstrated fix for every historical fatal error. Repeat actual window close on Windows.
- Case 153 (war-warning Fusion Laser gift → analysis → research → drone unlock) timed out at 45 seconds in the final 4.2.1 continuation. It passed focused and full-suite 4.2.2 checks. Its failed 4.2.1 log contains bulletin/input-lock activity; no managed trace was captured. Its historical cause is unproven; investigate any recurrence independently. Preserve the failed log and capture a stack before changing lifecycle code.
- Earlier combined runs and case 65 logged `SwapGCHandleForType: Handle is not initialized` (also `SetGodotObjectPtr`). The later case-61 investigation reproduced premature disposal of injected mouse events; the helpers are corrected and stress case 264 protects that lifetime. Cases 61/65/264 now pass focused headless and native Mac checks, but this does not establish the cause of every historical failure. Repeat them on Windows; select a case with `$env:DEUTEROS_TEST_CASE = "65"; & $env:GODOT --headless --path Godot res://Tests/Regression.tscn`, then clear the selector with `Remove-Item Env:DEUTEROS_TEST_CASE`. Preserve failed logs and capture diagnostics before changing resource ownership. Keep the current case-23 native teardown crash and historical case-150 fatal error separate from this reproduced harness defect.
- Godot 4.2.1's binary scene conversion leaked instances during fresh exports. `export/convert_text_resources_to_binary=false` avoids that path; retain it unless a tested replacement removes the need.
- The validator permits one exact documented `_EDITOR_GET` teardown diagnostic only in editor import/export stages. Do not broaden exclusions, suppress game errors or retry until green without investigating.

For the salvage batch, also exercise News replay/cancellation with sound enabled; OF deployment with missing, empty and valid crews; occupied grapples receiving gifts; and the complete comms progression without unlock cheats. Compare the OF warning with the source linked in [visual reference notes](visual-reference-notes.md). If typing-audio leaks recur, preserve them separately from the intermittent Research-entry stall, case 65 and the older navigation shutdown failure; a shared cause has not been established.

## Additional acceptance for menu artwork

- At the damaged Moon base, verify crossed-out Resource and Mining Store controls in B5/B6. Neither should navigate. Orbital production/stores and the surface shuttle bay must remain available. Deliver a repair kit with a piloted shuttle and complete the normal repair; the open menu must restore working controls that day. Check the same damage flag at another planet and an incomplete base, which should remain blank. Keep a bulletin open through an availability update and confirm its title is preserved.
- In an IOS orbiting a Methanoid-owned station, compare the face in menu row four with [the Asana reference](visual-reference-notes.md). It must clear on departure, friendly capture, ground contexts and leaving the interior. It is passive artwork and must not intercept neighboring controls. Verify both peace/war encounters and report any additional original palette or visibility rules; the still screenshot does not establish those variations.
- Cases 192/197 can capture native evidence with `DEUTEROS_SCREENSHOT_DIR`; clear capture/test environment selectors afterwards. The committed atlas references preserve the unmodified supplied PNGs and exclude their annotation rectangles.

## Additional acceptance for MTX installation and routes

Cases **163–173** cover route safety. Confirm send and balance leave both inventories unchanged when the destination is captured, incomplete or lacks an MTX; a retained route should resume after friendly recapture. Test a destination disappearing while the selector is open: its old button must not select a different planet or throw. Configurations containing only locked/unavailable items must terminate without advancing inventory. Check near-capacity receivers retain overflow at the source.

Cases **174–190** cover installation and access. Discover captured hardware through orbital Stores, complete research normally, then manufacture at a second station. Verify manual and AOC production, exact material charges, no transferable module stock, no repeat build, and the installed hover/indicator. Save/reload during and after construction; verify routes and stock. Keep Stores open across completion and station loss. Ground, unequipped and enemy stations must not expose MTX controls. Test the “activate MTX” development preset independently: it now creates its required station, and repeating it must remain a no-op.

Case 187 traverses these controls with staged capture/staff/AOC prerequisites; repeat a normal Windows campaign path without those shortcuts. Native Mac captures show the MTX inventory plus bounded artwork. Missing construction frames currently use the existing static illustration, not recovered original animation. See [batch evidence](validation-results.md#mtx-installation-and-stores-access--2026-10-02).

## Continue the 48-task goal

For each task: read its description/comments/attachments, establish expected behavior, reproduce the gap, add a failing regression, implement a focused fix, and verify the affected screen or progression. Track **unstarted / investigating / implemented / verified / evidence-blocked** separately. A missing feature or ambiguous research question needs acceptance criteria, not a superficial patch.

Check whether this Windows session has authenticated Asana access to project `1214891399253076` in workspace `507237966097081`; Mac credentials/connectors do not automatically transfer. If unavailable, work from the committed 48-task snapshot and record which live comments or attachments need access. Keep task status updates in the local ledger until Asana writes are authorized.

Use Craig's `WizzoUK2/deuteros-parallel` research where available. Read its `DIVERGENCES.md` and latest `docs/m2-findings.md` addenda before adopting behavior. The Mac's local checkout path is not portable; obtain the repository through existing authorized access. Distinguish decoded facts and emulator observations from provisional formulas. Raise concrete missing evidence while continuing independent tasks.

Engine-damage task **1216073204565506** now implements the [traced original escape rule](original-engine-damage-evidence.md). On IOS and SCG, reach an occupied planet during war and separately remain in orbit when a fleet attacks. Verify danger entry alone does not damage the drive; escape without DFCC can damage it, while DFCC prevents that roll. Deterministic tests cover both outcomes; do not require every physical escape to roll damage. Damaged travel must take twice the route duration, with **Damaged !** visible, accurate return ETA and damage surviving save/reload. Replace through the existing bay engine-fitting control: consume one local matching spare, clear damage/engagement and reject repeat fitting. No local spare must leave the engine damaged. Dismantle with full drive storage: damaged hardware yields no usable spare, while healthy hardware still returns normally. Test old saves without the new field and retain pre-upgrade backups. Do not map the original countdown directly to remake days or claim original random-sequence fidelity. Record the task-owner timing interpretation and normal progression separately from staged fixtures.

For **1215691951441128 — Methanoid Resource Trading**, read the [verified original instructions](original-trade-evidence.md) before extending the implemented choice. Acceptance increments the counter; explicit refusal decrements it with a zero floor. The later parallel-port claim that both outcomes increment is contradicted by these bytes. The war gate is **>=16**, and original decision expiry assigns 18; its wall-clock timing and the cargo layout still need mapping. The new choice implements acceptance/refusal, eligible cargo checks and interruption cleanup; cases 220–235 cover those paths and case 150 explicitly accepts after normal comms progression. On Windows, verify the cargo preview, both buttons, keyboard focus, Escape abandonment, quantities, repeat input, scene exit, save/reload and the war boundary. Compare IOS and SCG layouts. Escape abandons without a decision; only the Decline button decrements the counter. The modern modal pauses simulation and currently has no automatic timeout, so original trading fidelity remains incomplete.

For **1215685674676241 — SCGs**, the [hull gate and arrival fix](original-behavior-evidence.md#1215685674676241--scg-and-ios-travel) now have cases 236–248. Verify IOS can select and fly local planet/moon routes but cannot choose another star; rejection must preserve both ACC endpoints and display the SCG requirement. Test saved invalid IOS ACC routes: activation/continuation must leave fuel and cargo unchanged. Repeat shuttle ACC and valid SCG outbound/return travel. Follow normal SCG research and assembly separately from staged fixtures. Earth→Cerberus now resolves instead of remaining indefinitely in transit, but its current one-tick result is **not evidence of original interstellar timing**; the duration formula still needs original evidence. Record existing midflight saves separately, since the new departure gate does not migrate them.

## Deliverable

Create `docs/windows-validation-results.md` with the tested commit/dirty state, Windows/GPU details, tool versions, commands, case counts, source/export results, manual scenarios and exact unresolved failures. Include Asana IDs and evidence for every newly verified task. Store raw logs/screenshots under `artifacts/windows-validation/`; keep binaries and generated caches out of commits.

Use this compact report structure:

```markdown
# Windows validation results

## Environment and revision

Date, branch, full commit SHA, working-tree changes, Windows/GPU, tool versions.

## Automated results

Commands, discovered/passed/failed case counts, build/import/smoke/export results,
warnings, and paths to this attempt's logs. Preserve every failed attempt.

## Gameplay acceptance

| Asana task / scenario | Source | Export | Steps and evidence |
| --- | --- | --- | --- |
| Task ID and behavior | PASS / FAIL / NOT TESTED | PASS / FAIL / NOT TESTED | Reproduction, fixture shortcuts, screenshot/log |

## Remaining work and review readiness

Unresolved failures, missing original-game evidence, next actions, and whether
the tested contribution is ready for team review.
```

The Windows handoff is complete when the recorded revision has a passing automated run and fresh export, every applicable interactive check has a recorded outcome, and failures or untested scenarios have explicit follow-up work. The **48-task goal** is separate: count a task as accepted only when its own criteria and native checks pass. A blocker in one task should not prevent independent backlog work.

Conclude with what is verified, what remains, and whether the contribution is ready for team review. Do not equate an exported executable or a passing headless suite with a completed Windows gameplay test.

Prepare the eventual PR summary around player-visible fixes, linked Asana IDs and Windows evidence. Group changes into reviewable batches and list unresolved items explicitly; documentation and test infrastructure support that contribution. Leave publication to Craig's next instruction.
