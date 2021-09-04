using System;
using System.Collections.Generic;
using System.Text;

namespace DroneSpaceEngineers
{
    public interface IDroneFactory
    {
        Drone CreateDrone(EDrone type);
    }

    public class DroneFactory : IDroneFactory
    {


        public DroneFactory()
        {

        }

        public Drone CreateDrone(EDrone type)
        {
            throw new NotImplementedException();
        }
    }
}
