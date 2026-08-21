# duhBuh architecture

## Layers

```text
Streamer.bot C# actions
        |
        +-- RtsUI (settings definitions + WPF presentation)
        |
        +-- duhBuh.Core (logging, settings conventions, HTTP/retry helpers)
        |
        +-- platform providers (Twitch / YouTube / OBS)
        |
        +-- extension logic (Lurks, Watchtime, etc.)
```

## Settings

Extensions should use stable, namespaced keys such as:

```text
duhbuh.lurks.24hFormat
duhbuh.lurks.removeUnpresentLurkers
duhbuh.lurks.chattingUnlurks
```

The UI layer should be declarative: an extension registers controls, defaults, descriptions, and categories; the framework renders and persists them using Streamer.bot-compatible storage.

## Clean-room boundary

The implementation is independent code. Existing extensions and screenshots are used as behavioral/UI references only. Do not copy proprietary source, embedded credentials, or private service implementations.

## Provider isolation

Remote services should sit behind interfaces. For example, AI features should depend on an abstraction such as `ITextGenerator`, allowing cloud or local providers without changing extension logic.
