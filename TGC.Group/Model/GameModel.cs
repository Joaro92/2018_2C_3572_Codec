using TGC.Core.Example;

using TGC.Group.Model.GameStates;
using TGC.Group.Form;
using System.Windows.Forms;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
    {
        public IGameState GameState { get ; set ; }

        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = "Idea seleccionada: Twisted Metal - Derby de demolición";
        }

        public override void Init()
        {
            GameState = new MenuInicial(this);
        }

        public override void Update()
        {
            PreUpdate();

            GameState.Update();

            PostUpdate();
        }

        public override void Render()
        {
            PreRender();

            GameState.Render();
          
            PostRender();
        }

        public override void Dispose()
        {
            GameState.Dispose();
        }

        public void Exit()
        {
            var formEnumerator = Application.OpenForms.GetEnumerator();
            formEnumerator.MoveNext();
            var gameForm = (GameForm) formEnumerator.Current;
            gameForm.ShutDown();
            gameForm.Close();
        }
    }
}