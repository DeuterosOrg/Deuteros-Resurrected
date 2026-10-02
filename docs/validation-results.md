# Contribution and validation results

Local review on 2026-10-01, against baseline `9817216`, branch `codex/build-tests-and-gameplay-fixes`. No PR, push or Asana status changes have been made.

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
| 1215716464570907 | Dismantling returns pilot, cryo teams, fuel, pod hardware/contents, ACC and drones together. Capacity shortages or held grapple salvage prevent dismantling without losing anything. | Shuttle/IOS/SCG returns, staff capacity, aggregated fuel/store capacity, held salvage; replacement ACC conservation retained. Chassis/engines are not refunded because current installation leaves those stocks unchanged. |
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

The combined diagnostic suite and an earlier isolated case 65 intermittently reported `SwapGCHandleForType: Handle is not initialized`. Later passes do not establish a fix. Fresh-process isolation addresses test-state retention; it does **not** establish that the production resource-lifetime problem is fixed. Strict error detection remains enabled. [Godot issue 112067](https://github.com/godotengine/godot/issues/112067) describes a similar texture-wrapper failure, but its proposed cause is not proven for this project.

A broader navigation smoke (start game → Earth ground → training → Escape/settings → close) previously reproduced Ogg resource leaks or a `!rc_owner` shutdown hang on macOS. Clearing the audio player's stream did not resolve it. An isolated, checksum-verified Godot 4.3 / SDK 4.3.0 comparison compiled and passed the original 26-case suite, but repeated navigation still reproduced shutdown failures. That comparison did not establish an audio fix. The 4.2.2 patch addresses the separate script-registration deadlock; the subsequent shutdown change above addresses observed mixer-disposal leaks. Other causes of the historical fatal errors remain unproven. [Godot issue 89188](https://github.com/godotengine/godot/issues/89188) is related context, not proof of an identical cause.

Fresh-export scene leaks were traced separately to the engine's optional binary scene conversion. Keeping text resources eliminates those export leaks; only the exact documented editor teardown diagnostic is permitted during import/export. All other engine errors fail validation.

Linux/Windows CI has been authored and its YAML parsed locally, but it has not run remotely. Actual Windows gameplay, original-game fidelity, the full original day-event order and long save/load progression are not certified by this suite. Before release, exercise the Windows executable and the affected screens on Windows.

## Follow-up priorities

Continue through the remaining backlog, including Windows ACC activation reproduction, news, missing art/features and Windows acceptance. Deposit/store-cap conservation and bounded overview rendering remain separate review findings. Resolve original-game evidence questions before changing economics, travel, combat or timing. The parallel port's decoded research is useful; its documented provisional behavior is not an acceptance specification.

## Persistent menu click feedback — 2026-10-02

The previously unused supplied `sMainMenu_Button.wav` now plays through one persistent `MenuBase/MenuClickSound` player. Top and side menu controls use accepted activation; hold-to-advance uses press only. Binding before child navigation callbacks preserves the accepted cue when a scene change immediately disables the clicked side-menu slot. Disabled, hidden, empty, paused or globally locked controls are silent. Repeat tree entry retains one connection set; the player stops on exit. Existing training sounds and title-screen controls retain their behavior.

Cases **212–219** all failed before implementation because the player was absent, then passed focused strict checks. They exercise actual viewport pointer/key input, the `ui_accept` action, scene changes, cancelled/disabled input, time toggle/hold/release, input blocking, pause/mute and repeated tree entry. Native Mac cases **212/216–219** also passed with clean process exit. The automated action event is not evidence of physical controller testing. Build: 14 existing warnings, zero errors. Five Python validator tests pass.

**Full validation passed 219/219 isolated cases in one run**, strict import, startup smoke and a fresh Windows cross-export. The exported pack directory contains the menu scene, WAV import and imported sample. The exact known editor teardown diagnostic remains limited to import/export; no gameplay error filters changed. This passing run does not independently establish the root causes of earlier case-65 GC-handle or case-150 shutdown failures. Raw red/green/native logs and the full-run driver log are retained under ignored `artifacts/validation/evidence/menu-sound/`; previous validation logs and export were moved aside first. Native Windows listening, levels, physical input and original cue fidelity remain pending. Wiring all 11 supplied runtime audio assets does not complete the original cue inventory (S3/S4), or increase the 27/48 Asana-linked implementation count.
