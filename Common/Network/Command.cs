using System;
using System.Collections.Generic;
using System.Text;

namespace DroneSpaceEngineers
{
    public class CommandFactory
    {
        private Dictionary<EDroneCommand, ADroneCommand> _droneCommands;
        private Dictionary<EMainShipCommand, AMainShipCommand> _mainShipCommands;
        private ILogger _logger;

        public CommandFactory(ILogger logger)
        {
            _logger = logger;

            Initialize();
        }

        private void Initialize()
        {
            _droneCommands = new Dictionary<EDroneCommand, ADroneCommand>();
            _mainShipCommands = new Dictionary<EMainShipCommand, AMainShipCommand>();

            RegisterDroneCommands(_droneCommands);
            RegisterMainShipCommands(_mainShipCommands);
            _logger.Log("CommandFactory initialized");
        }

        private void RegisterDroneCommands(Dictionary<EDroneCommand, ADroneCommand> commands)
        {
            commands.Clear();
            commands.Add(EDroneCommand.Connect, new ConnectCommand());
            commands.Add(EDroneCommand.Disconnect, new DisconnectCommand());
            commands.Add(EDroneCommand.NotifyAlive, new NotifyAliveCommand());
            commands.Add(EDroneCommand.SetPosition, new SetPositionDroneCommand());
        }

        private void RegisterMainShipCommands(Dictionary<EMainShipCommand, AMainShipCommand> commands)
        {
            commands.Clear();
            commands.Add(EMainShipCommand.CommitConnect, new CommitConnectCommand());
            commands.Add(EMainShipCommand.DesiredOrbitPosition, new DesiredOrbitPositionCommand());
        }

        public ADroneCommand Create(EDroneCommand commandId)
        {
            ADroneCommand command = _droneCommands.GetValueOrDefault(commandId).Clone();
            return command;
        }
        public AMainShipCommand Create(EMainShipCommand commandId)
        {
            AMainShipCommand command = _mainShipCommands.GetValueOrDefault(commandId).Clone();
            return command;
        }
    }
    public abstract class ACommand<T> : ISerializable, IDeserializable
    {
        public long Sender { get; set; }

        public bool Initialized { get; private set; } = false;

        public void Initialize()
        {
            if (Initialized)
            {
                throw new Exception($"Command {this} already initialized");
            }
            Initialized = true;
        }

        public virtual void WriteToPacket(Packet packet)
        {
            packet.Write(Initialized);
        }

        public virtual void ReadFromPacket(Packet packet)
        {
            Initialized = packet.ReadBool();
        }

        public void Apply(T controller)
        {
            if (Initialized == false)
            {
                throw new Exception($"Command {this} are not initialized");
            }
            ApplyImplementation(controller);
        }

        protected abstract void ApplyImplementation(T controller);
    }
}
