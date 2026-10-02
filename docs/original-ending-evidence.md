# Original transmitter ending

Follow-up for the open alien-message/campaign work, 2026-10-02. This establishes the correct ending disk, container and script format. It is not an emulator playback or an implemented ending.

## Activation and required disk

The mounted item-1 action at `$34F96` requires ship state 3 and rank 4, then jumps to `$38032`. Before teardown, `$38032` calls `$3880A`. That routine reads the disk identifier through `$209A2` and requires **`$8B632804`**, retrying with prompt `$153` otherwise. `$209A2` reads the first `$400` bytes and obtains the identifier at offset `$3FC`.

The known Disk 2 image has that identifier; Disk 1 has `$4452F018`. Disk 2 SHA-256 is `99909db1e190be02e049084743af44f00e331be6bf2d97b4831ada5fe4c30b4a`. Thus Disk 2 selection is established directly, rather than inferred from plausible artwork at the requested offset.

## Important overlay correction

The earlier Disk 1 presentation extracts are **not the actual ending executable**. Disk 2 supplies a different overlay at raw offset `$5800`, loaded at `$20000`. Its entry jumps to `$2150A`, and selector `$216F8` indexes the five offsets at `$214DE`. Mode 4 selects **raw Disk 2 offset `$6E000`**. Reader `$2308A` loads the size-prefixed container at `$32B68`; parser `$2104C` reads its header. Earlier addresses `$21734/$21926/$21276` belong to Disk 1's presentation code and must not be used as ending-runtime breakpoints.

The container is **310,042 bytes**, SHA-256 `ff10894b9960e1b70ba10186d9108b837e3af4fa39eaaf1872eaa9e58b0bbf89`. It has seven script streams, palette/music pointers, an image-offset table at container-relative `$12E12` and a graphics bank at `$13162`.

## Decoded sequence and artwork

The actual script dispatcher starts at `$21280`. Static decoding reaches the palette section at `$AF8` with 635 header/command records and no unknown opcodes. Commands used include image/position changes, update waits, palette selection, music-position waits, relative calls/returns, text, input suppression and sequence completion. Music waits prevent treating all script delays as a simple frame count.

The image table has 212 slots but only **89 distinct offsets**; unused slots alias image zero. The existing four-plane decoder recovers the source artwork. A contact sheet was inspected with script-selected palette 33; that palette is a snapshot, not the correct palette for every frame. The final text command at `$810` addresses `$1112`, containing `COMPLETE` followed by repeated `DEUTEROS` labels. No narrative ending text should be invented.

Reproducible local scripts, image hashes, command records and exact disassembly are under ignored `artifacts/research/alien-messages/`: `decode-ending-images.py`, `decode-ending-script.py`, `ending-disk2-facts.json` and `ending-disk2-*.txt`. The original full-playback timing, masks, music, input/exit behavior and activation through normal Warlord progression still require verification before campaign acceptance.
