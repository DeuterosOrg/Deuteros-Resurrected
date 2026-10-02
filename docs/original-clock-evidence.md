# Original normal-speed clock

Partial evidence for Asana **1215716464570923**, the request for 0.01 day of natural advancement approximately every 300 seconds. Checked 2026-10-02 against the original Disk 1 image; no timer change is implemented here.

## Source and arithmetic

Disk SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; RAM addresses map to disk offsets with `$6E000 + address - $13000`. Capstone 5.0.7, big-endian M68000. Extracts are retained locally under ignored `artifacts/research/mtx/natural-clock-followup.txt`.

Normal-speed restoration `$20510` writes threshold **$F690 = 63,120** to `$2027E` and clock increment **1** to `$20280`, restoring the previous normal-speed accumulator from `$20276`. The producer `$203DE–$2045C` adds four per available call, compares with that threshold and subtracts it when advancing. A complete normal interval therefore requires **15,780 producer calls**.

The clock rendering [already traced for AMA](original-ama-mining-evidence.md#clock-and-short-cycle-findings) interprets one clock unit as **0.01 displayed day**. At a 50 Hz interrupt rate, the normal interval calculates to **315.6 seconds**. This explains the task's approximate 300-second report, but the rate remains an explicit assumption until checked in the original runtime. Installation at `$204C8` registers callback `$202CA` through the system interrupt-server call with selector 5; this is not a measured emulator timing result.

The platform identity of that callback is now confirmed: the [Amiga SDK interrupt definitions](https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_3._guide/node065C.html) assign selector 5 to `INTB_VERTB`, and the [official vertical-blank example](https://amigadev.elowar.com/read/ADCD_2.1/Libraries_Manual_guide/node05EB.html) installs an interrupt server to count frames. The producer therefore follows vertical blanks, rather than an arbitrary polling loop. Actual video mode, startup and stalls still require runtime evidence before promoting the nominal 50 Hz calculation to a measured wall-clock result.

The initial binary data has threshold 200, but the initialization follow-up below establishes normal-speed restoration before interrupt registration in that path. Pending flag `$20291` prevents the producer from advancing again until simulation consumption clears it. Consequently stalls can lengthen an interval; this is not a wall-clock catch-up accumulator.

The manual advance mode `$20536` instead writes threshold 60, accumulator 56 and increment 100: its first available call can advance a whole day. A second mode `$2055C` uses threshold 16, accumulator 12 and the same 100-unit increment. Both are distinct from the real-time countdown used by [self-destruct](original-self-destruct-evidence.md#countdown-and-loss).

## Remake boundary

`GameCore.UpdateTime` currently advances only on a requested day step or held skip; held steps use a 500 ms interval. It has no natural fractional-day producer. Adding only a fractional date label would not reproduce the original: each consumed fractional increment also invokes the simulation, including the AMA clock gate.

Implementation needs a saved fractional clock and a deliberate mapping between simulation updates and displayed days, including load, pause, manual advancement and interstellar star/ship clocks. Verify startup, an uninterrupted normal interval, input-held advancement, paused menus and a save/reload interval in an emulator before claiming exact timing. Keep this task aligned with the pending interstellar clock integration decision.

## Initialization follow-up

The decoded initialization path calls `$403F4` at `$404CE`; `$403FA` then calls normal-mode restoration `$20510`. Interrupt registration `$204C8` follows at `$404EA`. Thus this path replaces the on-disk threshold 200 with 63,120 **before** registering the producer, and restores the normal accumulator from `$20276`, whose initial value is zero. This resolves the static initialization-order uncertainty for that path; it does not measure video mode, pauses/stalls or later load/overlay behavior.

Aligned instructions and numeric facts are retained in `artifacts/research/mtx/natural-clock-startup.txt` and `natural-clock-startup-facts.json` (trace SHA-256 `3e121ded50aa0984e87334c5b0625e9a4e45d3f6107449fb8fe08165ccf248ec`). The exploratory extract includes misaligned/truncated decoding and is not cited as evidence. The nominal PAL interval remains 315.6 seconds, not a measured runtime result.

The three training consumers at `$22E86` initialize their counters to 24 (`$22EAA/$22F42/$22FDA`) and decrement once per consumed simulation call, including the start call. They do not subtract displayed dates. The remake also defaults all three durations to 24, but compares integer day timestamps; fractional integration must preserve consumed-update behavior rather than merely changing the displayed date. Aligned trace: `artifacts/research/mtx/training-clock-consumer.txt`, SHA-256 `bd6e9c49751184d45bd4cad8ad0d11a73d89af11599f2a672c7dbeed018d2d23`.
