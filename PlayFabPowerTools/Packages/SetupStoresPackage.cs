using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabPowerTools.Services;

namespace PlayFabPowerTools.Packages
{
    public class SetupStoresPackage : iStatePackage
    {
        /// <summary>
        /// Register self with MainLoop Package
        /// </summary>
        /// <param name="package"></param>
        public void RegisterMainPackageStates(iStatePackage package)
        {
            List<MainPackageStates> states = new List<MainPackageStates>()
            {
                MainPackageStates.SetStores
            };
            PackageManagerService.RegisterMainPackageStates(states, package);
        }

        /// <summary>
        /// Set State and settings based on line input
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool SetState(string line)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Setting Store List");
            var argsList = line.Split(' ');
            var storeIdList = argsList.ToList().FindAll(s => !s.ToLower().Contains("setstores") && !string.IsNullOrEmpty(s)).ToList();
            PlayFabService.StoreList = storeIdList;
            PlayFabService.Save();
            Console.ForegroundColor = ConsoleColor.White;
            return false;
        }

        /// <summary>
        /// do work for this package.
        /// </summary>
        /// <returns></returns>
        public bool Loop()
        {
            PackageManagerService.SetState(MainPackageStates.Idle);
            return false;
        }
    }
}
