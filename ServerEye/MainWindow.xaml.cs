﻿using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
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
                logManager.Log("App attemped to close with open connection. Closing prevented and connection automatically closed");
                azureConnectionManager.closeConnection();
            }
            else
            {
                e.Cancel = false;
                logManager.Log("Connection closeing");
            }
        }

        // Disconnected the connection if it's open
        private void MakeSafe()
        {
            if(azureConnectionManager.isConnected)
            {
                azureConnectionManager.closeConnection();
                logManager.Log("System safe");
            }
        }

        private String DataTableToHTML(DataTable dt) 
        {
            try
            {
                StringBuilder strHTMLBuilder = new StringBuilder();
                strHTMLBuilder.Append("<html >");
                strHTMLBuilder.Append("<head>");
                strHTMLBuilder.Append("</head>");
                strHTMLBuilder.Append("<body>");
                strHTMLBuilder.Append("<table border='1px' cellpadding='1' cellspacing='1' bgcolor='lightyellow' style='font-family:Garamond; font-size:smaller'>");
                strHTMLBuilder.Append("<tr >");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    strHTMLBuilder.Append("<td >");
                    strHTMLBuilder.Append(myColumn.ColumnName);
                    strHTMLBuilder.Append("</td>");
                }
                strHTMLBuilder.Append("</tr>");
                foreach (DataRow myRow in dt.Rows)
                {
                    strHTMLBuilder.Append("<tr >");
                    foreach (DataColumn myColumn in dt.Columns)
                    {
                        strHTMLBuilder.Append("<td >");
                        strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                        strHTMLBuilder.Append("</td>");
                    }
                    strHTMLBuilder.Append("</tr>");
                }
                //Close tags.
                strHTMLBuilder.Append("</table>");
                strHTMLBuilder.Append("</body>");
                strHTMLBuilder.Append("</html>");
                string Htmltext = strHTMLBuilder.ToString();
                //MessageBox.Show(Htmltext);
                //File.WriteAllText(Directory.GetCurrentDirectory() + "/data.html", Htmltext);
                return Htmltext;
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Failed to perpare reports \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void SendReports(DataTable pickList, DataTable amoryPick1)
        {
            String SendMailFrom = "scouteyereports@gmail.com";
            String SendMailSubject = "Scouting reports at->" + DateTime.Now.ToString("h:mm:ss tt");
            try
            {
                String SendMailBody = "<h1>PickList<h1>" + DataTableToHTML(pickList) + "<br>" + "<h1>Amory One seat pick<h1>" + DataTableToHTML(amoryPick1);
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage email = new MailMessage();
                // START
                email.From = new MailAddress(SendMailFrom);
                email.To.Add("jwtower@gmail.com");
                email.To.Add("gabriel.tower@baxter-academy.org");
                email.CC.Add(SendMailFrom);
                email.Subject = SendMailSubject;
                email.Body = SendMailBody;
                email.IsBodyHtml = true;
                //END
                SmtpServer.Timeout = 5000;
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new NetworkCredential(SendMailFrom, "dsoiisecxpnsiupz");
                SmtpServer.Send(email);

                logManager.Log("Email Successfully Sent");
            }
            catch(Exception ex)
            {
                logManager.Log(ex.Message); // A magical exception happens, but the email is still sent
                MessageBox.Show($"Failed to send report \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            try
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
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void generatePickList_Click(object sender, RoutedEventArgs e)
        {
            try
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
                    DataTableToHTML(ds.Tables[0]);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void generateAmoryFirstPick_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void generateAmorySecondPick_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Yeah so... \n this query hasn't been added yet...");
        }

        private void sendReports_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet pickListDS = new DataSet();
                DataSet amoryFirstPickDS = new DataSet();
                if (azureConnectionManager.isConnected)
                {
                    var adpter = azureConnectionManager.GetPickList();
                    adpter.Fill(pickListDS);
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adpter = azureConnectionManager.GetPickList();
                    adpter.Fill(pickListDS);
                }
                if (azureConnectionManager.isConnected)
                {
                    var adpter = azureConnectionManager.GenerateAmoryFirstPick();
                    adpter.Fill(amoryFirstPickDS);
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adpter = azureConnectionManager.GenerateAmoryFirstPick();
                    adpter.Fill(amoryFirstPickDS);
                }
                SendReports(pickListDS.Tables[0], amoryFirstPickDS.Tables[0]);
            }
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Failed to compile reports \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
