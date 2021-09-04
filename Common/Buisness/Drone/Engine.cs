using Sandbox.ModAPI.Ingame;
using System;
using VRageMath;

namespace DroneSpaceEngineers
{
    public interface IEngine
    {
        void SetVelocity(Vector3D targetVelocity);

        void AddVelocity(Vector3D velocity);

        void SetRotation(Vector3D direction, float rollAngle = 0);

        void Rotate(float pitch, float roll, float yaw);
    }

    public class Engine : IEngine
    {
        private IOrientationProvider _provider;
        private IMyThrust[] _thrusters;
        private IMyGyro[] _gyros;

        public Engine(IMyThrust[] thrusters, IMyGyro[] gyros, IOrientationProvider provider)
        {
            _thrusters = thrusters;
            _gyros = gyros;
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
            Matrix worldMatrix = _provider.WorldMatrix;

            velocity *= mass;

            float forwardThrust, leftThrust, upThrust, backwardsThrust, rightThrust, downThrust;

            forwardThrust = -(float)velocity.Dot(worldMatrix.Forward);
            leftThrust = -(float)velocity.Dot(worldMatrix.Left);
            upThrust = -(float)velocity.Dot(worldMatrix.Up);

            backwardsThrust = -forwardThrust;
            rightThrust = -leftThrust;
            downThrust = -upThrust;

            float forwardThrustMax = 0,
                leftThrustMax = 0,
                upThrustMax = 0,
                backwardsThrustMax = 0,
                rightThrustMax = 0,
                downThrustMax = 0;

            foreach (var thruster in _thrusters)
            {
                Matrix thrusterMatrix;
                thruster.Orientation.GetMatrix(out thrusterMatrix);
                if (thrusterMatrix.Forward.Equals(shipMatrix.Up))
                {
                    upThrustMax += thruster.MaxEffectiveThrust;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Forward))
                {
                    forwardThrustMax += thruster.MaxEffectiveThrust;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Right))
                {
                    rightThrustMax += thruster.MaxEffectiveThrust;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Down))
                {
                    downThrustMax += thruster.MaxEffectiveThrust;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Backward))
                {
                    backwardsThrustMax += thruster.MaxEffectiveThrust;
                    continue;
                }
                leftThrustMax += thruster.MaxEffectiveThrust;
            }

            foreach (var thruster in _thrusters)
            {
                Matrix thrusterMatrix;
                thruster.Orientation.GetMatrix(out thrusterMatrix);
                if (thrusterMatrix.Forward.Equals(shipMatrix.Up))
                {
                    thruster.ThrustOverridePercentage = upThrust / upThrustMax;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Forward))
                {
                    thruster.ThrustOverridePercentage = forwardThrust / forwardThrustMax;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Right))
                {
                    thruster.ThrustOverridePercentage = rightThrust / rightThrustMax;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Down))
                {
                    thruster.ThrustOverridePercentage = downThrust / downThrustMax;
                    continue;
                }
                if (thrusterMatrix.Forward.Equals(shipMatrix.Backward))
                {
                    thruster.ThrustOverridePercentage = backwardsThrust / backwardsThrustMax;
                    continue;
                }
                thruster.ThrustOverridePercentage = leftThrust / leftThrustMax;
            }
        }

        public void SetRotation(Vector3D forward, float rollAngle = 0)
        {
            MatrixD matrix = _provider.WorldMatrix;

            double pitch = AngleBetweenVectors(forward, matrix.Forward, matrix.Left);
            double roll = AngleBetweenVectors(forward, matrix.Forward, Matrix3x3.CreateRotationX(rollAngle).Forward);
            double yaw = AngleBetweenVectors(forward, matrix.Forward, matrix.Up);

            Rotate((float)pitch, (float)roll, (float)yaw);
        }


        public double AngleBetweenVectors(Vector3D a, Vector3D b, Vector3D normal)
        {
            double dot = a.Dot(b);
            double aLength = a.Length();
            double bLength = b.Length();
            double angle = Math.Acos(dot / (aLength * bLength));

            float sign = Math.Sign(Vector3.Dot(normal, Vector3.Cross(a, b)));
            angle *= sign;

            return angle;
        }

        public void Rotate(float pitch, float roll, float yaw)
        {
            int length = _gyros.Length;
            for (int i = 0; i < length; i++)
            {
                IMyGyro gyro = _gyros[i];
                gyro.GyroPower = 0.5f;
                gyro.GyroOverride = true;
                gyro.Roll = roll; // Rigth
                gyro.Pitch = pitch; //Forward
                gyro.Yaw = yaw; //Up
            }
        }
    }
}
