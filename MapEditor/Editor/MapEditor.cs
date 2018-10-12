using MapEditor.Engine;
using MapEditor.Repository;

namespace MapEditor.Editor
{
    public class MapEditor : Map
    {
        public MapEditor(MessageHub messageHub, ISession session, int width, int height)
            : base(messageHub, session, width, height)
        {
        }

        public MapEditor(MessageHub messageHub, ISession session, MapSettings settings)
            : base(messageHub, session, settings)
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
