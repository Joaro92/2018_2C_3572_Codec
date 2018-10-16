using Microsoft.DirectX.DirectSound;
using TGC.Core.Sound;

namespace TGC.Group.Model
{
    public class SoundManager
    {
        private Device device;

        public TgcMp3Player Mp3Player { get; private set; }
        private string currentMp3File = null;

        private TgcStaticSound sound;
        private string currentSoundFile = null;

        public SoundManager(Device device)
        {
            this.device = device;
            Mp3Player = new TgcMp3Player();

        }

        public void LoadMp3(string fileName)
        {
            if (currentMp3File == null || currentMp3File != fileName)
            {
                currentMp3File = fileName;

                //Cargar archivo
                Mp3Player.closeFile();
                Mp3Player.FileName = Game.Default.MediaDirectory + Game.Default.MusicDirectory + currentMp3File;
            }
        }

        public void PlaySound(string fileName)
        {
            if (currentSoundFile == null || currentSoundFile != fileName)
            {
                currentSoundFile = Game.Default.MediaDirectory + Game.Default.FXDirectory + fileName;

                //Borrar sonido anterior
                if (sound != null)
                {
                    sound.dispose();
                    sound = null;
                }

                //Cargar sonido
                sound = new TgcStaticSound();

                sound.loadSound(currentSoundFile, device);

                sound.play();
            }
        }

        public void Dispose()
        {
            if (sound != null)
                sound.dispose();
        }
    }
}