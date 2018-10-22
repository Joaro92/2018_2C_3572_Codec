using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Health : Item
    {
        private readonly float healingRate = 0.5f;

        public Health(TGCVector3 pos) : base(pos, "Heart")
        {
            respawnTime = 10f;
        }

        public override void Dissapear(Device dsDevice)
        {
            base.Dissapear(dsDevice);

            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\healthPickup.wav", Position, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override float DesplazamientoY => 0.3f;

        public override void Effect(Player1 player1)
        {
            var hitPointsGained =  healingRate * player1.maxHitPoints;
            player1.hitPoints = FastMath.Min(player1.maxHitPoints, player1.hitPoints + hitPointsGained);
        }
    }
}
