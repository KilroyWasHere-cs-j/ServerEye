using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ServerEye
{
    public partial class MainWindow : Window
    {
        private enum AccessLevels { Admin, Lead, User, None };
        private AzureConnectionManager azureConnectionManager;
        private LogManager logManager;
        private DispatcherTimer timer;
        private OpenFileDialog fdlg;
        private TableDisplay tableDisplay;

        private AccessLevels accessLevel = AccessLevels.None;

        public MainWindow()
        {
            InitializeComponent();
            logManager = new LogManager("logs/main_log.txt");
            azureConnectionManager = new AzureConnectionManager();
            fdlg = new OpenFileDialog();
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
            timer.Start();

            fdlg.Title = "Server Insert Tool";
            fdlg.Filter = "CSV Files (*.csv*)|*.csv*|All files (*.csv*)|*.csv*"; //Only allow xml files to be shown, which is the file used for configuration
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            int random = new Random().Next(0, 100);
        }

        private void playMissile()
        {
            SoundPlayer player = new SoundPlayer(Properties.Resources.missile);
            player.PlaySync();
        }

        private void playAuto()
        {
            SoundPlayer player = new SoundPlayer(Properties.Resources.autopilot);
            player.PlaySync();
        }

        /// <summary>
        /// Close connection if it's open
        /// </summary>
        private void MakeSafe()
        {
            if (azureConnectionManager.isConnected)
            {
                Thread player = new Thread(playAuto);
                player.Start();
                azureConnectionManager.closeConnection();
                logManager.Log("System safe");
            }
        }
        /// <summary>
        /// Converts a DataTable object to raw HTML
        /// </summary>
        /// <remarks>Returned HTML has minimal formatting</remarks>
        /// <param name="dt">Datatable</param>
        /// <returns>HTML file as a string</returns>
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
        /// <summary>
        /// Sends the reports to the email addresses in the emailRecps.txt file
        /// </summary>
        /// <param name="tables">List of datatables</param>
        private void SendReports(List<DataTable> tables)
        {
            String SendMailFrom = "scouteyereports@gmail.com";
            String SendMailSubject = "Scouting reports at->" + DateTime.Now.ToString("h:mm:ss tt");
            try
            {
                String SendMailBody = "<h1>ScoutEye Reports<h1> <hr> <br>";
                // Iterates through the list of tables and adds them to the email body
                foreach(DataTable table in tables)
                {
                    SendMailBody += DataTableToHTML(table);
                    SendMailBody += "<br> <hr>";
                }
                // Configure the SMTP client
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com", 587);
                SmtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage email = new MailMessage();
                // START
                email.From = new MailAddress(SendMailFrom);
                // Reads the email addresses from the emailRecps.txt file
                string[] lines = File.ReadAllLines(Directory.GetCurrentDirectory().ToString() + "/aaaBaba/emailRecps.txt");
                // Adds the email addresses to the email
                foreach (string line in lines)
                {
                    email.To.Add(line);
                }
                // Sets up basic email information
                email.CC.Add(SendMailFrom);
                email.Subject = SendMailSubject;
                email.Body = SendMailBody;
                email.IsBodyHtml = true;
                //END
                // Configures server connection and security
                SmtpServer.Timeout = 5000;
                SmtpServer.EnableSsl = true;
                SmtpServer.UseDefaultCredentials = false;
                SmtpServer.Credentials = new NetworkCredential(SendMailFrom, "dsoiisecxpnsiupz");
                // Sends the email
                SmtpServer.Send(email);
                // Update user on success
                logManager.Log("Email Successfully Sent");
                MessageBox.Show("Email Successfully Sent");
            }
            catch (Exception ex)
            {
                // Update user on failure
                logManager.Log(ex.Message); // A magical exception happens, but the email is still sent
                MessageBox.Show($"Failed to send report \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets and sets the access level of the user from the /aaaBaba/accessLevel.txt file
        /// </summary>
        /// <returns>AccessLevels enum</returns>
        private AccessLevels RetriveLevel()
        {
            try
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
            catch(Exception ex)
            {
                MessageBox.Show("Can not unlock UI" + ex.Message.ToString(), "Fatal... this is serious");
                logManager.Log(ex.Message);
                return AccessLevels.None;
            }
        }
        /// <summary>
        /// Coverts a CSV file to a DataTable object
        /// </summary>
        /// <param name="strFilePath">Path to the CSV file</param>
        /// <param name="csvDelimiter">Char used to delimit the CSV file</param>
        /// <returns></returns>
        public DataTable CSVtoDataTable(string strFilePath, char csvDelimiter)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(csvDelimiter);
                foreach (string header in headers)
                {
                    try
                    {
                        dt.Columns.Add(header);
                    }
                    catch (Exception ex)
                    {
                        logManager.Log(ex.Message);
                    }
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(csvDelimiter);
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        #region Event Handlers
        /// <summary>
        /// Captures window closing event and stops it if Azure connection is still open, and closes it
        /// </summary>
        /// <remarks>Currently if the connection is open and the </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                e.Cancel = false;
                try
                {
                    this.Close();
                }
                catch
                {
                    //Shit error works either way
                }
            }
            else 
            { 
                e.Cancel = false;
                logManager.Log("Connection closing");
            }
        }
        ///<summary>
        /// Watchdog to update UI
        ///<summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (azureConnectionManager.isConnected)
            {
                case true:
                    // If connected set the indictor button to green and play a sound
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Green);
                    try
                    {
                        // If the sound file is missing then don't crash that would be double plus ungood
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
                    // If connected set the indictor button to green and play a sound
                    ConnectionStatusLight.Fill = new SolidColorBrush(Colors.Red);
                    try
                    {
                        // If the sound file is missing then don't crash that would be double plus ungood
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
            // Get and update the user access level
            accessLevel = RetriveLevel();
            AccessLevelLB.Content = "Access Level: " + accessLevel.ToString();

            // If the value in the compID box isn't a number then don't enable the boxes
            try
            {
                _ = Int32.Parse(CompIDTB.Text);
                switch (accessLevel)
                {
                    case AccessLevels.None:
                        // Either the access level is none or it's really set to none. Either way disable all the buttons
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
                        // If the user is a user then enable all the buttons except for the insert button
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
                        // If the user is an admin then enable all the buttons
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
            catch(Exception ex)
            {
                logManager.Log(ex.Message);
                // If no level is found or a error happens in the above code then disable all the buttons
                foreach (UIElement element in Grid.Children)
                {
                    if (element is Button)
                    {
                        Button button = (Button)element;
                        button.IsEnabled = false;
                    }
                }
            }
        }
        /// <summary>
        /// Event handler it handles the key press event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bind_KeyPress(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.S:
                    MakeSafe();
                    break;
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void safe_Click(object sender, RoutedEventArgs e)
        {
            MakeSafe();
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            catch (Exception ex)
            {
                logManager.Log(ex.Message);
                MessageBox.Show($"Query failed \n {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sendReports_Click(object sender, RoutedEventArgs e)
        {
            List<string> sps = new List<string>() { "sp_MatchData_RetrieveAverageScores_Summed", "sp_amory_first_pick", "sp_MatchData_AmorySecondPick" };
            List<DataTable> tables = new List<DataTable>();
            MessageBoxResult result = MessageBox.Show("Are you sure you want to send reports? This is a very noisy action", "Bubble bubble I'm a fishy", MessageBoxButton.YesNo, MessageBoxImage.Hand);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    try
                    {
                        Thread playerThread = new Thread(playMissile);
                        playerThread.Start();
                        foreach (string sp in sps) 
                        { 
                            Parameters parameters = new Parameters();
                            Stored stored = new Stored();
                            stored.Name = sp;
                            stored.cID = Int32.Parse(CompIDTB.Text);
                            parameters.Name = "@CompetitionNumber";
                            parameters.value = stored.cID;
                            stored.Parameters = parameters;
                            if (azureConnectionManager.isConnected)
                            {
                                var adapter = azureConnectionManager.ExecuteProcedure(stored);
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);
                                tables.Add(ds.Tables[0]);                           
                            }
                            else
                            {
                                azureConnectionManager.Connect();
                                var adapter = azureConnectionManager.ExecuteProcedure(stored);
                                DataSet ds = new DataSet();
                                adapter.Fill(ds);
                                tables.Add(ds.Tables[0]);
                            }   
                        }
                        SendReports(tables);
                    }
                    catch (Exception ex)
                    {
                        logManager.Log(ex.Message);
                    }
                    break;
                case MessageBoxResult.No: break;
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void directQuery_Click(object sender, RoutedEventArgs e)
        {
            if (azureConnectionManager.isConnected)
            {
                string content = File.ReadAllText(Directory.GetCurrentDirectory() + "\\aaaBaba\\query.txt");
                var adapter = azureConnectionManager.RunDirectQuery(content);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
            else
            {
                azureConnectionManager.Connect();
                string content = File.ReadAllText(Directory.GetCurrentDirectory() + "\\aaaBaba\\query.txt");
                var adapter = azureConnectionManager.RunDirectQuery(content);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                tableDisplay = new TableDisplay(ds.Tables[0]);
                tableDisplay.Show();
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void insert_Click(object sender, RoutedEventArgs e)
        {
            // Promat user to locate csv file to be inserted
            fdlg.ShowDialog();
            // Ask user if the file and location is correct
            MessageBoxResult result = MessageBox.Show($"Pulling data from CSV at {fdlg.FileName} is this okay?", "I'm behind you", MessageBoxButton.YesNo, MessageBoxImage.Question);
            // Handle user response
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Is the user already connected to the database
                    // User is connected, thus don't need to connect again
                    if (azureConnectionManager.isConnected)
                    {
                        azureConnectionManager.Insert(Password.Text, fdlg.FileName, Int32.Parse(CompIDTB.Text));
                    }
                    //User is not connected, lets help them out
                    else
                    {
                        azureConnectionManager.Connect();
                        azureConnectionManager.Insert(Password.Text, fdlg.FileName, Int32.Parse(CompIDTB.Text));
                    }
                    break;
                // Either file type, location, or name is wrong, or they just like the word no
                case MessageBoxResult.No: break;
            }
        }
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            catch (Exception ex)
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
        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        ///<summary>
        /// Open query file in default system text editor
        ///<summary>
        private void open_query_editor_Click(object sender, RoutedEventArgs e)
        {
            // Attempts to open query file
            try
            {
                Process.Start(Directory.GetCurrentDirectory() + "\\aaaBaba\\query.txt");
            }
            // If can't be opened create one
            catch
            {
                // Create file
                FileStream fs = File.Create(Directory.GetCurrentDirectory() + "\\aaaBaba\\query.txt");
                // Immediately close file cause otherwise it will conflict when it's opened in default text editor
                fs.Close();
                Process.Start(Directory.GetCurrentDirectory() + "\\aaaBaba\\query.txt");
            }
        }
        #endregion
    }
}
