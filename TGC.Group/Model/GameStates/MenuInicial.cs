using System.Linq;
using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Text;
using System.Drawing;
using System.Drawing.Text;
using TGC.Group.Utils;
using TGC.Group.Model.TGCUtils;


namespace TGC.Group.Model.GameStates
{
    public class MenuInicial : IGameState
    {
        private readonly MenuOption[] options = new MenuOption[] { MenuOption.PLAY, MenuOption.CONTROLS, MenuOption.EXIT };

        private GameModel gameModel;
        private Drawer2D drawer2D;
        private CustomSprite background;

        private CustomSprite start;
        private bool showStart = true;
        private float timerStart1 = 0f;
        private float timerStart2 = 0f;
        private bool timerStartFlag = false;
        private float frecStart = 5f; //frecuencia de parpadeo inicial del start (en el parpadeo final es el doble)
        private readonly float timeAfterStart = 0.75f; //tiempo en segundos de parpadeo final despues de presionar start

        private TgcText2D play;
        private TgcText2D controls;
        private TgcText2D exit;
        private bool showMenu = false;
        private MenuOption selectedOption = MenuOption.PLAY;

        private bool isNextState = false;
        private bool isExit = false;

        public MenuInicial(GameModel gameModel)
        {
            this.gameModel = gameModel;
            drawer2D = new Drawer2D();


            //Leo las dimensiones de la ventana
            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;
           
            // Cargo la imagen de fondo
            background = new CustomSprite();
            background.Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\start-screen.png", D3DDevice.Instance.Device);
            // La ubico ocupando toda la pantalla
            var scalingFactorX = (float)screenWidth / (float)background.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)background.Bitmap.Height;
            background.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);

            //Cargo el mensaje de start
            start = new CustomSprite();
            start.Bitmap = new CustomBitmap(gameModel.MediaDir + "Images\\press-start.png", D3DDevice.Instance.Device);
            //La ubico en la pantalla
            start.Scaling = TGCVector2.One * (scalingFactorY / scalingFactorX);
            start.Position = new TGCVector2(screenWidth * 0.083f , screenHeight * 0.67f);

            //Cargo el menu
            var pfc = new PrivateFontCollection();
            pfc.AddFontFile(gameModel.MediaDir + "Fonts\\Minecraft.ttf");
            FontFamily family = pfc.Families[0];
            var menuFont = new Font(family, 75);

            //Play
            play = new TgcText2D
            {
                Text = "Play",
                Color = Color.Silver,
                Position = new Point(-screenWidth / 4, screenHeight / 2),
            };
            play.changeFont(menuFont);
            //Controls
            controls = new TgcText2D
            {
                Text = "Controls",
                Color = Color.Silver,
                Position = new Point(-screenWidth / 4, screenHeight / 2 + 150)
            };
            controls.changeFont(menuFont);
            //Exit
            exit = new TgcText2D
            {
                Text = "Exit",
                Color = Color.Silver,
                Position = new Point(-screenWidth / 4, screenHeight / 2 + 300)
            };
            exit.changeFont(menuFont);

            this.gameModel.Camara = new TgcThirdPersonCamera();
        }

        public void Update()
        {
            if (!showMenu)
            {
                timerStart1 += gameModel.ElapsedTime;
                if (timerStartFlag)
                    timerStart2 += gameModel.ElapsedTime;


                if (timerStart1 >= (1 / frecStart))
                {
                    if (showStart)
                        showStart = false;
                    else
                        showStart = true;
                    timerStart1 = 0f;
                }
                if (timerStart2 >= timeAfterStart)
                {
                    showStart = false;
                    showMenu = true;
                }

                if (gameModel.Input.keyPressed(Key.Return) || gameModel.JoystickButtonPressed(7))
                {
                    frecStart *= 2;
                    timerStartFlag = true;
                }
            }
            else
            {
                if (gameModel.Input.keyPressed(Key.DownArrow) || gameModel.JoystickDpadPressed(JoystickDpad.DOWN))
                {
                    if (selectedOption.Equals(options.Last()) == false)
                        selectedOption = options.getNextOption(selectedOption);
                }

                if (gameModel.Input.keyPressed(Key.UpArrow) || gameModel.JoystickDpadPressed(JoystickDpad.UP))
                {
                    if (selectedOption.Equals(options.First()) == false)
                        selectedOption = options.getNextOption(selectedOption, -1);

                }

                if (gameModel.Input.keyPressed(Key.Return) || gameModel.JoystickButtonPressed(0))
                {
                    switch (selectedOption)
                    {
                        case MenuOption.PLAY:
                            isNextState = true;
                            break;
                        case MenuOption.CONTROLS:
                            break;
                        case MenuOption.EXIT:
                            isExit = true;
                            break;
                        default:
                            break;
                    }
                }

                switch (selectedOption)
                {
                    case MenuOption.PLAY:
                        play.Color = Color.Yellow;
                        controls.Color = Color.Silver;
                        exit.Color = Color.Silver;
                        break;
                    case MenuOption.CONTROLS:
                        play.Color = Color.Silver;
                        controls.Color = Color.Yellow;
                        exit.Color = Color.Silver;
                        break;
                    case MenuOption.EXIT:
                        play.Color = Color.Silver;
                        controls.Color = Color.Silver;
                        exit.Color = Color.Yellow;
                        break;
                    default:
                        break;
                }
            }
        }

        public void Render()
        {
            if (isExit)
            {
                gameModel.Exit();
                return;
            }

            // Iniciar dibujado de sprites
            drawer2D.BeginDrawSprite();

            // Dibujar sprites
            drawer2D.DrawSprite(background);
            if (showStart)
                drawer2D.DrawSprite(start);

            // Finalizar el dibujado de Sprites
            drawer2D.EndDrawSprite();

            if (showMenu)
            {
                play.render();
                controls.render();
                exit.render();
            }

            if (isNextState)
                gameModel.GameState = new Partida(gameModel);
        }

        public void Dispose()
        {
            background.Dispose();
            start.Dispose();
        }
    }
}
