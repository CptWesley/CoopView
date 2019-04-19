using System.Windows.Forms;

namespace CoopView.Forms
{
    /// <summary>
    /// Main form of the application used to render everything.
    /// </summary>
    /// <seealso cref="Form" />
    public class MainForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            SetTitle(null);
        }

        /// <summary>
        /// Sets the title of the window.
        /// </summary>
        /// <param name="title">New title of the window.</param>
        private void SetTitle(string title)
            => this.Text = string.IsNullOrWhiteSpace(title) ? "CoopView" : $"CoopView - {title}";
    }
}
