# Upstream integration checkpoint — 2026-10-07

The tested contribution remains on `codex/build-tests-and-gameplay-fixes` at `20ce403`. Upstream `develop` is `942d993`; six commits are not yet integrated. A read-only `git merge-tree --write-tree --name-only 20ce403 origin/develop` reports 17 conflicting files. The preview is retained in `artifacts/integration-633-20261007/upstream-merge-preview.{txt,json}`.

Integration has started only in `artifacts/worktrees/upstream-integration`, branch `codex/upstream-integration-20261007`. Its unchanged `20ce403` baseline compiled successfully before `git merge --no-commit --no-ff origin/develop`. The merge remains in progress and **is not a validated candidate**. No default-branch merge or publication of this candidate has occurred.

## Resolutions staged so far

- Preserve the contribution's input ownership, prison controls, trade decisions, original transmission starts, mining fixes and ACC lamp; use the upstream cursor paths in Ship Bay/Interior.
- Combine upstream pause-aware battle/dialogue timers with existing cancellation/lifetime guards.
- Retain the separate training door sound while adopting Music/Effects/Interface routing. Those buses feed the existing Game bus so SDM alarm muting still controls ordinary game audio.
- Retain overlay identity and synchronous shutdown cleanup alongside upstream game-area scaling. Shutdown also detaches the new layout listener.

These resolve 11 of the 17 conflict files in the index; they have not yet been built or exercised as a merged runtime.

## Remaining integration work

Six files remain unmerged: `GameCore.cs`, `Objects/GameConfig.cs`, `Objects/GameData/SaveFile.cs`, `Platform/Screens/Bulletins.cs`, the old `Platform/Screens/Settings.cs` and `Screens/Base/Settings.tscn`.

The upstream changes move gameplay into `Master/GameContainer/GameViewport`, replace Settings with `SettingsScreen`/`SettingsManager`, add Apply/Revert and key binding rows, and optionally skip bulletin typing. Their alien-message scheduler overlaps the contribution's original-backed `AlienTransmissions` controller. Preserve the latter's sequence, saved progress and ordering; do not run both schedulers.

Resolve behavior, not just conflict markers:

1. Migrate all remaining runtime and regression references to the new game viewport, including shutdown, screen lookup, cursor ownership and pointer coordinates. Do not restore the old unblocked InputBlocker navigation handler.
2. Preserve settings file isolation, malformed-value handling, save-error feedback and native resource disposal while adopting upstream Apply/Revert. Establish migration for existing audio/display keys. Carry the tested cheat confirmations, progression presets and session-only behavior into the new UI; upstream's copied preset bodies do not prove those fixes survived.
3. Preserve the contribution's saved `AlienTransmissions` controller rather than adding upstream's parallel `NextAlienMessageDay`/`AlienTransmissionsReceived` scheduler. Read-only inspection of `942d993` confirms its Save screen still uses `NoScript.cs`; the only `Serialization.WriteObject`/`ReadObject` calls persist `BaseData` to `GameData.dat`, not an active `SaveFile`. No upstream gameplay-save producer for those two fields was found, so a speculative migration is not justified. Remove their duplicate initialization/dispatch together, while retaining all existing contribution save compatibility checks.
4. Add optional bulletin skipping to the cancellable, owned-lock typing flow, preserving alien decoding, notice acknowledgement and shutdown. Keep the recovered message content and original event sequence.
5. Check auto-merged files too, especially `CoreData.cs`, `Enums.cs`, `Unlocker.cs`, `Master.tscn` and `project.godot`. Resolve compile errors, then reproduce any remaining integration failures with focused regressions before fixes.
6. Run focused input/settings/save/bulletin/audio/shutdown checks, native viewport and overlay interactions, and the full suite/export audit before bringing this candidate back to the contribution branch. Protect the normal campaign checkpoints and user saves throughout.

The contribution's full 633-case run is independent of this unfinished merge. Results at `20ce403` cannot certify the upstream candidate.
