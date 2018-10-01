using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Group.Bullet.Physics;
using TGC.Group.Bullet_TGC_Object;
using TGC.Group.PlayerOne;
using TGC.Group.TGCEscenario;

namespace TGC.Group.Nivel1
{
    public class NivelUno : PhysicsGame
    {
        private Escenario escenario;
        private Player1 player1;
        private bool jumped = false;
        private bool flag = false;
        private TGCMatrix wheelTransform;
        private float fuerza;
        private float frame = 1f / 60f;
        private float frameDelta;

        public override Player1 Init()
        {
            base.Init();

            // Agregamos el escenario
            escenario = new Escenario("Escenarios\\escenario-objetos-TgcScene.xml");
            foreach(RigidBody rigid in escenario.rigidBodys)
            {
                world.AddRigidBody(rigid);
            }

            // Agregamos a nuestro jugador
            //player1 = new player1("vehicles\\centered car-minibus-tgcscene.xml", new tgcvector3(144, 30, 0), 0.2f, 0.6f);
            //player1.tgcmesh.autotransform = false;
            //world.addrigidbody(player1.rigidbody);

            player1 = new Player1(world, "vehicles\\chassis-minibus-TgcScene.xml", "vehicles\\tires-minibus-TgcScene.xml", new TGCVector3(144, 20, 0));
            player1.tgcMesh.AutoTransform = false;
            player1.Wheel.AutoTransform = false;

            return player1;
        }

        public override Player1 Update(TgcD3dInput Input)
        {
            world.StepSimulation(1 / 60f, 10);

            // Atributos Player 1
            var moveSpeed = 1f;
            var rotationSpeed = 1.25f;
            var engineForce = -80f;
            var steeringAngle = -0.3f;

            // Detectar según el Input, si va a Rotar, Avanzar y/o Saltar
            var moveForward = 0f;
            var rotate = 0f;
            var moving = false;
            var rotating = false;
            var jump = false;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = moveSpeed;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = -moveSpeed;
                moving = true;
                engineForce = -(engineForce * 0.6f);
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = rotationSpeed;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -rotationSpeed;
                rotating = true;
                steeringAngle = -steeringAngle;
            }

            //Saltar
            if (Input.keyDown(Key.Space))
            {
                jump = true;
            }

            // Calcular el vector Orientacion (Hacia adonde está apuntando) y multiplicarlo por la velocidad de movimiento
            var moveVector = Quat.rotate_vector_by_quaternion(new TGCVector3(0, 0, -1), player1.rigidBody.Orientation).ToBsVector * moveForward;

            //Key pressed events
 
            if (rotating)
            {
                player1.Vehicle.SetSteeringValue(steeringAngle, 2);
                player1.Vehicle.SetSteeringValue(steeringAngle, 3);
            }

            if (moving)
            {
                player1.Vehicle.ApplyEngineForce(engineForce, 2);
                player1.Vehicle.ApplyEngineForce(engineForce, 3);
            }

            //Handbrake
            //if (key == B3G_CONTROL)
            //{
            //    this->vehicle->setBrake(500, 2);
            //    this->vehicle->setBrake(500, 3);
            //    handled = true;
            //}

            //Key released events

            if (!rotating)
            {
                player1.Vehicle.SetSteeringValue(0, 2);
                player1.Vehicle.SetSteeringValue(0, 3);
            }

            if (!moving)
            {
                player1.Vehicle.ApplyEngineForce(0, 2);
                player1.Vehicle.ApplyEngineForce(0, 3);

                //Default braking force, always added otherwise there is no friction on the wheels
                player1.Vehicle.SetBrake(0.2f, 2);
                player1.Vehicle.SetBrake(0.2f, 3);
                player1.Vehicle.SetBrake(0.2f, 0);
                player1.Vehicle.SetBrake(0.2f, 1);
            }

            //if (key == B3G_CONTROL)
            //{
            //    this->vehicle->setBrake(0, 2);
            //    this->vehicle->setBrake(0, 3);
            //    handled = true;
            //}


            /*
            // Aplicar las fuerzas necesarias al Cuerpo Rigido del Player 1 para lograr el movimiento
            if (moving)
            {
                fuerza = -1000f;
                frameDelta = 0f;
                //player1.rigidBody.ApplyCentralForce(moveVector * 4);
            }
            else
            {
                if (frameDelta < 1f)
                {
                    frameDelta += frame;
                }
            }

            fuerza *= (1.0f - frameDelta);

            player1.Vehicle.ApplyEngineForce(fuerza, 2);
            player1.Vehicle.ApplyEngineForce(fuerza, 3);

            if (rotating)
            {
                player1.rigidBody.ApplyTorque(new Vector3(0, rotate, 0));
            }

            if (jump && !jumped && !flag)
            {
                player1.rigidBody.ApplyCentralForce(new Vector3(0, 220, 0));
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

            */
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

            //var wheelTransform2 = convertToLHMatrix(player1.Vehicle.GetWheelInfo(0).WorldTransform);

            //wheelTransform2 = new TGCMatrix(player1.Vehicle.GetWheelTransformWS(0));

            var wheelTransform2 = TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.RotationY(player1.Vehicle.GetSteeringValue(0)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(0).WorldTransform.Origin));
            player1.Wheel.Transform = wheelTransform2;
            player1.Wheel.Render();

            wheelTransform2 = TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.RotationY(player1.Vehicle.GetSteeringValue(1) + FastMath.PI) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(1).WorldTransform.Origin));

            //wheelTransform = TGCMatrix.RotationYawPitchRoll(Quat.ToEulerAngles(player1.rigidBody.Orientation).Y + player1.Vehicle.GetWheelInfo(2).Rotation *2, 0, 0) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(2).WorldTransform.Origin));

            player1.Wheel.Transform = wheelTransform2;
            player1.Wheel.Render();

            wheelTransform2 = TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.RotationY(-player1.Vehicle.GetSteeringValue(2)) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(2).WorldTransform.Origin));
          
            //wheelTransform = TGCMatrix.RotationYawPitchRoll(Quat.ToEulerAngles(player1.rigidBody.Orientation).Y + FastMath.PI, 0, 0) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(1).WorldTransform.Origin));

            player1.Wheel.Transform = wheelTransform2;
            player1.Wheel.Render();

            wheelTransform2 = TGCMatrix.RotationTGCQuaternion(new TGCQuaternion(player1.rigidBody.Orientation.X, player1.rigidBody.Orientation.Y, player1.rigidBody.Orientation.Z, player1.rigidBody.Orientation.W)) * TGCMatrix.RotationY(-player1.Vehicle.GetSteeringValue(3) + FastMath.PI) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(3).WorldTransform.Origin));

            // wheelTransform = TGCMatrix.RotationYawPitchRoll(Quat.ToEulerAngles(player1.rigidBody.Orientation).Y, 0, 0) * TGCMatrix.Translation(new TGCVector3(player1.Vehicle.GetWheelInfo(0).WorldTransform.Origin));

            player1.Wheel.Transform = wheelTransform2;
            player1.Wheel.Render();


            player1.tgcMesh.Render();
            escenario.tgcScene.RenderAll();

            foreach (var mesh in escenario.tgcScene.Meshes)
            {
                    mesh.BoundingBox.Render();
            }
            //obj.tgcMesh.Render();
            //floor.tgcMesh.Render();
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
            //floor.rigidBody.Dispose();
            //floor.tgcMesh.Dispose();
        }
    }
}
