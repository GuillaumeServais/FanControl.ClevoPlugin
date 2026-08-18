using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FanControl.ClevoHelper
{
    internal static class Program
    {
        private const int Port = 47873;
        private static ClevoEcInfo _ec;
        private static bool _running = true;

        private static void Main()
        {
            try
            {
                _ec = new ClevoEcInfo();
                var listener = new TcpListener(IPAddress.Loopback, Port);
                listener.Start();

                while (_running)
                {
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                    {
                        writer.WriteLine(Handle(reader.ReadLine() ?? string.Empty));
                    }
                }

                listener.Stop();
            }
            catch (Exception ex)
            {
                File.AppendAllText("FanControl.ClevoHelper.log", DateTime.Now + " " + ex + Environment.NewLine);
            }
        }

        private static string Handle(string command)
        {
            try
            {
                var parts = command.Split(' ');

                if (parts[0] == "PING")
                    return "OK PONG";

                if (parts[0] == "SET")
                {
                    _ec.SetFanSpeedPercent(int.Parse(parts[1]), float.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture));
                    return "OK";
                }

                if (parts[0] == "AUTO")
                {
                    _ec.SetAuto(int.Parse(parts[1]));
                    return "OK";
                }

                if (parts[0] == "EXIT")
                {
                    try { _ec.SetAuto(1); } catch { }
                    try { _ec.SetAuto(2); } catch { }
                    _running = false;
                    return "OK";
                }

                return "ERR unknown command";
            }
            catch (Exception ex)
            {
                return "ERR " + ex.Message;
            }
        }
    }
}
