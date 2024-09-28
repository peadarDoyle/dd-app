using System.Web.Configuration;
using System;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace DirectDebits.Dependencies
{
    internal class LoggerFactory
    {
        public static ILogger Create()
        {
            ILogger logger = CreateConcreteLogger();
            logger = EnrichLogger(logger);
            return logger;
        }

        private static ILogger EnrichLogger(ILogger logger)
        {
            logger = logger.ForContext("CorrelationId", Guid.NewGuid());
            return logger;
        }


        private static ILogger CreateConcreteLogger()
        {
            bool useSeq = WebConfigurationManager.AppSettings["UseSeq"] == "true";

            if (useSeq)
            {
                return new LoggerConfiguration()
                    .WriteTo.Seq("http://localhost:5341")
                    .CreateLogger();
            }

            return new LoggerConfiguration()
                .WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                .CreateLogger();
        }
    }
}