# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior, with reproducible build and regression checks. Of the 48 backlog tasks, 35 have implementation evidence and four research/internal requirements are locally accepted. These are separate from full gameplay and Windows acceptance.
>
> Normal campaign testing has cleared all six hostile Sol stations, built and fueled the SCG, repaired Jupiter's seven-derrick helium colony and established deuterium deliveries. It then exposed unreachable Star Drone research: the original chassis-completion trigger and older-save repair are now implemented. The campaign has researched and manufactured its first Star Drone with exact charges and save/reload verified. WAYFARER retains 164 drones at Pluto, with one further IOS drone in reserve. Expedition outfitting/resupply, the first interstellar journey and later progression remain open.
>
> Campaign testing also found and fixed stale overview buttons after station loss, blocked News/MTX menu input and unsynchronized MTX scrolling. The latest complete Mac run passes all 639 cases, build, strict import and startup, including the Star Drone discovery and legacy-save correction. The new scroll regression and normal wheel/arrow, scene reopening and save/load checks pass. All 19 tooling checks pass with original-disk verification. Original user saves are restored. Windows retains its earlier 595-case source audit and reported desktop passes across seven task areas; matching newer source/export acceptance and the retained Mac/Windows shutdown failures remain open.
>
> The contribution through `5467807` is published on [codex/build-tests-and-gameplay-fixes](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) in the main repository, including the upstream settings/viewport integration. The Star Drone follow-up is verified locally and not yet pushed. Default-branch integration remains separate. Next are normal SCG/interstellar progression, remaining original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md), [campaign evidence](native-gameplay-results.md) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
