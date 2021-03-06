﻿using BulletSharp;
using BulletSharp.Math;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using TGC.Core.BoundingVolumes;
using TGC.Core.Collision;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Shaders;
using TGC.Core.Terrain;
using TGC.Examples.Camara;
using TGC.Group.Model;
using TGC.Group.Model.Items;
using TGC.Group.Model.World;
using TGC.Group.Model.World.Characters;
using TGC.Group.Utils;
using TGC.Group.World.Characters;
using TGC.Group.World.Weapons.Bullets;

namespace TGC.Group.Physics
{
    public abstract class BulletRigidBodyConstructor

    {
        public static RigidBody CreateFloor(TGCVector3 normalVector)
        {
            //El piso es un plano estatico se dice que si tiene masa 0 es estatico.
            var floorShape = new StaticPlaneShape(normalVector.ToBsVector, 0);
            var floorMotionState = new DefaultMotionState();
            var floorInfo = new RigidBodyConstructionInfo(0, floorMotionState, floorShape);
            var floorBody = new RigidBody(floorInfo);
            floorBody.Friction = 0.9f;
            floorBody.RollingFriction = 1;
            // ballBody.SetDamping(0.1f, 0.9f);
            floorBody.Restitution = 1f;
            floorBody.UserObject = "floorBody";

            return floorBody;
        }

        public static RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh)
        {
            var vertexCoords = mesh.getVertexPositions();
            TriangleMesh triangleMesh = new TriangleMesh();
            for (int i = 0; i < vertexCoords.Length; i = i + 3)
            {
                triangleMesh.AddTriangle(vertexCoords[i].ToBsVector, vertexCoords[i + 1].ToBsVector, vertexCoords[i + 2].ToBsVector);
            }

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            //transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var bulletShape = new BvhTriangleMeshShape(triangleMesh, false);
            var boxLocalInertia = bulletShape.CalculateLocalInertia(0);

            var bodyInfo = new RigidBodyConstructionInfo(0, motionState, bulletShape, boxLocalInertia);
            var rigidBody = new RigidBody(bodyInfo);
            rigidBody.Friction = 0.9f;
            rigidBody.RollingFriction = 1;
            // ballBody.SetDamping(0.1f, 0.9f);
            rigidBody.Restitution = 1f;

            return rigidBody;
        }

        public static RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh, TGCVector3 position)
        {
            var meshAxisRadius = mesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var boxShape = new BoxShape(meshAxisRadius);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var boxLocalInertia = boxShape.CalculateLocalInertia(0);

            var qwe = new TriangleMesh();
            var asd = new BvhTriangleMeshShape(qwe, false);
            var bodyInfo = new RigidBodyConstructionInfo(0, motionState, asd, boxLocalInertia);
            var rigidBody = new RigidBody(bodyInfo);
            rigidBody.Friction = 0.4f;
            rigidBody.RollingFriction = 1;
            // ballBody.SetDamping(0.1f, 0.9f);
            rigidBody.Restitution = 1f;

            return rigidBody;
        }

        public static RigidBody CreateRigidBodyFromTgcMesh(TgcMesh mesh, TGCVector3 position, float mass, float friction)
        {
            var meshAxisRadius = mesh.BoundingBox.calculateAxisRadius().ToBsVector;
            var boxShape = new BoxShape(meshAxisRadius);

            var transformationMatrix = TGCMatrix.RotationYawPitchRoll(0, 0, 0).ToBsMatrix;
            transformationMatrix.Origin = position.ToBsVector;
            DefaultMotionState motionState = new DefaultMotionState(transformationMatrix);

            var boxLocalInertia = boxShape.CalculateLocalInertia(mass);

            var bodyInfo = new RigidBodyConstructionInfo(mass, motionState, boxShape, boxLocalInertia);

            var rigidBody = new RigidBody(bodyInfo)
            {
                LinearFactor = TGCVector3.One.ToBsVector,
                Friction = friction,
                RollingFriction = 1f,
                AngularFactor = new Vector3(1f, 0.2f, 1f),
                SpinningFriction = 0.7f
            };

            return rigidBody;
        }

        /// <summary>
        ///  Se crea una caja con una masa (si se quiere que sea estatica la masa debe ser 0),
        ///  con dimensiones x(ancho) ,y(alto) ,z(profundidad), Rotacion de ejes Yaw, Pitch, Roll y un coeficiente de rozamiento.
        /// </summary>
        /// <param name="size">Tamaño de la Cajas</param>
        /// <param name="mass">Masa de la Caja</param>
        /// <param name="position">Posicion de la Caja</param>
        /// <param name="yaw">Rotacion de la Caja respecto del eje x</param>
        /// <param name="pitch">Rotacion de la Caja respecto del eje z</param>
        /// <param name="roll">Rotacion de la Caja respecto del eje y</param>
        /// <param name="friction">Coeficiente de rozamiento de la Caja</param>
        /// <returns>Rigid Body de la caja</returns>
        public static RigidBody CreateBox(TGCVector3 size, float mass, TGCVector3 position, float yaw, float pitch, float roll, float friction)
        {
            var boxShape = new BoxShape(size.X, size.Y, size.Z);
            var boxTransform = TGCMatrix.RotationYawPitchRoll(yaw, pitch, roll).ToBsMatrix;
            boxTransform.Origin = position.ToBsVector;
            DefaultMotionState boxMotionState = new DefaultMotionState(boxTransform);
            //Es importante calcular la inercia caso contrario el objeto no rotara.
            var boxLocalInertia = boxShape.CalculateLocalInertia(mass);
            var boxInfo = new RigidBodyConstructionInfo(mass, boxMotionState, boxShape, boxLocalInertia);
            var boxBody = new RigidBody(boxInfo);
            boxBody.LinearFactor = TGCVector3.One.ToBsVector;
            boxBody.Friction = friction;
            return boxBody;
        }

        /// <summary>
        /// Se crea una capsula a partir de un radio, altura, posicion, masa y si se dedea o no calcular
        /// la inercia. Esto es importante ya que sin inercia no se generan rotaciones que no se
        /// controlen en forma particular.
        /// </summary>
        /// <param name="radius">Radio de la Capsula</param>
        /// <param name="height">Altura de la Capsula</param>
        /// <param name="position">Posicion de la Capsula</param>
        /// <param name="mass">Masa de la Capsula</param>
        /// <param name="needInertia">Booleano para el momento de inercia de la Capsula</param>
        /// <returns>Rigid Body de una Capsula</returns>
        public static RigidBody CreateCapsule(float radius, float height, TGCVector3 position, float mass, bool needInertia)
        {
            //Creamos el shape de la Capsula a partir de un radio y una altura.
            var capsuleShape = new CapsuleShape(radius, height);

            //Armamos las transformaciones que luego formaran parte del cuerpo rigido de la capsula.
            var capsuleTransform = TGCMatrix.Identity;
            capsuleTransform.Origin = position;
            var capsuleMotionState = new DefaultMotionState(capsuleTransform.ToBsMatrix);
            RigidBodyConstructionInfo capsuleRigidBodyInfo;

            //Calculamos o no el momento de inercia dependiendo de que comportamiento
            //queremos que tenga la capsula.
            if (!needInertia)
            {
                capsuleRigidBodyInfo = new RigidBodyConstructionInfo(mass, capsuleMotionState, capsuleShape);
            }
            else
            {
                var capsuleInertia = capsuleShape.CalculateLocalInertia(mass);
                capsuleRigidBodyInfo = new RigidBodyConstructionInfo(mass, capsuleMotionState, capsuleShape, capsuleInertia);
            }

            var localCapsuleRigidBody = new RigidBody(capsuleRigidBodyInfo);
            localCapsuleRigidBody.LinearFactor = TGCVector3.One.ToBsVector;
            //Dado que hay muchos parametros a configurar el RigidBody lo ideal es que
            //cada caso se configure segun lo que se necesite.

            return localCapsuleRigidBody;
        }

        /// <summary>
        ///     Se crea una esfera a partir de un radio, masa y posicion devolviendo el cuerpo rigido de una
        ///     esfera.
        /// </summary>
        /// <param name="radius">Radio de una esfera</param>
        /// <param name="mass">Masa de la esfera</param>
        /// <param name="position">Posicion de la Esfera</param>
        /// <returns>Rigid Body de la Esfera</returns>
        public static RigidBody CreateBall(float radius, float mass, TGCVector3 position)
        {
            //Creamos la forma de la esfera a partir de un radio
            var ballShape = new SphereShape(radius);

            //Armamos las matrices de transformacion de la esfera a partir de la posicion con la que queremos ubicarla
            //y el estado de movimiento de la misma.
            var ballTransform = TGCMatrix.Identity;
            ballTransform.Origin = position;
            var ballMotionState = new DefaultMotionState(ballTransform.ToBsMatrix);

            //Se calcula el momento de inercia de la esfera a partir de la masa.
            var ballLocalInertia = ballShape.CalculateLocalInertia(mass);
            var ballInfo = new RigidBodyConstructionInfo(mass, ballMotionState, ballShape, ballLocalInertia);

            //Creamos el cuerpo rigido de la esfera a partir de la info.
            var ballBody = new RigidBody(ballInfo)
            {
                LinearFactor = TGCVector3.One.ToBsVector
            };
            return ballBody;
        }

        /// <summary>
        ///     Crea una coleccion de triangulos para Bullet a partir de los triangulos generados por un heighmap
        ///     o una coleccion de triangulos a partir de un Custom Vertex Buffer con vertices del tipo Position Texured.
        ///     Se utilizo el codigo de un snippet de Bullet http://www.bulletphysics.org/mediawiki-1.5.8/index.php?title=Code_Snippets
        /// </summary>
        /// <param name="triangleDataVB">Custom Vertex Buffer que puede ser de un Heightmap</param>
        /// <returns>Rigid Body del terreno</returns>
        public static RigidBody CreateSurfaceFromHeighMap(CustomVertex.PositionTextured[] triangleDataVB)
        {
            //Triangulos
            var triangleMesh = new TriangleMesh();
            int i = 0;
            TGCVector3 vector0;
            TGCVector3 vector1;
            TGCVector3 vector2;

            while (i < triangleDataVB.Length)
            {
                var triangle = new Triangle();
                vector0 = new TGCVector3(triangleDataVB[i].X, triangleDataVB[i].Y, triangleDataVB[i].Z);
                vector1 = new TGCVector3(triangleDataVB[i + 1].X, triangleDataVB[i + 1].Y, triangleDataVB[i + 1].Z);
                vector2 = new TGCVector3(triangleDataVB[i + 2].X, triangleDataVB[i + 2].Y, triangleDataVB[i + 2].Z);

                i = i + 3;

                triangleMesh.AddTriangle(vector0.ToBsVector, vector1.ToBsVector, vector2.ToBsVector, false);
            }

            CollisionShape meshCollisionShape = new BvhTriangleMeshShape(triangleMesh, true);
            var meshMotionState = new DefaultMotionState();
            var meshRigidBodyInfo = new RigidBodyConstructionInfo(0, meshMotionState, meshCollisionShape);
            RigidBody meshRigidBody = new RigidBody(meshRigidBodyInfo);

            return meshRigidBody;
        }

        /// <summary>
        ///     Se arma un cilindro a partir de las dimensiones, una posicion y su masa
        /// </summary>
        /// <param name="dimensions">Dimensiones en x,y,z del Cilindro</param>
        /// <param name="position">Posicion del Cilindro</param>
        /// <param name="mass">Masa del Cilindro</param>
        /// <returns>Cuerpo rigido de un Cilindro</returns>
        public static RigidBody CreateCylinder(TGCVector3 dimensions, TGCVector3 position, float mass)
        {
            //Creamos el Shape de un Cilindro
            var cylinderShape = new CylinderShape(dimensions.X, dimensions.Y, dimensions.Z);

            //Armamos la matrix asociada al Cilindro y el estado de movimiento de la misma.
            var cylinderTransform = TGCMatrix.Identity;
            cylinderTransform.Origin = position;
            var cylinderMotionState = new DefaultMotionState(cylinderTransform.ToBsMatrix);

            //Calculamos el momento de inercia
            var cylinderLocalInertia = cylinderShape.CalculateLocalInertia(mass);
            var cylinderInfo = new RigidBodyConstructionInfo(mass, cylinderMotionState, cylinderShape, cylinderLocalInertia);

            //Creamos el cuerpo rigido a partir del de la informacion de cuerpo rigido.
            RigidBody cylinderBody = new RigidBody(cylinderInfo);
            return cylinderBody;
        }
    }

    public abstract class PhysicsGame
    {
        public DiscreteDynamicsWorld world { get; protected set; }
        protected CollisionDispatcher dispatcher;
        protected DefaultCollisionConfiguration collisionConfiguration;
        protected SequentialImpulseConstraintSolver constraintSolver;
        protected BroadphaseInterface broadphase;

        public Player1 player1 { get; protected set; }
        public Enemy enemy { get; protected set; }
        public readonly TGCVector3 initialPosP1 = new TGCVector3(144f, 7.5f, 0f);
        public readonly TGCVector3 initialPosEnemy = new TGCVector3(-192f, 7.5f, 576f); //new TGCVector3(144f, 7.5f, 22f);
        public Scenario escenario { get; protected set; }
        public List<Item> items { get; protected set; }
        public List<Bullet> bullets { get; protected set; }
        protected TgcSkyBox skyBox;
        protected List<Colisionable> objetos = new List<Colisionable>();
        protected List<Explosion> explosions = new List<Explosion>();
        protected Vector3 impactPos = new Vector3(-10, -10, -10);
        protected Effect toonFX, explosionFX;
        //protected List<Enemy> enemies; por ahora solo hay uno

        protected bool inflictDmg = true;
        public float time { get; protected set; }

        public PhysicsGame()
        {
            //Creamos el mundo fisico por defecto.
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            constraintSolver = new SequentialImpulseConstraintSolver();
            //broadphase = new DbvtBroadphase();
            broadphase = new AxisSweep3(new Vector3(-250, -12, -15), new Vector3(250, 70, 600));

            world = new DiscreteDynamicsWorld(dispatcher, broadphase, constraintSolver, collisionConfiguration)
            {
                Gravity = new TGCVector3(0, -9.8f, 0).ToBsVector
            };

            items = new List<Item>();
            bullets = new List<Bullet>();
            //enemies = new List<Enemy>();

            InitializeShadersAndEffects();
        }

        public abstract void Update(GameModel gameModel, TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara);

        public abstract void Render(GameModel gameModel);

        public void Dispose()
        {
            world.Dispose();
            dispatcher.Dispose();
            collisionConfiguration.Dispose();
            constraintSolver.Dispose();
            broadphase.Dispose();

            player1.Dispose();
            escenario.Dispose();
            skyBox.Dispose();
            enemy.Dispose();

            items.ForEach(item => item.Dispose());
            bullets.ForEach(bullet => bullet.Dispose());
            objetos.ForEach(obj => obj.Dispose());
            explosions.ForEach(ex => ex.Dispose());

            explosionFX.Dispose();
            toonFX.Dispose();
        }

        // ------- Métodos Privados -------

        protected void UpdateControlVariables(float elapsedTime)
        {
            time += elapsedTime;
            if (player1.timerMachineGun > 0) player1.timerMachineGun += elapsedTime;
            if (player1.timerMachineGun > player1.FireFrecuencyMachineGun) player1.timerMachineGun = 0;
            if (enemy.timerMachineGun > 0) enemy.timerMachineGun += elapsedTime;
            if (enemy.timerMachineGun > enemy.FireFrecuencyMachineGun) enemy.timerMachineGun = 0;
        }

        protected void AdjustCameraPosition(TgcThirdPersonCamera camaraInterna, ModoCamara modoCamara)
        {
            //if (!Player1.collision)
            //{
            //    currentCameraPosition = camaraInterna.Position;
            //}

            if (camaraInterna.OffsetHeight == 0.1f) return;

            camaraInterna.OffsetHeight = 0.1f;
            camaraInterna.OffsetForward = 30;

            //Pedirle a la camara cual va a ser su proxima posicion
            TGCVector3 position;
            TGCVector3 target;
            camaraInterna.CalculatePositionTarget(out position, out target);

            //Detectar colisiones entre el segmento de recta camara-personaje y todos los objetos del escenario
            TGCVector3 q;
            var minDistSq = FastMath.Pow2(camaraInterna.OffsetForward);
            foreach (var obstaculo in escenario.TgcScene.Meshes)
            {
                if (obstaculo.Name.Equals("Arbusto") || obstaculo.Name.Equals("Pasto") || obstaculo.Name.Equals("Flores"))
                    continue;

                //Hay colision del segmento camara-personaje y el objeto
                if (TgcCollisionUtils.intersectSegmentAABB(target, position, obstaculo.BoundingBox, out q))
                {
                    //Si hay colision, guardar la que tenga menor distancia
                    var distSq = TGCVector3.Subtract(q, target).LengthSq();
                    //Hay dos casos singulares, puede que tengamos mas de una colision hay que quedarse con el menor offset.
                    //Si no dividimos la distancia por 2 se acerca mucho al target.
                    minDistSq = FastMath.Min(distSq / 2, minDistSq);
                }
            }

            //Acercar la camara hasta la minima distancia de colision encontrada (pero ponemos un umbral maximo de cercania)
            var newOffsetForward = FastMath.Sqrt(minDistSq);

            if (FastMath.Abs(newOffsetForward) < 10f)
            {
                newOffsetForward = 10f;
            }
            if (newOffsetForward > modoCamara.ProfundidadCamara())
            {
                newOffsetForward = modoCamara.ProfundidadCamara();
            }
            if (modoCamara.AlturaCamara() > 1)
            {
                camaraInterna.OffsetHeight = 1.1f;
            }
            else
            {
                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
            }

            camaraInterna.OffsetForward = newOffsetForward;

            //Asignar la ViewMatrix haciendo un LookAt desde la posicion final anterior al centro de la camara
            camaraInterna.CalculatePositionTarget(out position, out target);
            camaraInterna.SetCamera(position, target);
        }

        protected void CollisionsHandler(GameModel gameModel)
        {
            // Items collisions
            player1.RigidBody.GetAabb(out Vector3 minP1, out Vector3 maxP1);
            minP1.Y -= player1.meshAxisRadius.Y;
            enemy.RigidBody.GetAabb(out Vector3 minE, out Vector3 maxE);
            minE.Y -= enemy.meshAxisRadius.Y;

            var player1AABB = new TgcBoundingAxisAlignBox(new TGCVector3(minP1), new TGCVector3(maxP1));
            var enemyAABB = new TgcBoundingAxisAlignBox(new TGCVector3(minE), new TGCVector3(maxE));

            // Rotar items, desaparecerlos y hacer efecto si colisionan y contar el tiempo que falta para que vuelvan a aparecer los que no estan
            foreach (Item i in items)
            {
                if (i.IsPresent)
                {
                    i.Update(gameModel.ElapsedTime, time);

                    if (TgcCollisionUtils.testAABBAABB(player1AABB, i.Mesh.BoundingBox))
                    {
                        i.Dissapear(gameModel.DirectSound.DsDevice);
                        i.Effect(player1);
                    }
                    if (TgcCollisionUtils.testAABBAABB(enemyAABB, i.Mesh.BoundingBox))
                    {
                        i.Effect(enemy);
                        i.Dissapear(null);
                    }
                }
                else
                    i.UpdateTimer(gameModel.ElapsedTime);
            }

            // Balas contra personajes

            foreach (Bullet b in bullets)
            {
                b.RigidBody.GetAabb(out Vector3 minB, out Vector3 maxB);
                var meshAxisRadius = b.Mesh.BoundingBox.calculateAxisRadius().ToBsVector;
                minB.Y -= meshAxisRadius.Y;
                var bulletAABB = new TgcBoundingAxisAlignBox(new TGCVector3(minB), new TGCVector3(maxB));

                if (b.shooter == enemy && TgcCollisionUtils.testAABBAABB(player1AABB, bulletAABB))
                {
                    b.DealDamage(player1);
                    world.RemoveRigidBody(b.RigidBody);

                }
                if (b.shooter == player1 && TgcCollisionUtils.testAABBAABB(enemyAABB, bulletAABB))
                {
                    b.DealDamage(enemy);
                    world.RemoveRigidBody(b.RigidBody);
                }

            }

            bullets = ObtainExistingBullets(gameModel);

            // Bullets collisions

            var overlappedPairs = broadphase.OverlappingPairCache.OverlappingPairArray;
            //if (overlappedPairs.Count == 0) return;

            RigidBody obj0, obj1;
            BroadphaseNativeType shapeType;
            List<RigidBody> toRemove = new List<RigidBody>();
            foreach (var pair in overlappedPairs)
            {
                obj0 = (RigidBody)pair.Proxy0.ClientObject;
                obj1 = (RigidBody)pair.Proxy1.ClientObject;

                if (obj0.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape)
                {
                    if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.BoxShape || obj1.Equals(player1.RigidBody)) continue;
                    if (obj1.CollisionShape.ShapeType == BroadphaseNativeType.TriangleMeshShape || obj1.CollisionShape.ShapeType == BroadphaseNativeType.ConvexTriangleMeshShape)
                    {
                        world.ContactTest(obj0, new BulletContactCallback(world, toRemove));
                    }
                    continue;
                }

                shapeType = obj1.CollisionShape.ShapeType;
                if (shapeType == BroadphaseNativeType.BoxShape)
                {
                    if (obj0.Equals(player1.RigidBody)) continue;
                    if (obj0.CollisionShape.ShapeType == BroadphaseNativeType.TriangleMeshShape || obj0.CollisionShape.ShapeType == BroadphaseNativeType.ConvexTriangleMeshShape)
                    {
                        world.ContactTest(obj1, new BulletContactCallback(world, toRemove));
                    }
                    continue;
                }
            }

            toRemove.ForEach(rigid => world.RemoveRigidBody(rigid));

            // Actualizar la lista de balas con aquellas que todavía siguen en el mundo después de las colisiones
            bullets = ObtainExistingBullets(gameModel);

            if (impactPos != new Vector3(-10, -10, -10))
            {
                player1.CalculateImpactDistanceAndReact(impactPos);
                enemy.CalculateImpactDistanceAndReact(impactPos);
                objetos.ForEach(obj => obj.CalculateImpactDistanceAndReact(impactPos));

                impactPos = new Vector3(-10, -10, -10);
            }

            // Limpiar del mundo aquellos objetos que se hayan caido del escenario
            objetos.ForEach(obj =>
            {
                if (obj.RigidBody.CenterOfMassPosition.Y < -15)
                    world.RemoveRigidBody(obj.RigidBody);
            });

            // Actualizar la lista de objetos colisionables que todavía siguen en el mundo
            objetos = ObtainExistingObstacles(gameModel);
        }

        protected List<Bullet> ObtainExistingBullets(GameModel gameModel)
        {
            List<Bullet> bullets2 = new List<Bullet>();
            bullets.ForEach(bullet =>
            {
                bullet.LifeTime += gameModel.ElapsedTime;

                if (bullet.RigidBody.IsInWorld)
                {
                    if (bullet.LifeTime > 10)
                    {
                        world.RemoveRigidBody(bullet.RigidBody);
                        bullet.Dispose();
                    }
                    else bullets2.Add(bullet);
                }
                else
                {
                    if (bullet.Mesh.Name.Equals("power-rocket"))
                    {
                        impactPos = bullet.RigidBody.CenterOfMassPosition;
                        explosions.Add(new Explosion(impactPos, explosionFX));
                    }
                    bullet.Dispose(gameModel.DirectSound.DsDevice);
                }
            });

            return bullets2;
        }

        protected List<Colisionable> ObtainExistingObstacles(GameModel gameModel)
        {
            List<Colisionable> objetos2 = new List<Colisionable>();
            objetos.ForEach(obj =>
            {
                if (!obj.RigidBody.IsInWorld) obj.Dispose();
                else objetos2.Add(obj);
            });

            return objetos2;
        }

        protected void InitializeShadersAndEffects()
        {
            // Toon Shader
            toonFX = TgcShaders.loadEffect(Game.Default.ShadersDirectory + "ToonShading.fx");

            // Explosion Shader
            explosionFX = TgcShaders.loadEffect(Game.Default.ShadersDirectory + "Explosion.fx");
        }
    }

    class BulletContactCallback : ContactResultCallback
    {
        private DynamicsWorld _world;
        private List<RigidBody> _toRemove;

        public BulletContactCallback(DynamicsWorld world, List<RigidBody> toRemove)
        {
            _world = world;
            _toRemove = toRemove;
        }

        public override float AddSingleResult(ManifoldPoint cp, CollisionObjectWrapper colObj0Wrap, int partId0, int index0, CollisionObjectWrapper colObj1Wrap, int partId1, int index1)
        {
            _toRemove.Add((RigidBody)colObj0Wrap.CollisionObject);
            return 0;
        }
    };
}
