﻿using NLog.Fluent;
using System;
using System.Data;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ServerEye
{
    public partial class MainWindow : Window
    {
        private enum AccessLevels { Admin, Lead, User, None};
        private AzureConnectionManager azureConnectionManager;
        private LogManager logManager;
        private DispatcherTimer timer;

        private TableDisplay tableDisplay;

        private AccessLevels accessLevel = AccessLevels.None;

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
                // Can't decide if we should keep this popup or not
                MessageBox.Show("Closing connection");
                e.Cancel = true;
                logManager.Log("App attempted to close with open connection. Closing prevented and connection automatically closed");
                azureConnectionManager.closeConnection();
            }
            else
            {
                e.Cancel = false;
                logManager.Log("Connection closing");
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
                File.WriteAllText(Directory.GetCurrentDirectory() + "/aaaBaba/data.html", Htmltext);
                return Htmltext;
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Failed to perpare reports \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void SendReports(DataTable pickList, DataTable amoryPick1, DataTable amoryPick2)
        {
            String SendMailFrom = "scouteyereports@gmail.com";
            String SendMailSubject = "Scouting reports at->" + DateTime.Now.ToString("h:mm:ss tt");
            try
            {
                String SendMailBody = "<h1>ScoutEye Reports<h1> <hr> <br> <h2>PickList<h2>" + DataTableToHTML(pickList) + "<br> <hr> <br <h2>Amory One seat pick<h2>" +
                    DataTableToHTML(amoryPick1) + "<br> <hr> <br <h1>Amory Two seat pick<h1>" + DataTableToHTML(amoryPick2) + "<br> <hr> <br <h2> Kilroy Was Here<h2>";
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage email = new MailMessage();
                // START
                email.From = new MailAddress(SendMailFrom);
                string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory().ToString() + "/aaaBaba/emailRecps.txt");
                foreach(string line in lines)
                {
                    email.To.Add(line);
                }
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
                MessageBox.Show("Email Successfully Sent");
            }
            catch(Exception ex)
            {
                logManager.Log(ex.Message); // A magical exception happens, but the email is still sent
                MessageBox.Show($"Failed to send report \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gets users level and sets the state of a public enum
        private AccessLevels RetriveLevel()
        {
            string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory().ToString() + "/aaaBaba/accessLevel.txt");
            foreach (string line in lines)
            {
                if (line.Contains("admin"))
                {
                    return AccessLevels.Admin;
                }
                else if (line.Contains("lead"))
                {
                    return AccessLevels.User;
                }
                else if (line.Contains("user"))
                {
                    return AccessLevels.User;
                }
            }
            return AccessLevels.None;
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
                    try
                    {
                        SoundPlayer player = new SoundPlayer(Properties.Resources.ping_82822);
                        player.Play();
                    }
                    catch (Exception ex) 
                    {
                        logManager.Log(ex.Message);
                    }
                    SafeBTN.IsEnabled = true;
                    break;
                case false:
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Red);
                    try
                    {
                        SoundPlayer player = new SoundPlayer(Properties.Resources.ping_82822);
                        player.Play();
                    }
                    catch (Exception ex)
                    {
                        logManager.Log(ex.Message);
                    }
                    SafeBTN.IsEnabled = false;
                    break;
            }
            accessLevel = RetriveLevel();
            AccessLevelLB.Content = "Access Level: " + accessLevel.ToString();

            // If the value in the compID box isn't a number then don't enable the boxes
            try
            {
                Int32.Parse(CompIDTB.Text);
                switch (accessLevel)
                {
                    case AccessLevels.None:
                        foreach (UIElement element in Grid.Children)
                        {
                            if (element is Button)
                            {
                                Button button = (Button)element;
                                button.IsEnabled = false;
                            }
                        }
                        break;
                    case AccessLevels.User:
                        foreach (UIElement element in Grid.Children)
                        {
                            if (element is Button)
                            {
                                Button button = (Button)element;
                                button.IsEnabled = true;
                            }
                        }
                        InsertBTN.IsEnabled = false;
                        break;
                    case AccessLevels.Admin:
                        foreach (UIElement element in Grid.Children)
                        {
                            if (element is Button)
                            {
                                Button button = (Button)element;
                                button.IsEnabled = true;
                            }
                        }
                        break;
                }
            }
            catch
            {
                foreach (UIElement element in Grid.Children)
                {
                    if(element is Button)
                    {
                        Button button = (Button)element;
                        button.IsEnabled = false;
                    }
                }
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
            //Construct the parameters
            Parameters parameters = new Parameters();
            // Construct the stored procedure
            Stored stored = new Stored();
            // Name stored procedure
            stored.Name = "sp_Get_All_Match_Data";
            // Set the ID
            stored.cID = Int32.Parse(CompIDTB.Text);
            // Set the stored procedure parameter name 
            parameters.Name = "@CompetitionNumber";
            // Set stored procedure parameter value
            parameters.value = stored.cID;
            // Add the parameters to the stored procedure
            stored.Parameters = parameters;
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void generatePickList_Click(object sender, RoutedEventArgs e)
        {
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_MatchData_RetrieveAverageScores_Summed";
            stored.cID = Int32.Parse(CompIDTB.Text);
            parameters.Name = "@CompetitionNumber";
            parameters.value = stored.cID;
            stored.Parameters = parameters;
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
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
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_amory_first_pick";
            stored.cID = Int32.Parse(CompIDTB.Text);
            parameters.Name = "@CompetitionNumber";
            parameters.value = stored.cID;
            stored.Parameters = parameters;
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
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
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_MatchData_AmorySecondPick";
            stored.cID = Int32.Parse(CompIDTB.Text);
            parameters.Name = "@CompetitionNumber";
            parameters.value = stored.cID;
            stored.Parameters = parameters;
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void sendReports_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to send reports? This is a very noisy action", "Bubble bubble I'm a fishy", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            switch(result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        SoundPlayer sndplayr = new SoundPlayer(Properties.Resources.sonar_ping_95840);
                        sndplayr.Play();
                    }
                    catch (Exception ex)
                    {
                        logManager.Log(ex.Message);
                    }
                    break;
                case MessageBoxResult.No: break;
            }
        }

        private void structuredQuery_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void directQuery_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The further on the edge\r\nThe hotter the intensity\r\nHighway to the Danger Zone");
        }

        private void insert_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("The further on the edge\r\nThe hotter the intensity\r\nHighway to the Danger Zone");
        }

        private void scout_names_Click(object sender, RoutedEventArgs e)
        {
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_Get_Scouts";
            stored.cID = Int32.Parse(CompIDTB.Text);
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void get_all_teams_Click(object sender, RoutedEventArgs e)
        {
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_Get_All_Teams";
            stored.cID = Int32.Parse(CompIDTB.Text);
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void get_comp_desc_Click(object sender, RoutedEventArgs e)
        {
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_Get_CompDesc";
            stored.cID = Int32.Parse(CompIDTB.Text);
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteCleanProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void get_metrics_Click(object sender, RoutedEventArgs e)
        {
            Parameters parameters = new Parameters();
            Stored stored = new Stored();
            stored.Name = "sp_Get_Metrics";
            try
            {
                stored.cID = Int32.Parse(SpecialCaseTB.Text);
                parameters.Name = "@Year";
                parameters.value = stored.cID;
                stored.Parameters = parameters;
            }
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
            }
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    tableDisplay = new TableDisplay(ds.Tables[0]);
                    tableDisplay.Show();
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // An HTML file with all the comp data
        private void run_raw_Click(object sender, RoutedEventArgs e)
        {
            DataTable dataTable = new DataTable();
            //Construct the parameters
            Parameters parameters = new Parameters();
            // Construct the stored procedure
            Stored stored = new Stored();
            // Name stored procedure
            stored.Name = "sp_Get_All_Match_Data";
            // Set the ID
            stored.cID = Int32.Parse(CompIDTB.Text);
            // Set the stored procedure parameter name 
            parameters.Name = "@CompetitionNumber";
            // Set stored procedure parameter value
            parameters.value = stored.cID;
            // Add the parameters to the stored procedure
            stored.Parameters = parameters;
            try
            {
                if (azureConnectionManager.isConnected)
                {
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dataTable = ds.Tables[0];
                }
                else
                {
                    azureConnectionManager.Connect();
                    var adapter = azureConnectionManager.ExecuteProcedure(stored);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);
                    dataTable = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            DataTableToHTML(dataTable);
            MessageBox.Show("HTML generated it's in ServerEye/aaaBaba/data.html", "Don't drink water upside down");
        }
        #endregion
    }
}
