using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;

namespace PlayFabCLI.Commands
{

    /// <summary>
    /// Base class for ManyConsole commands wrapped to execute in async environment
    /// </summary>
    public abstract class ConsoleCommandAsync : ConsoleCommand
    {

        /// <summary>
        /// Override original Run and execute abstract RunAsync as Task
        /// </summary>
        /// <param name="remainingArguments"></param>
        /// <returns></returns>
        public override int Run(string[] remainingArguments)
        {
            try
            {
                RunAsync(remainingArguments).Wait();
                return ConsoleCommandResult.Success;
            }
            catch (AggregateException ex)
            {
                foreach (var innerEx in ex.InnerExceptions)
                {
                    Console.WriteLine("Fatal Error!");
                    PrintException(innerEx);
                }
                return ConsoleCommandResult.Failure;
            }
        }

        /// <summary>
        /// Implement this method to run async command
        /// </summary>
        /// <param name="remainingArguments"></param>
        /// <returns></returns>
        public abstract Task RunAsync(string[] remainingArguments);


        private void PrintException(Exception ex, int ind = 0)
        {
            Console.WriteLine(ex.Message.PadLeft(ind,' '));
            if(ex.InnerException != null) PrintException(ex.InnerException,ind + 1);
        }
    }
}
