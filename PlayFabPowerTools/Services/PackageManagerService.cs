using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayFabPowerTools.Services
{
    public enum MainPackageStates
    {
        Idle,
        Exit,
        Help,
        Login,
        Logout,
        Migrate,
        SetStores

    }

    public class PackageManagerService
    {
        public static Dictionary<MainPackageStates, iStatePackage> PackageCache = new Dictionary<MainPackageStates, iStatePackage>();
        private static MainPackageStates _state = MainPackageStates.Idle;
        private static bool validCommand = false;

        public static void RegisterMainPackageStates(List<MainPackageStates> states, iStatePackage package)
        {
            foreach (var state in states)
            {
                PackageCache.Add(state, package);
            }
        }

        public static void SetState(MainPackageStates state)
        {
            _state = state;
        }

        public static iStatePackage GetPackage()
        {
            return PackageCache[_state];
        }

        public static bool IsIdle()
        {
            return _state == MainPackageStates.Idle;
        }

        public static bool SetState(string line)
        {

            var lline = line.ToLower();

            if (lline.Contains("help") || lline.Contains("?"))
            {
                validCommand = true;
                _state = MainPackageStates.Help;
            }

            if (lline.Contains("login") || lline.Contains("autologin"))
            {
                validCommand = true;
                _state = MainPackageStates.Login;
            }

            if (lline.Contains("migrate"))
            {
                validCommand = true;
                _state = MainPackageStates.Migrate;
            }

            if (lline.Contains("setstores"))
            {
                _state = MainPackageStates.SetStores;
            }

            if (lline.Contains("logout"))
            {
                validCommand = true;
                _state = MainPackageStates.Logout;
            }

            if (lline.Contains("exit"))
            {
                _state = MainPackageStates.Exit;
            }
            return false;
        }


    }
}
