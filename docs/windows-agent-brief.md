# Windows agent brief

## Start here

**Latest verified revision: `5053982b913af9eb8b07ee4da91a4fe09587180a` (2026-10-02).** Mac and native Windows each pass **403/403 regressions**, import, source startup and Windows export; the Windows package also passes startup. There are **34/48 tasks with implementation evidence and 0/48 fully accepted**. See [Windows results](windows-validation-results.md) for hashes and limits. A separate package probe confirms a pending `bandaid.png` filename-case defect; passing startup does not imply all artwork loads.

1. **Use the exact handoff revision.** Work in your own checkout from Craig's verified bundle or explicitly supplied remote branch, `codex/build-tests-and-gameplay-fixes`. Inspect existing changes before switching; preserve previous work and the SSH validation checkouts. No push has been made by the Mac agent. An earlier desktop pass already underway at `d11ff3b` or `8cdd458` should retain that revision in its report, then test the newer changes separately.
2. **Read the current instructions.** Preserve root `AGENTS.md`; its initial toolchain/testing notes are superseded by [README](../README.md), [contributing](../CONTRIBUTING.md) and [testing](testing.md). Read [backlog progress](backlog-progress.md), [validation results](validation-results.md) and the [48-task snapshot](asana-triage.md).
3. **Validate, then test the desktop.** Use the setup below and a fresh export of the recorded commit. Prioritize actual mouse/keyboard, audible sound, settings, save/load and repeated physical window closing, then the feature checks. Record source and export separately, with staged fixtures distinguished from normal progression. Automated input is not physical acceptance.
4. **Return a separate report.** Write `docs/windows-desktop-results-<sha>.md` with PASS/FAIL/NOT TESTED, evidence paths, steps and fixture shortcuts. Keep logs/screenshots under ignored `artifacts/windows-validation/`. Return the report summary, commit SHA and Git bundle path through Craig. Coordinate gameplay-file ownership before making concurrent fixes.

Do not push, open/merge a PR or change/comment on Asana without Craig's instruction. Complete Windows acceptance before recommending a team PR. Ignored Mac artifacts do not travel with Git. A brief or push does not start an agent: Craig must launch the Windows session and relay its results until direct coordination is available.

## Feature acceptance details

### Complete Cycle and shutdown follow-up

On a revision containing cases **314–329**, verify Cycle from each endpoint and during transit/fuel waits. It should finish one leg, unload, retain overflow, and stop without refuelling or loading return cargo. Engage should cancel the finish request; save/reload should preserve it. Check the Finishing and Refueling labels. Repeat with shuttle and IOS, then close the source and exported game normally.

The historical native Mac case-315 failure is preserved. A controlled debug input-cache failure now has a [correction](shutdown-input-evidence.md), covered by cases 348–349 and three clean native Mac case-315 exits. The full Windows 349-case run includes it. Repeat physical close with sound after real input in source and export; a passing gameplay marker alone is insufficient. Preserve failures and diagnose before retesting.

### SDM installation, recipes and trade follow-up

For SDM, research and manufacture at a friendly orbital station. One paid build must install locally, create no new stock and stop AOC repeat. Check Installed hover, duplicate rejection, saving/reloading mid-build and completion, and interruption by station loss/capture. Ground factories must reject it. Existing SDM stock is preserved. Cases 338–347 cover installation. Controls and destruction were added at `d61ef77`; test them separately from older desktop revisions as described below.

Check Captain remains visible through action 39 and Admiral begins at 40. Completed Hyperlight must not become a manufacturing recipe or crash ground/orbital Stores, including legacy stale selections. Automatic fuel refining must continue. Accepting an eligible peaceful trade fills fuel to 250; refusal, cancellation and stale offers must not. Cases 330–337 and strengthened 220–235 support these corrections.

### SDM controls and station loss follow-up

On a revision containing cases **360–374**, use disposable saves and record staged setup separately from normal play:

- Open **Self-destruct** at an installed friendly station. From the disarmed state, turn switch 2 off, then switch 1 on: the display starts at 12. Turn switch 1 off, then switch 2 on to disarm. Check mouse/keyboard controls, readability and save/reload while armed.
- At war, dock a human IOS/SCG at a hostile station. Ownership stays hostile and docking completion starts count 16. Opening an undiscovered SDM should deliver its research bulletin. Defuse with switch 1 off, then switch 2 on; capture must occur once and preserve the ship and crew. With Hyperlight discovered, trying switch 2 first interlocks switch 1; turn switch 2 back off to recover.
- Let a friendly timer expire and separately advance a simulation day while armed. Verify station loss, removed berthed ships and their cargo/crew, stopped fast-forward, one loss report and safe navigation. Undocked, approaching and travelling ships must survive. Repeat while viewing the SDM screen and another affected station screen.
- Compare Earth with a non-Earth colony: Earth ground services and landed shuttle survive; a colony loses its local shuttle, ground stock, derricks and both local crew lists and needs repair (ground-crew correction at `853986c`, case 375). Check rebuilding and stale enemy attacks cannot restore lost stock or facilities.
- Inspect [implementation limits](self-destruct-implementation.md): alarm reconstruction still needs listening comparison and timing uses a provisional one-second count; confirm the original casualty boundaries in a recorded gameplay comparison. Report these as known gaps, not passing original fidelity. Preserve old saves: older builds reject the new timer fields.

### Orbital planet colours follow-up

On a revision containing cases **380–381**, open a ship's enlarged location view while undocked/docking. Compare Earth/Moon, Mars, Neptune, Jupiter, Venus, Mercury, Crete and Julius with/without a local station. The view must contain the supplied artwork; sky/planet highlights change by the decoded palette while fixed station/cockpit colours remain intact. A moon inherits its parent's palette but only shows its own station. Toggle small/large views and repeat docked, travelling and launching states for Shuttle/IOS/SCG; no orbital colour may remain on those other images. Confirm save/reload retains the chosen view. Run native cases 380/381 and repeat in the exported game; distinguish original-emulator comparison from remake-only checks. See [palette evidence](original-planet-palette-evidence.md).

### Recovered construction artwork follow-up

On a revision containing cases **386–403**, manufacture Pulse Blast Laser, SCG chassis, Star Drive, Prejudice launcher, Star Drone, Prison Pod and Sonic Blaster with manual staff and AOC. Verify all three genuine stage images, correct material charges, completion/idle and cancellation. SDM/MTX intentionally show blank original construction stages while installing locally; they must not create stock or repeat an installation. Five formerly missing small illustrations now load for Pulse Blast Laser, MFL, Prejudice, Prison Pod and Sonic Blaster: inspect Production and Research pages, including transparent page background versus opaque black details. Original empty MFL stages retain a static fallback; no three-frame sequence is claimed. Run native cases 386–403, repeat visual/control checks in the export, and distinguish staged prerequisites from normal campaign discovery. See [recovery evidence](original-construction-artwork-evidence.md). Existing oversized research diagrams and calibrated original colour/timing comparison remain follow-ups.

### Production rod follow-up

On a revision containing cases **382–385**, watch the small rod at x240/y96 during manual/AOC production on ground and orbital factories. It should animate while that selected factory has an active product, stop when idle or paused, and retain its phase through redraws and screen navigation. Staff/resource shortages alone must not stop an already active product's rod; a waiting queue with no active product stays still. Normal/fast-forward simulation must not change its display cadence or construction-stage pictures. Compare all three original frames and nominal 12.5 fps with the [source evidence](original-production-rod-evidence.md), including its shorter initial frame; original wall-clock parity still needs a recorded comparison. Run the cases natively and check visible motion in the Windows export. Keep the existing `8cdd458` desktop report separate.

### SDM alarm follow-up

On a revision containing cases **376–379**, arm a disposable station and listen on stereo output: left starts first, right approximately 0.32 seconds later, then both loop. Ordinary ambience and clicks must be silent while the local alarm owns sound priority. Navigate to local Stores/bay, global pages and Earth ground; return to orbit, defuse and confirm ordinary sounds return. Check mute/volume, pause/resume, armed save/load, expiry and closing the game while sounding. Run cases 376–379 without `--headless` and in the exported game where applicable. Record the revision and audio device; compare with original emulator playback if available. Do not replace the existing `8cdd458` desktop report with results from a newer revision.

### Staff attrition follow-up

At `55316b6`, staff attrition joins the normal simulation. Test the 99→100 and later 100-day boundaries with active researchers, ground/orbital builders, waiting teams and ship pilots. A positive saved countdown decreases once; 1→0 does not lose a member. Zero permits a 0/1 loss and rearms to 0–15; loss is random, so do not require every boundary to kill someone. A depleted team stays at zero until replenished. Verify displayed counts, research/production suspension and resumption through training.

Load/swap/unload cryopod teams, save/reload before a boundary, and confirm frozen countdowns survive while other teams age. Existing saves without `AttritionCountdown` start at zero without historical losses; preserve backups because older builds reject the new field. Cases 350–359 provide automated coverage. Record staged setup separately from normal campaign progression and keep results for older desktop revisions separate. Original special stage 7, RNG sequence and fractional/star clocks remain documented limits.

### Supply-pod discard acceptance

The latest gameplay follow-up implements task **1215685674676219** through **Cargo...** in the ship interior. Open it, then use **Ditch** beside the chosen supply pod. The [original control and mutation](original-supply-pod-evidence.md) establish discard with no store credit; docked access through this modern dialog follows the task request and is not proof of original screen availability.

Repeat with shuttle, IOS and all five SCG slots; include docking, transit, active ACC and AMA mining. Verify only the selected cargo disappears, its supply pod stays fitted, both ground/orbital stores and neighboring pods are unchanged, and tools/cryo cannot be ditched. Check empty pods, Close/Escape, save/reload, input locks and leaving the screen. Keep normal mining progression distinct from staged fixtures. Cases **265–275** and native Mac captures cover the local implementation; record Windows source/export outcomes separately.

### Ship assembly acceptance

The current follow-up adds passing cases **249–263** for stock conservation and SCG assembly; native Mac cases 250/252/259/260/263 also passed. The [latest validation result](validation-results.md#ship-assembly-and-inventory-conservation--2026-10-02) records the separate case-61 failure and the 262/263 continuation result. On Windows, verify bay entry leaves research locked, the SCG selector follows its own chassis technology, each hull/drive fitting consumes one local part, and repeat creation cannot put two ships in one berth. Fit all five SCG pods and confirm there is no sixth mount. Dismantle powered and unpowered hulls with full and exactly sufficient part storage; repeat a build/fit/dismantle cycle and save/reload between steps.

Treat inventories from pre-fix saves separately: historical part deductions cannot be reconstructed. Normal SCG discovery and manufacture remain acceptance gaps; the parallel port's Sol-cleared unlock trigger is documented as a reconstruction, not verified original behavior.

### Menu sound acceptance

Menu-sound tests 212–219 now accompany the implementation: one persistent player uses the supplied cue for top/side menus, time toggle and hold. All eight focused cases pass; native Mac cases 212/216–219 also pass. The full 219-case run, startup smoke and fresh Windows cross-export also pass; use the actual pushed SHA for Windows validation.

On Windows, listen while activating those controls with the physical mouse, keyboard and a controller where available. Verify scene changes do not truncate the cue; disabled/empty/hidden controls, cancelled presses and right-clicks stay silent. Holding time should sound once on press, with no tick/release cues. Check rapid navigation, mute/volume and window close. Automated `ui_accept` events cover the action route, not physical controller hardware or listening quality. Original-game cue correspondence remains unverified.

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

Keep the Complete Cycle acceptance above separate. This Engage correction does not prove all AMA/ACC fidelity or original scan timing.

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

- The earlier finalizer correction collects unreachable wrappers and waits for their finalizers while the engine is still alive. Keep it after scene/audio release and before `Quit`. Case **276** deterministically queues 64 resources behind a bounded finalizer gate; the old code leaked all 64, while the correction exits cleanly. This is separate from premature injected-input disposal. Repeat 183/184/205/264/276 on Windows and test physical window close in source/export; preserve any recurrence instead of adding sleeps or excluding errors. See [controlled evidence](shutdown-finalizer-evidence.md).

- The menu-artwork **4.2.2** attempt hit a **case-150 shutdown failure** after its completed trade and passing assertions: `FATAL: Condition "!rc_owner" is true` at `_instance_binding_reference_callback` (`csharp_script.cpp:1379`), followed by a 180-second timeout. The native sample shows exception dispatch after engine disposal; the managed diagnostic tool returned no frames. This is distinct from the earlier Research-entry stall. Case 150 now passes focused headless/native and full-suite checks using the production shutdown path. Preserve the old failure, investigate shutdown on Windows, and do not treat passing assertions as a clean exit. Its original root cause and relationship to the earlier audio failures are unproven.

- The case-150 Research-entry deadlock was traced to opposing native/managed script locks and is addressed by the pinned 4.2.2 patch ([upstream fix](https://github.com/godotengine/godot/pull/87669)). On 4.2.1, instrumentation stopped inside the **prefab load**, before the separate script load or `SetScript`. Do not rewrite the generic buttons or remove audio to work around that old-engine failure. Repeat normal comms progression and case 191 on Windows. If a stall recurs on 4.2.2, preserve the engine log and capture a managed stack with [Microsoft's dotnet-stack tool](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-stack): `dotnet-stack report --process-id <pid>`.

- On Mac, the former direct-exit audio/navigation path leaked Ogg resources or hung with `!rc_owner`. An isolated Godot 4.3 comparison also failed. The new scene-release/mixer-drain sequence resolves the observed immediate-exit leak in strict checks; it is not a demonstrated fix for every historical fatal error. Repeat actual window close on Windows.
- Case 153 (war-warning Fusion Laser gift → analysis → research → drone unlock) timed out at 45 seconds in the final 4.2.1 continuation. It passed focused and full-suite 4.2.2 checks. Its failed 4.2.1 log contains bulletin/input-lock activity; no managed trace was captured. Its historical cause is unproven; investigate any recurrence independently. Preserve the failed log and capture a stack before changing lifecycle code.
- Earlier combined runs and case 65 logged `SwapGCHandleForType: Handle is not initialized` (also `SetGodotObjectPtr`). The later case-61 investigation reproduced premature disposal of injected mouse events; the helpers are corrected and stress case 264 protects that lifetime. Cases 61/65/264 now pass focused headless and native Mac checks, but this does not establish the cause of every historical failure. Repeat them on Windows; select a case with `$env:DEUTEROS_TEST_CASE = "65"; & $env:GODOT --headless --path Godot res://Tests/Regression.tscn`, then clear the selector with `Remove-Item Env:DEUTEROS_TEST_CASE`. Preserve failed logs and capture diagnostics before changing resource ownership. Keep the historical case-23 native teardown crash and case-150 fatal error separate from this reproduced harness defect.
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

Create `docs/windows-desktop-results-<sha>.md` with the tested commit/dirty state, Windows/GPU details, tool versions, commands, case counts, source/export results, manual scenarios and exact unresolved failures. Include Asana IDs and evidence for every newly verified task. Store raw logs/screenshots under `artifacts/windows-validation/`; keep binaries and generated caches out of commits.

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
