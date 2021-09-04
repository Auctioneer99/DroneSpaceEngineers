using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace DroneSpaceEngineers
{
    public class PositionDistributor
    {
        public const int 
            DEFENSE_DRONES_INNER_RADIUS = 100,
            DEFENSE_DRONES_OUTER_RADIUS = 120;

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
            for(int i = 0; i < length; i++)
            {
                Drone drone = drones[i];
                if (drone != null && drone.IsAlive)
                {
                    DroneOrbitPosition position = new DroneOrbitPosition();
                    switch(drone.MoveType)
                    {
                        case EDroneMove.Idle:

                            position.Radius = DEFENSE_DRONES_INNER_RADIUS;
                            break;
                        case EDroneMove.Running:
                            continue;
                        case EDroneMove.Completed:
                            position.Radius = DEFENSE_DRONES_OUTER_RADIUS;
                            break;
                    }
                    position.Origin = _orientationProvider.Position;
                    position.Direction = _desiredDefensePositions[i];

                    DesiredOrbitPositionCommand command = CommandFactory.Create(EMainShipCommand.DesiredOrbitPosition) as DesiredOrbitPositionCommand;
                    command.Initialize(position);
                    _network.SendCommandToTarget(command, drone.ID);
                }
            }
        }

        private Vector3D[] CalculateDefensePositionDistribution(int numDrones)
        {
            Vector3D[] vectors = new Vector3D[6]
                       {
                    new Vector3D(1, 0, 0),
                    new Vector3D(0, 1, 0),
                    new Vector3D(0, 0, 1),
                    new Vector3D(-1, 0, 0),
                    new Vector3D(0,-1, 0),
                    new Vector3D(0, 0, -1),
                       };
            return vectors;
            /*
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
            return vectors;*/
        }
    }
}
