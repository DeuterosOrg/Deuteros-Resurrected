# Transmitter Ending Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [x]`) syntax for tracking.

**Goal:** Activate the normally recovered/manufactured transmitter and play the complete original ending.

**Architecture:** Compile the fixed original presentation offline into indexed images and a sparse frame timeline, plus reconstructed PCM music. A small native Godot overlay presents these assets from its audio clock while the campaign is paused.

**Tech Stack:** Godot4.2.2 C#, net6.0, Newtonsoft.Json, Python stdlib.

**Spec:** docs/superpowers/specs/2026-10-03-transmitter-ending-design.md

## Global Constraints

Use Godot 4.2.2 .NET, net6.0, Newtonsoft.Json, Python stdlib and the existing regression runner; add no dependencies. Preserve root AGENTS.md and existing saves. Only one owned Godot/game/test process at a time. No push, PR, remote merge, Asana writes or external messages. Craig authorized autonomous local implementation/testing while AFK; Windows and original-runtime acceptance remain separate gates.

## Review Focus

- Long render stalls and audio warmup must not desynchronize the music cues or skip the final fade.
- A held mouse at the ending boundary postpones replay without exposing underlying campaign controls.
- DFCC conversion and retained module callbacks must not bypass eligibility or open duplicate players.
- Scene teardown/window close must stop ending audio before the existing mixer/finalizer shutdown.
- Truncated source/assets must fail safely without leaving the campaign paused or writing partial generated assets.

---

### Task 1: Reproducible original ending assets

**Files:** Create `scripts/extract_ending.py`, `scripts/test_extract_ending.py`, `Godot/Ending/sequence.json`, `Godot/Ending/music.wav`; update `docs/original-ending-evidence.md`.

**Interfaces:** Consumes a SHA-verified Disk2 path. Produces `compile_sequence(container, font)` returning JSON-compatible data with `rate`, `end_frame`, `frames`, `images`, `palettes`; each sparse frame is `[frame,palette,layers]`, layer `[image,x,y,masked]`, image `[width,height,base64_indices]`. `render_music(container, frame_count)` returns little-endian stereo16 PCM at44100Hz. CLI `python3 scripts/extract_ending.py DISK2 OUTPUT_DIRECTORY` writes validated deterministic assets.

- [x] Write failing unittest checks for actual source compilation and synthetic boundary cases. Example independent expectations:
```python
self.assertEqual(3669, sequence['end_frame'])
self.assertEqual(bytes([1,0]), composite(bytes([2,2]), bytes([1,0]), masked=False))
self.assertEqual(bytes([1,2]), composite(bytes([2,2]), bytes([1,0]), masked=True))
```
Real-disk checks run only when a disk path is explicitly supplied; normal CI uses committed assets plus hand-built source fragments.
- [x] Run `python3 -m unittest discover -s scripts -p 'test_extract_ending.py'`. Expected: assertions fail because compiler behavior/assets are absent; no typo/import failure accepted as behavior evidence.
- [x] Implement bounded source readers, indexed decoder, stream compiler and original custom tracker PCM reconstruction. Use `struct`, `base64`, `wave`, `hashlib`, `json`, `array`; fail on unknown opcodes/effects or bad ranges. Check source SHA before output. Generate outputs in temporary files then replace only after all compilation succeeds.
```python
# Command-line invocation, no ignored helper imports:
python3 scripts/extract_ending.py /absolute/path/disk2.adf Godot/Ending
```
- [x] Run all Python tests and regenerate into a temporary directory; compare both artifacts byte-for-byte. Expected: tests pass, deterministic outputs identical; source-derived cue and image hashes agree with independently retained research. Inspect first/middle/text/fade frames and audio; record discrepancies honestly.
- [x] Commit compiler/tests/assets/evidence with `git commit -m "Reconstruct the original transmitter ending assets"`.

### Task 2: Native synchronized ending player

**Files:** Create `Godot/Code/Platform/Screens/Ending.cs`, `Godot/PreFabs/Ending.tscn`, `Godot/Tests/RegressionRunner.Ending.cs`; modify regression registration, `Godot/Code/GameCore.Shutdown.cs`, `Godot/Code/Platform/Helpers/OverlayManager.cs`, `Godot/export_presets.cfg`.

**Interfaces:** Consumes Task1 JSON schema and native imported WAV. Produces full-size Control `Ending`, loaded as `res://PreFabs/Ending.tscn` by OverlayManager; exposes only ordinary playback state required by UI, not test-only mutators. Existing overlay callers retain their behavior.

- [x] Append actual-player cases after572. Check native image pixels at source frames, audio-clock catchup, full playback/black/replay with left held, keyboard/mouse suppression, parent removal and window close; catch a duplicate active player. Use existing runner reflection/input helpers for controlled state.
```csharp
var node = OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://PreFabs/Ending.tscn"), false);
Equal(true, GetTree().Paused, "ending freezes campaign");
Equal(true, node is Control, "full ending player exists");
```
- [x] Build/run focused cases through `scripts/validate.py --godot /tmp/deuteros-godot-isolated --case N`. Expected: missing playback behavior fails before implementation; validate actual CLI flags first and ledger any command correction.
- [x] Implement the player with native AudioStreamPlayer and ImageTexture. Deserialize/validate bounded resource data once, draw clipped indexed layers with global palette, select latest frame from corrected audio position, and restart only after completion and released left mouse. Consume input via `_Input`; stop/free owned audio on teardown. Make shutdown synchronously remove an open overlay before waiting for mixer drainage. Explicitly include Ending/sequence.json in exports.
```csharp
var seconds = Math.Max(0, audio.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - AudioServer.GetOutputLatency());
var frame = (int)(seconds * sequence.Rate);
```
- [x] Run focused headless/native and existing overlay/shutdown compatibility cases. Expected: strict terminal PASS, zero engine errors, no audio leak, original pause state restored after controlled close or failed load. Capture and inspect representative images.
- [x] Commit with `git commit -m "Play the original ending in a synchronized native overlay"`.

### Task 3: Normal transmitter activation and acceptance evidence

**Files:** Modify `Godot/Code/Platform/Screens/ShipInterior.cs`, ending regression file/registration, `docs/backlog-progress.md`, `docs/validation-results.md`, `docs/windows-agent-brief.md`, plan.

**Interfaces:** Consumes Task2 scene via existing OverlayManager. Produces normal mounted-item activation while preserving ItemTypes.alien_artifact and existing manufacture/fitting/save contracts.

- [ ] Add failing real-interior cases for rank3 rejection/rank4 activation, docked/travelling/rogue rejection, fitted sixth mount, DFCC precedence, repeated/retained callback and full eight-recovery-to-manufacture-to-fit activation. Assert campaign date/fuel/module counts stay unchanged during ending; save/load preserves replay eligibility.
```csharp
Equal(Ship_States.UnDocked, ship.ShipState, "normal activation state");
Equal(ItemTypes.alien_artifact, ship.Modules[5].ItemStored, "normal fitted transmitter");
// Emit the real sixth module Pressed signal, then assert the ending overlay.
```
- [ ] Run focused cases. Expected: eligible transmitter currently does not open the ending.
- [ ] Route the fitted tool before generic DFCC interception; check UnDocked and Pilot.GetLevel()>=4 plus existing shared rogue/lifecycle guard, then ShowOverlay once. Use existing warning panel for low rank; preserve docked bay navigation and do not consume fuel/item or create saved flags.
- [ ] Run focused/native compatibility and a desktop staged normal-path activation through complete playback/replay/window close. Expected: intended labels/artwork/music/input/fade and frozen campaign; original saves restored. Record evidence before claiming it.
- [ ] Run `python3 scripts/validate.py --godot /tmp/deuteros-godot-isolated --export-windows` and `python3 -m unittest discover -s scripts -p 'test_*.py'`. Expected: all cases/import/build/startup/export pass; package contains timeline/music and excludes tests. Review each log and manifest.
- [ ] Update docs/counts only from demonstrated evidence; commit with `git commit -m "Activate the recovered transmitter from the normal ship controls"`. Obtain the required fresh whole-branch review, reproduce/fix Important findings, rerun affected/full checks, then locally integrate under the merge lock.
