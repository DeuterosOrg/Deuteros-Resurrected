# Save files

Open the disk icon in the left menu. Five slots are available. Saving to an occupied slot asks before replacing it; loading asks before replacing the current game. Loading returns to Master Control with time advancement stopped. In an early game with no stations, use the Earth icon to return to the ground screen.

Files are stored in Godot's `user://saves` directory as `slot-1.json` through `slot-5.json`. The previous contents of an overwritten slot are retained as `slot-N.json.bak`. Save files and backups belong to the player, not the source repository. To recover a backup, close the game, preserve the current file, and copy the `.bak` file to its matching `.json` filename.

## Format and load boundary

Version 1 stores the current world's planets, stocks, crews and experience, training, production/research progress, ships, cargo, ACC settings, news, unlocks, war state and enemy scheduling position. Shared item/research references and ACC-to-ship references are preserved. Text/palette definitions come from the game version; transient input locks and running fast-forward timers are not restored.

The loader accepts only the known model subtypes. It rejects unknown versions/types, missing required fields, invalid references and selected invalid states before replacing the active world. This format is specific to this remake; it does not import original Amiga save files. Changes to persisted fields require a version/migration decision and regression coverage.

A save is serialized and validated before disk replacement. Its temporary file is flushed, then moved into an empty slot or replaces an existing slot with a backup. A failed write leaves the previous slot intact. Native Windows replacement/backup behavior remains part of the Windows acceptance pass.

## Saves from before the assembly stock fix

Earlier builds left chassis and drives in stores when fitting them. New fitting deducts one part, and dismantling returns the installed parts subject to capacity. Existing saves contain no record of those historical deductions, so loading preserves their inventories; the fix does not reconstruct earlier spending. Record the originating build when comparing old-save stocks during acceptance.

## Engine damage compatibility

Version 1 now includes `EngineDamaged` for ships. Saves written before this field existed load with healthy drives (`false`); no damage is inferred from an old attack counter. Explicit null values remain invalid, and the existing required-field checks remain enforced. Damage and repairs survive save/load, including the remaining duration of a damaged journey. A damaged drive yields no usable spare when dismantled.

This is backward reading compatibility in the updated game. Older builds reject the new field, so preserve a backup before moving a save between revisions.

## DFCC fuel compatibility

`Fuel` remains gauge units. A fitted DFCC now costs ten store units per gauge unit when loading, unloading or dismantling; ordinary ships remain 1:1. No new saved field or format version is needed. Already-loaded tanks retain their current amounts and range. Their later refunds use the corrected ratio, so a tank filled before the fix can return more stock than was originally paid. Historical spending is not reconstructed. New DFCC fitting first returns the old tank at its old rate, preventing that gain in new fitting cycles.

## Verification

The engine regressions cover private staff/news state, all ship subtypes, active research/production references, training, cargo, ACC ownership/cursors, corruption rejection, overwrite backups and failure preservation. Actual screen callbacks exercise save, overwrite/cancel, load/cancel, corrupt-file feedback advancing the restored game, and a mid-flight ship arriving on schedule. Mac pointer checks cover visible controls and loading; Windows gameplay and long progression still require acceptance.

Day updates now run the simulation event before the display event. The simulation resolves planets from the active save each tick; loading does not leave old worlds subscribed. Existing UI-driven actions, such as choosing a manual production recipe, remain screen behavior. The fixed simulation order preserves this repository's previous order, rather than claiming a complete reconstruction of the original game's timing.

## Staff attrition compatibility

Version 1 now includes `Staff.AttritionCountdown`. Missing values from older saves default to zero, matching original team allocation; explicit null is invalid. Loading preserves member counts and applies no historical attrition. The next crossed 100-day boundary begins normal processing. Current saves retain each countdown and the global phase through the displayed clock, including teams inside cryopods. Older game builds reject this new field; retain pre-upgrade backups.

## Self-destruct compatibility

Version 1 now includes `SpaceStation.SdmCountdown` and `SaveFile.SdmTimerRemainder`. Older saves missing either field default it to zero; loading does not invent an armed station. Current saves preserve the countdown and subsecond phase, including the simulation skip bit. Explicit nulls, countdowns outside 0–255, non-finite phases and phases outside [0, 1) are invalid. Switch positions are screen state derived on entry, rather than a second saved source of truth. Older builds reject these new fields; preserve backups. See [gameplay limits](self-destruct-implementation.md).

## Fractional clock and automation slots

Version 1 now saves `Clock.DateCentidays`, `Clock.NormalElapsed` and `Clock.PendingIncrement`. `CurrentDay` remains the consumed-update counter, preserving existing training and travel timestamps. A legacy save without `Clock` derives its displayed date from `CurrentDay * 100` and scales its old whole-day `EnemyBuildDay` deadline by 100. New deadlines retain exact centidays. Loading never synthesizes catch-up updates or discards a saved pending increment; fast-forward still stops on load.

The loader rejects explicit null clocks, missing required clock members, non-finite/out-of-range elapsed time, invalid pending increments and dates/deadlines outside the supported range before activating the save. Staff age at displayed calendar boundaries rather than consumed-update counts.

Interstellar ships now save `AutomationSlot` for scan/mining phases. Missing legacy values allocate deterministically into free slots: IOS per star, SCG globally. Surviving ships keep their slot after another is removed or the collection reordered. Existing fleets above sixteen ships are preserved. Duplicate allocated slots within a pool and invalid negative/null values are rejected.

Older builds reject these added fields. Preserve backups and record the originating revision when moving saves between builds. Cases 476–484/486/490–492 cover mixed timing, legacy migration, pending updates, slots, countdowns and malformed-state rejection.
