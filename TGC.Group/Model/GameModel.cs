using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Collections.Generic;
using TGC.Examples.Camara;
using TGC.Examples.Collision.SphereCollision;
using TGC.Examples.Example;
using TGC.Examples.UserControls;
using TGC.Examples.UserControls.Modifier;
using System.Drawing;
using TGC.Core.BoundingVolumes;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.SkeletalAnimation;
using TGC.Core.Terrain;
using TGC.Core.Textures;
using TGC.Core.Collision;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        private bool showBoundingBox = true;
        
        private readonly List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private readonly List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private readonly List<TgcBoundingAxisAlignBox> objetosColisionables = new List<TgcBoundingAxisAlignBox>();
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingSphere characterSphere;
        private SphereCollisionManager collisionManager;
        private TgcScene escenario;
        private TgcMesh autito;
        private TgcSkyBox skyBox;

        private float jumping;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = "Collision";
            Name = "Movimientos Esfera 3ra Persona";
            Description = "Movimientos Esfera 3ra Persona";
        }

        public override void Init()
        {
            //Cargar escenario específico para este ejemplo
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "Escenarios\\escenario-objetos2-TgcScene.xml");

            //Cargar autito
            autito = loader.loadSceneFromFile(MediaDir + "Vehicles\\car-minibus-blue-TgcScene.xml").Meshes[0];

            //Se utiliza autotransform, aunque este es un claro ejemplo de que no se debe usar autotransform,
            //hay muchas operaciones y la mayoria las maneja el manager de colisiones, con lo cual se esta
            //perdiendo el control de las transformaciones del personaje.
            autito.AutoTransform = true;
            //Escalarlo porque es muy grande
            autito.Position = new TGCVector3(0, 30, -105);
            //Rotarlo 180° porque esta mirando para el otro lado
            autito.RotateY(Geometry.DegreeToRadian(180f));
            //Escalamos el personaje ya que sino la escalera es demaciado grande.
            autito.Scale = new TGCVector3(0.75f, 0.75f, 0.75f);
            //BoundingSphere que va a usar el personaje
            autito.AutoUpdateBoundingBox = false;
            characterSphere = new TgcBoundingSphere(autito.BoundingBox.calculateBoxCenter(), autito.BoundingBox.calculateBoxRadius());
            
            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                if (!(mesh.Name.Equals("Arbusto") || mesh.Name.Equals("Pasto")))
                    objetosColisionables.Add(mesh.BoundingBox);
            }

            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(autito.Position, new TGCVector3(0, 2, 0), 3, -15);
            Camara = camaraInterna;

            //Crear SkyBox
            skyBox = new TgcSkyBox();
            skyBox.Center = TGCVector3.Empty;
            skyBox.Size = new TGCVector3(10000, 10000, 10000);
            var texturesPath = MediaDir + "Texturas\\Quake\\SkyBox3\\";
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Up, texturesPath + "Up.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Down, texturesPath + "Down.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Left, texturesPath + "Left.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Right, texturesPath + "Right.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Front, texturesPath + "Back.jpg");
            skyBox.setFaceTexture(TgcSkyBox.SkyFaces.Back, texturesPath + "Front.jpg");
            skyBox.Init();

            //Modifier para ver BoundingBox
            //showBoundingBoxModifier = AddBoolean("showBoundingBox", "Bouding Box", true);

            //Modifiers para desplazamiento del personaje
            //velocidadCaminarModifier = AddFloat("VelocidadCaminar", 0, 20, 10);
            //velocidadRotacionModifier = AddFloat("VelocidadRotacion", 1f, 360f, 150f);
            //habilitarGravedadModifier = AddBoolean("HabilitarGravedad", "Habilitar Gravedad", true);
            //gravedadModifier = AddVertex3f("Gravedad", new TGCVector3(-50, -50, -50), new TGCVector3(50, 50, 50), new TGCVector3(0, -10, 0));
            //slideFactorModifier = AddFloat("SlideFactor", 1f, 2f, 1.3f);

            //UserVars.addVar("Movement");
        }

        public override void Update()
        {
            PreUpdate();

            //obtener velocidades de Modifiers
            var velocidadCaminar = 0.5f;
            var velocidadRotacion = 150f;
            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Jump
            if (Input.keyUp(Key.Space) && jumping <= 0)
            {
                jumping = 2;
            }
            if (Input.keyUp(Key.Space) || jumping > 0)
            {
                jumping -= 2 * ElapsedTime;
                jump = jumping;
                moving = true;
            }

            if (Input.keyUp(Key.F1))
            {
                showBoundingBox = !showBoundingBox;
            }
            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                autito.RotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                //personaje.playAnimation("Caminando", true);
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                //personaje.playAnimation("Parado", true);
            }

            //Vector de movimiento
            var movementVector = TGCVector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new TGCVector3(FastMath.Sin(autito.Rotation.Y) * moveForward, jump,
                    FastMath.Cos(autito.Rotation.Y) * moveForward);
            }

            //Actualizar valores de gravedad
            collisionManager.GravityEnabled = true;
            collisionManager.GravityForce = new TGCVector3(0, -1, 0);
            collisionManager.SlideFactor = 1.3f;

            //Mover personaje con detección de colisiones, sliding y gravedad
            var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
            autito.Move(realMovement);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = autito.Position;

            //Actualizar valores de la linea de movimiento
            //directionArrow.PStart = characterSphere.Center;
            //directionArrow.PEnd = characterSphere.Center + TGCVector3.Multiply(movementVector, 50);
            //directionArrow.updateValues();

            //Cargar desplazamiento realizar en UserVar
            //UserVars.setValue("Movement", TGCVector3.PrintVector3(realMovement));

            //Ver cual de las mallas se interponen en la visión de la cámara en 3ra persona.
            objectsBehind.Clear();
            objectsInFront.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                TGCVector3 q;
                if (TgcCollisionUtils.intersectSegmentAABB(Camara.Position, camaraInterna.Target,
                    mesh.BoundingBox, out q))
                {
                    objectsBehind.Add(mesh);
                }
                else
                {
                    objectsInFront.Add(mesh);
                }
            }
            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            //Obtener boolean para saber si hay que mostrar Bounding Box
            var showBB = showBoundingBox;

            DrawText.drawText("Con la tecla F1 se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("W A S D para el movimiento básico", 0, 35, Color.OrangeRed);
            DrawText.drawText("Tecla SPACE para saltar", 0, 50, Color.OrangeRed);

            //Render mallas que no se interponen
            foreach (var mesh in objectsInFront)
            {
                mesh.Render();
                if (showBB)
                {
                    mesh.BoundingBox.Render();
                }
            }

            //Para las mallas que se interponen a la cámara, solo renderizar su BoundingBox
            foreach (var mesh in objectsBehind)
            {
                mesh.BoundingBox.Render();
            }

            //Render personaje
            autito.Render();
            if (showBB)
            {
                characterSphere.Render();
            }

            //Render linea
            //directionArrow.Render();

            //Render SkyBox
            skyBox.Render();

            PostRender();
        }

        public override void Dispose()
        {
            escenario.DisposeAll();
            autito.Dispose();
            skyBox.Dispose();
        }
    }
}