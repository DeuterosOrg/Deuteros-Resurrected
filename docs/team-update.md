# Deuteros contribution update

Draft for Discord, 8 October 2026. Not posted.

The contribution covers gameplay, ship controls, production, save/load, recovered artwork and original-game behavior. Of48 backlog tasks,35 have implementation evidence and four research/internal requirements are locally accepted. Normal gameplay and matching Windows acceptance remain separate gates.

The latest full Mac checkpoint, frozen runtime `52bf383`, passes664/664 fresh-process regressions,19 tooling checks with original-disk verification,build,strict import/startup and exit0. Exact-source/log audits pass. Recent additions include keyboard navigation, separate Quick Save/Autosave slots, display scaling/scanlines, Tooltips, Auto Pause, manual launch confirmation, calendar speed and optional News alerts. Focused/native/physical evidence and its limits are recorded in [validation results](validation-results.md#event-alerts-preference-and-persistent-news--2026-10-08). Classic Audio remains deferred; Edge Scroll and Pointer Speed remain inactive. Compatibility-bug toggle requirements still need clarification.

The current normal campaign is day33540, Sol `3133 342.00`, fast time off. Both fleets have200 drones: SCG atEarth with249fuel/Admiral Kingston28, WAYFARER atUranus with173fuel/Floyd32. WAYFARER defended Uranus, retaining67 losses before normal replacement. Jupiter's67 IOS recipes,136 new Star Drones and two material resupplies toMars are audited. All routes are restored, ownership/SDM state unchanged and crew attrition retained. Exact reload, source/profile and clean native exit pass. [Campaign evidence](native-gameplay-results.md#full-replacement-scg-fleet-and-uranus-defence--2026-10-08).

Next inspect interstellar travel and reinforcement logistics before combat, including Kingston's normal Hyperlight-exit Warlord promotion. Earth has only one spare Star Drone, so the full initial fleet does not yet establish sustained expedition readiness. Retain WAYFARER for Sol. Seven artifacts and later normal progression remain open; previous defeats and the Cercops capture are preserved.

The [main-repository contribution branch](https://github.com/DeuterosOrg/Deuteros-Resurrected/tree/codex/build-tests-and-gameplay-fixes) is published; the latest verified publication before this documentation update is `9a5cd04`. No PR or default-branch merge has been created. Matching Windows execution remains pending: Desktop Commander was freshly checked and Wizzo-Game-1 remains offline (last seen66h). The earlier648-case Windows export retains its1,259-hash audit and unexecuted status; earlier reported595-case coverage and shutdown failures retain their original revision limits. No matching664 Windows export or acceptance is claimed.

See [the backlog](backlog-progress.md), [current goal](autonomous-work-goal.md) and [Windows results](windows-validation-results.md). Ignored local saves, logs and export packages do not travel with Git.
