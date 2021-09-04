namespace DroneSpaceEngineers
{
    public class AliveChecker
    {
        private DroneNetworkHandler _network;

        public AliveChecker(DroneNetworkHandler network)
        {
            _network = network;
        }

        public void Update()
        {
            NotifyAliveCommand command = _network.CommandFactory.Create(EDroneCommand.NotifyAlive) as NotifyAliveCommand;
            command.Initialize();
            _network.SendCommandToHost(command);
        }
    }
}
