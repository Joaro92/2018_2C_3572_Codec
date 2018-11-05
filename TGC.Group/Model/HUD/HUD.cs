using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Core.SceneLoader;
using TGC.Core.Text;
using TGC.Examples.Camara;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.World.Characters;
using TGC.Group.Utils;
using Font = System.Drawing.Font;

namespace TGC.Group.Model
{
    public class HUD
    {
        Device device = D3DDevice.Instance.Device;
        Viewport view = new Viewport();
        Viewport original_view;

        private Player1 player1;
        private TgcMesh mesh;
        private float time;
        private Drawer2D drawer2D;
        private CustomSprite hudSprites, healthBar, specialBar;
        private TGCVector2 specialScale, hpScale;
        private TgcText2D speed, km, weaponName, ammoQuantity, reloj, turbo;
        private readonly int screenHeight = D3DDevice.Instance.Device.Viewport.Height;
        private readonly int screenWidth = D3DDevice.Instance.Device.Viewport.Width;
 
        public HUD(Player1 p1, float time)
        {
            player1 = p1;


            var loader = new TgcSceneLoader();
            mesh = loader.loadSceneFromFile(Game.Default.MediaDirectory + "Items\\power-item-TgcScene.xml").Meshes[0];
            mesh.Position = new TGCVector3(144f, -4.5f, 5f);
            mesh.Scale = new TGCVector3(2, 1.88f, 2);

            original_view = device.Viewport;
            view.X = 0;
            view.MinZ = 0;
            view.MaxZ = 1;
            view.Y = (int)(screenHeight * 0.759f);
            view.Width = (int)(screenWidth * 0.140f);
            view.Height = (int)(screenHeight * 0.158f);
            
            InitializeHUDSprites();
            InitializeHUDTexts(time);
        }

        public void Update(float matchTime)
        {
            // Actualizamos la barra de especial
            specialBar.Scaling = new TGCVector2(specialScale.X * (player1.specialPoints / player1.maxSpecialPoints), specialScale.Y);
            healthBar.Scaling = new TGCVector2(hpScale.X * (player1.hitPoints / (float)player1.maxHitPoints), hpScale.Y);

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

            // Actualizamos el reloj
            reloj.Text = formatTime(matchTime);
        }

        public void Render(GameModel gameModel)
        {
            // Mini Viewport
            device.Viewport = view;

            var posOriginal = mesh.Position;
            var cam = (TgcThirdPersonCamera)gameModel.Camara;
            time += gameModel.ElapsedTime;
            var asd = gameModel.Camara.LookAt - gameModel.Camara.Position;
            asd.Normalize();
            asd *= 4.85f;

            if (player1.SelectedWeapon != null)
            {
                mesh.Position = gameModel.Camara.Position + asd;
                mesh.Rotation = new TGCVector3(0, cam.RotationY, 0);
                mesh.RotateY(FastMath.Cos(time * 3) * 1.3f);
                mesh.Render();
            }

            device.Viewport = original_view;

            // Dibujar los Sprites
            drawer2D.BeginDrawSprite();
            drawer2D.DrawSprite(hudSprites);
            drawer2D.DrawSprite(healthBar);
            drawer2D.DrawSprite(specialBar);
            drawer2D.EndDrawSprite();

            // Renderizar Texto
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
            weaponName.render();
            ammoQuantity.render();
            reloj.render();

            if (player1.turbo)
                turbo.render();
        }

        public void Dispose()
        {
            hudSprites.Dispose();
            healthBar.Dispose();
            specialBar.Dispose();
            weaponName.Dispose();
            ammoQuantity.Dispose();
            speed.Dispose();
            km.Dispose();
            reloj.Dispose();
            turbo.Dispose();
            mesh.Dispose();
        }

        //--------------------------------------------------------------------------------------------------//

        private void InitializeHUDSprites()
        {
            var imgDir = Game.Default.MediaDirectory + Game.Default.ImagesDirectory;
            int imgW, imgH;
            Size maxSize = new Size(1920, 1017);

            // Inicializamos la interface para dibujar sprites 2D
            drawer2D = new Drawer2D();

            // Sprite del HUD principal
            hudSprites = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "HUD.png", D3DDevice.Instance.Device),
                Position = TGCVector2.Zero
            };
            imgW = hudSprites.Bitmap.ImageInformation.Width;
            imgH = hudSprites.Bitmap.ImageInformation.Height;
            hudSprites.Scaling = new TGCVector2((screenWidth / (float)hudSprites.Bitmap.Width), (screenHeight / (float)hudSprites.Bitmap.Height));

            // Sprite que representa la vida
            healthBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "healthBar.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(screenWidth * 0.8802f, screenHeight * 0.7762f)
            };
            imgW = healthBar.Bitmap.ImageInformation.Width;
            imgH = healthBar.Bitmap.ImageInformation.Height;
            healthBar.Scaling = new TGCVector2(((imgW / (float)healthBar.Bitmap.Width)) * (screenWidth / (float)maxSize.Width), ((imgH / (float)healthBar.Bitmap.Height)) * (screenHeight / (float)maxSize.Height));
            hpScale = healthBar.Scaling;

            // Sprite de la barra de especiales
            specialBar = new CustomSprite
            {
                Bitmap = new CustomBitmap(imgDir + "specialBar.png", D3DDevice.Instance.Device),
                Position = new TGCVector2(screenWidth * 0.8805f, screenHeight * 0.8585f)
            };
            imgW = specialBar.Bitmap.ImageInformation.Width;
            imgH = specialBar.Bitmap.ImageInformation.Height;
            specialBar.Scaling = new TGCVector2(((imgW / (float)specialBar.Bitmap.Width)) * (screenWidth / (float)maxSize.Width), ((imgH / (float)specialBar.Bitmap.Height)) * (screenHeight / (float)maxSize.Height));
            specialScale = specialBar.Scaling;
        }

        private void InitializeHUDTexts(float time)
        {
            Font speedFont, kmFont, relojFont, actualWeaponFont, ammoQuantityFont, turboFont;
            var scale = screenWidth / 1920f;

            speedFont = UtilMethods.createFont("Open 24 Display St", (int)(32 * scale));
            kmFont = UtilMethods.createFont("Open 24 Display St", (int)(20 * scale));
            relojFont = UtilMethods.createFont("appleberry", (int)(40 * scale));
            actualWeaponFont = UtilMethods.createFont("Insanibc", (int)(23 * scale));
            ammoQuantityFont = UtilMethods.createFont("Insanibc", (int)(26 * scale));
            turboFont = UtilMethods.createFont("Speed", (int)(30 * scale));

            // Nombre del Arma
            weaponName = new TgcText2D
            {
                Text = "[ None ]",
                Color = Color.Black,
                Format = DrawTextFormat.Bottom | DrawTextFormat.Center,
                Position = new Point(-(int)(screenWidth * 0.4230f), -(int)(screenHeight * 0.0345f))
            };
            weaponName.changeFont(actualWeaponFont);

            // Cantidad de balas
            ammoQuantity = new TgcText2D
            {
                Text = "-",
                Color = Color.Black,
                Format = DrawTextFormat.Bottom | DrawTextFormat.Center,
                Position = new Point(-(int)(screenWidth * 0.3755f), -(int)(screenHeight * 0.0875f))
            };
            ammoQuantity.changeFont(ammoQuantityFont);

            // Velocidad del Player 1
            speed = new TgcText2D
            {
                Text = "0",
                Color = Color.Green,
                Format = DrawTextFormat.Bottom | DrawTextFormat.Center,
                Position = new Point((int)(screenWidth * 0.4105f), -(int)(screenHeight * 0.0310f))
            };
            speed.changeFont(speedFont);

            // KM
            km = new TgcText2D
            {
                Text = "km",
                Color = Color.Black,
                Format = DrawTextFormat.Bottom | DrawTextFormat.Center,
                Position = new Point((int)(screenWidth * 0.4405f), -(int)(screenHeight * 0.0320f))
            };
            km.changeFont(kmFont);

            // Reloj
            reloj = new TgcText2D
            {
                Text = formatTime(time),
                Color = Color.Black,
                Position = new Point(0, 0)
            };
            reloj.changeFont(relojFont);

            // Turbo
            turbo = new TgcText2D
            {
                Text = "TURBO MODE",
                Color = Color.Blue,
                Position = new Point(0, (int)(screenHeight * 0.15f))
            };
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
