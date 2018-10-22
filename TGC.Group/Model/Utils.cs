using BulletSharp.Math;
using System.Drawing;
using System.Drawing.Text;
using TGC.Core.Mathematica;

namespace TGC.Group.Utils
{

    public enum MenuOption { PLAY, CONTROLS, EXIT };

    public enum ModoCamara { NORMAL, CERCA, LEJOS };

    public static class WheelContactInfo
    {
        public static Vector4 contactInfoByChassis(string meshName)
        {
            switch (meshName)
            {
                //                    ( eje X,  eje Y ,  back ,  front)
                case "car-minibus":
                    return new Vector4(1.215f, -1.148f, 2.080f, 2.294f);
                case "car-coupe":
                    return new Vector4(1.065f, -0.614f, 1.828f, 1.847f);
                case "car-hatchback":
                    return new Vector4(1.113f, -0.947f, 1.526f, 1.715f);
                case "car-micro":
                    return new Vector4(1.035f, -0.925f, 1.424f, 1.351f);
                case "car-microcargo":
                    return new Vector4(1.118f, -1.144f, 1.717f, 1.732f);
                case "car-mpv":
                    return new Vector4(1.065f, -1.078f, 1.868f, 2.047f);
                case "car-normal":
                    return new Vector4(1.065f, -1.016f, 1.593f, 1.782f);
                case "car-pickup":
                    return new Vector4(1.113f, -1.122f, 1.723f, 2.011f);
                case "car-pickup-small":
                    return new Vector4(1.065f, -1.017f, 1.440f, 1.935f);
                case "car-station":
                    return new Vector4(1.065f, -1.014f, 1.741f, 2.194f);
                default:
                    return Vector4.Zero;
            }
        }
    }

    public static class ModoCamaraMethods
    {
        public static float AlturaCamara(this ModoCamara m)
        {
            switch (m)
            {
                case ModoCamara.NORMAL:
                    return 1f;
                case ModoCamara.CERCA:
                    return 0.2f;
                case ModoCamara.LEJOS:
                    return 4.5f;
                default:  return 0f;
            }
        }

        public static float ProfundidadCamara(this ModoCamara m)
        {
            switch (m)
            {
                case ModoCamara.NORMAL:
                    return 18f;
                case ModoCamara.CERCA:
                    return 12f;
                case ModoCamara.LEJOS:
                    return 28f;
                default: return 0f;

            }
        }
    }

    public static class UtilMethods
    {
        public static T getNextOption <T> (this T[] options, T selectedOption, int direction = 1)
        {
            int newIndex = (System.Array.FindIndex(options, c => c.Equals(selectedOption)) + direction) % options.Length;
            if (newIndex < 0)
                newIndex = options.Length + newIndex;
            return options[newIndex];
        }

        public static Font createFont(string fontName, int size)
        {
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile(Game.Default.MediaDirectory + Game.Default.FontsDirectory + fontName + ".ttf");
            FontFamily family = pfc.Families[0];
            return new Font(family, size);
        }
    }

    public static class Quat
    {
        public static TGCVector3 rotate_vector_by_quaternion(TGCVector3 v, Quaternion q)
        {
            // Extract the vector part of the quaternion
            TGCVector3 u = new TGCVector3(q.X, q.Y, q.Z);

            // Extract the scalar part of the quaternion
            float s = q.W;

            // Do the math
            var vprime = 2.0f * TGCVector3.Dot(u, v) * u
                + (s * s - TGCVector3.Dot(u, u)) * v
                + 2.0f * s * TGCVector3.Cross(u, v);

            return vprime;
        }

        public static Vector3 ToEulerAngles(this Quaternion q)
        {
            // Store the Euler angles in radians
            Vector3 pitchYawRoll = new Vector3();
            float PI = FastMath.PI;
            float sqw = q.W * q.W;
            float sqx = q.X * q.X;
            float sqy = q.Y * q.Y;
            float sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            float unit = sqx + sqy + sqz + sqw;
            float test = q.X * q.Y + q.Z * q.W;

            if (test > 0.499f * unit)
            {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * FastMath.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = PI * 0.5f; // Pitch
                pitchYawRoll.Z = 0f; // Roll
                return pitchYawRoll;
            }
            else if (test < -0.499f * unit)
            {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * FastMath.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = -PI * 0.5f; // Pitch
                pitchYawRoll.Z = 0f; // Roll
                return pitchYawRoll;
            }

            pitchYawRoll.Y = FastMath.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw); // Yaw
            pitchYawRoll.X = FastMath.Asin(2 * test / unit); // Pitch
            pitchYawRoll.Z = FastMath.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw); // Roll

            return pitchYawRoll;
        }
    }

}
