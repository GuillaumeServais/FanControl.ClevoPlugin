using FanControl.Plugins;

namespace FanControl.ClevoPlugin
{
    internal sealed class ClevoFanControlSensor : IPluginControlSensor
    {
        private readonly ClevoHelperClient _client;
        private readonly int _fanNumber;
        private float? _lastValue;

        public ClevoFanControlSensor(ClevoHelperClient client, int fanNumber, string name)
        {
            _client = client;
            _fanNumber = fanNumber;
            Name = name;
            Id = "Clevo_Control_" + fanNumber;
        }

        public string Id { get; }
        public string Name { get; }
        public float? Value { get; private set; }

        public void Set(float val)
        {
            _lastValue = val;
            _client.Set(_fanNumber, val);
        }

        public void Reset()
        {
            _lastValue = null;
            _client.Auto(_fanNumber);
        }

        public void Update()
        {
            Value = _lastValue;
        }
    }
}
