using System;
using System.Collections;
using System.Linq;

namespace DroneSpaceEngineers
{
    public enum EDrone : byte
    {
        Defense
    }

    public enum EDroneMove: byte
    {
        Idle,
        Running,
        Completed
    }

    public class Drone
    {
        public long ID { get; private set; }
        public EDrone DroneType { get; private set; }
        public EDroneMove MoveType { get; set; }
        public bool IsAlive { get; set; }
        public long LastCheck { get; set; }

        public Drone(long id, EDrone type)
        {
            ID = id;
            DroneType = type;
            IsAlive = true;
            LastCheck = DateTime.Now.Ticks;
        }
    }

    public interface IDroneProvider
    {
        int DefenseDronesCount { get; }

        Drone[] GetDronesOfType(EDrone type);
    }

    public class DronePool : IDroneProvider
    {
        public const int 
            MAX_DEFENSE_DRONE = 7,
            MAX_DRONE_PING = 5 * 1000;

        public bool ShouldCreateNewDrones { get; set; } = false;
        public int DefenseDronesCount => MAX_DEFENSE_DRONE;

        private IDroneFactory _factory;
        private ILogger _logger;

        private Drone[] _drones;

        public DronePool(IDroneFactory factory, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
            Initialize();
        }

        private void Initialize()
        {
            _drones = new Drone[MAX_DEFENSE_DRONE];
            _logger.Log("DronePool Initialized");
        }

        public void Update()
        {
            UpdateDeadDrones();
            RemoveDeadDrones();

            if (ShouldCreateNewDrones)
            {

            }
        }

        private void UpdateDeadDrones()
        {
            int length = _drones.Length;
            long now = DateTime.Now.Ticks;
            for (int i = 0; i < length; i++)
            {
                Drone drone = _drones[i];
                if (drone != null && drone.IsAlive && drone.LastCheck + MAX_DRONE_PING < now)
                {
                    drone.IsAlive = false;
                }
            }
        }

        private void RemoveDeadDrones()
        {
            int length = _drones.Length;
            for (int i = 0; i < length; i++)
            {
                Drone drone = _drones[i];
                if (drone != null && drone.IsAlive == false)
                {
                    _drones[i] = null;
                }
            }
        }

        public bool TryConnectDrone(Drone drone)
        {
            for (int i = 0; i < MAX_DEFENSE_DRONE; i++)
            {
                Drone checkDrone = _drones[i];
                if (checkDrone != null && checkDrone.ID == drone.ID)
                {
                    _drones[i] = drone;
                    return true;
                }
            }

            for (int i = 0; i < MAX_DEFENSE_DRONE; i++)
            {
                Drone checkDrone = _drones[i];
                if (checkDrone == null)
                {
                    _drones[i] = drone;
                    return true;
                }
            }
            return false;
        }

        public void DisconnectDrone(long id)
        {
            for(int i = 0; i < MAX_DEFENSE_DRONE; i++)
            {
                Drone drone = _drones[i];
                if (drone != null && drone.ID == id)
                {
                    _drones[i] = null;
                }
            }
        }

        public void HealthCheckHandle(long id)
        {
            foreach (var d in _drones.Where(d => d != null && d.ID == id))
            {
                d.LastCheck = DateTime.Now.Ticks;
            }
        }

        public void UpdateDronePosition(long id, EDroneMove move)
        {
            foreach (var d in _drones.Where(d => d != null && d.ID == id))
            {
                d.MoveType = move;
            }
        }

        public Drone[] GetAllDrones()
        {
            return _drones;
        }

        public Drone[] GetDronesOfType(EDrone type)
        {
            return _drones.Where(d => d != null && d.DroneType == type).ToArray();
        }
    }
}
