# Original grapple-only ACC behavior

Task **1215683087492480** asks whether an engaged ACC deliberately disengages at the asteroids when the ship has a grapple but no AMA. **Yes: the original scan path calls the same handler as the manual “Disengage A.C.C.” control.** This is not an automatic grapple-capture command.

## Evidence identity and address mapping

Decoded on 2026-10-02 from `artifacts/research/mtx/disk1.adf`, SHA-256:

```text
6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38
```

The disk segment at `$6E000` loads at RAM `$13000`; disk offsets below are calculated as `$6E000 + RAM address - $13000`. Decode as big-endian Motorola 68000. Raw dumps and the text/control lookup are retained under ignored `artifacts/research/supply-pod/acc-disengage-verified.txt` and `acc-control-map.txt`; they do not accompany a Git checkout.

## Control identity

The ACC window installs input table `$21B14` at `$33994`. Its first four 12-byte records link labels to action numbers. Dispatch pointers are at `$212B0 + action * 4`:

| Control | Text ID | Action | Handler |
| --- | --- | --- | --- |
| Engage A.C.C. | `$EB` | `$49` | `$339E4` |
| Disengage A.C.C. | `$EC` | `$4A` | `$33A1E` |
| Complete Cycle | `$ED` | `$4B` | `$33A30` |
| Clear Settings | `$EE` | `$4C` | `$33A6E` |

Text entries are signed word offsets relative to `$1C482`. `$EB` and `$EC` embed text references using `$1A` followed by a word: `$E9` is “Engage ” and `$EF` is “Disengage ”. A plain first-NUL parser would truncate those labels incorrectly. The Disengage handler contains `4eb900033cae` at `$33A24`: `JSR $33CAE`.

## Automatic scan branch

After generating a packed asteroid result into ship byte `+$0D`, `$23B3E–$23B4C` requires `(ship[+$0F] & 6) == 6`. Engage sets both bits; Complete Cycle instead sets bit 2 and clears bit 1, so it does not enter this branch.

`$23B4E–$23B6A` compares the three module words at `+$16`, `+$18`, and `+$1A` against `$9600`, the AMA tool encoding. Without that module, `$23B6C` contains `4ef900033cae`: `JMP $33CAE`. This occurs **before mineral selection and asteroid-class checks**, so the stop is not limited to large, selected asteroids.

The equipment display resolves names as text ID `$101 + item` at `$32F32–$32F38`. Text `$116` at `$1D83F` is “A.M.A.”, establishing item `$15`. Installation at `$32FF2–$33006` packs `(item + 1) << 8` into the tool word and stores a single tool with low-byte zero; tool kind `$8000` therefore gives `$9600`.

`$33CAE–$33CEA` sets bit 1 and clears bit 2 in `+$0F`. It only changes ship state/countdown for states `$12` and `$13`; the scan state is left in orbit. It does not capture cargo, credit stores, clear filters or launch a return trip. The shared manual handler establishes the meaning of the flag change without guessing from bit names.

## Remake correction and remaining checks

The remake previously left normal engaged ACC active indefinitely without an AMA. `ACC.Update` now disengages on a non-null asteroid scan without AMA, preserving the scan for manual capture, cargo, fuel, route and selected resources. It waits when no scan is available and retains the existing AMA mining path. Cases **305–309** exercise the failure, unchanged modes, daily scanner, save round trip and manual grapple control.

This correction addresses ordinary Engage. The [Complete Cycle follow-up](original-acc-cycle-evidence.md) establishes and corrects its ground/orbital unload-and-stop lifecycle; the broader mining-expedition lifecycle remains under investigation. The [generation follow-up](original-ama-mining-evidence.md#generated-classes-and-minerals) restores the traced mineral/class set. Original RNG equivalence, scan cadence and broader AMA behavior remain research work. Native Windows source/export, physical controls and normal campaign acceptance are still required; see the [Windows brief](windows-agent-brief.md).

## Breakup quantities and animation follow-up

The same Disk 1 mapping resolves text `$130` at `$1D9F8` to the breakup heading and `$131` at `$1DA1C` to its completion message. `$32CC8–$32CF4` reads the held packed asteroid, resolves mass through `$364AE`, and locates the destination mineral stock. `$32D28–$32D46` prints remaining mass on the left and current stored quantity on the right. `$32D4C–$32D5A` decrements remaining mass and increments stock one unit at a time, stopping at exhaustion or `$C350` (50,000). Completion follows at `$32D5C`.

The normal Mac recovery exposed the remake's hardcoded `100 … 50000` display; the underlying transfer was correct. Its static summary now uses held mass and projected capped stock, while retaining the existing five-second unload. This does not reproduce the original counter animation or establish its runtime cadence. Raw instructions: ignored `artifacts/research/grapple-unload/breakup-display.txt`. No original-emulator observation is claimed.
