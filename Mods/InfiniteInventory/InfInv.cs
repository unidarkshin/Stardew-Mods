using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Menus;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using System.Timers;
using StardewValley.TerrainFeatures;
using StardewValley.Monsters;
using StardewValley.Locations;
using System.Collections;

namespace InfiniteInventory
{
    
    public class InfInv
    {
        public IList<Item> items;
        public ArrayList invs;
        public int currTab;
        public int maxTab;
        public static Mod instance;

        public InfInv(Mod ins)
        {
            instance = ins;

            items = new List<Item>();
            invs = new ArrayList();
            currTab = 1;
            maxTab = 5;
        }

        public void loadData()
        {

        }

        public void saveData()
        {

        }

        public void changeTabs(int n)
        {
            if (currTab != n && n <= maxTab && n > 0)
            {
                invs[currTab] = Game1.player.Items;
                if (invs[n] == null)
                {
                    invs[n] = new List<Item>();
                }

                Game1.player.Items = (List<Item>)invs[n];

                currTab = n;
            }
            else
            {
                instance.Monitor.Log($"You dont have {n} inventory tabs unlocked!");
                return;
            }
        }
    }
}
