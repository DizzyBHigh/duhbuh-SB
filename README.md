# duhBuh-SB

A collection of independent Streamer.bot extensions by DizzyBHigh. duhBuh is intended to provide useful streamer automation without requiring one large all-in-one package.

## Project principles

- Extensions are **independently installable** Streamer.bot imports.
- Users should only install the extensions they want.
- The core/UI helpers are reusable implementation infrastructure, not a requirement that every extension be bundled together.
- Selected extensions may be released commercially in the future; free and paid extensions can coexist in the same ecosystem.
- The project is not intended to reproduce another developer's catalogue. Common streamer functionality is common functionality, and new features should be designed independently.
- Existing projects may be used as behavioral references where appropriate, but duhBuh code is independently implemented and does not copy proprietary source, credentials, or private service implementations.
- Migration from existing settings may be supported as a convenience, but migration compatibility is deliberately not a headline feature or marketing message.

## Installation model

Each extension should ultimately have its own Streamer.bot import code/package and documentation. A user installing `Lurks` should not need to install `Watchtime`, `Mod Tools`, or any other unrelated extension.

The repository may contain shared source libraries and development tooling, but distribution should keep extension dependencies explicit and minimal.

## Core extensions

The initial set of extensions planned for duhBuh includes:

- **Lurks**
- **Shoutouts**
- **Mod Tools**
- **Watchtime**

Additional extensions will be added when there is a good reason to build them; the project is not committed to reproducing any particular existing catalogue.

## Longer-term possibilities

Potential future areas include chat utilities, timers, viewer/community tools, OBS/browser widgets, Twitch and YouTube integrations, and other streamer automation. Features will be selected based on usefulness and original design rather than completeness against another project.

## Architecture goals

- Reusable settings/UI helpers for Streamer.bot C# actions
- Twitch-first with YouTube support where it makes sense
- Local/self-hosted browser widgets where practical
- Provider-neutral AI integrations
- Consistent logging, configuration, retries, and error handling
- Small, composable extensions rather than one monolithic plugin

## Streamer.bot compatibility

The project targets the current Streamer.bot C# action model. Streamer.bot's C# actions expose `CPHInline` and the `CPH` API to custom code actions.

See `docs/architecture.md` and `docs/roadmap.md` for development details.
