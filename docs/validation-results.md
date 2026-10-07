# Contribution and validation results

## Overview refresh after real-time station loss — 2026-10-07

Normal Jupiter SDM failures exposed two `InvalidOperationException: Sequence contains no matching element` errors in `Overview.IOS_Pressed`. The real-time destruction callback skipped global screens, leaving the removed ship's button visible until another simulation update. The station-loss notification now immediately calls the overview's existing refresh method, before the selected-planet check. This refreshes both ship types, station entries and paging without changing SDM timing, combat or save data.

Extended existing fleet cases **589 and 590** fail before the correction and pass afterward. They destroy a docked IOS/SCG through the real SDM timer while the global overview is open and another planet is selected; the refreshed first button then opens the surviving ship. Fresh discovery confirms **635 total cases**. The initial test selection used older case numbers; those logs remain, and the correct current docking/discovery cases **361 and 371** pass. **47 related fresh-process cases**, build (zero warnings/errors), strict import and startup pass. This is focused validation, not a new full-suite or Windows result.

A separate native replay loads the unchanged day-21190 three-capture save, arms Titania's mechanism using its two switches, and returns to Master Control. Real-time expiry removes Titania and WAYFARER immediately, without any simulation-day or date advancement. Clicking the refreshed fleet button opens PROSPECTOR. The strict native log passes, the owned process exits **0**, and original saves are restored byte-for-byte with no settings file introduced. This deliberately destructive verification is separate from the preserved campaign continuation.

Evidence: `artifacts/three-factory-jupiter-20261007/` (original failing log, reproduced failures, focused/build/import/startup logs) and `artifacts/overview-sdm-loss-20261007/` (separate native replay, saves, audit and exit record). The successful campaign's second native log is clean; its first failed-run log remains failing evidence. [Three-capture campaign and resume point](native-gameplay-results.md#three-station-capture-and-overview-loss-follow-up--2026-10-07). Counts remain **35/48 implementation evidence and four locally accepted requirements**.

## News menu pointer obstruction — 2026-10-07

The normal Moon-defence campaign exposed an unresponsive Master Control button on News. Two invisible 40×40 parent controls (`Images` and `NewsLines`) intercepted the upper menu. Both decorative containers now ignore mouse input; their child labels and replay button retain their own input handling.

Extended case **126** uses real pointer events with both the News scene and persistent menu present. It fails before the fix when Time cannot start; afterwards it verifies start/stop, label hover and Master Control navigation, alongside existing replay availability checks. **26 related fresh-process cases**, compilation, strict import and startup pass. The first editor invocation omitted the pinned SDK from `PATH`; its failed log is retained separately from the corrected import. The last full **635-case aggregate** remains the fleet-refresh run; this two-line scene correction has focused validation, not a new full aggregate or Windows result.

Native normal-save verification loads the unchanged day-21824 checkpoint, navigates News → Master Control, starts/stops Time from News, and opens the retained alien bulletin through its replay icon. Three ordinary updates occur; a separate day-21827 save confirms both time flags are off and News history is unchanged. Reload preserves game data apart from elapsed-time bookkeeping. The owned game exits 0 with a strict log, and the original user save is restored byte-for-byte. A post-close UI observation relaunched the auxiliary project manager without the SDK environment; that manager was closed and the pre-existing Godot 4.2.1 process was untouched.

Evidence and runnable audit: `artifacts/news-menu-input-20261007/`. The campaign continuation remains the retained day-21824 boundary; day-21827 is only this pointer/replay verification branch. Task totals remain 35/48 implementation evidence and four locally accepted requirements.

## Live fleet-transfer inventory — 2026-10-07

Normal campaign production exposed a stale fleet-transfer display: the orbital pool, fleet count and power were redrawn only when opening the panel or completing a transfer. AOC production continued behind the unchanged count. The existing frame callback now reuses the existing refresh method after a ship is bound. Production, combat, transfer limits and save data are unchanged.

Extended case 22 fails before the fix on newly available star drones and passes afterward. It checks live IOS/SCG pool increases, external withdrawals, fleet count/power, existing bidirectional transfer and an unbound frame. A fresh complete run passes **635/635**, build, strict import and startup smoke, with exit 0. The isolated checkout/profile is separate from native campaign saves; its project override is restored, and all tracked `Godot`/`scripts` files match the main checkout. Existing build warnings remain. The first discovery invocation used a nonexistent scene name; its failure and corrected invocation are retained separately from the actual red/green regression.

The native campaign visibly changes the open orbital pool from four to five after six ordinary date updates, without touching transfer controls. Subsequent normal manufacture, transfers and defence preserve exact resource accounting and save/reload. [Campaign results and limits](native-gameplay-results.md#live-fleet-inventory-and-sustained-defence--2026-10-07). Evidence: `artifacts/fleet-transfer-refresh-20261007/` (red/green logs, full run, source hashes, complete log manifest and read-only scope refresh) and `artifacts/worktrees/fleet-refresh-20261007/artifacts/validation/`. No new Windows execution or export is claimed; broader campaign and previous shutdown investigations remain open.

## Upstream settings and viewport integration — 2026-10-07

The upstream integration at `365b0a6`, included locally through `fb0158d`, now passes **635/635 fresh-process regressions in one complete run**, build, strict import and startup smoke, with exit 0. The isolated run used a separate user-data directory; runtime and tests match the main checkout exactly, and the temporary project setting is restored. Earlier ten coordinate-test failures and their corrections remain retained. Nineteen distinct native cases, manual settings/discard/window-close, 19 tooling checks and the Windows export audit provide the separate evidence described in [the integration report](upstream-integration-results.md). The latest published contribution remains `20ce403`; the integration/export is local only. Windows acceptance, inherited inactive settings options and earlier Mac/Windows shutdown failures remain open.

All 1,259 Windows package payload hashes pass, with 64 illustration imports and no test resources. The executable has not been run on Windows. Full results, exact hashes, native screenshots and retained failures are in [the integration report](upstream-integration-results.md). Task counts remain 35/48 implementation evidence and four locally accepted requirements.

## 633-case complete integration checkpoint — 2026-10-07

Source/runtime `20ce403fb5c7fc5512dbdde5d7ef313775ffe05b` passes **633/633 fresh-process Mac regressions in one run**, build, strict import, startup and Windows cross-export. All **19 Python checks** pass with the original ending disk and no skips. The owned validator exits **0**; the full validation plus tooling checks took 1,068 seconds. Case 264 passes strict shutdown checks in this run. No runtime or validator changes were made during it.

The independent audit verifies fresh logs for every case, all **1,232 embedded payload hashes**, 64 illustration imports, ending JSON/music, embedded .NET dependencies and zero test resources. The exported DLL contains the new ACC lamp implementation. EXE: **163,908,912 bytes**, SHA-256 **`d22af129db4e88ef7990f3947d2341c0dc772e149ea372e142173731b6bd9445`**. Runtime DLL SHA-256: `3dad78a1afe41bf682cf1efce0f275c95db9814ff8dc7fa5afa276cc74dd74b3`.

Evidence: `artifacts/integration-633-20261007/results/`; run `python3 artifacts/integration-633-20261007/audit.py`. Original saves remain byte-identical and root `AGENTS.md` is unchanged. Prior validation logs, the complete 632-case export and all earlier failure evidence remain retained. The new source/export handoff is prepared locally; it has not been uploaded or run on Windows.

This checkpoint does **not** resolve the intermittent Mac audio teardown or the separate Windows crash, and does not certify the [later upstream integration](upstream-integration.md). Counts remain **35/48 implementation evidence and four locally accepted requirements**. Normal later-campaign and matching Windows acceptance remain open.

## ACC lamp animation — 2026-10-07

The cockpit lamp now reproduces the original four-step, 0.4-second pulse without recolouring the static highlight. Case 633 reproduces the old static lamp and verifies pixel selection, cadence, rollover and unchanged simulation across all three hulls and ACC modes. Build, **110 focused fresh-process regressions**, native case 633, rendered-pixel audit and strict startup pass. The build retains 14 existing warnings and zero errors. The native pointer/save/reload check and window-close exit 0 also pass; original saves were restored byte-for-byte.

See [implementation and evidence](original-acc-indicator-evidence.md#lamp-animation-implementation--2026-10-07). Logs, four native screenshots, retained temporary saves and a runnable audit are under `artifacts/acc-pulse-20261007/`. The subsequent complete 633-case aggregate/export passes above; matching Windows acceptance and existing shutdown failures remain open. The later caller trace identifies original dimming as an inactivity path, not an established modal requirement. Task counts are unchanged.

## 632-case complete integration checkpoint — 2026-10-07

Source `64df06fe5bd4978489400ec35cfd8624f5e495f6` (unchanged runtime `929b4a5`) now passes **632/632 fresh-process Mac regressions in one run**, build, strict import, source startup and Windows cross-export. All **19 Python checks** pass separately with the original ending disk and no skips. The owned validator session 5251 exits **0**. Case 264 passes both assertions and strict shutdown checks in this run; no runtime, timeout or error-filter change was made.

The independent audit verifies fresh logs for every discovered case, all **1,232 embedded payload hashes**, 64 illustration imports, ending JSON/music, embedded .NET dependencies and zero test resources. The executable is **163,906,784 bytes**, SHA-256 **`34756594b605d8673dd0fd9795592baa60466505de2fa6f55e0b46a270905a13`**, byte-identical to the already delivered `929b4a5` package. Runtime DLL SHA-256: `e63d0a5f82da09bc962ed74cad2c2c2824041126ea8aba776b1a14820a29e337`. Logs, package and results are retained under `artifacts/integration-632-20261007/results/`; reproduce the audit with `python3 artifacts/integration-632-20261007/audit.py`. Original saves remain byte-identical, and the root `AGENTS.md` is unchanged. Only the pre-existing Godot project manager remains running.

This establishes the latest complete aggregate; it **does not resolve the earlier intermittent audio-resource failure** or the separate Windows native crash. The failed aggregate at `artifacts/validation/evidence/full-run-929b4a5-failed/` is retained, along with the complete prior validation directory under `artifacts/integration-632-20261007/previous-validation/`. The exact known editor-only teardown diagnostic remains recorded; gameplay logs have no exemption. No new native Windows execution, normal later-campaign acceptance or task completion follows from this pass. Totals remain **35/48 implementation evidence and four locally accepted requirements**.


## Cleared-station danger correction — 2026-10-04

Runtime `d0ace3d497779faaecdb3ef982694827218123f8` passes **102 targeted fresh-process regressions**, build and strict source startup. Cases 631–632 first reproduced destruction in cleared orbit and an invalid escape-damage roll; both now pass for IOS/SCG with zero defenders or active SDM. Docking, simulation and departure share the same original-backed danger check. Existing defended-station, fleet, rogue, damage, News and SDM coverage passes. Build retains 14 existing warnings, zero errors.

Evidence: `artifacts/validation/evidence/cleared-station-danger/` includes failing logs, passing logs and `audit.json`. Original saves and root `AGENTS.md` are unchanged. The suite now declares 632 cases; this is **focused validation**, not a new full aggregate or Windows execution. The last complete aggregate and delivered export remain `003c087` below. [Source and limits](original-engine-damage-evidence.md#cleared-station-danger--2026-10-04).

## 630-case integration checkpoint — 2026-10-04

Runtime `003c087aa28b267087b5afe2719ddcb764c58fc7` passes **630/630 fresh-process Mac regressions**, **19 Python checks with the original ending disk**, strict import, source startup and Windows cross-export. This includes the integrated Windows v5 fixes and the docking/combat follow-ups. New cases 628–630 also pass native Mac checks; original save inventory is unchanged.

An independent audit checks all fresh case logs, all **1,232 embedded pack payload hashes**, 64 illustration imports, ending payload, embedded .NET dependencies and absence of test resources. EXE: **163,907,216 bytes**, SHA-256 `7519808ea4565adc404dacc78a8b83b9652b53610782814e54a7eeb96b220504`. Evidence: `artifacts/validation/evidence/full-run-003c087/`. The existing exact editor-only teardown diagnostic remains recorded; gameplay logs pass strict checks.

The executable ZIP was transferred to builder Downloads. A separate isolated Windows smoke attempt was rejected by PowerShell's execution policy before the game started; this checkpoint makes no new Windows execution claim. [Windows handoff](windows-validation-results.md#630-case-export-handoff-and-policy-block--2026-10-04). Counts remain **35/48 implementation evidence; four locally accepted requirements**.


Local review on 2026-10-01, against baseline `9817216`, branch `codex/build-tests-and-gameplay-fixes`. No PR, push or Asana status changes have been made.

Earlier complete Mac aggregate (2026-10-04): **621/621** at `e313b6d37af488d42523b3294a7c3a184874fec8`, with **19 Python checks**, strict import, startup and audited Windows cross-export. See [621-case audit](#621-case-aggregate-and-package-audit--2026-10-04). Windows retains its [595 audited source cases](windows-validation-results.md#595-case-source-checkpoint--2026-10-03) and separate packaged startup. The desktop agent reports scoped passes for seven task IDs, plus four related fixes accepted by Craig; see [current reconciliation](windows-validation-results.md#current-reconciliation--2026-10-04). Matching newer export acceptance remains pending.

The latest additions implement rogue takeover, raids, sabotage, recovery and prison containment. Earlier changes prevent manual store overflow and clear DFCC mode when its last fitted controller is removed. They also restore all six original SCG mounts through fitting, migration, interior display and cargo controls, following saved interstellar travel, private/star clocks and Hyperlight-specific Warlord promotion, building on the earlier transmission, simulation and UI fixes. The earlier 416-case attempt failed an outdated recipe-label expectation; that assertion is corrected and the failed evidence retained. The alien campaign remains open; totals are **35/48 with implementation evidence and 4/48 locally accepted** (DayTick splitting, AMA research, event-order research and palette research; [requirements](backlog-progress.md#requirements-accepted--2026-10-03)). Remaining Windows scenarios and full parent-task acceptance are tracked in the reconciliation above.

## Reproduced gameplay fixes

Every case below has a regression against actual game code or scene controls. Failures were observed before the corresponding fixes.

| Problem | Result and regression |
| --- | --- |
| MTX balancing creates resources | Even, odd, empty and near-capacity transfers preserve combined stock. |
| Repeating AOC produces without paying again | Each cycle consumes its recipe; production waits when stock is depleted and resumes after replenishment. |
| Earth production follows the selected screen | Ground and orbital factories deliver to their own stores. |
| Multiple ship losses abort the day update | Destroyed ships are removed without mutating an active enumeration. |
| Automatically fitted ACC references the wrong vessel | New and replacement shuttle, IOS and SCG controllers reference their own ship. |
| Empty menu slots retain old actions | Empty/unavailable entries clear actions and disable; valid entries re-enable. |
| Course selection crashes from system/galaxy view | Cancel preserves navigation; valid outbound/return routes still work. |
| SCG transfers display IOS drone stock | Display and transfers use the star-drone inventory. |
| Attacked SCG icon is missing | Correct asset path loads the existing icon. |
| Equipment selection duplicates installed AMA | Unavailable replacement leaves equipment and stores unchanged; removal returns one unit. Asana [1216065854613348](https://app.asana.com/0/1214891399253076/1216065854613348). |
| Second grapple unload requires reopening the bay | Both pods unload consecutively, credit stock once and release input; regression waits for both real timers. Asana [1215716464570921](https://app.asana.com/0/1214891399253076/1215716464570921). |
| First IOS ACC route reverses origin/destination | First course preserves source; labels and cargo loading agree at both endpoints. Asana [1216065854613323](https://app.asana.com/0/1214891399253076/1216065854613323). |

## Build and review evidence

- Native Apple Silicon toolchain: checksum-verified Godot 4.2.1 .NET, SDK 6.0.428, runtime 6.0.36.
- A fresh source copy without `.godot/` compiled with **0 errors and 14 existing warnings**. Initial import, strict second import, **26 regression cases** and startup smoke passed. The canonical suite now starts a fresh process per case, preventing prior worlds, day-event subscriptions and queued node deletion from contaminating later cases.
- The full local validation command passed all 26 isolated cases, startup smoke and Windows release export. The resulting `artifacts/windows/Deuteros.exe` is a roughly 142 MiB x64 Windows binary; test resources are excluded.
- Five Python tests verify that log-only engine errors, assertion failures and missing completion markers fail validation.
- Independent source review found no critical or important defect in the gameplay changes, regressions or validation scripts.
- Windowed macOS check reached title, Earth, training and Escape/settings screens with working mouse navigation and font rendering. This is limited smoke coverage, not a full playthrough.
- Full [Asana reconciliation](asana-triage.md) covers all **48 open tasks**, source locations, evidence gaps and next acceptance checks.

## Follow-on backlog batch — 2026-10-02

Full local `python3 scripts/validate.py --export-windows` passed **41 isolated regression cases**, startup smoke and Windows release export after this batch. These are local regression results; native Windows and visual acceptance remain outstanding.

| Asana task | Implemented behavior | Regression coverage |
| --- | --- | --- |
| 1214891399253086 | Ship bay opens directly at the remembered position instead of bouncing from a saved offset. | Real scene/tween confirms no unwanted cockpit movement. |
| 1215716464570907 | Dismantling returns pilot, cryo teams, fuel, pod hardware/contents, ACC and drones together. Capacity shortages or held grapple salvage prevent dismantling without losing anything. | Shuttle/IOS/SCG returns, staff capacity, aggregated fuel/store capacity, held salvage; replacement ACC conservation retained. At this checkpoint chassis/engines were not refunded because installation left those stocks unchanged; the later ship-assembly batch below fixes both sides of that transfer. |
| 1215691800680670 | Overview hover text shows station location and ship name/location, including transit destination. | Station/ship hover signals, changing locations and stale hover cleanup. |
| 1215716385204514 | Overview DFCC ships display live drone counts, including zero. | IOS/SCG counts update and hide when DFCC or the ship is removed. |
| 1214891399253092 | Shared palette byte RGB values are normalized correctly, restoring dark-blue production staff. | Real roster background and cryopod text, unchanged intended marine red, all shared palette entries. |

## Save, navigation and simulation batch — 2026-10-02

The full local command passed **75 isolated engine regressions**, startup smoke and Windows release export. Compilation reported 14 existing warnings and zero errors; all five Python validator tests passed. Raw logs are under `artifacts/validation/`; selected RED logs and the native save-screen smoke are retained in its ignored `evidence/backlog-batch-2/` directory.

| Asana task | Implemented behavior | Regression coverage |
| --- | --- | --- |
| 1216065854613319 | Right-click reaches Overview over bay/store controls, after descendant modal handlers. | Actual viewport input over controls; station eligibility; pause/overlay/cursor/UI locks; first click dismisses modal; real grapple timer cannot be interrupted. |
| 1215691800680662 | Ship bay hover labels cover navigation, fuel, crew, ACC, interior access and contextual pod actions. | All hulls/mounts, crew and pod states, stale-text cleanup; actual GUI hit testing in a SubViewport for texture, repeating and roster controls. |
| 1215691800680686 | Production selection persists in the matching ground/orbital inventory and is restored in stores with current recipe capacity. | Actual product/menu controls, both inventories, AOC, equipment/MTX round trips, capacity above 200 and depleted ingredients. |
| 1215685674676215 | Separate simulation/display events preserve the existing fixed simulation order. Planet updates resolve the active world; constructors no longer subscribe stale worlds. | Replacement world leaves old stocks unchanged, training runs once, and display observers see finished mining. This establishes ordering without an additional priority-event framework. |
| 1215685674676235 | Five versioned save slots with load/overwrite confirmation, backups and validation before activation. | Private experience/news, all ship types, ACC/item/research references, training/progress, corruption, backup/write failure, actual screen callbacks, restored-world advancement and a mid-flight ship reaching its destination. See [save files](save-files.md). |

Additional review fixes include the persisted enemy scheduling cursor, freeing closed ACC views and the detached MTX row template, removing invalid direct Tween construction, and including HeD fuel in both ACC cargo cycles. The old HeD cycle hung with empty stock; six regressions now cover both endpoints, termination, loading, conservation and save round trips. The HeD defect is an additional review finding. Task 1215683087492480 specifically concerns grapple-only ACC behavior at asteroids and remains a separate research item.

The recipe fixture removes its Production audio child before entering the scene, isolating recipe behavior from the separately documented audio shutdown failure. Runtime errors remain fatal to validation. One save fixture initially left fast-forward enabled on a synthetic partially equipped world; its teardown failed after assertions. The fixture now stops the clock after checking that serialization did not mutate it, and the full suite was rerun successfully.

Native macOS pointer checks reached the new save screen, saved an empty slot, cancelled a load, then confirmed a load into Master Control. The font and footer layout were corrected after visual inspection. Closing the native window ended the process without logged engine/resource errors, but Godot returned exit code 1; this is not a claim of a clean automated shutdown or Windows acceptance.

## ACC, settings and ship interior batch — 2026-10-02

This batch adds 48 isolated cases (76–123), all passing targeted runs. Build/import passed with 14 baseline warnings and zero errors. Across the full attempt and a separate continuation, 122 of 123 cases passed strict checks; case 65 remains unresolved. Cases 66–123, startup smoke and Windows release export passed the continuation; all five Python validator tests passed. No passing aggregate was written. The complete validator stopped at existing case 65: navigation assertions passed but three `SwapGCHandleForType`/`SetGodotObjectPtr` errors failed the strict engine-log check. A phase-traced diagnostic run then passed without a production change; this is an intermittent unresolved failure, not a fix or a green aggregate. Five more Asana fixes have local regression evidence, bringing the total to 18; none is declared fully accepted on Windows.

| Asana task | Implemented behavior | Evidence and remaining acceptance |
| --- | --- | --- |
| 1216065854613325 | ACC shared selections balance inventories instead of carrying the same stock back and forth; full stores retain excess cargo aboard. | Both hulls/endpoints, odd/equal/empty/near-cap stocks, several pods/minerals, conservation and stable repeated round trips. The half-difference rule implements the task's balance requirement; it is not claimed as a decoded original formula. |
| 1215691951441136 | Interior menus derive current ground/orbit/transit context and follow the ship to its destination. | Real takeoff/land/engine controls, day advancement, arrival/docking and preserved input locks. |
| 1215763655607738 | Click the ship name to rename it, with trim/blank/control-character/24-character validation and confirmation/cancel. | All hulls, both displays, save round trips, Enter/Escape and pause/locks. Existing long names are preserved until a valid replacement is confirmed; displayed names clip with full tooltip. Native visual checks remain pending. |
| 1215691800680640 | Time animation follows clock state independently of selected-screen indicators. | Five contexts, toggle/hold/release/exit, external stop, frame continuity and actual blocked/unblocked viewport input. Supplied eight-frame/5 fps art is unchanged; original cadence unverified. |
| 1215685674676237 | Settings provide persisted sound/volume, window scales/fullscreen and defaults. | Reopening/disk reload, type/range validation and pause restoration. Mac pointer inspection checked layout, tabs, confirmation/cancel and Escape; Windows display/audio application remains pending. |

Task 1216065854613317 (ACC activation while docked/landed) passed six baseline UI regressions before this batch's production changes: both endpoints for shuttle/IOS plus waiting for fuel and resuming. Activation was left unchanged; the original report still needs Windows reproduction before closure.

For 1215685674676239, existing cheats are separated from preferences, and progression presets require confirmation. All six shortcuts are exercised; the five later presets also have repeat/no-duplicate checks. A full crew roster fails before mutation. Presets now refresh the resumed screen, and an MTX bulletin without research staff uses a department sender. Original-bug compatibility toggles remain unspecified and are not implemented. Session cheats do not become save-file preferences.

Interior testing additionally reproduced an SCG five-pod/three-label indexing exception and a missing Moon location image. Five cargo labels and a supplied neutral location fallback now pass strict engine checks. The fallback is not recovered original art; see the [media gap inventory](media-gap-inventory.md), which also supplies local sound/animation sourcing tickets without Asana writes.

Native Mac start → Overview → settings → Cheats → preset confirmation/cancel → Escape → close still hung with `!rc_owner`; the process required termination. Subsequent empty-log test timeouts were traced by a process sample to macOS's window-recovery prompt before Godot startup, dismissed, and rerun. These failures are retained separately from successful gameplay cases. A temporary font measurement in the first interior RED fixture also ended in `!rc_owner` after its expected assertion; removing that diagnostic query allowed clean isolated runs, without establishing a general resource-lifetime fix.

## News, OF pilot warning and alien technology batch — 2026-10-02

Thirty new cases (124–153) extend the runner to 153. The full `python3 scripts/validate.py --export-windows` run passed **153/153 isolated cases**, asset import, startup smoke and Windows cross-export on macOS arm64. Five Python validator tests passed. Compilation reported 14 existing warnings and no errors. The exact documented editor teardown exception remains limited to import/export; gameplay logs were checked strictly. Windows acceptance remains pending.

| Asana task | Change and evidence |
| --- | --- |
| 1215691800680656 — pilot warning | An otherwise eligible OF deployment with no pilot or an empty crew shows the supplied warning and preserves ship/station state. All hulls, dismissal/pause, valid deployment and ineligible locations are tested. Completed module text windows are now freed. |
| 1215716464570897 — unknown objects | Capturing gifts no longer erases an unrelated orbital artifact; occupied grapples, stale artifact scans, unqualified crews and asteroids above 250 tonnes are rejected. Analysis consumes cargo once, maps the Fusion Laser prototype to `m__f__l`, and preserves later input locks when a close callback repeats. |
| 1215691951441138 — commspod unlock | Methanoid gifts use an empty grapple or wait for space. Repeated analysis does not duplicate the persistent unlock. Case 150 follows the real gift, timed unloading, Research and Production controls, recipe consumption, fitting and first trade. |
| 1215685674676231 — News | Existing dated latest-12 history, offscreen save history and promotion reports are tested. Replay is disabled until a bulletin exists. Leaving/replacing a typing bulletin releases only its own lock and cancels pending label access. Additional report types remain unspecified; this is not full feature acceptance. |

Case 153 exercises the actual war-warning gift, timed analysis, Research selection and normal research updates through the drone-programme unlock. It starts at the existing trade threshold and stages the return to a friendly dock; it does not validate the preceding trades, travel or original war timing. Both progression fixtures shorten narrative text, use qualified staff and pre-equipped ships. Case 150 records the fitted grapple's prerequisite research and excludes Production background audio, as earlier recipe fixtures do. It does not grant comms research completion, manufactured stock or a debug progression preset. Case 149 additionally checks repeated completion events cannot reopen the bulletin or duplicate drone unlocks.

The gift's identity is supported by the parallel project's [pinned original-game observation](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/docs/m2-findings.md#L85-L103). This batch retains the serialized `Blazer` enum and the existing pulse-weapon completion path for compatibility; no save fields or original trade/war rules are changed.

A separate review fix removes an unattached Research placeholder button. Case 152 reproduced one orphan on opening the screen before the fix; first selection and closing now leave no orphan controls.

### Diagnostic evidence and limits

Cases 128/149 initially passed assertions but leaked `Typing.wav` playback resources at immediate shutdown. A standalone GDScript probe reproduced this without game or C# code. Godot 4.2.1 [queues stopped playback for deletion during mixing](https://github.com/godotengine/godot/blob/4.2.1-stable/servers/audio_server.cpp#L1169-L1186). The test helper now waits for two observed mixer cycles, with a two-second deadline, after stopping audio; it neither suppresses errors nor changes sound playback. Bulletin and Research lifetime tests subsequently passed strict checks. This does not establish a fix for native navigation shutdown hangs.

Case 150 also intermittently stalled while Research entered the tree. Removing its audio controller did not eliminate the stall; a native process sample showed the main and .NET finalizer threads waiting on locks. Diagnostic tracing altered reproducibility. The corrected fixture and final code passed a targeted run with production traces removed, but the intermittent failure has no proven fix. Preserve this issue for Windows investigation alongside case 65. Earlier fixture failures (unrecorded grapple prerequisite, unselected ship, and the war gift's automatic takeoff before a staged dock return) were corrected rather than patched around in gameplay code.

The full run, failures, process sample and minimal audio probe are retained locally under ignored `artifacts/validation/evidence/backlog-batch-4/`; they do not travel with Git. Reproduce and retain Windows evidence separately.

## Station status and missing graphics batch — 2026-10-02

The fresh build reported 14 existing warnings and no errors; five Python validator tests passed. The full 162-case attempt passed cases 1–149, then case 150 timed out after 180 seconds while entering Research. A new process sample again showed native/main-thread and managed/finalizer lock waits; no fix is established. A separate continuation passed cases 151–162, startup smoke and a fresh Windows export. **161/162 cases passed across these runs; no passing aggregate was written.** Case 65 passed this time; its earlier intermittent failure remains unresolved.

Cases 154–162 reproduce and cover three additional Asana reports:

- **1215691800680634:** the static station screen lacked all status labels. A dedicated `Station` controller now displays local orbital production, local shuttle status and deployed ground derricks. Readouts refresh after simulation, so completion and flight transitions appear on the same day. Tests distinguish ground/orbit factories, stored/deployed rigs, Earth/Moon state and removed ships. The attachment establishes idle wording and layout; other concise status labels describe the existing simulation without asserting original-game wording.
- **1215691800680642:** Production always showed its staff panel. AOC mode now shows the exact plaque supplied in the task attachment, hiding the staff frame, labels and removal button. Tests cover ground/orbit isolation, idle/active automation, return to manual mode and normal AOC completion without reopening the screen. The source image remains unmodified; a nearest-filtered atlas region excludes annotations. See [provenance and coordinates](../Godot/Sprites/References/README.md). No indicator animation or production timing is invented.
- **1215691800680646:** deposit analysis now reuses the existing platform artwork for the selected body's station. Selection and day updates clear absent/lost stations and distinguish construction from completion; moons do not inherit a parent's station. Course-selection maps hide this graphic. Tests use actual planet/moon/back controls and both system and satellite views. This adds the missing static graphic; original animation cadence is unverified.

Native macOS OpenGL runs of cases 154, 156, 158 and 160 passed strict log checks. Captures exposed clipped shuttle text; concise labels were substituted and the final ascent/system-chart captures rerun and inspected. Station text, the AOC plaque and the deposit icon fit the 320 × 200 layout. The AOC capture uses an isolated Production overlay fixture, so its persistent menu retains “Disk Access”; this does not test the normal Production menu context. Production fixtures exclude background audio and do not establish audio fidelity.

RED failures, focused logs and rendered captures are retained under ignored `artifacts/validation/evidence/backlog-batch-5/`. To reproduce a native capture after building, set `DEUTEROS_SCREENSHOT_DIR` to an absolute output directory, set `DEUTEROS_TEST_CASE` to 154, 156, 158 or 160, and launch `godot --path Godot --rendering-method gl_compatibility res://Tests/Regression.tscn` without `--headless`. Clear both variables afterward. Screenshots are optional evidence; the normal headless suite does not capture images. Windows rendering and acceptance remain pending.

## MTX route safety — 2026-10-02

The MTX module task **1215683087492485** remains partially implemented: this batch fixes transfer and selection failures, while installation/progression is still pending. It does not increase the count of 24 implemented Asana-linked fixes.

Cases **163–173** first reproduced stock being sent to captured, incomplete and unequipped stations; missing-destination exceptions; transfer of a locked selected item; an empty-list exception; and stale destination buttons selecting the wrong planet or throwing. A route configured only for locked items hung until the 20-second focused-test timeout. The item scan is now bounded, transfers require a friendly built receiver with an MTX, and button actions resolve the displayed planet ID instead of an index into a changed list. Saved routes are retained when a receiver becomes unavailable, allowing transfer to resume after friendly recapture. Opening an unknown saved destination falls back to the solar-system view without rewriting the route.

All 11 new cases and the five existing balance/conservation cases passed focused strict engine validation after the fix. RED and focused passing logs are under ignored `artifacts/validation/evidence/mtx-routes/`. These fixtures establish station and research state directly; they do not prove normal discovery, installation or Windows input behavior. [Original instruction evidence](original-behavior-evidence.md#1215683087492485--mtx-installation) now establishes the production-completion flag and duplicate queue guard needed for the remaining installation work.

The full **173-case** attempt passed cases 1–149, then case 150 timed out after 180 seconds at Research entry again. A process sample retained the native/main-thread recursive-mutex and managed/finalizer lock waits; no root cause is established. Without retrying case 150, a separate continuation passed cases 151–173, startup smoke and a fresh Windows cross-export. This is **172/173 across two runs, not a passing aggregate**. Five Python validator tests passed; the fresh build reported 14 existing warnings and no errors. Current-attempt logs are preserved separately under `mtx-routes/full-attempt/` and `mtx-routes/continuation/`; earlier stage logs were archived before the run to avoid mixing old smoke/export results with new failures.

## MTX installation and Stores access — 2026-10-02

Task **1215683087492485** now has an implemented installation/progression path, bringing the ledger to **25 Asana-linked fixes with local regression evidence**. Windows acceptance is still required. Seventeen new cases (**174–190**) expand the suite to 190.

- Completing paid MTX production installs the module at that orbital station, without adding transferable inventory. Manual and AOC builds charge one recipe; repeat/duplicate orders stop after installation. In-progress production and the installed flag survive save/load. An unavailable station suspends its active MTX build without charging again on resumption. These completion and duplicate rules follow the [decoded original instructions](original-behavior-evidence.md#1215683087492485--mtx-installation).
- Stores binds its MTX controller before discovery and gates the view by the local friendly, built, equipped station. Ground, enemy and unequipped stores retain their ordinary equipment view. Captured hardware can be inspected before discovery; installation and station loss update an open Stores view on the same day.
- Case 187 starts at a captured friendly station, discovers its MTX through Stores, researches through the real research selector, pays for manufacturing at Earth, then selects the Moon receiver and send/balance controls. It verifies conservation, visible counts and serialized route/installation state. Capture, qualified staff and an AOC are staged; the bulletin is shortened and subsystem updates are driven directly. These limits do not constitute a full campaign or original timing verification.
- Production marks an installed MTX unavailable and explains it in hover text. Missing construction frames use the existing item illustration; production/research images fit their fixed display boxes. The original animation frames remain a [separate media gap](media-gap-inventory.md).

The initial full attempt exposed a preset prerequisite failure at case 122: “activate MTX” previously set a module on an unbuilt station. The preset now ensures an orbital factory exists, and case 122 passes without weakening the new local access check. That failed attempt is preserved under `artifacts/validation/evidence/mtx-installation/preliminary-preset-failure/`.

All installation cases passed focused strict engine validation. Native Mac cases 187 and 190 passed, and the transfer, production and research captures were inspected. The artwork fixture uses isolated panels and retains the underlying menu/location context; the final transfer capture uses the normal Stores scene. Logs and captures are under ignored `artifacts/validation/evidence/mtx-installation/`. Reproduce captures with `DEUTEROS_TEST_CASE=187` or `190`, an absolute `DEUTEROS_SCREENSHOT_DIR`, and native `godot --path Godot --rendering-method gl_compatibility res://Tests/Regression.tscn` after a fresh build. These results do not certify Windows rendering, physical mouse hit-testing, full campaign progression or original animation/audio fidelity.

Final validation: cases 1–149 passed, then case 150 timed out at 180 seconds. A separate continuation passed 151–152 but case 153 timed out at its 45-second focused limit; its last log messages are bulletin/input-lock activity, and no managed trace was captured for that process. Without retrying either failure, a second continuation passed 154–190, startup smoke and a fresh Windows cross-export. **188/190 passed across three runs; there is no passing aggregate.** Preserve case 153 as a separate unresolved failure rather than assuming case 150's cause. Five Python validator tests passed; the fresh compilation had 14 existing warnings and zero errors. Final attempt logs are separated into `mtx-installation/full-attempt/`, `continuation/` and `continuation-after-153/`.

During the final full-run case-150 stall, a managed trace was captured with `dotnet-stack` 6.0.257301. It places the main thread inside `Research._Ready` → `Buttons.CreateButtons` → `GD.Load` → Godot's `ScriptManagerBridge` script creation, and the finalizer inside `GodotObject.Finalize` → native refcounted disposal → `ScriptManagerBridge.RemoveScriptBridge`. Combined with the earlier native mutex samples, this localizes the interaction to C# script registration/disposal; it does not prove which ownership or locking change fixes it. Raw evidence: `mtx-installation/case150-managed-stacks-arm64.txt`. The tool's packaged apphost was x64; invoking its managed DLL with the pinned arm64 `dotnet` and a command-local `DOTNET_ROLL_FORWARD=LatestMajor` captured the stacks without changing the game's runtime configuration. No speculative lifecycle patch is included in the MTX batch.

## Godot 4.2.2 script lifetime fix — 2026-10-02

The Research-entry stall is an engine lock-order defect addressed by [Godot PR #87669](https://github.com/godotengine/godot/pull/87669), backported to 4.2.2. On 4.2.1, temporary instrumentation reproduced case 150 stopping **inside the button prefab load**, before the separate script load or `SetScript`. Its managed stacks again showed script creation on the main thread and `RemoveScriptBridge` on the finalizer. The instrumentation was removed after capture; gameplay code and the button hierarchy remain unchanged.

The [4.2.1 destructor](https://github.com/godotengine/godot/blob/4.2.1-stable/modules/mono/csharp_script.cpp#L2827-L2836) holds the native script mutex while entering the managed script map. Script creation takes those locks in the opposite order. [4.2.2 releases the native mutex before the managed callback](https://github.com/godotengine/godot/blob/4.2.2-stable/modules/mono/csharp_script.cpp). The native mutex involved is compiled under `DEBUG_ENABLED`; this diagnosis concerns the observed editor/debug-runtime hang, not proof that every exported-game failure has the same cause.

A separate 32-button C# probe timed out on 4.2.1 during repeated script loading with garbage collection, capturing the same opposing stacks. Removing generic inheritance also timed out, so a generic-button rewrite is not justified. Explicitly disposing each loaded script completed 1,000 iterations in that probe, as did unchanged deferred disposal on 4.2.2. These controls identify the engine interaction; they do not justify disposing arbitrary shared game resources.

New case **191** exercises the real Research, Production and Stores prefabs through 60 load/press/free cycles with collection between loads. It verifies one selection event and the correct derrick index/order after each load. It timed out after 30 seconds on 4.2.1, then passed strict validation on 4.2.2. Cases 150, 153 and 65 also passed focused checks. Case 153 has no historical stack, and case 65's earlier invalid-handle failure is not independently explained by this fix.

The engine, `Godot.NET.Sdk`, installer checksums, validator and Windows instructions now pin **4.2.2**; .NET SDK 6.0.428 is unchanged. **Clean-cache validation passed 191/191 isolated cases**, startup smoke and fresh Windows cross-export. The previous `.godot` directory was moved aside before building/importing. Five Python validator tests passed; the clean compilation reports 14 existing warnings and zero errors. All four installer checksum pins match the official 4.2.2 release manifest. The exact editor teardown exception remains limited to import/export; no gameplay error filters changed. Raw comparison evidence is under ignored `artifacts/research/script-lifetime/` and `artifacts/validation/evidence/script-lifetime/`; reproduce case 191 from the committed runner on Windows.

Native Mac cases 187 and 190 also passed on 4.2.2; their MTX transfer, production and research captures were inspected. The transfer shows iron 51 and the illustrations remain inside their intended panels. These staged fixtures do not certify physical pointer behavior, original audiovisual fidelity or native Windows gameplay.

## Methanoid and damaged-base menu artwork — 2026-10-02

Two more Asana tasks have implementation/local regression evidence, bringing the ledger to **27/48**; native Windows acceptance remains pending.

| Task | Player-visible change | Evidence |
| --- | --- | --- |
| 1215691800680621 — missing menu icon | The supplied Methanoid face appears in menu row four while the current ship is undocked in orbit around an occupied, built station. It clears during transit, after friendly capture, on ground and outside the interior. | Cases 197–198; native Mac capture `methanoid-orbit-menu.png`. The still reference supports a passive graphic, not a new communication action or animation. |
| 1215691800680623 — missing moon-base graphic | A built but damaged base shows crossed-out Resource and Mining Store icons. Their old navigation/actions are cleared. Orbital services and the surface shuttle bay remain available; normal shuttle repair restores ground controls that same day. | Cases 192–196/199; native Mac capture `damaged-moon-menu.png`. Moon initial data, the mining damage check and `Shuttle.CompleteRepairs()` establish the existing state; no new repair duration or economics are introduced. |

The unmodified task screenshots are committed with [atlas provenance](../Godot/Sprites/References/README.md). Atlas regions exclude annotations and scale to the existing menu slots. Incomplete bases remain blank, and moving to another body replaces stale damage art. Availability refresh runs after day updates without replacing the current screen's title.

The initial regressions reproduced enabled damaged-base controls, unintended ground-screen navigation and the absent Methanoid indicator. One initial bad-navigation reproduction also retained background resources after opening GroundMaterials; that failure is preserved. During review, case 199 caught an overly broad refresh overwriting “News Bulletins” with the planet label; narrowing it to menu availability resolved that regression. Eight cases (**192–199**) now pass focused strict validation. Native Mac cases 192/195/197/198 passed; final post-review captures reran 192/197. These fixtures stage stations, ownership and ships; the repair uses the normal day update with a staged repair kit. They do not certify an unstaged campaign, physical pointer behavior or Windows rendering.

The full attempt passed cases 1–149, then case 150 completed its gameplay assertions but failed during shutdown with `FATAL: Condition "!rc_owner" is true` in `_instance_binding_reference_callback` and timed out after 180 seconds. This is after Research entry and differs from the captured 4.2.1 script-creation deadlock. Without retrying case 150, a continuation passed cases 151–199, startup smoke and a fresh Windows cross-export. **198/199 passed across two runs; there is no passing aggregate.** All eight new cases passed. Five Python validator tests passed; the last recompilation reported 14 existing warnings and zero errors (the subsequent incremental full-run build reported zero warnings). The previous fully passing aggregate remains the 191-case engine-patch batch. A native process sample was captured after the fatal error; `dotnet-stack` exited successfully but returned no managed frames. The root cause of this shutdown failure is not proven. Logs and native captures are under ignored `artifacts/validation/evidence/menu-artwork/`; Windows must regenerate them. Set `DEUTEROS_TEST_CASE=192` or `197` and an absolute `DEUTEROS_SCREENSHOT_DIR`, then run `godot --path Godot --rendering-method gl_compatibility res://Tests/Regression.tscn` after building for native captures.

## Store and Ship Bay ambience and audio shutdown — 2026-10-02

The two supplied ambience tracks now play in ground/orbital Stores and Ship Bays. MTX shares its parent Store player, so toggling views does not create another loop. Existing master-bus sound preferences apply. This implements work item S1 in the [media inventory](media-gap-inventory.md); it does not complete the broader missing-sounds task or increase the **27/48** implementation count.

Cases **200–204** first failed because those scenes had no player, then passed after adding the existing background prefab. They verify the selected looping resource, a single playing instance, stop/free on scene exit, MTX continuity and settings mute/volume. Native Mac cases 201/203/204 also passed. These are playback-state checks, not listening or original-cue fidelity evidence.

The first full attempt then failed at **case 10**: its assertions passed, but immediate shutdown retained `AudioStreamPlaybackOggVorbis`, its stream and packet sequence. The failure is preserved. Focused cases had explicitly drained stopped playback, exposing a difference between their cleanup and the game's direct exit. `GameCore.RequestQuit` now handles normal window-close requests: defer out of input dispatch, stop simulation, release active screens (including typing cancellation), observe two audio mixes within a two-second bound, then quit. Failure to observe the mixes remains an error. The regression runner uses this production shutdown path; strict diagnostics are unchanged.

New case **205** navigates Store → Ship Bay → Training three times, opens paused settings, and emits the window-close notification. The external validator checks process termination and engine logs. Focused headless cases 10, 65, 150 and 200–205 passed; native Mac cases 10/150/205 passed. The subsequent **full aggregate passed 205/205**, startup smoke and fresh Windows cross-export. Five Python validator tests passed. The last recompilation reported 14 existing warnings and zero errors; the subsequent incremental build was clean. Raw evidence is under ignored `artifacts/validation/evidence/ambience/`; the successful full-run logs are in `artifacts/validation/`.

This establishes a fix for the reproduced audio-disposal leak in the exercised exit paths. It does not establish the root cause of every historical `!rc_owner` or invalid-GC-handle failure. Physical window-close interaction, listening quality and native Windows source/export acceptance remain pending; the native automated case invokes the notification programmatically. Keep the prior failure records when assessing readiness.

## Supplied construction-frame recovery — 2026-10-02

Nine construction frames are restored for IOS chassis, interplanetary drive and resource-station frame (`i_chassis`, `i_drive`, `r_frame`). The three runtime sheets are byte-identical to their supplied source files; [provenance](../Godot/Sprites/Items/Sheets/README.md) records hashes and atlas rectangles. A small display material makes only their magenta key transparent. Existing PNG frames, static research fallbacks and idle artwork retain their previous rendering path. Production timing, recipes and quantities are unchanged.

Cases **206–211** first reproduced the static illustration being displayed instead of construction stages, then passed through manual and AOC production of each item. They verify all three stage crops, completion, exact material charges, idle and switching back to existing PNG/fallback artwork. Native Mac cases **206/208/210** also passed **32,256 pixel comparisons** across the nine frames: every artwork pixel matches its source colour and every keyed pixel reveals the underlying scene. The three final-stage captures were visually inspected. Headless runs skip these GPU checks; Windows native rendering remains pending.

Visual inspection of the other eleven candidate sheets found placeholder text, empty boxes or unverified drawings. They were not exported as genuine frames. The production rod animation is a separate unresolved task; the animation inventory remains partial and the implementation count stays **27/48**. Raw RED/GREEN/native evidence and captures are under ignored `artifacts/validation/evidence/construction-artwork/`.

The full aggregate passed **211/211**, startup smoke and a fresh Windows cross-export. Five Python validator tests passed; the last recompilation reported 14 existing warnings and zero errors. The Windows executable's embedded pack directory includes all **17** new resource entries: nine atlases, shader/material, three image-import records and three imported textures. This confirms packaging, not native Windows rendering or gameplay.

## Known runtime limitations

The combined diagnostic suite and isolated cases 61/65 reported `SwapGCHandleForType: Handle is not initialized`. The [input-lifetime investigation below](#queued-test-input-lifetime--2026-10-02) now reproduces a test-helper trigger: prematurely disposed injected mouse events later consumed by physics picking. The corrected helpers leave ownership with Godot. This does not establish the cause of every earlier shutdown failure. Strict error detection remains enabled. A texture-retention experiment did not fix the reproduction; [Godot issue 112067](https://github.com/godotengine/godot/issues/112067) is therefore not evidence for adopting a texture-cache workaround here.

A broader navigation smoke (start game → Earth ground → training → Escape/settings → close) previously reproduced Ogg resource leaks or a `!rc_owner` shutdown hang on macOS. Clearing the audio player's stream did not resolve it. An isolated, checksum-verified Godot 4.3 / SDK 4.3.0 comparison compiled and passed the original 26-case suite, but repeated navigation still reproduced shutdown failures. That comparison did not establish an audio fix. The 4.2.2 patch addresses the separate script-registration deadlock; the subsequent shutdown change above addresses observed mixer-disposal leaks. Other causes of the historical fatal errors remain unproven. [Godot issue 89188](https://github.com/godotengine/godot/issues/89188) is related context, not proof of an identical cause.

Fresh-export scene leaks were traced separately to the engine's optional binary scene conversion. Keeping text resources eliminates those export leaks; only the exact documented editor teardown diagnostic is permitted during import/export. All other engine errors fail validation.

Linux/Windows CI has been authored and its YAML parsed locally, but it has not run remotely. Actual Windows gameplay, original-game fidelity, the full original day-event order and long save/load progression are not certified by this suite. Before release, exercise the Windows executable and the affected screens on Windows.

## Follow-up priorities

Continue through the remaining backlog, including Windows ACC activation reproduction, news, missing art/features and Windows acceptance. Deposit/store-cap conservation and bounded overview rendering remain separate review findings. Resolve original-game evidence questions before changing economics, travel, combat or timing. The parallel port's decoded research is useful; its documented provisional behavior is not an acceptance specification.

## Persistent menu click feedback — 2026-10-02

The previously unused supplied `sMainMenu_Button.wav` now plays through one persistent `MenuBase/MenuClickSound` player. Top and side menu controls use accepted activation; hold-to-advance uses press only. Binding before child navigation callbacks preserves the accepted cue when a scene change immediately disables the clicked side-menu slot. Disabled, hidden, empty, paused or globally locked controls are silent. Repeat tree entry retains one connection set; the player stops on exit. Existing training sounds and title-screen controls retain their behavior.

Cases **212–219** all failed before implementation because the player was absent, then passed focused strict checks. They exercise actual viewport pointer/key input, the `ui_accept` action, scene changes, cancelled/disabled input, time toggle/hold/release, input blocking, pause/mute and repeated tree entry. Native Mac cases **212/216–219** also passed with clean process exit. The automated action event is not evidence of physical controller testing. Build: 14 existing warnings, zero errors. Five Python validator tests pass.

**Full validation passed 219/219 isolated cases in one run**, strict import, startup smoke and a fresh Windows cross-export. The exported pack directory contains the menu scene, WAV import and imported sample. The exact known editor teardown diagnostic remains limited to import/export; no gameplay error filters changed. This passing run does not independently establish the root causes of earlier case-65 GC-handle or case-150 shutdown failures. Raw red/green/native logs and the full-run driver log are retained under ignored `artifacts/validation/evidence/menu-sound/`; previous validation logs and export were moved aside first. Native Windows listening, levels, physical input and original cue fidelity remain pending. Wiring all 11 supplied runtime audio assets does not complete the original cue inventory (S3/S4), or increase the 27/48 Asana-linked implementation count.

## Methanoid trading decisions and interrupted text — 2026-10-02

For Asana **1215691951441128**, the existing trade question now leads to an explicit Accept/Decline dialog showing each offered quantity and mineral exchange. [Verified original bytes](original-trade-evidence.md) determine the counter: accept +1, decline -1 with a zero floor; the next encounter enters war at >=16. The existing `AtWar` flag blocks further peaceful exchanges and duplicate gifts. Only nonempty supply cargo with table IDs 1–16 participates; equipment and empty pods do not advance the count. Escape abandons the modal without deciding, changing cargo or launching; that is an explicit modern UI behavior.

Six red regressions reproduced automatic acceptance/missing refusal, unsupported cargo counting, the above-16 gate, repeated war gifts and the screen lock left by interrupted typing. Cases **220–235** now pass focused strict checks, covering all exchange-table entries, quantity preservation, repeat signals, stale offers, pointer/key input, cancellation, response interruption and the 15→16→war boundary. The cargo exchange and counter commit together before the response. Module text cancels its pending timer and audio on tree exit; `finally` releases its screen lock. Case **150** also passed after its normal comms research/manufacture/fitting path was updated to press Accept.

Initial focused cases 224/225 failed because their fixture retained a replaced interior after normal departure. The corrected fixtures use the current scene; original logs are retained. Native Mac cases **220/227/231–233** passed. Screenshot review caught clipped dialog text; the two-line correction and width assertion passed native cases 220/232, and the corrected capture was inspected. Native input events are automated, not physical-device acceptance. Compilation retains 14 existing warnings with zero errors.

The first full attempt passed cases 1–211, then case 212 failed its instantaneous `Playing` assertion. A controlled 200 ms navigation delay reproduced that false failure: the 87 ms cue had finished exactly once by the time the callback returned (207 ms observed). Cases 212/213 now require the existing bounded `Finished` event/count check instead of assuming playback is still active after synchronous navigation. Case 212 retains the controlled delay; both cases passed headless and native checks. No production audio behavior or error filters changed. The failed aggregate and diagnostic probe are preserved. After that correction, **235/235 isolated cases passed in one full run**, plus strict import, startup smoke and a fresh Windows export. Five Python validator tests passed; the new trade dialog and module-text scene are present in the exported pack. The known exact editor teardown diagnostic remains limited to import/export; no gameplay exemptions were added.

Raw evidence is under ignored `artifacts/validation/evidence/trade-decision/`, including red failures, fixture corrections, both native layouts and the full-run driver log. Previous aggregate logs/export were moved aside first.

The task remains **partially implemented**, outside the existing 27 task-level implementation count: original decision timeout duration, ship-byte +6 semantics, IOS/SCG cargo-layout correspondence and native Windows acceptance are outstanding. No arbitrary deadline, fuel refill or original-looking artwork was introduced; the supplied modern settings style provides the choice. This batch does not certify all original trading mechanics or close the Asana task.

## Hull travel restrictions and elapsed arrival — 2026-10-02

As part of **1215685674676241 — SCGs**, only SCGs can now choose or engage a destination in another star system. The rule is supported by [original-game observation](original-behavior-evidence.md#1215685674676241--scg-and-ios-travel). Both course selection and `EngageEngine` use body records rather than trusting cached star fields. Rejected selections preserve the course and ACC endpoints, release the map cursor and show a short explanation. Successful engagement synchronizes star fields with the actual bodies. Invalid destinations cannot start transit or award a pilot action.

ACC activation and continuation reject invalid hull routes before loading fuel/cargo or launching, clear active/cycle/refuel flags, and leave inventories intact. The Cycle control cannot set cycle mode after activation fails. Shuttle ground/orbit ACC retains its existing path. SCG interstellar and IOS local travel remain available.

Eight pre-fix cases reproduced forbidden departure, stale metadata, unknown destinations, invalid course selection and ACC resource/state changes. **Cases 236–248 pass focused strict checks**; native Mac cases **241/243/246/247** also passed, and the IOS rejection screenshot was inspected. Compilation reports 14 existing warnings and zero errors.

Case **248** separately reproduced a stranded SCG on Earth→Cerberus: matching local orbital indexes produce a zero deadline in the existing duration formula, which becomes negative on the next tick. Arrival now tests `<=0` instead of `==0`; this does not establish original journey duration. The first fixture incorrectly used Chiron (index 0); its failed setup assertion is retained separately, and the corrected index-2 Cerberus fixture reproduced the actual transit failure before the fix.

**Full validation passed 248/248 isolated cases in one run**, strict import, startup smoke and a fresh Windows export. Five Python validator tests passed. The exact known editor teardown diagnostic remains limited to import/export; no gameplay error filters changed. Raw evidence is under ignored `artifacts/validation/evidence/hull-travel/`; previous logs and export were archived first. Normal SCG progression, original duration, already-in-flight legacy-save behavior and Windows acceptance remain outstanding. This is partial feature work and does not increase the existing 27 task-level implementation count or close the Asana task.


## Ship assembly and inventory conservation — 2026-10-02

For **1215685674676241 — SCGs**, tracing assembly exposed five concrete problems: entering any bay unlocked star-drive research; the SCG button checked shuttle technology; chassis and engine fitting did not remove stock; repeated IOS/SCG creation could leave two ships docked and throw `Sequence contains more than one matching element`; and the SCG bay exposed an unusable sixth mount despite its five-module model.

The bay now preserves research state, gates the SCG selector on its own chassis, consumes one chassis and drive when fitted, rejects an occupied berth, and exposes five functional SCG mounts. Dismantling returns chassis and installed drive alongside the existing cargo/crew returns, with the same atomic capacity check. This extends the local conservation fix for **1215716464570907**; it does not add a newly accepted Asana task.

Cases **249–263** reproduce those defects and check no-stock rejection, repeated callbacks, all three hulls, ground/orbit shuttle drive stock, two build/fit/dismantle cycles, unpowered hull returns, exact/full storage capacity and all five SCG pod fittings. All 15 passed focused strict checks after the fixes; native Mac cases 250/252/259/260/263 passed, and the SCG capture was inspected. Earlier dismantling fixtures now treat stock as spares beside fitted hardware; the empty-bay hover fixture unlocks the relevant SCG chassis.

The first full attempt passed cases 1–60, then failed strict validation at **case 61** with two `SwapGCHandleForType` and one `SetGodotObjectPtr` “Handle is not initialized” errors. Assertions passed and the process exited, which does not override the engine errors. At this checkpoint the stack signatures matched the earlier case-65 failure but the resource and cause were unknown; the subsequent input-lifetime investigation below identifies a reproducible test-helper trigger. The failed attempt is preserved and case 61 is not retried to obtain a green result. The continuation passed cases 62–263, startup smoke and a fresh Windows cross-export: **262/263 strict passes across two runs, not a passing aggregate**. Build and strict import passed; the five Python validator tests passed. Recompilation retains 14 existing warnings, with zero errors. No green `regression-summary.log` was written. Raw evidence is under ignored `artifacts/validation/evidence/ship-assembly/`, including every red log, focused/native results and the preceding 248-case aggregate/export.

These fixtures stage ships, available parts and technology; they do **not** prove the normal SCG discovery/research/manufacture chain. The current unlocker has no normal SCG/interstellar trigger. The parallel port explicitly labels its Sol-cleared unlock chain a reconstruction, so it has not been imported as original evidence. Part conservation is a remake consistency correction; original dismantling recovery rules still need comparison. Old saves do not record whether parts were deducted when fitted: no retrospective stock subtraction or economic migration is performed. Native Windows acceptance and original interstellar duration remain open.


## Queued test input lifetime — 2026-10-02

The case-61 failure from the assembly batch was reproduced in an 80-cycle probe using the actual Ship Bay → Stores → Overview navigation, followed by managed collection. The original probe logged **15 handle errors**, and the native-stack probe logged **12**. The errors occurred during the frame after Ship Bay reopened, not during its constructor. A managed texture-cache experiment still logged **12** errors and was reverted.

A paused first-chance exception allowed native sampling. Resolving its addresses against the official arm64 binary identified `SceneTree::physics_process` → `Viewport::_process_picking` → `RefCounted::reference` → `CSharpLanguage::_instance_binding_reference_callback`. The [4.2.2 viewport source](https://github.com/godotengine/godot/blob/4.2.2-stable/scene/main/viewport.cpp#L740-L751) takes a reference to an input event previously queued by the unhandled-input path. The test helper explicitly disposed injected mouse releases before that consumer necessarily ran. An empty-scene probe did not reproduce the issue, so it is not used as proof; the real navigation sequence is retained instead.

New **case 264** repeats that real sequence 80 times with collection between cycles. Before the fix it logged **12 errors** despite passing gameplay assertions. Removing explicit disposal of injected mouse events made the same probe pass. Navigation, menu, time-control and trading pointer helpers now let Godot manage those events after `PushInput`; synchronous direct-callback fixtures are unchanged. There are no production or engine-version changes, no suppressed log errors, and no removal of input, collection or navigation assertions.

Focused strict cases 61/65/264 pass. Native Mac cases 61/65/212/232/264 also pass, including all 80 stress cycles. The first full attempt passed 1–22, then **case 23 failed** after its icon assertion passed: native `handle_crash: Program crashed with signal 11` during teardown, followed by leaked `CSharpScript` resources for `SceneChangeButton`, `HoverButton` and `TimerHoldButton`. This is distinct from the queued-input trigger and remains unresolved. Case 23 is preserved rather than retried for a green result. Continuation **24–264**, startup smoke and a fresh Windows export passed: **263/264 strict passes across two runs, not a passing aggregate**. No green summary was written. Recompilation retains 14 existing warnings and zero errors; strict import passed.

The validator now also rejects `handle_crash: Program crashed` and `FATAL:` lines, even with a success marker and no accompanying `ERROR:`. A new unit test failed for both headers before the change; all **six Python validator tests** pass afterwards. The existing narrow editor exception is unchanged. Research/probe logs and sampled stacks are retained under ignored `artifacts/research/gc-handle-navigation/`; canonical verification is under `artifacts/validation/evidence/input-lifetime/`.

This corrects a reproduced harness lifetime error; it does not increase the **27/48** task implementation count, replace Windows gameplay acceptance, or establish the cause of the separate historical `!rc_owner` shutdown fault.

## Supply-pod discard — 2026-10-02

Task **1215685674676219** now has an explicit **Cargo...** dialog in the ship interior, with a **Ditch** action beside each loaded supply pod. It clears only that pod's cargo type and quantity, keeps the pod fitted, and credits no stores. Empty, tool and cryogenic slots cannot be ditched through this control. Closing or leaving the screen invalidates retained actions; changed cargo must be inspected again. The modal uses the existing pause/cancel behavior and preserves the usual module-click actions.

The [original instruction evidence](original-supply-pod-evidence.md) connects the “Ditch Contents !” text and hit rectangle to dispatch `$6D` and handler `$33F40`. Disk hash, the entire handler, table entry and text lookup were checked against the original image. Its packed-word write proves discard of the selected cargo; the separate bulk-unloading routine credits stores. Original screen entry has state gates, so docked access through this modern dialog follows the task request, not a claim of identical original navigation.

Cases **265–274** first failed on the missing control, then passed after implementation. They exercise real pointer input, all three hulls including the fifth SCG pod, docking/launching/transit/orbit, a staged AMA-mining state, unchanged ground/orbital inventories, retained ACC/journey/mining fields, save round trips, input locks, cancellation, stale cargo and screen exit. The initial fixture needed complete ACC lists/cursors for save validation; those setup failures are retained. Case **275** then reproduced a retained Ditch signal mutating cargo after Close, before queued deletion. Checking the dialog's queued-deletion state and overlay ownership prevents that write. The first aggregate was deliberately stopped to add this regression; it is not a completed run.

Native Mac cases **267/272/273/274** passed; final cases **267/275** also passed with pointer-driven dialog opening and the close guard. Screenshots of the five-pod dialog, mining dialog and interior entry were inspected. A clipped explanatory line was corrected and recaptured. These are staged fixtures, not normal campaign progression or native Windows acceptance. Raw red/green/native logs, the interrupted attempt, preceding logs and export are retained under ignored `artifacts/validation/evidence/supply-pods/`.

The final 275-case attempt passed cases **1–182**, then case **183** failed strict teardown after its MTX assertion passed. It logged `FATAL: Condition "!rc_owner" is true` at `_instance_binding_reference_callback`, leaked `CompressedTexture2D`/`PackedScene` references, 18 texture RIDs and resources still in use. This signature resembles the historical shutdown fault, but a shared cause is not proven. The failed log is preserved and case 183 is not retried for a green result. Case **184** also failed in the continuation after its MTX assertion passed, with the same fatal callback signature, one leaked texture RID and resources still in use. Both failed cases are preserved without retries. Cases **185–275**, startup smoke and a fresh Windows cross-export then passed. Final accounting is **273/275 strict passes across the full attempt and two continuations**, with cases 183/184 still failed; no green aggregate summary was written. All eleven new supply-pod cases pass on the final code. Six Python validator tests pass; recompilation retains 14 existing warnings and zero errors. The Windows pack directory contains the new dialog and supply-pod source entry (1,103 entries total), with no test-resource entries. This confirms packaging, not native Windows gameplay. The existing case-23 native teardown crash and historical case-150 fatal shutdown remain separate unresolved findings. The implementation count is now **28/48**; native Windows acceptance and original screen-availability comparison remain pending.

## Shutdown finalizer drain — 2026-10-02

The case-183 failure from the supply-pod batch reproduces on unchanged code in **4/4** diagnostic repetitions; case 184 fails **1/4**. Controlled case-183 probes fail with logging only (**4/4**), waiting for existing finalizers only (**4/4**) or a 100 ms delay (**3/4**), but pass with collection followed by a finalizer drain (**4/4**). The [diagnosis and primary-source links](shutdown-finalizer-evidence.md) explain the weak-reference cleanup boundary and distinguish this result from unrelated causes of the same fatal signature.

New **case 276** holds the finalizer thread briefly, queues 64 resource wrappers, verifies their weak targets have cleared, then exercises the production window-close path. The old code passes its assertions but leaks all 64 native references; strict validation fails. The fix collects and waits for finalizers immediately before engine quit, after scenes and audio have been released. There is no gameplay collection, arbitrary production delay, engine upgrade or broadened error exemption. All temporary logging/mode controls are removed.

Cases **183/184/276** each pass four focused repetitions. Native Mac cases **23/150/183/184/205/264/276** pass, including the 80-cycle navigation stress and pending-finalizer reproduction. **Fresh full validation passed 276/276 isolated cases in one run**, strict import, startup smoke and a fresh Windows cross-export. All six Python validator tests pass. The final compile retains 14 existing warnings and zero errors; no gameplay errors were exempted. The Windows pack contains 1,103 entries, including the supply-pod dialog, with no test resources. This verifies packaging, not native Windows gameplay. This does not increase the **28/48** task-level implementation count or certify native Windows acceptance. Raw controlled failures and the minimal project are under ignored `artifacts/research/mtx-shutdown/`; regression and validation evidence is under `artifacts/validation/evidence/shutdown-finalizers/`.


## Engine damage and recovery — 2026-10-02

Task **1216073204565506** now has an engine-damage workflow following the [traced original escape rule](original-engine-damage-evidence.md). A player IOS/SCG leaving wartime hostile orbit without DFCC has a binary damage roll. Both an occupied destination reached through travel and an orbit reached by an attacking fleet are covered. Danger entry alone does not damage the engine. Protected, peaceful, enemy and safe departures do not roll; invalid departures and repeated callbacks cannot reroll. The remake uses its existing .NET random source for the binary choice, not a reproduction of the original random sequence. Original countdown cadence is still unverified, so attack/destruction timing is unchanged.

Damage stays distinct from absence: the engine remains fitted, the interior shows **Damaged !**, and the ship can travel at double duration. The existing fitting control replaces a damaged drive for one matching local spare and clears engagement/damage. Dismantling discards damaged drives rather than returning usable stock or requiring storage space for them. Healthy drive recovery is unchanged. The original fuel precondition now prevents fuel-free engine engagement.

Cases **277–293** cover both hulls and danger contexts, both roll outcomes, immunity/invalid routes, unavailable spares, repeated fitting, all three hulls' replacement/dismantling, save/load, older saves, pointer-driven controls, doubled arrival and return ETA. Missing behavior was reproduced after adding only the state/API boundary: no roll, normal-speed damaged travel, rejected replacement, damaged-drive salvage, absent warning and fuel-free departure. A later test caught the strict loader rejecting older saves without the new field; only that field now permits absence, while explicit null and missing pre-existing fields remain rejected.

The first bay/interior fixtures mutated damage after opening screens which refresh on events, not every frame. They were corrected to enter with the damaged ship already present, including the consumed ship-selection token. A test variable shadowing compile error is also retained; no stale binary was tested after that failed build. Case **293** separately reproduced a zero-day return preview after an eight-day damaged journey: elapsed time is now subtracted only in transit, so waiting in orbit cannot shorten the next route.

All new cases have focused strict passes. Native Mac cases **277/280/285/287/291/292/293** pass, including actual pointer-driven replacement/departure. The damaged-engine transit screenshot was inspected at the native 320×200 layout. Compilation retains 14 existing warnings and zero errors. **Fresh full validation passed 293/293 isolated cases in one run**, strict import, startup smoke and a fresh Windows export. All six Python validator tests pass. The export pack contains 1,103 entries and no test resources; source hashes stayed unchanged through validation. The known exact editor teardown diagnostic remains restricted to import/export; no gameplay error exemptions were added. Raw failures, fixture corrections, focused/native results and earlier aggregate/export are preserved under ignored `artifacts/validation/evidence/engine-damage/`.

The local implementation count is now **29/48**. Native Windows source/export, physical controls, task-owner review of the original escape interpretation and normal campaign progression remain acceptance work; no Asana task is closed.


## DFCC fuel cost and conservation — 2026-10-02

Task **1215716464570901** now uses the [original ten-stock-per-gauge-unit rule](original-dfcc-fuel-evidence.md) for fitted DFCC ships. `FuelUnitCost` is derived from the existing hull flag, with no new saved field. Manual loading, unloading, ACC and dismantling apply the same ratio to MeH/HeD fuel. Ordinary ships retain 1:1 transfers. Gauge capacity and tick depletion are unchanged, and drone count does not affect the ratio.

ACC keeps sub-unit stock remainders, waits without consuming stock when its minimum cannot be reached, and clears the waiting flag when sufficiently fuelled. Manual unloading cannot discard fuel into full stores. Dismantling checks the combined converted tank return and raw cargo before changing either. Initial DFCC fitting returns the old tank at the old rate before changing the hull flag; a full store rejects the entire fitting, preserving equipment and fuel. The bay now refreshes the tank and stock readouts immediately after fitting.

Cases **294–304** cover IOS/SCG, both fuel types, partial/exact payments, tank capacity, full/exact return capacity, automatic thresholds/remainders, conversion rejection/retry, combined cargo returns, zero/one/full drone fleets, unchanged normal-hull costs and save persistence. Pre-fix tests reproduced undercharging, incorrect dismantling totals and fitting without fuel return. A subsequent display assertion reproduced the stale 25-unit readout after the tank was emptied; it passes after the refresh correction. All eleven have focused strict passes. Native Mac cases **294/296/298/300/301/303** pass; the SCG fitting screenshot was inspected. These exercise scene callbacks, not a physical Windows mouse or normal campaign unlock sequence.

Compilation retains 14 existing warnings and zero errors. **Fresh full validation passed 304/304 isolated cases in one run**, strict import, startup smoke and Windows cross-export. All six Python validator tests pass. The pack contains 1,103 entries, with no test resources, and validated source hashes remained unchanged. The known exact editor teardown exception remains limited to import/export; no gameplay error exemptions were added. Failed, focused/native and full-run evidence is preserved under ignored `artifacts/validation/evidence/dfcc-fuel/`, with previous logs/export archived first.

The compatibility choice preserves the range of already-loaded tanks in existing saves and applies the new ratio to future transfers/refunds. Such pre-fix tanks can refund more stock than originally paid; no retroactive inventory adjustment is made. Original ACC availability after DFCC conversion is not established; applying the same ratio to the remake's supported ACC path is a conservation choice. Native Windows and task-owner acceptance remain pending. The local implementation count is **30/48**; no Asana task is closed.


## Grapple-only asteroid ACC — 2026-10-02

Task **1215683087492480** now has a traced answer: the [original scan branch](original-asteroid-acc-evidence.md) calls the manual Disengage handler when normal engaged ACC finds no AMA, before mineral and size filters. It does not capture cargo automatically. The remake previously left ACC engaged; case **305** reproduced that mismatch before the correction.

`ACC.Update` now disengages normal engaged automation at a non-null asteroid scan without AMA. It retains the scan for manual grappling and leaves cargo, fuel, routes and selection settings intact. No-scan waiting and the existing AMA approach remain unchanged. Complete Cycle is distinguished from ordinary Engage and its wider lifecycle remains investigation work.

Cases **305–309** pass focused strict checks: stop/conservation, waiting and mode boundaries, AMA docking, actual daily scanning, and save/load followed by manual grapple capture. Native Mac cases **306/308/309** also pass. These use staged fixtures and scene callbacks, not physical Windows controls or normal campaign progression.

**Fresh full validation passed 309/309 isolated cases in one run**, strict import, startup smoke and Windows cross-export. All six Python validator tests pass. Recompilation retains 14 existing warnings and zero errors. The export pack contains 1,103 entries and no test resources. No gameplay errors were exempted. Failed/focused/native evidence, previous logs/export and the final export/source hash audit are retained under ignored `artifacts/validation/evidence/asteroid-acc/`.

The local implementation count is **31/48**; native Windows acceptance remains pending and no Asana task is closed. Full AMA timing/yields, original SCG mining availability and Complete Cycle behavior remain separate research questions. A possible mixed-mineral mining failure was identified during review and still needs a controlled reproduction.


## AMA compatible cargo — 2026-10-02

Follow-up to **1215685674676221** reproduces three `Sequence contains no matching element` failures in the daily mining path and an ACC approach despite no pod accepting the selected ore. The [original mining trace](original-ama-mining-evidence.md) establishes compatible-pod selection, a 250-unit cap and departure when none is usable.

Mining now skips incompatible/full pods, recognizes zero-count pods regardless of stale mineral type, and leaves without altering cargo when no pod can accept ore. ACC returns rather than docking for a selected mineral it cannot store. Cases **310–313** fail before and pass after the correction. Existing case 307 initially failed because its successful-mining fixture contained only incompatible cargo; that fixture now provides an empty pod, while case 311 explicitly tests the incompatible return. Both results are preserved.

Focused cases **305–313** pass. **Fresh full Mac validation passed 313/313 isolated cases in one run**, strict import, startup smoke and Windows cross-export. Compilation retains 14 existing warnings and zero errors. Six Python tests passed when that run started; the separate Windows tooling change subsequently expanded them to nine, all passing. No gameplay errors were exempted. Raw evidence and previous logs/export are under ignored `artifacts/validation/evidence/ama-cargo/`.

These are daily-state fixtures, not physical controls or normal campaign acceptance. The first native Windows baseline covers the earlier `44e37ba` revision and 309 cases; it does not validate this newer correction. A fresh native run and desktop acceptance remain pending. The task-level implementation count stays **31/48** because the broader AMA yield, timing and eligibility research remains incomplete.


## Native Windows baseline and RCEdit setup — 2026-10-02

Revision **`3004d9b`** passed fresh native Windows validation: **313/313 regressions**, strict import, source startup smoke and Windows release export in one canonical run. All nine Python tests pass, including three new RCEdit integrity/cache tests. A separate smoke check executed the packaged Windows game through an external test driver; its pack contains 1,103 entries and no test resources. See [the full Windows report](windows-validation-results.md) for environment, hashes and limits.

The initial `44e37ba` run passed 309 regressions and source smoke but failed export because RCEdit was missing. That attempt is preserved. The installer now downloads checksum-pinned RCEdit 2.0.0 alongside the Windows Godot binary when installing templates, and the validator adds its directory to the child PATH. A corrupted download never becomes `rcedit.exe`; modified cached binaries are rejected. The fresh Windows run exercised the real download and automatic PATH setup. No errors were excluded to obtain a passing result.

The Windows checkout was clean before import. Afterwards, 344 import sidecars appeared in status with line-ending notices, but normalized `git diff --exit-code` was empty. Source content matches the tested revision. Desktop controls, audible output, GPU rendering and normal campaign progression remain unverified; the local implementation count remains **31/48**, with no task declared fully accepted or closed in Asana.


## ACC Complete Cycle — 2026-10-02

Follow-up to **1215683087492480** restores the [original ground/orbital unload-and-stop command](original-acc-cycle-evidence.md). Complete Cycle starts or finishes one leg, then unloads without taking fuel, loading return cargo or departing again. Mode changes preserve an existing journey/fuel wait. Legacy dual flags finish on arrival, Engage cancels finishing, and the interior displays the actual mode and fuel wait. Finish mode cannot start another automatic asteroid mining approach; the wider mining-expedition lifecycle remains open.

Six focused cases reproduced the old failures before the correction. Cases **314–329** now cover shuttle/IOS controls, fuel waits, overflow conservation, save/load, legacy flags, Engage replacement, asteroid scanning and real shuttle flights in both directions. The previous immediate-activation test now asserts the selected mode rather than requiring continuous mode for Complete Cycle.

**Fresh canonical headless Mac validation passed 329/329 cases in one run**, strict import, source startup smoke and Windows cross-export. All nine Python tests pass. The export is 149,176,656 bytes, SHA-256 `f77f95a794d5096e62fee66772257db3aca56c60c4680435956f749031455143`; its embedded pack has 1,103 entries and no test resources.

**Separate native Mac validation failed.** Case 314 passed and exited cleanly. Case 315 passed gameplay assertions but emitted `FATAL: Condition "!rc_owner" is true` in Godot's `_instance_binding_reference_callback` during shutdown, then hit the 60-second timeout. The native batch stopped; planned cases 319/320/327/328/329 were not run. macOS crash report `Godot-2026-10-02-091514.ips` records a main-thread `EXC_BREAKPOINT/SIGTRAP`, followed by the validator's timeout kill. A debugger probe was denied attachment by macOS, so the exact retained-resource cause is unresolved. This failure is not exempted or replaced by the green headless aggregate.

Evidence, including the earlier logs/export, is retained under ignored `artifacts/validation/evidence/acc-cycle/`. This patch changes no shutdown code. Native Windows still has the separately verified **313-case `3004d9b` baseline**; the new 329-case revision and physical source/export acceptance remain pending. The task-level implementation count remains **31/48**, with **0/48 fully accepted**.


## Windows Complete Cycle baseline — 2026-10-02

Revision **`d057b9d`** passed a fresh native Windows aggregate: **329/329 isolated cases**, strict import, source startup and release export, plus all nine Python tests. A separate packaged-game smoke check passed; the pack has 1,103 entries and no test resources. The executable is 149,116,208 bytes, SHA-256 `13a279e225f5e9f8847e0ac526de573ee99c2f3f90cd5a0acd35e0f295c8f7cd`. The isolated checkout has no normalized content differences after import; 344 line-ending-only sidecars remain preserved.

This includes the Complete Cycle correction and Windows headless case 315. It does not resolve the separate native Mac teardown failure or prove physical controls/audio/window closing. See [the current Windows report](windows-validation-results.md). Earlier 313-case success and the missing-RCEdit failure remain archived. Counts remain **31/48 task-level implementations, 0/48 fully accepted**; no PR, push or Asana changes were made.

## Marine rank display — 2026-10-02

During Warlord investigation, the ordinary rank display was found to disagree with simulation/news: it showed Admiral from 30 actions, although actual promotion occurs at 40. New cases **330–331** reproduced the premature label directly and after save/load. Marine text now derives from `GetLevel`, so the label and effective rank share one threshold.

Cases **330–332** pass after the fix, covering all marine boundaries, save/load, the single promotion report at action 40, and unchanged researcher/production/Artisan labels. Existing save-graph case **45** and qualified-pilot deployment case **133** also pass. Compilation passed; this focused follow-up has **not** been represented as a full 332-case aggregate or new Windows export. The last full Mac/Windows baseline remains **329 cases at `d057b9d`**, and the earlier native Mac shutdown failure remains open.

Evidence is under ignored `artifacts/validation/evidence/staff-rank/`. This corrects an additional review finding; it does **not** implement Warlord or change the **31/48 implemented, 0/48 fully accepted** task count. The [travel investigation](original-interstellar-travel-evidence.md) explains the remaining Hyperlight/rank dependency.


## Research-only recipe safety — 2026-10-02

Completing Hyperlight research could expose a nonexistent manufacturing recipe in Stores and Production. The ground/orbital Stores path also restored a stale saved Hyperlight selection and threw `ArgumentNullException`. Recipe controls now require actual build requirements, Hyperlight is marked as non-manufacturable, and the shared resource check rejects a missing recipe. Legacy saves retaining the old manufacturing flag remain safe.

Cases **333–337** cover both inventories after save/load, invalid production selection, clicking every offered researched recipe, and actual MeH/HeD automatic refining. The initial broader resource guard incorrectly rejected automatic fuel recipes because they are not manually producible; case 337 reproduced that failure. The corrected guard checks for a recipe and preserves automatic fuel output and exact material consumption. Existing production/stock cases 6 and 48–53 pass too.

**Fresh canonical Mac validation passed 337/337 isolated cases**, strict import, startup smoke and Windows cross-export. All nine Python tests passed; compilation has 14 existing warnings and zero errors. This run also covers the committed marine-rank correction (330–332). The export contains 1,103 pack entries and no test resources; hash/size are retained with the raw evidence under ignored `artifacts/validation/evidence/research-only-recipes/`. Original failures, the fuel-regression failure, focused passes and previous aggregate/export are preserved there. No gameplay error exemptions were added.

This is a concrete Stores correction, not acceptance of the entire underspecified Stores feature. Counts remain **31/48 task-level implementations, 0/48 fully accepted**. Native Windows remains at the independently verified **329-case `d057b9d` baseline**; these newer changes require Windows verification. The native Mac case-315 shutdown failure remains unresolved.


## Accepted trade fuel gift — 2026-10-02

The [original acceptance instruction](original-trade-evidence.md) fills the ship's fuel gauge to 250. Its field identity is now independently established by the manual fuel routines. The remake omitted that gift; strengthened cases **220 and 233** reproduced both ordinary and interrupted-response failures (expected 250, actual 100).

Acceptance now settles fuel alongside cargo and the counter before response playback. Refused, ineligible, abandoned or stale offers cannot give fuel. All **16 trade cases (220–235)** pass after correction; compilation retains 14 existing warnings and zero errors. Raw before/after evidence is under ignored `artifacts/validation/evidence/trade-refuel/`. This is a focused follow-up after the full 337-case Mac run at `f148677`, not a replacement aggregate or freshly tested Windows export. Native Windows currently remains at `d057b9d` (329 cases). Counts stay **31/48 implemented, 0/48 fully accepted**; original trade timing and physical acceptance remain open.


## Windows recipe, rank and trade follow-up — 2026-10-02

Revision **`d11ff3b`** passed a fresh native Windows canonical run: **337/337 isolated cases**, strict import, source startup and release export, plus all nine Python tests. This includes the latest accepted-trade fuel correction as well as marine-rank and recipe safety checks. A separate packaged-game smoke check passed. The executable is 149,116,208 bytes, SHA-256 `9bc8710d289b79f1d56df03a22eef8ea539449e014d170850541608de24d01f4`, with 1,103 embedded pack entries and no test resources.

The new isolated checkout has no normalized content differences after import; 344 line-ending-only sidecars remain preserved. Logs and environment/export audit are collected under ignored `artifacts/windows-handoff/d11ff3b-evidence/`; earlier checkouts and failures are untouched. See [the current Windows report](windows-validation-results.md). Physical controls, audio, native window closing and normal progression remain acceptance work; the separate native Mac shutdown failure is unresolved. Counts remain **31/48 with implementation evidence, 0/48 fully accepted**.


## SDM local installation — 2026-10-02

Task **1215685674676229** now has the [original-backed manufacturing behavior](original-self-destruct-evidence.md#manufacturing-installs-the-mechanism): paid orbital production installs an SDM locally instead of creating stock. Completion stops AOC repeat and locks the recipe control; installed hardware rejects duplicate orders, ground factories cannot queue it, and unavailable stations suspend paid construction. Existing MTX installations and pre-fix SDM stock remain intact. The existing saved `SdmInstalled` field is reused. Arming, defusing/capture and destruction are still separate unfinished work.

Cases **338–347** cover manual/AOC controls, local scope, one recipe charge, save/load, stale repeats, ground requests and interrupted construction. Eight behavior failures were reproduced before correction. Initial UI fixtures had two items with research order zero; that fixture error was corrected before counting the UI behavior failures. All ten new cases, MTX cases 174–190 and related manufacturing/fuel checks pass. Both fixture and gameplay failures remain preserved under ignored `artifacts/validation/evidence/sdm-installation/`.

**Fresh full Mac validation passed 347/347 isolated cases**, strict import, source startup and Windows cross-export. All nine Python tests passed. Compilation has 14 existing warnings and zero errors; the export contains 1,103 pack entries and no test resources. Previous logs/export were archived first. Native Windows remains at the verified **337-case `d11ff3b` baseline**, and physical acceptance remains pending. The separate native Mac shutdown failure is under a new controlled investigation, not declared fixed by this headless run. Counts remain **31/48 with implementation evidence, 0/48 fully accepted**.


## Parsed input shutdown correction — 2026-10-02

A [controlled investigation](shutdown-input-evidence.md) reproduces the pinned engine's debug input-cache lifetime failure. New cases **348–349** reach the actual window-close path and print passing gameplay markers, then fail with `!rc_owner` and a 25-second timeout before correction. Explicitly disposing the C# event does not prevent the failure.

After scene/audio cleanup, debug-build shutdown now disables input delivery and replaces the retained event using GDScript, keeping its replacement outside C# bindings. Both new cases and headless 276/315 pass. Native Mac 348/349, case 315 three consecutive times, 319/320/327/328/329 and SDM cases 338/339 all exit cleanly. Raw failures and controls remain under ignored `artifacts/research/input-shutdown/` and `artifacts/validation/evidence/input-shutdown/`. The historical case-315 failure is preserved; its exact object was not identified by a native stack, so the diagnosis is supported by controlled reproduction and matching behavior rather than symbolication.

**Fresh full validation at `8cdd458` passes on both Mac and native Windows: 349/349 isolated cases, strict import, source startup and Windows release export.** Nine Python tests pass on each platform. The incremental Mac build reports no warnings/errors; the fresh Windows compile retains 14 existing warnings and zero errors. A separate packaged Windows startup passes. Both exports contain 1,104 entries, including the shutdown helper, and no test resources.

The Mac cross-export is 149,179,024 bytes, SHA-256 `4ba90dae4e887e582b0786cdfb466de39afa90a5d33b82dcd580ea1762318b7a`. The native Windows export is 149,118,592 bytes, SHA-256 `92106ed3bc9242ddad9bc19d068969838bcda7cf3a8cec9847b66d709c777b33`. Evidence is preserved under ignored `artifacts/validation/evidence/input-shutdown/` and `artifacts/windows-handoff/8cdd458-evidence/`; the earlier 337-case desktop checkout is untouched. Windows import again leaves 344 line-ending-only sidecars with no normalized content differences.

Physical desktop and normal campaign acceptance remain pending; **31/48 tasks have implementation evidence, 0/48 are fully accepted**. The SDM task remains partial because installation does not implement arming/defusing/destruction.

## Staff attrition — 2026-10-02

Task **1215716464570927** now runs the [traced original countdown/RNG kernel](original-behavior-evidence.md#1215716464570927--staff-attrition) for the remake's human staff. Each crossed 100-day boundary decrements the saved countdown; zero permits a one-member coin-flip loss and a 0–15 rearm. A one-to-zero transition makes no random draws. Teams can reach zero; cryopods freeze the retained countdown, and transferring a team resumes it. Ground/orbital rosters, both factories, researchers and pilots are visited once; synthetic enemy crews are excluded. The simulation event runs after current training/research/production and before display refresh.

New cases **350–359** all pass focused checks: boundaries and multiple gates, exact random-call behavior and floor, all staff locations, transfer/save persistence, older saves, event registration, replenishment and empty workforces. Seven behavior failures plus the older-save contract rejection were observed before implementation. Training and zero-workforce checks passed beforehand and protect existing behavior. **Fresh full validation at `55316b6` passes 359/359 isolated cases on Mac and native Windows**, strict import, source startup and Windows export. All nine Python tests pass on each platform; incremental Mac build reports zero warnings/errors, fresh Windows build retains 14 existing warnings and zero errors. Packaged Windows startup passes separately. Both exports contain 1,105 pack entries and no test resources.

The Mac cross-export is 149,181,392 bytes, SHA-256 `47433643b9996e4f01270ae34408750f3ed48a0424301e413afe9938d7831513`. Native Windows is 149,120,976 bytes, SHA-256 `ed7ffde6954cbb62d77bbe21787206e0e50fa7bf38c6adfa3174ca1f3d6a3063`. The Mac aggregate used the isolated launcher described below. Raw evidence is under ignored `artifacts/validation/evidence/staff-attrition/` and `artifacts/windows-handoff/55316b6-evidence/`; older runs and desktop checkouts are preserved. Windows import retains 344 line-ending-only sidecars and no normalized content differences.

The first test registration accidentally invoked the new group from `StartCase`, causing recursion; this harness error was corrected before counting behavior failures. After those crashes, a native process sample located the subsequent startup stall in AppKit's window-recovery modal, before engine logging. Focused checks use the per-process `-ApplePersistenceIgnoreState YES` launch argument after Godot's `--`. [Apple documents this option for isolated automated testing](https://developer.apple.com/library/archive/releasenotes/AppKit/RN-AppKitOlderNotes/). It preserves existing restorable state; no global preference or gameplay error filter was changed. Original errors, modal sample and corrected runs remain under ignored `artifacts/validation/evidence/staff-attrition/`.

Current qualified ranks are 1–3; original stage-7 identity and future story ranks are not inferred. No original RNG-sequence or fractional/interstellar-clock fidelity is claimed. Desktop/campaign acceptance remains pending. The local implementation count is now **32/48**, with **0/48 fully accepted**.


## SDM controls, capture and expiry — 2026-10-02

Task **1215685674676229** now has two-switch controls, research discovery, the Hyperlight interlock, defusal-based capture, independent real-time and simulation expiry, station/berth/shuttle losses, scene cleanup and saved timer phase. See [implementation](self-destruct-implementation.md) for behavior and [original evidence](original-self-destruct-evidence.md) for traced rules. Cases **360–374** pass focused checks; fourteen reproduced missing behavior before implementation, while case 373 characterized the cleanup already added with earlier expiry cases. Native Mac case 361 passes and its final screenshot was reviewed.

**Fresh canonical validation at `862e345dd56931b90781b4ffc04c9a105a425c2e` passes 374/374 isolated cases on Mac and native Windows**, strict asset import, source startup and Windows export. Nine Python tooling tests pass on each platform. Native Windows packaged startup passes separately. Mac incremental compilation reports zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. Both exports contain 1,108 pack entries and no test resources.

The Mac cross-export is 149,197,840 bytes, SHA-256 `c85e9aca3f0f0e68fd6071db77ac3f5531cafa249a06a11de0dae94b680f2da3`. Native Windows is 149,137,456 bytes, SHA-256 `467539e789238c79acb46d4f9cf5a46915330c6276d39bc1cd21f880031f2a44`. Evidence is under ignored `artifacts/validation/evidence/self-destruct/` and `artifacts/windows-handoff/862e345-evidence/`. Windows retains 344 line-ending-only import sidecars, with no normalized content differences.

Earlier attempts are preserved: one Mac editor timer error after the import marker, then whole-save comparisons in preset case 119 and pilot-warning case 129 on both platforms. A structured Mac comparison found only the newly advancing timer phase changed; focused case 134 reproduced the same assumption. Those fixtures now freeze frame processing narrowly and restore it afterwards, retaining whole-save equality. Focused preset 119–122 and pilot 129–134 checks pass, followed by the fresh full runs above. The intermittent editor timer cause remains unknown; no diagnostic was suppressed to obtain a pass.

Original alarm playback, observed timer cadence and the ground-team lifecycle remain incomplete. The new alarm trace identifies sample and channel descriptors but is not a listening test. Desktop testing at `8cdd458` remains separate; no new handoff replaces work in progress. Counts remain **32/48 with implementation evidence and 0/48 fully accepted**.


## SDM colony crew loss — 2026-10-02

The [original local roster trace](original-self-destruct-evidence.md#local-crew-ownership) resolves the ground-crew gap: non-Earth ground/orbital views use one local roster, whereas Earth ground has its own array. The remake retained colony ground teams after SDM loss. The shared destruction handler now clears that array with the existing `RemoveAllStaff` helper, preserving Earth ground crews, other colonies and crews on surviving vessels.

Case **375** failed before the correction and now passes real-time and simulation expiry, Earth/non-Earth locations, travelling pilots/cryopods, other colonies and save/reload. Focused neighboring loss/capture cases 362, 363, 366, 367 and 374 also pass. **Fresh canonical Mac and Windows validation at `853986c7064aaecbef431da7810599c7e2418d76` passes 375/375**, strict import, source startup and Windows export. Nine Python tests pass on each platform; Windows packaged startup passes separately. Build diagnostics remain zero warnings/errors on incremental Mac and 14 existing warnings/zero errors on fresh Windows.

Mac cross-export: 149,197,856 bytes, SHA-256 `fca45d7729e42f3b6bf290fe679828630e2e738b7c7583ca0d6db7fb38ae84fc`. Native Windows: 149,137,456 bytes, SHA-256 `07ebde47221f7c85b43ad879d113b88ebd4fbbfeec3f1c3f3ff594ce559e0352`. Both contain 1,108 pack entries and no test resources. Windows retains 344 line-ending-only sidecars with no normalized content changes. Logs are retained under ignored `artifacts/validation/evidence/self-destruct/full-run-853986c/` and `artifacts/windows-handoff/853986c-evidence/`; the new failed reproduction is in `ground-crews-red/`.

Original alarm playback and runtime timing/casualty comparisons remain pending. The existing `8cdd458` desktop handoff stays unchanged. Counts remain **32/48 with implementation evidence and 0/48 fully accepted**.

## Recovered SDM alarm — 2026-10-02

Revision **`6ac93012a70d5fa2c4e7c047430500c8a7069e7d`** restores the original waveform with calculated PAL playback, delayed right channel, looping and normal-sound priority. It follows the viewed armed station and stops on defusal, expiry or tree exit. Godot's Game bus supplies priority while Master retains user preferences. [Source/conversion evidence](original-self-destruct-evidence.md#alarm-source-and-playback-limits) distinguishes recovered bytes from unmeasured original-runtime behavior.

Cases **376–379** first reproduced the missing player and missing priority; navigation testing also exposed and corrected reliance on the transient `ShipSelected` GUID. All four pass headless and native Mac, alongside affected ambience/menu cases 200–205 and 212–219. Mixer capture detects competing audio leakage and measures right-channel onset at **0.3202 seconds** on both platforms.

Fresh canonical Mac and Windows runs pass **379/379**, strict import, source startup and Windows release export. Nine Python tests pass each; native Windows packaged startup passes separately. Mac incremental build has zero warnings/errors; fresh Windows has 14 existing warnings and zero errors. Both exports contain **1,111 resources and no tests**, including the alarm and default bus layout.

Mac cross-export: 149,205,712 bytes, SHA-256 `9453a7db2745469aab4f7da452aec5270f78301eabe9f3aa35ccffc43e023536`. Windows export: 149,145,328 bytes, SHA-256 `28eb66c617f25fd33951412acf1e7fd6699221c430ef5ecd61f99a2ef779f062`. Windows retains 345 import-sidecar line-ending warnings with no normalized content difference. Logs are preserved under ignored `artifacts/validation/evidence/sdm-alarm/full-run-6ac9301/` and `artifacts/windows-handoff/6ac9301-evidence/`; focused failures/passes remain beside the Mac full-run directory.

Counts remain **32/48 with implementation evidence and 0/48 fully accepted**. Original listening, descriptor runtime changes and timing/casualty comparisons remain open. Existing desktop testing at `8cdd458` stays separate. The [planet palette follow-up](original-planet-palette-evidence.md) also maps all 160 original bodies to 44 parent groups and documents PNG colour discrepancies; it makes no runtime artwork change.

## Orbital planet colours and location views — 2026-10-02

Task **1215685674676259** now has [original palette/bitmap evidence](original-planet-palette-evidence.md) and a correction for the blank enlarged orbital view. The supplied Asana reference exactly matches the repository image. `ShipInterior` uses that artwork, converts only the three palette colours, inherits a moon's parent palette and keeps station presence local. Cached converted textures leave source images intact. Docked/travel images retain their own colours; launching uses the large storm-door image.

Case **380** reproduced the null texture before the correction. It checks complete image pixels for nine palette classes and a moon, each with/without a station. Case **381** covers preview inheritance, cache reuse, save state and Shuttle/IOS/SCG transitions. The old neutral-placeholder assertion in arrival case 103 now verifies the Moon's recovered Earth colour. An early check exposed shared headless images; copying before conversion/disposal corrected that ownership issue. Native comparison initially omitted the cockpit foreground; its expected composition now includes all three foreground layers, preserving every sampled pixel check. Failures are retained alongside passing evidence.

Native Mac cases 380/381 pass with clean exits. Two reviewed captures validate **48,960 rendered pixels**, including the foreground. All 44 parent-group palette assignments were compared with the decoded source table.

Fresh full Mac and Windows validation at **`3871a6f94a0239e2c2a4fd38f0da60e19249b713`** passes **381/381**, strict import, source startup and Windows export. Nine Python tests pass each; Windows packaged startup passes separately. Incremental Mac build has zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. Both packs contain **1,111 resources and no tests**. Windows has 345 line-ending-only import notices and no normalized content changes.

Mac cross-export: 149,208,848 bytes, SHA-256 `c92c9dfbe90396ea6e83360d394a6f82529da64e872a79f426d3f675b0be3ff7`. Windows export: 149,148,496 bytes, SHA-256 `88754f1bc93d214aa39167e5c5fc640b2a88eb841ab8e19b9a25771d29e66fa2`. Logs: ignored `artifacts/validation/evidence/planet-view/full-run-3871a6f/` and `artifacts/windows-handoff/3871a6f-evidence/`.

Counts are now **33/48 with implementation evidence and 0/48 fully accepted**. Original emulator comparison, Windows desktop/campaign checks and separate map-art discrepancies remain pending; the small asteroid preview retains its previous neutral placeholder. The existing desktop handoff at `8cdd458` remains unchanged. [Production rod evidence](original-production-rod-evidence.md) also identifies its three exact supplied frames, order, placement and active-product timing gate; that animation is not implemented by this commit.


## Original production rod animation — 2026-10-02

Task **1215691951441144** is implemented at **`ca54582ff13825a78868dc8d33277f7d52797a2c`**. The separate rod now uses three exact source-sheet atlas regions at x240/y96. Its nominal 50 Hz display counter reproduces the original short initial frame, even-tick draws and repeating 130→129→128 sequence. It follows the selected factory's active product, pauses with the scene tree and retains its counter across idle/redraw/navigation without changing production state. See [direct evidence](original-production-rod-evidence.md).

Case 382 reproduced the missing node before the correction. Cases **382–385** then passed headless and native Mac for ground/orbit × manual/AOC, including exact decoded frame hashes, half-tick accumulation, fast-forward independence, shortages, idle/completion/cancellation, pause/resume, factory isolation, navigation and unchanged serialized factory state. Normal Godot frame processing also advances the rod. Native checks compare **3,456 rendered pixels**; screenshots were retained and a ground-manual view inspected. Construction-stage pictures remain separate.

Fresh full Mac and native Windows validation each passes **385/385**, strict import, source startup and Windows release export; all nine Python tests pass on each machine. Windows packaged startup passes. Mac's incremental build has zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. Both packs contain **1,113 entries and no test resources**. Windows import leaves 346 known LF/CRLF notices and no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,274,880 | `899b99a23d7f8372480807c9821c4dd09f0644c0ae1ac045b04b5fbb81b87e77` |
| Windows | 149,214,528 | `edde06de1c9bb4d05f144518214edcc7d5901286a939bcd2c6f2ae4f9ae50127` |

Evidence is retained under `artifacts/validation/evidence/production-rod/` and `artifacts/windows-handoff/ca54582-evidence/`. The verified source bundle SHA-256 is `58d3c39c52a5a05f90a024893db8ee9f849e5d1f427f955d2a9f084d7c0c0fee`. Both aggregate processes and the Windows collector exited zero; all collected case logs, source/export smoke and package contents were audited locally.

Counts are now **34/48 with implementation evidence and 0/48 fully accepted**. Original-emulator wall-clock comparison and Windows desktop acceptance remain outstanding. The human desktop handoff at `8cdd458` remains unchanged. Independent [construction-bank research](original-construction-artwork-evidence.md) also recovered 21 frames for seven remaining items, but those are not runtime changes or additional completed tasks.


## Recovered construction and research artwork — 2026-10-02

Revision **`5053982b913af9eb8b07ee4da91a4fe09587180a`** restores 21 genuine construction frames for seven items, six original blank SDM/MTX stages, and five missing small research illustrations. One atlas and the existing image-loading paths supply the stage art; no production simulation code changed. The committed recovery script verifies both original disk hashes and reproduces all 33 generated assets with `--check`. [Source evidence](original-construction-artwork-evidence.md) records addresses, masks, colour choices and remaining fidelity limits.

Cases **386–403** cover paid manual/AOC production, all three stages, resource charges, completion/idle and local SDM/MTX installation without extra stock. Native Mac checks pass **262,656 pixel comparisons**, including transparent research backgrounds and opaque black details; reviewed captures include Pulse Blast Laser, Star Drone and SDM. Cases 386/400 preserve pre-fix missing-image and incorrect-static-fallback failures. The retained MFL fallback is not presented as an original construction sequence.

Full Mac and native Windows validation each passes **403/403**, strict import, source startup and Windows export. All nine Python tests pass on each host; Windows packaged startup passes separately. Incremental Mac compilation has zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. Both packs contain **1,152 entries and no tests**. Windows has 352 import-sidecar line-ending notices and no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,293,616 | `959c90a22e111c13f4cf278df4c2d7c754b7705962e28f1b3b40a3d7e3a3dfdc` |
| Windows | 149,233,264 | `58538b21645386451f2d12a08c6ea7da411080af4e0ff5019c078ced82656d63` |

All aggregates and the Windows collector exited zero. Case logs, import/build/smoke/export and package contents were audited locally. Evidence is under `artifacts/validation/evidence/construction-recovered/` and `artifacts/windows-handoff/5053982-evidence/`; source bundle SHA-256 is `cb10bcd9b27be8704c2ede5709e8bfd6c51405121eab0bfbe6e9e6fb636286e1`.

Counts remain **34/48 with implementation evidence and 0/48 fully accepted**: this advances the existing animation-inventory task. Windows desktop and original timing/colour comparison remain outstanding, as does review of oversized existing research diagrams. The desktop handoff at `8cdd458` is unchanged.

Two follow-ups were identified during the caller audit. **Confirmed in the Windows package:** dynamic `bandaid.png` is absent while tracked `Bandaid.png` exists; the controlled external probe exited zero and confirmed both values. This was outside the startup smoke's coverage at `5053982`. **Static finding at that revision:** `Research.DrawData` calls `BuildRequirements.Select` for completed Hyperlight despite its null recipe, reachable through button selection and a saved current-research selection. The prior recipe tests covered Stores/Production, so Research required its own regression and guard. Both findings are corrected and verified in the following batch; neither adds a completed Asana task.


## Research details and packaged Bandaid artwork — 2026-10-02

Revision **`d0d15f7dad243fe1ba983c2c0fa7efa89ffb0793`** fixes completed Hyperlight rendering in the shared Research detail function. A missing recipe now shows “Research complete” and leaves manufacturing fields empty, including legacy saves retaining the old production flag. Real products still restore their recipe and mass. The Bandaid PNG/import sidecar now match the lowercase resource path used by Research and Production; image bytes and UID are preserved.

Cases **404–405** reproduced `ArgumentNullException` before the guard through normal research completion and restored current selection. Both then passed headless and native Mac, with reviewed screenshots, switching between Hyperlight and physical products, and all five recovered illustrations loading. The earlier wrong scene-enum build failure and headless screenshot-environment failure are retained as test setup errors, separate from the valid game reproductions.

The strengthened external smoke driver failed against the old `5053982` Windows executable with exit 1 when loading the canonical Bandaid resource. It passes against the new executable with exit 0. Full Mac and native Windows validation each passes **405/405**, strict import, source smoke and Windows export; all nine Python tests pass on each host. Mac incremental compilation has zero warnings/errors; fresh Windows has 14 existing warnings and zero errors. Both packs have **1,152 entries and no test resources**. Windows retains 352 import-sidecar LF/CRLF notices with no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,293,632 | `8d9191b2c9f84de6ae9371c0992d6c68b58ece2e051a17a7f0c53bdb46d328f8` |
| Windows | 149,233,280 | `b823a475c49a3a59e9f4667beefa0a175f056cf68859ff1e050315e717190d9d` |

Both aggregate processes and the Windows collector exited zero. All collected regression/build/import/smoke/export logs were audited locally. Evidence is under `artifacts/validation/evidence/research-details/` and `artifacts/windows-handoff/d0d15f7-evidence/`; verified source bundle SHA-256 is `a232d7b543f18baef835450d1eb8ecec5b23b5a2086e3c25adfab8cdc508a55b`.

Counts remain **34/48 with implementation evidence and 0/48 fully accepted**. These are additional review fixes. Windows desktop interaction, original colour/timing comparison and oversized-diagram replacement remain pending. The existing `8cdd458` human handoff is unchanged.


## Original item illustration recovery — 2026-10-02

Revision **`7c7db5e4550f432405acd62c45da0ba90608df30`** restores all 32 original item images, replacing seven oversized diagrams and adding the missing alien-artifact path. Research uses an opaque original-size bitmap at x208/y68; Production uses a separately masked 48×46 canvas at x136/y54. Native unscaled drawing preserves the SCG's complete 45th row. [Source evidence](original-construction-artwork-evidence.md#complete-item-illustrations-and-original-placement) explains why these screens cannot share transparency rules. The existing recovery script reproduces all 92 generated assets with `--check`.

Case **406** reproduced a 183×177 placeholder before correction. It now validates all 32 indexed source hashes, opaque/masked correspondence, bounds and origins. Native Mac compares **113,952 pixels across 60 rendered views**; SCG screenshots were reviewed. Native compatibility cases 190/206/386/389/400/404/405 also pass. Intermediate test setup failures involving transparent-pixel RGB and duplicate staged research orders are retained separately. An initially passing seven-image approach was expanded after tracing the original Research opaque blit; it is not the final implementation.

Full Mac and native Windows validation each passes **406/406**, strict import, source smoke and Windows export; nine Python tests pass on each machine. Windows packaged smoke loads Bandaid, alien-artifact and the SCG Production illustration successfully. Both packs contain **1,218 entries, all 64 illustration resources and no tests**. Mac incremental compilation has zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. Windows import has 385 line-ending notices with no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,318,784 | `2c26ed42ed733c989191ddda37d890b0629fb3e1f2625d95b956539ec11d72ce` |
| Windows | 149,258,432 | `26551d145cddb7c60843e0cbe04eb5c63eea4b901246452f25edbdd9fcdf58d5` |

Both aggregate processes and the collector exited zero; all downloaded logs were audited locally. Evidence: `artifacts/validation/evidence/research-diagrams/` and `artifacts/windows-handoff/7c7db5e-evidence/`. Verified bundle SHA-256: `186d595dc05ba9e8ef557b44119b85a043ad742efa07fc197ad3171321f76f49`.

Counts remain **34/48 with implementation evidence and 0/48 fully accepted**. This advances the existing media task. Original colour calibration, construction timing, MFL static-fallback fidelity and Windows desktop acceptance remain pending. The stable `8cdd458` desktop handoff is unchanged.


## SCG item names in new and legacy saves — 2026-10-02

Revision **`b3936c2668c430af5eec421d5e170dde2b4c464b`** supplies the missing SCG Chassis and SCG Drive short names. Save loading fills only blank names from canonical definitions, preserving existing labels, research and stock. This corrects blank Production headings, a misleading “None” Station factory summary, and unnamed Stores/MTX rows without a save-schema change.

Cases **407–408** reproduced the blank headings for new and legacy saves, then passed headless and natively on Mac after the four-line runtime correction. Native screenshots were reviewed; existing save cases 45/46/47/67/68/69 also pass. Test setup clones the initial save before changing names because new-game data shares the canonical definitions. The initial test-registration compile error is retained separately from the valid failing regressions.

Full Mac and native Windows validation each passes **408/408**, strict import, source smoke and Windows export; nine Python tests pass on each host. Windows packaged startup passes, including the canonical illustration loads. Both packs retain **1,218 entries, all 64 illustration resources and no tests**. Mac incremental compilation reports zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. All 385 Windows import notices concern line endings, with no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,319,424 | `07330138a9fd6f2b0d2e77f9ca332893c49decdd9616456c90f688708a5b3ac8` |
| Windows | 149,259,072 | `00d9f6243e79d3e27036575650a4c1561db2544193369c8608ad69613f1d2616` |

Both aggregate processes and the Windows collector exited zero; all downloaded logs were audited locally. Evidence is preserved under `artifacts/validation/evidence/scg-names/` and `artifacts/windows-handoff/b3936c2-evidence/`. Bundle SHA-256: `9453d21ee0384ab00a914103e97fc5895c5474b0551db5166ec0bc5c63f4a560`.

Counts remain **34/48 with implementation evidence and 0/48 fully accepted**: this is an additional review fix. Windows desktop checks remain pending, and the stable `8cdd458` human handoff is unchanged.


## Module dialogue colour correction — 2026-10-02

Revision **`a54e899d4c6796e6f406733112d39a7ba424e7d0`** fixes `Line.GetText`, the shared formatter used by module dialogue. Casting normalized Godot RGB channels directly to integers turned the intended `#aaccee` Methanoid text into `000000`. The formatter now uses the existing `Color.ToHtml(false)` API, with a valid BBCode hex prefix. White text and dynamic substitutions remain intact.

Case **409** reproduced the black markup through the real `ModuleTextFrame.PlayText` path. It now passes headless and with native Mac rendering, checking visible parsed text, formatting, pod number and five additional palette colours. The native screenshot was reviewed; trade/cancellation compatibility cases 220/225/231/233 pass. Evidence is under `artifacts/validation/evidence/dialogue-colours/`.

Full Mac and native Windows validation each passes **409/409**, strict import, source smoke and Windows export; nine Python tests pass on each host. Windows packaged startup also passes. Both packs retain **1,218 entries, all 64 illustration resources and no tests**. Mac incremental compilation reports zero warnings/errors; fresh Windows retains 14 existing warnings and zero errors. All 385 Windows import notices concern line endings, with no normalized content differences.

| Export host | Executable bytes | SHA-256 |
| --- | ---: | --- |
| Mac | 149,319,424 | `9350b4b9ff2546b5b409ae1234a6e9e9a10786f3e1700152220a582446a46101` |
| Windows | 149,259,072 | `6bc8b3c31a15dd881bb757b52b675dcc08e8101b9ebc37943773cfbed0619da3` |

Both aggregate processes and the Windows collector exited zero; all downloaded logs were audited locally. Evidence: `artifacts/validation/evidence/dialogue-colours/` and `artifacts/windows-handoff/a54e899-evidence/`. Bundle SHA-256: `769112021774e8687291ae6a75e71208e795641df62919300dd04a354bdc0ba0`.

Counts remain **34/48 with implementation evidence and 0/48 fully accepted**; alien transmission scheduling and desktop acceptance remain open. The stable `8cdd458` human handoff is unchanged.

## Artifact recovery, manufacture and tool fitting — 2026-10-02

Runtime **`f5a9cbb994b2f904426c0744856405328f243a16`** includes direct recovery credit, legacy credit conversion, zero-material orbital manufacture and the original five/eleven/eleven tool lists for Shuttle/IOS/SCG. Both display and selection share the eligibility rule. The device fits one SCG slot; legacy incompatible equipment survives until a valid replacement returns it. [Original evidence](original-alien-message-evidence.md) records the verified behavior and remaining campaign work.

All **421 cases**, nine Python tests, strict import, source startup and Windows export pass on Mac. Compilation reports 14 existing warnings and zero errors. Every case log was audited and archived under `artifacts/validation/evidence/tool-fitting/full-run-f5a9cbb/`. The Mac-built executable is **149,321,200 bytes**, SHA-256 **`a6894f2f26bf6ac0356090d671e069a9f8a7740c3bfeae394985adfb884a68ef`**; its pack has **1,219 entries, all 64 illustrations and no tests**. Documentation-only HEAD `2856168` has no runtime/tooling differences from the tested commit.

The fresh Windows runner also exited zero after **421/421**, nine Python tests, strict import, source startup and export. Its aggregate log is `artifacts/windows-handoff/validate-f5a9cbb.log`. A subsequent SSH authentication refusal prevented the separate packaged smoke, hash audit and individual-log download; these are still pending. Bundle SHA-256 is `29380ed8eb0ff2d2f76d5d318d11f7eafbde5ce2ce679cea4c2565457fe58ee9`.

Native Mac cases 417–421 and reviewed screenshots cover visible last rows, legacy replacement and manufacture-to-fitting/save-load. This does not establish normal campaign or Windows desktop acceptance. The stable `8cdd458` human handoff remains unchanged; transmission scheduling, capture assignment, device activation and ending integration remain open. Counts remain **34/48 with implementation evidence, 0/48 fully accepted**.


## Capture-driven alien transmissions — 2026-10-02

Isolated branch `codex/alien-capture`, revision **`642ed00a61817b3a09a2eece2d0df69ad787337f`**, now connects both war declarations to the original saved transmission stages and eligible-update delays. The research notification waits for acknowledgement; interrupted notices retry without consuming progress. News replays the last actual alien message. Its decoding mask rotates during replay as in the original, while stage/countdown remain unchanged.

Final hostile-station capture assigns one artifact per non-Sun system, preserves assignment history and pending location order through saves, and produces the missing SCG chassis/drive/HED discovery. Eight real grapple/bay deliveries reach final instructions; final recovery clears obsolete notices. Old saves preserve artifacts/cargo, start unknown at-war message history at the introduction, or select final instructions if recovery is already complete. New fields are validated before world activation; old executables may reject newer saves.

Cases **422–435** cover capture, discovery, both war starts, stage boundaries, legacy/corrupt saves, competing messages, interruption, stale acknowledgements, queued reports, eight real recoveries and replay. Native Mac 427/430/433/435 pass. Screenshot review caught overlapping alien glyph rows; a reproduced layout failure now passes with eight-pixel rows, and corrected message-family screenshots were inspected.

**Fresh full validation passed 435/435**, nine Python tests, strict import, source startup and Windows cross-export. All individual case logs and package contents were audited. The export is **149,340,000 bytes**, SHA-256 **`bfe471920e3b6f064d1e25b6f23c23d3b19dd217504761263bb8bb5d96654da7`**, with **1,221 pack entries**, **64 illustration imports** and **zero test resources**. Compilation retains 14 existing warnings and zero errors. Evidence: `artifacts/validation/evidence/alien-transmissions/full-run-642ed00/`; controlled failures and native captures remain alongside it.

This revision has not run on Windows or joined the contribution branch. The `f5a9cbb` Windows runner still has 421 passes, but packaged-smoke/log collection remains blocked by SSH authentication. The stable human desktop handoff remains `8cdd458`. Warlord/Hyperlight clocks, transmitter activation and the ending remain open. Counts stay **34/48 with implementation evidence, 0/48 fully accepted**.


## News producers and short-history layout — 2026-10-02

Isolated `codex/news-events` revision **`9b245f52d977e93074ee2ed459d0d39ab68fda65`** builds on the transmission checkpoint. Ship attacks, fuel/hostile-orbit/battle losses, fleet station attacks/captures and successful dismantling now publish reports at their existing state transitions. The original [event inventory](original-news-evidence.md) supports these event families; this batch does not invent ordinary research/production completion reports or implement crew-loss/pirate-warning producers.

Cases **436/437** reproduce silent losses and station attacks. Existing **297/301** reproduce missing successful dismantling reports and retain rejection/conservation checks. A subsequent failing partial-history assertion exposed leading empty rows: newest reports now appear first for short and full histories. Native text truncation keeps long names in the panel while hover text retains the full report. Native 124/436/437 and inspected screenshots cover ordering and rendering; physical hover remains a desktop check.

**Fresh full Mac validation passed 437/437**, nine Python tests, strict import, source startup and Windows cross-export. Individual logs and package contents were audited. Export: **149,341,328 bytes**, SHA-256 **`57b54722c1cfbead9fbd5e3bd305fd3dbc8b733382b32ad5d5f98a6207e983b2`**; **1,221 entries**, **64 illustration imports**, **zero tests**. Build: 14 existing warnings, zero errors. Evidence: `artifacts/validation/evidence/news-events/full-run-9b245f5/`, with red/focused/native evidence alongside it.

This branch is still separate from the contribution branch and has not run on Windows. The subsequent battle callback review reproduced leaked windows and lost reserved drones on interruption; those follow-ups are being tested in `codex/battle-cleanup`, not claimed fixed by this 437-case checkpoint. Counts remain **34/48 with implementation evidence, 0/48 fully accepted**. The read-only evening Asana refresh confirmed the same 48 open IDs; no external task changes were made.


## Battle lifecycle and consolidated candidate — 2026-10-02

Candidate **`625cae94ef40f4552399781b8ba205e1593b5ec0`** includes capture/discovery, saved alien transmissions, News producers/layout and battle cleanup. **442/442** isolated Mac regressions, nine Python tests, strict import, source startup and Windows cross-export pass. All individual logs and package contents were audited. Export: **149,343,296 bytes**, SHA-256 **`040e4c75a8e169afb3e53b9c039959fc748d08d4ba7f1ece0a2b71d35fa8b64c`**; **1,221 pack entries**, **64 illustration imports**, **zero test resources**. This incremental build reports zero warnings and zero errors; it does not establish removal of the 14 warnings recorded by earlier fresh builds. Evidence: `artifacts/validation/evidence/battle-cleanup/full-run-625cae9/`.

Cases **438–442** reproduce and fix leaked battle windows, lost reserved station drones and a defeated ship surviving interruption during result presentation. Completed results settle once, interrupted encounters preserve calculated casualties, and old encounters cannot mutate replacement saves or redirect newer screens. Focused native Mac checks pass; see [battle details](battle-lifecycle.md) for the staged-fixture limits and desktop scenarios.

These runtime changes are now consolidated into the local contribution branch. Its `Godot/`, `scripts/` and `.github/` trees match the validated candidate; merge resolutions only retain newer documentation. This is a local merge, with no push or PR. Windows execution and interactive acceptance are pending; the stable `8cdd458` desktop handoff remains unchanged. Counts stay **34/48 with implementation evidence, 0/48 fully accepted**.

A complete local Git bundle is prepared at `artifacts/windows-handoff/deuteros-625cae9.bundle`, SHA-256 **`4a620311be107eca189cfb11816b09c50e02e4aaca7e816e44a41357ec322d8f`**. It is not uploaded: the latest SSH retry still rejects authentication. The separate `desktop-prompt-625cae9.txt` alongside it describes validation and report requirements once transferred.


## Asteroid generation and AMA amounts — 2026-10-02

The original scanner tables establish eight minerals (including Copper and Silica), eight visible classes and the existing mass/artwork groups. The remake generated only six minerals and six classes, excluding 28 of the 64 combinations. Original mining adds twelve to a five-bit random amount, giving 12–43; the remake used 16–35. Both ranges are corrected at their shared generation/mining methods. [Instruction and table evidence](original-ama-mining-evidence.md#generated-classes-and-minerals) records the mapping and RNG/cadence limits.

Case **443** reproduced 36 rather than 64 combinations, then 20 rather than 32 distinct amounts. It now passes headless and native Mac with seeded sampling, and mines generated class-7 Copper and class-8 Silica through the actual ship updater. Compatibility cases **305–313** pass. Build reports 14 existing warnings and zero errors. The earlier missing-log-directory and recursive-registration errors were test setup errors, corrected before counting those gameplay failures. Evidence: `artifacts/validation/evidence/ama-ranges/{red2,amount-red,green,final}/`.

Full **443-case Mac validation passed at `e68c6bb5f8390e89b54d8f0c46c97dd79b78edbe`**, along with nine Python tests, strict import, source startup and Windows cross-export. All individual logs and package contents are audited: **1,221 entries**, **64 illustration imports**, **zero tests**. Export: **149,342,016 bytes**, SHA-256 **`2d9280fb01071cd81b2cfc66710dc690edc115941a5d845efbd383b8ab25bcf5`**. This incremental build reports zero warnings/errors. Evidence: `artifacts/validation/evidence/ama-ranges/full-run-e68c6bb/`. Windows execution remains pending. Mining/scan timing still uses the provisional remake scheduler; restoring the ranges does not complete the AMA investigation or establish original campaign cadence. Counts remain 34/48 with implementation evidence and 0/48 fully accepted.


## Manual AMA fuel eligibility — 2026-10-02

Case **444** reproduced a zero-fuel ship entering `Docking` through the real AMA Mine control, contrary to original manual entry `$31810–$3182E`. A shared control/callback check now rejects zero fuel and undersized scans before changing the mining timestamp. Refuelling and a class-six-or-larger scan allow retry and close the module normally. The original automatic scan branch enters separately, so this correction does not change generic docking or automatic mining.

Focused **444/443/307/310–313** and native Mac **444** pass; build and strict worktree import pass. Evidence: `artifacts/validation/evidence/ama-controls/{red,green,setup}/`. These are staged control checks, not physical input or campaign acceptance. Full 444-case and Windows verification are not claimed. Original slot allocation/reuse is now traced in the [AMA evidence](original-ama-mining-evidence.md#ship-slot-identity-and-reuse); its saved-clock integration remains outstanding.


## Phased fuel refining and station allocation — 2026-10-02

Revision **`f913679bf513d2ca9b44ecc1b2820df133bf34f1`** corrects persistent starvation of the second eligible factory, reproduced first in case 445. Refining now runs once after ships, using five original ground/orbital recipes, their input/output thresholds, and a saved global phase. Station allocations follow the original initial ranges and construction/capture relocation; legacy saves receive stable allocations without serialization mutating the world. Existing larger remake worlds remain loadable. [Source evidence and compatibility boundaries](original-fuel-refining-evidence.md) distinguish these corrections from remaining fractional-clock work.

Cases **445–450**, revised **337**, and native **445/447–450** pass. Full Mac validation passes **450/450**, nine Python tooling tests, strict import, source startup and Windows cross-export. Every case log and the package were audited: **1,222 entries, 64 item illustrations, zero test resources**, executable **149,350,816 bytes**, SHA-256 `a1138cf30cdf5d083cd753d157ac89b41451342fc3206796bf5f3040a5ddcfed`. Evidence: `artifacts/validation/evidence/refining/full-run-f913679/`; the red starvation and focused/native logs remain alongside it. Fresh build retains 14 existing warnings; incremental aggregate build reports zero warnings/errors.

This candidate has not executed on Windows. The existing `8cdd458` desktop handoff stays separate. Event-order and original timing acceptance remain open; counts stay **34/48 with implementation evidence, 0/48 fully accepted**.


## Training, research and arrival order (452-case checkpoint)

Training now runs once before Earth and local extraction; research and crew attrition precede ship arrivals. Cases 451–452 reproduce premature ACC departure, mining before graduation and arrival before crew attrition. They verify simultaneous production/research completion, promotion order, frozen crew and the actual arrival path. Focused and native checks pass.

Full Mac validation at `08eda4dd068d0ee65dc60c3c5c26006d85c4cfd8` passes **452/452**, nine Python tests, strict import, startup and Windows cross-export. All case logs and package contents are audited: **1,222 entries, 64 illustrations, zero test resources**. Executable: **149,350,832 bytes**, SHA-256 `0ec3d36d073e77b460776ee55e87947ff2cb9189bd43a9d1e1c9ed6b4b1d66c4`. Evidence: `artifacts/validation/evidence/simulation-order/full-run-08eda4d/`.

Integrated locally; no push or PR. Windows execution, original fractional/star clocks and pending-event priority remain outstanding. Counts remain **34/48 with implementation evidence, 0/48 fully accepted**. The `8cdd458` desktop handoff is unchanged.


## Ground extraction and survey correction (458-case checkpoint)

Cases 453–455 reproduced mining beyond the available deposit, surveys blocked by zero derricks and hostile-base extraction. Shared mining now follows the [original whole-batch and survey rules](original-ground-mining-evidence.md), caps output at 50,000, and honors local ownership/base eligibility. Exact depletion and zero-delay surveys remain distinct through saves and on the ground-materials screen. Legacy negative veins are repaired without changing inventories; malformed mining state is rejected before activation.

Cases 453–458 and native 453/457/458 pass. Full Mac validation at `50c7dafff70f1b6cc7b52a660e44967b848982fb` passes **458/458**, nine Python checks, strict import, source startup and Windows cross-export. All case logs and package contents are audited: **1,222 entries, 64 illustrations, zero test resources**. Executable: **149,352,192 bytes**, SHA-256 `37304d1b39992ccc28a18d59ea4edb396d8034e26a962678a4df8c08603a7a4f`. Evidence: `artifacts/validation/evidence/ground-mining/full-run-50c7daf/`; the three initial failures and focused/native checks remain alongside it. Fresh compilation retains 14 existing warnings and zero errors; incremental aggregate compilation reports zero warnings/errors.

Integrated locally with runtime trees identical to the tested revision. No push or PR. Windows execution, original fractional clocks and exact RNG-sequence comparison remain outstanding. Counts remain **34/48 with implementation evidence, 0/48 fully accepted**; these mining failures are separate review findings. Preserve the existing `8cdd458` desktop handoff.

## Delayed Hyperlight discovery (464-case checkpoint)

Cases 459–461 reproduced missing discovery, lost progression after save/load and the missing enemy-count sampling boundary. Hyperlight now becomes available after seven hostile systems remain, the original eight decrement-only passes and pending-bulletin dispatch. Discovery does not finish research. Saved pending state survives competing alien notices and interruption; normal Research controls and qualified staff can complete the subject. Cases 459–464 pass, including malformed/legacy saves and recapture boundaries.

Full Mac validation at `fbffac5e79b7e550215e5465a5434f93636a5620` passes **464/464**, nine Python checks, strict import, source startup and Windows cross-export. Audited package: **1,222 entries, 64 illustration imports, zero tests**, **149,354,784 bytes**, SHA-256 `a321b23dc0bc1815df57d5e59d444ac0c7b23d79438c95e310a359e2a41d4249`. Incremental build: zero warnings/errors; fresh focused build: 14 existing warnings, zero errors. Evidence: `artifacts/validation/evidence/hyperlight-discovery/full-run-fbffac5/`.

Native Mac cases 459/460/461/462/464 pass and the Research screenshot was inspected. Actual desktop mouse/keyboard checks passed for startup, empty save slots, Settings tabs, IOS preset confirmation, right-click navigation from Stores and window closing (process exit 0). These used a new unsaved game, wrote no save slots and changed no persistent audio/display settings; details and logs are under the adjacent `native/` directory. An initial case-462 fixture incorrectly bypassed an unacknowledged alien notice; the corrected acknowledgement flow passed, and both logs are retained.

Integrated locally; no push or PR. Windows execution, full Hyperlight travel, Warlord promotion and original clock/campaign acceptance remain pending. Counts stay **34/48 with implementation evidence, 0/48 fully accepted**. Preserve the existing `8cdd458` Windows desktop handoff.

## Enemy production scheduling (466-case checkpoint)

The [original interval table](original-enemy-production-evidence.md) uses remaining hostile systems. Case 465 reproduced the remake indexing that table with an unused captured-system counter; nine systems scheduled day 107 instead of 108. Production now uses the shared sampled hostile count and the full ten-entry table. A due-or-overdue check also prevents a zero-system interval or crossed deadline from permanently stopping production. Case 466 reproduced the zero-interval stall in the initial correction. Both cases cover actual drone output, recapture, peace and saved deadlines.

Full Mac validation at `64412e0acd7cd0e53aae03f4a24cbfc79e8e8cf9` passes **466/466**, nine Python tests, strict import, source startup and Windows cross-export. Individual logs and package contents are audited: **1,222 entries, 64 illustrations, zero tests**; **149,354,800 bytes**, SHA-256 `092a7616463df9fcfca248844fd1145c758c58578ad299bcd2666a5b7e4b4a17`. Evidence: `artifacts/validation/evidence/enemy-production/full-run-64412e0/`. Incremental build has zero warnings/errors; the focused fresh compile retains 14 existing warnings. An initial misplaced test registration caused fixture recursion; that invalid run is retained separately from the gameplay reproductions.

Native cases 465/466 pass. Actual mouse/keyboard checks in a fresh unsaved game passed ship rename/confirm/reopen, Cancel preserving the committed name, disabled empty-pod discard, returning from Cargo and physical window closing with Rename open. The process exited zero with clean disposal; screenshots and logs are under the adjacent `native/` directory. Escape while editing removed focus but did not dismiss Rename; no Escape-dismissal pass is claimed. No save slots or persistent audio/display settings were changed.

Integrated locally with runtime trees identical to the tested revision; no push or PR. Whole-day truncation and initial scheduler timing remain provisional until original fractional clocks are integrated. Windows execution and campaign acceptance remain pending; preserve the stable `8cdd458` desktop handoff. Counts remain **34/48 with implementation evidence, 0/48 fully accepted**.

## Discovery countdown ordering (467-case checkpoint)

Original `$37810` consumes an active countdown before `$37820` checks changed hostile-system ownership. Case 467 reproduced the reverse order clearing a delay on recapture (expected 7 remaining, got 0). The corrected branch order preserves the delay across saves and transient ownership changes; the pass reaching zero only decrements, and the next pass checks ownership. Case 464's earlier recapture expectation was corrected against this source evidence. Other story delays and fractional clocks remain outside this fix.

Full Mac validation at `29eb9a7cc2be17b379200a575fb20c054caf5b89` passes **467/467**, nine Python tests, strict import, source startup and Windows cross-export. All individual logs and package contents are audited: **1,222 entries, 64 illustrations, zero tests**; **149,354,800 bytes**, SHA-256 `88839eb64497faea90e60fc016d4edd969e96aff3cfaaab71fd95ebae41cbf7e`. Evidence: `artifacts/validation/evidence/discovery-countdown/full-run-29eb9a7/`; failing reproduction, focused cases 459–467 and native 467 are retained alongside it. Integrated locally without a push or PR. Windows execution and campaign acceptance remain pending; totals stay **34/48 with implementation evidence, 0/48 fully accepted**.

## Training lighting and door feedback (469-case checkpoint)

Physical Mac training play completed three simultaneous queues, but the subsequent light-switch click logged an invalid `Node` to `Node2D` cast. Case 468 reproduces the exception. Correcting the scene containers also restores inherited control brightness; native pixel checks reproduced the incomplete outer-container-only correction before the full fix. Case 469 reproduces the loaded-but-unplayed door sample and verifies one cue per transition batch, opening/closing, unchanged-update silence, preserved button feedback and exit stop. Original six-step door timing and staggered/interrupted animation lock ownership remain follow-ups.

Full Mac validation at `34ab06731fe7740672d3df564fb03e9fb4e407f7` passes **469/469**, nine Python tests, strict import, source startup and Windows cross-export. Individual logs and package audited: **1,222 entries, 64 illustrations, zero tests**; **149,355,840 bytes**, SHA-256 `38571ebd0e2fd874dfe4444bd312bd88033bcbab45b6ffce8da169f5298ed20c`. Focused native 468/469 pass and the corrected screenshot was inspected. Evidence: `artifacts/validation/evidence/training-feedback/full-run-34ab067/`, with reproductions, native results and separately identified fixture setup failures alongside it. The initial physical failure is retained under `discovery-countdown/native/manual-training.log`; its clean process exit does not make that session a pass.

Integrated locally; no push or PR. Windows execution, listening and original timing acceptance remain pending. Totals stay **34/48 with implementation evidence, 0/48 fully accepted**.

## Training animation ownership (470-case checkpoint)

Case 470 first reproduced a static closed door releasing an unrelated input lock, then reproduced one completion prematurely settling another moving door. Static states no longer play a finishing animation; moving doors record ownership, settle independently, ignore duplicate completions and release only their own locks on exit. Case 469 now exits during active door audio. Focused 468–470/432 and native 468–470 pass. Physical Mac window closing immediately after starting all three training queues exits zero with three locks released and clean disposal; exact frame timing and listening fidelity are not claimed.

Full Mac validation at `f6dd5c0911cb71830ef958afee01b88e42fb5f0e` passes **470/470**, nine Python tests, strict import, source startup and Windows cross-export. Audited package: **1,222 entries, 64 illustrations, zero tests**; **149,355,776 bytes**, SHA-256 `69aadb93cba61b90f2b3ae41180782e77487736bfaf2a2d5e96e405b9cb860e4`. Individual logs and package inventory: `artifacts/validation/evidence/training-lifecycle/full-run-f6dd5c0/`. Integrated locally without push/PR. Original cadence and Windows acceptance remain pending; totals remain **34/48 with implementation evidence, 0/48 fully accepted**.

## Crew-loss reports and captured factory staff (472-case checkpoint)

Cases 471–472 reproduced missing pilot/passenger/station-crew reports, then exposed a captured factory retaining its removed team and reporting it twice. Shared loss reporting now includes the crews actually removed; capture clears its factory-team reference. Saved history, repeat protection, typed ranks, distant crews and surviving Earth ground crews are checked. Focused 471/472/436–442/297/301/375 pass; strengthened native 471/472/436/437 pass and News screenshots were inspected. Original high-reference roster behavior and the rogue-crew/pirate controller remain separate gaps.

Full Mac validation at `2668a0f999c33d4ee56da2f6cbdbc8b6ddb18f2b` passes **472/472**, nine Python tests, strict import, source startup and Windows cross-export. Audited package: **1,222 entries, 64 illustrations, zero tests**; **149,357,056 bytes**, SHA-256 `c31f98fe7825f5ca96dfdb26ba6cd2814a0c97d770fa471cef709a268354d2e5`. Evidence: `artifacts/validation/evidence/crew-news/full-run-2668a0f/`, with red/focused/native evidence alongside it. Integrated locally without push/PR; Windows execution remains pending. Counts stay **34/48 with implementation evidence, 0/48 fully accepted**.

## Pending bulletin delivery (474-case checkpoint)

Cases 473–474 reproduce a research discovery overwriting the production bulletin from the same update and a discovery replacing a locked screen. Saved pending notices now preserve request order and modal ownership. Follow-up reproductions also corrected fresh notices overtaking saved ones and News replay consuming a different queued notice. Repeated pending requests coalesce; old saves start with an empty list and malformed lists are rejected. This retains the current remake producer order, not the original discovery-flag priority.

Full Mac validation at `34440aac483d537561abb43f01eaa42a4c273897` passes **474/474**, nine Python tests, strict import, source startup and Windows cross-export. Focused compatibility and native 473/474/128/462 pass; the screenshot was inspected. Audited package: **1,222 entries, 64 illustrations, zero tests**; **149,358,432 bytes**, SHA-256 `ac4efd35c8b34bca2fd1a0d3e5e757bd1c10b69d4bfb78411770e17b7de266ba`. Evidence: `artifacts/validation/evidence/bulletin-delivery/full-run-34440aa/`; red and final focused/native logs are retained alongside it. The headless screenshot configuration error under `setup/` is not counted as a game defect.

Integrated locally without push/PR. Windows execution, original clock/priority integration and campaign acceptance remain pending. Counts stay **34/48 with implementation evidence, 0/48 fully accepted**.

## Saved fractional clock and AMA phases (493-case checkpoint)

Natural time now produces a saved `.01` increment after a nominal 315.6 seconds and consumes one simulation update. Manual steps add `1.00` while preserving the partial normal interval. A pending increment survives locks and save/reload; stalls do not create catch-up bursts. The historical `CurrentDay` counter keeps training/travel progress independent of displayed calendar dates. Main date, News, save slots and ETA use fractional dates; ETA refreshes after hold/release, toggle and external stops.

Calendar attrition uses true 100-day crossings. Enemy production retains exact centiday deadlines, including the 950-unit interval, with legacy deadline migration. AMA scanning/mining uses original clock-bit gates and saved stable slots: IOS per star, SCG globally. Surviving ships retain phases after removal/reordering/reload. Asteroid approach and departure now consume two updates each. Existing SCG mining eligibility and fleets above the original sixteen-slot capacity are preserved pending full compatibility mapping.

Cases **475–493** include observed failures before the corresponding fixes, plus legacy/save/pause compatibility coverage. Final independent review found one Important ETA-refresh defect; case 493 reproduced it through real control handlers and passes after the shared display correction. Native date/News/save/ETA and calendar/AMA cases pass, with screenshots inspected. Physical Mac play at `862e421` observed natural `.00` → `.01` advancement and research training starting without time-control input, followed by a clean window close. This brackets the transition; it does not measure the exact interval or establish original-runtime timing.

Full Mac validation at `e369433757efc6654e62d8ee39ab6a767d2b7c86` passes **493/493**, nine Python tests, strict import, source startup and Windows cross-export. All individual case logs are audited against the current discovery manifest and timestamps. The build retains 14 existing warnings and zero errors. Package audit: **1,223 entries, 64 illustrations, zero tests**; **149,366,032 bytes**, SHA-256 `68288aed6bcfae85e1af64474c765982c069743963a3de71b73429e56ec34f68`. Evidence: `artifacts/validation/evidence/fractional-clock/full-run-e369433/`, with failing reproductions and focused/native logs alongside it.

The earlier `391822a` full attempt stopped at import with a Timer lifecycle error and script reformatting during editor shutdown. Restoring the mismatched new indentation reproduced both; matching the surrounding tabs produced a strict clean import without changing source bytes. No Timer diagnostic was exempted. Earlier fixture registration/initialization errors and a native font-wrapper teardown experiment are retained separately from gameplay failures.

Integrated locally without push/PR. Windows execution, measured original timing, independent star/SCG clocks and the remaining Hyperlight/rogue-crew/ending work remain open. Keep the `8cdd458` human desktop handoff separate. Totals remain **34/48 with implementation evidence, 0/48 fully accepted**; this batch advances several partial tasks without declaring full acceptance.


## Saved interstellar travel and Warlord (513-case checkpoint)

New SCG cross-star routes now consume the original 9×9 distance table, fixed-point acceleration, phase fuel and private-clock progress. Ordinary star arrival requires the exact destination clock; Hyperlight synchronizes it. The existing body-to-body course interface composes that star leg with the destination-star-to-body approach. Only the final body arrival notifies ACC, preserving cargo and complete-cycle behavior. Existing in-flight saves without `Flight` retain their old countdown for that journey.

An Admiral becomes Warlord only on an actual Hyperlight arrival, before ordinary arrival experience is awarded. The saved milestone survives crew transfers, cryopods and reload; ordinary experience still caps at Admiral. Screens display the private/star calendar, destination ETA and local approach. Active-flight course edits, including retained callbacks, cannot invalidate saved progress. Source-traced boundaries include late Hyperlight research, drive damage, final-unit coasting, phase-fuel underflow and a six-update stranded countdown after empty-fuel star arrival.

Cases **494–513** cover these paths. Reproductions established instant cross-star arrival, wrong fuel, missing promotion, premature rank eligibility, wrong displayed clock, retained-map rerouting and missing stranded/control boundaries. The independent whole-branch review found one Important regression: excluding generic fuel also excluded an old fall-counter reset. Case 513 reproduced premature loss after a rescued SCG's later empty-fuel arrival; the shared reset is restored and the case passes headlessly and natively. No Critical or Minor findings remain from that review. Initial build and fixture catch/return-duration mistakes are retained separately and are not counted as game defects.

Full Mac validation at `e1754ebb7779e433c9c516dfcb98baeaabc807c9` passes **513/513**, nine Python tests, strict import, source startup and Windows cross-export. The build has zero errors and the existing warning set. Individual case logs are audited against discovery and freshness; package audit: **1,224 entries, 64 illustration imports, zero test resources**, **149,377,456 bytes**, SHA-256 `43d014cd5924f78855092caa059eb7f4623b239321b29742fac62eb2c2b96a7c`. Evidence: `artifacts/validation/evidence/interstellar-flight/full-run-e1754eb/`. Native cases 503/505–508/510–513 pass; Warlord/arrival and loss-screen screenshots were inspected. These are staged real-engine/native checks, not an unstaged campaign playthrough or original-runtime comparison.

This adds Warlord implementation evidence, bringing the ledger to **35/48 with implementation evidence, 0/48 fully accepted**. Windows execution of this revision, original-runtime comparison, rogue crews, transmitter activation and ending remain open. Preserve the separate `8cdd458` human handoff. No push, PR or Asana status change is made.

### Desktop flight check

The same `e1754eb` runtime also passes a visible Mac mouse/keyboard roundtrip. A fresh game supplied an empty disposable save; only the SCG, Admiral, Hyperlight research and route UI unlocks were staged. Through the actual controls, Mercury → Atlantic consumed 250→137 fuel, preserved 12 iron and granted Warlord. Saving/reloading the arrival retained those values. The return was stopped at phase 3 with 132 fuel, saved and reloaded through the UI, then completed at Mercury with 20 fuel and one promotion report total. ETA changed when fast-forward stopped; private/star dates followed the journey. Fast-forward continued beyond arrival and crossed the ordinary day-200 attrition boundary, reducing the crew from ten to nine.

The Settings-open physical window close exited successfully with a clean strict log. Screenshots, initial/mid-flight/final saves and provenance are under `artifacts/validation/evidence/interstellar-flight/desktop/`. Generated slots were removed only after matching their archived copies; the pre-existing editor was untouched. This is staged desktop acceptance for these paths, not normal manufacture-to-flight progression, Windows execution or original-runtime parity. The pre-existing generic engine line can still say Engaged during local drift; the new flight status explicitly says Drifting. That UI follow-up is corrected in the engine-readout batch below.

## Original six-mount SCG correction (514-case checkpoint)

Runtime `771ceea74a4370eaf35e1210ec88b28bf1e970c5` restores six usable SCG mounts. Original selector `$398DC–$398F0` examines six module words; refit `$39D2A–$39D3C` writes six supply pods. The remake already contains six bay sections/navigation controls, but creation, callbacks and display used five. This supersedes the earlier five-mount assumption in case 263 and the ship-assembly checkpoint.

New SCGs now create six slots. Loading older hulls pads missing slots with empty mounts after validation, preserving existing cargo, crew, tools and references; repeat loading does not add more. No existing slots are removed. The sixth pod uses the shared fitting/service callbacks. The interior now has its sixth cargo label, and the existing Cargo dialog fits all six rows without overlapping Close.

Case 263 and new 514 first failed for hidden navigation and missing save migration. Extending 514 through actual interior entry then reproduced an index error and the sixth Ditch button overlapping Close; both are corrected. Native 263/267/514 pass, including pointer-driven sixth-pod disposal and untouched fifth-pod cargo. Screenshots of the bay, engine, six cargo labels and dialog were inspected. A physical desktop test loaded a disposable legacy save, fitted the sixth supply pod, loaded 250 iron, ditched only that cargo and saved the result. The saved hull retained six mounts, 123 titanium in pod 5, one remaining spare supply pod and 50 iron in stores. Native window closing exited zero with a strict-clean log. The generated slot and backup were archived and removed; the pre-existing save backup was unchanged. This is staged desktop evidence, not a normal campaign. The first aggregate at `3a6d7c8` was deliberately interrupted during case 60 after the caller review exposed the interior limit; it is preserved, not reported as a passing aggregate. The next run at `188a1fa` passed 496 cases before case 497 failed because its old one-module fixture used `Single()` after save migration. The corrected assertion checks six mounts, cargo in its original slot and total cargo conservation; 497–514 then passed focused checks before the fresh aggregate.

Fresh full validation at `771ceea` passes **514/514**, nine Python tests, strict import, source startup and Windows cross-export. Individual logs and the package were audited: **1,224 entries**, **64 illustration imports**, **zero test resources**, **149,377,184 bytes**, SHA-256 `35c70c9ea396446ffa6d8d0beb9f681a4672806465805fe845c0ed35da44bd54`. Evidence is under ignored `artifacts/validation/evidence/scg-sixth-mount/`. Existing compile warnings remain; there are zero build errors. This extends the SCG and supply-pod rows, keeping **35/48 with implementation evidence and 0/48 fully accepted**. Windows, original-runtime and normal campaign acceptance remain pending.

## Manual bay stock capacity follow-up

At `70cabb8`, manual cargo, equipment and empty-pod returns now reject transfers that would exceed the existing 50,000-unit store limit. One shared capacity check runs before any inventory or fitting change, including the fuel refund preceding DFCC conversion. Rejection preserves the original cargo, replacement stock, tank and hull mode; an exact-capacity return succeeds. This preserves the current remake's all-or-nothing manual transfer behavior, rather than introducing partial unloading or claiming original fidelity.

Cases **515–519** first reproduced overflowing stores on all three hulls, equipment replacement and spare-pod returns. All five pass headless and native Mac; 15 related cases (253/256/259/260/263/296/297/300/301/417–421/514), strict import and source startup also pass. Build has the 14 existing warnings and zero errors. Native screenshots exposed an oversized warning; the added panel-containment assertion reproduced that failure, and the final shorter wording passes headless and native case 517 with an inspected screenshot.

Evidence: ignored `artifacts/validation/evidence/bay-stock-capacity/`. These cases are now included in the fresh 523-case aggregate below; Windows execution remains pending. This is an additional review fix supporting existing Stores/supply-pod work; totals remain **35/48 implementation evidence, 0/48 fully accepted**.

## DFCC removal and 523-case checkpoint

At `171ed86d72a568a8dcdb54f79f195d695ff1f6f4`, removing or replacing the last fitted DFCC clears the conversion flag, refunds fuel at its current 10:1 rate and returns the correct IOS/SCG drones before changing equipment. All required returns must fit stores; otherwise nothing changes. Another fitted controller keeps conversion active. Unrelated equipment edits preserve older converted hulls without a controller module.

Cases **520–523** cover both hulls, removal/replacement, ordinary fuel cost afterwards, repeated conversion, save/reload, each full-store boundary, exact capacity, a controller in the sixth mount and legacy compatibility. Cases 520–522 reproduced retained DFCC state or non-atomic capacity handling before the fix; 523 adds compatibility coverage. All four pass headlessly and natively. This corrects the remake's existing equipment selector; it does **not** implement the original conversion routine's complete mount/ACC stripping behavior.

Fresh full Mac validation passes **523/523**, nine Python tests, strict import, source startup and Windows cross-export. Every individual case log was checked against discovery and freshness. The build has zero errors and the existing warning set. Package audit: **1,224 entries, 64 illustration imports, zero test resources**, **149,378,640 bytes**, SHA-256 `64222b50865970ddb5ff6d8df3cadf78014bb8d5804a9515b7a694667db458d5`. Evidence: ignored `artifacts/validation/evidence/dfcc-removal/full-run-171ed86/`.

Physical Mac controls loaded a disposable SCG fixture, removed its DFCC, refitted/removed it again, saved and reloaded. Persisted results: no DFCC mode, zero onboard drones/fuel, three controllers, twenty star drones and 120 HeD in stores; the other five mounts were unchanged. Native window closing exited zero with a strict-clean log. Screenshots, saves and provenance are in the adjacent `desktop/` directory. Generated files were archived and removed, restoring the original save inventory exactly. This is staged desktop evidence, not normal campaign or Windows acceptance. Totals remain **35/48 implementation evidence, 0/48 fully accepted**.

## Drifting engine readout follow-up

Runtime `01c2942` corrects the shared interior label: transit displays Engaged only while `EngineEngaged` is true. Launching, docking, landing and takeoff retain their powered indication; engine damage still takes precedence. New case **524** reproduced the misleading label through the real disengage callback, then passed headlessly and natively for shuttle, IOS and SCG. Native SCG screenshot inspection confirms matching Drifting/Disengaged text. Existing cases 103/104/292/502/505 also pass, covering menus, damage and interstellar boundaries. Build succeeds; a preliminary test-registration compile error and stale-build attempt are retained separately from the valid gameplay reproduction.

Evidence: ignored `artifacts/validation/evidence/engine-readout/`. The latest complete aggregate remains **523 at `171ed86`**; no full 524-case or Windows result is claimed. This additional UI correction leaves the Asana count at **35/48 implementation evidence, 0/48 fully accepted**.

## Rogue crew, sabotage and prison recovery (572-case checkpoint)

Runtime `941eca947db76c714a5cabe9829869851b324d78` adds saved one-time Warlord takeover and Pirate identity, BOUNTY routes/refit/raids, MTX redirection, occupied-dock sabotage and escape, cockpit recovery, prison containment and Mutiny/prison-discovery delivery. Existing gameplay and UI paths share active-world/rogue command guards, so retained callbacks cannot command a stolen ship or debit a replacement world. Prison research, paid manufacture and fitting use normal controls.

Cases **525–572** cover eligibility, exact identity validation, legacy saves, crew loss, routing, resources, SDM, recovery, capture gestures, full rosters, research/manufacture and notice priority. Independent review found two Important defects: temporary sabotage restoration resurrected attrition casualties, and a destroyed station could permanently strand its rogue crew. Cases 571/572 reproduced both before correction and now pass headlessly and with native rendering; seven related interruption checks also pass. The sole review correction round is complete; no second review was requested.

Full Mac validation passes **572/572**, nine Python tests, strict import, source startup and Windows cross-export. All case logs are audited against the fresh discovery manifest and timestamps. The incremental build reports zero warnings/errors; the preceding source compilation retained 14 existing warnings. Package: 1,225 entries, 64 illustration imports, zero test resources; **149,407,200 bytes**, SHA-256 `bc629d07881518ba0e73c69b666d73a9505b59dc336e32ac54e2fce8e030e08c`. Evidence: `artifacts/validation/evidence/rogue-crew/full-run-941eca9/`.

Before the full run, 82 focused/compatibility checks and five rendered tests passed. Their screenshots were inspected. A staged physical Mac session triggered the full Mutiny notice, followed actual hostile refit/return, recovered the cockpit crew, fitted a prison, exercised timeout/right-click retention and saved/reloaded the same contained Pirate crew of 81. The fixture explicitly supplied research/stocks and corrected its missing Tool Pod prerequisite; provenance and before/after saves are retained. The game exited cleanly and the original player-save inventory was restored. Physical testing also found clipped help and an obscured Equipment action; font-bound and actual-pointer regressions reproduced those failures before correction. A cosmetic disabled-button transparency overlap remains deferred; the enabled action is legible/reachable and occupied removal is blocked.

The first full attempt at `014d4fe` failed strict editor import while Godot converted two new space-indented GameCore lines to tabs. Matching the surrounding tabs produced a clean strict import without source mutation; no additional engine error was exempted. Failure logs and editor diff remain under `import-failure-014d4fe/`. The following `63e770e` attempt passed 423 cases before case 424 rejected a fixture ship registered twice. Removing that duplicate left production code unchanged; the fresh `941eca9` aggregate includes the corrected fixture. Its failed attempt is preserved under `full-attempt-63e770e/`.

This advances existing News/SCG/save/event-order tasks: **35/48 with implementation evidence, 0/48 fully accepted**. The read-only Asana refresh still matches all 48 open IDs among 86 tasks. Windows execution, original-runtime timing/gesture comparison and unstaged campaign acceptance remain pending. Composed star/body routes, expanded stable slots and safe empty-prison removal are explicit remake adaptations. Transmitter activation and the ending remain open. No push, PR or Asana changes.

The verified runtime and documentation are integrated locally. The contribution branch has identical `Godot`, `scripts` and `.github` trees to the tested revision, and root `AGENTS.md` is unchanged.

## Original transmitter ending (586-case checkpoint)

Runtime `8353f6ce14b0a4499b5239ea85b80fb5ecbf1d85` restores the original Disk 2 ending through the existing mounted device. A bounded Python compiler emits source-derived indexed artwork, six bitmap labels and stereo PCM; Godot uses native audio and an audio-clocked image overlay. Normal undocked Warlord activation precedes DFCC interception, preserves fuel/items and blocks retained ship commands while permitting callbacks belonging to the active Rename/Cargo overlay.

Full Mac validation passes **586/586**, **19 Python tests** with the original disk, strict import, source startup and Windows cross-export. All case logs match the fresh discovery list and timestamps. The fresh incremental build reports zero warnings/errors; the preceding source build retained 14 existing warnings. Package: **1,230 entries**, 64 illustration imports, zero test resources; **163,889,360 bytes**, SHA-256 `53efd06d9898d5bc0ed1340a131ddb1ead377e2685a09f7901d196a6d8396cd1`. The embedded ending JSON equals the committed file, and the JSON/audio import/native sample payloads match their pack MD5 records. Evidence: `artifacts/validation/evidence/transmitter-ending/full-run-8353f6c/`.

Cases 573–586 cover composition, pause/input/replay/lifetime, rank/state/rogue eligibility, sixth-mount and DFCC precedence, retained callbacks and staged capture-to-activation progression. All six activation cases passed natively; the player checkpoint additionally compares four native texture hashes against independently composed artwork/labels/fade/black. A physical Mac Load/SCG/sixth-module click played through the six labels and automatically replayed without seeking; Escape/right/left did not dismiss it and native window close exited cleanly. Original saves were restored exactly. This was a staged nine-station copy: the unmodified eight-recovery save exposed an existing Master Control overflow beyond 16 displayed stations, retained for the next fix.

The first full attempt at `3206208` stopped at case 107: a broad pause guard prevented the existing prepaused Rename contract. The corrected active-overlay ownership guard passed eleven focused checks and native 107/585 before the fresh aggregate. Source music order-jump RED/GREEN testing also corrected its independent table pointer; regenerated assets remain byte-identical because that jump follows the ending.

The sole independent review found no Critical/Important defect. Deferred minor coverage gaps: malformed-data testing exercises the parser after successful initialization, not the actual error panel; seek-based catchup does not establish warmup/stalled-render timing near the fade. No subjective listening or original-emulator comparison is claimed. Native Windows and full unstaged campaign acceptance remain pending. Counts remain **35/48 with implementation evidence, 0/48 accepted**. No push, PR or Asana changes.

## Overview capacity (589-case checkpoint)

Runtime `89d39ffb164ac1c6cffe21f2cfff18610e193bbe` fixes a physical Master Control crash when a save contains more than 16 friendly stations. All three fixed-size station/IOS/SCG grids now use bounded pages, with hostile stations filtered before indexing, stable identities/hover text and page clamping after losses. A pager appears only when required. Original single-page spacing is preserved; overflow controls stay clear of hover text and the footer. This is a remake compatibility adaptation, not a recovered original paging interface.

Cases **587–589** cover stations and both fleets, including 16 friendly stations followed by hostile entries, all 33 identities, last-page losses/captures, selection and input locks. The station overflow, pager/hover overlap and single-page spacing each have retained failing checks before correction. Sixteen focused cases and three native rendered cases pass; the final spacing correction also passes 587–589 and native 587.

Fresh full Mac validation passes **589/589**, **19 Python tests**, strict import, source startup and Windows cross-export. Individual logs match fresh discovery and timestamps. Package: **1,230 entries**, 64 illustration imports, zero test resources; **163,893,120 bytes**, SHA-256 `ec25f985c70fca58c8c9cc4e65805dd0c12c6f423690fa957cab80a0f9d400a6`. Ending JSON matches source and ending payload MD5s match the pack. Evidence: `artifacts/validation/evidence/overview-capacity/`.

Physical Mac testing loaded the unmodified eight-recovery transmitter save, traversed all five pages, selected Alpha Orbital in Tau Ceti on the last page, returned with right-click, used Next/Prev, opened the SCG and activated its sixth mounted transmitter. The window closed cleanly, and the original save-file/hash inventory was restored exactly. This removes the full-fixture crash recorded in the ending checkpoint; it remains staged evidence, not an unstaged campaign or Windows acceptance. Counts stay **35/48 with implementation evidence, 0/48 accepted**.

## Early-game follow-ups (591-case checkpoint)

Runtime **`833cb85da2e7bf6d377b0135e2c8f6c88e5f4a24`** passes **591/591** isolated Mac Godot regressions, **19 Python checks** with the original ending disk supplied, strict asset import, source startup and Windows cross-export. Every per-case log was checked against the fresh discovery manifest and timestamps. This is additional review work: counts remain **35/48 with implementation evidence, 0/48 fully accepted**.

- **Recruit capacity:** all three training plus handlers omitted the shared population limit. Case 590 first failed with research count 2 instead of 1 after the three remaining recruits were allocated across the teams. One computed remaining count now serves the label and all three guards. The regression covers rejection, cancellation/reassignment, completion and zero-population rejection without changing training times, door locks or saves.
- **Research mass layout:** normal native play showed the `t.` unit overlapping a one-digit mass. The old spacing depended on digit count although the value is right-aligned. Case 591 reproduces the overlap and verifies one/two/three/four-digit real items after restoring the scene's fixed spacing.

Both new regressions pass headless and natively; native screenshots were inspected. Eleven related training, research, Stores and fuel-refining cases also pass. Early versions of the layout test hit a Godot mono `rc_owner` shutdown error after direct font measurement; these failed logs remain preserved. Measuring the existing Label's prefix minimum size instead gives both the expected original-code failure and corrected-code success with clean teardown. No engine error exemption or production shutdown workaround was added.

The exported executable is **163,893,728 bytes**, SHA-256 **`eec9dd84bacf8e24a76d6753dc81e7ca43b01d198d4512ccbba0a4c165340c9d`**. Its pack contains 1,230 entries, 64 illustration imports and 0 test resources. Ending JSON equals source, and ending JSON/audio payload hashes match the pack table. Build: 0 errors; source recompilation retains 14 existing warnings. Evidence: `artifacts/validation/evidence/desktop-followups/`. Cross-export does not establish native Windows acceptance.

A separate [normal new-game desktop route](native-gameplay-results.md) passed at the preceding 89d39ff runtime: training, research, paid shuttle manufacture, refining, rename, first flight/landing and save/load. Original user saves were restored exactly. Orbital construction and later unstaged campaign acceptance remain pending.

## Interior Service control (593-case checkpoint)

Runtime `50ea4af6765025928f36324dc79be999884cd853` repairs the user-reported dead Service artwork in the upper-left ship interior. The scene had no interactive control over it. A flat button now enters the existing bay at its crew section, preserving the selected hull and correct ground/orbital resource context. Existing flight, rogue-control and input ownership restrictions apply; completed damaged colonies still allow repair servicing.

Cases 592–593 reproduce the missing control/dead pointer before the fix, then pass headless and natively for ground/orbital shuttles, IOS and SCG, with state, location and overlay restrictions. Physical Mac clicks at the reported position open the Earth bay and repeat the interior/bay roundtrip in the unmodified normal-play checkpoint. Fuel remains 6T and Captain Blunket retains nine crew. The owned game exits zero, the strict log audit passes, and the exact original save inventory is restored. Evidence is preserved under `artifacts/validation/evidence/interior-service/`.

An initial implementation build failed because an inherited property shadowed the scene-variable enum; qualifying the enum corrected compilation. A stale-binary check accidentally run after that failed build is retained separately and excluded from the valid RED/GREEN evidence. The final source build retains 14 existing warnings and zero errors.

Fresh full Mac validation passes **593/593** isolated game cases, **19 Python checks** with the original ending disk, strict import, source startup and Windows cross-export. Each case log matches fresh discovery and timestamps. Package: **1,230 entries**, 64 illustration imports, zero test resources; **163,895,264 bytes**, SHA-256 `bcef31fba25ffe9019403fed828dd881651b715b5c22fec42d6fb66e66454847`. Ending JSON equals source and ending JSON/audio payloads match the pack hashes. The incremental build has zero warnings/errors; the preceding source build retains the 14 existing warnings. Aggregate evidence: `artifacts/validation/evidence/interior-service/full-run-50ea4af/`.

This is an additional user-reported fix, not a new Asana completion. Counts remain **35/48 implementation evidence and 0/48 fully accepted**. Current native Windows and later campaign acceptance remain pending.

## Orbital production-assignment hover

Runtime `8fec147419d2a91d69e78cd50aeb7a9e1160def6` corrects the missing explanation over an available production team in an orbital ship bay. The shared hover helper had restricted its assignment text to Earth, although the existing action also assigns orbital factory staff. The correction mirrors that action's availability: manual factory, no existing team. Automated and occupied factories remain silent.

Existing case 58 now fails before the fix and passes afterwards, checks both rejection states and verifies the actual assignment preserves the team. Six related headless checks and native case 58 pass. No new test case was added. Evidence: `artifacts/validation/evidence/orbital-staff-hover/`. The [normal campaign](native-gameplay-results.md#orbital-staffing-continuation) separately transported and assigned Redman's 200-person Expert team, then verified the staffed factory and docked shuttle through save/reload.

Fresh full Mac validation passes **593/593**, **19 Python checks**, strict import, source startup and Windows cross-export. Every individual case log matches the fresh discovery manifest and timestamps. The incremental build reports zero warnings/errors; source compilation retains 14 existing warnings. Export records only the existing exact Godot 4.2.2 editor-teardown exemption. Package: **1,230 entries**, 64 illustration imports, no tests; **163,895,264 bytes**, SHA-256 `ebe7fa54355ec20d91d6974c87577cb2562ade22526b173f7a43bbd0e3bdee56`. Ending JSON equals source and embedded ending payload hashes match. Full evidence: `artifacts/validation/evidence/orbital-staff-hover/full-run-8fec147/`.

The verified bundle and isolated Windows validation script are uploaded, but execution is deferred while another Godot process is present. No new Windows result, push, PR or Asana closure is claimed. Counts remain **35/48 with implementation evidence, 0/48 fully accepted**.


## Cargo fuel readout and takeoff guard (594-case checkpoint)

Runtime `20e74aaf48de64834b5bb533dc3f21ac4032adfc` includes two small shared fixes found during the normal IOS campaign and caller review:

- Cargo load/unload refreshed the pod but left the lower fuel-stock readout stale. The handler now uses the existing full bay refresh. Strengthened cases 515–517 cover fuel loading, unloading and replacement, both fuel types, unchanged tanks and an open selector. Case 515 failed before the fix; focused headless/native checks pass.
- `Ship.TakeOff(bool)` allowed non-docked calls to grant pilot experience and clear bay state without a real departure. Its shared guard now requires `Docked`, retaining the internal fuel bypass used by rogue crews. Case 594 failed before the correction and passes for all four ground/orbital hull contexts, non-docked states, one valid departure and repeated commands. Fourteen related cases and native case 594 pass. An initial test-compilation error is preserved separately from the valid RED result.

Fresh full Mac validation passes **594/594 isolated game cases**, **19 Python checks**, strict import, source startup and Windows cross-export. Every case log matches fresh discovery and timestamps. The exported executable is **163,895,264 bytes**, SHA-256 `2bf05ca34ac109d4dbd21aff814b4319285cc80c7b93fab847fb2b063450356d`; its pack has **1,230 entries**, 64 illustration imports and no test resources. Ending JSON equals source; embedded ending JSON/audio hashes match. Aggregate evidence: `artifacts/validation/evidence/cargo-fuel-readout/full-run-20e74aa/`; focused evidence is archived beside it and under `takeoff-gates/focused/`.

Physical Mac checks loaded the unmodified normal IOS checkpoint. Moving 200 MeH into and out of the shuttle updates both stock readouts immediately (200→0→200), with tank fuel unchanged at 17T. One IOS undock followed by ten repeated Take Off clicks increases Raphael's saved action count only from 3 to 4; ordinary time completes undocking with fuel 44→43T. An initial save-check expectation of 44T was corrected after confirming the completed departure state; no runtime change was needed. The game exits zero, strict log audit passes, and original saves/hashes are restored exactly. Evidence: `cargo-fuel-readout/physical/`.

The [normal campaign](native-gameplay-results.md#first-ios-and-interplanetary-roundtrip) also completes paid IOS manufacture, crew assignment, rename, Earth–Moon–Earth travel, docking and save/reload. Current Windows export execution and later campaign acceptance remain pending. These additional fixes leave totals at **35/48 with implementation evidence, 0/48 fully accepted**.


## AMA exhausted-tank departure (595-case checkpoint)

Runtime `b2c435c7c38923297af432db8dab94d7f6d93126` fixes another boundary in the existing AMA investigation. A one-unit tank can empty during the remake's two-update asteroid approach. The shared TakeOff fuel guard then rejected both manual departure and the automatic no-compatible-cargo exit. Original `$30B2E` explicitly allows zero-fuel departure from mining state `$0D`; `$23CA2` reaches the same departure helper. The correction applies that exception to docked interstellar hulls at the asteroid field, retaining the engine requirement, docked-state gate and ordinary station/ground fuel checks.

Case 595 first fails with `expected Launching, got Docked`, then passes for manual and automatic exits, unchanged cargo/zero fuel and rejected ordinary station departure. Thirteen related headless cases and native case 595 also pass. This is staged regression evidence; a normal mining expedition and Windows pointer/export acceptance remain open. The [original trace](original-ama-mining-evidence.md#departure-with-an-exhausted-tank) distinguishes the remake's reachable approach state from the original's separate zero-fuel entry gate; broader fuel cadence is not changed.

Fresh full Mac validation passes **595/595**, **19 Python checks** with the original ending disk, strict import, source startup and Windows cross-export. Individual case logs match fresh discovery/timestamps. Export: **163,895,264 bytes**, SHA-256 `71bd57f496a8d1b30f98bfaeea1abc1dbcb3fa07e3814c792b5b7c83807b89a1`; **1,230 pack entries**, 64 illustration imports, no test resources. Ending JSON equals source and ending payload hashes match. The incremental build has zero errors/warnings; the preceding source compilation retains 14 existing warnings. Evidence: `artifacts/validation/evidence/ama-departure-fuel/{focused,full-run-b2c435c}/`.

The previous Windows candidate did not start validation: the first SSH invocation failed path quoting, then the corrected invocation stopped at the existing-game guard. No checkout was created and the desktop process was left untouched. Counts remain **35/48 implementation evidence, 0/48 fully accepted**; no push, PR or Asana write.


## Grapple breakup quantities and normal recovery

Runtime `0a2f417eb8821d704d71a8b7ec85aae1d6cf0ff0` replaces hardcoded `100 … 50000` breakup quantities with the actual held mass and projected stock capped at 50,000. Normal play exposed the false readout while correctly crediting a 250T palladium asteroid. Existing case 26 now checks two consecutive pods: iron 12+250→262 and carbon 49,900+150→50,000. It fails against the old placeholder and passes headlessly and natively after correction. No additional test case or framework was added.

Fresh full Mac validation passes **595/595**, **19 Python checks** with the original ending disk, strict import, startup and cross-export. All case logs were audited for freshness and success. Export: **163,895,248 bytes**, SHA-256 `4aec5335de6192e839a8f391953d03376843b2f0544dbf6d1b5d8ecbed7613d2`; **1,230 pack entries**, **64 illustration imports**, **zero test resources**. Ending JSON matches source and packed ending payload hashes pass. Evidence: `artifacts/validation/evidence/grapple-readout/{focused,full-run-0a2f417}/`.

The normal campaign now includes asteroid travel, naturally generated capture, held-object save/reload, Earth return, unloading and delivered-stock reload. Service pointer checks pass at Earth ground and orbit. Corrected breakup visual capture, original counter animation, native Windows execution and later campaign acceptance remain open. Counts remain **35/48 implementation evidence, 0/48 fully accepted**; AGENTS.md remains unchanged.


## Concurrent asteroid mining

Runtime `bcaeb1cc9da5ae956059857f822296a1d86988d8` corrects station occupancy leaking into asteroid approaches. Two separately equipped IOS hulls could approach together, but only the first entered mining; the second remained docking and continued consuming travel fuel. The shared completion condition now exempts asteroids while retaining ordinary station occupancy.

Existing case 492 first reproduces `expected Docked, got Docking`, then passes both two-update approaches across save/reload, independent ore collection, unchanged post-approach fuel and one miner departing without disturbing the other. It also keeps the second hull blocked at an occupied Earth station. Thirteen related headless checks, including rogue occupied-docking case 535, and native case 492 pass. The [original instruction trace](original-ama-mining-evidence.md#independent-mining-approaches) supports independent per-hull transitions.

Fresh full Mac validation passes **595/595**, **19 Python checks** with the original ending disk, strict import, source startup and Windows cross-export. Individual case logs were checked for freshness and success. Export: **163,895,248 bytes**, SHA-256 `95a6739dc4b8293413fe8d593bfde36a8f07f89c4c01bda4860d8c1555822bbf`; **1,230 pack entries**, **64 illustration imports**, **zero test resources**. Packed ending JSON matches source and ending payload hashes pass. The source compilation retains 14 existing warnings with zero errors. Evidence: `artifacts/validation/evidence/concurrent-mining/{focused,full-run-bcaeb1c}/`. Native Windows and a normal two-IOS expedition remain pending. This strengthens the existing AMA investigation and leaves totals at **35/48 implementation evidence, 0/48 fully accepted**.


## Consecutive grapple pointer cleanup — 2026-10-03

Runtime `06d09c31c7b1daf64efb34c632a7d5f85bb254f3` passes all **595/595 fresh-process regressions**, 19 Python checks including the original-disk check, strict import, source startup and Windows cross-export. Existing case 26 reproduces the invisible parent blocking a second pod; the one-line cleanup passes pointer hit-testing for all three pods. Normal Mac replay unloads three naturally captured asteroids in one bay visit and preserves credited stocks across save/reload.

Evidence: `artifacts/validation/evidence/grapple-pointer/full-run-06d09c3`. The independent package audit verifies 1,230 entries, 64 illustration imports, no test resources, and ending JSON/audio payload hashes. Windows EXE: 163,893,920 bytes, SHA-256 `1d43d8a214aef797302ce50fa6bf02148836f92f3d0d2a6b7b79396817fca04d`. This is a Mac cross-export; Windows source/package execution of this revision remains pending. Counts remain 35/48 with implementation evidence and 2/48 locally accepted.


## MTX ordering 597-case checkpoint

Runtime `551f31ae653aee08861918f9b452b66e166d5a25` passes **597/597 fresh-process regressions**, 19 Python checks including original-disk validation, strict import, source startup and Windows cross-export. Cases 596/597 first fail on the former order, then pass with the single registration moved after extraction and before production. Incoming ore starts paid AOC work in the same update; completed products transfer on the following update. The trace comes from the original active Disk 2 overlay, not a guessed event priority.

**An initial aggregate failed during editor-import shutdown**, after `IMPORT OK`, with `FATAL: Condition "!rc_owner" is true` and leaked editor shortcuts/export-plugin resources. It did not reach gameplay regressions. That failure is preserved under `artifacts/validation/evidence/mtx-order/import-failure/`. An isolated identical strict import and the subsequent complete fresh aggregate passed without source or error-filter changes. The cause of that observed shutdown failure is not established; it is not reclassified as success.

Audited fresh-run evidence: `artifacts/validation/evidence/mtx-order/full-run-551f31a`. Package audit verifies 1,230 entries, 64 illustration imports, no test resources, and ending JSON/audio payload hashes. EXE: 163,893,920 bytes, SHA-256 `f67d4b75caf53ff4361b20f3f06f86a2fcb3465d61a4acb3e3dee651dd529893`. Windows execution remains pending. The event-order source investigation now meets its seven named research requirements, taking local acceptance to **3/48**, with **35/48 implementation evidence**; this does not count Windows desktop gameplay acceptance.


## Asteroid Complete Cycle launch follow-up

Runtime `a06bb0ff4a2c5ef5f0e3242e29ea65f1ad4d90c3` corrects a stall reproduced during a normal AMA expedition: selecting Complete Cycle during asteroid launch left the ship idle in orbit with a full pod. `ACC.Update` handled the completed launch as an asteroid scan and returned before its existing return-flight transition. The scanner branch now excludes launch-completion events. Stationary finishing-mode scans still do not initiate mining.

Existing case 325 fails before the correction (`expected InTransit, got UnDocked`) and passes after, including launch-time save/reload, actual travel, one delivery and stopped automation. Twenty-five related headless cases and native case 325 pass. Source builds retain 14 existing warnings and zero errors. The fresh full **597/597** aggregate, **19 Python checks** with the original disk, strict import/startup and Windows cross-export all pass. Independent audit verifies every case log and the exported pack: **1,230 entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. The executable is **163,893,920 bytes**, SHA-256 `9cbf914295215853b4edcd8f7cc0bedd1e949566942910480397a9764ce86043`. Focused evidence is in `artifacts/ama-acc-campaign/regression/`; full audited evidence is archived under `artifacts/validation/evidence/acc-asteroid-cycle/full-run-a06bb0f/`.

The [normal replay](native-gameplay-results.md#normal-ama-mining-and-acc-return) delivers exactly 250 copper, stops at Earth and preserves the result through reload. Service opens the correct orbital bay. Both native campaigns close with exit zero and clean strict logs; original saves are restored exactly. No current native Windows execution is claimed.


## Selected AMA and grapple mount numbers

Runtime `56884880684c054d64446c72ce88fcd6e4c27262` moves the two hardcoded panel labels from `_Ready` into the existing `Load(ship, module)` methods and uses the selected module's one-based position. Existing navigation case 65 now opens mount-two grapple and mount-three AMA windows through the interior, checks literal labels 2/3, and retains right-click dismissal/navigation checks. It failed separately on grapple (`expected 2, got 1`) and AMA (`expected 3, got 1`) before each correction. Cases 65, 24, 444, 558 and 559 pass headlessly, and case 65 passes natively. Builds retain 14 existing warnings and zero errors.

The [normal refitting check](native-gameplay-results.md#ama-removal-refitting-and-module-labels) verifies physical labels, AMA removal without duplication and saved equipment conservation. Fresh full validation passes **597/597 regressions**, **19 Python checks** with the original disk, strict import, source startup and Windows cross-export. The independent audit verifies fresh case logs, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. The executable is **163,893,920 bytes**, SHA-256 `391d71fb5606c3454c41b4ccd8c53010ddddca7b3c423ca8270a4e908d399cf1`. Evidence: `artifacts/module-panel-number/` and `artifacts/validation/evidence/module-panel-number/full-run-5688488/`. Windows execution remains pending. Counts remain **35/48 with implementation evidence, 4/48 locally accepted research/internal requirements and 0/48 Windows desktop gameplay acceptance**.


## Departure scan lifetime and ACC close ownership

Runtime `eda1edd` moves scan invalidation from the manual interior button into successful `Ship.EngageEngine` paths, including SCG re-engagement. Existing cases 325 and 502 fail before the change because automatic IOS/SCG departure retains the old scan. They pass after, with rejected travel and mining-surface launch preserving local scans. Nine focused headless cases and native 325/502 pass; the normal saved mining replay delivers exactly 250 copper after a mid-flight reload with no retained departure scan.

That replay exposes a separate Service failure: ACC command-close leaves the global cursor locked. Runtime `2151aa31398719aa6d8a1b4aac059a837e5e7124` releases that lock in the existing `CloseACC` method. Case 325 fails on the retained lock; case 592 reproduces the failed Service pointer navigation after ACC Off. Both pass after the one-line correction. Twenty focused headless cases and native 325/592/593 pass, including cursor/modal ownership and Service eligibility. The physical Off→Service sequence passes without a right-click workaround. Builds retain 14 existing warnings and zero errors.

The combined fresh full run passes **597/597 regressions**, **19 Python checks** with the original disk, strict import, source startup and Windows cross-export. Independent audit verifies every case log, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. Export: **163,893,952 bytes**, SHA-256 `87eb7716d9de74a3e1e11a2f03c9c301d5879cd5931ea1d37603bc4ed38d837f`. Evidence: `artifacts/scan-lifetime/`, `artifacts/acc-cursor-lock/` and `artifacts/validation/evidence/scan-and-acc-lock/full-run-2151aa3/`. [Normal campaign evidence](native-gameplay-results.md#departure-scans-and-acc-cursor-lock) records the preserved saves and limits. Windows acceptance remains pending; counts stay 35/48 implementation evidence, 4/48 locally accepted research/internal requirements and 0/48 Windows desktop gameplay acceptance.


## Cargo-picker input confinement follow-up

Runtime `00949a5a7e3addbda785caa588a61d2447e81290` corrects a normal-play reproduction in which the visible cursor stayed inside Equipment Stocks while real clicks changed the background mount. The retained picker then fitted an OF frame into a supply pod. `GlobalInput._Input` now rejects pressed mouse buttons outside its locked rectangle; releases and right-click dismissal remain available. The shared correction covers cargo, equipment and crew pickers.

Existing case 64 fails before the fix (`open Supply panel ... expected 1, got 2`) and passes afterward for all three picker types. Twenty-four focused headless cases and native case 64 pass; the physical loading replay verifies valid selection, dismissal, subsequent navigation and correct saved cargo/stock conservation. Both normal game sessions exit zero with strict logs passing and exact original saves restored. See [campaign evidence](native-gameplay-results.md#shuttle-supply-runs-and-moon-station-preparation). Evidence: `artifacts/bay-modal/`.

The fresh full run passes **597/597 regressions**, **19 Python checks** with the pinned original disk, strict import, startup and Windows cross-export. Independent audit verifies every case log, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. Export: **163,894,640 bytes**, SHA-256 `3ba08a94efb9148a5c4de0ad57e0c10d1776f3d111124f804144d50a0b99a500`. Archived evidence: `artifacts/validation/evidence/bay-modal/full-run-00949a5/`. Native Windows acceptance remains pending; counts stay 35/48 implementation evidence, 4/48 locally accepted requirements and 0/48 complete Windows desktop acceptance.


## Repair and launch status width

Normal Moon repair exposed a 14-character status line extending beyond its 104-pixel area into cargo. Runtime `149ec85` changes only the first-line wording for repair and launch. Existing regression 593 failed separately for each overwide state before the correction; focused cases 103/104/195/196/325/592/593 pass afterward. The physical repair replay exits zero with a clean strict log, readable status, consumed kit and restored base controls; original saves are restored exactly. Evidence is retained in `artifacts/repair-status/`.

The longer normal campaign has passing bounded inventory/repair/save assertions but fails strict runtime validation with an unresolved `SwapGCHandleForType` handle error. Two shorter repair replays do not reproduce it. See [normal campaign evidence](native-gameplay-results.md#moon-staffing-manufacture-and-ground-base-repair). The fresh full run at `149ec85` passes **597/597 cases**, **19 Python checks**, strict import, startup and Windows cross-export. Independent audit verifies all fresh case logs, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and ending JSON/audio payload hashes. The incremental build reports zero warnings/errors; the earlier recompilation retains 14 baseline warnings. Export: **163,894,640 bytes**, SHA-256 `8d9ff33cad3dc6cca69defb11d46379031422e92576e031af9fc388429eab677`. Archive: `artifacts/validation/evidence/repair-status/full-run-149ec85/`. No Windows acceptance or backlog completion count changes are claimed.

## ACC Clear and ground crew guards — 599-case checkpoint

Runtime `fe05bc6` refreshes the existing ACC display immediately after Clear. Case 598 first fails on the retained selection diamond, then passes with cleared selections/cycle markers and no time advance. Related headless cases, native case 598 and a physical normal-save replay pass; [normal balancing and Clear evidence](native-gameplay-results.md#acc-stock-balancing-and-clear-refresh) records save restoration and limits.

Runtime `c4e1f41119246e2a7657ca22f0c4c5f9090adb0e` requires a nonempty crew for ground resource-frame construction and Bandaid activation. Case 599 first reproduces a zero-person crew constructing a section, then passes headlessly and natively for absent, empty and one-person crews across both actions. Rejected work preserves cargo/fuel and uses existing bay navigation; staffed construction and repair remain available. These crew boundaries are staged regression evidence, not normal-campaign or Windows acceptance.

The first aggregate attempt fails during editor shutdown with `FATAL: Condition "!rc_owner" is true`, after `IMPORT OK` and before any regressions. Its logs remain in `artifacts/ground-crew-guard/failed-import/`. A separate strict import and fresh full run then pass **599/599 cases**, **19 Python checks** with the original disk, source startup and Windows cross-export. The existing narrowly identified EditorSettings shutdown exemption is unchanged; `!rc_owner` is not exempt. This result does not resolve that intermittent failure or the separate long-session `SwapGCHandleForType` error.

Independent audit verifies every fresh case log, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. Export: **163,894,640 bytes**, SHA-256 `7c2b4dd1c710db1fd237f62930fe4b8d12597baadb5361ec0c05890d1441a857`. Archive: `artifacts/validation/evidence/ground-crew-guard/full-run-c4e1f41/`. The incremental build has zero warnings/errors; the earlier recompilation retains 14 baseline warnings. Original user saves are unchanged, and the pre-existing editor is preserved. Counts remain 35/48 implementation evidence and four locally accepted research/internal tasks; previously reported Windows passes and Craig's confirmations remain credited separately.

## Pod fitting and removal — 601-case checkpoint

Runtime `3ebeca031529c974f9b8137bd974c50470f0c690` adds original-backed vertical pod motion to the shared `Torso` controller. Existing artwork and bay clipping are reused. The capacity-checked stock transaction commits once; presentation then removes the old sprite before fitting its replacement, with an owned input lock. Completion, reassignment and scene exit release that lock without clearing another owner's lock.

Case 600 first fails because fitting appears immediately at offset zero; after correction it checks supply/tool/cryo positions, real engine progression, repeated input and single stock transfers. Case 601 covers rejected replacement, old/new artwork order, cross-mount input and exit cleanup. Existing assembly and capacity tests now wait for the animation before their next operation; their inventory assertions remain intact. Focused headless and native checks pass, with inspected captures for all three pod types. The [normal supply removal/refit/save/reload route](native-gameplay-results.md#supply-pod-removal-and-refitting) also passes with a clean strict log and exact restoration of original user saves.

The fresh aggregate passes **601/601 cases**, **19 Python checks** with the original disk, strict import, source startup and Windows cross-export. Independent audit confirms every case log is fresh, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and the ending JSON/audio payload hashes. Export: **163,898,032 bytes**, SHA-256 `5d91e468f97ac7a1d552fabf75ccef015451fc2ebfe720b96869d589121193b6`. Archive: `artifacts/validation/evidence/pod-motion/full-run-3ebeca0/`. Incremental build: zero warnings/errors; the earlier recompilation retains 14 baseline warnings. The existing narrow EditorSettings teardown exemption is unchanged.

This does not resolve the previously retained `!rc_owner` import shutdown or long-session `SwapGCHandleForType` failure. Native Windows execution of this candidate and exact original hardware timing remain pending. The original 48-task counts stay **35 with implementation evidence and four locally accepted research/internal tasks**; previously reported Windows passes and Craig's confirmations remain credited separately.

## Original trade cargo positions — 2026-10-04

A two-line gameplay diff restores original trade selection to the first three module positions, preserving later SCG cargo and the save format. Case 604 reproduces the broader offer before correction, then verifies both boundary layouts, actual Accept/no-offer paths and saved cargo. All 18 affected headless checks (150, 220–235, 604) and native 604 pass strict logs; build has 14 existing warnings and zero errors. See [trade evidence](original-trade-evidence.md#original-position-trading--2026-10-04). The 603-case `bd56658` aggregate/export remains the last complete checkpoint; no new aggregate/export or Windows result is claimed for this correction.

## PTL discovery and captive-colony stock events

The existing research handler stopped after Hyperlight and had no six-system discovery path. Case 605 reproduces PTL remaining locked after the original delay. The correction extends the existing handler, saves its shared countdown and colony cooldown, and reuses pending bulletins. A successful captive-colony event credits 10,000 ground mineral stock up to 50,000; failed selection discovers PTL. Completed research is preserved. Capture, ground repair and station loss now maintain the separate captive flag.

All **37 focused headless cases** (195, 360–375, 430–435, 459–467, 599, 605–608) and **native Mac 605–606** pass strict logs. Tests include real day progression across reload, research already completed/in progress, recapture delay, pending notices, stock limits, unchanged orbital stock/ore veins, bulletin replay without duplicate credit, lifecycle and malformed/legacy saves. Compilation reports 14 existing warnings and zero errors. Evidence: `artifacts/validation/evidence/ptl-discovery/`, including the retained failing reproduction and `focused-audit.json`.

Runtime `f99b462831bff91eb8a3801c437dfba4c09813f9` passes the fresh full **608/608** suite, **19 Python checks** with the original disk, strict import, source startup and Windows cross-export. Independent audit verifies every fresh case log, **1,230 pack entries**, **64 illustration imports**, **zero test resources**, and matching ending JSON/audio payload hashes. Export: **163,901,280 bytes**, SHA-256 `d6a188b94ed3752ad9e59c82d6b2f69694b796d702a06d7127cd1aadaf4d6ba9`. Archive: `artifacts/validation/evidence/ptl-discovery/full-run-f99b462/`. The aggregate's incremental build has zero warnings/errors; the preceding compilation retains 14 baseline warnings.

The initial aggregate attempt exited 1 during editor teardown with `!rc_owner`, after `IMPORT OK` and before any regression. Its complete logs remain in `failed-import-48263/`. A separate strict import then passed, followed by the fresh successful aggregate. No error filter, engine setting or timeout changed. The intermittent shutdown failure remains unresolved; the existing exact EditorSettings teardown exemption does not cover it.

No Windows execution or normal PTL campaign acceptance is claimed. Cockpit installation was still missing at this checkpoint; the follow-up below supplies it. See [source findings and compatibility policy](original-ptl-evidence.md). Existing Windows results remain credited, and the 48-task completion counts do not change.

## Permanent PTL cockpit fitting

The existing cockpit ACC control now fits PTL on DFCC-converted IOS/SCG hulls after research, using the recovered original 24×16 icon and hover text. One local item sets the saved permanent flag; pods, cargo, ACC settings, fuel and drones remain unchanged. Existing bay validity and rogue-crew checks are reused, with explicit UI/ownership gates and duplicate/no-stock feedback.

Case 609 first fails through real pointer input, then passes fitting, duplicate prevention, inventory conservation, save/reload and battle use for both hulls. Case 610 covers research, input locks, stale ships, missing stock and unchanged ordinary ACC fitting. Review reproduced an additional retained-callback defect after hostile capture; the ownership guard corrects it. These are staged prerequisites, not normal research/manufacture acceptance.

All **46 focused headless cases** pass, covering fitting, related bay/assembly/DFCC, hover, pod motion and rogue behavior. Final ownership checks 609/610/547/556 and native Mac 609/610 pass; both native screenshots were inspected. Build: 14 existing warnings, zero errors. Strict import, source startup and Windows cross-export pass. Independent audit confirms **1,232 pack entries**, matching PTL texture/remap payloads, **64 illustration imports** and **zero test resources**. Export: **163,902,608 bytes**, SHA-256 `f71871e09ac4997708a30873013bad3229f5484ba16e1d09b2001d55d5a828e5`.

Evidence and runnable audit: `artifacts/validation/evidence/ptl-fitting/`. Initial import still failed at teardown with `!rc_owner`; its log is preserved, and a separate strict import passes without filter changes. Fixture compile/cursor failures and both failing gameplay reproductions are retained. The shutdown issue remains unresolved. The last complete aggregate remains **608 at f99b462**; no full610 or Windows execution is claimed. Normal PTL campaign acceptance and Windows source/export checks remain open; 48-task counts are unchanged.

## ACC mining endpoint selection

Normal refitting and course selection can put Asteroids at the ACC source (left) endpoint. The panel saves those mineral selections correctly, but mining always read `DestinationItems`, causing an indefinite scan or mining minerals selected for Earth instead. The shared scan branch now selects the current endpoint's list for both mining eligibility and incompatible-cargo departure.

Extended case **307** fails before the fix on the wrong-endpoint mining assertion, then passes both route orientations, rejected small asteroids, saved approaches and incompatible-cargo return without cargo loss. Eleven related cases (305–306, 308–313, 325, 492, 595) also pass; build reports 14 existing warnings and zero errors. Evidence: `artifacts/validation/evidence/acc-mining-endpoint/`. Full aggregate and Windows execution are pending at this checkpoint.

Replaying the untouched normal failure save on the fix visibly mines platinum. The run ends in a real captured-station self-destruct, so successful delivery/reload is **not** claimed; see the [campaign outcome](native-gameplay-results.md#rare-metal-route-and-endpoint-failure). This is additional evidence for existing ACC tasks, not another completed Asana row.

## Ending input regression correction

The full 610-case attempt at `c8ac3d7` passed cases 1–574, then failed case 575 because playback restarted during the supposed held-button interval. It did not reach startup/export. Preserve that failed run in `artifacts/validation/evidence/acc-mining-endpoint/failed-full-c8ac3d7/`.

Diagnostic traces reproduce the failure and show the injected press never reached `Input`: the button remained false even on nominally passing runs. Godot 4.2.2 [buffers parsed events](https://github.com/godotengine/godot/blob/4.2.2-stable/core/input/input.cpp#L878-L928), while its [headless display server](https://github.com/godotengine/godot/blob/4.2.2-stable/servers/display_server_headless.h#L116) does not pump them. The old one-frame assertion could pass before replay began.

Case 575 now flushes press/release, asserts actual button state, waits for audio completion and the output-latency interval, checks black pixels while held, and checks restart after release. Cleanup releases input even on failure. Case 574 sends Escape through the viewport so it reaches the ending's input handler in headless mode. Removing each production input guard temporarily makes its respective corrected test fail; the guards are restored unchanged. Evidence: `artifacts/validation/evidence/ending-held-replay/`. These are test corrections, not additional resolved Asana tasks.

All 14 ending/transmitter cases (573–586), ten additional isolated repetitions of case 575, and native-window cases 574–575 pass with strict logs. Build: 14 existing warnings, zero errors. Production ending code is unchanged. A fresh full aggregate remains required.

## 610-case aggregate and package audit

Candidate `e6e5ccb4480f2a0d0df2dc3de02cad57a0e88128` passes the complete pinned `python3 scripts/validate.py --export-windows` command with exit zero: **610/610 isolated engine regressions**, strict import, startup smoke and Windows cross-export. All **19 Python checks** pass with the original ending disk. The gameplay includes the asteroid-source ACC correction and PTL fitting; the ending follow-up corrects tests without changing production ending code.

An independent audit checks contiguous discovery IDs, every fresh per-case log, strict diagnostics, startup/export order, unchanged tested runtime files and the protected `AGENTS.md`. The export has **1,232 entries**, **64 illustration imports** and **zero test resources**; ending JSON/audio payload hashes also agree. Executable: **163,902,624 bytes**, SHA-256 `cd4aa6f8089bbd375d7bbd74171d0cec65066acd1cda64707754be0a7cfd1b92`. Build retains 14 existing warnings and zero errors.

Archived evidence, reproducer and SHA manifest: `artifacts/validation/evidence/ending-held-replay/full-run-e6e5ccb/`. The earlier failed 575 run remains preserved. Previous long-session/native teardown failures remain unresolved; this pass does not erase them. Windows remains at its independently audited 595-case source/export checkpoint plus the six reported task scenarios and three Craig-confirmed fixes. This candidate has not executed on Windows, and campaign/48-task acceptance remains open.

The [normal asteroid-source expedition](native-gameplay-results.md#normal-asteroid-source-acc-delivery) subsequently delivers 500 palladium, stops on Complete Cycle and retains the delivery after reload. The native log is clean and original saves are restored. This supplements case 307 with ordinary Mac gameplay; it does not add a fully accepted Asana task or Windows run.

## Asteroid fuel and exhaustion

The new source-backed correction charges scanning/mining fuel on the shared saved update phase, charges asteroid approach on alternating counter values, and preserves exhaustion-specific ACC/countdown/loss behavior. Existing zero-fuel scanners/miners survive; empty arrivals strand for six updates; exhausted approaches cannot begin mining. [Implementation and source limits](original-ama-mining-evidence.md#asteroid-fuel-and-exhaustion-implementation--2026-10-04).

Cases 611–614 failed before production changes for the intended fuel/countdown reasons. Case 615 separately reproduced the incorrect “Falling” status. Case 616 rejects a removed save-validation guard and reproduced an overdue legacy approach remaining stuck before its migration correction. All six now pass focused headless checks, with existing cases 307/325/492/595 passing after the two source-backed expectation corrections. Evidence is retained under `artifacts/validation/evidence/asteroid-fuel/`; the broader focused run passed all **71 cases** with strict log checks. All six new native-window cases also pass with strict logs, and the scanning/stranded screenshots were inspected. The subsequent 616-case aggregate is audited below. Normal Mac scanning, mining 165 titanium, mid-departure reload, return delivery and a second reload pass with strict exit and restored user saves; see [normal fuel evidence](native-gameplay-results.md#normal-asteroid-fuel-and-reloaded-departure). No Windows run or task completion is claimed.

The first full attempt at `d7c2ef3` stopped during import after `IMPORT OK`, with `FATAL: Condition "!rc_owner" is true` in Godot's C# reference callback during editor teardown. No regression cases ran in that attempt. Its terminal exit was 1 and logs are preserved in `asteroid-fuel/failed-import-d7c2ef3/`. A separate strict import check passed without that fatal diagnostic; the fresh 616-case full run passed. No new exemption or timeout change was added, and the intermittent teardown fault remains unresolved.

## 616-case aggregate and package audit

Candidate **`d7c2ef37d73e394524eb5e7eb3c5117be7c6a48c`** completed a fresh full run with exit zero: **616/616 isolated engine cases**, strict import, source startup and Windows cross-export. The separate Python run passed **19/19 with the original ending disk**, without a skip. This follows the preserved first-attempt import failure above; its intermittent `!rc_owner` teardown cause remains unresolved.

The independent audit checks all 616 distinct fresh logs, complete discovery, success/error markers, source identity and unchanged `AGENTS.md`. The package contains **1,232 entries**, **64 illustration imports**, **zero test resources**, and verified ending JSON/audio payload hashes. Export size is **163,904,144 bytes**, SHA-256 `6b79dd11bd8ef626ca355b9dc7c3171c1a6b9ded919ce9d46fafce66d353cf7f`. Archived logs, environment/export metadata, pack listing, runnable audit and SHA manifest are at `artifacts/validation/evidence/asteroid-fuel/full-run-d7c2ef3/`.

All six new native cases pass; the inspected status captures distinguish scanning from stranded arrival. The [normal expedition](native-gameplay-results.md#normal-asteroid-fuel-and-reloaded-departure) separately verifies actual scanning fuel, mining, partial-cargo departure, mid-launch reload, delivery and final reload with protected original saves. These do not establish Windows execution of this candidate: Windows595, six reported task scenarios and three Craig confirmations remain credited at their recorded revisions. Counts remain **35/48 with implementation evidence and four locally accepted research/internal tasks**.

## ACC activation during mining — 2026-10-04

The new case **617** failed before the fix because Engage discarded a manually mined asteroid. Both commands from Off also treated mining as a refuelling berth because the remake represents it as `Docked`. The shared activation now preserves the scan and pending action, and only starts immediate resupply outside the asteroids. [Original handler evidence](original-acc-cycle-evidence.md#changing-acc-commands-during-asteroid-activity--2026-10-04) establishes the state distinction.

Build succeeds with 14 existing warnings and zero errors. **91 targeted cases pass**, covering ACC ownership, routes, balancing, fuel waits, asteroid transitions and the new 32-combination activation check. Normal Mac play from the unmodified expedition save then verifies Off → Engage and Off → Complete Cycle while mining a naturally generated class-7 silver asteroid. Cargo grows from 52 to 93 after Engage, then to 144 after Complete Cycle, UI save/load and further consumed updates; fuel stays 239 and the ship remains mining. Native exit is zero with a strict clean log; original saves are restored byte-for-byte.

Raw before/after logs, screenshots, three normal saves and the runnable audit are under `artifacts/validation/evidence/acc-asteroid-activation/`. This is targeted verification, not a new full-suite/export or Windows result. The previous full aggregate remains **616 at `d7c2ef3`**; Windows595, six reported gameplay passes and Craig's three confirmations remain credited. No new task is counted accepted.

## Resuming paid manual production — 2026-10-04

The review found that selecting a suspended manual job checked for another full recipe in stores, although its materials had already been deducted when it entered the queue. With precisely enough materials for two orders, switching between them left the first impossible to resume. This also survived save/load. The manual start path now recognizes a queued job as already paid; a new order still needs its recipe, and existing location, station-installation, staff and rank gates remain in force.

New case **618** reproduces the failure through Production controls before the correction. Ground and orbital variants each pay for a Derrick and Supply Pod, save/load, resume with empty stores, reject an unpaid Tool Pod, and finish exactly one of each paid product without another charge. **74 targeted cases** and native-window case618 pass with strict logs; build has 14 existing warnings and zero errors. An initial fixture error omitted distinct research-order values; that setup failure is archived separately from the valid failing-before reproduction.

Original disk-1 trace independently supports this correction: `$24F72–$24FA0` reads the saved per-product progress nibble and bypasses `$25034` recipe payment when nonzero. `$25074–$250C4` returns a removed team to a free staff slot and calls the shared pause routine, preserving product progress rather than cancelling paid work. The AOC scheduler uses the same saved-progress distinction at `$23584–$235CE`. These are source findings, not original-runtime acceptance. Reproduce with `uv run --offline --with capstone==5.0.7 python artifacts/validation/evidence/paid-production-resume/verify-original.py` against the locally preserved disk.

Evidence and runnable audit: `artifacts/validation/evidence/paid-production-resume/`. This is an additional review correction, not another completed Asana task or Windows result. Separate accounting paths still need follow-up: removing a manual team clears paused paid jobs while refunding only the active one, and AOC conversion retains manual queue entries before the automated scheduler charges for activation. Those are source-review findings, not reproduced or fixed by case618.


## Removing a production team preserves paid work — 2026-10-04

Removing a manual production team cleared every queued job but refunded only the active recipe, losing the materials and completed stages of paused jobs. The original staff-removal routine `$25074–$250C4` instead returns the team to a free local slot and pauses work through `$24E16`. The shared removal handler now preserves the paid queue, pauses its active job at the existing stage boundary, and leaves materials reserved for resumption.

The same action rebuilt product controls while retaining a deleted selected-button reference. Clicking another product after deletion raised `ObjectDisposedException` in `ProductionButton.Redraw`. Removal now clears that selection before rebuilding. The strict log check caught this error even though the earlier test's success marker was printed.

New case **619** reuses the paid-production scenario: ground/orbit, full staff quarters rejecting removal, actual progress before removal, unchanged reserved materials, working replacement controls, save/load, team reassignment, and exactly one completion per paid order. The material-loss and disposed-button failures are preserved separately. **75 related cases**, native-window case619, and strict log audit pass; build reports 14 existing warnings and zero errors.

Evidence: `artifacts/worktrees/production-queue/artifacts/validation/evidence/team-removal/`, including `audit.py`. This is staged control-level verification, not normal campaign or Windows acceptance. AOC conversion's separate paid-queue accounting remains under review; no new Asana task is counted accepted.


## 618-case aggregate and package audit — 2026-10-04

The combined ACC asteroid-activation and paid-job resumption revision **`89a2659`** passes all **618/618** fresh-process game regressions, **19/19 Python checks** including the original ending disk, strict editor import, source startup and Windows cross-export. The aggregate exits zero. The existing narrowly recognized editor-settings teardown diagnostic is recorded during export; no additional error exemption was introduced.

The independent audit checks every fresh case log, discovery/count agreement, strict build/import/startup/export logs and the embedded package. It contains **1,232 entries**, **64 illustration imports** and **zero test resources**; ending JSON and audio payload hashes also verify. Export: **163,904,672 bytes**, SHA-256 `022b8be27ae29a399b54f84e7ee797ab6811b8f72e8a50267b1128df099eb3a4`.

Evidence: `artifacts/validation/evidence/paid-production-resume/full-run-89a2659/`, with `audit-full.py` beside the archive. The later staff-removal fix is integrated as **`cef95c0`**, runtime-identical to its isolated `1dc8936` checkpoint, and has the separate 75-case/native619 verification above. Its integrated source build, case619 and strict startup also pass. The full aggregate and exported executable remain revision89a2659 evidence, not a full619 or Windows-execution claim. Windows595, six reported desktop scenarios and Craig's three confirmations remain credited; no new Asana acceptance is claimed.

## Paid work through AOC conversion — 2026-10-04

The production review found that a paid manual job left behind when constructing an AOC could not be selected through the automated controls. With spare materials, the scheduler could also start it without selection and charge the recipe again. Original Disk 1 separates the selection bytes (`$23510–$23522`, `$24EA8–$24EB6`) from saved progress (`$235A4–$235CE`): only selected jobs run, and nonzero progress bypasses payment. The disk hash and instruction operands are checked by the retained `verify-original.py`.

Runtime **`3f67fd0`** (isolated **`597806b`**) records whether a queued recipe is paid, independently of one-time/repeat/off selection. Deselecting a paid pending job preserves its stages and reservation. Selected paid work resumes with empty stores; later repeat cycles still charge each new recipe. Older saves infer manual reservations and active work from their existing state; old unstarted AOC selections still require payment. The optional field is backward-readable by this build, not forward-readable by old executables.

New case **620** fails before the fix through actual Production controls. It constructs the AOC normally with staged research/staff/material prerequisites, then checks initial and accumulated progress, empty/spare stocks, selection cycling, old/new reloads, exact one-time output and newly paid repeats. **179 related engine cases and native620 pass**, with strict logs. The existing MTX ordering fixture needed an explicit AOC selection; its original failure is retained, and its material-consumption assertions remain intact. These are controlled regression checks, not normal campaign or Windows acceptance.

Before/after logs, the fixture failure, original trace, selected-case inventory and runnable audit are under `artifacts/worktrees/production-queue/artifacts/validation/evidence/aoc-paid-queue/`. Integrated build/case620/startup results are under `artifacts/validation/evidence/aoc-paid-queue/integrated/`. The latest complete aggregate/export remain **618 at `89a2659`**; no full620 or Windows run is claimed. Backlog totals remain **35/48 implementation evidence and four accepted research/internal requirements**.

## AOC completion and full staff quarters — 2026-10-04

Runtime **`e313b6d`** (isolated **`2e798fd`**) fixes a reproduced `IndexOutOfRangeException` when an AOC finishes while all four local staff slots are full. The prior sequence also produced transferable AOC stock and announced completion before returning its team. Original Disk 1 `$2336C–$233AE` checks four quarters, returns without completing stage three when full, otherwise moves the same team and installs automation while bypassing ordinary stock output. `$23530–$23542` removes automated AOC selections. The retained instruction verifier checks these operands against the known disk hash.

The fix reuses station-installation handling. Waiting preserves the paid job, crew and progress without output, repeated experience or notifications. Once a slot is free, completion returns the team and installs the AOC before notifying observers. The UI marks it installed; new duplicate orders and obsolete saved repeat orders are rejected. Other staff slots remain untouched.

**29 targeted cases and native621 pass**, including manual/AOC production, MTX/SDM installation, ordering, paid queues and the new full-capacity case. Case621 covers waiting and completed save/load, precise staff return, one completion/experience award, no transferable stock, panel visibility and duplicate rejection. Raw failures, original trace/verifier and passing logs are at `artifacts/worktrees/production-queue/artifacts/validation/evidence/aoc-staff-capacity/`. These staged checks do not establish normal campaign or Windows acceptance. A complete **621-case** run with import/startup/export audit is underway; it is not yet a result.

## MTX preset and indirect crew creation — isolated follow-up

While the main checkout's full621 run remained unchanged, caller review found that Enable MTX can invoke orbital setup → station setup → shuttle setup. The existing full-roster preflight covered preset choices0–3 but excluded MTX (choice4). Actual Settings confirmation with four non-marine teams reproduced an `IndexOutOfRangeException` after partial world mutation.

Isolated commit **`53ee18d`** extends the existing preflight only when MTX needs that indirect crew-creation path. Case622 verifies rejection before any saved-world change and actionable feedback. Seven existing station parts bypass shuttle setup, so MTX can still complete with a full roster while preserving all teams and the name-allocation index. **11 related checks and native622 pass**. Raw failures, passing logs and a runnable audit are under `artifacts/worktrees/production-queue/artifacts/validation/evidence/mtx-preset-capacity/`.

This change is committed in the isolated worktree and **awaits integration after the full621 run**. It is not included in runtime `e313b6d`, its pending export, or any Windows result. Normal-play acceptance remains open; no additional Asana task is counted complete.

## 621-case aggregate and package audit — 2026-10-04

Frozen runtime `e313b6d` passed all **621 isolated regressions**, 19 Python checks with original-disk verification and no skips, compilation, strict import, source startup and Windows cross-export. Run: 10:12:27–10:29:29 UTC. Independent audit checked fresh individual logs, contiguous discovery, engine diagnostics, unchanged runtime and unchanged root `AGENTS.md`.

The export is **163,904,848 bytes**, SHA-256 `5ffae63f732259ca26577f5fe424339954e179a303ff472f2f51ee24d0ec5ad7`. Its 1,232 pack entries contain 64 illustration imports, alien font and verified ending JSON/audio payloads, with no test resources. Logs, audit, pack listing and hash manifest: `artifacts/validation/evidence/aoc-staff-capacity/full-run-e313b6d/`. This includes staff-removal, paid AOC conversion and staff-capacity fixes; subsequent MTX preset and Windows v5 integration checks are separate. It is a Mac-produced Windows export, not Windows execution evidence.

## Windows v5 integration — 2026-10-04

Runtime **`01c81f9`** integrates the received SCG pointer fix, Stocktaker clock hit testing, unavailable-pod feedback, automatically sized error panel, module-background return and mini-screen refresh while expanded. The newer guarded Service implementation, original planet/station palettes, storm doors, message colour fix and six SCG mounts are retained. The blue-only atlas was superseded by existing researched artwork. Delivered tests were remapped to 623–627 and updated to wait for pod animation; the MTX preset capacity check is 622 (`d301a19`).

**50 focused headless cases and seven native Mac cases pass**, plus compilation, strict import, startup and an audited cross-export. Native cases: 112, 263, 623–627. The focused source tree at `cad14ea` is byte-identical to integrated runtime `01c81f9`; root build/import/startup/export were then repeated. An initial test adaptation referenced a nonexistent blocker property and failed compilation; that attempt remains preserved. No engine-error exemptions were added. This is not a full627 aggregate or Windows execution claim.

The complete executable embeds its .NET dependencies: **163,905,616 bytes**, SHA-256 `88c7019b07ab260518dee1806d4fb438b0c6e16a66c9b15bd423517ba898df2d`. Audit verifies 1,232 pack entries, 64 illustration imports, no tests, and ending/game-assembly payload hashes. Evidence: `artifacts/validation/evidence/windows-v5-integrated/`. Original user-save hash remains unchanged. Windows matching-export acceptance and normal campaign progression remain open; totals stay 35/48 implementation evidence and four locally accepted requirements.

## SDM bulletin correction and incomplete aggregate — 2026-10-04

Runtime `929b4a5` pauses SDM countdown consumption while mandatory bulletin playback blocks input. Extended case 370 reproduces station destruction before the correction; 27 focused cases pass afterwards. A normal funded campaign manufactured 162 drones, defended Moon and Earth and cleared Jupiter. Replaying the unchanged post-battle checkpoint in the corrected build captured Jupiter and preserved exact ships, planets and News through reload. Both native sessions exited zero with strict logs and restored original saves. See [campaign evidence](native-gameplay-results.md#funded-fleet-two-defences-and-first-station-capture--2026-10-04).

The subsequent **632-case aggregate failed**, despite case 264 passing its navigation assertions: shutdown retained `AudioStreamPlaybackOggVorbis`, `OggPacketSequencePlayback` and the `ShuttleBay.ogg` stream/packet resource. Cases **1–263 passed strictly**. Cases 265–632, startup and export were not reached in this attempt. All 19 Python checks passed separately without skips. No error exemption or retry for a green result was added. The failure archive is `artifacts/validation/evidence/full-run-929b4a5-failed/`; start the next session by tracing case 264 and `GameCore.RequestQuit` teardown. Its relationship to earlier shutdown faults is unproven. The last complete aggregate/export remains 630 at `003c087`.

### Unchanged shutdown baseline — 2026-10-05

A predetermined four-attempt repetition of case 264 at unchanged runtime `929b4a5` passes assertions, strict diagnostics and process exit in **4/4 attempts**. This establishes intermittency; it does not repair or invalidate the preserved full-run failure, and no passing aggregate is claimed. Logs: `artifacts/research/audio-shutdown-20261005/baseline-264-{1..4}.log`. Production code and error filters remain unchanged. The separately collected Windows native-binding crash report is recorded in [Windows results](windows-validation-results.md#retained-crash-evidence-and-windows-resume--collected-2026-10-05).

## Remaining cases and current Windows package — 2026-10-05

At unchanged runtime `929b4a5`, a separate continuation passes **cases 265–632 (368 cases)**, fresh build/import, source startup and Windows cross-export. All **19 Python checks** pass with original-disk verification and no skips. The original attempt's cases 1–263 and failed case 264 remain preserved. Four unchanged diagnostic repetitions of 264 pass, but its intermittent teardown failure remains unresolved: **this is not a passing full 632-case aggregate**. No production code or error filter changed during the continuation.

The independent package audit verifies all **1,232 embedded payload hashes**, 64 illustration imports, no test resources and source-identical ending JSON. Executable: **163,906,784 bytes**, SHA-256 `34756594b605d8673dd0fd9795592baa60466505de2fa6f55e0b46a270905a13`. Logs, executable and runnable audit: `artifacts/validation/evidence/continuation-929b4a5-20261005/`. The original user save remains byte-identical. Native Windows execution and its separate binding-callback crash remain open.
