using System;
using MapEditor.Handlers;

namespace MapEditor.Engine
{
    public class Game
    {
        public bool Running { get; set; } = true;
        private readonly MapHandler _mapHandler;

        public Game(IGraphics graphics)
        {
            //var messageHub = new MessageHub();
            //_map = new Map(messageHub, graphics, 50, 100)
            //{
            //    //ShowGrid = gridChk.Checked
            //};
        }

        public void Init()
        {
            _mapHandler.Init();
        }

        public const double MsPerUpdate = 10.0;
        public void Update()
        {
            var previous = DateTime.Now;
            var lag = 0.0;
            while (!Running)
            {

                var current = DateTime.Now;
                var elapsed = current - previous;
                lag += elapsed.TotalMilliseconds;

                //ProcessInput();

                while (lag >= MsPerUpdate)
                {
                    //Update(elapsed);
                    lag -= MsPerUpdate;
                }

                //Render();
            }
        }
    }
}
