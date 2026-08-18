using System;
using FanControl.Plugins;

namespace FanControl.ClevoPlugin
{
    public sealed class ClevoPlugin : IPlugin, IDisposable
    {
        private ClevoHelperClient _client;
        private DchuRpmSensor _rpmSensor;

        public string Name => "Clevo Fan Control";

        public void Initialize()
        {
            _client = new ClevoHelperClient();
            _client.Start();

            _rpmSensor = new DchuRpmSensor();
        }

        public void Load(IPluginSensorsContainer container)
        {
            container.ControlSensors.Add(new ClevoFanControlSensor(_client, 1, "Clevo CPU Fan Control"));
            container.ControlSensors.Add(new ClevoFanControlSensor(_client, 2, "Clevo GPU Fan Control"));
            container.FanSensors.Add(_rpmSensor);
        }

        public void Close()
        {
            try
            {
                _client?.Dispose();
            }
            finally
            {
                _client = null;
            }

            try
            {
                _rpmSensor?.Dispose();
            }
            finally
            {
                _rpmSensor = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
