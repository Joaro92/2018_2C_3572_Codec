namespace TGC.Group.Model.World.Weapons
{
    public class Bomba : Weapon
    {
        public Bomba() : base(1, "Bomb", 10)
        {
            //Otras inicializaciones
        }

        public override void Fire()
        {
            //Fisica del disparo
            base.Fire();
        }
    }
}
