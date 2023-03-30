using NLog;

namespace ServerEye
{
    class LogManager
    {
        private static readonly NLog.Logger _log_ = NLog.LogManager.GetCurrentClassLogger();
        private string path = "";
        public LogManager(string _path) 
        {
            path = _path;
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = path };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            _log_.Debug("__LOGGING-INIT__");
        }

        ~LogManager()
        { 
            _log_.Debug("__LOGGING-EXIT__");
        }

        public void Log(string message)
        {
            _log_.Debug(message);
        }
    }
}
