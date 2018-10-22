using TGC.Core.Mathematica;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public class PowerItem : WeaponItem
    {
        public PowerItem(TGCVector3 pos) : base(pos, "Power")
        {
        }

        protected override void spawn()
        {
            base.spawn();
            Weapon = new Power();
        }
    }
}