using TGC.Core.Mathematica;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Energia : Item
    {
        private readonly float gainRate = 0.5f;

        public Energia(TGCVector3 pos) : base(pos,"Energy")
        {
            Name = "Energy";
            respawnTime = 5f;
        }

        public override void Effect(Player1 player1)
        {
            var specialPointsGained = gainRate * player1.maxSpecialPoints;
            player1.specialPoints = FastMath.Min(player1.maxSpecialPoints, player1.specialPoints + specialPointsGained);
        }
    }
}
