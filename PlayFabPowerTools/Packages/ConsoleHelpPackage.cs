using System.Collections.Generic;
using PlayFabPowerTools.Services;

namespace PlayFabPowerTools.Packages
{
    class ConsoleHelpPackage : iStatePackage
    {
        public void RegisterMainPackageStates(iStatePackage package)
        {
            List<MainPackageStates> states = new List<MainPackageStates>()
            {
                MainPackageStates.Help
            };
            PackageManagerService.RegisterMainPackageStates(states, package);
        }

        public bool SetState(string line)
        {
            return false;
        }

        public bool Loop()
        {
            HelpService.ShowHelp();
            PackageManagerService.SetState(MainPackageStates.Idle);
            return false;
        }
    }
}
