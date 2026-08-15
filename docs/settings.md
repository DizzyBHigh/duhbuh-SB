# duhBuh settings conventions

## Canonical keys

New settings use dotted, namespaced keys:

```text
duhbuh.lurks.24hFormat
duhbuh.lurks.removeUnpresentLurkers
duhbuh.lurks.chattingUnlurks
duhbuh.overlay.lurks.position
```

The extension name is the first namespace. Related subsystems may add a second namespace.

## Persistence

Settings are persisted as Streamer.bot global variables. Shared code should read persisted values with explicit defaults and write values with `persisted = true`.

`DuhBuhSettings` provides the common read/write pattern for new extensions.

## Migration

Existing releases may have underscore-style keys. A migration should read the canonical key first, then optionally read the legacy key, copy the legacy value to the canonical key, and log the migration once the canonical value has been established.

Do not delete legacy values automatically. Keeping them available makes rollback and upgrades safer.

## Compatibility rule

Do not rename existing Lurks keys in place until a migration path is wired into the Lurks action. The current Lurks settings remain compatible; new code should use the canonical dotted convention.
