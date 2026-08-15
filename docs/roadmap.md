# duhBuh roadmap

The roadmap is intentionally feature-driven rather than a checklist of another project's catalogue. Standard streamer functionality is fair game, and new ideas should be designed independently.

## Foundation

- [x] Repository initialized
- [x] Shared settings/UI helpers
- [x] Settings persistence conventions
- [ ] Common logging helpers
- [ ] Twitch helpers
- [ ] YouTube helpers where useful
- [ ] HTTP/retry/cache helpers
- [ ] Independent extension import/package conventions
- [ ] Release workflow for individual extensions

## Initial duhBuh extensions

These are the first four extensions to prioritize:

- [ ] **Lurks** — viewer lurk tracking and configurable chat responses
- [ ] **Shoutouts** — streamer shoutout automation, with optional AI/provider support
- [ ] **Mod Tools** — practical moderation utilities
- [ ] **Watchtime** — viewer watch-time tracking and statistics

Each will be independently installable. Users will not need to install the rest of duhBuh to use one extension.

## Later ideas

Possible future work includes viewer/community tools, chat utilities, timers, OBS/browser widgets, Twitch/YouTube integrations, games, and other streamer automation. These are intentionally not commitments to reproduce another project's catalogue.

## Distribution and monetisation

- Each extension should have its own Streamer.bot import code/package.
- Shared libraries should not force unrelated extensions to be installed.
- Free extensions and optional paid extensions should be able to coexist.
- Paid features should be packaged and documented independently.
- Migration helpers may exist where useful, but migration compatibility is not a marketing feature.

## Quality bar

Every extension should have clear documentation, safe defaults, useful logging, an independent install path, and a repeatable release process.
