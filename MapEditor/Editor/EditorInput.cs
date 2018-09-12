using System.Windows.Forms;
using MapEditor.Engine;

namespace MapEditor.Editor
{
    public class EditorInput : IInputController
    {
        public void OnKeyboardEvent(KeyPressEventArgs e)
        {

        }

        public void OnMouseEvent(MouseEventArgs e)
        {
            var button = e.Button;
            if (button == MouseButtons.Right)
            {
                // create and post command
            }
            else if (button == MouseButtons.Left)
            {

            }
        }
    }
}
