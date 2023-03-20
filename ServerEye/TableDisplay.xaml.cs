using System.Data;
using System.Windows;
using System.Windows.Input;

namespace ServerEye
{
    /// <summary>
    /// Interaction logic for TableDisplay.xaml
    /// </summary>
    public partial class TableDisplay : Window
    {
        private LogManager logManager;

        public TableDisplay(DataTable table)
        {
            InitializeComponent();
            display.DataContext = table;
            logManager.Log("Table displayed");
        }

        private void bind_KeyPress(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Escape:
                    this.Close();
                    break;
                case Key.Space:
                    this.WindowState = WindowState.Minimized;
                    break; 
            }
        }
    }
}
