# Star Drone discovery and first normal manufacture — 2026-10-07

Completing SCG chassis research never unlocked Star Drone research in the remake. The item and its existing `SCG_Drone` bulletin were otherwise present, so staged fleet/production tests passed while the normal campaign could not obtain an SCG battle fleet. This is a progression correction within the SCGs task, not an additional accepted backlog task.

## Original trigger

Pinned Disk 1 SHA-256: `6ea0cc68d3af37203a885032eddf7c28e839e6abb59d8c9cd3792f1308bdec38`. Research completion reads the item's one-based ID at `$236F4`. `$23734–$2374A` compares it with `$0D` (SCG chassis) and calls `$37760`. That routine checks the chassis research byte at `$1A036`, then selects the unlock list at `$37608`.

The list is the three big-endian words `29, 0, 7`: zero-based item 29, terminator, bulletin 7. The common dispatcher `$377B0–$377DE` indexes the 40-byte research records at `$19E54`, changes an undiscovered item's progress byte from zero to one, and displays the bulletin. Record 29 has one-based header `$1E`, identifying Star Drone. This unlocks research; it grants neither a completed technology nor stock. It follows chassis completion, not drive completion, fitting a DFCC or arrival at another star.

Reproduce aligned instruction and pinned-byte checks with `uv run --offline --with capstone==5.0.7 python artifacts/star-drone-discovery-20261007/verify-original.py`. The script and `original-trace.txt` are retained locally. This is source evidence, not a new original-emulator observation.

## Implementation and checks

`Unlocker` now discovers Star Drones on the existing chassis-research completion event and uses the existing queued bulletin path. Its shared discovery check requires completed chassis research and an undiscovered, unfinished Star Drone project. Loading an older completed-chassis save uses that same check and queues the missing bulletin behind any existing notices. It does not replay research, grant stock, reset progress or change the save format. Repeated completion/load does not repeat discovery.

New cases **638–639** both fail before the change and pass afterward. They exercise normal research calculation, the different drive/chassis triggers, modal ownership, saved bulletin delivery, Research-button selection and Star Drone completion. The legacy case compares the entire serialized world after accounting for only the intended lock/queue changes, preserves earlier bulletin priority, and verifies idempotence and already completed drone research. Red/green logs and builds are in `artifacts/star-drone-discovery-20261007/`.

The complete isolated run passes **639/639 fresh-process regressions**, all **19 tooling checks with original-disk verification**, build, strict import and startup, exit **0**. The per-case/source audit confirms the tested Godot/scripts files match the contribution checkout and restores the isolated project setting. Build retains 14 existing warnings and zero errors. Logs, runnable audit and `result.json`: `artifacts/star-drone-full-validation-20261007/`. Matching Windows source/export acceptance is pending.

## Native campaign

The native build loads the unedited day-21530 Jupiter checkpoint. A save before advancing time differs only in the Star Drone research lock, one queued discovery and elapsed timers. Cavell's existing 241-person team then completes research in 15 manual updates, gaining one action (22→23). The recovered bulletin is readable and the normal Research button selects SCG Battle Drone.

Transfer 139 silver and 251 titanium from Titania to Earth, and balance copper with Jupiter, moving 224 copper to Earth. Copper replenishment also funds one existing repeating IOS order before it is disabled; retain that outcome. Queue exactly one Star Drone. Both orders finish, the Earth queue becomes empty, Titania's drone route is restored, and the new IOS drone reaches Pluto's pool. Combined material charges are exactly 420 iron, 320 titanium, 420 aluminium, 15 carbon, 155 copper, 120 palladium, 110 platinum, 95 silver and 50 gold, matching one IOS and one Star Drone at the current recipes.

Primary checkpoint: `artifacts/star-drone-native-20261007/reloaded-slot-5.json`, SHA-256 `3c970c19f2985cb205405bb5c867d01a44753f2944f187f79f270a365fa56752`, **day 21559**, date `3121 464.95`, fast time off. Earth has one Star Drone; Pluto has one reserve IOS drone plus WAYFARER's unchanged 164 onboard. SCG300000 remains docked Earth with Thackray/40, 250 fuel and six empty mounts. PROSPECTOR/FIRST LIGHT retain 62/46 fuel. Earth/Jupiter/Uranus hold 265/90/35 HeD. The Sol enemy fleet is 75, trigger 80, with no active attack/countdown; stage-four transmission is due in four updates.

Exact migration, two-recipe charges, transfer conservation, fleet state, save/reload comparison, strict native log, exit 0 and original-save hash restoration pass in the local `audit.py`/`audit.log`. The owned game is closed. Native manufacture verifies the implemented recipe, not every original recipe constant: the original Star Drone record contains 300 titanium while the existing remake recipe uses 200; that discrepancy needs a separate correction and legacy-queue check. No recipe was changed in this discovery fix.

The campaign still needs fleet funding, DFCC/fuel outfitting, Sol defence, interstellar travel and later ending acceptance. Counts remain **35/48 implementation evidence and four locally accepted requirements**. Ignored saves, original disks and logs do not travel with Git.
