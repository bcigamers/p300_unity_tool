using System.Runtime.InteropServices;
/// <summary>
/// Class with a JS Plugin functions for WebGL.
/// </summary>
public static class JSplugin
{
    [DllImport("__Internal")]
    public static extern void SendToServer();

    [DllImport("__Internal")]
    public static extern void SendStringToServer(string text);

    [DllImport("__Internal")]
    public static extern void SendFloatToServer(float num);

    [DllImport("__Internal")]
    public static extern void SendNumToServer(int num);
}