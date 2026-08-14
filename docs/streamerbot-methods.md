# Streamer.bot C# method subactions

Streamer.bot's **Execute C# Method** subaction only exposes methods that are named/public, return `bool`, and take no parameters.

For duhBuh extensions, runtime behavior should therefore expose small public wrappers such as:

```csharp
public bool StartLurk()
{
    return StartLurkInternal();
}

public bool EndLurk()
{
    return EndLurkInternal();
}
```

The main `Execute()` dispatcher can remain for compatibility with argument-driven execution, but named wrappers are preferable for Streamer.bot action graphs because they appear directly in the **Methods** dropdown.

This lets an extension use a single compiled C# action with multiple Streamer.bot subactions instead of requiring separate C# actions for each operation.
