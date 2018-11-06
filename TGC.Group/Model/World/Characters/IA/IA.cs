using System;
using System.Collections.Generic;
using TGC.Core.Mathematica;
using TGC.Group.Model;
using Action = TGC.Group.Model.World.Characters.IA.Action;
using TGC.Group.Physics;

namespace TGC.Group.World.Characters.ArtificialIntelligence
{
    public class IA
    {

        //acciones configurables por X tiempo que sobrescriben el currentMode
        private List<Action> currentRoutine = new List<Action>(); // rutina (lista de acciones con su tiempo) que se lleva a cabo secuencialmente 

        //modo de accionar (si no esta en rutina)
        public IMode currentMode { get; set; }

        //parametros configurables
        private readonly int minSpeed = 10;
        private readonly int maxSpeed = 30;
        private readonly float specialWeaponShootFrec = 1f;

        //parametros de control
        public bool ShootMachineGun { get; set; } = false;
        public bool ShootSpecialWeapon { get; set; } = false;
        private float timerToShootSpecial = 0f;
        //private bool hayObstaculo = false;
        private int alternate = 0;
        public bool justAppeared { get; set; } = true;

        public void TakeAction(Enemy e, GameModel gameModel, PhysicsGame nivel)
        {
            //seteo el modo por defecto
            if (currentMode == null)
            {
                //currentMode = new SeekPlayer(nivel);
                currentMode = new SeekWeapon(nivel); //power weapon en base enemiga
            }

            //ejecuto la rutina actual si existiera
            if (currentRoutine.Count > 0)
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

            //movimientos regulares
            //rutina para esquivar obstaculos
            if (sp == 0 && !justAppeared) //si no tengo velocidad (salvo si recien aparece) alterno entre 3 soluciones (deberia ser con hayObstaculo)
            {
                currentRoutine.Add(new Action(() => { e.Reverse(); }, 0.5f));
                if (alternate == 0)
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); }, 1f));
                    currentRoutine.Add(new Action(() => { e.Jump(); }, 0.5f));
                    alternate = 1;
                }
                else if (alternate == 1)
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); e.TurnRight(); }, 0.5f));
                    alternate = 2;
                }
                else
                {
                    currentRoutine.Add(new Action(() => { e.Accelerate(); e.TurnLeft(); }, 0.5f));
                    alternate = 0;
                }
            }
            //acelerar o frenar para mantener una velocidad entre dos valores
            if (sp < minSpeed)
            {
                e.Accelerate();
                justAppeared = false;
            }

            if (sp > maxSpeed)
            {
                e.Brake();
            }

            currentMode.Do(this); // hago lo correspondiente al modo actual

            //disparar armas si corresponde
            if (ShootMachineGun)
                e.FireMachinegun(gameModel, nivel);
            if (ShootSpecialWeapon)
            {
                if (timerToShootSpecial >= specialWeaponShootFrec)
                {
                    e.FireWeapon(gameModel, nivel, e.SelectedWeapon);
                    timerToShootSpecial = 0f;
                }
                else
                {
                    timerToShootSpecial += gameModel.ElapsedTime;
                }
            }
        }
    }
}