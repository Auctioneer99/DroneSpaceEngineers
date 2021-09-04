using System;
using System.Collections.Generic;

namespace DroneSpaceEngineers
{
    public enum EMainShipCommand : byte
    {
        CommitConnect,
        DesiredOrbitPosition,
    }

    public abstract class AMainShipCommand : ACommand<DroneController>
    {
        public abstract EMainShipCommand CommandType { get; }

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

        public abstract AMainShipCommand Clone();
        protected abstract void WriteAttributes(Packet packet);
        protected abstract void ReadAttributes(Packet packet);

        public override string ToString()
        {
            return CommandType.ToString();
        }
    }

    public class CommitConnectCommand : AMainShipCommand
    {
        public override EMainShipCommand CommandType => EMainShipCommand.CommitConnect;

        public bool Connected { get; private set; }

        public CommitConnectCommand Initialize(bool connected)
        {
            base.Initialize();

            Connected = connected;
            return this;
        }

        public override AMainShipCommand Clone()
        {
            return new CommitConnectCommand();
        }

        protected override void ApplyImplementation(DroneController controller)
        {
            if (Connected)
            {
                controller.NetworkHandler.HandleConnected(Sender);
            }
            else
            {

            }
        }

        protected override void ReadAttributes(Packet packet)
        {
            Connected = packet.ReadBool();
        }

        protected override void WriteAttributes(Packet packet)
        {
            packet.Write(Connected);
        }
    }

    public class DesiredOrbitPositionCommand : AMainShipCommand
    {
        public override EMainShipCommand CommandType => EMainShipCommand.DesiredOrbitPosition;

        public DroneOrbitPosition Position { get; private set; }

        public DesiredOrbitPositionCommand Initialize(DroneOrbitPosition position)
        {
            base.Initialize();

            Position = position;
            return this;
        }

        public override AMainShipCommand Clone()
        {
            return new DesiredOrbitPositionCommand();
        }

        protected override void ApplyImplementation(DroneController controller)
        {
            controller.MoveController.SetPosition(Position);
            controller.Logger.Log("Setting orbit position");
        }

        protected override void ReadAttributes(Packet packet)
        {
            Position = packet.ReadDronePosition();
        }

        protected override void WriteAttributes(Packet packet)
        {
            packet.Write(Position);
        }
    }
}
