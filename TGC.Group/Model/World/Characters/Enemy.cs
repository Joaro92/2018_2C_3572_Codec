using BulletSharp;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Characters;
using TGC.Group.Physics;
using TGC.Group.World.Characters.ArtificialIntelligence;

namespace TGC.Group.World.Characters
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
            // Chequea y actualiza el status del Salto
            CheckJumpStatus(gameModel);

            //reseteo todo
            ResetBrake();
            ResetSteering();
            ResetEngineForce();
            TurboOff();

            ia.ShootMachineGun = false;
            ia.ShootSpecialWeapon = false;

            ia.TakeAction(this, gameModel, nivel);

        }

        public override void Respawn(bool inflictDmg, TGCVector3 initialPos, float rotation)
        {
            base.Respawn(inflictDmg, initialPos, rotation);
            ia.justAppeared = true;
        }

        protected override void Straighten()
        {
            base.Straighten();
            ia.justAppeared = true;
        }
    }
}
