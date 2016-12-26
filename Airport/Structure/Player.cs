using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airport.Structure
{
    public class Player
    {
        public Client client { get; set; }
        public NetHandle netHandle { get; set; }
        public NetHandle vehicle { get; set; }
        public bool hasVehicle { get; set; }

        public Player()
        {
            this.hasVehicle = false;
        }

        public Player(Client client, NetHandle netHandle)
        {
            this.client = client;
            this.netHandle = netHandle;
            this.hasVehicle = false;
        }
    }
}
