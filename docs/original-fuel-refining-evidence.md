# Original fuel refining

Follow-up for **1215685674676225 — event order**, 2026-10-02. Refining quantities and mineral identities are now mapped. The remake starvation failure is reproduced and corrected in the isolated refining branch; full 450-case Mac validation and package audit pass; Windows execution and acceptance remain pending.

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

These are pre-addition comparisons, not a clamp to 50,000: some boundary values can finish above that number. The correction preserves these comparisons rather than silently imposing a new clamp. No other-ground HeD branch appears in this function. Ground processing additionally requires record `+$F0 >= 3`. Source states 1/2 have disabled services, 3/4 working services and 5 captive services; the remake uses a completed, undamaged ground base after the outer friendly completed-station gate.

## Phase and station allocation

The phase byte `$36848` increments once per eligible function call, independent of stock availability. Earth ground runs on even parity. Local processing selects alternating records in the **98-record** table at `$13810`, skipping owner bit 7 and station types below 8. This is separate from the 160 body IDs.

The station-system offsets at `$19646` are **0,16,20,32,48,54,56,72,82**. Allocator `$2FD1C–$2FD42` finds a free bit in the system bitmap and adds that system offset; `$2FD74` initializes the resulting record. Body ID is stored separately at `+$EF` and mapped through `$1965A`. When initialization finds type 9 in the chosen slot, `$2FF4C` scans the current system for a free record. `$2FE94` swaps the complete `$F6` bytes and repairs embedded record references and body-to-record lookups. Thus another station can move records, changing its refining parity. Capture `$35B8A–$35BBC` calls the first-free-human allocator, compares its result with the captured record and swaps complete records through `$2FE94` when necessary. The displaced occupant takes the captured station's former slot. Planet enumeration order or a changing human-station ordinal is not sufficient evidence for that mapping. The additional extract is `artifacts/research/refining/station-relocation.txt`.

## Initial records and save compatibility

Capacities at `$1963C` are **16,4,12,16,6,2,16,10,16**. Sol initializer `$37460` places Uranus, Titania, Neptune, Triton, Pluto and Jupiter in local slots **10–15**, following body IDs at `$37454`. Other-system initialization `$372C6` uses base words **19,25,33,50,55,57,73,83** and a descending remaining-count counter: initial alien stations occupy the highest local slots. The remake retains its existing random choice of non-Sol bodies; this correction maps their allocation order, not the original body-selection algorithm.

The new `SpaceStation.RefiningSlot` and `SaveFile.RefiningPhase` preserve allocation and phase across saves. Construction displaces an alien occupant into the first empty slot; SDM capture swaps its old slot with an occupant of the first free human slot. Destruction already replaces the station, freeing its allocation. Enemy capture also preserves the slot: `$38E5A–$38E62` saves the existing body-to-record index, `$3601A` clears that record (type zero at `$3615C`), and `$38EE6–$38EF4` reinitializes the saved index as type 9. It does not invoke the human first-free-slot allocator. Extracts: `enemy-actions.txt` and `enemy-capture-reset.txt` in the same evidence directory.

Older saves have no recoverable original allocation: missing slots start at -1 and are assigned deterministically on first use, friendly stations first by ordinal/body ID, then hostile stations from the highest free original slots. Serialization does not mutate the world. New saves use the traced initial slots. Existing remake worlds above original capacities retain distinct overflow slots; this patch does not impose a new construction limit or discard stations. Older executables may reject the added fields, so retain save backups.

Capture/initialization extract: `artifacts/research/refining/capture-initial-allocation.txt`, SHA-256 `7c716bd704bb90b445df3c1b304161e9bb35ffb15b25bcecf98d9a9fd8e83a46`.

## Reproduced remake failure and next checks

The remake toggles each shared item's `AutoProduceFlip` inside the factory loop, only when ingredients are available. Two consistently supplied factories therefore leave the first producing on every call and the second producing nothing. It also uses the same 2+2→3 recipe across contexts, excludes ground HeD, and lacks these output thresholds.

Isolated branch `codex/refining-investigation`, commit `6bab36d`, adds **case 445**. With completed, type-8 human Earth and Moon stations and their orbital stores supplied, two production updates leave the Moon without fuel; the assertion fails. Once scheduling is corrected, the case also requires the original orbital 6+6→8 batch. Strict import/build passed. Logs: `artifacts/validation/evidence/refining/red-qualified/` (the initial reproduction is in `red/`). An earlier wrong-checkout invocation is retained separately and is not gameplay evidence. This intentionally failing test is not merged into the contribution branch.

The implementation removes refining from the factory loop and registers one post-ship model phase. Focused cases **445–449** pass: station fairness, all five batch boundaries and eligibility gates, saved phase/legacy migration/malformed allocation rejection, construction/capture/loss allocation, and actual simulation ordering where ACC consumes refined fuel on the following update. Case 337 now tests original Earth MeH and HeD batches. Case 450 covers traced startup slots and preservation of worlds exceeding original station capacities. Evidence is under `artifacts/validation/evidence/refining/green/`; native cases 445/447–450 also pass. Full 450/450, nine Python checks, strict import, source startup and Windows cross-export pass at `f913679bf513d2ca9b44ecc1b2820df133bf34f1`; individual logs and the package are audited in `full-run-f913679/`. These are scripted checks, not Windows desktop acceptance.

The remake still consumes whole-day updates. Original fractional/star clocks and the remaining production/research/attrition ordering are separate outstanding work; this fix does not establish full original timing fidelity. Windows and original-runtime comparison remain outstanding.
