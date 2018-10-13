using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.Group.Model.Interfaces
{
    public interface IGameState
    {
        void Update();
        void Render();
        void Dispose();
    }
}
