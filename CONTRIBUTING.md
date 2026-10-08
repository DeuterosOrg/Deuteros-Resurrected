# Contributing to Deuteros Resurrected

Thanks for helping bring Deuteros back. This is a faithful remake of Ian Bird's
1990 Amiga/Atari ST game, released under CC0. Contributions of all sizes are welcome.

## Reporting bugs and ideas

Open a [GitHub issue](https://github.com/DeuterosOrg/Deuteros-Resurrected/issues/new/choose)
and pick the closest form:

- **Bug** — something is broken in the remake.
- **Fidelity** — the remake behaves differently from the original game.
- **Feature** — a modernisation or quality-of-life idea.

The maintainers also plan work in Asana; issues sync there automatically, so you
don't need an Asana account. Please search existing issues first.

Good reports include the build or commit, platform, the starting game state, exact
steps, what you expected and what happened. Attach a screenshot or short recording
for anything visual. For fidelity reports, cite your evidence — a decoded routine or
table, a reproducible emulator observation, or the manual — and label anything you
are assuming. Behaviour in another remake is supporting evidence, not proof.

Never post security problems in a public issue — see [SECURITY.md](SECURITY.md).

## Making a change

1. Branch from `develop`. Keep one focused change per pull request — bug fixes,
   tooling changes and engine upgrades in separate PRs.
2. Match the surrounding C# style rather than reformatting unrelated code. Keep
   scene node names and paths in sync with their controllers, and preserve asset
   filename case (Linux builds are case-sensitive).
3. Keep generated `.godot/`, build output, local logs, personal configuration,
   credentials and original-game disk or memory dumps out of commits.

## Pull requests

- **Title** uses [Conventional Commits](https://www.conventionalcommits.org/):
  `fix: stop ACC reversing source and destination`, `feat: add self-destruct`,
  `docs: …`, `refactor: …`, `test: …`, `ci: …`, `chore: …`. CI checks this,
  because release notes and version numbers are generated from PR titles.
  Add `!` (`feat!: …`) for a change that breaks existing saves or behaviour.
- **Body** describes the player-visible problem and the result, includes
  `Closes #123` for each issue it fixes, lists how you tested it, and has
  screenshots for visual changes.
- PRs are **squash-merged**, so the PR title becomes the commit on `develop`.
- CI must pass. Run `dotnet build Godot/Deuteros.csproj` locally, then exercise the
  screens you changed, including mouse interaction and transitions — a build does
  not check animation, audio or hit areas.

## Releases

Releases are cut with release-please; see [docs/workflow.md](docs/workflow.md). You
don't need to bump versions or edit the changelog.

## Licence

By contributing you agree your contribution is released under
[CC0 1.0](LICENSE). Third-party material keeps its own licence; say so in the PR.
