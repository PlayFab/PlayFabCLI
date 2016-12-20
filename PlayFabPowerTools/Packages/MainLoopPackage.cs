using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabPowerTools.Services;

namespace PlayFabPowerTools.Packages
{
    public class MainLoopPackage : iStatePackage
    {
        public enum States
        {
            None,
            Exit
        }

        private static States _state = States.None;

        public void RegisterMainPackageStates(iStatePackage package)
        {
            List<MainPackageStates> states = new List<MainPackageStates>()
            {
                MainPackageStates.Idle,
                MainPackageStates.Exit
            };
            PackageManagerService.RegisterMainPackageStates(states, package);
        }

        public bool SetState(string line)
        {
            
            var lline = line.ToLower();

            if (lline.Contains("exit"))
            {
                _state = States.Exit;
            }
            return false;
        }

        public bool Loop()
        {
            if (_state != States.Exit)
            {
                return false;
            }
            _state = States.None;
            PackageManagerService.SetState(MainPackageStates.Idle);

            //Returning true will exit the program.
            return true;
        }
    }
}
