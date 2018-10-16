using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.Text;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.World;
using TGC.Group.Utils;

namespace TGC.Group.Model
{
    public class HUD
    {
        private Player1 player1;
        private Drawer2D drawer2D;
        private CustomSprite statsBar, healthBar, specialBar, weaponsHud;
        private TGCVector2 specialScale, hpScale;
        private TgcText2D speed, km, actualWeapon, ammoQuantity, border, reloj, turbo;
        private readonly int scaling = GameModel.GetWindowsScaling(); // 96 es el 100%, 120 es el 125%
        private readonly int screenHeight = D3DDevice.Instance.Device.Viewport.Height;
        private readonly int screenWidth = D3DDevice.Instance.Device.Viewport.Width;

        public HUD(Player1 p1, float time)
        {
            player1 = p1;

            InitializeHUDSprites();

            InitializeHUDTexts(time);
        }

        public void Update(float matchTime)
        {
            // Actualizamos la barra de especial
            specialBar.Scaling = new TGCVector2(specialScale.X * (player1.specialPoints / player1.maxSpecialPoints), specialScale.Y);
            healthBar.Scaling = new TGCVector2(hpScale.X * (player1.hitPoints / player1.maxHitPoints), hpScale.Y);

            // Actualizamos la munición y velocidad actual
            speed.Text = player1.linealVelocity;
            border.Text = ammoQuantity.Text;

            reloj.Text = formatTime(matchTime);
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

            reloj.render();
            if (player1.turbo)
                turbo.render();

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
            reloj.Dispose();
            turbo.Dispose();
        }

        //--------------------------------------------------------------------------------------------------//

        private void InitializeHUDSprites()
        {
            var imgDir = Game.Default.MediaDirectory + Game.Default.ImagesDirectory;

            // Inicializamos la interface para dibujar sprites 2D
            drawer2D = new Drawer2D();

            // Sprite del HUD de la velocidad y stats del jugador
            statsBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "stats.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(screenWidth * 0.81f, screenHeight * 0.695f)
            };

            var scalingFactorX = (float)screenWidth / (float)statsBar.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)statsBar.Bitmap.Height;

            statsBar.Scaling = new TGCVector2(0.25f, 0.42f) * (scalingFactorY / scalingFactorX);

            // Sprite del HUD de las armas
            weaponsHud = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "weapons hud 2.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(-15, screenHeight * 0.64f)
            };

            scalingFactorX = (float)screenWidth / (float)weaponsHud.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)weaponsHud.Bitmap.Height;

            weaponsHud.Scaling = new TGCVector2(0.6f, 0.6f) * (scalingFactorY / scalingFactorX);

            // Sprite que representa la vida
            healthBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "healthBar.png", D3DDevice.Instance.Device)
            };
            if (scaling == 96) healthBar.Position = new TGCVector2(screenWidth * 0.8515f, screenHeight * 0.7215f);
            else healthBar.Position = new TGCVector2(screenWidth * 0.8605f, screenHeight * 0.728f);

            scalingFactorX = (float)screenWidth / (float)healthBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)healthBar.Bitmap.Height;

            healthBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            hpScale = healthBar.Scaling;

            // Sprite de la barra de especiales
            specialBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "specialBar.png", D3DDevice.Instance.Device)
            };
            if (scaling == 96) specialBar.Position = new TGCVector2(screenWidth * 0.8515f, screenHeight * 0.8025f);
            else specialBar.Position = new TGCVector2(screenWidth * 0.861f, screenHeight * 0.83f);

            scalingFactorX = (float)screenWidth / (float)specialBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)specialBar.Bitmap.Height;

            specialBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            specialScale = specialBar.Scaling;
        }

        private void InitializeHUDTexts(float time)
        {
            // Fuente para mostrar la velocidad y el reloj
            var speedFont = UtilMethods.createFont("Open 24 Display St", 32);
            var kmFont = UtilMethods.createFont("Open 24 Display St", 20);
            var relojFont = UtilMethods.createFont("Open 24 Display St", 40);

            speed = new TgcText2D
            {
                Text = "0",
                Color = Color.Green
            };
            if (scaling == 96) speed.Position = new Point((int)(screenWidth * 0.38f), (int)(screenHeight * 0.865f));
            else speed.Position = new Point((int)(screenWidth * 0.397f), (int)(screenHeight * 0.906f));
            speed.changeFont(speedFont);

            km = new TgcText2D
            {
                Text = "km",
                Color = Color.Black
            };
            if (scaling == 96) km.Position = new Point((int)(screenWidth * 0.41f), (int)(screenHeight * 0.88f));
            else km.Position = new Point((int)(screenWidth * 0.431f), (int)(screenHeight * 0.927f));
            km.changeFont(kmFont);

            reloj = new TgcText2D
            {
                Text = formatTime(time),
                Color = Color.Black
            };
            reloj.Position = new Point(0, 0);
            reloj.changeFont(relojFont);

            // Fuentes para mostrar la munición y armas
            var actualWeaponFont = UtilMethods.createFont("Insanibc", 24);
            var ammoQuantityFont = UtilMethods.createFont("Insanibc", 22);

            actualWeapon = new TgcText2D
            {
                Text = "[ None ]",
                Color = Color.Black
            };
            if (scaling == 96) actualWeapon.Position = new Point(-(int)(screenWidth * 0.421f), (int)(screenHeight * 0.87f));
            else actualWeapon.Position = new Point(-(int)(screenWidth * 0.406f), (int)(screenHeight * 0.921f));
            actualWeapon.changeFont(actualWeaponFont);

            ammoQuantity = new TgcText2D
            {
                Text = "-",
                Color = Color.Black
            };
            if (scaling == 96) ammoQuantity.Position = new Point(-(int)(screenWidth * 0.3725f), (int)(screenHeight * 0.815f));
            else ammoQuantity.Position = new Point(-(int)(screenWidth * 0.345f), (int)(screenHeight * 0.856f));
            ammoQuantity.changeFont(ammoQuantityFont);

            border = new TgcText2D // El borde es para que tenga un color blanco de fondo para que se distinga más
            {
                Text = "-",
                Color = Color.White
            };
            if (scaling == 96) border.Position = new Point(-(int)(screenWidth * 0.3728f), (int)(screenHeight * 0.8125f));
            else border.Position = new Point(-(int)(screenWidth * 0.3453f), (int)(screenHeight * 0.8535f));
            border.changeFont(actualWeaponFont);

            //Fuente para TURBO
            var turboFont = UtilMethods.createFont("Speed", 30);

            turbo = new TgcText2D // El borde es para que tenga un color blanco de fondo para que se distinga más
            {
                Text = "TURBO MODE",
                Color = Color.Blue
            };
            turbo.Position = new Point(0, (int)(screenHeight * 0.15f));
            turbo.changeFont(turboFont);
        }

        private string formatTime(float time)
        {
            int trunkTime = (int)time;
            int minutos = trunkTime / 60;
            int segundos = trunkTime % 60;
            string minString = minutos.ToString();
            string segString = segundos.ToString();
            if (segundos < 10)
                segString = "0" + segString;
            return $"{minString}:{segString}";
        }

    }

}
