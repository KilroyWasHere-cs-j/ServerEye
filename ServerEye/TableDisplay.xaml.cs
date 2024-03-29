﻿using System.Data;
using System.Windows;
using System.Windows.Input;

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
            display.DataContext = table;
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
