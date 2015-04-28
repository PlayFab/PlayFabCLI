using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayFabPowerTools
{
    class Program
    {
        static CommandManager _manager;
        static PlayFabManager _playFabManager;

        static void Main(string[] args)
        {
            string line;

            _manager = new CommandManager();
            Console.WriteLine("Welcome to PlayFab Power Tools");
            Console.WriteLine("Type: Help for a list of commands");
            _playFabManager = new PlayFabManager();

            do
            {
                line = Console.ReadLine();
                var command = _manager.GetCommand(line);

                if (command == CommandManager.CommandTypes.HELP)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine( "" );
                    Console.WriteLine("Command Usages:");
                    Console.WriteLine( "" );
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine( "Clear - Clears the console screen" );
                    Console.WriteLine("");
                    Console.WriteLine( "Version - Outputs the current version of the cloudscript." );
                    Console.WriteLine("");
                    Console.WriteLine("Publish - Uploads any changes to The CloudScript to the server.");
                    Console.WriteLine("use 'Publish /newversion' to increment the version instead of the revision.");
                    Console.WriteLine("");
                    Console.WriteLine("Pull - Pulls down the existing cloudscript file and saves it into data/files/ directory.");
                    Console.ResetColor();
                }

                if (command == CommandManager.CommandTypes.UNKNOWN)
                {
                    Console.WriteLine("Invalid or Unknown command");
                }

                if (command == CommandManager.CommandTypes.CLEAR)
                {
                    Console.Clear();
                }

                if (command == CommandManager.CommandTypes.PUBLISH)
                {
                    if (line.Contains("/newversion"))
                    {
                        _playFabManager.Publish(true);
                    }
                    else
                    {
                        _playFabManager.Publish();
                    }
                    
                }

                if (command == CommandManager.CommandTypes.VERSION)
                {
                    _playFabManager.GetVersion();
                }

                if (command == CommandManager.CommandTypes.EXIT)
                {
                    return;
                }

                if( command == CommandManager.CommandTypes.PULL )
                {
                    _playFabManager.Pull();
                }

                if (command == CommandManager.CommandTypes.BUILD)
                {
                    _playFabManager.Build();
                }

                Console.WriteLine("");
                Console.Write(">");
            } while (line != null);
            
        }
    }
}
