using System;
using System.Collections.Generic;

namespace DroneSpaceEngineers
{
    public enum EDroneCommand : byte
    {
        Connect,
        Disconnect,
        NotifyAlive,
        SetPosition
    }

    public abstract class ADroneCommand : ACommand<MainShipController>
    {
        public abstract EDroneCommand CommandType { get; }

        public override void WriteToPacket(Packet packet)
        {
            packet.Write(CommandType);
            WriteAttributes(packet);
            base.WriteToPacket(packet);
        }

        public override void ReadFromPacket(Packet packet)
        {
            ReadAttributes(packet);
            base.ReadFromPacket(packet);
        }

        public abstract ADroneCommand Clone();
        protected abstract void WriteAttributes(Packet packet);
        protected abstract void ReadAttributes(Packet packet);

        public override string ToString()
        {
            return CommandType.ToString();
        }
    }

    public class ConnectCommand : ADroneCommand
    {
        public override EDroneCommand CommandType => EDroneCommand.Connect;

        public long ID { get; private set; }
        public EDrone DroneType { get; private set; }

        public override ADroneCommand Clone()
        {
            return new ConnectCommand();
        }

        public ConnectCommand Initialize(long myID, EDrone type)
        {
            base.Initialize();

            ID = myID;
            DroneType = type;
            return this;
        }

        protected override void ApplyImplementation(MainShipController controller)
        {
            if (Sender == ID)
            {
                Drone drone = new Drone(Sender, DroneType);
                if (controller.DronePool.TryConnectDrone(drone))
                {
                    controller.Logger.Log($"Ship with ID = {ID} successfully connected");
                    var command = (controller.NetworkHandler.CommandFactory.Create(EMainShipCommand.CommitConnect) as CommitConnectCommand).Initialize(true);
                    controller.NetworkHandler.SendCommandToTarget(command, ID);
                }
                else
                {
                    controller.Logger.Log($"Can't connect ship with ID = {ID}");
                }
            }
        }

        protected override void ReadAttributes(Packet packet)
        {
            ID = packet.ReadLong();
            DroneType = packet.ReadEDrone();
        }

        protected override void WriteAttributes(Packet packet)
        {
            packet.Write(ID)
                .Write(DroneType);
        }
    }

    public class DisconnectCommand : ADroneCommand
    {
        public override EDroneCommand CommandType => EDroneCommand.Disconnect;

        public override ADroneCommand Clone()
        {
            return new DisconnectCommand();
        }

        public new DisconnectCommand Initialize()
        {
            base.Initialize();
            return this;
        }

        protected override void ApplyImplementation(MainShipController controller)
        {
            controller.DronePool.DisconnectDrone(Sender);
        }

        protected override void ReadAttributes(Packet packet)
        {

        }

        protected override void WriteAttributes(Packet packet)
        {

        }
    }

    public class NotifyAliveCommand : ADroneCommand
    {
        public override EDroneCommand CommandType => EDroneCommand.NotifyAlive;

        public override ADroneCommand Clone()
        {
            return new NotifyAliveCommand();
        }

        public new NotifyAliveCommand Initialize()
        {
            base.Initialize();
            return this;
        }

        protected override void ApplyImplementation(MainShipController controller)
        {
            controller.DronePool.HealthCheckHandle(Sender);
        }

        protected override void ReadAttributes(Packet packet)
        {
        }

        protected override void WriteAttributes(Packet packet)
        {
        }
    }

    public class SetPositionDroneCommand : ADroneCommand
    {
        public override EDroneCommand CommandType => EDroneCommand.SetPosition;

        public EDroneMove MoveType { get; private set; }

        public SetPositionDroneCommand Initialize(EDroneMove position)
        {
            base.Initialize();
            MoveType = position;
            return this;
        }

        public override ADroneCommand Clone()
        {
            return new SetPositionDroneCommand();
        }

        protected override void ApplyImplementation(MainShipController controller)
        {
            controller.DronePool.UpdateDronePosition(Sender, MoveType);
        }

        protected override void ReadAttributes(Packet packet)
        {
            MoveType = packet.ReadEDronePosition();
        }

        protected override void WriteAttributes(Packet packet)
        {
            packet.Write(MoveType);
        }
    }
}
