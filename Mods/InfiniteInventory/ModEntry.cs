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
            if (Context.IsWorldReady)
            {
                this.Monitor.Log($"Menu: {e.NewMenu.GetType().FullName}");

            }
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            iv = new InfInv(instance);
        }

        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {

        }

        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {

        }

        private void TimeEvents_AfterDayStarted(object sender, EventArgs e)
        {

        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady)
            {
                if (e.Button == SButton.RightControl)
                {

                }
            }
        }

        public int rand(int val, int add)
        {
            return (int)(Math.Round((rnd.NextDouble() * val) + add));
        }





    }
}