using System;

namespace DroneSpaceEngineers
{
    public interface ISerializable
    {
        void WriteToPacket(Packet packet);
    }

    public interface IDeserializable
    {
        void ReadFromPacket(Packet packet);
    }
}
