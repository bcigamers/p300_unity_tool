//using LSL;
using System;
using System.Globalization;
using System.Net;

// Sends data to JS backend
public class DataSender : Singleton<DataSender>
{
    
    public static DateTime GetNetTime()
    {
        var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");
        var response = myHttpWebRequest.GetResponse();
        string todaysDates = response.Headers["date"];
        return DateTime.ParseExact(todaysDates,
                                   "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                   CultureInfo.InvariantCulture.DateTimeFormat,
                                   DateTimeStyles.AssumeUniversal);
    }

    // This has to be formatted for the data to be sent.
    public void SendStringToJS(string my_string_to_send)
    {
        //JSplugin.SendStringToServer("{'data':" + my_string_to_send + ", " + "'TIMESTAMP': " + liblsl.local_clock().ToString() +"}");
        JSplugin.SendStringToServer("{'data':" + my_string_to_send + ", " + "'TIMESTAMP': " + DateTime.UtcNow.Millisecond.ToString() + "}");
    }

    public void SendFloatToJS(float my_float_to_send)
    {
        JSplugin.SendFloatToServer(my_float_to_send);
        //JSplugin.SendStringToServer("From Unity With Love: " + " LSL Clock Timestamp: " + liblsl.local_clock().ToString());
    }

    public void SendNumToJS(int my_num_to_send)
    {
        JSplugin.SendNumToServer(my_num_to_send);
        //JSplugin.SendStringToServer("From Unity With Love: " + " LSL Clock Timestamp: " + liblsl.local_clock().ToString());
    }
}
