using System;

public class CPHInline
{
    public bool Execute()
    {
        string json = "{\"timeStamp\":\"" + DateTime.UtcNow.ToString("o") + "\",\"event\":{\"source\":\"Custom\",\"type\":\"Event\"},\"data\":{\"eventName\":\"duhbuh.overlay\",\"useArgs\":true,\"args\":{\"title\":\"duhBuh\",\"message\":\"WebSocket connection works!\",\"meta\":\"Overlay test\",\"duration\":5000}}}";
        CPH.WebsocketBroadcastJson(json);
        CPH.LogInfo("[duhBuh Overlay] Test notification broadcast to WebSocket clients.");
        return true;
    }
}
