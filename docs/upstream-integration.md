# Upstream integration checkpoint — 2026-10-07

The upstream integration at `365b0a6`, included locally through `fb0158d`, now passes **635/635 fresh-process regressions in one complete run**, build, strict import and startup smoke, with exit 0. The isolated run used a separate user-data directory; runtime and tests match the main checkout exactly, and the temporary project setting is restored. Earlier ten coordinate-test failures and their corrections remain retained. Nineteen distinct native cases, manual settings/discard/window-close, 19 tooling checks and the Windows export audit provide the separate evidence described in [the integration report](upstream-integration-results.md). The latest published contribution remains `20ce403`; the integration/export is local only. Windows acceptance, inherited inactive settings options and earlier Mac/Windows shutdown failures remain open.

All 17 conflicts are resolved. The isolated worktree `artifacts/worktrees/upstream-integration`, branch `codex/upstream-integration-20261007`, is clean at `365b0a6`; its merge parents are `20ce403` and upstream `942d993`. The active checkout remains `codex/build-tests-and-gameplay-fixes`, now containing that candidate via local merge `fb0158d`. No default-branch merge, push, PR or Asana write occurred in this integration.

## Evidence and resume

- [Behavior, verification, screenshots and limitations](upstream-integration-results.md).
- Isolated worktree evidence: `artifacts/worktrees/upstream-integration/artifacts/upstream-integration/` (`full-final`, `full-first`, `corrected`, `native`, `manual`, `windows`, `audit.json`, `coverage.json`). Failed logs and the initial tested source patch remain intact.
- Active checkout build/import/startup: `artifacts/upstream-integration-main/`. Its `Godot`, `scripts` and `.github` trees match `365b0a6` exactly.
- The new settings lifecycle case is 119 and bulletin skip is 635. Original later cases shift by one; ACC lamp is now 634. Discover case IDs using `DEUTEROS_TEST_CASE=list` instead of assuming older IDs still apply.
- Earlier pre-war saves remain unchanged. The latest reserve checkpoint is day 19868, funding 400 drones plus a DFCC; follow the [autonomous priorities](autonomous-work-goal.md) for its exact save/hash and normal war preparation. Protect user saves before loading it.

Counts remain **35/48 implementation evidence and four locally accepted requirements**. Upstream exposes placeholder settings and unhandled gameplay shortcuts; these are not accepted functionality. Existing Windows and intermittent Mac shutdown failures remain open. The previous full all-green 633-case checkpoint at `20ce403` is retained independently.
