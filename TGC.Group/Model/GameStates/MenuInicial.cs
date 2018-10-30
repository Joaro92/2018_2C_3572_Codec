using TGC.Core.Direct3D;
using TGC.Core.Mathematica;
using TGC.Examples.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Core.Text;
using System.Drawing;
using TGC.Group.Utils;
using TGC.Group.Model.TGCUtils;
using TGC.Group.Model.Interfaces;
using TGC.Core.Sound;
using Button = TGC.Group.Model.Input.Button;
using Dpad = TGC.Group.Model.Input.Dpad;

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

        private readonly string mainMenuSong = "Twisted Metal Black - Main Menu Theme.mp3";

        private bool isNextState = false;
        private bool isExit = false;

        public MenuInicial(GameModel gameModel)
        {
            this.gameModel = gameModel;
            drawer2D = new Drawer2D();

            //Leo las dimensiones de la ventana
            var screenHeight = D3DDevice.Instance.Device.Viewport.Height;
            var screenWidth = D3DDevice.Instance.Device.Viewport.Width;
            Size maxSize = new Size(1920, 1017);

            var imgDir = Game.Default.MediaDirectory + Game.Default.ImagesDirectory;

            // Cargo la imagen de fondo
            background = new CustomSprite();
            background.Bitmap = new CustomBitmap(imgDir + "start-screen.png", D3DDevice.Instance.Device);
            // La ubico ocupando toda la pantalla
            var scalingFactorX = (float)screenWidth / (float)background.Bitmap.Width;
            var scalingFactorY = (float)screenHeight / (float)background.Bitmap.Height;
            background.Scaling = new TGCVector2(scalingFactorX, scalingFactorY);

            //Cargo el mensaje de start
            start = new CustomSprite();
            start.Bitmap = new CustomBitmap(imgDir + "press-start.png", D3DDevice.Instance.Device);
            //La ubico en la pantalla
            start.Scaling = new TGCVector2(start.Scaling.X * 0.55f * (screenWidth / (float)maxSize.Width), start.Scaling.Y * 0.55f * (screenHeight / (float)maxSize.Height));
            start.Position = new TGCVector2(screenWidth * 0.159f , screenHeight * 0.61f);

            //Cargo el menu
            
            var menuFont = UtilMethods.createFont("Twisted Stallions", (int)(75 * (screenHeight / (float)maxSize.Height)));

            //Play
            play = new TgcText2D
            {
                Text = "Play",
                Color = Color.Silver,
                Position = new Point(-(int)(screenWidth * 0.194f), (int)(screenHeight * 0.52f))
            };
            play.changeFont(menuFont);
            //Controls
            controls = new TgcText2D
            {
                Text = "Controls",
                Color = Color.Silver,
                Position = new Point(-(int)(screenWidth * 0.194f), (int)(screenHeight * 0.52f) + (int)(0.147f * screenHeight))
            };
            controls.changeFont(menuFont);
            //Exit
            exit = new TgcText2D
            {
                Text = "Exit",
                Color = Color.Silver,
                Position = new Point(-(int)(screenWidth * 0.194f), (int)(screenHeight * 0.52f) + (int)(0.147f * 2 * screenHeight))
            };
            exit.changeFont(menuFont);

            this.gameModel.Camara = new TgcThirdPersonCamera();

            var sm = gameModel.SoundManager;
            sm.LoadMp3(mainMenuSong);
            if(sm.Mp3Player.getStatus() != TgcMp3Player.States.Playing)
            {
                sm.Mp3Player.play(true);
            }
        }

        public void Update()
        {
            var Input = gameModel.Input;
            var sm = gameModel.SoundManager;

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

                if (Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START) || Input.buttonPressed(Button.X))
                {
                    frecStart *= 2;
                    sm.PlaySound("menuEnter.wav");
                    timerStartFlag = true;
                }
            }
            else
            {
                if (Input.keyPressed(Key.DownArrow) || Input.buttonPressed(Dpad.DOWN))
                {
                    selectedOption = options.getNextOption(selectedOption);
                    sm.PlaySound("menuRight.wav");
                }

                if (Input.keyPressed(Key.UpArrow) || Input.buttonPressed(Dpad.UP))
                {
                    selectedOption = options.getNextOption(selectedOption, -1);
                    sm.PlaySound("menuLeft.wav");
                }

                if (Input.keyPressed(Key.Return) || Input.buttonPressed(Button.START) || Input.buttonPressed(Button.X))
                {
                    sm.PlaySound("menuEnter.wav");
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
            {
                gameModel.GameState = new SelectorVehiculo(gameModel);
                this.Dispose();
            }
        }

        public void Dispose()
        {
            background.Dispose();
            start.Dispose();
            play.Dispose();
            controls.Dispose();
            exit.Dispose();
        }

        
    }
}
