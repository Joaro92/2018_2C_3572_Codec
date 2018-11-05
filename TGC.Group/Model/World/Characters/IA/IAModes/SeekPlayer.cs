using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Group.Physics;
using TGC.Group.Utils;

namespace TGC.Group.World.Characters.ArtificialIntelligence
{
    internal class SeekPlayer : Seek
    {
        private readonly float maxDistanceToShoot = 100f;
        private readonly float minDistanceToSeekWeapons = 200f;
        private readonly float rateToSeekHealth = 0.2f;

        public SeekPlayer(PhysicsGame nivel) : base(nivel)
        {
        }

        public override void Do(IA ia)
        {
            var e = nivel.enemy;
            var target = new TGCVector3(nivel.player1.RigidBody.CenterOfMassPosition);
            var oriented = DoSearch(target);

            //orden para disparar si estoy suficientemente cerca
            if (oriented && this.Distance(target) <= maxDistanceToShoot)
            {
                if(e.SelectedWeapon != null)
                {
                    ia.ShootSpecialWeapon = true;
                }
                else
                {
                    ia.ShootMachineGun = true;
                }
            }
            else if(e.hitPoints <= e.maxHitPoints * rateToSeekHealth) //si no disparo y tengo poca vida voy a buscar salud
            {
                ia.currentMode = new SeekHealth(nivel);
            }
            else if(e.SelectedWeapon == null && this.Distance(target) >= minDistanceToSeekWeapons) // si tengo vida y estoy lejos del player voy a buscar armas
            {
                ia.currentMode = new SeekWeapon(nivel);
            }
        }
    }
}