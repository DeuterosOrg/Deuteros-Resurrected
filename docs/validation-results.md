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

## Known runtime limitations

The combined diagnostic suite, and now isolated case 65 in the 2026-10-02 full run, intermittently report `SwapGCHandleForType: Handle is not initialized`. Fresh-process isolation addresses test-state retention; it does **not** establish that the production resource-lifetime problem is fixed. Strict error detection remains enabled. [Godot issue 112067](https://github.com/godotengine/godot/issues/112067) describes a similar texture-wrapper failure, but its proposed cause is not proven for this project.

A broader navigation smoke (start game → Earth ground → training → Escape/settings → close) reproduces Ogg resource leaks or a `!rc_owner` shutdown hang on macOS. Clearing the audio player's stream did not resolve it. An isolated, checksum-verified Godot 4.3 / SDK 4.3.0 comparison compiled and passed the original 26-case suite, but repeated navigation still reproduced shutdown failures. Therefore no engine upgrade or speculative audio/cache change is included. [Godot issue 89188](https://github.com/godotengine/godot/issues/89188) is related context, not proof of an identical cause.

Fresh-export scene leaks were traced separately to the engine's optional binary scene conversion. Keeping text resources eliminates those export leaks; only the exact documented editor teardown diagnostic is permitted during import/export. All other engine errors fail validation.

Linux/Windows CI has been authored and its YAML parsed locally, but it has not run remotely. Actual Windows gameplay, original-game fidelity, the full original day-event order and long save/load progression are not certified by this suite. Before release, exercise the Windows executable and the affected screens on Windows.

## Follow-up priorities

Continue through the remaining backlog, including Windows ACC activation reproduction, news, missing art/features and Windows acceptance. Deposit/store-cap conservation and bounded overview rendering remain separate review findings. Resolve original-game evidence questions before changing economics, travel, combat or timing. The parallel port's decoded research is useful; its documented provisional behavior is not an acceptance specification.
