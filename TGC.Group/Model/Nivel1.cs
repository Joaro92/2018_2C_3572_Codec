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
        //private Bullet_TGC floor;
        //private Bullet_TGC obj;
        private Escenario escenario;
        private Player1 player1;
        private bool jumped = false;
        private bool flag = false;

        public override Player1 Init()
        {
            base.Init();

            // Agregamos un piso
            //floor = new Bullet_TGC("Texturas\\granito.jpg", new TGCVector3(-2000, 0, -2000), new TGCVector3(4000, 0, 4000), TgcPlane.Orientations.XZplane);
            //world.AddRigidBody(floor.rigidBody);

            //obj = new Bullet_TGC("Escenarios\\columna-TgcScene.xml", new TGCVector3(-5, 19.23f, -40));
            //obj.tgcMesh.AutoTransform = false;
            //world.AddRigidBody(obj.rigidBody);

            //var radio = obj.tgcMesh.BoundingBox.calculateAxisRadius();
            //var pmin = obj.tgcMesh.BoundingBox.PMin;

            //obj.tgcMesh.Transform = new TGCMatrix(obj.rigidBody.InterpolationWorldTransform);
            //obj.tgcMesh.Transform = obj.tgcMesh.Transform * TGCMatrix.Translation(-(radio + pmin));

            // Agregamos el escenario
            escenario = new Escenario("Escenarios\\escenario-objetos-TgcScene.xml");
            foreach(RigidBody rigid in escenario.rigidBodys)
            {
                world.AddRigidBody(rigid);
            }

            // Agregamos a nuestro jugador
            player1 = new Player1("Vehicles\\centered car-minibus-TgcScene.xml", new TGCVector3(144, 30, 0), 0.2f, 0.6f);
            player1.tgcMesh.AutoTransform = false;
            world.AddRigidBody(player1.rigidBody);

            return player1;
        }

        public override Player1 Update(TgcD3dInput Input)
        {
            world.StepSimulation(1 / 60f, 10);
            player1.rigidBody.Activate();

            // Atributos Player 1
            var moveSpeed = 1f;
            var rotationSpeed = 1.25f;

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
            }

            //Saltar
            if (Input.keyDown(Key.Space))
            {
                jump = true;
            }

            // Calcular el vector Orientacion (Hacia adonde está apuntando) y multiplicarlo por la velocidad de movimiento
            var moveVector = Quat.rotate_vector_by_quaternion(new TGCVector3(0, 0, -1), player1.rigidBody.Orientation).ToBsVector * moveForward;
            
            // Aplicar las fuerzas necesarias al Cuerpo Rigido del Player 1 para lograr el movimiento
            if (moving)
            {
                player1.rigidBody.ApplyCentralForce(moveVector * 4);
            }

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

            return player1;
        }

        public override void Render()
        {
            player1.tgcMesh.Transform = new TGCMatrix(player1.rigidBody.InterpolationWorldTransform);
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
