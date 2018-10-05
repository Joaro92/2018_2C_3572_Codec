using System.Drawing;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using TGC.Group.Bullet.Physics;
using TGC.Group.Nivel1;
using TGC.Group.PlayerOne;
using TGC.Group.Utils;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Direct3D;
using BulletSharp.Math;
using BulletSharp;
using TGC.Core.Textures;
using Microsoft.DirectX.Direct3D;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        private PhysicsGame physicsEngine;
        private Player1 player1;
        private TgcThirdPersonCamera camaraInterna;
        private bool drawUpVector = false;
        private bool showBoundingBox = false;
        public int ScreenHeight, ScreenWidth;
        private TgcArrow directionArrow;
        private float anguloCamara;
        private float halfsPI;
        private ModoCamara modoCamara = ModoCamara.NORMAL;

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
        {
            // Obtener las dimensiones de la ventana
            ScreenWidth = D3DDevice.Instance.Device.Viewport.Width;
            ScreenHeight = D3DDevice.Instance.Device.Viewport.Height;
            
            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            physicsEngine = new NivelUno();
            player1 = physicsEngine.Init();

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara() , modoCamara.ProfundidadCamara());
            Camara = camaraInterna;

            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.1f;
            directionArrow.HeadSize = new TGCVector2(1, 2);
        }

        public override void Update()
        {
            PreUpdate();

            // Actualizar el mundo físico
            player1 = physicsEngine.Update(Input, camaraInterna, ElapsedTime);

            // Mostrar bounding box del TgcMesh
            if (Input.keyPressed(Key.F1))
            {
                showBoundingBox = !showBoundingBox;
            }

            // Girar la cámara unos grados
            if (Input.keyPressed(Key.F2))
            {
                if (anguloCamara == 0.33f)
                {
                    anguloCamara = -anguloCamara;
                }
                else
                {
                    anguloCamara += 0.33f;
                }
            }

            // Dibujar el Vector UP
            if (Input.keyPressed(Key.F3))
            {
                drawUpVector = !drawUpVector;
            }

            if (Input.keyPressed(Key.F4))
            {
                TgcTexture[] diffuseMaps = player1.tgcMesh.DiffuseMaps;

                string newTextureName = "";
                int index = 0;
                foreach (TgcTexture texture in diffuseMaps)
                {
                    if (texture.FileName.Contains("Car Material"))
                    {
                        newTextureName = texture.FilePath;
                        break;
                    }
                    index++;
                }

                if (newTextureName.Contains("Blue"))
                {
                    newTextureName = newTextureName.Replace("Blue", "Citrus");
                }
                else if (newTextureName.Contains("Citrus"))
                {
                    newTextureName = newTextureName.Replace("Citrus", "Green");
                }
                else if (newTextureName.Contains("Green"))
                {
                    newTextureName = newTextureName.Replace("Green", "Orange");
                }
                else if (newTextureName.Contains("Orange"))
                {
                    newTextureName = newTextureName.Replace("Orange", "Red");
                }
                else if (newTextureName.Contains("Red"))
                {
                    newTextureName = newTextureName.Replace("Red", "Silver");
                }
                else if (newTextureName.Contains("Silver"))
                {
                    newTextureName = newTextureName.Replace("Silver", "Violet");
                }
                else if (newTextureName.Contains("Violet"))
                {
                    newTextureName = newTextureName.Replace("Violet", "Blue");
                }

                var textureAux = TgcTexture.createTexture(D3DDevice.Instance.Device, newTextureName.Split('\\')[5], newTextureName);
                player1.tgcMesh.addDiffuseMap(textureAux);
                player1.tgcMesh.deleteDiffuseMap(index, 4);
            }

            // Rotar 90° la cámara
            if (Input.keyPressed(Key.F5))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Modo cámara
            if (Input.keyPressed(Key.V))
            {
                if(modoCamara == ModoCamara.NORMAL)
                {
                    modoCamara = ModoCamara.LEJOS;        
                }
                else if (modoCamara == ModoCamara.LEJOS)
                {
                    modoCamara = ModoCamara.CERCA;
                }
                else
                {
                    modoCamara = ModoCamara.NORMAL;
                }
                camaraInterna.OffsetHeight = modoCamara.AlturaCamara();
                camaraInterna.OffsetForward = modoCamara.ProfundidadCamara();
            }

            // Hacer que la cámara apunte a nuestro Player 1
            camaraInterna.Target = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
            camaraInterna.RotationY = Quat.ToEulerAngles(player1.rigidBody.Orientation).Y + anguloCamara + halfsPI;

            // Actualizar el Vector UP si se dibuja
            if (drawUpVector)
            {
                directionArrow.PStart = new TGCVector3(player1.rigidBody.CenterOfMassPosition);
                directionArrow.PEnd = directionArrow.PStart + new TGCVector3(Vector3.TransformNormal(Vector3.UnitY, player1.rigidBody.InterpolationWorldTransform)) * 3.5f;
                directionArrow.updateValues();
            }

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            // Texto en pantalla sobre los comandos disponibles
            DrawText.drawText("Con la tecla F1 se dibuja el bounding box (Deprecado, las colisiones las maneja Bullet)", 3, 20, Color.YellowGreen);
            DrawText.drawText("Con la tecla F2 se rota el ángulo de la cámara", 3, 35, Color.YellowGreen);
            DrawText.drawText("Con la tecla F3 se dibuja el Vector UP del vehículo", 3, 50, Color.YellowGreen);
            DrawText.drawText("Con la tecla V se cambia el modo de cámara (NORMAL, LEJOS, CERCA)", 3, 65, Color.YellowGreen);
            DrawText.drawText("W A S D para el movimiento básico", 3, 80, Color.YellowGreen);
            DrawText.drawText("Control Izquierdo para frenar", 3, 95, Color.YellowGreen);
            DrawText.drawText("Tecla ESPACIO para saltar", 3, 110, Color.YellowGreen);

            // Texto en pantalla sobre el juego
            DrawText.drawText("Velocidad: " + player1.linealVelocity, 15, ScreenHeight - 50, Color.White);

            if (player1.flippedTime > 0)
            {
                DrawText.drawText("Tiempo dado vuelta: " + player1.flippedTime, 15, ScreenHeight - 35, Color.White);
            }

            // Renderiza todo lo perteneciente al mundo físico
            physicsEngine.Render();

            // Renderizar el Vector UP
            if (drawUpVector)
            {
                directionArrow.Render();
            }

            PostRender();
        }

        public override void Dispose()
        {
            physicsEngine.Dispose();
            directionArrow.Dispose();
            player1.rigidBody.Dispose();
        }
    }
}