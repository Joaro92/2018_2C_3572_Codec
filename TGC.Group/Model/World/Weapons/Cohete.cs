using BulletSharp;

namespace TGC.Group.Model.World.Weapons
{
    public class Cohete : Weapon
    {
        public Cohete() : base(2, "Power Missile", 5)
        {
            //Otras inicializaciones
        }

        public override void Fire(DiscreteDynamicsWorld world, Player1 player1)
        {
            //Fisica del disparo
            base.Fire(world, player1);
        }
    }
}
