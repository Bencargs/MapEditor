using Common;
using System;

namespace MapEngine
{
    public class Scene
    {
        private IGraphics _graphics;

        public Scene(IGraphics graphics)
        {
            _graphics = graphics;
        }

        public void Initialise()
        {
            var loader = new ResourceLoader();
            var animation = (WpfAnimation)loader.LoadAnimation(@"C:\Source\MapEditor\MapEngine\Content\water.gif");
            _animation = animation;
        }

        private DateTime _previous;
        public void Display()
        {
            const int MS_PER_UPDATE = 500;
            _previous = DateTime.Now;
            var lag = 0.0;
            var current = DateTime.Now;
            var elapsed = current - _previous;
            _previous = current;
            lag += elapsed.TotalMilliseconds;

            ProcessInput();
            while (lag >= MS_PER_UPDATE)
            {
                Update();
                lag -= MS_PER_UPDATE;
            }
            Render();
        }

        private void Update()
        {
            
        }

        private IAnimation _animation;
        private void Render()
        {
            _graphics.Clear();

            var image = _animation.Image;
            var area = new Common.Rectangle(0, 0, 0, 0);
            _graphics.DrawImage(image, area);
            area = new Common.Rectangle(252, 0, 0, 0);
            _graphics.DrawImage(image, area);
            area = new Common.Rectangle(0, 252, 0, 0);
            _graphics.DrawImage(image, area);
            area = new Common.Rectangle(252, 252, 0, 0);
            _graphics.DrawImage(image, area);
            area = new Common.Rectangle(504, 0, 0, 0);
            _graphics.DrawImage(image, area);
            area = new Common.Rectangle(504, 252, 0, 0);
            _graphics.DrawImage(image, area);

            _graphics.Render();
        }

        private void ProcessInput()
        {

        }
    }
}
