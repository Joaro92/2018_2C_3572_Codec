using TGC.Core.Mathematica;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Corazon : Item
    {
        private readonly float healingRate = 0.5f;

        public Corazon(TGCVector3 pos) : base(pos, "Heart")
        {
            respawnTime = 10f;
        }

        public override float DesplazamientoY => 0.3f;

        public override void Effect(Player1 player1)
        {
            var hitPointsGained =  healingRate * player1.maxHitPoints;
            player1.hitPoints = FastMath.Min(player1.maxHitPoints, player1.hitPoints + hitPointsGained);
        }
    }
}
