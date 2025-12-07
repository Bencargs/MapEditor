using System.Collections.Generic;
using System.IO;
using Common;
using MapEngine.Factories;
using MapEngine.ResourceLoading;

namespace MapEngine.Handlers
{
    public class CursorHandler
    {
        private readonly Dictionary<string, CursorDefinition> _cursors = new Dictionary<string, CursorDefinition>();
        private readonly InputState _inputState;

        public CursorHandler(InputState inputState)
        {
            _inputState = inputState;
        }

        public void Initialise(string cursorFolderPath)
        {
            foreach (var file in Directory.GetFiles(cursorFolderPath, "*.json"))
            {
                var cursor = CursorLoader.LoadCursor(file);
                _cursors[cursor.Name] = cursor;
            }
            
            var cursorPath = @"C:\src\MapEditor\MapEngine\Content\Cursors";
            TextureFactory.LoadTextures(cursorPath, 120);
        }

        public void Render(Rectangle viewport, IGraphics graphics)
        {
            if (!viewport.Contains(_inputState.Location))
                return;

            var cursor = GetCursor();

            if (TextureFactory.TryGetTexture(cursor.Name, out var texture))
            {
                var screenX = (int)(_inputState.Location.X - cursor.Hotspot.X);
                var screenY = (int)(_inputState.Location.Y - cursor.Hotspot.Y);
                var area = new Rectangle(screenX, screenY, texture.Width, texture.Height);
                graphics.DrawImage(texture.Image, area);
            }
        }

        private CursorDefinition GetCursor()
        {
            var cursor = _cursors["normal"];
            if (_inputState.HoveredEntity != null &&
                _inputState.CurrentCommand == InputState.Command.None)
            {
                cursor = _cursors["select"];
            }
            else
            {
                switch (_inputState.CurrentCommand)
                {
                    case InputState.Command.Unload:
                        cursor = _cursors["unload"];
                        break;
                }
            }

            return cursor;
        }
    }
}