﻿using System;
using System.Data;
using System.Data.Odbc;
using System.Net;
using System.Text;
using System.Windows;

namespace ServerEye
{
    class AzureConnectionManager
    {
        // Privileges are extremely limited so security is not of the highest concern :)
        private readonly string connectionString = "Driver={ODBC Driver 18 for SQL Server};Server=tcp:scouteye.database.windows.net,1433;Database=ScoutEye;Uid=scouts;Pwd=ctx9hfd4cfk.tzx*TAT;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;"; // Stores connection string
        // My code is a SQL injection
        public bool isConnected = false; // Indicates if the connection is open or closed
        public bool safe = true; // Indicates if it's okay to perform actions that could break the connection

        private readonly LogManager logManager;
        private OdbcConnection cnn;  

        public AzureConnectionManager()
        {
            logManager = new LogManager("logs/connection_log.txt");
        }

        ~AzureConnectionManager()
        {
            closeConnection();
            logManager.Log("~");
        }

        // Yes I made a whole ass function for this...
        private string BuildIConnectionString(string password)
        {
            return "Driver={ODBC Driver 18 for SQL Server};Server=tcp:scouteye.database.windows.net,1433;Database=ScoutEye;Uid=ScoutEye;Pwd=" + password + ";Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;";
        }

        ///<summary>
        /// Connects to Azure database
        ///</summary>
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

        ///<summary>
        /// Disconnects from Azure database
        ///</summary>
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

        ///  <summary>
        /// Sends a message to the webhook
        /// </summary>
        /// <param name="message">Definds the message to be sent to the discord server</param>
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
                client.Dispose();
            }
            catch (Exception e) 
            { 
                logManager.Log(e.Message); 
            }
        }

        #region Queries

        #region Direct Queries
        // SQL injections baby!
        /// <summary>
        /// Runs a direct query on the database, uses a readonly user so it's safeish
        /// </summary>
        /// <param name="query">Valid SQL query</param>
        /// <returns>OdbcDataAdapter</returns>
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

        ///<summary>
        /// Runs a stored procedure on the database with parameters
        ///</summary>
        /// <param name="stored">Instance of the Stored class, that repersents the sp to be executed</param>
        /// <returns>OdbcDataAdapter</returns>
        public OdbcDataAdapter ExecuteProcedure(Stored stored)
        {
            // Needs to be able to handle instances with no parameters
            try
            {
                var cmd = new OdbcCommand("{call " + stored.Name + " (?)}", cnn);
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

        /// <summary>
        /// Runs a stored procedure on the database with parameters
        /// </summary>
        /// <param name="stored">Instance of the Stored class, that repersents the sp to be executed. WITHOUT params</param>
        /// <returns>OdbcDataAdapter</returns>
        public OdbcDataAdapter ExecuteCleanProcedure(Stored stored)
        {
            // Needs to be able to handle instances with no parameters
            try
            {
                var cmd = new OdbcCommand("{call " + stored.Name + "}", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                return new OdbcDataAdapter(cmd);
            }
            catch (Exception e)
            {
                logManager.Log(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Inserts data into the database
        /// As this uses writing privileges it requires the user to input
        /// </summary>
        /// <param name="password">Password to be inserted</param>
        /// <param name="dataPath">Path to CSV file to be inserted</param>
        /// <param name="compID">ID of comp to insert into</param>
        public void Insert(string password, string dataPath, int compID)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
