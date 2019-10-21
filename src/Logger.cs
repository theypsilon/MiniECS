using System;
using System.Collections.Generic;
using System.IO;

namespace MiniECS
{
	public static class Logger
	{
		private static ILoggerInstance _instance;

		public static void SetInstance(ILoggerInstance instance) {
			_instance = instance;
			_instance.Initialize();
		}

		public static List<Log> GetLogs() {
			return _instance?.GetLogs();
		}

		public static List<Log> GetErrors() {
			return _instance?.GetErrors();
		}

		public static void LogImportant(object message)
		{
			_instance?.Log(message);
		}

		[System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
		public static void Log(object message)
		{
			_instance?.Log(message);
		}
	
		[System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
		public static void LogFormat(string message, params object[] args)
		{
			_instance?.LogFormat(message, args);
		}
	
		[System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
		public static void LogWarning(object message)
		{
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			_instance?.LogWarning(message + "\n" + t);
		}
	
		[System.Diagnostics.Conditional(CompilationConstant.DebugSymbol)]
		public static void LogWarningFormat(string message, params object[] args)
		{
			_instance?.LogWarningFormat(message, args);
		}
	
		public static void LogError(object message)
		{
			_instance?.LogError(message);
		}

		public static void LogErrorFormat(string message, params object[] args)
		{
			_instance?.LogErrorFormat(message, args);
		}
	
		public static void LogException(System.Exception exception)
		{
			_instance?.LogException(exception);
		}
	}
	public sealed class Log
	{
		public string Condition;
		public string Stacktrace;
		public string Type;
	}

	public interface ILoggerInstance {
		void Initialize();
		List<Log> GetLogs();
		List<Log> GetErrors();
		void Log(object message);
		void LogFormat(string message, params object[] args);
		void LogWarning(object message);
		void LogWarningFormat(string message, params object[] args);
		void LogError(object message);
		void LogErrorFormat(string message, params object[] args);
		void LogException(Exception exception);
	}

    public class ConsoleLogger : ILoggerInstance {

        private ConsoleLogger() {}
        public static ILoggerInstance New() {
            return new MessageLoggerDecorator(new ConsoleLogger());
        }
		public List<Log> GetLogs() {
			return null;
		}
		public List<Log> GetErrors() {
			return null;
		}
		public void Initialize()
		{}

		public void Log(object message)
		{
			Console.WriteLine(message);
		}
	
		public void LogFormat(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}
	
		public void LogWarning(object message)
		{
			Console.WriteLine(message);
		}
	
		public void LogWarningFormat(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}
	
		public void LogError(object message)
		{
			Console.WriteLine(message);
		}

		public void LogErrorFormat(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}
	
		public void LogException(System.Exception exception)
		{
			Console.WriteLine(exception.ToString());
		}
	}

    public class MultiLogger: ILoggerInstance
    {
        private ILoggerInstance[] _loggers = null;

        public MultiLogger(ILoggerInstance[] loggers)
        {
            _loggers = loggers;
        }

        public List<Log> GetLogs()
        {
            return null;
        }
        public List<Log> GetErrors()
        {
            return null;
        }
        public void Initialize()
        {
            foreach (var logger in _loggers)
                logger.Initialize();
        }

        public void Log(object message)
        {
            foreach (var logger in _loggers)
                logger.Log(message);
        }

        public void LogFormat(string message, params object[] args)
        {
            foreach (var logger in _loggers)
                logger.LogFormat(message, args);
        }

        public void LogWarning(object message)
        {
            foreach (var logger in _loggers)
                logger.LogWarning(message);
        }

        public void LogWarningFormat(string message, params object[] args)
        {
            foreach (var logger in _loggers)
                logger.LogWarningFormat(message);
        }

        public void LogError(object message)
        {
            foreach (var logger in _loggers)
                logger.LogError(message);
        }

        public void LogErrorFormat(string message, params object[] args)
        {
            foreach (var logger in _loggers)
                logger.LogErrorFormat(message);
        }

        public void LogException(System.Exception exception)
        {
            foreach (var logger in _loggers)
                logger.LogException(exception);
        }
    }


    public class FileLogger : ILoggerInstance
    {
        private StreamWriter _writer = null;
        private string _path;

        private FileLogger(string path)
        {
            _path = path;
        }

        public static ILoggerInstance New(string path) {
            return new MessageLoggerDecorator(new FileLogger(path));
        }

        public List<Log> GetLogs()
        {
            return null;
        }
        public List<Log> GetErrors()
        {
            return null;
        }
        public void Initialize()
        {
            _writer = new StreamWriter(_path, append: false);
        }

        public void Log(object message)
        {
            _writer.WriteLine(message);
            _writer.Flush();
        }

        public void LogFormat(string message, params object[] args)
        {
            _writer.WriteLine(message, args);
            _writer.Flush();
        }

        public void LogWarning(object message)
        {
            _writer.WriteLine(message);
            _writer.Flush();
        }

        public void LogWarningFormat(string message, params object[] args)
        {
            _writer.WriteLine(message, args);
            _writer.Flush();
        }

        public void LogError(object message)
        {
            _writer.WriteLine(message);
            _writer.Flush();
        }

        public void LogErrorFormat(string message, params object[] args)
        {
            _writer.WriteLine(message, args);
            _writer.Flush();
        }

        public void LogException(System.Exception exception)
        {
            _writer.WriteLine(exception.ToString());
            _writer.Flush();
        }
    }


    public class MessageLoggerDecorator : ILoggerInstance
    {
        private ILoggerInstance _logger;

        public MessageLoggerDecorator(ILoggerInstance logger)
        {
            _logger = logger;
        }

        public List<Log> GetLogs()
        {
            return _logger.GetLogs();
        }
        public List<Log> GetErrors()
        {
            return _logger.GetErrors();
        }
        public void Initialize()
        {
            _logger.Initialize();
        }

        public void Log(object message)
        {
            _logger.Log(message);
        }

        public void LogFormat(string message, params object[] args)
        {
            _logger.LogFormat(message, args);
        }

        public void LogWarning(object message)
        {
            _logger.LogWarning("Warning! " + message);
        }

        public void LogWarningFormat(string message, params object[] args)
        {
            _logger.LogWarningFormat("Warning! " + message, args);
        }

        public void LogError(object message)
        {
            _logger.LogError("ERROR! " + message);
        }

        public void LogErrorFormat(string message, params object[] args)
        {
            _logger.LogErrorFormat("ERROR! " + message, args);
        }

        public void LogException(System.Exception exception)
        {
            _logger.LogException(exception);
        }
    }

}