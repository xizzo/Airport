using GTANetworkServer;
using GTANetworkShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airport.Structure
{
    public class Vehicle
    {
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public VehicleHash Hash { get; set; }

        public Vehicle()
        {

        }

        public Vehicle(Vector3 position, Vector3 rotation, VehicleHash hash)
        {
            this.position = position;
            this.rotation = rotation;
            this.Hash = hash;
        }
    }
}
