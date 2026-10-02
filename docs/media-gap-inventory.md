# Sound and animation gap inventory

Source audit: **2026-10-02**, current C# checkout. This supplies the inventories requested by [1215683087492491 — missing sounds](https://app.asana.com/0/1214891399253076/1215683087492491) and [1215683087492495 — missing animations](https://app.asana.com/0/1214891399253076/1215683087492495). Both descriptions request a list followed by tasks to source and implement it; neither has comments, subtasks, or a specific cue/frame list. The work items below are local implementation tickets; no Asana tasks were created or marked complete.

**Status terms:** *wired* means an asset or procedural renderer has a scene and controller path; it does not certify audible/visual fidelity. *Unused* means a supplied runtime asset has no matching consumer. *Missing export* means the runtime asks for an absent image despite candidate source artwork. *Missing wiring* means a required trigger/controller is absent or incomplete. *Unverified* means original behavior or asset identity is not established. No game/emulator session or audio listening test was run for this inventory.

## Sounds

All **12** runtime audio files were checked against scene references and dynamic paths: seven background OGGs, three button WAVs, one typing WAV, and the recovered SDM alarm WAV. All twelve now have playback paths; original cue completeness and native Windows listening remain unverified. The OGG import settings all enable looping. [`BackgroundSound.cs`](../Godot/Code/Platform/BackgroundSound.cs) resolves the `BackgroundSound` enum to `Sounds/Background/{name}.ogg`, plays on `_Ready`, and stops on exit.

| Supplied asset under `Godot/Sounds/` | Runtime evidence | Status |
| --- | --- | --- |
| `Background/Earth_Ground.ogg` | `Screens/Earth/Ground.tscn` instances `PreFabs/Sound/BackGround.tscn`, default enum 0 | Wired |
| `Background/Earth_Training.ogg` | `Screens/Earth/Training.tscn`, enum 1 | Wired |
| `Background/Production.ogg` | `Screens/Production.tscn`, enum 2 | Wired |
| `Background/Resource.ogg` | `Screens/GroundMaterials.tscn`, enum 3; this is the mining screen, not deposit analysis | Wired |
| `Background/Research.ogg` | `Screens/Earth/Research.tscn`, enum 4 | Wired |
| `Background/ShuttleBay.ogg` | `Screens/ShipBay.tscn` instances the background prefab with enum 5 for ground/orbital bays | Wired; native Windows listening pending |
| `Background/Store.ogg` | `Screens/Store.tscn` instances the background prefab with enum 6; its MTX child shares that player | Wired; native Windows listening pending |
| `Button/sMainMenu_Button.wav` | `Screens/Base/MenuBase.tscn/MenuClickSound` persists across navigation; `MainMenu.cs` binds top/side menu activation and time hold | Wired; S2 native Windows listening pending |
| `Button/sTrainingRoom_Button.wav`, `Button/sTrainingRoom_Door.wav` | `Screens/Earth/Training.tscn/SoundPlayer`; `Screens/Training.cs` loads both and switches/plays them in button/door callbacks | Wired; check synchronization with door animation |
| `Typing.wav` | `Screens/Bulletins.tscn/TypeSound` + `Bulletins.cs`; `PreFabs/ShipModuleWindows/ModuleTextFrame.tscn/AudioStreamPlayer` + `ModuleTextFrame.PlayText` | Wired per-character feedback |
| `SdmAlarm.wav` | Persistent `MenuBase/SdmAlarm`; selected armed local station owns sound priority; [source and limits](original-self-destruct-evidence.md#alarm-source-and-playback-limits) | Wired; original timing/listening comparison pending |

| Local work item | Gap and concrete delivery | Acceptance / evidence still needed |
| --- | --- | --- |
| **S1 — Wire Store and Ship Bay ambience** | Implemented using the existing OGGs and background prefab. Cases 200–204 cover ground/orbital playback, loop selection, exit stop, MTX continuity and sound preferences; case 205 covers navigation then window close. | Native Mac checks pass. Listen on Windows for loop quality, relative levels and overlap, then repeat window close. The shutdown change releases scenes before draining stopped playback; it does not establish every historical native-failure cause. See [validation results](validation-results.md#store-and-ship-bay-ambience-and-audio-shutdown--2026-10-02). |
| **S2 — Wire menu click feedback** | Implemented for top and side menus, time toggle and hold using the supplied WAV and one persistent player. Cases 212–219 cover pointer/keyboard/accept-action input, scene changes, disabled/empty/hidden/cancelled controls, hold/release, pause/mute and repeat tree entry. | Focused headless and native Mac checks pass; listen on Windows and check physical mouse/keyboard/controller input. Hold sounds once on press; ticks/release stay silent. The title screen and training controls retain existing behavior. Original cue correspondence remains unverified. |
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
| **Production rod — A2**, [1215691951441144](https://app.asana.com/0/1214891399253076/1215691951441144) | `Production.tscn` now overlays the three recovered atlas frames. `Production._Process` drives the original counter at nominal PAL cadence when the selected factory has a current product. | **Implemented; desktop/original acceptance pending.** [Instruction and pixel evidence](original-production-rod-evidence.md) establishes resources 130→129→128, x240/y96, the short initial phase and retained counter. Cases 382–385 pass headless/native on Mac, including 3,456 rendered pixel comparisons. |
| **Product construction stages — A3** | `Production.DrawData()` selects `Sprites/Items/Production/{ItemType}_{Production_Complete}.png` or a recovered `.tres` atlas, then falls back to the existing research illustration if neither exists. | **Nine frames recovered for three items; 11 item types still lack all three progress frames**, listed below. Native pixel checks verify the recovered art and magenta display mask. The other candidate sheets contain placeholders or unverified sequences. Source genuine replacements before exporting them. Construction stages remain separate from the production rod (A2) and do not certify original animation timing. |
| Training doors | `PreFabs/TrainingDoors.tscn` has `open`, `closed`, `opening`, `close`; three instances in `Screens/Earth/Training.tscn`; `Training.DrawData()` plays transitions from model lock changes; completion handler settles state/unlocks UI | Wired; original cadence and concurrent-door/audio synchronization unverified. |
| Menu and item indicators | `Screens/Base/MenuBase.tscn` uses seven `Nav_Animations` sheets; `MainMenu.UpdateAnimations()` calls `Play`. Research/Production/Store button prefabs share `Research_Animations` frames, driven by their respective `*Button.cs` scripts. | Wired; production currently selects static states while research/store can blink. Do not declare every supplied blink sequence required/implemented: original active/queued/selected rules need comparison. |
| Ship bay and MTX movement | `ShipBay.ScrollToScreen()` tweens the component strip; `MTX.cs` creates scrolling tweens on arrow actions | Wired movement, not missing sprite animation. Verify view arrival and interrupted/repeated scroll; the entry jiggle has a separate regression fix. |
| Ship travel, grapple, AMA and overview | `ShipInterior.UpdateState()`/location drawing swap fixed location images from `Sprites/SceneSprites/Ships/Interior`; `Grapple.UpdateState()` and `AMA.UpdateState()` select static asteroid images; `Overview.UpdateState()` selects status icons and station `ProdCycle` textures | State graphics wired; no continuous flight/asteroid sequence established. Record original launch/dock/travel, grapple grab/release and mining transitions before asserting missing frame art. |
| Battle and text | `PreFabs/ShipModuleWindows/Battle.tscn/Timer` (0.06 s) → `Battle.TimerTimeout()` → `BattleCanvas.QueueRedraw()` → `BattleLogic.BattleTick()` draws battle effects procedurally. `Bulletins.cs` and `ModuleTextFrame.PlayText()` reveal text with delays. | Wired procedural animation/typewriter paths; no basis to demand replacement sprite sheets. Original fidelity, interruption and full combat-state coverage remain unverified. |

### A3: recovered construction frames and remaining source gaps

Visual inspection of all 14 candidate sheets found complete, unlabelled construction sequences in only three. Their nine frames now use `Godot/Sprites/Items/Production/<item>_<stage>.tres` atlases backed by unchanged source PNGs. The renderer masks their magenta background; existing PNGs and static fallbacks retain their original behaviour. See [source hashes and crop coordinates](../Godot/Sprites/Items/Sheets/README.md).

| Recovered item | Supplied sheet | Local evidence |
| --- | --- | --- |
| `i_chassis` | `item_interplanetary_chassis.png` | Cases 206–207: manual/AOC stages, completion, idle and PNG fallbacks; native case 206 checks all frame pixels. |
| `i_drive` | `item_interplanetary_drive.png` | Cases 208–209; native case 208 checks all frame pixels. |
| `r_frame` | `item_resource_frame.png` | Cases 210–211; native case 210 checks all frame pixels. |

The other **11** items still lack three genuine runtime construction frames. Paths below are under `SourceMaterials/SpriteSheets/`. A filename match is insufficient: several sheets visibly contain unfinished labels or borrowed drawings. Do not export their placeholders as recovered original art.

| Runtime item | Candidate sheet | Inspection result | Small research image absent? |
| --- | --- | --- | --- |
| `pulse_blaster_laser` | `item_blaser.png` | Empty magenta boxes, `p` label | Yes |
| `g_chassis` | `item_star_chassis.png` | Chassis drawing overprinted with `place` labels/numbers | No |
| `star_drive` | `item_star_drive.png` | Drive drawing overprinted with `place` labels | No |
| `s__d__m` | `item_self_destruct.png` | `place 1/2/3` boxes; no construction art | No |
| `hyperlight` | `item_hyper_light.png` | Empty magenta boxes, `p` label | No |
| `m__t__x` | `item_mass_transceiver.png` | `place 1/2/3` boxes; no construction art | No |
| `m__f__l` | `item_fuzlaser.png` | Empty magenta boxes, `p` label | Yes |
| `prejudice_torpedo_launcher` | `item_torpedo_launcher.png` | `place 1/2/3` boxes; no construction art | Yes |
| `star_drone` | `item_drone_star.png` | Drone drawings with `p`/`place` labels; sequence identity remains unverified | No |
| `prison_pod` | `item_pod_prison.png` | `place 1/2/3` boxes; no construction art | Yes |
| `sonic_blaster` | `item_blaster.png` | Empty magenta boxes, `p` label | Yes |

The five missing small research images need genuine source art as well. `meh_fuel`/`hed_fuel` are `AutoProduce`, so absent manual construction stages are not counted; `alien_artifact` is collected/researched rather than an ordinary recipe. The production rod (A2) is a separate animation and remains unresolved. Native Windows visual acceptance and original stage timing remain unverified for the recovered frames.

### Related static artwork, kept distinct

[Visual references](visual-reference-notes.md) separately cover AOC plaque (1215691800680642), Methanoid menu icon (1215691800680621), moon-base unavailable buttons (1215691800680623), station stats (1215691800680634), and deposit-analysis platform (1215691800680646). The Methanoid menu face and crossed-out damaged-base service buttons now use the exact task attachments through atlas regions; native Mac placement and state transitions are covered by cases 192–199. `UI.png` contains other framed portrait artwork and `ui_buttons.png` contains menu sources, but neither was substituted for these task references. The previously identified AOC candidate was not the complete staff-panel plaque: that exact graphic now comes from the supplied Asana reference through a clipped atlas region. See [artwork provenance](../Godot/Sprites/References/README.md). Its visibility follows the current factory automation state; the static indicator does not certify original animation timing. `SourceMaterials/Notes.txt` additionally flags the IOS landing-arrow variant and small ship-view border as missing exports; recheck current `ShipInterior.tscn/LandingBlank` and location layers before creating art. These are static graphic/state tasks, not additional claimed missing animations.

The initial neutral moon-preview fallback has been superseded by the [orbital palette correction](original-planet-palette-evidence.md#orbital-view-correction): the enlarged undocked/docking view now uses the supplied planet/station artwork, and the preview inherits its parent's recovered palette while retaining local station presence. Exact source colours are substituted through the existing image cache, so missing colour-specific filenames are no longer required. Cases 380/381 and native Mac pixel checks cover it. The small asteroid preview remains the existing neutral placeholder; the enlarged asteroid field uses its dedicated supplied art. Separate star-map PNG discrepancies and Windows/original comparison remain pending.

## Evidence and handoff limits

The sound cue sheet and original frame cadence remain incomplete: the Asana inventory tasks supply no recordings; still attachments cannot establish motion or audio; repository manual PDFs are LFS pointers (see [original evidence](original-behavior-evidence.md)). The local inventory is actionable without claiming original audiovisual parity. Complete S1/S2 native Windows listening and Windows acceptance for recovered A3 frames. Source the remaining genuine A3 artwork and gather original recordings/address-backed mappings for S3/S4 and A1/A2 timing. Each implementation should retain its own native audio/visual acceptance record; passing headless gameplay tests does not prove playback quality.
