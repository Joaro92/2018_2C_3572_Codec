using BulletSharp;

namespace TGC.Group.Model.World.Weapons
{
    public class BombaHielo : Weapon
    {
        public BombaHielo() : base(3, "Ice-Bomb", 15)
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
