
namespace DroneSpaceEngineers
{
    public class DroneNetworkHandler
    {
        public CommandFactory CommandFactory { get; private set; }

        public bool Connected { get; private set; }
        private long _connectionId;

        private DroneController _controller;
        private INetwork Network => _controller.Network;
        private ILogger Logger => _controller.Logger;
        private long _trustedHost;

        public DroneNetworkHandler(DroneController controller, long trustedHost)
        {
            _controller = controller;
            _trustedHost = trustedHost;

            Initialize();
        }

        private void Initialize()
        {
            CommandFactory = new CommandFactory(Logger);
            Logger.Log("DroneNetworkHandler initialized");
        }

        public void Update()
        {
            while (Network.HasPendingMessage)
            {
                //Logger.Log("There is a message to be parsed");
                Message message = new Message();
                if (Network.TryAcceptMessage(ref message) && CanHandle(message))
                {
                    EMainShipCommand commandType = message.Packet.ReadEMainShipCommand();
                    AMainShipCommand command = CommandFactory.Create(commandType);
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
            bool can = message.Sender == _trustedHost;
            if (can == false)
            {
                Logger.LogException($"Cant handle {message.Packet.ToString()} from {message.Sender}");
            }
            return can;
        }

        public void SendCommand(ADroneCommand command)
        {
            Packet packet = new Packet();
            command.WriteToPacket(packet);
            Network.SendPacket(packet);
        }

        public void SendCommandToTarget(ADroneCommand command, long target)
        {
            Packet packet = new Packet();
            command.WriteToPacket(packet);
            Network.SendPacketToTarget(target, packet);
        }

        public void SendCommandToHost(ADroneCommand command)
        {
            SendCommandToTarget(command, _connectionId);
        }

        public void TryConnect(long host, EDrone type)
        {
            Logger.Log("Trying to connect to host, MyID = " + Network.ID);
            var command = CommandFactory.Create(EDroneCommand.Connect) as ConnectCommand;
            command.Initialize(Network.ID, type);
            SendCommandToTarget(command, host);
        }

        public void Disconnect()
        {
            if (Connected)
            {
                DisconnectCommand command = (CommandFactory.Create(EDroneCommand.Disconnect) as DisconnectCommand).Initialize();
                SendCommandToTarget(command, _connectionId);
            }
        }

        public void HandleConnected(long connection)
        {
            _connectionId = connection;
            Connected = true;
            Logger.Log("Successfully connected to host");
        }
    }
}
