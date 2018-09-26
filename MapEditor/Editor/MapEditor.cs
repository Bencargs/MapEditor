using MapEditor.Engine;

namespace MapEditor.Editor
{
    public class MapEditor : Map
    {
        public MapEditor(MessageHub messageHub, IGraphics graphics, int width, int height)
            : base(messageHub, graphics, width, height)
        {
        }

        public MapEditor(MessageHub messageHub, IGraphics graphics, MapSettings settings)
            : base(messageHub, graphics, settings)
        {
        }

        public void ShowGrid(bool show)
        {
            Settings.ShowGrid = show;
        }

        public void ShowTerrain(bool show)
        {
            Settings.ShowTerrain = show;
        }
    }
}
