using TGC.Core.Mathematica;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public class CoheteItem : WeaponItem
    {
        public CoheteItem(TGCVector3 pos) : base(pos, "Rocket")
        {
            Weapon = new Cohete();
        }

        protected override void spawn()
        {
            base.spawn();
            Weapon = new Cohete();
        }
    }
}