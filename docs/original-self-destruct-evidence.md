# Original self-destruct controls and station loss

Evidence for Asana task **1215685674676229**, checked 2026-10-02. These are static instruction traces, not an emulator acceptance run. Local installation now has regression evidence; arming, defusing and destruction integration remain incomplete.

## Reproduce the trace

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. The disk segment beginning at `$6E000` loads at RAM `$13000`; use `disk_offset = $6E000 + RAM_address - $13000`. Capstone 5.0.7, big-endian M68000. Local extracts: ignored `artifacts/research/mtx/self-destruct-controls-and-expiry.txt`. Start at the named instruction boundaries; older speculative combat labels are not authoritative.

## Arming and controls

The docking completion at `$313BE` records the docked ship in the station. At `$313EE–$31438`, the hostile-location check, player ship check (ship `+3` bit 7 clear) and war-state check (`$1BF32 == 2`) gate a write of **16** to `$1BE90[planet]`. The station is not made player-owned by this block. This differs from the remake's `ShipInterior.Dock_Pressed`, which immediately clears `ActiveMethanoid` and ignores `SdmInstalled`.

The SDM screen opens at `$35D86`. Its research entry is `$1A124`, item index 18: text ID `$113` resolves to **S.D.M.**. A zero discovery/progress byte at `$1A126` sets `$1C2D0` and closes input; `$37678` consumes that flag through definition `$3760E` (`0012 0000 0004`), requesting SDM discovery and bulletin 4. The screen checks nonzero progress, not explicitly 100-percent completion.

Controls `$35CBE` and `$35D0E` manipulate two switch values in `$3598C`. Combined state `$0002` selects `$35B12`: if the alarm is inactive, set the current planet's countdown to **12**, start its alarm and refresh the display. Combined state `$0200` selects `$35B40`: only while the alarm is active, clear the countdown and stop the alarm. For a hostile station this disarm path also clears its hostile ownership bit, writes completed player-station type 8, and updates ownership/resource state. Defusing therefore participates in capture; a simple automatic explosion on capture omits this control path.

At `$35DB8–$35DD2`, hostile station type 9 plus nonzero Hyperlight discovery/progress (`$1A1C6`) sets a switch restriction. `$35CC4–$35CD2` then rejects one switch change when bit 9 is set. The exact player-facing lock behavior still needs original-screen observation; do not describe this as a research-completion requirement.

## Manufacturing installs the mechanism

Manual production completion at `$233BE–$233EC` recognizes zero-based item `$12` (the SDM). It selects a word in `$1305E` using the production record's byte `+7D`, sets bit 5, refreshes the interface, and bypasses the ordinary stock-credit call at `$233FA`. The automated completion path repeats this at `$2347A–$234A8`. Its queue selector `$23558–$23574` tests that same bit and branches to queue removal when already installed. Thus completion installs the local facility capability instead of creating transferable SDM stock, and automatic repeat stops once installed. Initializer `$2FD74` selects the planet/station record by index (`index * $F6`), writes that index to record `+7D` at `$2FDB2`, and selects `$1305E + index * 2` at `$2FDF6–$2FDFE`. This confirms the capability belongs to the producing local record, rather than every station. The remake maps it to that planet's existing `Station.SdmInstalled` field. Raw extract: ignored `artifacts/research/mtx/self-destruct-installation.txt`.

## Countdown and loss

`$23E60–$23ED2` consumes flag `$20290`, decrements every nonzero planet countdown and calls `$3601A` on zero. The producer `$203AE–$203DC` adds four to a separate timer accumulator and sets the flag at 200: **50 producer calls with `$202C6` clear per countdown update**. This is separate from the simulation-day clock. Its wall-clock rate still needs emulator verification; the older pack's blanket claim that value 150 means three seconds is not supported by this consumer.

There is also a simulation-step removal pass `$35E02`, called at `$23D28`: a high-bit countdown first loses that bit and is skipped; on a subsequent pass a nonzero countdown can trigger immediate station removal. Hostile station type 9 is temporarily exempt while its low countdown is at least 15. Any remake implementation must account for both update paths before assigning seconds or days to these values.

The display `$359E6–$35A2E` divides the low countdown by 60 and renders the remainder as two decimal digits. This supports a minutes/seconds display, alongside the 50-producer-call timer; the assumed PAL wall-clock rate remains unmeasured. The high-bit writer is in the later pirate handler: `$39FBC–$39FC4` stores **$96**, then emits news 11. `$96` has the one-step skip bit plus a low count of 22. It must not be interpreted as a generic three-second delay or reused as the normal manual/docking countdown. The byte timer decrements the stored byte directly; the simulation pass separately clears its high bit. Raw follow-up: ignored `artifacts/research/mtx/self-destruct-pirate-timer.txt`.

### Casualty scope

`$3601A` removes the station ownership/map entry and station type, updates base state, and emits station-loss news type 7. The detailed loss path is narrower than destroying every vessel at the planet:

- `$36100–$3612C` reads the station's berth reference at `+$7E` and hull marker at `+$80`. Marker low bits 2 dispatch the IOS remover `$362B6`; the other occupied berth path dispatches SCG remover `$36362`. It does not walk nearby orbiting ships.
- Those removers clear the selected ship's existence field, release its pilot, process cryogenic occupants, update availability and emit the hull-specific loss news. IOS iterates three module words; the original SCG routine iterates six and also recognizes a special tool-contained team. That extra original slot is not a reason to silently change the remake's current five-slot SCG layout.
- `$36130–$36150` processes the station's four resident team references at `+$84` and its positive production-team reference at `+0`. It does not iterate all ground rosters.
- `$360D0–$360F8` selects the planet's shuttle record in `$1A376` (stride `$18`). An existing shuttle is removed at nonzero planet ID. The Earth/planet-zero slot has an additional state-byte test: only states `$04/$05` enter removal. Dock initiation `$30CD6–$30CE2` sets state 4, action 5 and one pending update; completion `$313BE–$313EA` sets state 5 and writes the berth. Ground landing completion `$3118E–$311A6` instead sets state 0. Thus the Earth shuttle exception preserves its landed and free-flight states, while docking/docked states are casualties. The nonzero-planet branch has no corresponding state filter. The mapping is retained in `artifacts/research/mtx/self-destruct-shuttle-states.txt`.

Raw instructions are preserved in ignored `artifacts/research/mtx/self-destruct-{casualties,loss-and-capture}.txt`. The base-state conversion and original six-slot special-team semantics still require mapping. The recorded first-capture loss remains consistent with a docked capturing ship and crew being destroyed, without establishing losses for every ship in orbit.

## Implemented installation

Paid orbital production now sets the producing station's existing `SdmInstalled` flag without adding stock. Manual and AOC controls reject duplicate installations; existing repeat orders are cleared once installed. Paid construction suspends at a missing or captured station and resumes without another debit. The recipe control locks and identifies installed hardware. Existing pre-fix SDM inventory is preserved; it is not silently consumed or retroactively converted.

Cases 338–347 cover local ownership, independent MTX hardware, one recipe charge, manual/AOC repeat handling, ground rejection, station interruption and save/load. Full Mac and Windows validation passes 349 cases; [validation details](validation-results.md#sdm-local-installation--2026-10-02) retain the failed reproductions and limits. This does not yet supply the arming/defusing gameplay.

## Remaining implementation work

Reproduce the two switches and their Hyperlight restriction on an original screen; verify alarm/countdown cadence and fast-forward behavior; verify the exact neighboring-ship casualty boundary. Then implement persistent arming, safe disarming/capture, destruction cleanup and save/load with deterministic regression coverage. Keep the earlier recorded first-capture loss scenario as an acceptance case. The existing parallel-port rule that only enemy capture of a player station detonates is not adopted.
