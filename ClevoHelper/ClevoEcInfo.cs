using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FanControl.ClevoHelper
{
    internal sealed class ClevoEcInfo : IDisposable
    {
        private IntPtr _dll;
        private SetFanDuty _setFanDuty;
        private SetFanDutyAuto _setFanDutyAuto;

        public ClevoEcInfo()
        {
            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClevoEcInfo.dll");
            _dll = NativeMethods.LoadLibrary(dllPath);

            if (_dll == IntPtr.Zero)
                throw new Exception("Failed to load ClevoEcInfo.dll from " + dllPath + ". Win32 error: " + Marshal.GetLastWin32Error());

            var initIoPtr = NativeMethods.GetProcAddress(_dll, "InitIo");
            var setPtr = NativeMethods.GetProcAddress(_dll, "SetFanDuty");
            var autoPtr = NativeMethods.GetProcAddress(_dll, "SetFanDutyAuto");

            if (initIoPtr == IntPtr.Zero || setPtr == IntPtr.Zero || autoPtr == IntPtr.Zero)
                throw new Exception("ClevoEcInfo.dll does not expose the expected functions.");

            var initIo = (InitIo)Marshal.GetDelegateForFunctionPointer(initIoPtr, typeof(InitIo));
            _setFanDuty = (SetFanDuty)Marshal.GetDelegateForFunctionPointer(setPtr, typeof(SetFanDuty));
            _setFanDutyAuto = (SetFanDutyAuto)Marshal.GetDelegateForFunctionPointer(autoPtr, typeof(SetFanDutyAuto));

            if (!initIo())
                throw new Exception("InitIo failed. Run FanControl as administrator and check the NTPort driver.");
        }

        public void SetFanSpeedPercent(int fanNumber, float percent)
        {
            var safe = Math.Max(0, Math.Min(100, percent));
            _setFanDuty(fanNumber, (int)(safe * 255 / 100));
        }

        public void SetAuto(int fanNumber)
        {
            _setFanDutyAuto(fanNumber);
        }

        public void Dispose()
        {
            if (_dll != IntPtr.Zero)
            {
                NativeMethods.FreeLibrary(_dll);
                _dll = IntPtr.Zero;
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool InitIo();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetFanDuty(int fanNumber, int duty255);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void SetFanDutyAuto(int fanNumber);
    }
}
