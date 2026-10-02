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
