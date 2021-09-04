using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DroneSpaceEngineers
{
    public class Packet
    {
        private List<byte> buffer;
        private byte[] readableBuffer;
        private int readPos;

        public Packet()
        {
            buffer = new List<byte>();
            readPos = 0;
        }

        public Packet(int _id)
        {
            buffer = new List<byte>();
            readPos = 0;

            Write(_id);
        }

        public Packet(byte[] _data)
        {
            buffer = new List<byte>();
            readPos = 0;

            SetBytes(_data);
        }

        #region Functions
        public void SetBytes(byte[] _data)
        {
            Write(_data);
            readableBuffer = buffer.ToArray();
        }

        public void WriteLength()
        {
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        }

        public void InsertInt(int _value)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public byte[] ToArray()
        {
            readableBuffer = buffer.ToArray();
            return readableBuffer;
        }

        public int Length()
        {
            return buffer.Count;
        }

        public int UnreadLength()
        {
            return Length() - readPos;
        }

        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                buffer.Clear();
                readableBuffer = null;
                readPos = 0;
            }
            else
            {
                readPos -= 4;
            }
        }
        #endregion

        #region Write Data
        public Packet Write(byte _value)
        {
            buffer.Add(_value);
            return this;
        }

        public Packet Write(byte[] _value)
        {
            buffer.AddRange(_value);
            return this;
        }

        public Packet Write(ushort _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(short _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(int _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(long _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(float _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(double _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(bool _value)
        {
            buffer.AddRange(BitConverter.GetBytes(_value));
            return this;
        }

        public Packet Write(string _value)
        {
            Write(_value.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(_value));
            return this;
        }
        #endregion

        #region Read Data

        public byte ReadByte(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte _value = readableBuffer[readPos];
                if (_moveReadPos)
                {
                    readPos += 1;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                byte[] _value = buffer.GetRange(readPos, _length).ToArray();
                if (_moveReadPos)
                {
                    readPos += _length;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        public ushort ReadUShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                ushort _value = BitConverter.ToUInt16(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 2;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public short ReadShort(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                short _value = BitConverter.ToInt16(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 2;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        public int ReadInt(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                int _value = BitConverter.ToInt32(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 4;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        public long ReadLong(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                long _value = BitConverter.ToInt64(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 8;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        public float ReadFloat(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                float _value = BitConverter.ToSingle(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 4;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        public double ReadDouble(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                double _value = BitConverter.ToDouble(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 8;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'double'!");
            }
        }

        public bool ReadBool(bool _moveReadPos = true)
        {
            if (buffer.Count > readPos)
            {
                bool _value = BitConverter.ToBoolean(readableBuffer, readPos);
                if (_moveReadPos)
                {
                    readPos += 1;
                }
                return _value;
            }
            else
            {
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        public string ReadString(bool _moveReadPos = true)
        {
            try
            {
                int _length = ReadInt();
                string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length);
                if (_moveReadPos && _value.Length > 0)
                {
                    readPos += _length;
                }
                return _value;
            }
            catch
            {
                throw new Exception("Could not read value of type 'string'!");
            }
        }
        #endregion

        #region My
        public Packet Write(EDrone type)
        {
            Write((byte)type);
            return this;
        }

        public EDrone ReadEDrone()
        {
            return (EDrone)ReadByte();
        }

        public Packet Write(EMainShipCommand command)
        {
            Write((byte)command);
            return this;
        }

        public EMainShipCommand ReadEMainShipCommand()
        {
            return (EMainShipCommand)ReadByte();
        }

        public AMainShipCommand ReadMainShipCommand(CommandFactory factory)
        {
            EMainShipCommand commandId = ReadEMainShipCommand();
            AMainShipCommand command = factory.Create(commandId);
            command.ReadFromPacket(this);
            return command;
        }

        public Packet Write(EDroneCommand command)
        {
            Write((byte)command);
            return this;
        }

        public EDroneCommand ReadEDroneCommand()
        {
            return (EDroneCommand)ReadByte();
        }

        public ADroneCommand ReadDroneCommand(CommandFactory factory)
        {
            EDroneCommand commandId = ReadEDroneCommand();
            ADroneCommand command = factory.Create(commandId);
            command.ReadFromPacket(this);
            return command;
        }

        public Packet Write(Vector3D vector)
        {
            Write(vector.X)
                .Write(vector.Y)
                .Write(vector.Z);
            return this;
        }

        public Vector3D ReadVector3D()
        {
            double x, y, z;
            x = ReadDouble();
            y = ReadDouble();
            z = ReadDouble();

            return new Vector3D(x, y, z);
        }

        public Packet Write(DroneOrbitPosition position)
        {
            Write(position.Origin)
                .Write(position.Direction)
                .Write(position.Radius);
            return this;
        }

        public DroneOrbitPosition ReadDronePosition()
        {
            Vector3D origin, direction;
            int radius;
            origin = ReadVector3D();
            direction = ReadVector3D();
            radius = ReadInt();

            return new DroneOrbitPosition() { Direction = direction, Origin = origin, Radius = radius };
        }

        public Packet Write(EDroneMove moveType)
        {
            return Write((byte)moveType);
        }

        public EDroneMove ReadEDronePosition()
        {
            return (EDroneMove)ReadByte();
        }
        #endregion
    }
}
