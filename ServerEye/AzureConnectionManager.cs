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
        //  "Driver={ODBC Driver 18 for SQL Server};Server=tcp:scouteye.database.windows.net,1433;Database=ScoutEye;Uid=ScoutEye;Pwd={Q0L1Agpop-)+=R\\I;'G`};Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"; 
        // Privileges are extremely limited so security is not of the highest concern
        public string connectionString = "Driver={ODBC Driver 18 for SQL Server};Server=tcp:scouteye.database.windows.net,1433;Database=ScoutEye;Uid=scouts;Pwd=ctx9hfd4cfk.tzx*TAT;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"; // Stores connection string
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
            try
            {
                //WindowsIdentity.GetCurrent().Name
                string webhook = Properties.Resources.Webhook;

                WebClient client = new WebClient();
                client.Headers.Add("Content-Type", "application/json");
                string payload = "{\"content\": \"" + "Stevie Wonder " + message + " at-> " + DateTime.Now.ToString("h:mm:ss tt") + "\"}";
                client.UploadData(webhook, Encoding.UTF8.GetBytes(payload));
            }
            catch (Exception e) 
            { 
                logManager.Log(e.Message); 
            }
        }

        #region Queries

        #region Direct Queries
        // SQL injections baby!
        public OdbcDataAdapter RunDirectQuery(string query)
        {
            try
            {
                Connect();
                OdbcCommand cmd = new OdbcCommand(query, cnn);
                cmd.ExecuteScalar();
                return new OdbcDataAdapter(cmd);

            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }
        #endregion

        public OdbcDataAdapter ExecuteProcedure(Stored stored)
        {
            // Needs to be able to handle instances with no parameters
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(stored.Parameters.Name, stored.Parameters.value);
                return new OdbcDataAdapter(cmd);
            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }
        public OdbcDataAdapter ExecuteCleanProcedure(Stored stored)
        {
            // Needs to be able to handle instances with no parameters
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + "}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                return new OdbcDataAdapter(cmd);
            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GetMatchData(Stored stored)
        {
            try
            {
                // sp_Get_All_Match_Data
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(stored.Parameters.Name, stored.Parameters.value);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GetPickList(Stored stored)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(stored.Parameters.Name, stored.Parameters.value);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GenerateAmoryFirstPick(Stored stored)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(stored.Parameters.Name, stored.Parameters.value);
                return new OdbcDataAdapter(cmd);
            }
            catch(Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public OdbcDataAdapter GenerateAmorySecondPick(Stored stored)
        {
            try
            {
                OdbcCommand cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(stored.Parameters.Name, stored.Parameters.value);
                return new OdbcDataAdapter(cmd);
            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        public void Insert()
        {

        }
        #endregion
    }
}
