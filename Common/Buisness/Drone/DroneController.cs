namespace DroneSpaceEngineers
{
    public class DroneController
    {
        public const int
            NETWORK_TICKRATE = 1,
            MOVE_CONTROLLER_TICKRATE = 1,
            SYSTEM_UP_TICKRATE = 30,
            ALIVE_CHECK_TICKRATE = 10;

        public DroneNetworkHandler NetworkHandler { get; private set; }
        public DroneMoveController MoveController { get; private set; }
        public AliveChecker AliveChecker { get; private set; }
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
            NetworkHandler = new DroneNetworkHandler(this, thrustedHost);
            MoveController = new DroneMoveController(provider, Engine, NetworkHandler, Logger);
            AliveChecker = new AliveChecker(NetworkHandler);

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

            if (_tick % ALIVE_CHECK_TICKRATE == 0)
            {
                AliveChecker.Update();
            }

            if (_tick % SYSTEM_UP_TICKRATE == 0)
            {
                Logger.Log("System is up");
            }
        }
    }
}
