# duhBuh Lurks — development installation

This is the first development build. The final release will be distributed as a normal Streamer.bot import code. For now, the source is intentionally kept visible while the import package is validated against a real Streamer.bot instance.

## Files

- `../../src/duhBuhUI/DuhBuhUI.cs` — reusable settings UI source
- `LurksSettings.cs` — settings action
- `Lurks.cs` — runtime action

## 1. Settings

Create a Streamer.bot C# action named:

`duhBuh Lurks - Settings`

Paste `src/duhBuhUI/DuhBuhUI.cs` and `LurksSettings.cs` into the same C# action. Execute it once to open the settings window.

The settings are persisted as Streamer.bot global variables using the `duhbuh_lurks_` namespace.

## 2. Runtime action

Create a C# action named:

`duhBuh Lurks - Runtime`

Paste `Lurks.cs` into it.

The action uses the argument `duhbuhLurksAction` to select a behavior:

- `start` — start a lurk
- `end` — finish a lurk and record its duration
- `check` — list active lurkers
- `stats` — show the current user's lurk statistics
- `leaderboard` — show the lurk leaderboard
- `chatUnlurk` — increment the user's chat-message threshold and end their lurk when reached

The action expects normal Streamer.bot Twitch user arguments (`userName`, `displayName`; `user` is accepted as a fallback). Streamer.bot's C# argument system recommends `CPH.TryGetArg()` for this purpose.

## 3. Suggested commands

Create commands that execute the runtime action and set `duhbuhLurksAction`:

| Command | Argument | Purpose |
|---|---|---|
| `!lurk` | `start` | Start a lurk |
| `!unlurk` | `end` | End a lurk |
| `!lurkers` | `check` | List current lurkers |
| `!lurkstats` | `stats` | Show personal stats |
| `!lurkleaderboard` | `leaderboard` | Show leaderboard |

## 4. Chat unlurk

Create a Twitch Chat Message trigger that runs the runtime action with `duhbuhLurksAction=chatUnlurk`. This should be a separate action from the `!lurk` command so the command itself does not count as a normal chat message unless you want it to.

## 5. Present Viewers

Create a Twitch `Present Viewers` trigger that runs `RemoveUnpresentLurkers()` from the runtime code. Enable Streamer.bot's Twitch Present Viewers **Live Update** mode if you want the viewer list to be refreshed directly from Twitch. Streamer.bot documents this trigger as a periodic list of users present in chat.

## Notes

The current build deliberately uses Twitch-specific user variables (`GetTwitchUserVar`, `SetTwitchUserVar`, `GetTwitchUsersVar`) rather than deprecated generic user-variable methods. This keeps the extension aligned with the current Streamer.bot API.
