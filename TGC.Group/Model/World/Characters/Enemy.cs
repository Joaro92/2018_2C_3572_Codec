using BulletSharp;
using BulletSharp.Math;
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

        public void CalculateImpactDistanceAndReact(Vector3 impactPos)
        {
            distanceToExplosion = (impactPos - rigidBody.CenterOfMassPosition).Length;

            if (distanceToExplosion < 25)
            {
                var forceVector = rigidBody.CenterOfMassPosition - new Vector3(impactPos.X, impactPos.Y - 3, impactPos.Z);
                forceVector.Normalize();
                rigidBody.ApplyImpulse(forceVector * 23.33f, new Vector3(impactPos.X, impactPos.Y - 3, impactPos.Z));
            }

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
            ia.timerJustAppeared = 2f;
        }

        protected override void Straighten()
        {
            base.Straighten();
            ia.justAppeared = true;
            ia.timerJustAppeared = 2f;
        }
    }
}
