# Windows agent brief

## Start here

**Latest revision verified on both hosts: `a54e899d4c6796e6f406733112d39a7ba424e7d0` (2026-10-02).** Mac and native Windows each pass **409/409 regressions**, import, source startup and Windows export; the Windows package also passes startup. At that checkpoint there were **34/48 tasks with implementation evidence and 0/48 fully accepted**; the later Mac checkpoint below raises implementation evidence to **35/48**. See [Windows results](windows-validation-results.md) for hashes and limits. The packaged smoke now also verifies the corrected lowercase `bandaid.png` resource; interactive artwork checks remain pending.

Previous Windows-run candidate **`f5a9cbb994b2f904426c0744856405328f243a16`** adds recovery, manufacture and original hull fitting. Full **421-case validation passed on Mac and in the Windows runner**, including source startup and export. Mac logs/package are audited. On 2026-10-03 SSH collection recovered and verified all 421 Windows case logs and the existing export hash/package contents; Windows packaged startup is still missing. The fully audited Windows checkpoint above therefore remains `a54e899`. Preserve the existing `8cdd458` desktop handoff.

1. **Use the exact handoff revision.** Work in your own checkout from Craig's verified bundle or explicitly supplied remote branch, `codex/build-tests-and-gameplay-fixes`. Inspect existing changes before switching; preserve previous work and the SSH validation checkouts. No push has been made by the Mac agent. An earlier desktop pass already underway at `d11ff3b` or `8cdd458` should retain that revision in its report, then test the newer changes separately.
2. **Read the current instructions.** Preserve root `AGENTS.md`; its initial toolchain/testing notes are superseded by [README](../README.md), [contributing](../CONTRIBUTING.md) and [testing](testing.md). Read [backlog progress](backlog-progress.md), [validation results](validation-results.md) and the [48-task snapshot](asana-triage.md).
3. **Validate, then test the desktop.** Use the setup below and a fresh export of the recorded commit. Prioritize actual mouse/keyboard, audible sound, settings, save/load and repeated physical window closing, then the feature checks. Record source and export separately, with staged fixtures distinguished from normal progression. Automated input is not physical acceptance.
4. **Return a separate report.** Write `docs/windows-desktop-results-<sha>.md` with PASS/FAIL/NOT TESTED, evidence paths, steps and fixture shortcuts. Keep logs/screenshots under ignored `artifacts/windows-validation/`. Return the report summary, commit SHA and Git bundle path through Craig. Coordinate gameplay-file ownership before making concurrent fixes.

Do not push, open/merge a PR or change/comment on Asana without Craig's instruction. Complete Windows acceptance before recommending a team PR. Ignored Mac artifacts do not travel with Git. A brief or push does not start an agent: Craig must launch the Windows session and relay its results until direct coordination is available.

The latest completed Mac-audited runtime checkpoint is **`20e74aaf48de64834b5bb533dc3f21ac4032adfc`**, with **594/594 regressions**, 19 Python checks and an audited cross-export. It includes the Service, orbital staff-hover, cargo fuel-stock refresh and docked-only TakeOff corrections. It also includes the transmitter ending and overview capacity follow-ups at the end of this brief. It includes the earlier transmission, News, battle and later simulation/training follow-ups below. The prepared but unuploaded `625cae9` bundle is older and excludes these later fixes. No Windows execution of the latest candidate is claimed.


## Rogue crew and prison acceptance

Candidate **`941eca947db76c714a5cabe9829869851b324d78`** passes **572/572 Mac regressions** and audited cross-export; it adds cases **525–572**. Use the exact completed checkpoint in [validation results](validation-results.md), validate/export that revision in a separate checkout, and preserve the existing `8cdd458` desktop work. No new Windows execution is claimed here. Report automated/native tests separately from physical staged tests and ordinary campaign progression.

- Trigger takeover through normal time advancement: partial Hyperlight research, fewer than five hostile systems, a mixed human/hostile system, undocked fully fuelled Warlord SCG, no occupied cryopod or pulse blaster. Confirm one BOUNTY takeover and one Mutiny notice, with save/reload before delivery. Ineligible ships must remain yours.
- Follow actual travel to hostile refit and a human station. Check six cargo mounts, fuel, raids and eligible MTX redirection without duplicated stock. Docked commands provoke departure; retained fuel, equipment, cargo, navigation, ACC, grapple, mining and battle controls must not command the rogue or debit a replacement world.
- Occupy the destination bay to provoke sabotage. Check the single roll, self-destruct warning, defusing and escape without granting permanent SDM hardware. Apply attrition while the team is divided, then save/reload and recover it: casualties must stay lost. Destroy the target before/during sabotage: the surviving crew must rejoin and resume routing.
- Recover the pilot through the bay roster, and check a free Pirate can hijack a docked SCG while preserving its displaced pilot. Complete prison research, pay for manufacture, fit the pod, and capture through the roster. Right-click within half a second retains the prisoner; waiting releases only into available space. Check the visible Equipment button and readable instructions.
- Save/reload contained, free and piloting crews; test full rosters, scene exit, ship loss and occupied-pod removal. Require exactly one crew identity with no loss or duplication. Prisoners remain excluded from normal attrition.
- Check competing notices defer Mutiny/prison discovery; the saved prison countdown advances once per fourth eligible low-priority visit and unlocks research once. Record original-runtime timing/gesture comparisons separately.

Retain exact revision, logs, screenshots, steps and save provenance. Staging research/stocks is useful for targeted testing but must be declared and does not establish normal campaign acceptance.

## Interstellar travel and Warlord acceptance

Candidate **`e1754ebb7779e433c9c516dfcb98baeaabc807c9`** passes **513/513 Mac regressions**, strict import, source startup and audited Windows cross-export. It has not executed on Windows. Keep the existing `8cdd458` desktop report separate; use disposable save copies because older builds reject the new flight/rank fields. Run the full source validation and export this exact revision before desktop acceptance.

- Start a new Mercury → Atlantic SCG journey with 250 fuel and completed Hyperlight. It must remain travelling after the first update. Without damage, the star leg takes 12 consumed updates and leaves 141 fuel; the four-update Atlantic approach leaves 137. Manual and natural modes change displayed time per update, not these phase counts.
- Save before Hyperlight, during its transition and during the body approach. Reload must preserve fuel/progress and produce one arrival. Check the private date resets during transition, synchronizes to Proxima and remains correct after leaving/re-entering the ship screen. Test hold/release ETA changes.
- Use an Admiral for the route: Warlord and one rank report occur at Hyperlight arrival. A Captain at 39 ordinary actions becomes Admiral only after the eligibility check. Ordinary travel/experience must not grant Warlord; transfers and cryopods must retain it.
- Record staged ordinary-flight clock mismatch, insufficient phase fuel and empty-fuel star arrival separately from normal progression. The latter strands for six updates. Confirm one loss report and exit from destroyed-ship controls. Case 513 covers reset of earlier fall debt during a rescued fuelled journey.
- Exercise complete-cycle and automatic roundtrip cargo delivery, damaged travel, research completed mid-flight and an open course map when travel starts. No intermediate star arrival may credit a station; active-flight rerouting is disabled.
- Load a pre-flight-field save already travelling: that one legacy journey keeps its old countdown. Loading alone must not create a phase, promote a crew, spend fuel or lose a ship.

Run native cases 494–513 as automated evidence, then report physical acceptance separately with exact revision, steps, screenshots/logs and save provenance. The body-to-body composition is a deliberate remake UI adaptation, requiring team review; original star/body controls are not claimed. Existing local-only flight timing and ordinary body-arrival fall behavior remain on the shared legacy path. Preserve all failure logs and do not overwrite the older handoff.

## Fractional clock and AMA phase follow-up

Candidate **`e369433757efc6654e62d8ee39ab6a767d2b7c86`** adds cases **475–493**. Keep the existing `8cdd458` desktop report separate and use a disposable copy of saves; newer clock/slot fields are not supported by older builds. Run full source validation and make a fresh export of this exact candidate before testing both:

- Start a new game, queue a trainee, and leave time controls untouched for a little over 315.6 seconds. Record timestamps and video: the date should gain `.01` and training should start. Pause must stop accumulation. The interval is nominal PAL-derived, not yet an original-runtime measurement.
- Partway through a normal interval, save/reload and continue. Manual steps add `1.00` and preserve the partial normal interval; a natural step adds `.01`. Check training/travel consume one update, not 100. Compare old-save dates, stocks and remaining training/flight with their original values.
- Inspect main date, News, save slots and ship ETA, including a year boundary. Hold/release and toggle fast-forward while watching ETA; an external stop should also refresh its projection without another simulation tick. Check labels fit and saving/reopening preserves fractions.
- Exercise actual AMA scan/mine/return with empty, compatible, incompatible and full cargo. IOS allocation is per star; SCG allocation is global. Ship removal/reload must not shift another ship's cadence. Mining matches its four-phase clock gate and scanning its eight-phase gate; asteroid approach/departure each consume two updates. Normal-mode actions can take several minutes, so distinguish accelerated fixtures from normal play.
- Use staged saves for a calendar attrition boundary and the enemy scheduler's exact 9.50-day interval (three remaining hostile systems). Record those fixtures and verify reload does not cause early or duplicate work. Run native cases 475–493 as automated evidence, separately from physical acceptance.

Compare original-runtime timing where available. The later 513-case checkpoint adds star/SCG clocks and Hyperlight travel; original-runtime comparison, rogue crews/ending, original SCG mining eligibility and the original sixteen-slot construction limit remain open. Report PASS/FAIL/NOT TESTED with exact revision, source/export distinction, logs and screenshots; do not close Asana tasks from passing automated checks alone.

## Transmission candidate

Mac-only candidate **`642ed00a61817b3a09a2eece2d0df69ad787337f`** on `codex/alien-capture` passes all 435 regressions, strict import, startup and Windows cross-export. It is now included in the contribution branch through the validated battle candidate, but has not been uploaded to the Windows host. Keep existing `8cdd458` desktop work separate. Once Craig supplies this exact revision, run full validation and record the new export hash before the following desktop checks:

- Trigger war through trade and sixth-station deployment. Verify the introductory notice, keyboard/pointer acknowledgement, typing sound, input release and News replay in source and export.
- Leave a notice before acknowledgement and save/load. It must return without skipping a stage; replay must not change the countdown. The original glyph decoding mask does rotate again on replay.
- Capture a system's last hostile station, unlock the three SCG projects, and follow the reported location through scan, grapple and unloading. Queue multiple captures; notices must keep capture order. Repeat capture after collecting a segment without receiving a duplicate.
- Unload eight segments with saves between them. Completion must reach 100 and final instructions after two eligible updates. A ninth legacy segment cannot restart the sequence. Check readable location insertion and all message layouts, including long lines and typing cancellation.

Cases 422–435 provide staged regression coverage. Distinguish those fixtures from normal campaign play. These changes do not implement Warlord/Hyperlight clocks, transmitter activation or the ending; final instructions alone are not ending acceptance. Read [integration details](alien-message-integration.md) for save compatibility and evidence.

## Isolated News follow-up

Candidate **`9b245f52d977e93074ee2ed459d0d39ab68fda65`** on `codex/news-events` adds the missing report producers and short-history layout after the transmission candidate. Full 437-case Mac validation and audited Windows export pass; it has not been uploaded or executed on Windows. Once supplied separately, test fuel/hostile-orbit losses, fleet attack/capture and both failed/successful dismantling in source/export. Reports must occur once at committed transitions, survive saves and show newest first with fewer than twelve entries. Hover long names to read the complete report; unused rows must have no stale text. Cases 297/301/436/437 cover these operations automatically. Battle-window cleanup is a separate follow-up; do not assume 9b245f5 includes it.

## Isolated battle follow-up

Candidate **`625cae94ef40f4552399781b8ba205e1593b5ec0`** on `codex/battle-cleanup` includes the transmission and News candidates above. Cases 438–442 pass focused headless and native Mac checks; its full 442-case Mac run and audited Windows cross-export also pass. It has not been uploaded or executed on Windows. Preserve the `8cdd458` desktop pass and use a separate checkout when this candidate is supplied.

- Fight both station defenders and a roaming fleet through the DFCC control. Verify victory, defeat and retreat, with no detached battle window or retained-resource errors after closing the game.
- Leave an unfinished encounter and return. Surviving drones must remain on the player ship and the original defending station; reopening must not duplicate them. A roaming fleet's survivors must not be credited to a station.
- Leave at the completed-defeat boundary. The ship must remain destroyed, its News report must appear once, and the previous encounter must not redirect the new screen. Check that a surviving ship remains usable after enemy retreat.
- Load another save during an encounter where the UI permits it. The old battle must release its window without changing the replacement world. Record unavailable scenarios as NOT TESTED; the automated cases use staged combat outcomes, not normal campaign play.

Record source and export separately, including exact revision and logs. These fixes address lifecycle and result handling; original combat arithmetic and campaign acceptance remain separate checks.

## Later asteroid/AMA follow-up

The contribution branch now also restores all eight original asteroid classes/minerals and mining amounts 12–43 (case 443). This is **not** included in the prepared `625cae9` bundle. The range correction passes full 443-case Mac validation at `e68c6bb`. A subsequent case-444 correction blocks manual mining without fuel and disables ineligible Mine controls; focused native/headless Mac checks pass. Once separately supplied, verify Copper/Silica scans, class-7/8 display and mining, compatible cargo, pod capacity and ACC return in source/export. Try manual Mine with zero fuel, then refuel and retry an eligible scan; rejection must preserve the mining state. The fractional-clock follow-up above replaces provisional scan/mining cadence; wider eligibility and original-runtime comparison remain unresolved. Do not count the range correction alone as full AMA acceptance.

## Feature acceptance details

### Artifact recovery follow-up

At `d3f00ac` or later, recover and unload artifacts through real grapple controls. Completion should read 12, 24, 36, 48, 60, 72, 84, then 100 without scientists; duplicate unloads and a legacy ninth segment must not add credit or completion notifications. Save/reload between deliveries, and check old saves retain held cargo and locations while old 11-point credits convert once. Cases 135/410/411 pass focused headless/native Mac checks.

At `ef21e5e` or later, the completed `Unknown` device can be manufactured in orbit with no material charge. Check manual and automated production, save/reload while building, the Stores recipe label and rejection of ground orders. Cases 412–416 pass focused headless/native Mac checks.

At `f5a9cbb` or later, check the five shuttle and eleven IOS/SCG tool choices in fixed item order, including the last row. Fit the device only to an SCG, one per slot; removal must return it once. Blaser/A.M.A. are IOS-only and Prison Pod SCG-only. Old incompatible equipment must survive opening a bay and be returned when replaced; an unavailable replacement must leave it aboard. Cases 417–421 pass focused headless/native Mac checks. Transmissions, device activation and the ending remain unfinished; these corrections alone do not establish campaign acceptance.

### Module dialogue colour follow-up

At `a54e899` or later, check Methanoid introductions, trade replies and deployment messages in source and export: coloured dialogue must remain legible, with light-blue alien text, normal white text and no visible colour tags. Case **409** passes focused headless/native Mac checks; full 409-case validation passes on both hosts. This correction does not implement the missing [transmission sequence](original-alien-message-evidence.md), and is separate from the stable `8cdd458` desktop handoff.

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

### Research completion and packaged image follow-up

On revision `d0d15f7` or later, complete Hyperlight while viewing Research, then leave/reopen the page and save/reload with it selected. It must show the name, tech level and “Research complete” without a manufacturing recipe or mass. Select a physical product and check its real recipe returns; switch back and ensure those fields clear. Inspect Bandaid and the five recovered small illustrations in both Research and Production, especially in the exported game. Cases **404–405** cover the source paths; the package smoke independently loads Bandaid. The older desktop handoff does not contain these corrections.

### Recovered construction artwork follow-up

On a revision containing cases **386–403**, manufacture Pulse Blast Laser, SCG chassis, Star Drive, Prejudice launcher, Star Drone, Prison Pod and Sonic Blaster with manual staff and AOC. Verify all three genuine stage images, correct material charges, completion/idle and cancellation. SDM/MTX intentionally show blank original construction stages while installing locally; they must not create stock or repeat an installation. Five formerly missing small illustrations now load for Pulse Blast Laser, MFL, Prejudice, Prison Pod and Sonic Blaster: inspect Production and Research pages, including transparent page background versus opaque black details. Original empty MFL stages retain a static fallback; no three-frame sequence is claimed. Run native cases 386–403, repeat visual/control checks in the export, and distinguish staged prerequisites from normal campaign discovery. See [recovery evidence](original-construction-artwork-evidence.md). The oversized diagrams are replaced at `7c7db5e`; calibrated original colour/timing comparison remains a follow-up.

### Original item illustration follow-up

At `7c7db5e`, all 32 items use original bitmap shapes. Inspect Research at x208/y68: its image background must be opaque black, without stretched pixels. Production uses the separate masked illustration at x136/y54 over the page artwork. Check all seven formerly oversized diagrams and the complete SCG bottom row; changing products must clear the prior image. The alien artifact intentionally has a blank 32×3 source image. Case **406** checks source hashes and screen rules; the external package smoke loads both resource families. Repeat visible checks in the Windows source and export, including the existing smaller icons and MTX. The `8cdd458` desktop handoff predates this change.

### SCG item names follow-up

At `b3936c2`, check SCG Chassis and SCG Drive names in Production, the Station factory summary, Stores and MTX transfers. Repeat after loading a legacy save with missing names: labels should be restored without changing stock, research or existing custom names. Cases **407–408** cover these paths. This is a separate follow-up to the stable `8cdd458` desktop handoff.

### Production rod follow-up

On a revision containing cases **382–385**, watch the small rod at x240/y96 during manual/AOC production on ground and orbital factories. It should animate while that selected factory has an active product, stop when idle or paused, and retain its phase through redraws and screen navigation. Staff/resource shortages alone must not stop an already active product's rod; a waiting queue with no active product stays still. Normal/fast-forward simulation must not change its display cadence or construction-stage pictures. Compare all three original frames and nominal 12.5 fps with the [source evidence](original-production-rod-evidence.md), including its shorter initial frame; original wall-clock parity still needs a recorded comparison. Run the cases natively and check visible motion in the Windows export. Keep the existing `8cdd458` desktop report separate.

### SDM alarm follow-up

On a revision containing cases **376–379**, arm a disposable station and listen on stereo output: left starts first, right approximately 0.32 seconds later, then both loop. Ordinary ambience and clicks must be silent while the local alarm owns sound priority. Navigate to local Stores/bay, global pages and Earth ground; return to orbit, defuse and confirm ordinary sounds return. Check mute/volume, pause/resume, armed save/load, expiry and closing the game while sounding. Run cases 376–379 without `--headless` and in the exported game where applicable. Record the revision and audio device; compare with original emulator playback if available. Do not replace the existing `8cdd458` desktop report with results from a newer revision.

### Staff attrition follow-up

At `55316b6`, staff attrition joins the normal simulation. Test the 99→100 and later 100-day boundaries with active researchers, ground/orbital builders, waiting teams and ship pilots. A positive saved countdown decreases once; 1→0 does not lose a member. Zero permits a 0/1 loss and rearms to 0–15; loss is random, so do not require every boundary to kill someone. A depleted team stays at zero until replenished. Verify displayed counts, research/production suspension and resumption through training.

Load/swap/unload cryopod teams, save/reload before a boundary, and confirm frozen countdowns survive while other teams age. Existing saves without `AttritionCountdown` start at zero without historical losses; preserve backups because older builds reject the new field. Cases 350–359 provide automated coverage. Record staged setup separately from normal campaign progression and keep results for older desktop revisions separate. Original special stage 7, RNG sequence and fractional/star clocks remain documented limits.

### Supply-pod discard acceptance

The latest gameplay follow-up implements task **1215685674676219** through **Cargo...** in the ship interior. Open it, then use **Ditch** beside the chosen supply pod. The [original control and mutation](original-supply-pod-evidence.md) establish discard with no store credit; docked access through this modern dialog follows the task request and is not proof of original screen availability.

Repeat with shuttle, IOS and all six SCG slots; include docking, transit, active ACC and AMA mining. Verify only the selected cargo disappears, its supply pod stays fitted, both ground/orbital stores and neighboring pods are unchanged, and tools/cryo cannot be ditched. Check empty pods, Close/Escape, save/reload, input locks and leaving the screen. Keep normal mining progression distinct from staged fixtures. Cases **265–275** and native Mac captures cover the local implementation; record Windows source/export outcomes separately.

### Ship assembly acceptance

The current follow-up adds passing cases **249–263** for stock conservation and SCG assembly; native Mac cases 250/252/259/260/263 also passed. The [latest validation result](validation-results.md#ship-assembly-and-inventory-conservation--2026-10-02) records the separate case-61 failure and the 262/263 continuation result. On Windows, verify bay entry leaves research locked, the SCG selector follows its own chassis technology, each hull/drive fitting consumes one local part, and repeat creation cannot put two ships in one berth. The later six-mount correction below supersedes this checkpoint’s five-mount assumption; fit and service all six SCG pods. Dismantle powered and unpowered hulls with full and exactly sufficient part storage; repeat a build/fit/dismantle cycle and save/reload between steps.

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


## Fuel refining follow-up: f913679

This candidate passes all 450 Mac regressions, source startup and an audited Windows cross-export; it has not run on Windows. Preserve the existing `8cdd458` desktop pass and use a separately supplied checkout/build. Verify Earth MeH/HeD, orbital MeH/HeD and completed friendly undamaged non-Earth ground MeH. Two supplied stations must both refine over two phases. Check input exhaustion/refill, capture, first-frame station construction and save/load without phase resets; cases 445–450 describe exact batches and boundary quantities. ACC waiting for fuel must consume newly refined stock on the next update. Retain save backups: older executables may reject the new saved phase/allocation fields. Full original fractional-clock fidelity remains outstanding.


## Simulation-order follow-up (`08eda4d`, Mac 452-case checkpoint)

Use a separate checkout from the stable `8cdd458` desktop handoff. The full Mac suite and Windows cross-export are audited; this revision has not run on Windows. Cases 451–452 cover training before all mining, research and crew attrition before arrivals, and ordered promotion News. Exercise simultaneous graduation, factory/research completion and ACC arrival/departure in source and exported builds; preserve before/after saves and logs. Full original fractional/star clocks and discovery priority remain separate open work.


## Ground-mining follow-up (`50c7daf`, Mac 458-case checkpoint)

The six new mining cases and full Mac validation pass; native Windows execution remains outstanding. Use a separate checkout from the stable `8cdd458` desktop handoff. Verify near-empty deposits and near-full stores on Earth and another colony, with/without MTX; unsupported batches must produce nothing and output must cap at 50,000. With zero derricks, surveys continue. Check exact depletion displays `0`, a started zero-delay survey displays `SURVEY`, and both survive save/load. Preserve legacy save backups; negative old veins are normalized without rewriting inventories. Cases 453–458 cover precise transitions, eligibility and malformed-state handling. Original fractional-clock fidelity is still separate work.

## Hyperlight discovery follow-up (`fbffac5`, Mac 464-case checkpoint)

Use a separate checkout from the stable `8cdd458` handoff. Mac regressions, native checks and Windows cross-export pass; this candidate has not executed on Windows. With seven hostile systems remaining, check enemy-scheduler sampling followed by the eight-pass discovery delay. Save/reload midway, interrupt a competing alien notice, acknowledge it and verify Hyperlight appears once. Select Hyperlight through Research and complete it with qualified staff; discovery alone must not complete research. Cases 459–464 cover exact boundaries, recapture and legacy/malformed saves. Full Hyperlight travel and Warlord promotion remain separate work. Record source and packaged desktop results separately.

## Enemy production follow-up (`64412e0`, Mac 466-case checkpoint)

The full Mac run and cross-export are audited; Windows execution remains pending. In a separate checkout, verify production cadence responds to remaining hostile systems and resumes after recapture from zero systems. Save/reload before the next batch and verify no early or duplicate production; a crossed deadline should produce one batch and schedule forward. Cases 465/466 cover all counts, legacy unused counters and peace. The current whole-day clock still truncates fractional intervals. Repeat actual ship rename/cancel, empty cargo and physical close with Rename open; Mac checks passed, but source and Windows package results need separate records. Preserve the existing `8cdd458` handoff.

## Discovery countdown follow-up

The historical Mac checkpoint for this follow-up is **`29eb9a7cc2be17b379200a575fb20c054caf5b89`**, with 467/467 regressions, strict import, startup and Windows cross-export. It includes the enemy-production correction and preserves an active Hyperlight discovery countdown before detecting recapture, matching original branch order. Once supplied, test saves during the delay, recapture lasting past expiry, and temporary recapture reversed before expiry. A decrement reaching zero must not also dispatch discovery. Case 467 covers these boundaries automatically; this is not full Hyperlight travel acceptance. This revision has not run on Windows. Preserve the separate `8cdd458` desktop handoff.

## Training feedback follow-up

At **`34ab06731fe7740672d3df564fb03e9fb4e407f7`**, test the training light switch twice, confirm arrow controls remain bright when the room dims, and leave/re-enter while dimmed. Queue trainees in all three disciplines and advance through closing and opening: each simultaneous batch should play one door cue, and later button presses should retain their button sound. Repeat screen exit and physical game closing during sound. Cases 468/469 pass headless/native Mac checks, including brightness pixels; full 469-case validation and cross-export pass. Listening, Windows execution and staggered/interrupted animation ownership remain separate checks. Preserve the existing `8cdd458` desktop handoff.

The subsequent **`f6dd5c0911cb71830ef958afee01b88e42fb5f0e`** checkpoint also corrects training animation/lock ownership and passes all 470 Mac regressions plus audited cross-export. Test reopening a room with active training, staggered door motion, navigation where allowed and physical closing after starting training. Static doors must not release other input locks, and one completion must not cut short another door. Native case 470 covers injected completion/interruption boundaries; the physical Mac close is separately recorded. Original animation cadence and Windows execution remain pending.

## Crew-loss News follow-up

At `2668a0f999c33d4ee56da2f6cbdbc8b6ddb18f2b`, verify actual ship losses report the named pilot and cryopod passengers before the vessel. Destroy/capture a station with orbital, ground and factory teams; each removed crew should appear once and the captured factory must retain no human builder. Earth ground crews and distant ships must survive without false death reports. Check saved News history, newest-first layout and long-name hover text. Cases 471/472 and native 436/437 pass on Mac, with full 472-case validation and audited cross-export; Windows source/export execution remains pending. Preserve the separate `8cdd458` desktop handoff.

## Pending bulletin delivery follow-up

At `34440aac483d537561abb43f01eaa42a4c273897`, cause production and research discoveries in the same update. The first report must remain readable; after leaving and advancing time, the second must appear once. Save/reload while a notice is waiting, then produce another: older notices retain request order. Discoveries must not replace a screen owned by a modal/input lock. Replay the current News report while another notice is queued; replay must leave that notice pending. Cases 473/474 and native Mac checks pass, with full 474-case validation and audited cross-export. Verify Windows source/export separately. Pending notices are new save data, so preserve backups before opening them in older builds. Keep the `8cdd458` desktop handoff unchanged.

## Six SCG mounts and legacy saves

At `771ceea` or later, build an SCG and fit all six pods. Select pod 6, load/unload cargo, remove/refit the empty pod, and move to the engine mounting. Enter the ship: all six cargo labels must display without errors. Open Cargo, click the sixth Ditch button and check it does not overlap Close or change pod 5. Repeat in the exported build.

Load an older five-mount SCG save containing fifth-pod cargo, a frozen team and a loaded grapple. It must gain one empty mount, retain the original five pods and their contents, and remain at six after repeated save/load. No free pod or cargo is added. Native Mac cases 263/267/514 pass with screenshots; full 514 Mac validation and Windows cross-export pass. Native Windows and normal campaign acceptance remain pending; preserve the separate `8cdd458` human handoff.

## Manual bay capacity follow-up

Candidate `70cabb8` adds cases 515–519, now included in the full 523-case Mac aggregate at `171ed86`. In shuttle, IOS and SCG bays, try returning 250 cargo units to a store holding 49,751; both unloading and swapping resources must leave cargo and both stocks unchanged. At 49,750, the return must succeed. Repeat with stacked equipment and a full spare-pod store, including replacement and the sixth SCG mount. A rejected DFCC fitting must not refund fuel or change hull mode. Confirm the warning fits its panel, dismiss it, and retry after making room. Five new Mac headless/native cases and 15 related checks pass; the later 523-case aggregate passes. Windows validation remains pending.

## DFCC removal follow-up

Candidate `171ed86` adds cases 520–523. On IOS and SCG, fit a DFCC, load fuel and drones, then remove it or replace it with equipment. Removing the last controller must return its drones and refund fuel at 10:1, clear DFCC mode and empty the tank. Subsequent ordinary fueling uses 1:1. Repeat fitting/removal and save/reload: no resource duplication. With insufficient controller, fuel or drone store space, the entire action must be rejected; exact capacity succeeds. Another fitted DFCC, including in SCG mount six, preserves the conversion and fleet. Unrelated edits on legacy converted hulls without a fitted controller must remain compatible.

Full 523-case Mac validation and cross-export, four focused native cases and a physical SCG remove/refit/remove/save/reload check pass. Run these on Windows once this revision is supplied; the old `8cdd458` desktop handoff remains separate. This fixes selector consistency, not full original conversion behavior or campaign acceptance.

## Drifting engine readout

Follow-up `01c2942` adds case 524 after the full 523 Mac checkpoint. Disengage a travelling IOS or an SCG on its local destination approach: the status must say Drifting and the engine line Disengaged in red. Re-engaged travel says Engaged; takeoff/landing/docking/launching retain that powered indication, and Damaged still overrides it. The shared readout also covers shuttle state. Focused Mac headless/native and five related checks pass; full 524 and Windows checks remain pending.

## Transmitter ending follow-up

Runtime `8353f6ce14b0a4499b5239ea85b80fb5ecbf1d85` adds original Disk 2 artwork, bitmap labels and reconstructed PCM music. Full 586-case Mac validation, 19 Python checks, package audit and independent review pass; prior 409/421 Windows results do not cover this work. Cases 573–586 cover playback, lifetime, eligibility, pause isolation and eight-recovery activation on Mac.

On Windows source and exported builds, recover all eight segments, manufacture the existing Unknown device, fit an SCG mount, assign a Warlord, launch, then press that mount. Verify the original sequence and six labels, audible stereo music, final fade/black and automatic replay after releasing the left mouse button. Check Escape/right-click cannot expose the campaign, the campaign clock/fuel/items remain frozen, and physical window close exits cleanly. Repeat after normal save/load, on the sixth mount and with DFCC. Lower ranks get the existing Warlord warning; docked clicks retain bay access and travelling/rogue vessels cannot activate it.

Inspect the package for `Ending/sequence.json` and imported lossless music; no original disk or test scenes are required at runtime. Compare cadence/audio with the original if available: Mac PCM/texture checks do not establish subjective audio fidelity. Report the exact source/export revisions and evidence. The following overview correction addresses the separate Mac crash with more than 16 visible stations; repeat that check independently of ending playback.

## Overview capacity follow-up

Runtime `89d39ffb164ac1c6cffe21f2cfff18610e193bbe` passes full 589-case Mac validation and audited cross-export. Master Control now pages stations, IOS and SCG fleets in groups of 16, filters hostile stations before assigning controls, and clamps the page after loss/capture. The pager appears only when needed; existing single-page spacing remains. This preserves large remake saves rather than imposing original-world capacity limits.

On source and exported Windows builds, load a save with more than 16 friendly stations and fleets. Reach every entry with Prev/Next, inspect hover names/drone counts, and open a station/ship on the last page. Remove or lose the last-page entry and confirm the page clamps without stale selection. Check 16 friendly stations plus hostile stations, overlay/input locks and the single-page layout. Repeat the full eight-recovery transmitter save: normal Load → all five overview pages → station beyond slot 16 → back → SCG → mounted ending. Cases 587–589 cover station, IOS and SCG capacity; focused Mac/native and physical full-save navigation pass. Record the exact candidate revision and distinguish these checks from unstaged campaign acceptance.

## Early-game progression and desktop follow-ups

Follow-up runtime `833cb85da2e7bf6d377b0135e2c8f6c88e5f4a24` adds cases 590–591 for shared recruit capacity and Research mass-unit layout. Focused headless/native Mac checks and the full 591-case aggregate, strict import/startup and audited cross-export pass. See the completed checkpoint in [validation results](validation-results.md) before testing a supplied branch.

- With three available recruits in a declared disposable fixture, allocate one to each training team. Further plus clicks must leave the counts and remaining population unchanged. Cancel one allocation, assign it to another team, complete training and verify no free staff can be trained at zero population. Check press-and-hold as well as clicks.
- In Research, inspect completed MeH fuel, shuttle drive, shuttle chassis and SCG chassis. The mass value and `t.` must be distinct for one through four digits; switching to research-only technology must still clear its recipe.
- Repeat the [normal new-game route](native-gameplay-results.md) through training, paid manufacturing, fuel refining, first shuttle flight and save/reload in both source and export. Mac passed the first-flight route at 89d39ff and continued through first orbital construction, docking and completed-station reload at 833cb85; later campaign progression remains open. Do not substitute a staged fixture for this check.

Preserve your own saves, restore their exact inventory afterwards, and report exact revision, steps and source/export results separately. The Mac checkpoint and screenshots are ignored artifacts and require a separate authorized handoff.

## Interior Service control

Runtime `50ea4af6765025928f36324dc79be999884cd853` connects the previously inert upper-left Service artwork to the existing bay. Mac cases 592–593 pass headless and natively, and the exact reported click opens the Earth ground bay in the unmodified normal-play checkpoint. Full 593-case Mac validation, 19 Python checks, source startup and audited Windows cross-export pass; this does not establish native Windows acceptance.

On Windows source and export, click Service in a docked shuttle on Earth, a completed colony, an orbital shuttle, an IOS and an SCG. It must open the correct hull's crew section with the correct ground/orbital stores; fuel, crew and mounted cargo must remain unchanged. Return to the interior and repeat. During launch, landing, travel, repairs, active dialogs or rogue control, Service must not bypass existing restrictions. Unfinished colonies, hostile locations, asteroids and missing orbital stations must not open a bay. A completed damaged colony still permits the bay needed for repairs. Record the revision and source/export results separately.

## Orbital production staff hover

Follow-up `8fec147` corrects the missing assignment explanation over a production team in an orbital bay. The existing case 58 now covers available, automated and already-staffed factories and the actual assignment. It fails before the fix and passes afterwards, including a native Mac check and related bay hover checks. Full 593-case Mac validation, 19 Python checks, startup and package audit also pass. Repeat with a normally transported team: an available manual orbital factory should show “Assign crew to production”; an AOC or occupied factory should not advertise that action. Confirm clicking assigns the same team without changing rank or count.

The `8fec147` bundle and `validate-8fec147.ps1` are available in `C:\Users\builder\Downloads`. Bundle SHA-256: `b8a964b48d00b8d44840eb72cc1ac11adbfc16dcb9b4199cf13e9b18f778aaad`. The script creates a fresh `Deuteros-validation\8fec147` checkout, verifies the existing pinned toolchain, runs the full suite/export, then checks packaged startup and archives evidence. It refuses to start while any Godot/Deuteros process is present or that checkout already exists. Another Godot process was visible during preparation; this run has **not started**. Coordinate ownership first and preserve the other agent's session. The original-disk Python test will be skipped on Windows because the disk was not copied; Mac's 19 checks include it.


## Desktop report reconciliation — 2026-10-03

Read the [retrieved desktop reports](windows-validation-results.md#desktop-agent-reports-retrieved-2026-10-03) before repeating SCG, Stocktaker, Service or unavailable-pod work. Windows source `44e37ba` and its subsequent local patches differ from the current Mac candidate. Preserve the running desktop game and saves. Return the existing `DEUTEROS-COORDINATION.md`, a bundle containing `5a9b1ef` and `878d74d`, and separate Stocktaker/pod-feedback patches through a path readable by SSH user `builder`, such as `C:\Users\builder\Downloads`; do not change access permissions. Current regression numbers conflict with that older desktop checkout, so port test intent rather than replacing numbered cases. The Mac Service fix already has normal ground/orbit pointer evidence.


## Cargo stock and repeated Take Off

For `20e74aa` or later, load/unload MeH and HeD cargo while the selector stays open: the lower bay stock must match the selector immediately, without changing tank fuel. Check shuttle/IOS/SCG as available. One real departure should grant one pilot action; repeated Take Off commands while launching, orbiting or travelling must grant none and must not clear another docked hull's bay. Current cases 515–517 and 594 cover staged boundaries; Mac normal-save physical checks also pass. Record current source and matching export separately; neither has been run on Windows by the Mac agent.
