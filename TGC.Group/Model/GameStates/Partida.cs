﻿using System.Drawing;
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
using TGC.Examples.Engine2D.Spaceship.Core;

namespace TGC.Group.Model.GameStates
{
    public class Partida : IGameState
    {
        private GameModel gameModel;

        private readonly string[] vehicleColors = new string[] { "Blue", "Citrus", "Green", "Orange", "Red", "Silver", "Violet" };
        private readonly ModoCamara[] modosCamara = new ModoCamara[] { ModoCamara.NORMAL, ModoCamara.LEJOS, ModoCamara.CERCA };

        private PhysicsGame physicsEngine;
        private Player1 player1;
        private TgcThirdPersonCamera camaraInterna;
        private bool drawUpVector = false;
        private bool showBoundingBox = false;
        private TgcArrow directionArrow;
        private float anguloCamara;
        private float halfsPI;
        private ModoCamara modoCamara = ModoCamara.NORMAL;
        private Drawer2D drawer2D;
        private int screenHeight, screenWidth;
        private CustomSprite healthBar;


        public Partida(GameModel gameModel)
        {
            this.gameModel = gameModel;

            screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            screenWidth = D3DDevice.Instance.Device.Viewport.Width;
            
            drawer2D = new Drawer2D();
            healthBar = new CustomSprite();
            healthBar.Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\healthBar.png", D3DDevice.Instance.Device);
            healthBar.Position = new TGCVector2(screenWidth * 0.82f, screenHeight * 0.86f);

            var scalingFactorX = (float)screenWidth / (float)healthBar.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)healthBar.Bitmap.Height;

            healthBar.Scaling = new TGCVector2(0.45f, 0.28f) * (scalingFactorY / scalingFactorX);

            // Preparamos el mundo físico con todos los elementos que pertenecen a el
            physicsEngine = new NivelUno();
            player1 = physicsEngine.Init();

            // Configuramos la Cámara en tercera persona para que siga a nuestro Player 1
            camaraInterna = new TgcThirdPersonCamera(new TGCVector3(player1.rigidBody.CenterOfMassPosition), new TGCVector3(0, 2, 0), modoCamara.AlturaCamara(), modoCamara.ProfundidadCamara());
            this.gameModel.Camara = camaraInterna;


            // Creamos una flecha que representara el vector UP del auto
            directionArrow = new TgcArrow();
            directionArrow.BodyColor = Color.Red;
            directionArrow.HeadColor = Color.Green;
            directionArrow.Thickness = 0.1f;
            directionArrow.HeadSize = new TGCVector2(1, 2);
        }


        public void Update()
        {
            // Actualizar el mundo físico
            player1 = physicsEngine.Update(gameModel.Input, camaraInterna, gameModel.ElapsedTime);

            // Mostrar bounding box del TgcMesh
            if (gameModel.Input.keyPressed(Key.F1))
            {
                showBoundingBox = !showBoundingBox;
            }

            // Girar la cámara unos grados
            if (gameModel.Input.keyPressed(Key.F2))
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
            if (gameModel.Input.keyPressed(Key.F3))
            {
                drawUpVector = !drawUpVector;
            }

            if (gameModel.Input.keyPressed(Key.F4))
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

                string oldColor = newTextureName.Split('\\')[5].Split(' ')[2].Split('.')[0];
                string newColor = vehicleColors.getNextOption(oldColor);
                newTextureName = newTextureName.Replace(oldColor, newColor);

                var textureAux = TgcTexture.createTexture(D3DDevice.Instance.Device, newTextureName.Split('\\')[5], newTextureName);
                player1.tgcMesh.addDiffuseMap(textureAux);
                player1.tgcMesh.deleteDiffuseMap(index, 4); //de donde sale el 4?
            }

            // Rotar 90° la cámara
            if (gameModel.Input.keyPressed(Key.F5))
            {
                halfsPI = (halfsPI + FastMath.PI_HALF) % FastMath.TWO_PI;
            }

            // Modo cámara
            if (gameModel.Input.keyPressed(Key.V))
            {
                modoCamara = modosCamara.getNextOption(modoCamara);

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
        }

        public void Render()
        {
            
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(healthBar);
            drawer2D.EndDrawSprite();

            // Texto en pantalla sobre los comandos disponibles
            var DrawText = gameModel.DrawText;
            DrawText.drawText("Con la tecla F1 se dibuja el bounding box (Deprecado, las colisiones las maneja Bullet)", 3, 20, Color.YellowGreen);
            DrawText.drawText("Con la tecla F2 se rota el ángulo de la cámara", 3, 35, Color.YellowGreen);
            DrawText.drawText("Con la tecla F3 se dibuja el Vector UP del vehículo", 3, 50, Color.YellowGreen);
            DrawText.drawText("Con la tecla V se cambia el modo de cámara (NORMAL, LEJOS, CERCA)", 3, 65, Color.YellowGreen);
            DrawText.drawText("W A S D para el movimiento básico", 3, 80, Color.YellowGreen);
            DrawText.drawText("Control Izquierdo para frenar", 3, 95, Color.YellowGreen);
            DrawText.drawText("Tecla ESPACIO para saltar", 3, 110, Color.YellowGreen);

            // Texto en pantalla sobre el juego
            DrawText.drawText("Velocidad: " + player1.linealVelocity, 15, screenHeight - 50, Color.White);

            if (player1.flippedTime > 0)
            {
                DrawText.drawText("Tiempo dado vuelta: " + player1.flippedTime, 15, screenHeight - 35, Color.White);
            }

            // Renderiza todo lo perteneciente al mundo físico
            physicsEngine.Render();

            // Renderizar el Vector UP
            if (drawUpVector)
            {
                directionArrow.Render();
            }

            // Finalizar el dibujado de Sprites
            //
        }

        public void Dispose()
        {
            physicsEngine.Dispose();
            directionArrow.Dispose();
            player1.rigidBody.Dispose();
        }
    }
}