using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Core.Collision;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Group.Bullet.Physics;
using TGC.Group.Bullet_TGC_Object;
using TGC.Group.PlayerOne;
using TGC.Group.TGCEscenario;
using TGC.Examples.Camara;

namespace TGC.Group.Nivel1
{
    public class NivelUno : PhysicsGame
    {
        private Escenario escenario;
        private Player1 player1;
        private bool showBoundingBox = false;
        private List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private bool jumped = false;
        private bool flag = false;
        private TGCMatrix wheelTransform;

        public override Player1 Init()
        {
            base.Init();

            // Cargamos el escenario y lo agregamos al mundo
            escenario = new Escenario("Escenarios\\escenario-objetos-TgcScene.xml");
            foreach(RigidBody rigid in escenario.rigidBodys)
            {
                world.AddRigidBody(rigid);
            }

            // Creamos a nuestro jugador y lo agregamos al mundo
            player1 = new Player1(world, "vehicles\\chassis-minibus-TgcScene.xml", "vehicles\\tires-minibus-TgcScene.xml", new TGCVector3(144, 20, 0));
            player1.tgcMesh.AutoTransform = false;
            player1.Wheel.AutoTransform = false;

            return player1;
        }

        public override Player1 Update(TgcD3dInput Input, TgcThirdPersonCamera camaraInterna)
        {
            world.StepSimulation(1 / 60f, 10);

            if (player1.rigidBody.CenterOfMassPosition.Y < -100)
            {
                var transformationMatrix = TGCMatrix.RotationYawPitchRoll(FastMath.PI, 0, 0).ToBsMatrix;
                transformationMatrix.Origin = new Vector3(144, 20, 0);

                player1.rigidBody.MotionState = new DefaultMotionState(transformationMatrix);
                player1.rigidBody.LinearVelocity = Vector3.Zero;
                player1.rigidBody.AngularVelocity = Vector3.Zero;
            }

            // Atributos Player 1
            var engineForce = -700f;
            var steeringAngle = -0.27f;

            // Detectar según el Input, si va a Rotar, Avanzar y/o Saltar
            var moving = false;
            var rotating = false;
            var jump = false;

            if (Input.keyPressed(Key.F1))
            {
                showBoundingBox = !showBoundingBox;
            }

            // Adelante
            if (Input.keyDown(Key.W))
            {
                player1.Vehicle.ApplyEngineForce(engineForce, 2);
                player1.Vehicle.ApplyEngineForce(engineForce, 3);
                moving = true;
            }

            // Atras
            if (Input.keyDown(Key.S))
            {
                player1.Vehicle.ApplyEngineForce(-engineForce * 0.15f, 0);
                player1.Vehicle.ApplyEngineForce(-engineForce * 0.15f, 1);
                player1.Vehicle.ApplyEngineForce(-engineForce * 0.75f, 2);
                player1.Vehicle.ApplyEngineForce(-engineForce * 0.75f, 3);
                moving = true;
            }

            // Derecha
            if (Input.keyDown(Key.D))
            {
                player1.Vehicle.SetSteeringValue(steeringAngle, 2);
                player1.Vehicle.SetSteeringValue(steeringAngle, 3);
                rotating = true;
            }

            // Izquierda
            if (Input.keyDown(Key.A))
            {
                player1.Vehicle.SetSteeringValue(-steeringAngle, 2);
                player1.Vehicle.SetSteeringValue(-steeringAngle, 3);
                rotating = true;
            }

            // Saltar
            if (Input.keyDown(Key.Space))
            {
                jump = true;
            }

            // Si no se presionó ninguna tecla
            if (!rotating)
            {
                player1.Vehicle.SetSteeringValue(0, 2);
                player1.Vehicle.SetSteeringValue(0, 3);
            }

            if (!moving)
            {
                player1.Vehicle.ApplyEngineForce(0, 0);
                player1.Vehicle.ApplyEngineForce(0, 1);
                player1.Vehicle.ApplyEngineForce(0, 2);
                player1.Vehicle.ApplyEngineForce(0, 3);

                //Default braking force, always added otherwise there is no friction on the wheels
                player1.Vehicle.SetBrake(0.2f, 0);
                player1.Vehicle.SetBrake(0.2f, 1);
                player1.Vehicle.SetBrake(0.2f, 2);
                player1.Vehicle.SetBrake(0.2f, 3);
            }

            // Frenar
            if (Input.keyDown(Key.LeftControl))
            {
                player1.Vehicle.SetBrake(27, 2);
                player1.Vehicle.SetBrake(27, 3);
            }

            if (jump && !jumped && !flag)
            {
                player1.rigidBody.ApplyCentralImpulse(new Vector3(0, 2500, 0));
                jumped = true;
            }

            if (jumped && player1.rigidBody.LinearVelocity.Y < -0.1f)
            {
                flag = true;
                jumped = false;
            }

            if (player1.rigidBody.LinearVelocity.Y > -0.05f)
            {
                flag = false;
            }

            // Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            objectsBehind.Clear();
            objectsInFront.Clear();
            foreach (var mesh in escenario.tgcScene.Meshes)
            {
                TGCVector3 q;
                if (TgcCollisionUtils.intersectSegmentAABB(camaraInterna.Position, camaraInterna.Target, mesh.BoundingBox, out q))
                {
                    objectsBehind.Add(mesh);
                }
                else
                {
                    objectsInFront.Add(mesh);
                }
            }

            return player1;
        }

        public TGCMatrix convertToLHMatrix(Matrix matriz)
        {
            var matrizTGC = Matrix.Identity;
            //matriz.Transpose();
            matrizTGC.M11 = matriz.M11;
            matrizTGC.M12 = matriz.M13;
            matrizTGC.M13 = matriz.M12;
            matrizTGC.M14 = matriz.M14;

            matrizTGC.M21 = matriz.M21;
            matrizTGC.M22 = matriz.M23;
            matrizTGC.M23 = matriz.M22;
            matrizTGC.M24 = matriz.M34;

            matrizTGC.M31 = matriz.M31;
            matrizTGC.M32 = matriz.M33;
            matrizTGC.M33 = matriz.M32;
            matrizTGC.M34 = matriz.M24;

            matrizTGC.M41 = matriz.M41;
            matrizTGC.M42 = matriz.M42;
            matrizTGC.M43 = matriz.M43;

            return new TGCMatrix(matrizTGC);
        }

        public override void Render()
        { 
            player1.tgcMesh.Transform = new TGCMatrix(player1.Vehicle.ChassisWorldTransform);
            player1.tgcMesh.Render();

            wheelTransform = TGCMatrix.RotationY(player1.Vehicle.GetSteeringValue(0)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(0).WorldTransform.Origin));
            player1.Wheel.Transform = wheelTransform;
            player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(player1.Vehicle.GetSteeringValue(1) + FastMath.PI) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(1).WorldTransform.Origin));
            player1.Wheel.Transform = wheelTransform;
            player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-player1.Vehicle.GetSteeringValue(2)) * TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(2).WorldTransform.Origin));
            player1.Wheel.Transform = wheelTransform;
            player1.Wheel.Render();

            wheelTransform = TGCMatrix.RotationY(-player1.Vehicle.GetSteeringValue(3) + FastMath.PI) *  TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(3).WorldTransform.Origin));
            player1.Wheel.Transform = wheelTransform;
            player1.Wheel.Render();

            // Render mallas que no se interponen
            foreach (var mesh in objectsInFront)
            {
                mesh.Render();
                if (showBoundingBox)
                {
                    mesh.BoundingBox.Render();
                }
            }

            // Para las mallas que se interponen a la cámara, solo renderizar su BoundingBox (Si hago esto los bloques que no son solidos desaparecen cuando los atravieso)
            // REVISAR
            foreach (var mesh in objectsBehind)
            {
                mesh.Render();
                if (showBoundingBox)
                {
                    mesh.BoundingBox.Render();
                }
            }
        }

        public override void Dispose()
        {
            world.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            broadphase.Dispose();
            player1.tgcMesh.Dispose();
            player1.rigidBody.Dispose();
            escenario.Dispose();
        }
    }
}
