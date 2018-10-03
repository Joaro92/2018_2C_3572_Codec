using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Utils
{

    public enum ModoCamara { NORMAL, CERCA, LEJOS };

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
