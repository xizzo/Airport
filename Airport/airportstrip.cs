using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using Airport.Structure;


namespace Airport
{

    public class airportstrip : Script
    {

        #region "Variables"

        public static List<Player> Players = new List<Player>();
        public static List<Structure.Vehicle> Vehicles = new List<Structure.Vehicle>();
        public static List<Item> Items = new List<Item>();
        public double currUpdateTick = 0;
        private int ClearVehicleAfterTicks = 10;

        #endregion

        #region "Initialize"

            public airportstrip()
            {
                API.onResourceStart += onResourceStart;
                API.onPlayerConnected += onPlayerConnected;
                API.onUpdate += onUpdate;
                API.onPlayerRespawn += onPlayerRespawn;
                API.onPlayerDisconnected += onPlayerDisconnected;
                API.onVehicleDeath += onVehicleDeath;
                API.onPlayerEnterVehicle += onPlayerEnterVehicle;
                API.onPlayerPickup += onPlayerPickup;

                GetAllVehicleData();
                GetAllPickupData();
                SpawnAllVehicles();
                SpawnAllPickupItems();
            }

        #endregion

        #region "Events"

            public void onPlayerConnected(Client sender)
            {
                Players.Add(new Player(sender, sender.handle));
                API.setPlayerSkin(sender, (PedHash)(-308279251));
                SetupPlayerSpawn(sender);
                API.sendChatMessageToAll(sender.name + " connected to the server!");
            }

            public void onUpdate()
            {
                currUpdateTick++;
                if (currUpdateTick == 2000)
                {
                    DoVehicleCleanup();
                    currUpdateTick = 0;
                }
            }

            public void onPlayerRespawn(Client sender)
            {
                SetupPlayerSpawn(sender);
            }

            public void onPlayerDisconnected(Client sender, string reason)
            {
                foreach (Player p in Players)
                {
                    if (p.client == sender)
                    {

                        if (p.hasVehicle)
                        {
                            API.deleteEntity(p.vehicle);
                        }

                        API.sendChatMessageToAll(sender.name + " left the server! (" + reason + ")");
                        Players.Remove(p);
                        break;
                    }
                }
            }

            public void onResourceStart()
            {
                API.consoleOutput("Airport strip script loaded");
                API.setWeather(1);
                API.setTime(10, 0);
            }

            public void onVehicleDeath(NetHandle vehicle)
            {
                if(API.getEntityData(vehicle, "RESPAWNABLE") == true){
                    API.delay(8000, true, () =>
                        {
                            var model = API.getEntityModel(vehicle);
                            var spawnPos = API.getEntityData(vehicle, "SPAWN_POS");
                            var spawnRot = API.getEntityData(vehicle, "SPAWN_ROT");

                            API.deleteEntity(vehicle);

                            var veh = API.createVehicle((VehicleHash) model, spawnPos, spawnRot,0,0);

                            API.setEntityData(veh, "RESPAWNABLE", true);
                            API.setEntityData(veh, "SPAWN_POS", spawnPos);
                            API.setEntityData(veh, "SPAWN_ROT", spawnRot);
                            API.setEntityData(veh, "HasBeenUsed", false);
                            API.setEntityData(veh, "CleanupTicks", 0);
                        });
                }
            }

            public void onPlayerEnterVehicle(Client sender, NetHandle vehicle)
            {
                API.givePlayerWeapon(sender,(WeaponHash)(-72657034) , 1, true, true);
                API.setEntityData(vehicle, "HasBeenUsed", true);
                API.setEntityData(vehicle, "CleanupTicks", 0);
            }

            public void onPlayerPickup(Client sender, NetHandle pickup)
            {
                API.sendChatMessageToPlayer(sender, "pickup");
                foreach (Item item in Items)
                {
                    if (item.netHandle == pickup)
                    {
                        API.sendChatMessageToPlayer(sender, "found item");
                        switch(item.itemType){
                            case "PICKUP_WEAPON_ASSAULTRIFLE":
                                API.sendChatMessageToPlayer(sender, "found wep");
                                API.givePlayerWeapon(sender, (WeaponHash)(-1074790547), 999, false, false);
                                break;
                        }
                    }
                }
            }

        #endregion

        #region "Commands"

            [Command("test")]
            public void PlaceMine(Client sender, String txt)
            {
                API.consoleOutput("test command working: " + txt);
            }

            [Command("savepos")]
            public void SavePlayerPos(Client sender)
            {

                var rot = API.getEntityRotation(sender.handle);
                var pos = sender.position;

                API.consoleOutput(rot + "  :  " + pos);

                using (StreamWriter writer = new StreamWriter(@"C:\pos.txt", true))
                {
                    writer.WriteLine(rot + "  :  " + pos);
                }
            }

            [Command("v")]
            public void SpawnCarCommand(Client sender, VehicleHash model)
            {
                Player p = GetPlayerClientObj(sender); 
                if(p.hasVehicle)
                {
                    API.deleteEntity(p.vehicle);
                }
                else
                {
                    p.hasVehicle = true;
                }

                var rot = API.getEntityRotation(p.client.handle);
                p.vehicle = API.createVehicle(model, p.client.position, new Vector3(0, 0, rot.Z), 0, 0);
                API.setEntityData(p.vehicle, "RESPAWNABLE", false);
                API.setPlayerIntoVehicle(p.client, p.vehicle, -1);
        
            }


        #endregion

        #region "Functions"

            private Player GetPlayerClientObjByHandle(NetHandle handle)
            {
                foreach (Player p in Players)
                {
                    if (p.netHandle == handle)
                    {
                        return p;
                    }
                }
                return new Player();
            }

            private Player GetPlayerClientObj(Client sender)
            {
                foreach (Player p in Players)
                {
                    if (p.client == sender)
                    {
                        return p;
                    }
                }
                return new Player();
            }

            private void SetupPlayerSpawn(Client sender)
            {
                API.setEntityPosition(sender.handle, new Vector3(1691.427, 3285.981, 41.14657));
                API.setEntityRotation(sender.handle, new Vector3(0, 0, -146.1883));
                API.givePlayerWeapon(sender, (WeaponHash)(-1074790547), 999, false, false);
            }

            private void GetAllVehicleData()
            {
                using (StreamReader rdr = new StreamReader(@"resources\airportstrip\vehicles.txt"))
                {
                    while (!rdr.EndOfStream)
                    {
                        string s = rdr.ReadLine();
                        string[] vehData = s.Split(';');

                        Vector3 pos = new Vector3(double.Parse(vehData[1]), double.Parse(vehData[2]), double.Parse(vehData[3]));
                        Vector3 rot = new Vector3(double.Parse(vehData[4]), double.Parse(vehData[5]), double.Parse(vehData[6]));
                        VehicleHash modelVeh = API.vehicleNameToModel(vehData[0]);

                        Structure.Vehicle veh = new Structure.Vehicle(pos, rot, modelVeh);
                        Vehicles.Add(veh);

                    }
                }
            }

            private void GetAllPickupData()
            {
                using (StreamReader rdr = new StreamReader(@"resources\airportstrip\pickups.txt"))
                {
                    while (!rdr.EndOfStream)
                    {
                        string s = rdr.ReadLine();
                        string[] pickupData = s.Split(';');
                        Vector3 pos = new Vector3(double.Parse(pickupData[1]), double.Parse(pickupData[2]), double.Parse(pickupData[3]));
                        Vector3 rot = new Vector3(double.Parse(pickupData[4]), double.Parse(pickupData[5]), double.Parse(pickupData[6]));
                        PickupHash modelPickup = (PickupHash)API.getHashKey(pickupData[0]);                       

                        Item item = new Item(modelPickup, pos, rot, 5, pickupData[0]);
                        Items.Add(item);
                    }
                }
            }

            private void SpawnAllVehicles()
            {
                API.consoleOutput("Spawning all vehicles");
                foreach(NetHandle h in API.getAllVehicles())
                {
                    API.deleteEntity(h);
                }

                foreach (Structure.Vehicle veh in Vehicles)
                {
                    var car = API.createVehicle(veh.Hash, veh.position, veh.rotation, 0, 0);
                    API.setEntityData(car, "RESPAWNABLE", true);
                    API.setEntityData(car, "SPAWN_POS", veh.position);
                    API.setEntityData(car, "SPAWN_ROT", veh.rotation);
                    API.setEntityData(car, "HasBeenUsed", false);
                    API.setEntityData(car, "CleanupTicks", 0);                 
                }

            }

            private void SpawnAllPickupItems()
            {
                API.consoleOutput("Spawning all items");
                foreach(NetHandle p in API.getAllPickups())
                {
                    API.deleteEntity(p);
                }

                foreach (Item item in Items)
                {
                    item.netHandle = API.createPickup(item.model, item.pos, item.rot, 1, item.respawnTime);            
                }
            }

            private void DoVehicleCleanup()
            {
                try
                {
                    foreach (NetHandle veh in API.getAllVehicles())
                    {
                        if (API.getEntityData(veh, "RESPAWNABLE"))
                        {
                            if (API.getEntityData(veh, "HasBeenUsed"))
                            {
                                bool vehInUse = false;
                                foreach (Player p in Players)
                                {
                                    if (API.isPlayerInAnyVehicle(p.client))
                                    {
                                        if (API.getPlayerVehicle(p.client) == veh)
                                        {
                                            vehInUse = true;
                                            break;
                                        }
                                    }
                                }
                                if (!vehInUse)
                                {
                                    if (API.getEntityData(veh, "CleanupTicks") != ClearVehicleAfterTicks)
                                    {
                                        API.setEntityData(veh, "CleanupTicks", API.getEntityData(veh, "CleanupTicks") + 1);
                                        continue;                                  
                                    }
                                    var model = API.getEntityModel(veh);
                                    var spawnPos = API.getEntityData(veh, "SPAWN_POS");
                                    var spawnRot = API.getEntityData(veh, "SPAWN_ROT");
                                    
                                    API.deleteEntity(veh);

                                    var vehicle = API.createVehicle((VehicleHash)model, spawnPos, spawnRot, 0, 0);

                                    API.setEntityData(vehicle, "RESPAWNABLE", true);
                                    API.setEntityData(vehicle, "SPAWN_POS", spawnPos);
                                    API.setEntityData(vehicle, "SPAWN_ROT", spawnRot);
                                    API.setEntityData(vehicle, "HasBeenUsed", false);
                                    API.setEntityData(vehicle, "CleanupTicks", 0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    API.consoleOutput("caught error: " + ex.Message);
                }
            }


        #endregion
       
    }

}