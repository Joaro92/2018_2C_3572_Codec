using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Mathematica;
using TGC.Group.Model.Vehicles;
using TGC.Group.Model.World.Characters;
using TGC.Group.Physics;
using TGC.Group.Utils;
using Button = TGC.Group.Model.Input.Button;
using Dpad = TGC.Group.Model.Input.Dpad;

namespace TGC.Group.Model.World
{
    public class Player1 : Character
    {
        public Player1(DiscreteDynamicsWorld world, Vehiculo vehiculo, TGCVector3 position, float orientation, GameModel gameModel) : base(world, vehiculo, position, orientation, gameModel)
        {

        }


       

        public void ReactToInputs(GameModel gameModel, PhysicsGame nivel)
        {
            var Input = gameModel.Input;

            var moving = false;
            var rotating = false;

            var rightStick = Input.JoystickLeftStick();
            float grades = 0;
            if (FastMath.Abs(rightStick) > 1800)
            {
                grades = ((FastMath.Abs(rightStick) - 1800f) / 30000f) * (FastMath.Abs(rightStick) / rightStick);
            }

            // Adelante
            if (Input.keyDown(Key.W) || Input.keyDown(Key.UpArrow) || Input.buttonDown(Button.X))
            {
                Accelerate();
                moving = true;
            }

            // Atras
            if (Input.keyDown(Key.S) || Input.keyDown(Key.DownArrow) || Input.buttonDown(Button.TRIANGLE))
            {
                Reverse();
                moving = true;
            }

            if (grades != 0)
            {
                vehicle.SetSteeringValue(steeringAngle * grades, 2);
                vehicle.SetSteeringValue(steeringAngle * grades, 3);
                rotating = true;
            }

            // Derecha
            if (Input.keyDown(Key.D) || Input.keyDown(Key.RightArrow) || Input.buttonDown(Dpad.RIGHT))
            {
                TurnRight();
                rotating = true;
            }

            // Izquierda
            if (Input.keyDown(Key.A) || Input.keyDown(Key.LeftArrow) || Input.buttonDown(Dpad.LEFT))
            {
                TurnLeft();
                rotating = true;
            }

            if (!rotating)
            {
                ResetSteering();
            }
            if (!moving)
            {
                ResetEngineForce();
            }

          

            // Turbo
            if (specialPoints >= costTurbo && (Input.keyDown(Key.LeftShift) || Input.JoystickButtonPressedDouble(0, gameModel.ElapsedTime)))
            {
                TurboOn();
                if (!turboSound.SoundBuffer.Status.Playing)
                {
                    turboSound.play(true); // 26.532
                }
                if (turboSound.SoundBuffer.PlayPosition > 25208)
                {
                    turboSound.SoundBuffer.SetCurrentPosition(18036);
                }
            }
            else
            {
                TurboOff();
                turboSound.stop();
                turboSound.SoundBuffer.SetCurrentPosition(0);
            }

            // Frenar
            if (Input.keyDown(Key.LeftControl) || Input.buttonDown(Button.SQUARE))
            {
                Brake();
            }
            else
            {
                ResetBrake();
            }

            // Chequea y actualiza el status del Salto
            CheckJumpStatus(gameModel);

            // Saltar
            if (Input.keyPressed(Key.Space) || Input.buttonPressed(Button.CIRCLE))
            {
                if (specialPoints > 12 && canJump && onTheFloor)
                {
                    rigidBody.ApplyCentralImpulse(new Vector3(0, jumpImpulse, 0));
                    specialPoints -= 12;
                    canJump = false;
                    onTheFloor = false;
                }
            }

            // Cambiar de arma especial
            if (Input.keyPressed(Key.Tab) || Input.buttonPressed(Button.R1))
            {
                if (Weapons.Count != 0)
                {
                    var arrayWeapons = Weapons.ToArray();
                    SelectedWeapon = arrayWeapons.getNextOption(SelectedWeapon);
                }
            }

            // Disparar Machinegun
            if (Input.keyDown(Key.E) || Input.buttonDown(Button.R2))
            {
                FireMachinegun(gameModel, nivel);
            }

            // Disparar arma especial
            if (Input.keyPressed(Key.R) || Input.buttonPressed(Button.L2))
            {
                FireWeapon(gameModel, nivel, SelectedWeapon);
            }
        }
    }
}
