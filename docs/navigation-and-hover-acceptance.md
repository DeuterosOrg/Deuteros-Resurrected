# Navigation and ship bay hover acceptance

Status: both fixes implemented and verified by 13 focused Godot scene regressions (current discovery IDs 54–66). Each final focused run reports `1 passed, 0 failed`, exits 0 and has no logged engine errors. Latest focused build: 0 errors, 14 existing warnings. Full-suite validation is coordinated separately.

## 1215691800680662 — Ship bay hover text

[Asana task](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1215691800680662) explicitly lists: dismantle ship; new IOS; new SCG; remove ship's crew; assign crew to production; fit ACC; access ship; crew section; pod mounts 1–5; engine mounting; empty fuel; fuel ship; install supply, tool and team pods. There are no task comments.

### Baseline and implementation

- `Code/Platform/Base/HoverButton.cs` writes `GameCore.HoverText` on mouse enter and clears it on exit. It derives from **Button**.
- `Code/Platform/Screens/MainMenu.cs:_Process` displays that value in `HoverInfo`.
- Before this change, the listed bay controls had no hover signal handlers, `HoverText` assignments or scene tooltip properties. Most are native **TextureButton** nodes; fuel controls are **RepeatingButton** instances. Attaching `HoverButton` to all of them would change their types or replace existing scripts.
- Crew removal and production assignment are **contextual actions on the four roster rows**, not separate buttons. `StaffList.ButtonPress` sends vacant/marine rows to `PilotChanged`; other occupied rows reach `ProductionChanged`. `ShipBay.CockpitInstance_ProductionChanged` assigns a team only on Earth's ground with an empty factory staff slot.
- `Torso.ChangeModuleType` toggles a matching installed pod to `None`; `ShipBay_ModuleChanged` rejects removing a loaded pod. Hover wording now reflects install/remove state; an occupied matching pod says `Unload pod before removal`.

### Implemented labels and actual controls

Paths below are relative to `Screens/ShipBay.tscn`. `ShipParts` abbreviates `ShipContainer/ScrollContainer2/HBoxContainer`.

| Task action | Control path | Hover text / condition |
| --- | --- | --- |
| Dismantle | `Buttons/Nav_Dismantle` | `Dismantle ship` |
| New IOS | `Buttons/Nav_Create_IOS` | `Build IOS` |
| New SCG | `Buttons/Nav_Create_SCG` | `Build SCG` |
| Remove crew | `ShipParts/Cockpit/StaffList/Staff/Buttons/01`–`04` | `Remove ship's crew` on a vacant roster row when a pilot is aboard |
| Assign production | Same four roster buttons | `Assign crew to production` for a production team on Earth's ground when the factory has no builder |
| Assign pilot (same shared controls) | Same four roster buttons | `Assign ship's crew` for an available marine team and a present ship |
| Fit ACC | `ShipParts/Cockpit/Buttons/AddACC` | `Fit A.C.C.` |
| Access ship | `ShipParts/{Cockpit,Torso1…Torso6,Engine}/OpenShipInterior` | `Access ship` when a ship is present |
| Crew section | `Buttons/ShipNav/Nav_Cockpit` | `Crew section` |
| Pod mount 1–6 | `Buttons/ShipNav/Nav_Torso1`–`Nav_Torso6` | `Pod mount 1` … `Pod mount 6` |
| Engine mounting | `Buttons/ShipNav/Nav_Engine` | `Engine mounting` |
| Empty fuel | `Fuel/FuelGauge/Minus/RepeatingButton` | `Unload fuel` |
| Fuel ship | `Fuel/FuelGauge/Plus/RepeatingButton` | `Fuel ship` |
| Supply pod | `ShipParts/TorsoN/SpriteHolder/Buttons/AddSupplyPod` | `Install supply pod`; `Remove supply pod` if that mount contains an empty supply pod |
| Tool pod | `ShipParts/TorsoN/SpriteHolder/Buttons/AddToolPod` | `Install tool pod`; `Remove tool pod` if that mount contains an empty tool pod |
| Team pod | `ShipParts/TorsoN/SpriteHolder/Buttons/AddCryoPod` | `Install team pod`; `Remove team pod` if that mount contains an empty cryo pod |

The later source-backed six-mount correction makes `Torso6`/`Nav_Torso6` usable and migrates older saves. Include all six mounts in Windows hover acceptance. Engine-installation and pod-activation behavior have separate acceptance checks.

### Implementation and verified coverage

`ShipBay.BindHoverLabels` binds existing controls with a small `Control`-based helper. Label callbacks cover contextual roster and pod state. The current hovered control owns its label; leaving, hiding, disabling or freeing it clears that text. Existing control inheritance and button actions are unchanged. The regular menu displays `GameCore.HoverText`.

The original six hover regressions cover the listed controls for Shuttle/IOS/SCG and the five SCG mounts then available, empty-bay creation controls, marine/production/vacant roster rows, Earth-ground versus orbital production eligibility, pod install/remove/occupied wording, and unchanged fuel/module state when hovering. A separate pointer regression exercises actual GUI hit testing over a texture button, repeating button and roster row, then verifies clearing on disable, hide and scene exit.

The pointer fixture uses the real bay in a `SubViewport`: Godot 4.2 native headless-window hover reads physical DisplayServer mouse state even when `PushInput` delivers GUI motion to the correct control. Instrumentation confirmed that discrepancy. SubViewport input uses injected positions and emits real hover signals; the test does not emit those signals manually. No production pointer workaround was added.

## 1216065854613319 — Right-click overview navigation

[Asana task](https://app.asana.com/1/507237966097081/project/1214891399253076/task/1216065854613319) names ship bay and orbital stores as examples. Its description and comments are empty.

### Reproduced root cause

`Code/Platform/Helpers/InputBlocker.cs:_UnhandledInput` previously contained the only general right-click-to-overview handler. It ran only after GUI input and required an unlocked cursor and at least one non-Methanoid planet whose station has `BuildParts > 0`.

Ship bay contains a scroll container, buttons and background controls with default mouse filtering; Store contains `TradStore`, `ButtonsImage`, `StoreButtons` and `SwitchStoreImage` with default filtering. These can consume pointer events before `_UnhandledInput`. The empty ship-bay background explicitly ignores input, which explains why the behavior can depend on the pointed-at region. Store has no local right-click handler. ShipBay's `_Input` handles only dismissal of locked cargo/equipment/staff panels.

Godot documents `_Input` before GUI dispatch, GUI handling before `_UnhandledInput`, and reverse depth-first node input dispatch ending at the root. Viewport-injected clicks reproduced the failures over ship-bay and orbital-store controls before the fix. [Godot 4.2 input dispatch](https://docs.godotengine.org/en/4.2/tutorials/inputs/inputevent.html), [Control mouse filtering](https://docs.godotengine.org/en/4.2/classes/class_control.html#class-control-property-mouse-filter).

### Implemented fix

Moved the general overview fallback to `GameCore._Input` on the **Master root**, rather than moving it to `InputBlocker._Input`. Descendant scene handlers run first and can consume panel-dismissal/course-selection events. The root then sees an otherwise unhandled right press before GUI controls can swallow it.

The existing station-availability predicate is preserved. Navigation also requires `_screenLocker.Blocked == false`, `GlobalInput.UiLocked == false`, `cursor.IsLocked == false`, no open `OverlayManager` overlay, and an unpaused tree. It does nothing when already on Overview, marks a successful right press handled and performs one scene change. InputBlocker retains GUI/blocked-input behavior; its duplicate navigation branch was removed.

Moving the fallback to InputBlocker's `_Input` would be unsafe: `Master.tscn` places that sibling after `MainScene`, so it would run before screen handlers and could steal clicks intended to close ship-interior windows that do not lock the cursor. Blanket `MouseFilter.Ignore` changes would also break buttons and are unnecessary.

### Verified navigation coverage

Seven regressions inject real viewport input after layout; they do not call `_Input` or `_UnhandledInput` directly:

1. Ship bay: right-click over the interior access region, pod navigation, fuel control and blank background navigates to Overview without fuelling the ship.
2. Orbital Store: item grid, resource list, mode switch and decorative background, plus the MTX alternate display, navigate without changing store mode.
3. Missing/all-enemy stations block navigation; an eligible player station permits it. Right-button release alone does nothing, and Overview is not reconstructed by another right-click.
4. Screen, cursor and UI locks and the Settings overlay prevent navigation; closing/unlocking restores it.
5. Supply, equipment and cryo staff panels consume the first right-click to close. A second right-click navigates.
6. Course, ACC, grapple and AMA interior windows also close before overview navigation. The test asserts each closed modal is actually freed, not merely detached.
7. During the real five-second grapple unload, right-click preserves the screen lock and bay scene. Completion releases the lock and credits exactly 37 units once; navigation then works.

The modal test exposed an additional concrete leak: `ShipInterior.CloseACC` removed the ACC node without freeing it. It now queues the node for deletion and clears its reference. The test's modal-lifetime assertion and strict log check pass.

The Store test also exposed MTX's invalid `new Tween()` and leaked detached row template. Those were corrected in the coordinating agent's MTX change; the Store/MTX navigation run now has no engine errors.

## Evidence and limits

- Before fixes: `/tmp/deuteros-red-54.log` through `deuteros-red-65.log` record missing hover/navigation assertions. The original native-window pointer fixture also required the headless correction described above.
- Final focused results: `/tmp/deuteros-green-54.log` through `deuteros-green-66.log`, except case 59's final result is `/tmp/deuteros-hover-subviewport59.log` (the earlier `green-59` log records its fixture failure).
- Final focused build: `/tmp/deuteros-hover-subviewport-build.log` (0 errors, 14 existing warnings).
- Run through `scripts/validate.py` for discovery and isolated per-case processes; these numeric IDs can change as more cases are registered.

This verifies headless scene behavior and GUI hit testing, not a native Windows visual pass. The independent Godot 4.2.1 intermittent resource-lifetime/audio-shutdown limitation remains recorded; no engine errors were exempted for these tests.
