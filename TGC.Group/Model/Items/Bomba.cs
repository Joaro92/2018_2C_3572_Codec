using TGC.Core.Mathematica;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public class BombaItem : WeaponItem
    {
        public BombaItem(TGCVector3 pos) : base(pos,"Bomb")
        {
            Weapon = new Bomba();
        }

        protected override void spawn()
        {
            base.spawn();
            Weapon = new Bomba();
        }

    }
}
