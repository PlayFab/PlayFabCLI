using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayFabPowerTools.Packages
{
    public class MainLoopPackage : iStatePackage
    {
        public static  Dictionary<MainPackageStates, iStatePackage> PackageCache = new Dictionary<MainPackageStates,iStatePackage>();

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

        private static MainPackageStates _state = MainPackageStates.Idle;
        private bool validCommand = false;

        public iStatePackage GetPackage()
        {
            return PackageCache[_state];
        }

        public void RegisterMainPackageStates(iStatePackage package)
        {
            PackageCache.Add(MainPackageStates.Idle, package);
            PackageCache.Add(MainPackageStates.Exit, package);
        }

        public static void SetState(MainPackageStates state)
        {
            _state = state;
        }

        public bool SetState(string line)
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

        public bool Loop()
        {
            if (_state == MainPackageStates.Exit)
            {
                return true;
            }

            if (_state != MainPackageStates.Idle)
            {
                //run the first iteration of the selected state loop.
                PackageCache[_state].Loop();
            }
            return false;
        }
    }
}
