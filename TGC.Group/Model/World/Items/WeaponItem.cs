using TGC.Core.Mathematica;
using TGC.Group.Model.World;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public class WeaponItem : Item
    {
        public Weapon Weapon { get; protected set; }

        public WeaponItem(TGCVector3 pos, string name) : base(pos, name)
        {
            respawnTime = 20f;
        }

        public override void Dispose()
        {
            base.Dispose();
            //weapon.Dispose();
        }

        public override void Effect(Player1 player1)
        {
            player1.AddWeapon(Weapon);
        }
    }
}
