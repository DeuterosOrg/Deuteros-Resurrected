# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior. Of the48 backlog tasks,35 have implementation evidence and four research/internal requirements are locally accepted. Full gameplay and matching Windows acceptance remain separate gates.
>
> Normal play has cleared Sol, built the SCG, researched Hyperlight, promoted two crews to Warlord and recovered the first artifact for12% research. The latest day27466 checkpoint has WAYFARER atNeptune with200 drones/67fuel/Floyd35, and SCG dockedEarth with68 drones/250fuel/Raphael29 plus97 Star drones in reserve. The Neptune defence lost57 drones; all57 replacements were manufactured and loaded normally. Original routes are restored, exact reload passes, and original user saves are restored. Earlier failures and natural crew attrition remain preserved. Both Proxima stations are hostile again; the artifact credit persists.
>
> Native progression exposed two corrected defects: fitted tools on friendly DFCC ships opened fleet transfer, and enemy fleets repeatedly attacked already-hostile stations and reset their stocks. Both have reproduced failures, original-source support, regressions and normal campaign checks. Corrected play leaves Proxima idle while allowing a valid Neptune attack and defence.
>
> The current full Mac run passes648/648 regressions, all19 tooling checks, build, strict import/startup and source/log audit with exit0. The first aggregate's unbuilt-station test-fixture failure is retained and corrected. A matching local Windows export passes all1,259 payload hashes and runtime/source checks, but has not run onWindows. Windows' earlier595-case audit and reported desktop coverage keep their original revision limits; earlier shutdown failures remain open.
>
> The main repository's [contribution branch](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) is published through `e58e7ed`; subsequent work remains local. Next are Star fleet funding, seven remaining artifact recoveries, later progression, original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md#enemy-target-ownership--2026-10-07), [campaign evidence](native-gameplay-results.md#neptune-replacement-production--2026-10-07) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
