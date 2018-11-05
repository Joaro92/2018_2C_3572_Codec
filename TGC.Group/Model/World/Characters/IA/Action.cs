using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.World.Characters.IA
{
    public delegate void Proc(); //tipo Proc para guardar metodos que no reciben parametros ni devuelven nada

    public class Action
    {
        public Proc proc { get; set; }
        public float t { get; set; }

        public Action (Proc proc, float t)
        {
            this.proc = proc;
            this.t = t;
        }
    }
}
