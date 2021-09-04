using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace DroneSpaceEngineers
{
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
}
