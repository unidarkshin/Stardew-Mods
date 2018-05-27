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

namespace ModEntry
{
    /// <summary>The mod entry point.</summary>
    /// 
    public class ModEntry : Mod
    {
        public static Random rnd;
        public static Mod instance;

        public bool sprinting = false;

        public int dSpeed;
        public int factor;

        public ModEntry()
        {
            instance = this;
        }

        public override void Entry(IModHelper helper)
        {
            rnd = new Random();
            dSpeed = Game1.player.addedSpeed;
            factor = 3;

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            GameEvents.HalfSecondTick += GameEvents_HalfSecondTick;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;

            //helper.ConsoleCommands.Add("buy_tab", "Buys the next inventory tab.", this.buy_tab);
        }

        private void GameEvents_HalfSecondTick(object sender, EventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (sprinting && Game1.player.Stamina > 0.0f)
            {
                Game1.player.Stamina -= 1.5f;
                Game1.player.addedSpeed = dSpeed + factor;

            }
            else if(sprinting && Game1.player.Stamina <= 0.0f)
            {
                sprinting = false;
            }


            if (!sprinting && Game1.player.addedSpeed == dSpeed + factor)
            {
                Game1.player.addedSpeed = dSpeed;
            }



        }

        private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null || Game1.player.Stamina <= 0f)
                return;

            if (e.Button == SButton.LeftControl)
            {
                sprinting = true;
            }
        }

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button == SButton.LeftControl)
            {
                //Monitor.Log("--------> Released");
                sprinting = false;
            }
        }

        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {

        }


        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {

        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {

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

        public int rand(int val, int add)
        {
            return (int)(Math.Round((rnd.NextDouble() * val) + add));
        }


    }

}
