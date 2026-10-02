# Original self-destruct controls and station loss

Evidence for Asana task **1215685674676229**, checked 2026-10-02. These are static instruction traces, not an emulator acceptance run. Local installation and the new control/capture/expiry paths have focused regression evidence; see [implementation and remaining limits](self-destruct-implementation.md). Original-runtime and desktop acceptance remain incomplete.

## Reproduce the trace

Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. The disk segment beginning at `$6E000` loads at RAM `$13000`; use `disk_offset = $6E000 + RAM_address - $13000`. Capstone 5.0.7, big-endian M68000. Local extracts: ignored `artifacts/research/mtx/self-destruct-controls-and-expiry.txt`. Start at the named instruction boundaries; older speculative combat labels are not authoritative.

## Arming and controls

The docking completion at `$313BE` records the docked ship in the station. At `$313EE–$31438`, the hostile-location check, player ship check (ship `+3` bit 7 clear) and war-state check (`$1BF32 == 2`) gate a write of **16** to `$1BE90[planet]`. The station is not made player-owned by this block. The remake previously cleared `ActiveMethanoid` immediately in `ShipInterior.Dock_Pressed`; the new implementation retains hostile ownership until defusal.

The SDM screen opens at `$35D86`. Its research entry is `$1A124`, item index 18: text ID `$113` resolves to **S.D.M.**. A zero discovery/progress byte at `$1A126` sets `$1C2D0` and closes input; `$37678` consumes that flag through definition `$3760E` (`0012 0000 0004`), requesting SDM discovery and bulletin 4. The screen checks nonzero progress, not explicitly 100-percent completion.

Controls `$35CBE` and `$35D0E` manipulate two switch values in `$3598C`. Combined state `$0002` selects `$35B12`: if the alarm is inactive, set the current planet's countdown to **12**, start its alarm and refresh the display. Combined state `$0200` selects `$35B40`: only while the alarm is active, clear the countdown and stop the alarm. For a hostile station this disarm path also clears its hostile ownership bit, writes completed player-station type 8, and updates ownership/resource state. Defusing therefore participates in capture; a simple automatic explosion on capture omits this control path.

At `$35DB8–$35DD2`, hostile station type 9 plus nonzero Hyperlight discovery/progress (`$1A1C6`) sets a switch restriction. `$35CC4–$35CD2` then rejects one switch change when bit 9 is set. The exact player-facing lock behavior still needs original-screen observation; do not describe this as a research-completion requirement.

### Alarm source and playback limits

`$359B0–$359E4` dispatches sound IDs 40/41 to channels 0/1 and ID 42 to channels 2/3, then sets the sound-dispatch lock `$3F752`. IDs 40/41 both reference sample `$3EAC4`, length `$0148` words (656 bytes), period 2000 and volume 63. ID 41 additionally has control word `$0410`: the descriptor reader at `$3FDE0–$3FE0A` counts down 16 update calls while outputting silence. The sample SHA-256 and descriptor bytes are retained in the extract below.

The start handler also writes `count × 140 + 300` to `$79F18`, the period in ID 42's indirect descriptor. That descriptor is silent in the static image; this does **not** establish a countdown-dependent audible pitch. Runtime descriptor changes, hardware timing and stereo playback still need observation. Disarm `$3598E` clears the dispatch lock, resets the channels through `$3F7A8`, then dispatches IDs 38/39. Local screen entry `$21008–$2103A` can restart the alarm from the selected planet's countdown, so an implementation must check navigation behavior as well as the SDM panel.

Reproduction extract: ignored `artifacts/research/mtx/self-destruct-alarm.txt`, SHA-256 `0a6882c69e5e9ab43a5ee28eeaa06743be4ceba7e7d1351fcda7079864c0adb9`. Original listening acceptance remains pending.

The [Amiga Hardware Reference Manual, chapter 5](https://www.amigarealm.com/computing/knowledge/hardref/ch5.htm) specifies signed eight-bit samples, two samples per length word, channels 0/3 on the left and 1/2 on the right, and PAL sample rate `3,546,895 / period`. Thus period 2000 calculates to 1,773.4475 samples/second. The runtime asset `Godot/Sounds/SdmAlarm.wav` converts each signed source byte to a signed 16-bit sample by multiplying by 256, without normalization. For frame `i` from 0 to 1223, left uses `sample[i % 656]`; right is silent before frame 568, then uses `sample[(i - 568) % 656]`. The WAV rate is 1,773; scene pitch `1.0002524` restores the calculated PAL rate and volume `-0.136788 dB` implements 63/64. Import loop mode 2 (Forward) repeats frames 568–1224, retaining both phases after startup. Godot 4.2.2's [WAV importer source](https://raw.githubusercontent.com/godotengine/godot/4.2.2-stable/editor/import/resource_importer_wav.cpp) defines these frame bounds and importer enum.

The 568-frame right delay assumes 16 updates at nominal 50 Hz. This is a calculated reconstruction, not an original recording. The source sample is at disk offset `0x6E000 + 0x3EAC4 - 0x13000`, length 656, in the disk identified above. Raw sample SHA-256: `6f27bb13586049f2694531ba7a8a6b2cd6676e65f261800b907e8facb069ebbc`; WAV SHA-256: `03b4c7e9f13ba4c7b16be429dbe122f56016218fb82846e3acef78b0cc0a4bd6`. Regression 376 checks imported PCM against the source hash and both channel formulas; 379 measures stereo onset in Godot's mixer with a competing normal sound. Native Windows listening and matched original playback remain necessary.


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

Raw instructions are preserved in ignored `artifacts/research/mtx/self-destruct-{casualties,loss-and-capture}.txt`. The original six-slot special-team semantics still require mapping. The recorded first-capture loss remains consistent with a docked capturing ship and crew being destroyed, without establishing losses for every ship in orbit.

### Ground services and rebuilding

The persistent planet-state conversion at `$360BC–$360CC` is now mapped to the service and repair paths: values below 3 remain unchanged, **3→1**, **4→2**, and values at least 5 become **1**. This is one conversion, not successive `5→3→1` gameplay stages.

At `$20C80–$20CB2`, a completed station with nonzero local base state below 3 draws the two unavailable-service overlays. Local extraction `$231EC–$231F4` also skips states below 3. The landed shuttle repair control `$3434A–$343A8` permits states below 3 or captive state 5, and schedules action 6 with countdown 2. Completion `$315B6–$31608` adds 2 to the local state, writes persistent planet state 4, restores capability bits `$0B00` and consumes the tool word. Separately, completing two resource-frame deployments writes state 4 at `$342AE–$342DA`. Thus states 1/2 are service-disabled states; they are not simply “damaged versus healthy with no orbital station.” Their finer distinction is still unassigned.

Station loss therefore records a service-disabled non-Earth colony for subsequent rebuilding, including a previously working state 3/4. Earth retains its special zero state. The remake has separate `BaseBuildParts` and `BaseDamaged` fields, so clearing only `Station.Built` would omit this recovery requirement. State 5 is identified as captive by the station text branch `$211C6–$211D2` (text `$181`, “METHANOID CAPTIVE”); defusing does not itself write the local base-state byte.

The lifetime of resources also matters. Removal clears the planet-to-record mapping and record type at `$36158–$3615C`; it does not zero every store byte there. The later allocator `$2FD74–$2FDA2` clears the entire `$F6`-byte record before reconstructing local state from `$19722`. Preserving inaccessible old store bytes in a reusable original record is **not** evidence that resources return when rebuilding. The roster mapping below explains why retaining a separate colony ground roster would also preserve crews that the original loses.

Reproduction extract: ignored `artifacts/research/mtx/self-destruct-base-recovery.txt`, SHA-256 `d2185231eed4804beab0f73c97c1633847be4a91528a058f66500d3539a161f3`. These are static paths, not a recorded explosion/rebuild session.

### Local crew ownership

The location initializer `$20F04–$21006` selects a `$F6`-byte record and sets `$19D2C` to its four crew words at `+$84`. Both the normal roster renderer `$31BEA` and the empty/grounded shuttle-bay path `$31DC8` read that pointer. Cryopod transfer `$32A92–$32ACA` exchanges a selected entry through the same pointer. Earth ground initialization instead writes `$19E0A` to `$19D2C` at `$20DBC`; that roster is separate from Earth's orbital record.

Loss `$36130–$36150` visits all four local crew references and the orbital production team. Reallocation clears the record. These paths establish a shared local crew roster outside Earth, rather than an independently surviving non-Earth ground roster. The remake keeps its existing separate ground/orbital arrays, but now clears both local arrays on non-Earth SDM loss. Earth's ground array, other colonies and crews aboard surviving ships remain intact. This mapping is supported by static instructions; an original-runtime casualty comparison is still useful for acceptance.

Reproduction extract: ignored `artifacts/research/mtx/self-destruct-ground-rosters.txt`, SHA-256 `5ab0e9d93780d02bd80e75e8e682aac47d2136b9722e68a440b9e4f004804993`. Case 375 reproduced the retained-ground-crew failure, then passed both timer paths, Earth/non-Earth boundaries, travelling pilot/cryo survival and save/reload after the correction.

## Implemented installation

Paid orbital production now sets the producing station's existing `SdmInstalled` flag without adding stock. Manual and AOC controls reject duplicate installations; existing repeat orders are cleared once installed. Paid construction suspends at a missing or captured station and resumes without another debit. The recipe control locks and identifies installed hardware. Existing pre-fix SDM inventory is preserved; it is not silently consumed or retroactively converted.

Cases 338–347 cover local ownership, independent MTX hardware, one recipe charge, manual/AOC repeat handling, ground rejection, station interruption and save/load. Full Mac and Windows validation passes 349 cases; [validation details](validation-results.md#sdm-local-installation--2026-10-02) retain the failed reproductions and limits. The subsequent [gameplay implementation](self-destruct-implementation.md) adds arming/defusing and expiry; its verification is recorded separately.

## Remaining implementation work

Reproduce the two switches and their Hyperlight restriction on an original screen; verify alarm/countdown cadence and fast-forward behavior; verify the exact neighboring-ship casualty boundary. The remake now implements persistent arming, disarming/capture, destruction cleanup and save/load with focused regression coverage. Finish the original alarm and runtime timing/casualty observations; the local roster lifecycle is now mapped above. Keep the earlier recorded first-capture loss scenario as an acceptance case. The existing parallel-port rule that only enemy capture of a player station detonates is not adopted.
