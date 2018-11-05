using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using Action = TGC.Group.Model.World.Characters.IA.Action;
using TGC.Group.Physics;

namespace TGC.Group.World
{
    public class IA
    {
        //acciones configurables por X tiempo
        private  List<Action> currentRoutine = new List<Action>(); // rutina (lista de acciones con su tiempo) que se lleva a cabo secuencialmente 

        //parametros configurables
        private float shootForXSeconds = 5f; //dispara X seg. si, X seg. no
        private int minSpeed = 10;
        private int maxSpeed = 30;

        //parametros de control
        private float timerToShoot = 0f;
        //private bool hayObstaculo = false;
        private int alternate = 0;

        public enum Orientation { SUR, ESTE, NORTE, OESTE, SURESTE, SUROESTE, NORESTE, NOROESTE };

        public void TakeAction(Enemy e, GameModel gameModel, PhysicsGame nivel)
        {
            //reseteo todo
            e.ResetBrake();
            e.ResetSteering();
            e.ResetEngineForce();

            if(currentRoutine.Count > 0)
            {
                var currentAction = currentRoutine.Find(a => a.t > 0f);
                if (currentAction != null)
                {
                    currentAction.proc.Invoke();
                    currentAction.t -= gameModel.ElapsedTime;
                }
                else
                    currentRoutine = new List<Action>();
                return;
            }

            //variables
            var sp = e.currentSpeed;
            var t = nivel.time;

            var myPos = new TGCVector3(e.RigidBody.CenterOfMassPosition);
            var posP1 = new TGCVector3(nivel.player1.RigidBody.CenterOfMassPosition);

            var deltaX = posP1.X - myPos.X;
            var deltaZ = posP1.Z - myPos.Z;

            var myRot = e.yawPitchRoll.Y;
            var rotP1 = nivel.player1.yawPitchRoll.Y;
            var myOr = GetOrientation(myRot);
            var orP1 = GetOrientation(rotP1);

            var chaseVector = posP1 - myPos;
            var dist = TGCVector3.Length(chaseVector);
            // no se usa pero se puede dejar de perseguirlo si ya estoy cerca, o si esta muy lejos hacer algo distinto como buscar items


            if (sp == 0 && t > 3f) //si no puedo avanzar (salvo al inicio) alterno entre 3 soluciones (deberia ser con hayObstaculo)
            {
                currentRoutine.Add(new Action(() => { e.Reverse(); }, 0.5f));
                if (alternate == 0)
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); e.TurnLeft(); }, 0.25f));
                    alternate = 1;
                }
                else if (alternate == 1)
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); e.TurnRight(); }, 0.25f));
                    alternate = 2;
                }
                else
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); }, 1f));
                    currentRoutine.Add(new Action(() => { e.Jump(); }, 0.5f));
                    alternate = 0;
                }
            }
            //acelerar o frenar para mantener una velocidad entre dos valores
            if (sp < minSpeed)
            {
                e.Accelerate();
            }

            if (sp > maxSpeed)
            {
                e.Brake();
            }

            //buscar player1 (chequeo el cuadrante del rival respecto a mi)
            if (deltaX > 0f && deltaZ  < 0f) //posiciones iniciales
            {
                if(myOr == Orientation.SUR || myOr == Orientation.SUROESTE || myOr == Orientation.OESTE)
                {
                    e.TurnLeft();
                }
                else if (myOr == Orientation.NORTE || myOr == Orientation.NOROESTE)
                {
                    e.TurnRight();
                }
                else if(myOr == Orientation.ESTE || myOr == Orientation.NORESTE)
                {
                    e.TurnRight();
                }
                else // myOr == Orientation.SURESTE
                {
                    //no hago nada 
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
                    //no hago nada

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
                    //no hago nada 
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
                    //no hago nada
                }
            }

            //disparar
            if (timerToShoot >= shootForXSeconds)
            {
                e.FireMachinegun(gameModel, nivel);
            }
            if (timerToShoot >= shootForXSeconds * 2)
            {
                timerToShoot = 0f;
            }

            timerToShoot += gameModel.ElapsedTime;
        }

        public static Orientation GetOrientation(float rotation)
        {
            var roundSin = Math.Round(FastMath.Sin(rotation));
            var roundCos = Math.Round(FastMath.Cos(rotation));
            if (roundSin == 0 && roundCos == 1)
            {
                return Orientation.SUR;
            }
            if (roundSin == 0 && roundCos == -1)
            {
                return Orientation.NORTE;
            }
            if (roundSin == 1 && roundCos == 0)
            {
                return Orientation.OESTE;
            }
            if (roundSin == -1 && roundCos == 0)
            {
                return Orientation.ESTE;
            }
            if (roundSin == -1 && roundCos == -1)
            {
                return Orientation.NORESTE;
            }
            if (roundSin == 1 && roundCos == -1)
            {
                return Orientation.NOROESTE;
            }
            if (roundSin == -1 && roundCos == 1)
            {
                return Orientation.SURESTE;
            }
            if (roundSin == 1 && roundCos == 1)
            {
                return Orientation.SUROESTE;
            }
            return Orientation.SUR;
        }

    }
}