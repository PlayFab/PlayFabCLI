using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Awareness;
using ManyConsole;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.ServerModels;
using PlayFabCLI.Commands;
using PlayFabCLI.Services;
using PlayFabCLI.Utils;
using PlayFabToolSDK.Services;

namespace PlayFabCLI
{
    public class Program
    {
        private static Container _container;

        static void Main(string[] args)
        {
            // Create and configure container
            _container = new Container();
            _container.Register<ILogger, LoggerImpl>();
            _container.Register<IPlayFabEditorService, PlayFabEditorService>();
            _container.Register<IFileService, FileService>();
            _container.Register<IMigrationService, MigrationService>();
            _container.Register<IRemoteTransferService, RemoteTransferService>();
            _container.Register<IAuthenticationService, AuthenticationService>();
            _container.Register<IMigrationConfigService, MigrationConfigService>();
            _container.Register<ITitleRepositoryService, TitleRepositoryService>();

            // automagically register all the command types
            _container.RegisterCollection<ConsoleCommand>(typeof(Program).Assembly.GetConcreteImplementationsOf<ConsoleCommand>());
            _container.Verify();

            // Dispatch arguments to locate and execute suitable command
            ConsoleCommandDispatcher.DispatchCommand(GetCommands(), args, Console.Out);
        }

        /// <summary>
        /// Get available commands
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return _container.GetAllInstances<ConsoleCommand>();
        }

    }
}
