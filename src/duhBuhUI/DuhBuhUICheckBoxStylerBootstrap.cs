public static class DuhBuhUICheckBoxStylerBootstrap
{
    // Kept as a compatibility shim. The UI now initializes the checkbox
    // styling explicitly when the WPF window is built, avoiding reliance on
    // module-initializer timing in Streamer.bot's .NET Framework host.
    public static void Initialize()
    {
        DuhBuhUICheckBoxStyler.Initialize();
    }
}
