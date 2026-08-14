# duhBuh-SB

A clean-room Streamer.bot extension ecosystem inspired by the capabilities we want from existing Twitch/YouTube automation extensions, with no dependency on Tawmae services or `TawmaeUI.dll`.

## Goals

- Reusable settings/UI framework for Streamer.bot C# actions
- Twitch-first, with YouTube support designed in from the start
- Local/self-hosted browser widgets where practical
- Provider-neutral AI integrations
- Consistent logging, configuration, retries, and error handling
- Small, composable extensions rather than one monolithic plugin

## First milestone

1. `duhBuhUI` compatibility-style settings API
2. Lurks extension
3. Streamer.bot import examples
4. Documentation and release packaging

## Planned extension families

- Community: Lurks, Watchtime, Loyalty, Inventory, Giveaways, Temporary VIP, Mod Tools
- Chat: Command Check, Hot Words, Better Shoutouts, Chat Lookup, Alerts, Vertical Chat
- Automation: Dynamic Timers, Time Trigger, Live Trigger, Event List, Random Source Position
- Widgets: Goalbar, Subathon, Social Rotator, Stream Receipt, Slot Machine, Throne Holder
- Integrations: Spotify, Giphy, Steam, Bluesky, X, Twitch, YouTube

## Design principle

The project will reproduce useful *functionality and workflows*, not proprietary implementation or credentials. Tawmae-hosted services are not required for duhBuh to operate.

## Streamer.bot compatibility

The project targets the current Streamer.bot C# action model. Streamer.bot's C# actions expose `CPHInline` and the `CPH` API to custom code actions.

See `docs/architecture.md` and `docs/roadmap.md` for the current plan.
