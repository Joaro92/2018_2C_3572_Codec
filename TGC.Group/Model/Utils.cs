using BulletSharp.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;

namespace TGC.Group.Utils
{

    public enum ModoCamara { NORMAL, CERCA, LEJOS };
    //public enum TipoChasis { COUPE, HATCHBACK, MICRO, MICROCARGO, MICROTRANSPORT, MINIBUS, MPV, NORMAL, PICKUP, SMALLPICKUP, STATION, VAN }

    static class WheelContactInfo
    {
        public static Vector4 contactInfoByChassis(string meshName)
        {
            switch (meshName)
            {
                case "car-minibus":
                    return new Vector4(1.215f, -1.148f, 2.08f, 2.294f);
                case "car-coupe":
                    return new Vector4(1.065f, -0.614f, 1.828f, 1.847f);
                default:
                    return Vector4.Zero;
            }
        }
    }

    static class ModoCamaraMethods
    {
        public static float AlturaCamara(this ModoCamara m)
        {
            switch (m)
            {
                case ModoCamara.NORMAL:
                    return 1f;
                case ModoCamara.CERCA:
                    return 0.1f;
                case ModoCamara.LEJOS:
                    return 5f;
                default:  return 0f;
            }
        }

        public static float ProfundidadCamara(this ModoCamara m)
        {
            switch (m)
            {
                case ModoCamara.NORMAL:
                    return 20f;
                case ModoCamara.CERCA:
                    return 10f;
                case ModoCamara.LEJOS:
                    return 30f;
                default: return 0f;

            }
        }
    }

}
