using System.Windows.Forms;

namespace CoopView.Controls
{
    /// <summary>
    /// Control to let the user select host or client mode.
    /// </summary>
    /// <seealso cref="Control" />
    public class ModeSelectionControl : Control
    {
        private Control hostControl;
        private Control clientControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModeSelectionControl"/> class.
        /// </summary>
        /// <param name="hostControl">The host control.</param>
        /// <param name="clientControl">The client control.</param>
        public ModeSelectionControl(Control hostControl, Control clientControl)
        {
            this.hostControl = hostControl;
            this.clientControl = clientControl;
        }
    }
}
