using BulletSharp.Math;

namespace TGC.Group.Utils
{

    public enum MenuOption { PLAY, CONTROLS, EXIT };

    public enum ModoCamara { NORMAL, CERCA, LEJOS };

    public enum JoystickDpad { UP, RIGHT, DOWN, LEFT };

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
            return options[(System.Array.FindIndex(options, c => c.Equals(selectedOption)) + direction) % options.Length];
        }
    }

    

}
