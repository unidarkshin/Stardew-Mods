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
        public Texture2D back;

        public ModEntry()
        {
            instance = this;
        }

        public override void Entry(IModHelper helper)
        {
            rnd = new Random();
            back = Helper.Content.Load<Texture2D>("backpack.png");

            InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            TimeEvents.AfterDayStarted += TimeEvents_AfterDayStarted;
            GameEvents.OneSecondTick += GameEvents_OneSecondTick;
            SaveEvents.BeforeSave += SaveEvents_BeforeSave;
            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            MenuEvents.MenuChanged += MenuEvents_MenuChanged;
            GraphicsEvents.OnPostRenderEvent += GraphicsEvents_OnPostRenderEvent;

            helper.ConsoleCommands.Add("buy_tab", "Buys the next inventory tab.", this.buy_tab);
            helper.ConsoleCommands.Add("set_tab", "(CHEAT) Sets the number of tabs in you inventory. Syntax: set_tab <Integer>", this.set_tab);

            helper.ConsoleCommands.Add("cost", "Cost for the next inventory tab.", this.cost);
            helper.ConsoleCommands.Add("set_cost", "Sets cost multiplier for tabs. Syntax: set_cost <Integer>. Default: 30000.", this.set_cost);
        }

        private void GraphicsEvents_OnPostRenderEvent(object sender, EventArgs e)
        {
            if (Context.IsWorldReady && Game1.activeClickableMenu is GameMenu && Game1.player.MaxItems >= 36)
            {
                List<IClickableMenu> tabs = this.Helper.Reflection.GetField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").GetValue();
                IClickableMenu curTab = tabs[(Game1.activeClickableMenu as GameMenu).currentTab];

                if (curTab is InventoryPage)
                {
                    Game1.spriteBatch.Draw(back, new Vector2(curTab.xPositionOnScreen+curTab.width - 100, curTab.yPositionOnScreen+curTab.height - 100), new Rectangle?(new Rectangle(0, 0, back.Width, back.Height)), Color.White, 0.0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, 0.5f);
                    
                    Game1.spriteBatch.DrawString(Game1.smallFont, $"Tab: {iv.currTab}", new Vector2(curTab.xPositionOnScreen + curTab.width - 150, curTab.yPositionOnScreen + curTab.height - 300), new Color(36,47,48), 0.0f, Vector2.Zero, (float)(Game1.pixelZoom/3), SpriteEffects.None, 0.5f);

                    Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0.1f);
                }
            }
        }

        private void set_cost(string arg1, string[] arg2)
        {
            if (int.TryParse(arg2[0], out int r))
            {
                iv.cost = r;
                return;
            }

            Monitor.Log("Error: Invalid parameters. Syntax: set_cost <Integer>", LogLevel.Error);
        }

        private void cost(string arg1, string[] arg2)
        {
            Monitor.Log($"Cost for tab {iv.maxTab + 1}: {iv.maxTab * iv.cost}; cost = {iv.cost}.");
        }

        private void set_tab(string arg1, string[] arg2)
        {
            if (int.TryParse(arg2[0], out int r))
            {
                if (r > 1 && r > iv.maxTab)
                {
                    iv.maxTab = r;

                    Monitor.Log("Set_tab was successful.");
                    return;
                }
            }

            Monitor.Log("Error: Invalid parameters. Syntax: set_tab <Integer>", LogLevel.Error);
        }

        private void buy_tab(string arg1, string[] arg2)
        {
            int cost = (iv.maxTab) * iv.cost;

            if (Game1.player.Money < cost)
            {
                Game1.chatBox.addMessage($"You dont have {cost} coins for tab {iv.maxTab + 1}.", Color.DarkRed);

                return;
            }
            else
            {
                Game1.player.Money -= cost;

                iv.maxTab++;

                Game1.chatBox.addMessage($"You successfully purchased tab {iv.maxTab} for {cost} coins.", Color.ForestGreen);
            }
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
            if (Context.IsWorldReady && Game1.activeClickableMenu is GameMenu && Game1.player.MaxItems >= 36)
            {
                List<IClickableMenu> tabs = this.Helper.Reflection.GetField<List<IClickableMenu>>(Game1.activeClickableMenu, "pages").GetValue();
                IClickableMenu curTab = tabs[(Game1.activeClickableMenu as GameMenu).currentTab];
                if (curTab is InventoryPage)
                {
                    if (e.Button == SButton.NumPad1)
                    {
                        iv.changeTabs(iv.currTab - 1);
                    }
                    else if (e.Button == SButton.NumPad2)
                    {
                        iv.changeTabs(iv.currTab + 1);
                    }

                    if(e.Button == SButton.MouseLeft)
                    {
                        int xval = curTab.xPositionOnScreen + curTab.width;
                        int yval = curTab.yPositionOnScreen + curTab.height;
                        if (e.Cursor.ScreenPixels.X > (xval - 100) && e.Cursor.ScreenPixels.X < (xval - 50) && e.Cursor.ScreenPixels.Y > (yval - 100) && e.Cursor.ScreenPixels.Y < (yval - 50))
                        {
                            string[] str = {""};
                            buy_tab("buy_tab", str);
                        }
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
        public int cost { get; set; }

        public ModData()
        {
            this.cost = 30000;
            this.maxTab = 1;

            this.itemInfo = new List<List<string>>();


        }
    }

}