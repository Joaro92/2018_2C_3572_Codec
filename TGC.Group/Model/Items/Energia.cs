using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Mathematica;
using TGC.Group.Model.World;

namespace TGC.Group.Model.Items
{
    public class Energia : Item
    {
        private readonly float gainRate = 0.5f;

        public Energia(TGCVector3 pos) : base(pos)
        {
            respawnTime = 5f;
            this.spawn();
        }

        public override void Effect(Player1 player1)
        {
            var specialPointsGained = gainRate * player1.maxSpecialPoints;
            player1.specialPoints = FastMath.Min(player1.maxSpecialPoints, player1.specialPoints + specialPointsGained);
        }

        protected override void spawn()
        {
            Mesh = ItemCreator.SpawnItem("Energy", Position);
            base.spawn();
        }
    }
}
