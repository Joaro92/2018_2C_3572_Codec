using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Group.Model.World;
using TGC.Group.Model.World.Weapons;

namespace TGC.Group.Model.Items
{
    public abstract class WeaponItem : Item
    {
        public Weapon Weapon { get; protected set; }

        public WeaponItem(TGCVector3 pos, string name) : base(pos, name)
        {
            respawnTime = 20f;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Dissapear(Device dsDevice)
        {
            base.Dissapear(dsDevice);

            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\weaponPickup.wav", Position, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override void Effect(Player1 player1)
        {
            player1.AddWeapon(Weapon);
        }

        public override float DesplazamientoY => 0.2f;
    }
}
