using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ServerEye
{
    public partial class MainWindow : Window
    {
        private enum ConnectionStatus { Connected, Disconnected };
        private AzureConnectionManager azureConnectionManager;
        private LogManager logManager;
        private DispatcherTimer timer;

        private TableDisplay tableDisplay;

        public MainWindow()
        {
            InitializeComponent();
            logManager = new LogManager("logs/main_log.txt");
            azureConnectionManager = new AzureConnectionManager();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        //<summary>
        // If a server connection is open, the app will not close until the connection is closed
        // <summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Needs to check if it's safe to close
            if (azureConnectionManager.isConnected)
            {
                // Can't deside if we should keep this popup or not
                MessageBox.Show("Closing connection");
                e.Cancel = true;
                logManager.Log("App trying to close with open connection");
                azureConnectionManager.closeConnection();
            }
            else
            {
                e.Cancel = false;
                logManager.Log("Closing");
            }
        }

        // Disconnected the connection if it's open
        private void MakeSafe()
        {
            if(azureConnectionManager.isConnected)
            {
                azureConnectionManager.closeConnection();
            }
        }

        #region Event Handlers
        //<summary>
        // Watchdog to update UI
        //<summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (azureConnectionManager.isConnected)
            {
                case true:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Green);
                    SafeBTN.IsEnabled = true;
                    break;
                case false:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Red);
                    SafeBTN.IsEnabled = false;
                    break;
            }
        }

        private void bind_KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                    MakeSafe();
                    break;
            }
        }   
        private void safe_Click(object sender, RoutedEventArgs e)
        {
            MakeSafe();
        }

        private void pullDownAll_Click(object sender, RoutedEventArgs e)
        {
            if (azureConnectionManager.isConnected)
            {
                var adpter = azureConnectionManager.GetMatchData();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
            else
            {
                azureConnectionManager.Connect();
                var adpter = azureConnectionManager.GetMatchData();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
        }

        private void generatePickList_Click(object sender, RoutedEventArgs e)
        {
            if (azureConnectionManager.isConnected)
            {
                var adpter = azureConnectionManager.GetPickList();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
            else
            {
                azureConnectionManager.Connect();
                var adpter = azureConnectionManager.GetPickList();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
        }
        private void generateAmoryFirstPick_Click(object sender, RoutedEventArgs e)
        {
            if (azureConnectionManager.isConnected)
            {
                var adpter = azureConnectionManager.GenerateAmoryFirstPick();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
            else
            {
                azureConnectionManager.Connect();
                var adpter = azureConnectionManager.GenerateAmoryFirstPick();
                DataSet ds = new DataSet();
                adpter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
        }

        private void generateAmorySecondPick_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Yeah so... \n this query hasn't been added yet...");
        }
        #endregion
    }
}
