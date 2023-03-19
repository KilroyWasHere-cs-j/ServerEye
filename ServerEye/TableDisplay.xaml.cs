using System.Data;
using System.Windows;

namespace ServerEye
{
    /// <summary>
    /// Interaction logic for TableDisplay.xaml
    /// </summary>
    public partial class TableDisplay : Window
    {
        public TableDisplay(DataTable table)
        {
            InitializeComponent();
            display.DataContext = table.DefaultView;
        }
    }
}
