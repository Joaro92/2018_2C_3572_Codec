namespace TGC.Group.Model.World.Weapons
{
    public class BombaHielo : Weapon
    {
        public BombaHielo() : base(3, "Ice-Bomb", 15)
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
