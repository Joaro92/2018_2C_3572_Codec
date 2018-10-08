﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Examples.Engine2D.Spaceship.Core;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using Microsoft.DirectX.DirectInput;

namespace TGC.Group.Model.GameStates
{
    public class MenuInicial : IGameState
    {
        private GameModel gameModel;
        private Drawer2D drawer2D;
        private CustomSprite background;
        private CustomSprite start;
        private bool showStart = true;
        private float timerStart1 = 0f;
        private float timerStart2 = 0f;
        private bool timerStart1flag = true;
        private bool timerStart2flag = false;
        private float frecStart = 2.5f; //frecuencia de parpadeo del start

        public MenuInicial(GameModel gameModel)
        {
            this.gameModel = gameModel;
            drawer2D = new Drawer2D();

            // Cargo la imagen de fondo
            background = new CustomSprite();
            background.Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\start-screen.png", D3DDevice.Instance.Device);

            // La ubico ocupando toda la pantalla
            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;
            var scalingFactorX = (float)screenWidth / (float)background.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)background.Bitmap.Height;

            background.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);

            // Cargo el mensaje de start
            start = new CustomSprite();
            start.Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\press-start.png", D3DDevice.Instance.Device);
            // La ubico en la pantalla
            // var startSize = background.Bitmap.Size;
            start.Scaling = TGCVector2.One * (scalingFactorY / scalingFactorX);
            start.Position = new TGCVector2(screenWidth * 0.083f , screenHeight * 0.67f);

            this.gameModel.Camara = new TgcThirdPersonCamera();
        }

        public void Update()
        {
            if(timerStart1flag)
            timerStart1 += gameModel.ElapsedTime;
            if (timerStart2flag)
                timerStart2 += gameModel.ElapsedTime;

            if(timerStart1 >= (1/frecStart) )
            {
                if (showStart)
                    showStart = false;
                else
                    showStart = true;
                timerStart1 = 0f;
            }
            if(timerStart2 >= 0.75f)
            {
                showStart = false;
                gameModel.GameState = new Partida(gameModel);
            }

            if (gameModel.Input.keyDown(Key.Return))
            {
                frecStart *= 2;
                timerStart2flag = true;
            }

        }

        public void Render()
        {
            // Iniciar dibujado de sprites
            drawer2D.BeginDrawSprite();

            // Dibujar sprites
            drawer2D.DrawSprite(background);
            if(showStart)
                drawer2D.DrawSprite(start);

            // Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();
        }

        public void Dispose()
        {
            background.Dispose();
            start.Dispose();
        }
    }
}
