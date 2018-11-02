using TGC.Core.Mathematica;
using TGC.Group.Model.World.Characters;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public abstract class WeaponItem : Item
    {
        public Weapon Weapon { get; protected set; }

        public WeaponItem(TGCVector3 pos, string name) : base(pos, name, Game.Default.MediaDirectory + Game.Default.FXDirectory + "weaponPickup.wav")
        {
            respawnTime = 20f;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Effect(Character character)
        {
            character.AddWeapon(Weapon);
        }

        public override float DesplazamientoY => 0.2f;
    }
}
