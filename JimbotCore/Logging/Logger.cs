using System;
using log4net;

namespace Jimbot.Logging {
    /// <summary>
    /// Logger wrapping a logging framework.
    /// This exists because we might, eventually, someday,
    /// want to change the logging framework and crawling through the whole application
    /// to change all occurrences of some apache logger... blergh.
    /// </summary>
    public class Logger {

        private readonly ILog log;

        public Logger(ILog logger) {
            log = logger;
        }

        public void Debug(object message) {
            log.Debug(message);
        }

        public void Debug(object message, Exception exception) {
            log.Debug(message, exception);
        }


        public void Error(object message) {
            log.Error(message);
        }

        public void Error(object message, Exception exception) {
            log.Error(message, exception);
        }


        public void Fatal(object message) {
            log.Fatal(message);
        }

        public void Fatal(object message, Exception exception) {
            log.Fatal(message, exception);
        }

        public void Info(object message) {
            log.Info(message);
        }

        public void Info(object message, Exception exception) {
            log.Info(message, exception);
        }

        public void Warn(object message) {
            log.Warn(message);
        }

        public void Warn(object message, Exception exception) {
            log.Warn(message, exception);
        }
    }
}