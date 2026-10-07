# Upstream integration checkpoint — 2026-10-07

The upstream integration is committed locally as `365b0a6` and included in the contribution branch at `fb0158d`. Its 635-case full pass had 625 passes and ten outdated overlay-coordinate test failures; all ten corrections and 23 targeted cases now pass. Nineteen distinct native cases, manual settings/discard/window-close, 19 tooling checks, startup and the Windows export audit pass. This is [aggregate evidence with explicit limits](upstream-integration-results.md), not a second all-green 635-case run. The main checkout rebuilt/imported/smoked cleanly and has identical runtime source. The latest published contribution remains `20ce403`; the new integration/export is local only. Windows acceptance, inherited inactive settings options and earlier Mac/Windows shutdown failures remain open.

All 17 conflicts are resolved. The isolated worktree `artifacts/worktrees/upstream-integration`, branch `codex/upstream-integration-20261007`, is clean at `365b0a6`; its merge parents are `20ce403` and upstream `942d993`. The active checkout remains `codex/build-tests-and-gameplay-fixes`, now containing that candidate via local merge `fb0158d`. No default-branch merge, push, PR or Asana write occurred in this integration.

## Evidence and resume

- [Behavior, verification, screenshots and limitations](upstream-integration-results.md).
- Isolated worktree evidence: `artifacts/worktrees/upstream-integration/artifacts/upstream-integration/` (`full-first`, `corrected`, `native`, `manual`, `windows`, `audit.json`, `coverage.json`). Failed logs and the initial tested source patch remain intact.
- Active checkout build/import/startup: `artifacts/upstream-integration-main/`. Its `Godot`, `scripts` and `.github` trees match `365b0a6` exactly.
- The new settings lifecycle case is 119 and bulletin skip is 635. Original later cases shift by one; ACC lamp is now 634. Discover case IDs using `DEUTEROS_TEST_CASE=list` instead of assuming older IDs still apply.
- The day-15901 funded pre-war save remains unchanged. Continue the [autonomous priorities](autonomous-work-goal.md), including replacement reserves and normal later progression. Protect user saves before loading it.

Counts remain **35/48 implementation evidence and four locally accepted requirements**. Upstream exposes placeholder settings and unhandled gameplay shortcuts; these are not accepted functionality. Existing Windows and intermittent Mac shutdown failures remain open. The previous full all-green 633-case checkpoint at `20ce403` is retained independently.
