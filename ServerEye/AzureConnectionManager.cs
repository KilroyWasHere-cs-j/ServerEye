using System;
using System.Data;
using System.Data.Odbc;
using System.Windows;

namespace ServerEye
{
    class AzureConnectionManager
    {
        public string connectionString = "Driver={ODBC Driver 18 for SQL Server};Server=tcp:scouteye.database.windows.net,1433;Database=ScoutEye;Uid=ScoutEye;Pwd={Q0L1Agpop-)+=R\\I;'G`};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"; // Stores connection string
        public bool isConnected = false; // Indicates if the connection is open or closed
        public bool safe = true; // Indicates if it's okay to perform actions that could break the connection

        private LogManager logManager;
        private TableDisplay td;

        private OdbcConnection cnn;
        public AzureConnectionManager()
        {
            logManager = new LogManager("logs/connection_log.txt");
        }

        //<summary>
        // Connects to Azure database
        //<summary>
        public void Connect()
        {
            logManager.Log("Connecting to Azure...");
            cnn = new OdbcConnection(connectionString);
            try
            {
                cnn.Open();
                isConnected = true;
                logManager.Log("Connected to Azure");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                logManager.Log(e.Message);
            }
        }

        //<summary>
        // Disconnects from Azure database
        //<summary>
        public void closeConnection()
        {
            logManager.Log("Disconnecting form Azure...");
            try
            {
                cnn.Close();
                isConnected = false;
                logManager.Log("Disconnected form Azure");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                logManager.Log(e.Message);
            }
        }

        public void GetMatchData()
        {
            DataTable table = new DataTable();
            string query = "SELECT CD.Description + CAST(CD.[Year] AS VARCHAR(50)) AS Competition,\r\n\tS.ScoutName AS Scout, [Match], Alliance, TeamNumber, AutoZero, AutoOne, AutoTwo, AutoThree, AutoFour, AutoFive, TeleopZero, TeleopOne, TeleopTwo, TeleopThree, TeleopFour, TeleopFive\r\nFROM MatchData MD\r\nJOIN CompDesc CD ON MD.CompetitionID = CD.CompID\r\nJOIN Scouts S ON MD.ScoutId = S.ScoutID";
            OdbcCommand cmd = new OdbcCommand(query, cnn);
            using (OdbcDataReader returnRead = cmd.ExecuteReader())
            {
                logManager.Log("Executing SQL Query");
                while (returnRead.Read())
                {
                    table = returnRead.GetSchemaTable();
                    td = new TableDisplay(table);
                }
                logManager.Log("Query complete");
                td.Show();
            }
        }
    }
}
