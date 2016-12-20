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
            Console.WriteLine("Help [help] - You are lookin at it.");
            Console.WriteLine("");
            Console.WriteLine("Login [login] - Login to playfab. Usage: login auto [username] [password] for autologin");
            Console.WriteLine("");
            Console.WriteLine("Migrate [migrate [from titleid] [to titleid] - migrate, titledata, cloudscript, files, catalogs and stores to a new title.");
            Console.WriteLine("");
            Console.WriteLine("Exit [--exit] - Exit this console app");
            Console.ResetColor();
        }
    }
}
