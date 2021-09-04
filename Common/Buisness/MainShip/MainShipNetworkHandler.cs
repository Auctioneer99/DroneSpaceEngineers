using System;
using System.Collections.Generic;
using System.Text;

namespace DroneSpaceEngineers
{
    public class MainShipNetworkHandler
    {
        public CommandFactory CommandFactory { get; private set; }

        private MainShipController _controller;
        private INetwork Network => _controller.Network;
        private ILogger Logger => _controller.Logger;

        public MainShipNetworkHandler(MainShipController controller)
        {
            _controller = controller;

            Initialize();
        }

        private void Initialize()
        {
            CommandFactory = new CommandFactory(Logger);
            Logger.Log("MainShipNetworkHandler initialized");
        }

        public void Update()
        {
            while (Network.HasPendingMessage)
            {
                //Logger.Log("There is a message to be parsed");
                Message message = new Message();
                if (Network.TryAcceptMessage(ref message) && CanHandle(message))
                {
                    EDroneCommand commandType = message.Packet.ReadEDroneCommand();
                    ADroneCommand command = CommandFactory.Create(commandType);
                    try
                    {
                        command.ReadFromPacket(message.Packet);
                    }
                    catch
                    {
                        Logger.LogException($"Parsing Error in {message.Packet.ToString()} from {message.Sender}");
                        return;
                    }
                    command.Sender = message.Sender;
                    command.Apply(_controller);
                }
            }
        }

        private bool CanHandle(Message message)
        {
            bool can = true;// message.Sender == _trustedHost;
            if (can == false)
            {
                Logger.LogException($"Cant handle {message.Packet.ToString()} from {message.Sender}");
            }
            return can;
        }

        public void SendCommand(AMainShipCommand command)
        {
            Packet packet = new Packet();
            command.WriteToPacket(packet);
            Network.SendPacket(packet);
        }

        public void SendCommandToTarget(AMainShipCommand command, long target)
        {
            Packet packet = new Packet();
            command.WriteToPacket(packet);
            Network.SendPacketToTarget(target, packet);
        }
    }
}
