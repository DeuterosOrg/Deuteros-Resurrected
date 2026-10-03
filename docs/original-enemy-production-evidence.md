# Enemy production scheduling

## Original evidence

Disk 1 uses the hash/address mapping in [interstellar travel evidence](original-interstellar-travel-evidence.md). At `$38A64–$38A7C`, the enemy scheduler compares elapsed Sol-clock units against the current interval. When due, `$38A86–$38AA4` counts systems with nonzero hostile-station counts. `$38ABC–$38ACA` indexes the ten-word table at `$38A42` with that remaining-system count:

```text
remaining systems: 0   1    2    3   4   5   6   7   8   9
interval:          0 700 1000  950 900 900 800 700 700 800
```

The initial interval at `$1C33C` is 500. Raw table values and disk hash are retained under ignored `artifacts/research/enemy-production/frequency-facts.json`; the scheduler disassembly is in `artifacts/research/supply-pod/hyperlight-progression-producer.txt`.

## Remake correction

The previous nine-entry table was indexed by `StarSystemsCaptured`, which has no production writers. New games therefore always selected 700; arbitrary saved values could also index outside the array. Production now uses the same sampled remaining-system count as Hyperlight discovery and the full original table.

A due-or-overdue check replaces exact date equality. This handles crossed deadlines and the original zero-system interval, allowing production to resume if a station is recaptured. It performs one batch and schedules from the current date; it does not invent historical catch-up batches.

Case 465 first reproduced day 107 instead of 108 with nine hostile systems. It covers all ten counts, saved deadlines, actual drone output and the unused legacy counter. Case 466 exposed a zero-interval stall after the table correction and covers recapture, overdue dates and peace. Both pass headless and native Mac checks. An initial test-registration recursion was a fixture mistake, retained separately; it is not a game defect. Full 466-case Mac validation, nine Python tests, strict import, startup and audited Windows cross-export pass at `64412e0`; Windows execution remains pending.

## Fractional deadline follow-up

The scheduler now stores `EnemyBuildDay` as an unsigned centiday deadline and adds the exact table interval. Case **486** reproduces early production caused by truncating 950 to nine displayed days and verifies the actual next boundary across saving. Legacy saves without `Clock` scale their old whole-day deadline by 100; case **481** covers migration and **482** rejects malformed/overflowing deadlines before activating a world. Existing due/overdue, recapture and peace behavior remains covered by 465–466.

## Limits

Initial deadline zero is retained. The on-disk interval 500 alone does **not** prove a five-day startup or post-war delay: stored last-Sol-clock `$1C26C` is zero, while `$385C8` copies the current Sol clock there inside an apparent save/load path. Initialization/load semantics need further tracing before inventing a delay. The raw follow-up is `artifacts/research/enemy-production/startup-clock-followup.txt`.

The remake still uses one displayed clock rather than independent Sol/star/SCG clocks. Original-runtime cadence, initialization behavior, complete enemy fleets and Windows execution remain pending. No additional Asana task is declared complete.
