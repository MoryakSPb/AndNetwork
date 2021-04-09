using System;
using Microsoft.Extensions.Logging;

namespace AndNetwork.Server
{
    public class ExceptionLogger
    {
        private readonly ILogger<ExceptionLogger> _logger;

        public ExceptionLogger(ILogger<ExceptionLogger> logger) => _logger = logger;

        public void SetEvent()
        {
            _logger.LogInformation("ExceptionLogger is ready");
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                Exception exception = (Exception)args.ExceptionObject;
                if (args.IsTerminating)
                    _logger.LogCritical(exception, "Unhandled Exception");
                else
                    _logger.LogError(exception, "Unhandled Exception");
            };
        }
    }
}
