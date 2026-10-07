# Deuteros contribution update

Draft for Discord, 7 October 2026. Not posted.

> The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior, with reproducible build and regression checks. Of the48 backlog tasks,35 have implementation evidence and four research/internal requirements are locally accepted. Full gameplay and matching Windows acceptance remain separate gates.
>
> The normal campaign has cleared Sol, built the SCG, captured both Proxima stations, researched Hyperlight and promoted two crews to Warlord. The latest continuation recaptured Atlantic, recovered the first artifact and credited12% research. It retained72 SCG drone losses and returned all29 crew safely to Earth; the SCG is docked with68 drones/250fuel. WAYFARER remains atPluto with200 drones under Floyd's Warlord crew. Earlier failed battles, capture attempts and the failed artifact approach remain preserved. Save/reload is exact apart from elapsed timers, and original user saves are restored.
>
> Native artifact recovery exposed a DFCC routing bug: clicking a fitted grapple at a friendly station opened fleet transfer. The shared handler now routes the selected tool correctly while retaining hostile combat interception. Its new regression fails on the original code and passes after the correction; the native campaign then captures and analyses the artifact normally. The fresh full Mac run passes646/646 regressions, all19 tooling checks, build, strict import/startup and source/log audit with exit0. Matching newer Windows source/export acceptance and earlier retained shutdown failures remain open; Windows' earlier595-case audit and reported desktop coverage keep their original revision limits.
>
> The main repository's [contribution branch](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) is published through `e58e7ed`; the subsequent Pluto defence, first-artifact evidence and routing correction remain local. Next are fleet/material funding, seven remaining artifact recoveries, later progression, original-behavior gaps and matching Windows acceptance.

See [the backlog](backlog-progress.md), [current validation](validation-results.md#friendly-dfcc-module-routing--2026-10-07), [campaign evidence](native-gameplay-results.md#atlantic-recapture-and-first-artifact-recovery--2026-10-07) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
