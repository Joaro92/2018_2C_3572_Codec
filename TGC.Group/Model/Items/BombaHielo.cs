using TGC.Core.Mathematica;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public class BombaHieloItem : WeaponItem
    {
        public BombaHieloItem(TGCVector3 pos) : base(pos, "Ice-Bomb")
        {
        }

        protected override void spawn()
        {
            base.spawn();
            Weapon = new BombaHielo();
        }
    }
}
