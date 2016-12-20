using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabPowerTools.Packages;
using PlayFabPowerTools.Services;

namespace PlayFabPowerTools
{
    class Program
    {
        private static iStatePackage _currentPackage;
        private static bool _isTypedTextHidden = false;
        private static string _line;
        private static StringBuilder _lineSb;
        private static bool _argsRead;
        private static string[] _args = new string[0];
        private static bool _readLine = false;
        private static bool _isCLI = false;
        static void Main(string[] args)
        {
            _args = args;
            _lineSb = new StringBuilder();

            //Parse Args and determine if this is a CLI or Console mode.
            if (args.Length > 0 && !_argsRead)
            {
                _isCLI = true;
                foreach (var a in args)
                {
                    _lineSb.Append(a + " ");
                }
                _line = _lineSb.ToString();
                _argsRead = true;
                _args = new string[0];
            }
            else
            {
                _readLine = true;
                _argsRead = true;
            }

            //Init PlayFab Service Settings
            PlayFabService.Init();
            //Load Settings File
            PlayFabService.Load();
            
            //Out put to screen some fancy playfab jazz
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(".oO={ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("PlayFab Power Tools CLI");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" }=Oo.");
            Console.ForegroundColor = ConsoleColor.White;

            //if this is a console app then we want to show them how to get help.
            if (!_isCLI)
            {
                Console.WriteLine("");
                Console.WriteLine("Type: 'Help' for a list of commands");
                Console.WriteLine("");
                Console.Write(">");
            }
            else
            {
                Console.WriteLine("");
                Console.WriteLine("");
            }

            //Load all the packages that process commands
            var type = typeof(iStatePackage);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && p.Name != "iStatePackage");

            //foreach package we need to register with Package Manager
            foreach (var t in types)
            {
                var packageType = (iStatePackage)Activator.CreateInstance(t);
                packageType.RegisterMainPackageStates(packageType);
            }

            //get the correct package for the state we are in
            _currentPackage = PackageManagerService.GetPackage();

            //This is the main program loop.
            do
            {
                //if we are a console app, read the command that is entered.
                if (_args.Length == 0 && _readLine)
                {
                    if (!_isTypedTextHidden)
                    {
                        //Read the line input from the console.
                        _line = Console.ReadLine();
                    }
                    else
                    {
                        //Read the line in a different way.
                        ConsoleKeyInfo key;
                        do
                        {
                            key = Console.ReadKey(true);
                            if (key.Key != ConsoleKey.Enter)
                            {
                                var s = string.Format("{0}", key.KeyChar);
                                _lineSb.Append(s);
                            }
                        } while (key.Key != ConsoleKey.Enter);
                        _line = _lineSb.ToString();
                    }
                }

                //Set read line to true, not it will only be false if we came from a CLI.
                _readLine = true;
                var loopReturn = false;
                if (PackageManagerService.IsIdle())
                {
                    //If we are idle then we want to check for commands.
                    PackageManagerService.SetState(_line);
                    _currentPackage = PackageManagerService.GetPackage();
                    _isTypedTextHidden = _currentPackage.SetState(_line);
                    loopReturn = _currentPackage.Loop();
                }
                else
                {
                    //If we are not idle, then we want to process the _line for arguments.

                    //get the correct package for the state we are in
                    _currentPackage = PackageManagerService.GetPackage();

                    //process the package state
                    _isTypedTextHidden = _currentPackage.SetState(_line);

                    //do package loop, which contains logic to do stuff.
                    loopReturn = _currentPackage.Loop();
                }

                //if this is a CLI then we just want to exit.
                if (!_isCLI)
                {
                    //Prompt or exit.
                    if (!loopReturn)
                    {
                        Console.Write(">");
                    }
                    else
                    {
                        _line = null;
                    }
                }
                else
                {
                    _line = null;
                }
            } while (_line != null);

            //Always save before we completely exit.
            PlayFabService.Save();
        }
    }
}
