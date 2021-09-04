using Sandbox.ModAPI.Ingame;
using System;
using System.Text;

namespace DroneSpaceEngineers
{
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
}
