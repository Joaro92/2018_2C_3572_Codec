namespace TGC.Group.Model.World.Weapons
{
    public class Cohete : Weapon
    {
        public Cohete() : base(2, "Rocket", 5)
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
