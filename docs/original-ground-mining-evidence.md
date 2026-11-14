# Original ground mining and surveys

## Source

The supplied Disk 1 image (`artifacts/research/mtx/disk1.adf`, SHA-256 `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`) contains Earth extraction at `$230CE–$231DE` and local extraction at `$231E0–$232DE`. The disassembly is retained in `artifacts/research/ground-mining/extraction-and-survey.txt`, SHA-256 `435d15c458d0d0aeece4a564d70c197db51add0ac689a12d606c18dab0bc9f78`.

## Verified behavior

- Earth survey and extraction accumulators each add 128 and act on carry. Local bases require original ground state at least 3 and a clear hostile-owner bit. Surveys do not require derricks or a completed local orbital station.
- A deposit word's high bit distinguishes a known vein, including known zero, from a survey countdown. Zero or one discovers a vein on the next eligible update; larger counts decrement.
- Starting a survey uses `(random & 7) * multiplier`. Completion uses `(((random & 32767) * multiplier) & 32767) | 0x32`: forced low bits guarantee at least 50. The mineral multipliers and extraction rates match the existing CoreData table.
- Extraction consumes a whole `derricks * rate` batch. Insufficient ore starts a new survey and produces nothing; it does not grant a partial or unsupported batch. Exact depletion leaves a known zero until a subsequent extraction underflows.
- Sufficient extraction consumes the deposit even when stores are full; output caps at 50,000. MTX redirects output to orbit. Earth's redirect additionally requires station type 8; local extraction tests the installed MTX flag.

## Remake correction and saves

Cases 453–455 first reproduced unsupported ore, stationary surveys without derricks and hostile mining. The shared `Planet.DayTick` now enforces the mapped batch, survey and eligibility rules. It rejects nonadvancing updates, consumes one update per call, and uses shared randomness instead of constructing identically seeded generators for adjacent planets.

`Material.SurveyTicks = -1` preserves known zero using the existing saved fields; zero remains a valid survey countdown. The ground screen shows `SURVEY` for that countdown and `0` for known exhaustion. Legacy negative veins load as zero: active countdowns remain intact, otherwise the next extraction starts a survey. Existing inventory is preserved because past excess production cannot be reconstructed safely. Invalid rule ranges, negative derrick counts and invalid deposit/countdown values are rejected before activation. Large nonnegative legacy derrick counts use wide arithmetic and cannot overflow into ore production.

Cases 456–458 cover all mineral multipliers, controlled random inputs, exact depletion, zero derricks, save/load, legacy repair, malformed state, and real scene labels. Focused/native and aggregate results are recorded separately; a test count alone does not establish Windows acceptance.

## Remaining boundaries

Earth still uses the remake's even-day cadence. Original fractional/star clocks, exact RNG sequence and original-runtime/Windows comparisons remain outstanding. This correction does not claim to finish the broader mining/campaign tasks or increase the accepted issue count.
