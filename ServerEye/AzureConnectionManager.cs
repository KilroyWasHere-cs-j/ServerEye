using System;
using System.Data;
using System.Data.Odbc;
using System.Net;
using System.Text;
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
                SendToHook("Connected to Azure");
            }
            catch (Exception e)
            {
                SendToHook($"Failed Azure connect {e.Message}");
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
                SendToHook("Disconnected form Azure");
            }
            catch (Exception e)
            {
                SendToHook($"Failed Azure disconnect {e.Message}");
                MessageBox.Show(e.Message);
                logManager.Log(e.Message);
            }
        }

        public void SendToHook(string message)
        {
            //WindowsIdentity.GetCurrent().Name
            string webhook = "https://discord.com/api/webhooks/1082337403567620126/vlmEzBxb8jvIapfZLz4PDreZWWOFwP59-LE9rOkaRZ4YQNofF6nw-CLrJS0r6cbIBIwl";

            WebClient client = new WebClient();
            client.Headers.Add("Content-Type", "application/json");
            string payload = "{\"content\": \"" + "Stevie Wonder " + message + " at-> " + DateTime.Now.ToString("h:mm:ss tt") + "\"}";
            client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
        }

        #region Queries

        #region Direct Queries
        public void RunDirectQuery(int cID)
        {

        }

        // SQL injections baby!
        public OdbcDataAdapter RunStructuredQuery(int cID, string query)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand(query, cnn);
                return new OdbcDataAdapter(cmd);

            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }
        #endregion

        public OdbcDataAdapter GetMatchData(int cID)
        {
            try
            {
                // sp_Get_All_Match_Data
                OdbcCommand cmd = new OdbcCommand("{call sp_Get_All_Match_Data (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompetitionNumber", cID);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GetPickList(int cID)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call sp_MatchData_RetrieveAverageScores_Summed (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompetitionNumber", cID);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GenerateAmoryFirstPick(int cID)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call sp_amory_first_pick}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompetitionNumber", cID);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }
        public OdbcDataAdapter GenerateAmorySecondPick(int cID)
        {
            try
            {
                MessageBox.Show(cID.ToString());
                OdbcCommand cmd = new OdbcCommand("{call sp_MatchData_AmorySecondPick}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CompetitionNumber", cID);
                return new OdbcDataAdapter(cmd);
            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }
        #endregion
    }
}
