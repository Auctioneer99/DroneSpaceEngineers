using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DroneSpaceEngineers
{
    public class DroneMoveController
    {
        private OrbitMoveStrategy _orbitMove;

        private IMoveStrategy _moveStrategy;
        private IOrientationProvider _provider;
        private IEngine _engine;
        private ILogger _logger;
        private DroneNetworkHandler _network;

        public DroneMoveController(IOrientationProvider provider, IEngine engine, DroneNetworkHandler network, ILogger logger)
        {
            _provider = provider;
            _engine = engine;
            _logger = logger;
            _network = network;

            Initialize();
        }

        private void Initialize()
        {
            _moveStrategy = new NullMoveStrategy();
            _orbitMove = new OrbitMoveStrategy(_provider, _engine, _network, _logger);

            _logger.Log("DroneMoveController initialized");
        }

        public void Update()
        {
            _moveStrategy.UpdateMove();
            _moveStrategy.UpdateRotation();
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
        void UpdateRotation();
    }

    public class NullMoveStrategy : IMoveStrategy
    {
        public void UpdateMove()
        {
            
        }

        public void UpdateRotation()
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
            RADIUS_THRESHHOLD = 7,
            MAX_VELOCITY = 16;

        public DroneOrbitPosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                MoveType = EDroneMove.Running;
            }
        }
        private DroneOrbitPosition _position;
        public EDroneMove MoveType
        {
            get
            {
                return _moveType;
            }
            private set
            {
                _moveType = value;
                SetPositionDroneCommand command = _network.CommandFactory.Create(EDroneCommand.SetPosition) as SetPositionDroneCommand;
                command.Initialize(value);
                _network.SendCommandToHost(command);
            }
        }
        private EDroneMove _moveType;

        private DroneNetworkHandler _network;
        private IOrientationProvider _provider;
        private IEngine _engine;
        private ILogger _logger;
        private EOrbitMove _orbitMoveType;

        public OrbitMoveStrategy(IOrientationProvider provider, IEngine engine, DroneNetworkHandler network, ILogger logger)
        {
            _engine = engine;
            _provider = provider;
            _logger = logger;
            _network = network;
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

        public void UpdateRotation()
        {
            Vector3D forward = Position.Direction;
            _engine.SetRotation(forward);
        }

        private double GetDistanceToOrigin()
        {
            Vector3D position = _provider.Position;
            Vector3D origin = Position.Origin;

            double distance = Vector3D.Distance(position, origin);
            return distance;
        }

        private double AngleBetweenVectors(Vector3D a, Vector3D b, Vector3D normal)
        {
            double dot = a.Dot(b);
            double aLength = a.Length();
            double bLength = b.Length();
            double angle = Math.Acos(dot / (aLength * bLength));

            float sign = Math.Sign(Vector3.Dot(normal, Vector3.Cross(a, b)));
            angle *= sign;

            return angle;
        }

        private Vector3D Normal(Vector3D a, Vector3D b, Vector3D c)
        {
            var dir = Vector3D.Cross(b - a, c - a);
            var norm = Vector3D.Normalize(dir);
            return norm;
        }

        private Vector3D HandleMaxVelocity(Vector3D velocity)
        {
            if (velocity.Length() > MAX_VELOCITY)
            {
                velocity = Vector3D.Normalize(velocity) * MAX_VELOCITY;
            }
            return velocity;
        }

        private void MoveToInnerRadius()
        {
            double distance = GetDistanceToOrigin();

            if (Math.Abs(Position.Radius - distance) > RADIUS_THRESHHOLD)
            {
                Vector3D position = _provider.Position;
                Vector3 radiusTargetDirection = position - Position.Origin;
                radiusTargetDirection.Normalize();

                Vector3D orbitCoord = Position.ToVector3DWithCustomDirection(radiusTargetDirection);

                Vector3D targetVelocity = HandleMaxVelocity(orbitCoord - position);

                _engine.SetVelocity(targetVelocity);
            }
            else
            {
                _orbitMoveType = EOrbitMove.MovingOnRadius;
            }
        }

        private void MoveOnInnerRadius()
        {
            double distance = GetDistanceToOrigin();

            if (Math.Abs(Position.Radius - distance) < RADIUS_THRESHHOLD)
            {
                Vector3D orbitDroneDirection = Vector3D.Normalize(_provider.Position - Position.Origin);
                Vector3D targetDirection = Position.Direction;

                double angle = AngleBetweenVectors(
                        orbitDroneDirection,
                        targetDirection,
                        Normal(orbitDroneDirection, targetDirection, Vector3D.Zero));
                double distanceToTarget = Math.Abs(angle) * Position.Radius;

                Vector3D normal = Normal(Normal(Vector3D.Zero, orbitDroneDirection, targetDirection), orbitDroneDirection, Vector3D.Zero);
                Vector3D tangentialVelocity = HandleMaxVelocity(normal * distanceToTarget);
                double tangentialSpeed = tangentialVelocity.Length();
                Vector3D targetVelocity = tangentialVelocity + (tangentialSpeed * tangentialSpeed / Position.Radius) * orbitDroneDirection;
                _engine.SetVelocity(targetVelocity);

                if (distanceToTarget < RADIUS_THRESHHOLD)
                {
                    MoveType = EDroneMove.Completed;
                }
            }
            else
            {
                _orbitMoveType = EOrbitMove.MovingToRadius;
            }
        }
    }
}
