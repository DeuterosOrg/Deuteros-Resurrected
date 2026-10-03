# Transmitter activation and original ending

## Outcome

The existing recovered alien device, researched and manufactured through the normal campaign, plays the original Disk 2 ending when its fitted tool is activated by an undocked Warlord. Preserve all seven animation streams, original indexed artwork, bitmap text, palette changes, music-position cues, fade, input suppression and replay. No substitute bulletin, new recipe, invented item or victory narrative.

Use Godot 4.2.2 .NET, net6.0, Newtonsoft.Json, Python stdlib and the existing regression runner; add no dependencies. Preserve root AGENTS.md and existing saves. Only one owned Godot/game/test process at a time. No push, PR, remote merge, Asana writes or external messages. Craig authorized autonomous local implementation/testing while AFK; Windows and original-runtime acceptance remain separate gates.

## Source and compilation

Use SHA-verified original Disk 2 `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`, ending container at raw 0x6e000 (310042 bytes; SHA `ff10894b9960e1b70ba10186d9108b837e3af4fa39eaaf1872eaa9e58b0bbf89`), and font at raw 0x59c8. Aligned source and prior decoder findings are documented in original-ending-evidence.md; preserve reproducibility in a stdlib compiler, not a dependency on ignored research scripts.

Compile this fixed presentation offline into JSON containing indexed images, RGB4 palettes and sparse frame/layer states, plus 44.1kHz stereo signed16 WAV. Runtime uses FileAccess/Newtonsoft and native ImageTexture/AudioStreamPlayer. Do not ship a general Amiga interpreter or custom audio decoder. Full disk images are not runtime assets. Export explicitly includes the JSON; WAV uses native import without lossy conversion.

Interpret used opcodes with bounded reads/steps: stop, image, position, update wait, palette, music wait, x/y deltas, backward relative jump, single-slot call/return, text, end, input suppression. Wait1 displays one update; music wait requires matching zero-based order and row strictly greater than its operand. Decode both four-plane RLE layouts. Unflagged images overwrite index0; bit8000 sprites mask index0; C000 captures the full-width affected framebuffer rows and subsequent FFFF restores that snapshot. Framebuffer clears each frame. Text terminates at the first zero and uses original 8x8 glyphs, x*8/y*4 positions and foreground/background indices. Preserve layer order and clipping. End stops its stream, then remaining streams complete the current update before fade.

The custom music player uses initial speed6 at nominal50Hz, 15 instruments, sequential sample relocation, period slides, volume, filter flag, speed and order effects. Instrument trigger silences DMA; next tick starts the initial sample; following tick installs its loop. Two-word loops use the original silent sentinel. Use PAL audio clock3546895 from the Commodore hardware manual. Preserve four-channel stereo assignment and integer volume/fade behavior. This is a digital reconstruction; analogue hardware filtering and original-machine timing acceptance remain explicitly unverified. No invented BPM conversion for speed values above31.

The nominal source-derived ending cue is order8,row>46, around73.4seconds; exact compiled frame count must be corrected if an aligned source discrepancy is found. Fade decrements each RGB4 component once per update and volume256 by8 for32updates. Clear to black and wait for left-button release before restarting the same ending. The minimal black interval is a documented50Hz scheduling approximation, not measured disk/loading time.

## Activation and lifetime

Reuse ItemTypes.alien_artifact, mass2000, zero-recipe orbital production, six-mount SCG fitting and ordinary Staff.GetLevel. The action requires UnDocked and assigned rank>=4; no additional fuel/research/count check was present in the original handler. Existing rogue-command restrictions still apply. A low-rank crew gets the existing module warning presentation with concise Warlord requirement. Docked clicks retain normal bay navigation. The mounted transmitter takes precedence over the remake's generic DFCC module interception.

Show a full320x200 opaque ending through OverlayManager; its paused tree freezes simulation and underlying audio/UI. The ending consumes keyboard/mouse input including Escape, while normal window close remains available. No in-game skip or temporary resume button is added. Closing the window must free ending audio before existing mixer shutdown waits. Reuse the overlay close path with a synchronous teardown option if necessary. Do not change save data or consume the device. Reloading an ordinary save leaves it available for replay.

Use audio playback position, corrected for mixer/output latency, as the visual clock; catch up directly after a slow frame. Handle initial audio warmup, terminal fade/black, held mouse at replay, repeated activation and teardown without a second player or stale callback. Loading/asset failure must restore the prior paused state and leave the campaign usable.

## Verification

Compiler tests independently check RLE literal/repeat modes, masked versus opaque zero, snapshot/text behavior, strict music waits, source cue frames, delayed sample start and silent sentinel, malformed/truncated sources and deterministic output. Inspect rendered representative frames and listen to reconstructed audio; do not claim original emulator comparison.

Real-engine cases cover player frame/loop behavior, ordinary activation, rank/state/rogue guards, DFCC precedence, paused campaign and audio, input suppression, failed asset initialization, scene/world replacement, and window-close cleanup. Exercise the existing eight-recovery/manufacture/fitting path into activation, not only direct player construction. Run focused headless/native and desktop playback through one complete ending/replay. One independent final review, full regressions/Python/import/startup/Windows cross-export and package audit precede local integration. Windows native/campaign acceptance remains pending its actual evidence.
