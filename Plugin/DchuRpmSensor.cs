using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using FanControl.Plugins;

namespace FanControl.ClevoPlugin
{
    internal sealed class DchuRpmSensor : IPluginSensor, IDisposable
    {
        private const int DchuDataId = 0x0C;
        private const float CorrectionFactor = 1.096f;
        private const string AssetsFolderName = "FanControl.ClevoPlugin";
        private IntPtr _dll;
        private GetDchuDataBuffer _getBuffer;

        public string Id => "Clevo_DCHU_Fan_RPM";
        public string Name => "Clevo CPU Fan RPM";
        public float? Value { get; private set; }

        public DchuRpmSensor()
        {
            Init();
        }

        private void Init()
        {
            var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var dllPath = Path.Combine(pluginDirectory, AssetsFolderName, "InsydeDCHU.dll");

            if (!File.Exists(dllPath))
                throw new Exception("InsydeDCHU.dll was not found in the FanControl.ClevoPlugin asset folder.");

            _dll = NativeMethods.LoadLibrary(dllPath);
            if (_dll == IntPtr.Zero)
                throw new Exception("Failed to load InsydeDCHU.dll. Win32 error: " + Marshal.GetLastWin32Error());

            var proc = NativeMethods.GetProcAddress(_dll, "GetDCHU_Data_Buffer");
            if (proc == IntPtr.Zero)
                throw new Exception("GetDCHU_Data_Buffer was not found in InsydeDCHU.dll.");

            _getBuffer = (GetDchuDataBuffer)Marshal.GetDelegateForFunctionPointer(proc, typeof(GetDchuDataBuffer));
        }

        public void Update()
        {
            if (_getBuffer == null)
            {
                Value = null;
                return;
            }

            IntPtr buffer = Marshal.AllocHGlobal(512);
            try
            {
                for (int i = 0; i < 512; i++)
                    Marshal.WriteByte(buffer, i, 0);

                _getBuffer(DchuDataId, buffer);

                byte high = Marshal.ReadByte(buffer, 0x02);
                byte low = Marshal.ReadByte(buffer, 0x03);
                int n = (high << 8) | low;

                if (n <= 0)
                {
                    Value = 0;
                    return;
                }

                float rpm = (32768f * 60f / n) * CorrectionFactor;
                Value = rpm >= 100 && rpm <= 10000 ? rpm : (float?)null;
            }
            catch
            {
                Value = null;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public void Dispose()
        {
            if (_dll != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(_dll);
                _dll = IntPtr.Zero;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GetDchuDataBuffer(int id, IntPtr buffer);
    }
}
