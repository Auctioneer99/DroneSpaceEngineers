using System.Text;
using VRageMath;

namespace DroneSpaceEngineers
{
    public struct DroneOrbitPosition
    {
        public Vector3D Origin { get; set; }
        public Vector3D Direction { get; set; }
        public int Radius { get; set; }

        public Vector3D ToVector3D()
        {
            return ToVector3DWithCustomDirection(Direction);
        }

        public Vector3D ToVector3DWithCustomDirection(Vector3 dir)
        {
            Vector3D position = Origin + dir * Radius;
            return position;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Origin: {Origin}, ");
            sb.Append($"Direction: {Direction}, ");
            sb.Append($"Radius: {Radius}, ");
            return sb.ToString();
        }
    }
}
