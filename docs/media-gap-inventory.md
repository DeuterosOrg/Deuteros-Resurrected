# Sound and animation gap inventory

Source audit: **2026-10-02**, current C# checkout. This supplies the inventories requested by [1215683087492491 — missing sounds](https://app.asana.com/0/1214891399253076/1215683087492491) and [1215683087492495 — missing animations](https://app.asana.com/0/1214891399253076/1215683087492495). Both descriptions request a list followed by tasks to source and implement it; neither has comments, subtasks, or a specific cue/frame list. The work items below are local implementation tickets; no Asana tasks were created or marked complete.

**Status terms:** *wired* means an asset or procedural renderer has a scene and controller path; it does not certify audible/visual fidelity. *Unused* means a supplied runtime asset has no matching consumer. *Missing export* means the runtime asks for an absent image despite candidate source artwork. *Missing wiring* means a required trigger/controller is absent or incomplete. *Unverified* means original behavior or asset identity is not established. No game/emulator session or audio listening test was run for this inventory.

## Sounds

All **11** runtime audio files were checked against scene references and dynamic paths: seven background OGGs, three button WAVs, and one typing WAV. Eight have playback paths; three are unused. The OGG import settings all enable looping. [`BackgroundSound.cs`](../Godot/Code/Platform/BackgroundSound.cs) resolves the `BackgroundSound` enum to `Sounds/Background/{name}.ogg`, plays on `_Ready`, and stops on exit.

| Supplied asset under `Godot/Sounds/` | Runtime evidence | Status |
| --- | --- | --- |
| `Background/Earth_Ground.ogg` | `Screens/Earth/Ground.tscn` instances `PreFabs/Sound/BackGround.tscn`, default enum 0 | Wired |
| `Background/Earth_Training.ogg` | `Screens/Earth/Training.tscn`, enum 1 | Wired |
| `Background/Production.ogg` | `Screens/Production.tscn`, enum 2 | Wired |
| `Background/Resource.ogg` | `Screens/GroundMaterials.tscn`, enum 3; this is the mining screen, not deposit analysis | Wired |
| `Background/Research.ogg` | `Screens/Earth/Research.tscn`, enum 4 | Wired |
| `Background/ShuttleBay.ogg` | Enum 5 exists; `Screens/ShipBay.tscn` has no audio node/controller | **Unused; S1** |
| `Background/Store.ogg` | Enum 6 exists; `Screens/Store.tscn` and its MTX child have no audio node/controller | **Unused; S1** |
| `Button/sMainMenu_Button.wav` | No resource reference or dynamic load in the C# project; menu scripts do not play sound | **Unused; S2** |
| `Button/sTrainingRoom_Button.wav`, `Button/sTrainingRoom_Door.wav` | `Screens/Earth/Training.tscn/SoundPlayer`; `Screens/Training.cs` loads both and switches/plays them in button/door callbacks | Wired; check synchronization with door animation |
| `Typing.wav` | `Screens/Bulletins.tscn/TypeSound` + `Bulletins.cs`; `PreFabs/ShipModuleWindows/ModuleTextFrame.tscn/AudioStreamPlayer` + `ModuleTextFrame.PlayText` | Wired per-character feedback |

| Local work item | Gap and concrete delivery | Acceptance / evidence still needed |
| --- | --- | --- |
| **S1 — Wire Store and Ship Bay ambience** | Reuse the two existing OGGs and background prefab, choosing enum Store/ShuttleBay in their scenes. No new recording is needed. | Enter ground/orbital stores and bays; switch to/from MTX; verify one loop, exit stop, mute, no overlap, and clean native shutdown. Existing [audio shutdown blocker](validation-results.md) remains relevant; adding consumers does not resolve it. |
| **S2 — Wire menu click feedback** | Use existing `sMainMenu_Button.wav` from real menu controls (`Platform/MenuButton.cs`, `SceneChangeButton.cs`, `Screens/Base/MenuBase.tscn`). Keep keyboard/controller activation consistent with pointer activation. | Establish the intended set of controls; verify exactly one click per accepted activation and none for disabled controls. The asset is supplied, but original correspondence is unverified. |
| **S3 — Source original status, save and encounter cues** | No dedicated status/accept/reject/save/encounter assets or corresponding audio calls were found in `SaveScreen.cs`, `ShipInterior.cs`, module dialogs, or `Battle.cs`. Archived original findings identify sound IDs/callers below; map and extract those samples before choosing substitutes. | Record/export the sample with source address, sound ID, rate/channel data and trigger. Compare save confirmation, rejection, task completion and Methanoid accept/cancel/end separately. Do not label an alert as a weapon/explosion. |
| **S4 — Resolve uncovered screen/action sound requirements** | `Overview`, `Station`, `ResourceMap`, `ShipInterior`, `IntroScreen`, `News`, `SaveScreen`, and battle have no dedicated ambience player; `News` can open a typing bulletin. Ship launch/landing, fuel/cargo/grapple/AMA/MTX actions, research/production completion and combat have no dedicated effect calls. This is an implementation coverage gap, **not proof that each original action needs a distinct sound**. | Capture one original session through those states and make a cue sheet (trigger, silence versus cue, loop, interruption, sample identity). Split only confirmed cues into sourcing tickets. Do not automatically spread Earth ambience across every screen. |

The parallel checkout at `WizzoUK2/deuteros-parallel` has **byte-identical copies of all 11 files**. Its [`ui/audio_manager.gd`](https://github.com/WizzoUK2/deuteros-parallel/blob/ebaeb61a973114e30d97fa65fb0b672f7c6fc1a9/ui/audio_manager.gd) wires Store/Ship Bay/menu sounds, but describes other screen mappings and volume trims as port choices. It is a useful wiring example, not an independent original-game specification or a source of extra samples. Its CC0 comment alone is not a provenance/license audit.

Original archive leads (read from local commit `6fabd73bbc7fff0eb08825b2dd6043f618e596a4`):

- [`combat_hunt_v4.md`, sound-driven discovery and corrections](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/combat_hunt_v4.md#L78-L104): dispatcher `$3FBF8`, template table `$3F7FA`; IDs 4/11 button feedback, 50/51 and 71/72 accept/reject, 19 save UI. Its later correction identifies 65 as alert/status, **not combat**. These are research-note identifications, not newly verified sound recordings.
- [`7BE24_encounter_handler.md`](https://github.com/WizzoUK2/deuteros-parallel/blob/6fabd73bbc7fff0eb08825b2dd6043f618e596a4/outputs/decompiled/findings/7BE24_encounter_handler.md#L153-L180): cancel/cleanup calls sound 9; encounter-over calls tone `0x10, 2`. This provides concrete trigger/address leads for S3, but no extracted runtime assets.
- `SourceMaterials/Sounds/Typing.wav` and `SourceMaterials/output.wav` are real WAVs. The latter is unlabeled 48 kHz stereo PCM: its contents/identity were not auditioned and must not be assigned to a missing cue by filename alone.

## Animations and changing graphics

The audit checked all `AnimatedSprite2D`/`AnimationPlayer` scene nodes, controller `Play` calls, dynamic texture paths, timers and procedural battle drawing. There are animated sprites in the menu, training-door prefab and three item-button prefabs; no `AnimationPlayer` scene nodes were found. Texture swaps and procedural drawing below are included, so absence of sprite sheets is not mistaken for absence of animation.

| Area | Asset → scene → controller evidence | Status / required follow-up |
| --- | --- | --- |
| **Time device — A1**, [1215691800680640](https://app.asana.com/0/1214891399253076/1215691800680640) | `Sprites/Buttons/Nav_Animations/Time_Sheet.png` + `Time_Static.png` → `Screens/Base/MenuBase.tscn/Top/Time/TimeAnimation` (8 frames, 5 fps) → `MainMenu.UpdateAnimations()` | **State wiring fixed; eight isolated regressions pass.** The time indicator now follows `TimeSkip`/`TimeSkipDay` independently of selected-screen icons, including click, hold, release, pointer exit and automatic stop. Repeated refresh preserves frame progress; blocked pointer input cannot start the clock. Existing eight-frame/5 fps artwork is unchanged. Native visual acceptance and original cadence remain unverified. |
| **Production rod — A2**, [1215691951441144](https://app.asana.com/0/1214891399253076/1215691951441144) | `Screens/Production.tscn` has a static production background plus small product and construction image. `Production.DrawData()` changes product images only. No rod node, timer, animation or frame driver exists. | **Missing animation wiring/export.** The annotated [visual reference](visual-reference-notes.md) points between the book platform and vertical progress column, not the three product construction stages. `SourceMaterials/SpriteSheets/UI.png` visibly has three small rod-like strips near x544/y384–471, beside the door frames; they are candidates, not verified ordered frames. Match original footage, crop with recorded coordinates, then wire manual/AOC active and idle states without changing production timing. |
| **Product construction stages — A3** | `Production.DrawData()` dynamically loads `Sprites/Items/Production/{ItemType}_{Production_Complete}.png`, plus `Sprites/Items/Research/{ItemType}.png`; `SpriteManager.LoadImage()` directly calls `GD.Load`, with no missing-image fallback. Existing items have `_1`, `_2`, `_3` progress exports. | **14 producible item types lack all three progress exports**, listed below. This is separate from A2. Candidate source sheets exist. Export exact runtime filenames and exercise each stage, idle, completion and AOC, with strict missing-resource logs. |
| Training doors | `PreFabs/TrainingDoors.tscn` has `open`, `closed`, `opening`, `close`; three instances in `Screens/Earth/Training.tscn`; `Training.DrawData()` plays transitions from model lock changes; completion handler settles state/unlocks UI | Wired; original cadence and concurrent-door/audio synchronization unverified. |
| Menu and item indicators | `Screens/Base/MenuBase.tscn` uses seven `Nav_Animations` sheets; `MainMenu.UpdateAnimations()` calls `Play`. Research/Production/Store button prefabs share `Research_Animations` frames, driven by their respective `*Button.cs` scripts. | Wired; production currently selects static states while research/store can blink. Do not declare every supplied blink sequence required/implemented: original active/queued/selected rules need comparison. |
| Ship bay and MTX movement | `ShipBay.ScrollToScreen()` tweens the component strip; `MTX.cs` creates scrolling tweens on arrow actions | Wired movement, not missing sprite animation. Verify view arrival and interrupted/repeated scroll; the entry jiggle has a separate regression fix. |
| Ship travel, grapple, AMA and overview | `ShipInterior.UpdateState()`/location drawing swap fixed location images from `Sprites/SceneSprites/Ships/Interior`; `Grapple.UpdateState()` and `AMA.UpdateState()` select static asteroid images; `Overview.UpdateState()` selects status icons and station `ProdCycle` textures | State graphics wired; no continuous flight/asteroid sequence established. Record original launch/dock/travel, grapple grab/release and mining transitions before asserting missing frame art. |
| Battle and text | `PreFabs/ShipModuleWindows/Battle.tscn/Timer` (0.06 s) → `Battle.TimerTimeout()` → `BattleCanvas.QueueRedraw()` → `BattleLogic.BattleTick()` draws battle effects procedurally. `Bulletins.cs` and `ModuleTextFrame.PlayText()` reveal text with delays. | Wired procedural animation/typewriter paths; no basis to demand replacement sprite sheets. Original fidelity, interruption and full combat-state coverage remain unverified. |

### A3: exact missing construction exports and candidate sources

Every row lacks `Godot/Sprites/Items/Production/<item>_1.png`, `_2.png` and `_3.png`. Source paths are under `SourceMaterials/SpriteSheets/`; the table maps names, not approved crop/frame order. `item_interplanetary_chassis.png` was visually inspected and contains three distinct construction-stage images; the other mappings require the same crop/palette/order check.

| Runtime item | Candidate source sheet | Small research image also absent? |
| --- | --- | --- |
| `pulse_blaster_laser` | `item_blaser.png` | Yes |
| `i_chassis` | `item_interplanetary_chassis.png` | No |
| `i_drive` | `item_interplanetary_drive.png` | No |
| `g_chassis` | `item_star_chassis.png` | No |
| `star_drive` | `item_star_drive.png` | No |
| `s__d__m` | `item_self_destruct.png` | No |
| `hyperlight` | `item_hyper_light.png` | No |
| `m__t__x` | `item_mass_transceiver.png` | No |
| `m__f__l` | `item_fuzlaser.png` | Yes |
| `r_frame` | `item_resource_frame.png` | No |
| `prejudice_torpedo_launcher` | `item_torpedo_launcher.png` | Yes |
| `star_drone` | `item_drone_star.png` | No |
| `prison_pod` | `item_pod_prison.png` | Yes |
| `sonic_blaster` | `item_blaster.png` | Yes |

Cross-check used item definitions in `CoreData.CreateBaseGameData()` and the loader above. `meh_fuel`/`hed_fuel` are `AutoProduce`, so their absent stages are not counted as manual production gaps. `alien_artifact` is a special research/collected item, not counted as an ordinary recipe. Five missing small images should be exported to `Godot/Sprites/Items/Research/<item>.png` alongside the stages; they also affect research display.

### Related static artwork, kept distinct

[Visual references](visual-reference-notes.md) separately cover AOC plaque (1215691800680642), Methanoid menu icon (1215691800680621), moon-base unavailable buttons (1215691800680623), station stats (1215691800680634), and deposit-analysis platform (1215691800680646). `UI.png` visibly supplies an AOC plaque and framed Methanoid image; `ui_buttons.png` is another source candidate. Their presence does not prove they are sliced, scene-bound or shown under correct conditions. `SourceMaterials/Notes.txt` additionally flags the IOS landing-arrow variant and small ship-view border as missing exports; recheck current `ShipInterior.tscn/LandingBlank` and location layers before creating art. These are static graphic/state tasks, not additional claimed missing animations.

The ship interior also lacked a valid small location image for moons: default moon data has no `PlanetColor`, so arrival attempted `SmallLocation_Planet_0_Station.png`. The existing interior exports cover White, White_Blue, White_Green, Blue, Green and Yellow; red and asteroid variants are absent. `ShipInterior.UpdateState()` now selects the exact-cased supplied image when available and otherwise uses `SmallLocation_Planet_White.png` or `SmallLocation_Planet_White_Station.png`, preserving the station marker. This is an **existing neutral placeholder, not recovered original moon/red/asteroid artwork**. Isolated regression case 104 (interior travel/arrival menus) reproduced the missing resource, then passed with a non-null White_Station texture and a clean engine log. Source and verify the original variants before claiming visual parity.

## Evidence and handoff limits

The sound cue sheet and original frame cadence remain incomplete: the Asana inventory tasks supply no recordings; still attachments cannot establish motion or audio; repository manual PDFs are LFS pointers (see [original evidence](original-behavior-evidence.md)). The local inventory is actionable without claiming original audiovisual parity. Complete S1/S2 wiring and A3 exports from known assets, while gathering original recordings/address-backed mappings for S3/S4 and A1/A2 timing. Each implementation should retain its own native audio/visual acceptance record; passing headless gameplay tests does not prove playback quality.
