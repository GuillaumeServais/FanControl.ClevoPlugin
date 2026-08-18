using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FanControl.ClevoPlugin
{
    internal sealed class ClevoHelperClient : IDisposable
    {
        private const int Port = 47873;
        private const string AssetsFolderName = "FanControl.ClevoPlugin";
        private Process _helperProcess;
        private readonly object _lock = new object();

        public void Start()
        {
            var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var assetsDirectory = Path.Combine(pluginDirectory, AssetsFolderName);
            var helperPath = Path.Combine(assetsDirectory, "FanControl.ClevoHelper.exe");
            var ecDllPath = Path.Combine(assetsDirectory, "ClevoEcInfo.dll");

            if (!File.Exists(helperPath))
                throw new Exception("FanControl.ClevoHelper.exe was not found in the FanControl.ClevoPlugin asset folder.");

            if (!File.Exists(ecDllPath))
                throw new Exception("ClevoEcInfo.dll was not found in the FanControl.ClevoPlugin asset folder.");

            if (!Ping())
            {
                _helperProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = helperPath,
                    WorkingDirectory = assetsDirectory,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }

            var deadline = DateTime.Now.AddSeconds(8);
            while (DateTime.Now < deadline)
            {
                if (Ping()) return;
                Thread.Sleep(250);
            }

            throw new Exception("The 32-bit Clevo helper did not respond. Run FanControl as administrator and check the NTPort driver.");
        }

        public void Set(int fanNumber, float percent)
        {
            var safe = Math.Max(0, Math.Min(100, percent));
            var response = Send("SET " + fanNumber + " " + safe.ToString(System.Globalization.CultureInfo.InvariantCulture));
            if (!response.StartsWith("OK"))
                throw new Exception(response);
        }

        public void Auto(int fanNumber)
        {
            var response = Send("AUTO " + fanNumber);
            if (!response.StartsWith("OK"))
                throw new Exception(response);
        }

        private bool Ping()
        {
            try { return Send("PING") == "OK PONG"; }
            catch { return false; }
        }

        private string Send(string command)
        {
            lock (_lock)
            {
                using (var client = new TcpClient("127.0.0.1", Port))
                {
                    client.ReceiveTimeout = 2000;
                    client.SendTimeout = 2000;

                    using (var stream = client.GetStream())
                    {
                        var bytes = Encoding.UTF8.GetBytes(command + "\n");
                        stream.Write(bytes, 0, bytes.Length);

                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                            return reader.ReadLine() ?? string.Empty;
                    }
                }
            }
        }

        public void Dispose()
        {
            try { Auto(1); } catch { }
            try { Auto(2); } catch { }
            try { Send("EXIT"); } catch { }
            try
            {
                if (_helperProcess != null && !_helperProcess.HasExited)
                    _helperProcess.Kill();
            }
            catch { }
        }
    }
}
