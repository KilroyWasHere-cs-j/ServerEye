
using Microsoft.Data.SqlClient;
using System;

namespace ServerEye
{
    class AzureConnectionManager
    {
        public string connectionString = ""; // Stores connection string
        public bool isConnected = false; // Indicates if the connection is open or closed
        public bool safe = true; // Indicates if it's okay to perform actions that could break the connection
        private LogManager logManager;

        private SqlConnection cnn;
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
            cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();
                isConnected = true;
            }
            catch (Exception e)
            {
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
            }
            catch (Exception e) 
            {
                logManager.Log(e.Message);
            }
        }
    }
}
