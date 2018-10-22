using Microsoft.DirectX.DirectSound;
using TGC.Core.Mathematica;
using TGC.Core.Sound;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Energy : Item
    {
        private readonly float gainRate = 0.5f;

        public Energy(TGCVector3 pos) : base(pos, "Energy")
        {
            Name = "Energy";
            respawnTime = 5f;
        }

        public override void Dissapear(Device dsDevice)
        {
            base.Dissapear(dsDevice);

            sound = new Tgc3dSound(Game.Default.MediaDirectory + "Sounds\\FX\\specialPickup.wav", Position, dsDevice);
            sound.MinDistance = 150f;
            sound.play(false);
        }

        public override float DesplazamientoY => 0.10f;

        public override void Effect(Player1 player1)
        {
            var specialPointsGained = gainRate * player1.maxSpecialPoints;
            player1.specialPoints = FastMath.Min(player1.maxSpecialPoints, player1.specialPoints + specialPointsGained);
        }
    }
}
