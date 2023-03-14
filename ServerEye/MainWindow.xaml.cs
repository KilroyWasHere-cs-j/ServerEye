using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
            timer.Interval = TimeSpan.FromSeconds(1);
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

        //<summary>
        // Watchdog to update UI
        //<summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (azureConnectionManager.isConnected)
            {
                case true:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Green);
                    ConnectionStatusText.Text = ConnectionStatus.Connected.ToString();
                    break;
                case false:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Red);
                    ConnectionStatusText.Text = ConnectionStatus.Disconnected.ToString();
                    break;
            }
        }


        //TODO extract hashed password
        private string GetConnectionString()
        {
            string content = File.ReadAllText("cnnString.txt");
            MessageBox.Show(content);
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("hello world"));
                // Get the hashed string.  
                var hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                // Print the string.   
                return hash;
            }
        }
    }
}
