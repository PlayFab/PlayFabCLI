using PlayFabPowerTools.Services;

namespace PlayFabPowerTools.Packages
{
    class ConsoleHelpPackage : iStatePackage
    {
        public void RegisterMainPackageStates(iStatePackage package)
        {
            MainLoopPackage.PackageCache.Add(MainLoopPackage.MainPackageStates.Help, package);
        }

        public bool SetState(string line)
        {
            return false;
        }

        public bool Loop()
        {
            HelpService.ShowHelp();
            return false;
        }
    }
}
