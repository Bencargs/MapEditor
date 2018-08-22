using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapEditor.Engine;

namespace MapEditor.Entities
{
    public class UnitController
    {
        private readonly List<Entity> _units;
        //private List<Entity> _activeUnits;
        private readonly IGraphics _graphics;

        public UnitController(IGraphics graphics)
        {
            _graphics = graphics;
            _units = new List<Entity>();
        }

        //todo: refactor
        //public void CreateUnit(int x, int y)
        //{
        //    var images = new List<Image>
        //    {
        //        Image.FromFile(@"C:\Source\MapEditor\MapEditor\Entities\Unit.png"),
        //        Image.FromFile(@"C:\Source\MapEditor\MapEditor\Entities\Unit1.png")
        //    };

        //    var newUnit = new IdleUnit 
        //    {
        //        Position = new Point(x, y),
        //        Animation = new Animation(images)
        //    };
        //    _units.Add(newUnit);
        //}

        //public void Update()
        //{
            
        //}

        //public void Render()
        //{
        //    foreach (var u in _units)
        //    {
        //        u.Render(_graphics);
        //    }
        //}
    }
}
