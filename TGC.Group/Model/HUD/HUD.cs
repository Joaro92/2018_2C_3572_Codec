using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Geometry;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.World;
using TGC.Group.Utils;

namespace TGC.Group.Model
{
    public class HUD
    {
        Device device = D3DDevice.Instance.Device;
        Viewport view = new Viewport();
        Viewport original_view;

        private Player1 player1;
        private TgcMesh mesh, background;
        private float time;
        private Drawer2D drawer2D;
        private CustomSprite statsBar, healthBar, specialBar, weaponsHud;
        private CustomSprite[] weapons;
        private TGCVector2 specialScale, hpScale;
        private TgcText2D speed, km, weaponName, ammoQuantity, reloj, turbo;
        private readonly int scaling = GameModel.GetWindowsScaling(); // 96 es el 100%, 120 es el 125%
        private readonly int screenHeight = D3DDevice.Instance.Device.Viewport.Height;
        private readonly int screenWidth = D3DDevice.Instance.Device.Viewport.Width;

        public HUD(Player1 p1, float time)
        {
            player1 = p1;

            var loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + "Items\\power-item-TgcScene.xml").Meshes[0];

            var boxRadius = new TGCVector3(8.05f, 13, 0.1f);
            var tgcBox = TGCBox.fromSize(boxRadius, Color.FromArgb(255, 211, 206, 170));
            background = tgcBox.ToMesh("Background");
            tgcBox.Dispose();
            background.AutoTransform = true;

            mesh.Position = new TGCVector3(144f, -4.5f, 5f);
            mesh.Scale = new TGCVector3(2, 1.91f, 2);
            background.Position = new TGCVector3(144f, -4.5f, 7f);

            original_view = device.Viewport;

            view.X = 0;
            view.MinZ = 0;
            view.MaxZ = 1;

            if (scaling == 96)
            {
                view.Y = (int)(screenHeight * 0.7675f);
                view.Width = (int)(screenWidth * 0.132f);
                view.Height = (int)(screenHeight * 0.143f);
            }
            else
            {
                view.Y = (int)(screenHeight * 0.725f);
                view.Width = (int)(screenWidth * 0.16f);
                view.Height = (int)(screenHeight * 0.174f);
            }
            
            InitializeHUDSprites();
            InitializeHUDTexts(time);
        }

        public void Update(float matchTime)
        {
            // Actualizamos la barra de especial
            specialBar.Scaling = new TGCVector2(specialScale.X * (player1.specialPoints / player1.maxSpecialPoints), specialScale.Y);
            healthBar.Scaling = new TGCVector2(hpScale.X * (player1.hitPoints / player1.maxHitPoints), hpScale.Y);

            // Actualizamos velocidad actual y el hud de armas
            speed.Text = player1.currentSpeed.ToString();

            var selectedWeapon = player1.SelectedWeapon;
            if (selectedWeapon != null)
            {
                weaponName.Text = player1.SelectedWeapon.Name;
                ammoQuantity.Text = player1.SelectedWeapon.Ammo.ToString();
            }
            else
            {
                weaponName.Text = "[ None ]";
                ammoQuantity.Text = "-";

            }
            //border.Text = ammoQuantity.Text;

            // Actualizamos el reloj
            reloj.Text = "Time: " + formatTime(matchTime);
        }

        public void Render(GameModel gameModel)
        {
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(statsBar);
            drawer2D.DrawSprite(healthBar);
            drawer2D.DrawSprite(specialBar);
            drawer2D.DrawSprite(weaponsHud);
            drawer2D.EndDrawSprite();

            device.Viewport = view;

            var posOriginal = mesh.Position;
            var cam = (TgcThirdPersonCamera)gameModel.Camara;
            time += gameModel.ElapsedTime;
            var asd = gameModel.Camara.LookAt - gameModel.Camara.Position;
            asd.Normalize();
            asd *= 6;

            background.Position = gameModel.Camara.Position + asd * 1.12f;
            background.Rotation = new TGCVector3(-0.05f, cam.RotationY, 0);
            background.Render();

            if (player1.SelectedWeapon != null)
            {
                mesh.Position = gameModel.Camara.Position + asd * 0.8f;
                mesh.Rotation = new TGCVector3(0, cam.RotationY, 0);
                mesh.RotateY(FastMath.Cos(time * 3));
                mesh.Render();
            }

            device.Viewport = original_view;


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

            weaponName.render();
            //border.render();
            ammoQuantity.render();
        }

        public void Dispose()
        {
            statsBar.Dispose();
            healthBar.Dispose();
            specialBar.Dispose();
            weaponsHud.Dispose();
            //foreach (CustomSprite w in weapons)
            //{
            //    w.Dispose();
            //}
            weaponName.Dispose();
            ammoQuantity.Dispose();
            //border.Dispose();
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
            };
            if (scaling == 96) statsBar.Position = new TGCVector2(screenWidth * 0.842f, screenHeight * 0.750f); // 100%
            else statsBar.Position = new TGCVector2(screenWidth * 0.81f, screenHeight * 0.695f); // 125%

            var scalingFactorX = (float)screenWidth / (float)statsBar.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)statsBar.Bitmap.Height;

            statsBar.Scaling = new TGCVector2(0.25f, 0.42f) * (scalingFactorY / scalingFactorX);

            // Sprite que representa la vida
            healthBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "healthBar.png", D3DDevice.Instance.Device)
            };
            if (scaling == 96) healthBar.Position = new TGCVector2(screenWidth * 0.8828f, screenHeight * 0.7762f); // 100%
            else healthBar.Position = new TGCVector2(screenWidth * 0.8605f, screenHeight * 0.728f); // 125%

            scalingFactorX = (float)screenWidth / (float)healthBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)healthBar.Bitmap.Height;

            healthBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            hpScale = healthBar.Scaling;

            // Sprite de la barra de especiales
            specialBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "specialBar.png", D3DDevice.Instance.Device)
            };
            if (scaling == 96) specialBar.Position = new TGCVector2(screenWidth * 0.883f, screenHeight * 0.858f); // 100%
            else specialBar.Position = new TGCVector2(screenWidth * 0.861f, screenHeight * 0.83f); // 125%

            scalingFactorX = (float)screenWidth / (float)specialBar.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)specialBar.Bitmap.Height;

            specialBar.Scaling = new TGCVector2(0.079f, 0.08f) * (scalingFactorY / scalingFactorX);
            specialScale = specialBar.Scaling;

            // Sprite del HUD de las armas
            weaponsHud = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "weapons-hud-2.png", D3DDevice.Instance.Device),
            };
            if (scaling == 96) weaponsHud.Position = new TGCVector2(-13, screenHeight * 0.703f); // 100%
            else weaponsHud.Position = new TGCVector2(-15, screenHeight * 0.64f); // 125%

            scalingFactorX = (float)screenWidth / (float)weaponsHud.Bitmap.Width;
            scalingFactorY = (float)screenHeight / (float)weaponsHud.Bitmap.Height;

            weaponsHud.Scaling = new TGCVector2(0.6f, 0.6f) * (scalingFactorY / scalingFactorX);

            //// Sprites de armas
            //var weaponNames = Game.Default.Weapons;
            //var cant = weaponNames.Count;
            //weapons = new CustomSprite[cant];
            //for (int i = 0; i < cant; i++)
            //{
            //    weapons[i] = new CustomSprite
            //    {
            //        Bitmap = new CustomBitmap(imgDir + weaponNames[i].ToLower() + ".png", D3DDevice.Instance.Device),
            //        Position = new TGCVector2(screenWidth * 0.04f, screenHeight * 0.7f)
            //    };

            //    scalingFactorX = (float)screenWidth / (float)weapons[i].Bitmap.Width;
            //    scalingFactorY = (float)screenHeight / (float)weapons[i].Bitmap.Height;

            //    weapons[i].Scaling = new TGCVector2(1f, 1f) * (scalingFactorY / scalingFactorX);

            //}


        }

        private void InitializeHUDTexts(float time)
        {
            // Fuente para mostrar la velocidad y el reloj
            var speedFont = UtilMethods.createFont("Open 24 Display St", 32);
            var kmFont = UtilMethods.createFont("Open 24 Display St", 20);
            var relojFont = UtilMethods.createFont("appleberry", 40);

            speed = new TgcText2D
            {
                Text = "0",
                Color = Color.Green
            };
            if (scaling == 96) speed.Position = new Point((int)(screenWidth * 0.412f), (int)(screenHeight * 0.9195f)); // 100%
            else speed.Position = new Point((int)(screenWidth * 0.397f), (int)(screenHeight * 0.906f)); // 125%
            speed.changeFont(speedFont);

            km = new TgcText2D
            {
                Text = "km",
                Color = Color.Black
            };
            if (scaling == 96) km.Position = new Point((int)(screenWidth * 0.442f), (int)(screenHeight * 0.936f)); // 100%
            else km.Position = new Point((int)(screenWidth * 0.431f), (int)(screenHeight * 0.927f)); // 125%
            km.changeFont(kmFont);

            reloj = new TgcText2D
            {
                Text = formatTime(time),
                Color = Color.Black
            };
            reloj.Position = new Point(0, 0);
            reloj.changeFont(relojFont);

            // Fuentes para mostrar la munición y armas
            //var borderFont = UtilMethods.createFont("Insanibc", 26);
            var actualWeaponFont = UtilMethods.createFont("Insanibc", 23);
            var ammoQuantityFont = UtilMethods.createFont("Insanibc", 26);

            weaponName = new TgcText2D
            {
                Text = "[ None ]",
                Color = Color.Black
            };
            if (scaling == 96) weaponName.Position = new Point(-(int)(screenWidth * 0.423f), (int)(screenHeight * 0.932f)); // 100%
            else weaponName.Position = new Point(-(int)(screenWidth * 0.406f), (int)(screenHeight * 0.921f)); // 125%
            weaponName.changeFont(actualWeaponFont);

            ammoQuantity = new TgcText2D
            {
                Text = "-",
                Color = Color.Black
            };
            ammoQuantity.Align = TgcText2D.TextAlign.CENTER;
            if (scaling == 96) ammoQuantity.Position = new Point(-(int)(screenWidth * 0.3727f), (int)(screenHeight * 0.8724f)); // 100%
            else ammoQuantity.Position = new Point(-(int)(screenWidth * 0.345f), (int)(screenHeight * 0.856f)); // 125%
            ammoQuantity.changeFont(ammoQuantityFont);

            //border = new TgcText2D // El borde es para que tenga un color blanco de fondo para que se distinga más
            //{
            //    Text = "-",
            //    Color = Color.White
            //};
            //border.Align = TgcText2D.TextAlign.CENTER;
            //if (scaling == 96) border.Position = new Point(-(int)(screenWidth * 0.3727f), (int)(screenHeight * 0.8727f)); // 100%
            //else border.Position = new Point(-(int)(screenWidth * 0.3453f), (int)(screenHeight * 0.8535f)); // 125%
            //border.changeFont(borderFont);

            //Fuente para TURBO
            var turboFont = UtilMethods.createFont("Speed", 30);

            turbo = new TgcText2D
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
