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
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        /*
        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        //Caja que se muestra en el ejemplo.
        private TGCBox Box { get; set; }

        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;

            //Textura de la carperta Media. Game.Default es un archivo de configuracion (Game.settings) util para poner cosas.
            //Pueden abrir el Game.settings que se ubica dentro de nuestro proyecto para configurar.
            var pathTexturaCaja = MediaDir + Game.Default.TexturaCaja;

            //Cargamos una textura, tener en cuenta que cargar una textura significa crear una copia en memoria.
            //Es importante cargar texturas en Init, si se hace en el render loop podemos tener grandes problemas si instanciamos muchas.
            var texture = TgcTexture.createTexture(pathTexturaCaja);

            //Creamos una caja 3D ubicada de dimensiones (5, 10, 5) y la textura como color.
            var size = new TGCVector3(5, 10, 5);
            //Construimos una caja según los parámetros, por defecto la misma se crea con centro en el origen y se recomienda así para facilitar las transformaciones.
            Box = TGCBox.fromSize(size, texture);
            //Posición donde quiero que este la caja, es común que se utilicen estructuras internas para las transformaciones.
            //Entonces actualizamos la posición lógica, luego podemos utilizar esto en render para posicionar donde corresponda con transformaciones.
            Box.Position = new TGCVector3(-25, 0, 0);

            //Cargo el unico mesh que tiene la escena.
            Mesh = new TgcSceneLoader().loadSceneFromFile(MediaDir + "LogoTGC-TgcScene.xml").Meshes[0];
            //Defino una escala en el modelo logico del mesh que es muy grande.
            Mesh.Scale = new TGCVector3(0.5f, 0.5f, 0.5f);

            //Suelen utilizarse objetos que manejan el comportamiento de la camara.
            //Lo que en realidad necesitamos gráficamente es una matriz de View.
            //El framework maneja una cámara estática, pero debe ser inicializada.
            //Posición de la camara.
            var cameraPosition = new TGCVector3(0, 0, 125);
            //Quiero que la camara mire hacia el origen (0,0,0).
            var lookAt = TGCVector3.Empty;
            //Configuro donde esta la posicion de la camara y hacia donde mira.
            Camara.SetCamera(cameraPosition, lookAt);
            //Internamente el framework construye la matriz de view con estos dos vectores.
            //Luego en nuestro juego tendremos que crear una cámara que cambie la matriz de view con variables como movimientos o animaciones de escenas.
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

            //Capturar Input teclado
            if (Input.keyPressed(Key.F))
            {
                BoundingBox = !BoundingBox;
            }

            //Capturar Input Mouse
            if (Input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Como ejemplo podemos hacer un movimiento simple de la cámara.
                //En este caso le sumamos un valor en Y
                Camara.SetCamera(Camara.Position + new TGCVector3(0, 10f, 0), Camara.LookAt);
                //Ver ejemplos de cámara para otras operaciones posibles.

                //Si superamos cierto Y volvemos a la posición original.
                if (Camara.Position.Y > 300f)
                {
                    Camara.SetCamera(new TGCVector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
                }
            }

            PostUpdate();
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones según nuestra conveniencia.
            PreRender();

            //Dibuja un texto por pantalla
            DrawText.drawText("Con la tecla F se dibuja el bounding box.", 0, 20, Color.OrangeRed);
            DrawText.drawText("Con clic izquierdo subimos la camara [Actual]: " + TGCVector3.PrintVector3(Camara.Position), 0, 30, Color.OrangeRed);

            //Siempre antes de renderizar el modelo necesitamos actualizar la matriz de transformacion.
            //Debemos recordar el orden en cual debemos multiplicar las matrices, en caso de tener modelos jerárquicos, tenemos control total.
            Box.Transform = TGCMatrix.Scaling(Box.Scale) * TGCMatrix.RotationYawPitchRoll(Box.Rotation.Y, Box.Rotation.X, Box.Rotation.Z) * TGCMatrix.Translation(Box.Position);
            //A modo ejemplo realizamos toda las multiplicaciones, pero aquí solo nos hacia falta la traslación.
            //Finalmente invocamos al render de la caja
            Box.Render();

            //Cuando tenemos modelos mesh podemos utilizar un método que hace la matriz de transformación estándar.
            //Es útil cuando tenemos transformaciones simples, pero OJO cuando tenemos transformaciones jerárquicas o complicadas.
            Mesh.UpdateMeshTransform();
            //Render del mesh
            Mesh.Render();

            //Render de BoundingBox, muy útil para debug de colisiones.
            if (BoundingBox)
            {
                Box.BoundingBox.Render();
                Mesh.BoundingBox.Render();
            }

            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            //Dispose de la caja.
            Box.Dispose();
            //Dispose del mesh.
            Mesh.Dispose();
        }
    }
    */

        /*
        private TGCBooleanModifier showBoundingBoxModifier;
        private TGCFloatModifier velocidadCaminarModifier;
        private TGCFloatModifier velocidadRotacionModifier;
        private TGCBooleanModifier habilitarGravedadModifier;
        private TGCVertex3fModifier gravedadModifier;
        private TGCFloatModifier slideFactorModifier;
        */

        private readonly List<TgcMesh> objectsBehind = new List<TgcMesh>();
        private readonly List<TgcMesh> objectsInFront = new List<TgcMesh>();
        private readonly List<TgcBoundingAxisAlignBox> objetosColisionables = new List<TgcBoundingAxisAlignBox>();
        private TgcThirdPersonCamera camaraInterna;
        private TgcBoundingSphere characterSphere;
        private SphereCollisionManager collisionManager;
        //private TgcArrow directionArrow;
        private TgcScene escenario;
        private TgcMesh autito;
        //private TgcSkeletalMesh personaje;
        private TgcSkyBox skyBox;

        private float jumping;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = "Collision";
            Name = "Movimientos Esfera 3ra Persona";
            Description =
                "Estrategia integral de colisión: BoundingSphere + Gravedad + Sliding + Jump. Movimiento con W, A, S, D, Space. No ha sido implementado en su totalidad y aún existen muchos puntos por mejorar y algunos bugs.";
        }

        public override void Init()
        {
            //Cargar escenario específico para este ejemplo
            var loader = new TgcSceneLoader();
            escenario = loader.loadSceneFromFile(MediaDir + "Escenarios\\escenario-base-attached-TgcScene.xml");

            //Cargar personaje con animaciones
            //var skeletalLoader = new TgcSkeletalLoader();
            autito = loader.loadSceneFromFile(MediaDir + "Vehicles\\car-minibus-blue-TgcScene.xml").Meshes[0];
            /*
            personaje = skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\Robot\\Robot-TgcSkeletalMesh.xml",
                    MediaDir + "SkeletalAnimations\\Robot\\",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\Robot\\Caminando-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\Robot\\Parado-TgcSkeletalAnim.xml"
                    });

            
            //Configurar animacion inicial
            personaje.playAnimation("Parado", true);
            */

            //Se utiliza autotransform, aunque este es un claro ejemplo de que no se debe usar autotransform,
            //hay muchas operaciones y la mayoria las maneja el manager de colisiones, con lo cual se esta
            //perdiendo el control de las transformaciones del personaje.
            autito.AutoTransform = true;
            //Escalarlo porque es muy grande
            autito.Position = new TGCVector3(0, 500, -100);
            //Rotarlo 180° porque esta mirando para el otro lado
            autito.RotateY(Geometry.DegreeToRadian(180f));
            //Escalamos el personaje ya que sino la escalera es demaciado grande.
            //personaje.Scale = new TGCVector3(0.12f, 0.12f, 0.12f);
            //BoundingSphere que va a usar el personaje
            //personaje.AutoUpdateBoundingBox = false;
            characterSphere = new TgcBoundingSphere(autito.BoundingBox.calculateBoxCenter(), autito.BoundingBox.calculateBoxRadius());

            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                objetosColisionables.Add(mesh.BoundingBox);
            }

            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;

            //Configurar camara en Tercer Persona
            camaraInterna = new TgcThirdPersonCamera(autito.Position, new TGCVector3(0, 5, 0), 10, -24);
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
            if (Input.keyUp(Key.Space) && jumping < 30)
            {
                jumping = 30;
            }
            if (Input.keyUp(Key.Space) || jumping > 0)
            {
                jumping -= 30 * ElapsedTime;
                jump = jumping;
                moving = true;
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
            collisionManager.GravityForce = new TGCVector3(0, -10, 0);
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
            var showBB = false;

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