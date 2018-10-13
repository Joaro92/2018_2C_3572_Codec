using System;
using System.Drawing;
using System.Drawing.Text;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Text;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.World;

namespace TGC.Group.Model
{
    public class HUD
    {
        private Drawer2D drawer2D;
        private readonly int screenHeight, screenWidth;
        private CustomSprite statsBar, healthBar, specialBar, weaponsHud;
        private TGCVector2 specialScale, hpScale;
        private TgcText2D speed, km, actualWeapon, ammoQuantity, border;

        public HUD(GameModel gameModel)
        {
            // Tamaño de la pantalla
            screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            screenWidth = D3DDevice.Instance.Device.Viewport.Width;

            // Inicializamos la interface para dibujar sprites 2D
            drawer2D = new Drawer2D();

            // Sprite del HUD de la velocidad y stats del jugador
            statsBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\stats.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(screenWidth * 0.81f, screenHeight * 0.695f)
            };

            var scalingFactorX = (float)screenWidth / (float)statsBar.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)statsBar.Bitmap.Height;

            statsBar.Scaling = new TGCVector2(0.25f, 0.42f) * (scalingFactorY / scalingFactorX);

            // Sprite del HUD de las armas
            weaponsHud = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\weapons hud 2.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(-15, screenHeight * 0.64f)
            };

            scalingFactorX = (float)screenWidth / (float)weaponsHud.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)weaponsHud.Bitmap.Height;

            weaponsHud.Scaling = new TGCVector2(0.6f, 0.6f) * (scalingFactorY / scalingFactorX);

            // Sprite que representa la vida
            healthBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\healthBar.png", D3DDevice.Instance.Device),
                //Position = new TGCVector2(screenWidth * 0.8605f, screenHeight * 0.728f); //para 125 % escalado
                Position = new TGCVector2(screenWidth * 0.8515f, screenHeight * 0.7215f) //para 100% escalado
            };

            scalingFactorX = (float)screenWidth / (float)healthBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)healthBar.Bitmap.Height;

            healthBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            hpScale = healthBar.Scaling;

            // Sprite de la barra de especiales
            specialBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\specialBar.png", D3DDevice.Instance.Device),
                //Position = new TGCVector2(screenWidth * 0.861f, screenHeight * 0.83f); //para 125 % escalado
                Position = new TGCVector2(screenWidth * 0.8515f, screenHeight * 0.8025f) //para 100 % escalado
            };

            scalingFactorX = (float)screenWidth / (float)specialBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)specialBar.Bitmap.Height;

            specialBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            specialScale = specialBar.Scaling;

            // Fuente para mostrar la velocidad
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile(gameModel.MediaDir + "Fonts\\Open 24 Display St.ttf");
            FontFamily family = pfc.Families[0];
            var speedFont = new Font(family, 32);
            var kmFont = new Font(family, 20);

            speed = new TgcText2D
            {
                Text = "0",
                Color = Color.Green,
                //Position = new Point((int)(screenWidth * 0.397f), (int)(screenHeight * 0.906f))  // para 125% escalado
                Position = new Point((int)(screenWidth * 0.38f), (int)(screenHeight * 0.865f)) // para 100% escalado

            };
            speed.changeFont(speedFont);
            km = new TgcText2D
            {
                Text = "km",
                Color = Color.Black,
                //Position = new Point((int)(screenWidth * 0.431f), (int)(screenHeight * 0.927f)) // para 125% escalado
                Position = new Point((int)(screenWidth * 0.41f), (int)(screenHeight * 0.88f)) // para 100% escalado

            };
            km.changeFont(kmFont);

            // Fuentes para mostrar la munición y armas
            pfc.AddFontFile(gameModel.MediaDir + "Fonts\\Insanibc.ttf");
            family = pfc.Families[0];
            var actualWeaponFont = new Font(family, 24);
            var ammoQuantityFont = new Font(family, 22);

            actualWeapon = new TgcText2D
            {
                Text = "[ None ]",
                Color = Color.Black,
                //Position = new Point(-(int)(screenWidth * 0.406f), (int)(screenHeight * 0.921f))  // para 125% escalado
                Position = new Point(-(int)(screenWidth * 0.421f), (int)(screenHeight * 0.87f)) // para 100% escalado

            };
            actualWeapon.changeFont(actualWeaponFont);

            ammoQuantity = new TgcText2D
            {
                Text = "-",
                Color = Color.Black,
                //Position = new Point(-(int)(screenWidth * 0.345f), (int)(screenHeight * 0.856f))  // para 125% escalado
                Position = new Point(-(int)(screenWidth * 0.3725f), (int)(screenHeight * 0.815f)) // para 100% escalado

            };
            ammoQuantity.changeFont(ammoQuantityFont);
            border = new TgcText2D // El borde es para que tenga un color blanco de fondo para que se distinga más
            {
                Text = "-",
                Color = Color.White,
                //Position = new Point(-(int)(screenWidth * 0.3453f), (int)(screenHeight * 0.8535f))  // para 125% escalado
                Position = new Point(-(int)(screenWidth * 0.3728f), (int)(screenHeight * 0.8125f)) // para 100% escalado

            };
            border.changeFont(actualWeaponFont);
        }

        public void Update(GameModel gameModel, Player1 player1)
        {
            //if (gameModel.Input.keyPressed(Key.Z))
            //{
            //    ammoQuantity.Text = (int.Parse(ammoQuantity.Text) + 1).ToString();
            //}

            // Actualizamos la barra de especial
            specialBar.Scaling = new TGCVector2(specialScale.X * (player1.specialPoints / 100f), specialScale.Y);
            healthBar.Scaling = new TGCVector2(hpScale.X * (player1.hitPoints / 100f), hpScale.Y);

            // Actualizar los stats
            if (player1.specialPoints < 100)
            {
                player1.specialPoints += gameModel.ElapsedTime;
            }
            else
            {
                player1.specialPoints = 100;
            }

            speed.Text = player1.linealVelocity;

            border.Text = ammoQuantity.Text;
        }

        public void Render()
        {
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(statsBar);
            drawer2D.DrawSprite(healthBar);
            drawer2D.DrawSprite(specialBar);
            drawer2D.DrawSprite(weaponsHud);
            drawer2D.EndDrawSprite();

            if (speed.Text.Contains("-"))
            {
                speed.Color = Color.IndianRed;
            }
            else
            {
                speed.Color = Color.Green;
            }

            speed.render();
            km.render();

            actualWeapon.render();
            border.render();
            ammoQuantity.render();
        }

        public void Dispose()
        {
            statsBar.Dispose();
            healthBar.Dispose();
            specialBar.Dispose();
            weaponsHud.Dispose();
            actualWeapon.Dispose();
            ammoQuantity.Dispose();
            border.Dispose();
            speed.Dispose();
            km.Dispose();
        }
    }


}
