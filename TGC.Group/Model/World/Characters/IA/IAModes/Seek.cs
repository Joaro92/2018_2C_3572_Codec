using TGC.Core.Mathematica;
using TGC.Group.Model;
using TGC.Group.Physics;
using TGC.Group.Utils;

namespace TGC.Group.World.Characters.ArtificialIntelligence
{
    public abstract class Seek : IMode
    {
        protected PhysicsGame nivel;
    
        public Seek(PhysicsGame nivel)
        {
            this.nivel = nivel;
        }

        public abstract void Do(IA ia);

        public float Distance(TGCVector3 target)
        {
            var e = nivel.enemy;
            var myPos = new TGCVector3(e.RigidBody.CenterOfMassPosition);
            var chaseVector = target - myPos;
            return TGCVector3.Length(chaseVector);
        }

        public bool DoSearch(TGCVector3 target)
        {
            var e = nivel.enemy;
            var myPos = new TGCVector3(e.RigidBody.CenterOfMassPosition);

            var deltaX = target.X - myPos.X;
            var deltaZ = target.Z - myPos.Z;

            var myRot = e.yawPitchRoll.Y;
            var rotP1 = nivel.player1.yawPitchRoll.Y;
            var myOr = UtilMethods.GetOrientation(myRot);
            var orP1 = UtilMethods.GetOrientation(rotP1);

            //buscar target (chequeo el cuadrante del rival respecto a mi)
            if (deltaX > 0f && deltaZ < 0f) //posiciones iniciales
            {
                if (myOr == Orientation.SUR || myOr == Orientation.SUROESTE || myOr == Orientation.OESTE)
                {
                    e.TurnLeft();
                }
                else if (myOr == Orientation.NORTE || myOr == Orientation.NOROESTE)
                {
                    e.TurnRight();
                }
                else if (myOr == Orientation.ESTE || myOr == Orientation.NORESTE)
                {
                    e.TurnRight();
                }
                else // myOr == Orientation.SURESTE
                {
                    return true;
                }
            }
            else if (deltaX > 0f && deltaZ >= 0f)
            {
                if (myOr == Orientation.SUR || myOr == Orientation.SUROESTE)
                {
                    e.TurnLeft();
                }
                else if (myOr == Orientation.NORTE || myOr == Orientation.NOROESTE || myOr == Orientation.OESTE)
                {
                    e.TurnRight();
                }
                else if (myOr == Orientation.ESTE || myOr == Orientation.SURESTE)
                {
                    e.TurnLeft();
                }
                else // myOr == Orientation.NORESTE
                {
                    return true;
                }
            }
            else if (deltaX <= 0f && deltaZ < 0f)
            {
                if (myOr == Orientation.SUR || myOr == Orientation.SURESTE || myOr == Orientation.ESTE)
                {
                    e.TurnRight();
                }
                else if (myOr == Orientation.NORTE || myOr == Orientation.NOROESTE)
                {
                    e.TurnLeft();
                }
                else if (myOr == Orientation.OESTE || myOr == Orientation.NORESTE)
                {
                    e.TurnLeft();
                }
                else // myOr == Orientation.SUROESTE
                {
                    return true;
                }
            }
            else if (deltaX <= 0f && deltaZ >= 0f)
            {
                if (myOr == Orientation.SUR || myOr == Orientation.SUROESTE || myOr == Orientation.OESTE)
                {
                    e.TurnRight();
                }
                else if (myOr == Orientation.NORTE || myOr == Orientation.NORESTE)
                {
                    e.TurnLeft();
                }
                else if (myOr == Orientation.ESTE || myOr == Orientation.SURESTE)
                {
                    e.TurnLeft();
                }
                else // myOr == Orientation.NOROESTE
                {
                    return true;
                }
            }
            return false;
        }
    }
}