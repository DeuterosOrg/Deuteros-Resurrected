# Original UI sound evidence

Direct disk audit, 2026-10-02. This narrows the [sound inventory](media-gap-inventory.md); it does not certify audible fidelity or add runtime cues.

## Source and sample identities

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. RAM addresses map to disk offset `0x6E000 + address - 0x13000`. The sound table at `$3F7FA` contains 14-byte descriptors: sample pointer, length in words, period, volume and two control words. Dispatcher `$3FBF8` selects channels from its mask; the control words require separate playback interpretation.

All four supplied mono, unsigned 8-bit WAVs match the original signed sample bytes after XOR with `0x80`, except that each WAV omits the final two source bytes:

| WAV under `Godot/Sounds/` | ID | Sample address | Original / supplied bytes | Period / volume | Omitted bytes |
| --- | ---: | --- | ---: | --- | --- |
| `Typing.wav` | 19 | `$3CE50` | 598 / 596 | 300 / 32 | `00 00` |
| `Button/sMainMenu_Button.wav` | 20 | `$3ED54` | 608 / 606 | 512 / 32 | `00 00` |
| `Button/sTrainingRoom_Button.wav` | 22 | `$3EFB4` | 512 / 510 | 300 / 32 | `00 00` |
| `Button/sTrainingRoom_Door.wav` | 53 | `$3CBFA` | 598 / 596 | 800 / 56 | `08 FA` |

The door's omitted samples are not silence. No asset was changed without listening evidence. Shared sample pointers do not establish identical cues: other descriptors reuse samples with different period/volume/control values.

## Verified typing trigger

At `$1FBE6`, a text-mode flag selects the delayed character path. That path draws the glyph through `$1FC22`, compares the character with space at `$1FBFA`, and dispatches sound **19** for non-space characters at `$1FC08`, with channel mask `$0C`. It then runs a 20,000-iteration CPU delay loop. The loop is not a measured millisecond delay.

This establishes sound 19 as typing feedback. The earlier archive label “save UI” does not establish a dedicated save-confirmation cue.

## Menu feedback and remaining work

Wrapper `$3F780` returns when the feedback selector `$3F77C` is zero or suppression byte `$1C396` is nonzero. Otherwise it adds 19 to the selector and calls the dispatcher with mask `$0C`. The generic command path calls this wrapper at `$2236E` before dispatching the action. Thus selector 1 reaches sound 20 and selector 3 reaches sound 22; complete screen/action assignments still require tracing the selector writers.

Audit output and instruction excerpts are retained under ignored `artifacts/research/ui-sounds/`. Next checks are selector ownership, descriptor playback behavior and original/remake listening comparison. No dedicated save, status or encounter sound is inferred from these four sample matches.

## Menu selection and hover follow-up

The selector is chosen by the input region, not just the current screen. The contiguous input path `$223AA–$22618` establishes:

| Input path | Selector / dispatched sound | Evidence |
| --- | --- | --- |
| Left region, x below 97 and y below 79 | 4 / **23** | `$22414`; enabled commands are resolved from `$214D8`. |
| Left region below that boundary | 1 / **20** | `$2250E`; the button row is computed from `(y - 88) >> 4`. |
| Rectangle x128–317, y186–197 | 1 / **20** | `$2258C–$225AC`; action `$6C`. Its gameplay label is not established here. |
| Other screen controls | Copies `$3F77E` | `$225CE–$225D4`; the selected screen supplies this value. |
| Alternate-button path | 0 / no wrapper cue | `$223B2–$223DC`; callback dispatch remains separate. |

For both left-menu regions, changing the selected help/command value to a nonzero value dispatches **66**, mask 4 (`$22468–$2247E`, `$22568–$2257A`). An unchanged selection returns without replaying it; clearing the selection skips the sound. This establishes a hover-selection cue independently of the later activation cue. Coordinates are original logical coordinates, not remake window pixels.

Screen selector writers include value 2 at `$3014A`, and value 3 at `$20D32`, `$21120`, `$324FE`, `$3592A`, `$36B2C`, `$36EA2`, `$3842C` and `$3967C` (News). Thus sound 22 is not exclusive to the training screen despite its supplied filename. The reset helper `$3F7A8` clears the screen selector. Remaining screen identities and interruption behavior still need verification.

| Sound | Sample address / bytes | Period / volume | Supplied asset comparison |
| ---: | --- | --- | --- |
| 21 | `$3ED54` / 608 | 820 / 63 | Same sample as menu sound 20, with different playback parameters. |
| 23 | `$3D0A6` / 216 | 600 / 32 | No matching supplied mono 8-bit WAV prefix. |
| 66 | `$3B514` / 96 | 2400 / 16 | No matching supplied mono 8-bit WAV prefix. |

The remake currently shares sound 20 across its top/side menu and has no corresponding hover cue. These are confirmed sourcing/comparison gaps; no runtime sound mapping or audio asset was changed by this audit. Original listening, channel interruption and descriptor control behavior remain required before claiming fidelity. Raw input disassembly and exact descriptor/sample hashes are in `artifacts/research/ui-sounds/feedback-selector-context.txt` and `menu-feedback-descriptors.json`.

## Training-door trigger

Training updates at `$22E86` write three transition bytes at `$2E432`: 1 when training starts, 2 while still training, and 3 when it finishes. Door renderer `$2E438` returns unless at least one byte has bit 0 set. Its six-step loop draws all three doors, using resources 12–17 in opposite orders for starting/finishing transitions. Only the first loop pass (`d7 = 5`) dispatches sound **53**, mask `$0C`, when a door is transitioning (`$2E4EC–$2E4FE`). Concurrent doors share that dispatch; a steady closed door does not create an extra cue. The blitter wait and `$2D580` call do not alone establish an elapsed animation duration.

The remake's `Training.cs` loads the matched door WAV into `DoorSound`, but only assigns `ButtonSound` to its player. There is no door-stream assignment or playback call. This corrects the earlier inventory's claim that both files were wired. Evidence: `artifacts/research/ui-sounds/door-transition.txt` and the training producer in `artifacts/research/news/roster-retirement.txt`. A fix should cover opening, closing, simultaneous transitions, silent initial/static states, interruption and preserved button feedback.

The subsequent training correction wires sound 53 through a separate player and a shared transition check. Case 469 first reproduces the missing playback, then verifies closing/opening, simultaneous doors, unchanged-update silence, retained button feedback and exit stop. Native/headless checks pass; sample bytes and provisional animation speed are unchanged. This does not establish listening fidelity or resolve the separate staggered-transition/lock lifecycle.
