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
    /// <summary>The mod entry point.</summary>
    /// 
    public class ModEntry : Mod
    {
        public static Random rnd;
        public static InfInv iv;
        public static Mod instance;

        public ModEntry()
        {
            instance = this;
        }

        public override void Entry(IModHelper helper)
        {
            rnd = new Random();

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
        }

        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {

        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            iv = new InfInv(instance);
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            iv.changeTabs(1);
            iv.saveData();
        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {

        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {

        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady && Game1.activeClickableMenu is GameMenu)
            {
                List<IClickableMenu> tabs = this.Helper.Reflection.GetField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").GetValue();
                IClickableMenu curTab = tabs[(Game1.activeClickableMenu as GameMenu).currentTab];
                if (curTab is InventoryPage)
                {
                    if (e.Button == SButton.NumPad1)
                    {
                        iv.changeTabs(iv.currTab - 1);
                    }
                    else if(e.Button == SButton.NumPad2)
                    {
                        iv.changeTabs(iv.currTab + 1);
                    }
                }

            }
        }

        public int rand(int val, int add)
        {
            return (int)(Math.Round((rnd.NextDouble() * val) + add));
        }





    }

    public class ModData
    {
        public List<List<string>> itemInfo { get; set; }
        

        public int maxTab { get; set; }

        public ModData()
        {

            this.itemInfo = new List<List<string>>();
            
            this.maxTab = 5;
        }
    }

}