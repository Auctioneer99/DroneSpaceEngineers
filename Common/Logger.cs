using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DroneSpaceEngineers
{
    public interface ILogger
    {
        void Log(string message);
        void LogException(string message);
    }

    public class Logger : ILogger
    {
        private const int MAX_LINES = 10;

        private IMyTextPanel _panel;

        private List<string> _messages;

        public Logger(IMyTextPanel panel)
        {
            _panel = panel;
            Initialize();
        }

        private void Initialize()
        {
            _panel.WriteText("");
            _messages = new List<string>();
        }

        public void Log(string message)
        {
            _messages.Insert(0, $"[{DateTime.Now.ToString("hh:mm:ss")}] {message}\n");
            if (MAX_LINES < _messages.Count)
            {
                _messages.RemoveAt(MAX_LINES);
            }
            _panel.FontColor = Color.White;
            _panel.WriteText(string.Join("", _messages));
        }

        public void LogException(string message)
        {
            _panel.FontColor = Color.Red;
            _panel.WriteText($"[{DateTime.Now.ToString("hh:mm:ss")}] {message}\n", true);
        }
    }

    public interface IDroneLogger
    {
        void SetDrones(Drone[] drones);

        void Update();
    }

    public class DroneLogger : IDroneLogger
    {
        private Drone[] _drones;
        private IMyTextPanel _panel;

        public DroneLogger(IMyTextPanel panel)
        {
            _panel = panel;
        }

        public void SetDrones(Drone[] drones)
        {
            _drones = drones;
        }

        public void Update()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{DateTime.Now.ToString("hh: mm:ss")} Drones:");
            int length = _drones.Length;
            for(int i = 0; i < length; i++)
            {
                Drone d = _drones[i];
                if (d != null)
                {
                    sb.AppendLine($"\t[ ID: {d.ID}, Move: {d.MoveType}]");
                }
                else
                {
                    sb.AppendLine($"\t[ NULL ]");
                }
            }
            _panel.WriteText(sb.ToString());
        }
    }
}
