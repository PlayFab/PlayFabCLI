using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabPowerTools.Packages;

namespace PlayFabPowerTools
{
    public interface iStatePackage
    {
        void RegisterMainPackageStates(iStatePackage package);

        bool SetState(string line);
        bool Loop();
    }
}
