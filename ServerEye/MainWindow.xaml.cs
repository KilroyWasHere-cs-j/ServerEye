using System;
using System.Windows;
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
                    break;
                case false:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Red);
                    break;
            }
        }

        
        private void execute_Click(object sender, RoutedEventArgs e)
        {
            azureConnectionManager.Connect();
            azureConnectionManager.GetMatchData();
        }
        #endregion
    }
}
