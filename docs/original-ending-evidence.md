# Original transmitter ending

Follow-up for the open alien-message/campaign work, 2026-10-02. This establishes the correct ending disk, container and script format. The source-derived assets are now compiled reproducibly; native playback and normal activation are still implementation work. This is not an original-emulator comparison.

## Activation and required disk

The mounted item-1 action at `$34F96` requires ship state 3 and rank 4, then jumps to `$38032`. Before teardown, `$38032` calls `$3880A`. That routine reads the disk identifier through `$209A2` and requires **`$8B632804`**, retrying with prompt `$153` otherwise. `$209A2` reads the first `$400` bytes and obtains the identifier at offset `$3FC`.

The known Disk 2 image has that identifier; Disk 1 has `$4452F018`. Disk 2 SHA-256 is `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. Thus Disk 2 selection is established directly, rather than inferred from plausible artwork at the requested offset.

## Important overlay correction

The earlier Disk 1 presentation extracts are **not the actual ending executable**. Disk 2 supplies a different overlay at raw offset `$5800`, loaded at `$20000`. Its entry jumps to `$2150A`, and selector `$216F8` indexes the five offsets at `$214DE`. Mode 4 selects **raw Disk 2 offset `$6E000`**. Reader `$2308A` loads the size-prefixed container at `$32B68`; parser `$2104C` reads its header. Earlier addresses `$21734/$21926/$21276` belong to Disk 1's presentation code and must not be used as ending-runtime breakpoints.

The container is **310,042 bytes**, SHA-256 `ff10894b9960e1b70ba10186d9108b837e3af4fa39eaaf1872eaa9e58b0bbf89`. It has seven script streams, palette/music pointers, an image-offset table at container-relative `$12E12` and a graphics bank at `$13162`.

## Decoded sequence and artwork

The actual script dispatcher starts at `$21280`. Static decoding reaches the palette section at `$AF8` with 635 header/command records and no unknown opcodes. Commands used include image/position changes, update waits, palette selection, music-position waits, relative calls/returns, text, input suppression and sequence completion. Music waits prevent treating all script delays as a simple frame count.

The image table has 212 slots but only **89 distinct offsets**; unused slots alias image zero. The existing four-plane decoder recovers the source artwork. A contact sheet was inspected with script-selected palette 33; that palette is a snapshot, not the correct palette for every frame. The final text command at `$810` addresses `$1112`, containing `COMPLETE`. Text stops at the first zero: adjacent strings are separate labels. Six stream text commands display `VICTORY`, `END`, `COMPLETE` and three `DEUTEROS` labels. No narrative ending text should be invented.

Reproducible local scripts, image hashes, command records and exact disassembly are under ignored `artifacts/research/alien-messages/`: `decode-ending-images.py`, `decode-ending-script.py`, `ending-disk2-facts.json` and `ending-disk2-*.txt`. The original full-playback timing, masks, music, input/exit behavior and activation through normal Warlord progression still require verification before campaign acceptance.

## Reproducible presentation assets (2026-10-03)

Run `python3 scripts/extract_ending.py /path/to/disk2.adf Godot/Ending` from the repository root. The compiler verifies both disk/container SHA values before output; no full disk or research helper is needed at runtime. It emits97 indexed images (89 distinct original entries, two full-row snapshots and six bitmap labels),844 sparse frame changes and44.1kHz stereo16 PCM. All89 original indexed payloads match the earlier independent decoder hashes. A second generation produced identical bytes.

`sequence.json`:1,395,816bytes, SHA `9d1bd3ac09baff63002b8c05e1483718d706700b973f97454a4c6ed02b0197f5`.
`music.wav`:13,064,228bytes, SHA `905546ad4a31dee10d65283385fac3ee7ace26908c34f43fa4dd7f6a092486bc`.

Aligned runtime trace: `artifacts/research/alien-messages/ending-runtime-audio.txt`, SHA `65402ea731bd9d0cab3f49d71ce3fa487b8c3ca736bf297ac2b31fb969383bed`. A separate followup at21450–21468 confirms end16 sets the global end flag then stops its stream via21284; other streams finish their current update. Update waits are rechecked immediately after parsing, and music waits require matching zero-based order and strictly greater row. The final cue order8,row47 arrives at nominal frame3669. Fade lasts32 nominal50Hz updates; the compiled one-update black interval yields3703frames/74.06seconds. Original disk/reset delays are not included. Runtime must wait for left-button release before replay.

Images use16-pixel x words. Ordinary copies write zero pixels;8000 sprites mask zero;C000 captures affected full-width framebuffer rows andFFFF restores that shared snapshot. Text uses the728-byte original font at raw59C8, eight-pixel x columns and four-pixel y rows. Representative compiled artwork, all six final labels, fading and terminal black have been visually inspected; snapshots are retained with the implementation evidence.

The custom15-instrument tracker uses VBlank tick counts, including speeds above31; these are not standard MOD BPM commands. Instruments start DMA one tick after a note and install repeat registers on the following tick. Two-word repeats point to four silent bytes. The digital mixer uses PAL3546895ticks/second and channels0/3 left,1/2 right, consistent with the [Commodore Amiga Hardware Reference Manual, Audio Hardware](https://www.amigarealm.com/computing/knowledge/hardref/ch5.htm). It preserves integer volume scaling, period slides and the ending fade. PCM sample range is−32512..32384, without clipping.

The compiler tests cover literal/repeat RLE layouts, transparent zero/clipping, full-row snapshots, exact glyph/string boundaries, strict music-row waits, high tick speeds, delayed DMA/silent repeats, and malformed inputs. Run `python3 -m unittest discover -s scripts -p 'test_*.py'`; optionally set `DEUTEROS_ENDING_DISK2` to verify the original final cue and artwork directly. Subjective listening, analogue Amiga output/filter characteristics, measured original cadence, native Windows and complete campaign acceptance remain pending.
