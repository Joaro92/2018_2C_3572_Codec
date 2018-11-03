using System;
using TGC.Group.Model;
using TGC.Group.Physics;

namespace TGC.Group.World
{
    public class IA
    {
        public void TakeAction(Enemy e, GameModel gameModel, PhysicsGame nivel)
        {
            e.FireMachinegun(gameModel, nivel);
        }
    }
}