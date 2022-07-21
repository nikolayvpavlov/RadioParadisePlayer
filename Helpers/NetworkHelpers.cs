using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadioParadisePlayer.Helpers
{
    public static class NetworkHelpers
    {
        public static bool IsMeteredConnection()
        {
            var icp = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
            if (icp?.GetConnectionCost()?.NetworkCostType == Windows.Networking.Connectivity.NetworkCostType.Unrestricted)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
