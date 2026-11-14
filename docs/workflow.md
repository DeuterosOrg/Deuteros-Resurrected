# How we work

Maintainers push straight to `develop` or open a pull request, whichever is
convenient. There are no branch-protection rules, no required checks, and no
required labels or commit-title format.

The `build` CI job compiles the C# project on pushes and pull requests. It is
advisory only and is not a required check.

Releases are optional: tag when you feel like it. A changelog and numbered
GitHub Releases can be added later if they're useful.

## Asana (optional)

The Deuteros Development Asana board can stay in sync with GitHub Issues so
testers can report on GitHub while planning stays in Asana. That would be a
small service we run on our side, not code in this repo, and it would start
only after a dry run we review together. It needs no labels from maintainers.
If any open Backlog tasks shouldn't become public issues, say so and we'll
leave them out.
