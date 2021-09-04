namespace DroneSpaceEngineers
{
    public class MainShipController
    {
        public const int
            DRONE_POOL_TICKRATE = 5,
            NETWORK_TICKRATE = 1,
            POSITION_DISTRIBUTOR_TICKRATE = 1,
            SYSTEM_UP_TICKRATE = 15,
            DRONE_LOGGER_TICKRATE = 5;

        public MainShipNetworkHandler NetworkHandler { get; private set; }
        public DronePool DronePool { get; private set; }
        public PositionDistributor PositionDistributor { get; private set; }
        public INetwork Network { get; private set; }
        public ILogger Logger { get; private set; }
        public IDroneLogger DroneLogger { get; private set; }

        private long _tick = 0;

        public MainShipController(INetwork network, IOrientationProvider provider, IDroneLogger droneLogger, ILogger logger)
        {
            Network = network;
            Logger = logger;
            DroneLogger = droneLogger;

            Initialize(provider);
        }

        private void Initialize(IOrientationProvider provider)
        {
            NetworkHandler = new MainShipNetworkHandler(this);
            DronePool = new DronePool(null, Logger);
            PositionDistributor = new PositionDistributor(DronePool, NetworkHandler, provider, Logger);

            DroneLogger.SetDrones(DronePool.GetAllDrones());
            DroneLogger.Update();

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

            if (_tick % DRONE_LOGGER_TICKRATE == 0)
            {
                DroneLogger.Update();
            }
        }
    }
}
