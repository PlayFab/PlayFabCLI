using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awareness;

namespace PlayFabCLI.Utils
{
    /// <summary>
    /// Simple logger implementation
    /// </summary>
    public class LoggerImpl : ILogger
    {
        public void Append(string message)
        {
            Console.Write(message);
        }

        public void Log(string message, object caller)
        {
            var instance = caller as INamedInstance;
            var name = instance != null ? instance.Name : caller?.GetType().Name;
            Append($"[ {name} ] {message}\n");
        }

        public void Verbose(string message, object caller)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Log(message, caller);
        }

        public void Debug(string message, object caller)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Log(message, caller);
        }

        public void Error(string message, object caller)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Log("Error: "+message,caller);
        }
    }
}
