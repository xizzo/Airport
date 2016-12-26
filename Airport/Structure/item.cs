﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace Airport.Structure
{
    public class Item
    {
        public NetHandle netHandle { get; set; }
        public PickupHash model { get; set; }
        public Vector3 pos { get; set; }
        public Vector3 rot { get; set; }
        public uint respawnTime { get; set; }
        public string itemType { get; set; }

        public Item()
        {       

        }

        public Item(PickupHash model, Vector3 pos, Vector3 rot, uint respawnTime, string itemType)
        {
            this.model = model;
            this.pos = pos;
            this.rot = rot;
            this.respawnTime = respawnTime;
            this.itemType = itemType;
        }

    }
}
