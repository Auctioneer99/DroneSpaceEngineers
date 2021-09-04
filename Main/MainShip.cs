using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Ingame;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Linq;

namespace DroneSpaceEngineers.MainShipMain
{
    public sealed class Program : MyGridProgram
    {
        private MainShipController _controller;

        public bool Running { get; private set; }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            IMyTextPanel panel = GridTerminalSystem.GetBlockWithName("panel") as IMyTextPanel;
            IMyTextPanel panelDrones = GridTerminalSystem.GetBlockWithName("panelDrones") as IMyTextPanel;
            List<IMyShipController> controlList = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controlList);
            IMyShipController control = controlList.FirstOrDefault();

            ILogger logger = new Logger(panel);
            IDroneLogger droneLogger = new DroneLogger(panelDrones);
            INetwork network = new Network(IGC, logger);
            IOrientationProvider provider = new OrientationProvider(control);
            _controller = new MainShipController(network, provider, droneLogger, logger);
        }

        public void Main(string args)
        {
            if (args == "start")
            {
                Running = true;
            }

            if (args == "stop")
            {
                Running = false;
            }

            if (Running)
            {
                _controller.Update();
            }
        }

        public void Save()
        {
            
        }
    }
}
