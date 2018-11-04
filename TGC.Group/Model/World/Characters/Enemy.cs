using BulletSharp;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Characters;
using TGC.Group.Physics;

namespace TGC.Group.World
{
    public class Enemy : Character
    {
        private IA ia;

        public Enemy(DiscreteDynamicsWorld world, TGCVector3 position, float orientation, GameModel gameModel) : base(world, Vehiculo.GetRandom(), position, orientation, gameModel)
        {
            ia = new IA();
        }

        public void TakeAction(GameModel gameModel, PhysicsGame nivel)
        {
            ia.TakeAction(this,gameModel, nivel);
        }

    }
}
