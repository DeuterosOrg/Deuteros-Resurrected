# Original fuel refining

Follow-up for **1215685674676225 — event order**, 2026-10-02. Refining quantities and mineral identities are now mapped. A remake starvation failure is reproduced; its correction is not implemented yet.

## Source and resource identities

Use the Disk 1 identity and RAM conversion in [the event-order trace](original-event-order-evidence.md). Extract: `artifacts/research/refining/recipes-phase-allocation.txt`, SHA-256 `c992090e0cc88d9147b83e3fc5bda0fc8257835084af9245623792d19fcde55f`. Decoded table values are alongside it in `tables.json`.

The stock selection pointers at `$20D6C–$20D78` and `$20F66–$20F92` select Earth `$19D84`, orbital record `+$36`, and local ground `+$AC`; the resource words follow their two-byte headers. Resource-name rendering starts at text `$32`. Texts `$37/$38/$39/$3A/$40/$41` resolve to Hydrogen, Deuterium, Methane, Helium, MeH Fuel and HeD Fuel. This identifies the refining offsets without guessing from the remake's recipes.

`$1A354` starts at **14** in the disk image. Research completion `$23700–$23712` increments it for IDs 5 and 15, exposing the successive fuel entries in resource-name loops. Refining requires count at least 15; its HeD branches additionally require a nonzero remainder after subtracting 15. This is a resource-list boundary, not an arbitrary campaign-stage number.

## Batches and original thresholds

`$3684A–$3697A` implements these batches on an eligible phase. Every input threshold applies independently to both inputs.

| Location / fuel | Inputs | Minimum input | Consumed each | Output added | Output must initially be below |
| --- | --- | ---: | ---: | ---: | ---: |
| Earth ground / MeH | Hydrogen + Methane | 3 | 2 | 3 | 50,000 |
| Earth ground / HeD | Deuterium + Helium | 3 | 2 | 2 | 50,000 |
| Orbital / MeH | Hydrogen + Methane | 6 | 6 | 8 | 49,994 |
| Orbital / HeD | Deuterium + Helium | 5 | 5 | 5 | 50,000 |
| Other ground / MeH | Hydrogen + Methane | 2 | 2 | 3 | 49,999 |

These are pre-addition comparisons, not a clamp to 50,000: some boundary values can finish above that number. Preserve this distinction in the evidence; a compatibility policy is needed before labelling the original thresholds intentional or correcting them. No other-ground HeD branch appears in this function. Ground processing additionally requires record `+$F0 >= 3`; identify that state before equating it with a remake build-part count.

## Phase and station allocation

The phase byte `$36848` increments once per eligible function call, independent of stock availability. Earth ground runs on even parity. Local processing selects alternating records in the **98-record** table at `$13810`, skipping owner bit 7 and station types below 8. This is separate from the 160 body IDs.

The station-system offsets at `$19646` are **0,16,20,32,48,54,56,72,82**. Allocator `$2FD1C–$2FD42` finds a free bit in the system bitmap and adds that system offset; `$2FD74` initializes the resulting record. Body ID is stored separately at `+$EF` and mapped through `$1965A`. When initialization finds type 9 in the chosen slot, `$2FF4C` scans the current system for a free record. `$2FE94` swaps the complete `$F6` bytes and repairs embedded record references and body-to-record lookups. Thus another station can move records, changing its refining parity. The capture path still needs tracing before assigning equivalent saved phases. Planet enumeration order or a changing human-station ordinal is not sufficient evidence for that mapping. The additional extract is `artifacts/research/refining/station-relocation.txt`.

## Reproduced remake failure and next checks

The remake toggles each shared item's `AutoProduceFlip` inside the factory loop, only when ingredients are available. Two consistently supplied factories therefore leave the first producing on every call and the second producing nothing. It also uses the same 2+2→3 recipe across contexts, excludes ground HeD, and lacks these output thresholds.

Isolated branch `codex/refining-investigation`, commit `6bab36d`, adds **case 445**. With completed, type-8 human Earth and Moon stations and their orbital stores supplied, two production updates leave the Moon without fuel; the assertion fails. Once scheduling is corrected, the case also requires the original orbital 6+6→8 batch. Strict import/build passed. Logs: `artifacts/validation/evidence/refining/red-qualified/` (the initial reproduction is in `red/`). An earlier wrong-checkout invocation is retained separately and is not gameplay evidence. This intentionally failing test is not merged into the contribution branch.

Complete allocation/capture and saved-phase mapping, then cover stock exhaustion/replenishment, research gates, ownership, ground/orbit batches, boundary quantities, save/reload, and an ACC fuel wait before the separate refining phase. Windows and original-runtime comparison remain outstanding.
