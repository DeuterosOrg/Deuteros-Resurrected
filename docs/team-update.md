# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior, with reproducible build and regression checks. Of the 48 backlog tasks, 35 have implementation evidence and four research/internal requirements are locally accepted. These are separate from full gameplay and Windows acceptance.
>
> Normal campaign testing has now cleared all six hostile Sol stations and unlocked SCG chassis, star drive and HeD research through the normal bulletin. The latest three first-attempt battles lost 140 drones; six factories manufactured 60 and Pluto contributed two captured drones, leaving 97. Exact resource costs, ship state and save/reload reconcile. Earlier losses and failed attempts remain retained. The continuation now researches and builds an SCG, ferries its 40-person crew, refines 325 HeD and fills its tank to 250. All 164 remaining/new drones are consolidated at Pluto, with exact costs and reload verified. Expedition outfitting/resupply, the first interstellar journey, captive-colony repairs and later progression remain open.
>
> Campaign testing also found and fixed stale overview buttons after station loss, blocked News/MTX menu input and unsynchronized MTX scrolling. The latest full Mac run passes all 637 cases, build, strict import and startup, including the scroll correction. The new scroll regression and normal wheel/arrow, scene reopening and save/load checks pass. All 19 tooling checks pass with original-disk verification. Original user saves are restored. Windows retains its earlier 595-case source audit and reported desktop passes across seven task areas; matching newer source/export acceptance and the retained Mac/Windows shutdown failures remain open.
>
> The complete contribution is prepared for publication on [codex/build-tests-and-gameplay-fixes](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) in the main repository, including the upstream settings/viewport integration. Default-branch integration remains separate. Next are normal SCG/interstellar progression, remaining original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md), [campaign evidence](native-gameplay-results.md) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
