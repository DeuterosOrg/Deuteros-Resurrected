# Original normal-speed clock

Partial evidence for Asana **1215716464570923**, the request for 0.01 day of natural advancement approximately every 300 seconds. Source checked against the original Disk 1 image; saved fractional-clock implementation added 2026-10-03. Original-runtime timing and independent interstellar clocks remain unverified.

## Source and arithmetic

Disk SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`; RAM addresses map to disk offsets with `$6E000 + address - $13000`. Capstone 5.0.7, big-endian M68000. Extracts are retained locally under ignored `artifacts/research/mtx/natural-clock-followup.txt`.

Normal-speed restoration `$20510` writes threshold **$F690 = 63,120** to `$2027E` and clock increment **1** to `$20280`, restoring the previous normal-speed accumulator from `$20276`. The producer `$203DE–$2045C` adds four per available call, compares with that threshold and subtracts it when advancing. A complete normal interval therefore requires **15,780 producer calls**.

The clock rendering [already traced for AMA](original-ama-mining-evidence.md#clock-and-short-cycle-findings) interprets one clock unit as **0.01 displayed day**. At a 50 Hz interrupt rate, the normal interval calculates to **315.6 seconds**. This explains the task's approximate 300-second report, but the rate remains an explicit assumption until checked in the original runtime. Installation at `$204C8` registers callback `$202CA` through the system interrupt-server call with selector 5; this is not a measured emulator timing result.

The platform identity of that callback is now confirmed: the [Amiga SDK interrupt definitions](https://amigadev.elowar.com/read/ADCD_2.1/Includes_and_Autodocs_3._guide/node065C.html) assign selector 5 to `INTB_VERTB`, and the [official vertical-blank example](https://amigadev.elowar.com/read/ADCD_2.1/Libraries_Manual_guide/node05EB.html) installs an interrupt server to count frames. The producer therefore follows vertical blanks, rather than an arbitrary polling loop. Actual video mode, startup and stalls still require runtime evidence before promoting the nominal 50 Hz calculation to a measured wall-clock result.

The initial binary data has threshold 200, but the initialization follow-up below establishes normal-speed restoration before interrupt registration in that path. Pending flag `$20291` prevents the producer from advancing again until simulation consumption clears it. Consequently stalls can lengthen an interval; this is not a wall-clock catch-up accumulator.

The manual advance mode `$20536` instead writes threshold 60, accumulator 56 and increment 100: its first available call can advance a whole day. A second mode `$2055C` uses threshold 16, accumulator 12 and the same 100-unit increment. Both are distinct from the real-time countdown used by [self-destruct](original-self-destruct-evidence.md#countdown-and-loss).

## Remake boundary

`GameCore.UpdateTime` now produces one centiday after a nominal 315.6 seconds of normal play and consumes one simulation update. Manual steps add 100 centidays while retaining the previous normal-mode remainder. The existing 500 ms hold cadence is retained; this is not a claim that it matches the original manual mode. Pause and intro do not accumulate normal time. A blocked simulation holds one pending increment; stalls do not create a catch-up queue.

The saved `GameClock` holds displayed centidays, partial normal elapsed time and the pending increment. Historical `CurrentDay` remains the consumed-update counter, preserving training, travel and other update-based progress. Missing legacy clocks migrate from `CurrentDay * 100`; malformed clocks/deadlines are rejected before replacing the world. Calendar attrition and enemy deadlines use displayed dates instead. News, save slots, the main clock and ETA use the fractional date; ETA follows hold/release, toggle and external stops on the next display frame.

Cases **475–493** cover timing, mixed modes, stalls, pending save/reload, legacy migration, invalid input, calendar gates, AMA phases and live ETA controls. Native Mac checks include date/News/save/ETA rendering. A physical new-game check at `862e421` queued a research trainee without touching time controls: the observed clock changed from `.00` to `.01` and the training door closed. The screenshots bracket the transition; they do not measure its exact boundary or compare it with an emulator.

Independent star/private-SCG clocks remain open. Compare startup, uninterrupted and held intervals, pauses, stalls and reload with the original runtime before claiming exact timing. See [validation results](validation-results.md#saved-fractional-clock-and-ama-phases-493-case-checkpoint) and the [Windows brief](windows-agent-brief.md#fractional-clock-and-ama-phase-follow-up).

## Initialization follow-up

The decoded initialization path calls `$403F4` at `$404CE`; `$403FA` then calls normal-mode restoration `$20510`. Interrupt registration `$204C8` follows at `$404EA`. Thus this path replaces the on-disk threshold 200 with 63,120 **before** registering the producer, and restores the normal accumulator from `$20276`, whose initial value is zero. This resolves the static initialization-order uncertainty for that path; it does not measure video mode, pauses/stalls or later load/overlay behavior.

Aligned instructions and numeric facts are retained in `artifacts/research/mtx/natural-clock-startup.txt` and `natural-clock-startup-facts.json` (trace SHA-256 `3e121ded50aa0984e87334c5b0625e9a4e45d3f6107449fb8fe08165ccf248ec`). The exploratory extract includes misaligned/truncated decoding and is not cited as evidence. The nominal PAL interval remains 315.6 seconds, not a measured runtime result.

The three training consumers at `$22E86` initialize their counters to 24 (`$22EAA/$22F42/$22FDA`) and decrement once per consumed simulation call, including the start call. They do not subtract displayed dates. The remake also defaults all three durations to 24, but compares integer day timestamps; fractional integration retains that consumed-update counter rather than merely changing the displayed date. Aligned trace: `artifacts/research/mtx/training-clock-consumer.txt`, SHA-256 `bd6e9c49751184d45bd4cad8ad0d11a73d89af11599f2a672c7dbeed018d2d23`.
