using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabPowerTools.Packages;

namespace PlayFabPowerTools.Services
{
    public class HelpService
    {
        public static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("");
            Console.WriteLine("Commands and Usages:");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Clear [--clear] - Clears the console screen");
            Console.WriteLine("");
            Console.WriteLine("Help [--help || --?] - You are lookin at it.");
            Console.WriteLine("");
            Console.WriteLine("Login [--login || --autologin] - Login to playfab, for autologin  pass  --user:[username] --pass:[password]");
            Console.WriteLine("");
            Console.WriteLine("Exit [--exit] - Exit this console app");
            Console.ResetColor();
            MainLoopPackage.SetState(MainLoopPackage.MainPackageStates.Idle);
        }
    }
}
