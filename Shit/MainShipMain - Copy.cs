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

namespace asdasd
{
    public sealed class Program : MyGridProgram
    {
        #region Main
        private IMyTextPanel _panel;
        private MainShipController _controller;

        public bool Running { get; private set; }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _panel = GridTerminalSystem.GetBlockWithName("panel") as IMyTextPanel;
            List<IMyCockpit> controlList = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(controlList);
            IMyCockpit control = controlList.FirstOrDefault();

            ILogger logger = new Logger(_panel);
            INetwork network = new Network(IGC, logger);
            IOrientationProvider provider = new OrientationProvider(control);
            _controller = new MainShipController(network, provider, logger);
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
        #endregion
        #region OrientationProvider
        public interface IOrientationProvider
        {
            double Mass { get; }
            Vector3D Position { get; }
            Matrix WorldMatrix { get; }
            Matrix OrientationMatrix { get; }
            MyShipVelocities Velocity { get; }
        }

        public class OrientationProvider : IOrientationProvider
        {
            public double Mass => _entity.CalculateShipMass().PhysicalMass;

            public MyShipVelocities Velocity => _entity.GetShipVelocities();

            public Vector3D Position => _entity.GetPosition();

            public Matrix WorldMatrix => _entity.WorldMatrix;

            public Matrix OrientationMatrix
            {
                get
                {
                    Matrix matrix = new Matrix();
                    _entity.Orientation.GetMatrix(out matrix);
                    return matrix;
                }
            }


            private IMyShipController _entity;

            public OrientationProvider(IMyShipController entity)
            {
                _entity = entity;
            }
        }
        #endregion
        #region Logger
        public interface ILogger
        {
            void Log(string message);
            void LogException(string message);
        }

        public class Logger : ILogger
        {
            private const int MAX_LINES = 10;

            private IMyTextPanel _panel;

            private int _counter = 0;

            public Logger(IMyTextPanel panel)
            {
                _panel = panel;
                Initialize();
            }

            private void Initialize()
            {
                _panel.WriteText("");
            }

            public void Log(string message)
            {
                if (_counter >= MAX_LINES)
                {
                    _panel.WriteText("");
                    _counter = 0;
                }

                _counter++;
                _panel.FontColor = Color.White;
                _panel.WriteText($"[{DateTime.Now.ToString("hh:mm:ss")}] {message}\n", true);
            }

            public void LogException(string message)
            {
                _panel.FontColor = Color.Red;
                _panel.WriteText($"[{DateTime.Now.ToString("hh:mm:ss")}] {message}\n", true);
            }
        }
        #endregion
        #region DroneOrbitPosition
        public struct DroneOrbitPosition
        {
            public Vector3D Origin { get; set; }
            public Vector3D Direction { get; set; }
            public int Radius { get; set; }

            public Vector3D ToVector3D()
            {
                return ToVector3DWithCustomDirection(Direction);
            }

            public Vector3D ToVector3DWithCustomDirection(Vector3 dir)
            {
                Vector3D position = Origin + dir * Radius;
                return position;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"Origin: {Origin}, ");
                sb.Append($"Direction: {Direction}, ");
                sb.Append($"Radius: {Radius}, ");
                return sb.ToString();
            }
        }
        public interface ISerializable
        {
            void WriteToPacket(Packet packet);
        }

        public interface IDeserializable
        {
            void ReadFromPacket(Packet packet);
        }
        #endregion
        #region Packet
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
            #endregion
        }
        #endregion
        #region DroneCommand
        public enum EDroneCommand : byte
        {
            Connect,
            Disconnect
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
        #endregion
        #region Command
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
        #endregion
        #region PositionDistributor
        public class PositionDistributor
        {
            public const int DEFENSE_DRONES_RADIUS = 100;

            public CommandFactory CommandFactory => _network.CommandFactory;

            private Vector3D[] _desiredDefensePositions;
            private MainShipNetworkHandler _network;
            private IDroneProvider _dronesProvider;
            private IOrientationProvider _orientationProvider;
            private ILogger _logger;

            public PositionDistributor(IDroneProvider dronesProvider, MainShipNetworkHandler network, IOrientationProvider orientationProvider, ILogger logger)
            {
                _dronesProvider = dronesProvider;
                _network = network;
                _orientationProvider = orientationProvider;
                _logger = logger;
                Initialize();
            }

            private void Initialize()
            {
                _desiredDefensePositions = CalculateDefensePositionDistribution(_dronesProvider.DefenseDronesCount);
                _logger.Log("PositionDistributor initialized");
            }

            public void Update()
            {
                UpdateDefenseDronesPosition();
            }

            private void UpdateDefenseDronesPosition()
            {
                Drone[] drones = _dronesProvider.GetDronesOfType(EDrone.Defense);
                int length = drones.Length;
                for (int i = 0; i < length; i++)
                {
                    Drone drone = drones[i];
                    if (drone != null && drone.IsAlive)
                    {
                        DroneOrbitPosition position = new DroneOrbitPosition();
                        position.Radius = DEFENSE_DRONES_RADIUS;
                        position.Origin = _orientationProvider.Position;
                        position.Direction = _desiredDefensePositions[i];

                        _logger.Log($"Setting position for drone: {position.ToString()}");
                        DesiredOrbitPositionCommand command = CommandFactory.Create(EMainShipCommand.DesiredOrbitPosition) as DesiredOrbitPositionCommand;
                        command.Initialize(position);
                        _network.SendCommandToTarget(command, drone.ID);
                    }
                }
            }

            private Vector3D[] CalculateDefensePositionDistribution(int numDrones)
            {
                Vector3D[] vectors = new Vector3D[numDrones];

                double phi = Math.PI * (3d - Math.Sqrt(5d));

                for (int i = 0; i < numDrones; i++)
                {
                    double y = 1 - (i / (numDrones - 1d)) * 2;
                    double radius = Math.Sqrt(1 - y * y);
                    double theta = phi * i;
                    double x = Math.Cos(theta) * radius;
                    double z = Math.Sin(theta) * radius;

                    Vector3 direction = new Vector3D(x, y, z);
                    direction.Normalize();

                    vectors[0] = direction;
                }
                return vectors;
            }
        }
        #endregion
        #region MainShipNetworkHandler
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
        #endregion
        #region MainShipController
        public class MainShipController
        {
            public const int
                DRONE_POOL_TICKRATE = 5,
                NETWORK_TICKRATE = 1,
                POSITION_DISTRIBUTOR_TICKRATE = 1,
                SYSTEM_UP_TICKRATE = 15;

            public MainShipNetworkHandler NetworkHandler { get; private set; }
            public DronePool DronePool { get; private set; }
            public PositionDistributor PositionDistributor { get; private set; }
            public INetwork Network { get; private set; }
            public ILogger Logger { get; private set; }

            private long _tick = 0;

            public MainShipController(INetwork network, IOrientationProvider provider, ILogger logger)
            {
                Network = network;
                Logger = logger;

                Initialize(provider);
            }

            private void Initialize(IOrientationProvider provider)
            {
                NetworkHandler = new MainShipNetworkHandler(this);
                DronePool = new DronePool(null, Logger);
                PositionDistributor = new PositionDistributor(DronePool, NetworkHandler, provider, Logger);
                Logger.Log("MainShipController initialized");
            }

            public void Update()
            {
                _tick++;

                if (_tick % NETWORK_TICKRATE == 0)
                {
                    NetworkHandler.Update();
                }
                if (_tick % DRONE_POOL_TICKRATE == 0)
                {
                    DronePool.Update();
                }
                if (_tick % POSITION_DISTRIBUTOR_TICKRATE == 0)
                {
                    PositionDistributor.Update();
                }

                if (_tick % SYSTEM_UP_TICKRATE == 0)
                {
                    Logger.Log("System is up");
                }
            }
        }
        #endregion
        #region DronePool
        public enum EDrone
        {
            Defense
        }

        public class Drone
        {
            public long ID { get; private set; }
            public EDrone DroneType { get; private set; }
            public bool IsAlive { get; private set; }
            public long LastCheck { get; private set; }

            public Drone(long id, EDrone type)
            {
                ID = id;
                DroneType = type;
                IsAlive = true;
                LastCheck = DateTime.Now.Millisecond;
            }
        }

        public interface IDroneProvider
        {
            int DefenseDronesCount { get; }

            Drone[] GetDronesOfType(EDrone type);
        }

        public class DronePool : IDroneProvider
        {
            public const int MAX_DEFENSE_DRONE = 7;

            public bool ShouldCreateNewDrones { get; set; } = false;
            public int DefenseDronesCount => MAX_DEFENSE_DRONE;

            private Drone[] _defenseDrones;
            private IDroneFactory _factory;
            private ILogger _logger;

            public DronePool(IDroneFactory factory, ILogger logger)
            {
                _factory = factory;
                _logger = logger;
                Initialize();
            }

            private void Initialize()
            {
                _defenseDrones = new Drone[MAX_DEFENSE_DRONE];
                _logger.Log("DronePool Initialized");
            }

            public void Update()
            {
                RemoveDeadDrones();

                if (ShouldCreateNewDrones)
                {

                }
            }

            private void RemoveDeadDrones()
            {
                int length = _defenseDrones.Length;
                for (int i = 0; i < length; i++)
                {
                    Drone drone = _defenseDrones[i];
                    if (drone != null && drone.IsAlive == false)
                    {
                        _defenseDrones[i] = null;
                    }
                }
            }

            public bool TryConnectDrone(Drone drone)
            {
                Drone[] pool = GetDronesOfType(drone.DroneType);
                int length = pool.Length;
                for (int i = 0; i < length; i++)
                {
                    if (pool[i] == null)
                    {
                        pool[i] = drone;
                        return true;
                    }
                }
                return false;
            }

            public void DisconnectDrone(long id)
            {
                for (int i = 0; i < MAX_DEFENSE_DRONE; i++)
                {
                    Drone drone = _defenseDrones[i];
                    if (drone != null && drone.ID == id)
                    {
                        _defenseDrones[i] = null;
                    }
                }
            }

            public Drone[] GetDronesOfType(EDrone type)
            {
                switch (type)
                {
                    case EDrone.Defense:
                        return _defenseDrones;
                    default:
                        return null;
                }
            }
        }
        #endregion
        #region Engine
        public interface IDroneFactory
        {

        }

        public class DroneFactory : IDroneFactory
        {

        }
        public interface IEngine
        {
            void SetVelocity(Vector3D targetVelocity);

            void AddVelocity(Vector3D velocity);

            void Rotate(Vector3D direction);
        }

        public class Engine : IEngine
        {
            private IOrientationProvider _provider;
            private IMyThrust[] _thrusters;

            public Engine(IMyThrust[] thrusters, IOrientationProvider provider)
            {
                _thrusters = thrusters;
                _provider = provider;
            }

            public void SetVelocity(Vector3D targetVelocity)
            {
                MyShipVelocities velocity = _provider.Velocity;
                Vector3 shipVelocity = velocity.LinearVelocity;
                Vector3 addVelocity = targetVelocity - shipVelocity;

                AddVelocity(addVelocity);
            }

            public void AddVelocity(Vector3D velocity)
            {
                double mass = _provider.Mass;

                Matrix shipMatrix = _provider.OrientationMatrix;
                Matrix lookMatrix = _provider.WorldMatrix;

                velocity *= mass;

                float forwardThrust, leftThrust, upThrust, backwardsThrust, rightThrust, downThrust;

                forwardThrust = (float)velocity.Dot(lookMatrix.Forward);
                leftThrust = (float)velocity.Dot(lookMatrix.Left);
                upThrust = (float)velocity.Dot(lookMatrix.Up);

                backwardsThrust = -forwardThrust;
                rightThrust = -leftThrust;
                downThrust = -upThrust;

                foreach (var thruster in _thrusters)
                {
                    Matrix thrusterMatrix;
                    thruster.Orientation.GetMatrix(out thrusterMatrix);
                    if (thrusterMatrix.Forward.Equals(shipMatrix.Up))
                    {
                        thruster.ThrustOverridePercentage = upThrust / thruster.MaxEffectiveThrust;
                        continue;
                    }
                    if (thrusterMatrix.Forward.Equals(shipMatrix.Forward))
                    {
                        thruster.ThrustOverridePercentage = forwardThrust / thruster.MaxEffectiveThrust;
                        continue;
                    }
                    if (thrusterMatrix.Forward.Equals(shipMatrix.Right))
                    {
                        thruster.ThrustOverridePercentage = rightThrust / thruster.MaxEffectiveThrust;
                        continue;
                    }
                    if (thrusterMatrix.Forward.Equals(shipMatrix.Down))
                    {
                        thruster.ThrustOverridePercentage = downThrust / thruster.MaxEffectiveThrust;
                        continue;
                    }
                    if (thrusterMatrix.Forward.Equals(shipMatrix.Backward))
                    {
                        thruster.ThrustOverridePercentage = backwardsThrust / thruster.MaxEffectiveThrust;
                        continue;
                    }
                    thruster.ThrustOverridePercentage = leftThrust / thruster.MaxEffectiveThrust;
                }
            }

            public void Rotate(Vector3D direction)
            {
                throw new Exception();
            }
        }
        #endregion
        #region DroneNetworkHandler
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
        #endregion
        #region DroneMoveController
        public class DroneMoveController
        {
            private OrbitMoveStrategy _orbitMove;

            private IMoveStrategy _moveStrategy;
            private IOrientationProvider _provider;
            private IEngine _engine;
            private ILogger _logger;

            public DroneMoveController(IOrientationProvider provider, IEngine engine, ILogger logger)
            {
                _provider = provider;
                _engine = engine;
                _logger = logger;

                Initialize();
            }

            private void Initialize()
            {
                _moveStrategy = new NullMoveStrategy();
                _orbitMove = new OrbitMoveStrategy(_provider, _engine);

                _logger.Log("DroneMoveController initialized");
            }

            public void Update()
            {
                _moveStrategy.UpdateMove();
            }

            public void SetPosition(DroneOrbitPosition position)
            {
                _orbitMove.Position = position;
                _moveStrategy = _orbitMove;
            }

            public void SetPosition(object position)
            {

            }
        }

        public interface IMoveStrategy
        {
            void UpdateMove();
        }

        public class NullMoveStrategy : IMoveStrategy
        {
            public void UpdateMove()
            {

            }
        }


        public enum EOrbitMove
        {
            MovingToRadius,
            MovingOnRadius,
        }

        public class OrbitMoveStrategy : IMoveStrategy
        {
            public const int
                RADIUS_THRESHHOLD = 2;

            public DroneOrbitPosition Position { get; set; }

            private IOrientationProvider _provider;
            private IEngine _engine;
            private EOrbitMove _orbitMoveType;

            public OrbitMoveStrategy(IOrientationProvider provider, IEngine engine)
            {
                _engine = engine;
                _provider = provider;
            }

            public void UpdateMove()
            {
                switch (_orbitMoveType)
                {
                    case (EOrbitMove.MovingToRadius):
                        MoveToInnerRadius();
                        break;
                    case (EOrbitMove.MovingOnRadius):
                        MoveOnInnerRadius();
                        break;
                    default:
                        break;
                }
            }

            private double GetDistanceToMain()
            {
                Vector3D position = _provider.Position;
                Vector3D targetPosition = Position.ToVector3D();

                double distance = Vector3D.DistanceSquared(position, targetPosition);
                return distance;
            }

            private double AngleBetweenVectors(Vector3 a, Vector3 b)
            {
                float dot = a.Dot(b);
                float aLength = a.Length();
                float bLength = b.Length();

                return Math.Acos(dot / (aLength * bLength));
            }

            private Vector3D Normal(Vector3D a, Vector3D b, Vector3D c)
            {
                var dir = Vector3D.Cross(b - a, c - a);
                var norm = Vector3D.Normalize(dir);
                return norm;
            }

            private void MoveToInnerRadius()
            {
                double distance = GetDistanceToMain();

                if (Math.Abs(Position.Radius - distance) > RADIUS_THRESHHOLD)
                {
                    Vector3D position = _provider.Position;
                    Vector3 radiusTargetDirection = position - Position.Origin;
                    radiusTargetDirection.Normalize();

                    Vector3D orbitCoord = Position.ToVector3DWithCustomDirection(radiusTargetDirection);

                    Vector3D targetVelocity = (orbitCoord - position);
                    _engine.SetVelocity(targetVelocity);
                }
                else
                {
                    _orbitMoveType = EOrbitMove.MovingOnRadius;
                }
            }

            private void MoveOnInnerRadius()
            {
                double distance = GetDistanceToMain();

                if (Math.Abs(Position.Radius - distance) > RADIUS_THRESHHOLD)
                {
                    Vector3D orbitDroneDirection = Vector3D.Normalize(_provider.Position - Position.Origin);

                    double angle = AngleBetweenVectors(Position.Direction, orbitDroneDirection);
                    double distanceToTarget = angle * Math.PI * Position.Radius / 180;

                    if (distanceToTarget > RADIUS_THRESHHOLD)
                    {
                        Vector3D position = _provider.Position;
                        Vector3D targetPosition = Position.ToVector3D();
                        Vector3D normal = Normal(Position.Origin, position, targetPosition);

                        Vector3D targetVelocity = Normal(normal, position, Position.Origin) * distanceToTarget;
                        _engine.SetVelocity(targetVelocity);
                    }
                }
                else
                {
                    _orbitMoveType = EOrbitMove.MovingToRadius;
                }
            }
        }
        #endregion
        #region DroneController
        public class DroneController
        {
            public const int
                NETWORK_TICKRATE = 1,
                MOVE_CONTROLLER_TICKRATE = 1,
                SYSTEM_UP_TICKRATE = 30;

            public DroneNetworkHandler NetworkHandler { get; private set; }
            public DroneMoveController MoveController { get; private set; }
            public INetwork Network { get; private set; }
            public ILogger Logger { get; private set; }
            public IEngine Engine { get; private set; }


            private int _tick;

            public DroneController(INetwork network, IOrientationProvider provider, IEngine engine, ILogger logger, long thrustedHost)
            {
                Engine = engine;
                Network = network;
                Logger = logger;

                Initialize(thrustedHost, provider);
            }

            private void Initialize(long thrustedHost, IOrientationProvider provider)
            {
                MoveController = new DroneMoveController(provider, Engine, Logger);
                NetworkHandler = new DroneNetworkHandler(this, thrustedHost);

                Logger.Log("DroneController initialized");
            }

            public void Update()
            {
                _tick++;

                if (_tick % NETWORK_TICKRATE == 0)
                {
                    NetworkHandler.Update();
                }

                if (_tick % MOVE_CONTROLLER_TICKRATE == 0)
                {
                    MoveController.Update();
                }

                if (_tick % SYSTEM_UP_TICKRATE == 0)
                {
                    Logger.Log("System is up");
                }
            }
        }
        #endregion
        #region Network
        public struct Message
        {
            public long Sender { get; set; }
            public Packet Packet { get; set; }
        }

        public interface INetwork
        {
            long ID { get; }
            bool HasPendingMessage { get; }

            bool TryAcceptMessage(ref Message message);

            void SendPacket(Packet data);

            void SendPacketToTarget(long target, Packet data);
        }

        public class Network : INetwork
        {
            protected const string TAG = "defensematrix";
            protected const TransmissionDistance DISTANCE = TransmissionDistance.TransmissionDistanceMax;

            public long ID => _igc.Me;
            public bool HasPendingMessage => _listener.HasPendingMessage;

            private IMyUnicastListener _listener;
            private IMyIntergridCommunicationSystem _igc;
            private ILogger _logger;

            public Network(IMyIntergridCommunicationSystem IGC, ILogger logger)
            {
                _igc = IGC;
                _logger = logger;
                Initialize();
            }

            private void Initialize()
            {
                _listener = _igc.UnicastListener;//_igc.RegisterBroadcastListener(TAG);
                _logger.Log("UnicastListener Initialized");
            }

            public bool TryAcceptMessage(ref Message message)
            {
                var igcMessage = _listener.AcceptMessage();
                string rawMessage = (string)igcMessage.Data;
                byte[] data = Convert.FromBase64String(rawMessage);
                Packet packet = new Packet(data);
                if (packet != null)
                {
                    message = new Message() { Sender = igcMessage.Source, Packet = packet };
                    //_logger.Log("Message accepted, Sender = " + message.Sender);
                    return true;
                }
                else
                {
                    message = new Message();
                    //_logger.Log("Cant't accept message");
                    return false;
                }
            }

            public void SendPacket(Packet data)
            {
                _igc.SendBroadcastMessage(TAG, "data", DISTANCE);
            }

            public void SendPacketToTarget(long target, Packet data)
            {
                byte[] bytes = data.ToArray();
                string message = Convert.ToBase64String(bytes);
                bool sent = _igc.SendUnicastMessage(target, TAG, message);
                //_logger.Log("Sending unicast packet to target " + target + ", success: " + sent);
            }
        }
        #endregion
        #region MainShipCommand
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
        #endregion
    }
}
