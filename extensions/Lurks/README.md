# duhBuh Lurks

First extension target for RTS UI.

## Reference behavior

The initial implementation is based on the supplied Lurks & SB settings/action example. It will support:

- 24-hour or AM/PM display
- optional removal of unpresent lurkers after the configured absence period
- chat-based unlurking
- configurable chat-message threshold
- chat replies
- leaderboard rank count
- reset of stored lurk counts/times
- configurable response text
- configurable singular/plural translations

## Settings namespace

Use `duhbuh.lurks.*` keys for new settings. Existing Tawmae keys should not be written by default; compatibility/migration can be added later as an explicit feature.

## Implementation notes

Lurk state should be stored using Streamer.bot user variables so it follows the viewer. Aggregate settings should use global/persistent variables. The extension should avoid destructive resets without explicit confirmation.
