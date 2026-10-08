# How we work

## Life of an issue

1. Someone opens a GitHub issue (bug, fidelity gap or feature), or a maintainer adds
   a task in the Asana project. Either one appears in the other automatically.
2. Maintainers set priority and assignee in Asana; they show on the issue as
   `priority:*` labels and the GitHub assignee. Moving the task to **In Progress**
   adds `status:in-progress`.
3. Work happens on a branch and lands as a squash-merged PR into `develop` with
   `Closes #N` (or `Fixes` / `Resolves`) in the description. The issue closes and
   gets `staged` (fixed on `develop`, not released yet). The Asana task moves to
   **Done**. Plain mentions like "see #N" or "part of #N" don't stage anything.
4. When the next release is published, `staged` comes off and the Asana task moves
   to **Released**.

Closing an issue or completing its Asana task closes both; reopening either reopens
both. **Done** and **Released** are set automatically from merges and releases, so
don't drag tasks into them by hand. Tick the task complete instead (or close the
issue), and the sections will follow.

## Labels

Defined in [`.github/labels.yml`](../.github/labels.yml). Edit that file to change them.

| Group | Labels | Set by |
|---|---|---|
| Type | `bug`, `enhancement`, `fidelity`, `documentation`, `tech-debt`, `performance` | Issue forms / triage |
| Area | `area:ui`, `area:simulation`, `area:assets`, `area:audio`, `area:tooling` | Triage |
| Priority | `priority:high`, `priority:medium`, `priority:low` | Asana (synced) |
| Status | `status:in-progress` | Asana (synced) |
| Status | `staged` | Automatic on merge; removed on release |
| Process | `good first issue`, `help wanted`, `question`, `duplicate`, `invalid`, `wontfix`, `do-not-merge`, `security`, `dependencies` | Anyone triaging |

## Releases

We use [release-please](https://github.com/googleapis/release-please). Every merge
to `develop` updates an open **release PR** that bumps `version.txt` and writes
`CHANGELOG.md` from the PR titles since the last release. Merging the release PR
tags `vX.Y.Z` and publishes a GitHub Release.

- `fix:` / `perf:` → patch (0.1.0 → 0.1.1)
- `feat:` → minor (0.1.0 → 0.2.0)
- `!` (breaking) → minor while we're below 1.0, major after
- `docs:`, `refactor:`, `test:`, `ci:`, `build:`, `chore:` → no release on their own

Merge the release PR when there's a useful batch. Never auto-merge it. The first
release will be **0.1.0**. Attaching Windows/Linux builds to releases comes next,
once the export tooling is on `develop`.

## One-time repository setup (admin, after approval)

These are settings, not files, so they're applied by hand after this proposal is
accepted:

1. **Ruleset on `develop`:** require a pull request; require status checks `build`,
   `hygiene` and `pr-title`; squash merge only; block force pushes and deletion.
   Allow admin bypass for emergencies.
2. **General → Pull Requests:** allow squash merging only; default commit message
   "Pull request title and description".
3. **Security:** enable private vulnerability reporting, Dependabot alerts, secret
   scanning and push protection.
4. **Actions → General:** workflow permissions "Read repository contents".
5. **GitHub App "Deuteros Bot"**, owned by DeuterosOrg and installed on this
   repository only, with Contents, Issues and Pull requests: read and write
   (Metadata: read). Add repository variable `RELEASE_APP_ID` and secret
   `RELEASE_APP_PRIVATE_KEY`. Release-please and the Asana sync both use it, so their
   changes show as "Deuteros Bot".
6. Run the **Labels** workflow once (Actions → Labels → Run workflow).
