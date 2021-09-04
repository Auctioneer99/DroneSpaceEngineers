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

namespace DroneSpaceEngineers.DroneMain
{
    public sealed class Program : MyGridProgram
    {
        private const long MAINSHIP_ID = 142581789343697931;

        public bool Running { get; private set; }

        private DroneController _controller;

        public Program()
        {
            IMyTextPanel _panel = GridTerminalSystem.GetBlockWithName("panel") as IMyTextPanel;
            List<IMyCockpit> remoteControlList = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(remoteControlList);
            IMyCockpit remoteControl = remoteControlList.FirstOrDefault();
            List<IMyThrust> thrustList = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrustList);

            List<IMyGyro> gyroList = new List<IMyGyro>();
            GridTerminalSystem.GetBlocksOfType(gyroList);

            ILogger logger = new Logger(_panel);
            INetwork network = new Network(IGC, logger);
            IOrientationProvider provider = new OrientationProvider(remoteControl);
            IEngine engine = new Engine(thrustList.ToArray(), gyroList.ToArray(), provider);
            _controller = new DroneController(network, provider, engine, logger, MAINSHIP_ID);
            logger.Log(gyroList.Count.ToString());
        }

        public void Main(string args)
        {
            if (args == "start")
            {
                Running = true;
                _controller.NetworkHandler.TryConnect(MAINSHIP_ID, EDrone.Defense);
                Runtime.UpdateFrequency = UpdateFrequency.Update10;
            }

            if (args == "stop")
            {
                Running = false;
                _controller.NetworkHandler.Disconnect();
                Runtime.UpdateFrequency = UpdateFrequency.None;
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