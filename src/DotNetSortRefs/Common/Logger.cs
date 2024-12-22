using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Common;

namespace DotnetSortAndSyncRefs.Common
{
    internal class Logger : ILogger
    {
        private readonly Reporter _reporter;

        public Logger(Reporter reporter)
        {
            _reporter = reporter;
        }

        public void LogDebug(string data)
        {
            LogVerbose(data);
        }

        public void LogVerbose(string data)
        {
            _reporter.Verbose(data);
        }

        public void LogInformation(string data)
        {
            _reporter.Output(data);
        }

        public void LogMinimal(string data)
        {
            LogInformation(data);
        }

        public void LogWarning(string data)
        {
            _reporter.Warn(data);
        }

        public void LogError(string data)
        {
            LogWarning(data);
        }

        public void LogInformationSummary(string data)
        {
            LogInformation(data);
        }

        public void Log(LogLevel level, string data)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    LogDebug(data);
                    break;
                case LogLevel.Verbose:
                    LogVerbose(data);
                    break;
                case LogLevel.Information:
                    LogInformationSummary(data);
                    break;
                case LogLevel.Minimal:
                    LogMinimal(data);
                    break;
                case LogLevel.Warning:
                    LogWarning(data);
                    break;
                case LogLevel.Error:
                    LogError(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        public Task LogAsync(LogLevel level, string data)
        {
            Log(level, data);
            return Task.CompletedTask;
        }

        public void Log(ILogMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            Log(message.Level, message.ToString());
        }

        public Task LogAsync(ILogMessage message)
        {
            Log(message);
            return Task.CompletedTask;
        }
    }
}
