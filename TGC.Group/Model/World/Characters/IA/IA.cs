using System;
using TGC.Group.Model;
using TGC.Group.Physics;

namespace TGC.Group.World
{
    public class IA
    {
        private float timer = 0f;

        public void TakeAction(Enemy e, GameModel gameModel, PhysicsGame nivel)
        {
            if(timer >= 5f){
                e.FireMachinegun(gameModel, nivel);
            }
            if(timer >= 10f)
            {
                timer = 0;
            }

            timer += gameModel.ElapsedTime;
        }
    }
}