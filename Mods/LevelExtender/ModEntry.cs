using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Reflection;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Locations;
using StardewValley.Characters;
using System.Collections.Generic;
using System.Collections;
using System.Timers;
using System.Linq;
using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Netcode;
using Newtonsoft.Json;
using System.IO;

namespace LevelExtender
{
    /// <summary>The mod entry point.</summary>

    public class ModEntry : Mod, IAssetEditor
    {
        public static Mod instance;
        private static System.Timers.Timer aTimer;
        int[] oldXP = { 0, 0, 0, 0, 0 };
        int[] newXP = { 0, 0, 0, 0, 0 };
        bool[] old = { false, false, false, false, false };
        //int[] addedXP = { 0, 0, 0, 0, 0 };
        int[] sLevs = { 0, 0, 0, 0, 0 };
        int[] max = { 100, 100, 100, 100, 100 };
        bool firstFade = false;
        ModData config = new ModData();
        public static Random rand = new Random(Guid.NewGuid().GetHashCode());
        int[] origLevs = { 0, 0, 0, 0, 0 };
        int[] origExp = { 0, 0, 0, 0, 0 };
        bool wm = false;
        bool pres_comp = false;
        int[] oldLevs = { 0, 0, 0, 0, 0 };
        int[] newLevs = { 0, 0, 0, 0, 0 };
        bool[] olev = { false, false, false, false, false };
        bool[] shLev = { true, true, true, true, true };
        double xp_mod = 1.0;

        float oStamina = 0.0f;
        public bool initialtooluse = false;



        bool no_mons = false;

        private LEModApi API;

        public LEEvents LEE;

        public ModEntry LE;

        public EXP addedXP;
        private int total_m;
        private double s_mod;

        public MPModApi mpmod;
        private bool mpload;
        private double mpMult;

        private Timer aTimer2 = new Timer();
        private bool showXPBar;
        //private Timer xpBarTimer = new Timer();

        int[] dxp = { 0, 0, 0, 0, 0 };

        List<Timer> xpBarTimers = new List<Timer>();

        //int skillCount = 5;

        List<XPBar> xpBars = new List<XPBar>();

        public static List<string> snames = new List<string>();

        HarmonyInstance harmony;

        public static List<Monster> monsters = new List<Monster>();

        public List<Skill> skills = new List<Skill>();
        //public static List<Skill> tskills = new List<Skill>();
        public static List<int[]> categories = new List<int[]>();

        JsonSerializer serializer = new JsonSerializer();

        public static List<int> skillLevs = new List<int>();


        public ModEntry()
        {
            instance = this;
            LE = this;
            LEE = new LEEvents();
            //addedXP = new EXP(LEE);
            total_m = 0;
            s_mod = -1.0;
            mpload = false;
            mpMult = 1.0;

            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
        }

        public override object GetApi()
        {
            return API = new LEModApi(this);
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Fish");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {

            IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
            foreach (var pair in data.ToArray())
            {
                string[] fields = pair.Value.Split('/');
                if (int.TryParse(fields[1], out int val))
                {
                    int x = Math.Max(val - rand.Next(0, (int)(Game1.player.fishingLevel.Value / 4)), val / 2);
                    //if (x < 1)
                    //    x = rand.Next(1, val);
                    fields[1] = x.ToString();
                    data[pair.Key] = string.Join("/", fields);
                }
            }
        }


        public override void Entry(IModHelper helper)
        {
            Initialize(instance.Monitor);

            harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            Type[] types1 = { typeof(Microsoft.Xna.Framework.Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) };
            Type[] types2 = { typeof(Item) };
            Type[] types3 = { typeof(Item), typeof(bool) };

            harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.GameLocation), "damageMonster", types1), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.DM))
                );
            /*harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Farmer), "addItemToInventory", types2), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.AITI1))
                );*/
            harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Farmer), "addItemToInventoryBool", types3), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.AITI2))
                );
            /*harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Menus.ItemGrabMenu), "dropHeldItem"), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.DHI))
                );
            harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Menus.ItemGrabMenu), "dropRemainingItems"), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.DHI))
                );*/

            /*harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Monsters.Monster), "takeDamage"), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    postfix: new HarmonyMethod(typeof(ModEntry), nameof(this.TD))
                );*/

            //instance = this;
            helper.Events.GameLoop.OneSecondUpdateTicked += this.GameEvents_OneSecondTick;
            helper.Events.GameLoop.UpdateTicked += this.GameEvents_QuarterSecondTick;
            helper.Events.GameLoop.GameLaunched += this.GameEvents_FirstUpdateTick;
            helper.Events.GameLoop.SaveLoaded += this.SaveEvents_AfterLoad;
            helper.Events.GameLoop.Saving += this.SaveEvents_BeforeSave;
            helper.Events.GameLoop.ReturnedToTitle += this.SaveEvents_AfterReturnToTitle;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonPressed += this.ControlEvent_KeyPressed;
            helper.Events.GameLoop.DayStarted += this.TimeEvent_AfterDayStarted;
            helper.Events.Input.ButtonReleased += this.ControlEvent_KeyReleased;
            helper.Events.Display.Rendered += this.Display_Rendered;
            helper.Events.Player.Warped += this.Player_Warped;

            //LEE.OnXPChanged += LEE;

            helper.ConsoleCommands.Add("xp", "Displays the xp table for your current levels.", this.XPT);
            helper.ConsoleCommands.Add("lev", "Sets the player's level: lev <type> <number>", this.SetLev);
            helper.ConsoleCommands.Add("wm_toggle", "Toggles monster spawning: wm_toggle", this.WmT);
            helper.ConsoleCommands.Add("xp_m", "Changes the xp modifier for levels 10 and after: xp_m <decimal 0.0 -> ANY> : 1.0 is default.", this.XpM);
            helper.ConsoleCommands.Add("spawn_modifier", "Forcefully changes monster spawn rate to specified decimal value: spawn_modifier <decimal(percent)> : -1.0 to not have any effect.", this.SM);
            helper.ConsoleCommands.Add("xp_table", "Tells the players current XP above or at level 10.", this.TellXP);
            helper.ConsoleCommands.Add("set_xp", "Sets your current XP for a given skill: set_xp <skill> <XP: int 0 -> ANY>", this.SetXP);
            helper.ConsoleCommands.Add("draw_bars", "Sets whether the XP bars should be drawn or not: draw_bars <bool>, Default; true.", this.DrawBars);
            //helper.ConsoleCommands.Add("LE_cmds", "Displays the xp table for your current levels.", this.XPT);

            this.Helper.Content.InvalidateCache("Data/Fish");
            LEE.OnXPChanged += this.OnXPChanged;


        }

        private void DrawBars(string arg1, string[] arg2)
        {
            if (!bool.TryParse(arg2[0], out bool val))
                return;

            config.drawBars = val;
            Monitor.Log($"You succesfully set draw XP bars to {val}.");
        }

        public static bool AITI2(Item item, bool makeActiveObject)
        {
            try
            {
                if (item == null || item.HasBeenInInventory)
                    return true;

                //Monitor.Log($"Picking up item {item.DisplayName}");
                int cat = item.Category;
                string str = "";

                //tskills = skills
                int i = 0;

                foreach (int[] cats in categories)
                {
                    if (cats.Contains(cat) && ShouldDup(i))
                    {
                        item.Stack += 1;
                        str = $"Your {snames[i]} level allowed you to obtain an extra {item.DisplayName}!";
                        break;
                    }

                    i++;
                }

                /*if ((cat == -16 || cat == -74 || cat == -75 || cat == -79 || cat == -80 || cat == -81) && ShouldDup(0, 2))
                {
                    item.Stack += 1;
                    str = $"Your Farming/Foraging level allowed you to obtain an extra {item.DisplayName}!";
                }
                else if ((cat == -4) && ShouldDup(1))
                {
                    item.Stack += 1;
                    str = $"Your Fishing level allowed you to obtain an extra {item.DisplayName}!";
                }
                else if ((cat == -2 || cat == -12 || cat == -15) && ShouldDup(3))
                {
                    item.Stack += 1;
                    str = $"Your Mining level allowed you to obtain an extra {item.DisplayName}!";
                }
                else if ((cat == -28 || cat == -29 || cat == -95 || cat == -96 || cat == -98) && ShouldDup(4))
                {
                    item.Stack += 1;
                    str = $"Your Combat level allowed you to obtain an extra {item.DisplayName}!";
                }*/

                if (str.Length > 0 && item.salePrice() >= 100)
                    Game1.chatBox.addMessage(str, Color.DeepSkyBlue);
                //item.HasBeenInInventory = true;

                return true;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(AITI2)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {

        }

        public static bool DM(
      Microsoft.Xna.Framework.Rectangle areaOfEffect,
      int minDamage,
      int maxDamage,
      bool isBomb,
      float knockBackModifier,
      int addedPrecision,
      float critChance,
      float critMultiplier,
      bool triggerMonsterInvincibleTimer,
      Farmer who)
        {
            try
            {
                GameLocation cl = Game1.currentLocation;
                if (!cl.IsOutdoors)
                    return true;

                int dmg = 0;

                bool flag1 = false;

                for (int index = cl.characters.Count - 1; index >= 0; --index)
                {
                    if (index < cl.characters.Count && cl.characters[index] is Monster character && (character.IsMonster && character.Health > 0) && character.TakesDamageFromHitbox(areaOfEffect))
                    {
                        if (!monsters.Contains(character))
                            continue;

                        if (character.currentLocation == null)
                            character.currentLocation = cl;
                        if (!character.IsInvisible && !character.isInvincible() && (isBomb || instance.Helper.Reflection.GetMethod(Game1.currentLocation, "isMonsterDamageApplicable").Invoke<bool>(who, character, true) || instance.Helper.Reflection.GetMethod(Game1.currentLocation, "isMonsterDamageApplicable").Invoke<bool>(who, character, false)))
                        {
                            bool flag2 = !isBomb && who != null && (who.CurrentTool != null && who.CurrentTool is MeleeWeapon) && (int)(NetFieldBase<int, NetInt>)(who.CurrentTool as MeleeWeapon).type == 1;
                            bool flag3 = false;
                            if (flag2 && MeleeWeapon.daggerHitsLeft > 1)
                                flag3 = true;
                            if (flag3)
                                triggerMonsterInvincibleTimer = false;
                            flag1 = true;
                            //if (Game1.currentLocation == this)
                            //Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 8.0), (float)(200 + Game1.random.Next(-50, 50)));
                            Microsoft.Xna.Framework.Rectangle boundingBox = character.GetBoundingBox();
                            Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(boundingBox, who);
                            if ((double)knockBackModifier > 0.0)
                                trajectory *= knockBackModifier;
                            else
                                trajectory = new Vector2(character.xVelocity, character.yVelocity);
                            if (character.Slipperiness == -1)
                                trajectory = Vector2.Zero;
                            bool flag4 = false;
                            if (who != null && who.CurrentTool != null && character.hitWithTool(who.CurrentTool))
                                return true;
                            if (who.professions.Contains(25))
                                critChance += critChance * 0.5f;
                            int amount1;
                            if (maxDamage >= 0)
                            {
                                int num = Game1.random.Next(minDamage, maxDamage + 1);
                                if (who != null && Game1.random.NextDouble() < (double)critChance + (double)who.LuckLevel * ((double)critChance / 40.0))
                                {
                                    flag4 = true;
                                    //this.playSound("crit");
                                }
                                int amount2 = Math.Max(1, (flag4 ? (int)((double)num * (double)critMultiplier) : num) + (who != null ? who.attack * 3 : 0));
                                if (who != null && who.professions.Contains(24))
                                    amount2 = (int)Math.Ceiling((double)amount2 * 1.10000002384186);
                                if (who != null && who.professions.Contains(26))
                                    amount2 = (int)Math.Ceiling((double)amount2 * 1.14999997615814);
                                if (who != null & flag4 && who.professions.Contains(29))
                                    amount2 = (int)((double)amount2 * 2.0);
                                if (who != null)
                                {
                                    foreach (BaseEnchantment enchantment in who.enchantments)
                                        enchantment.OnCalculateDamage(character, character.currentLocation, who, ref amount2);
                                }
                                //amount1 = character.takeDamage(amount2, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
                                dmg = Math.Max(1, amount2 - (int)(NetFieldBase<int, NetInt>)character.resilience);
                                //this.seenPlayer.Value = true;
                                if (Game1.random.NextDouble() < (double)(NetFieldBase<double, NetDouble>)character.missChance - (double)(NetFieldBase<double, NetDouble>)character.missChance * addedPrecision)
                                {
                                    //num = -1;
                                    return true;
                                }
                            }
                            if (character.Health - dmg <= 0)
                            {

                                who.gainExperience(4, character.ExperienceGained);

                            }

                        }
                    }
                }

                return true; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(DM)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        DateTime otime;

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (otime == null)
                otime = DateTime.Now;

            for (int i = 0; i < xpBars.Count; i++)
            {

                try
                {

                    if (xpBars[i] == null)
                        continue;

                    //string[] skills = { "F a r m i n g", "F i s h i n g", "F o r a g i n g", "M i n i n g", "C o m b a t" };
                    //int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
                    int startX = 8;
                    int startY = 8;
                    int sep = 30;
                    int barSep = 60;

                    /*for (int i = 0; i < skillsToDrawOrder.Count; i++)
                    {


                        int key = skillsToDrawOrder[i];
                        int xp = skillsToDraw[key];
                        int lev = skillLevs[key];
                        int startXP = StartXP(lev);
                        */


                    int key = xpBars[i].skill.key;
                    int xp = xpBars[i].skill.xp;
                    int xpc = xpBars[i].skill.xpc;
                    int lev = xpBars[i].skill.level;
                    int startXP = xpBars[i].skill.StartXP();
                    double deltaTime = DateTime.Now.Subtract(xpBars[i].time).TotalMilliseconds;
                    float transp;

                    /*if (i == 0)
                    {

                        using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(@"C:\Users\lematd\Desktop\deltatime.txt", true))
                        {
                            file.WriteLine($"{DateTime.Now} - {xpBars[i].time} = {deltaTime}");
                        }
                    }*/

                    if (deltaTime >= 0 && deltaTime <= 1000)
                    {
                        transp = ((float)deltaTime) / 1200.0f;
                    }
                    else if (deltaTime > 1000 && deltaTime <= 4000)
                    {
                        transp = 0.833f;
                    }
                    else
                    {
                        transp = ((float)(5000 - deltaTime)) / 1200.0f;
                    }


                    int curXP;

                    /*if (lev < 10 && key < 5)
                    {
                        curXP = Game1.player.experiencePoints[key];
                    }
                    else
                    {
                        curXP = xpBars[i].skill.xp;
                    }*/

                    curXP = xp;

                    int maxXP = xpBars[i].skill.getReqXP();

                    if (startXP > 0)
                    {
                        maxXP = maxXP - startXP;
                        curXP = curXP - startXP;
                        startXP = 0;
                    }

                    int iWidth = 198;
                    double mod = iWidth / (maxXP * 1.0);
                    int bar2w = (int)Math.Round(xpc * mod) + 1;
                    int bar1w = (int)Math.Round(curXP * mod) - bar2w;


                    if (i == 0 && xpBars[i].ych < 0)
                    {
                        double ms = (DateTime.Now - otime).TotalMilliseconds;
                        double addv = (xpBars[i].ych + (ms / 15.625));
                        xpBars[i].ych = (addv >= 0 ? 0 : addv);
                        //Monitor.Log($"NEG yChange Value: {xpBars[i].ych} -> {deltaTime}");
                    }
                    else if (i == 0 && deltaTime >= 4000)
                    {
                        double addv = (deltaTime - 4000) / 15.625;
                        xpBars[i].ych = (addv >= 64 ? 64 : addv);
                        //Monitor.Log($"yChange Value: {xpBars[i].ych} -> {deltaTime}");
                    }


                    xpBars[i].ych = xpBars[0].ych;



                    /*if (deltaTime >= 4970)
                    {
                        xpBars[i].ych = 0;
                        //xpBars[i].startmove = false;
                        Monitor.Log("endmove!");
                    }*/

                    if (config.drawBars)
                    {

                        Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX - 7, startY + (barSep * i) - 7 - xpBars[i].ychi, 214, 64), Color.DarkRed * transp);
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX - 5, startY + (barSep * i) - 5 - xpBars[i].ychi, 210, 60), new Color(210, 173, 85) * transp);
                        //Game1.spriteBatch.DrawString(Game1.dialogueFont, $"{skills[key]}", new Vector2((int)Math.Round(((startX - 7 + 214) / 2.0) - ((skills[key].Length / 2.0) * 12)), startY - 3 + (barSep * i) - xpBars[i].ychi), Color.Black * transp, 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 6f), SpriteEffects.None, 0.5f);
                        //Game1.spriteBatch.DrawString(Game1.dialogueFont, $"{skills[key]}", new Vector2((int)Math.Round(((startX - 7 + 214) / 2.0) - ((skills[key].Length / 2.0) * 12)) + 1, startY - 3 + (barSep * i) + 1 - xpBars[i].ychi), Color.Black * transp, 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 6f), SpriteEffects.None, 0.5f);
                        Vector2 sn1loc = new Vector2((int)Math.Round(((startX - 7 + 214) / 2.0) - (Game1.dialogueFont.MeasureString(xpBars[i].skill.name).X * (Game1.pixelZoom / 6.0f / 2.0f))), startY - 3 + (barSep * i) - xpBars[i].ychi);
                        //Utility.drawTextWithColoredShadow(Game1.spriteBatch, skills[key], Game1.dialogueFont, sn1loc, Color.Black * (transp), new Color(90, 35, 0) * transp, Game1.pixelZoom / 6f, 0.5f);

                        Game1.spriteBatch.DrawString(Game1.dialogueFont, $"{skills[key].name}", new Vector2((int)Math.Round(((startX - 7 + 214) / 2.0) - (Game1.dialogueFont.MeasureString(xpBars[i].skill.name).X * (Game1.pixelZoom / 6.0f / 2.0f))), startY - 3 + (barSep * i) - xpBars[i].ychi), new Color(30, 3, 0) * (transp * 1.1f), 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 6f), SpriteEffects.None, 0.5f);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, $"{skills[key].name}", new Vector2((int)Math.Round(((startX - 7 + 214) / 2.0) - (Game1.dialogueFont.MeasureString(xpBars[i].skill.name).X * (Game1.pixelZoom / 6.0f / 2.0f))) + 1, startY - 3 + (barSep * i) - xpBars[i].ychi + 1), new Color(90, 35, 0) * (transp), 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 6.0f), SpriteEffects.None, 0.5f);

                        Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX, startY + (barSep * i) + sep - xpBars[i].ychi, 200, 20), Color.Black * transp);
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX + 1, startY + (barSep * i) + sep + 1 - xpBars[i].ychi, bar1w, 18), Color.SeaGreen * transp);
                        Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX + 1 + bar1w, startY + (barSep * i) + sep + 1 - xpBars[i].ychi, bar2w, 18), Color.Turquoise * transp);

                        Vector2 mPos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
                        Vector2 bCenter = new Vector2(startX + (200 / 2), startY + (barSep * i) + sep + (20 / 2) - xpBars[i].ychi);
                        float dist = Vector2.Distance(mPos, bCenter);

                        if (dist <= 250f)
                        {
                            /*if (lev < 10 && xpBars[i].skill.key < 5)
                            {
                                curXP = Game1.player.experiencePoints[key];
                            }
                            else
                            {
                                curXP = xpBars[i].skill.xp;
                            }*/
                            curXP = xp;

                            maxXP = xpBars[i].skill.getReqXP();

                            float f = Math.Min(25f / dist, 1.0f);

                            string xpt = $"{curXP} / {maxXP}";

                            Game1.spriteBatch.DrawString(Game1.dialogueFont, xpt, new Vector2((int)Math.Round(((startX + 200) / 2.0) - (Game1.dialogueFont.MeasureString(xpt).X * (Game1.pixelZoom / 10.0f / 2.0f))), startY + (barSep * i) + sep + 1 - xpBars[i].ychi), Color.White * f * (transp + 0.05f), 0.0f, Vector2.Zero, (Game1.pixelZoom / 10f), SpriteEffects.None, 0.5f);
                        }

                    }
                }

                catch (Exception ex)
                {
                    Monitor.Log($"Non-Serious draw violation: {ex.Message}");
                    continue;
                }

            }



            otime = DateTime.Now;





            //Game1.spriteBatch.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0.0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0.1f);
        }

        /* public int StartXP(int lev)
         {
             int xp = 0;

             if (lev > 0 && lev < 10)
             {
                 xp = defReqXPs[lev - 1];
             }

             return xp;
         }*/

        private void SetXP(string command, string[] arg)
        {
            if (!Context.IsWorldReady || arg.Length < 2 || !int.TryParse(arg[1], out int xp))
                return;

            //int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
            Skill skill = skills.SingleOrDefault(sk => sk.name.Equals(arg[0], StringComparison.OrdinalIgnoreCase));

            if (skill == null)
            {
                Monitor.Log($"Invalid skill name: {arg[0]}");
                return;
            }

            if (skill.key < 5)
                Game1.player.experiencePoints[skill.key] = Math.Min(xp, skill.getReqXP() - 2);
            else
                skill.xp = xp;

            /*if (arg[0].ToLower() == "farming")
            {
                if (skillLevs[0] < 10)
                    Game1.player.experiencePoints[0] = Math.Min(xp, GetReqXP(skillLevs[0]) - 2);
                else
                    addedXP[0] = xp;
            }
            else if (arg[0].ToLower() == "fishing")
            {
                if (skillLevs[1] < 10)
                    Game1.player.experiencePoints[1] = Math.Min(xp, GetReqXP(skillLevs[1]) - 2);
                else
                    addedXP[1] = xp;
            }
            else if (arg[0].ToLower() == "foraging")
            {
                if (skillLevs[2] < 10)
                    Game1.player.experiencePoints[2] = Math.Min(xp, GetReqXP(skillLevs[2]) - 2);
                else
                    addedXP[2] = xp;
            }
            else if (arg[0].ToLower() == "mining")
            {
                if (skillLevs[3] < 10)
                    Game1.player.experiencePoints[3] = Math.Min(xp, GetReqXP(skillLevs[3]) - 2);
                else
                    addedXP[3] = xp;
            }
            else if (arg[0].ToLower() == "combat")
            {
                if (skillLevs[4] < 10)
                    Game1.player.experiencePoints[4] = Math.Min(xp, GetReqXP(skillLevs[4]) - 2);
                else
                    addedXP[4] = xp;
            }
            else
            {
                Monitor.Log("ERROR - setXP: Invalid skill name.");
            }*/
        }

        private new static IMonitor Monitor;

        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        private void ControlEvent_KeyReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

        }

        private void GameEvents_FirstUpdateTick(object sender, EventArgs e)
        {

        }

        List<DateTime> xpBarStartTime = new List<DateTime>();

        private void SetTimer(int time, int index)
        {

            if (index == 0)
            {
                // Create a timer with a two second interval.
                aTimer = new System.Timers.Timer(1100);
                // Hook up the Elapsed event for the timer. 
                aTimer.Elapsed += OnTimedEvent;
                aTimer.AutoReset = false;
                aTimer.Enabled = true;

            }
            else if (index == 1)
            {
                // Create a timer with a two second interval.
                aTimer2 = new System.Timers.Timer(time);
                // Hook up the Elapsed event for the timer. 
                aTimer2.Elapsed += OnTimedEvent2;

                aTimer2.AutoReset = false;
                aTimer2.Enabled = true;

            }
            else if (index == 2)
            {
                shouldDraw = new System.Timers.Timer(time);
                // Hook up the Elapsed event for the timer. 
                shouldDraw.Elapsed += sDrawEnd;

                shouldDraw.AutoReset = false;
                shouldDraw.Enabled = true;
            }



        }

        private void sDrawEnd(object sender, ElapsedEventArgs e)
        {
            shouldDraw.Enabled = false;
        }

        public void EndXPBar(int key)
        {

            //xpBars[0] = null;
            //xpBars.RemoveAt(0);
            //pushElementsToZero(xpBars);

            var bar = xpBars.SingleOrDefault(x => x.skill.key == key);
            double yval = bar.ych;

            xpBars.Remove(bar);

            pushElementsToZero(xpBars);

            xpBars[0].ych = 0;
            //setYchVals(yval * -1);

        }

        private void pushElementsToZero(List<int> list)
        {
            List<int> temp = new List<int>();

            foreach (var item in list)
            {
                temp.Add(item);
            }

            list = temp;
        }
        private void pushElementsToZero(List<Timer> list)
        {
            List<Timer> temp = new List<Timer>();

            foreach (var item in list)
            {
                temp.Add(item);
            }

            list = temp;
        }
        private void pushElementsToZero(List<XPBar> list)
        {
            List<XPBar> temp = new List<XPBar>();

            foreach (var item in list)
            {
                temp.Add(item);
            }

            list = temp;
        }

        private void OnTimedEvent2(object sender, ElapsedEventArgs e)
        {
            mpMult = mpmod.Exp_Rate();
            aTimer2.Enabled = false;
        }

        private void XPT(string arg1, string[] arg2)
        {

            Monitor.Log("Skill:  | Level:  |  Current Experience:  | Experience Needed:", LogLevel.Info);

            for (int i = 0; i < skills.Count; i++)
            {
                int xpn = skills[i].getReqXP();
                Monitor.Log($"{skills[i].name} | {skills[i].level} | {skills[i].xp} | {xpn}", LogLevel.Info);
            }

            /*int[] skillLevs = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            for (int i = 0; i < 5; i++)
            {
                double xpn = GetReqXP(skillLevs[i]);
                Monitor.Log($"{snames[i]} | {skillLevs[i]} | {addedXP[i]} | {xpn}", LogLevel.Info);
                //Monitor.Log($"     {i} | {Math.Round((1000 * i + (i * i * i * 0.33)) * xp_mod)}");
            }*/

        }

        private void SM(string command, string[] args)
        {
            if (args.Length < 1 || args[0] == null || !double.TryParse(args[0], out double n))
            {
                Monitor.Log("No decimal value found.");
                return;
            }

            s_mod = n;
            Monitor.Log($"Modifier set to {n * 100}%.");

        }

        private void OnXPChanged(object sender, EXPEventArgs e)
        {
            XPBar bar = xpBars.SingleOrDefault(b => b.skill.key == e.key);
            Skill skill = skills.SingleOrDefault(sk => sk.key == e.key);

            if (skill == null || skill.xpc < 0 || skill.xpc > 100001 || shouldDraw.Enabled)
                return;

            //Monitor.Log($"XP Changed: index {e.key}, EXP {e.xp}");

            //bool exists = false;


            if (bar != null)
            {
                bar.timer.Stop();
                bar.timer.Start();
                bar.time = DateTime.Now;
                //xpBars[i].xpc = e.xp;
                //exists = true;

                /*if (i == 0)
                {
                    //xpBars[i].ych = -1 * xpBars[i].ych;
                    xpBars[i].movedir = -1;
                    Monitor.Log("reset on xpchange");
                }*/
                double val = bar.ych * -1;

                //Monitor.Log($"xpchanged, val: {val}");

                setYchVals(val);


                sortByTime();

                //break;                   
            }
            else
            {
                xpBars.Add(new XPBar(skill));
            }


            /*if (!exists)
            {
                
            }*/

            //skillsToDraw[e.key] = e.xp;

            //if (!skillsToDrawOrder.Contains(e.key))
            //skillsToDrawOrder.Add(e.key);

        }

        public void sortByTime()
        {
            List<XPBar> SortedList = xpBars.OrderBy(o => o.time).ToList();

            xpBars = SortedList;
        }
        public void setYchVals(double val)
        {
            foreach (var bar in xpBars)
            {
                bar.ych = val;
            }
        }

        private void TellXP(string command, string[] args)
        {

            /*int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            Monitor.Log($"XP: Farming  Fishing  Foraging  Mining  Combat |");
            for (int i = 0; i < addedXP.Length; i++)
            {
                if (temp[i] >= 10)
                    Monitor.Log($"XP: {addedXP[i]}");
                else
                    Monitor.Log($"XP: Empty.");
            }
            Monitor.Log($"\n");
            for (int i = 0; i < sLevs.Length; i++)
            {
                if (temp[i] >= 10)
                    Monitor.Log($"NL: {GetReqXP(temp[i])}");
                else
                    Monitor.Log($"XP: Empty.");
            }
            for (int i = 0; i < 5; i++)
            {
                Monitor.Log($"Current XP - Default: {Game1.player.experiencePoints[i]}.");
            }*/



        }

        private void SetLev(string command, string[] args)
        {
            if (args[0] == null || args[1] == null || !int.TryParse(args[1], out int n))
            {
                Monitor.Log($"Function Failed!");
                return;
            }
            //n = int.Parse(args[1]);
            if (n < 0 || n > 100)
            {
                Monitor.Log($"Function Failed!");
                return;
            }

            Skill skill = skills.SingleOrDefault(sk => sk.name == args[0]);

            if (skill == null)
                return;

            skill.level = n;

            /*if (args[0].ToLower() == "farming")
            {
                Game1.player.farmingLevel.Value = n;
                Game1.player.experiencePoints[0] = GetDefStartXP(n);

                if (n < 10)
                {
                    sLevs[0] = 10;
                    addedXP[0] = 0;
                    config.FaLV = 10;
                    config.FaXP = 0;
                    oldXP[0] = 0;
                    newXP[0] = 0;
                    old[0] = false;
                }
                else
                {
                    sLevs[0] = n;
                    addedXP[0] = 0;
                    oldXP[0] = 0;
                    newXP[0] = 0;
                    old[0] = false;
                }
            }
            else if (args[0].ToLower() == "fishing")
            {
                Game1.player.fishingLevel.Value = n;
                Game1.player.experiencePoints[1] = GetDefStartXP(n);
                if (n < 10)
                {
                    sLevs[1] = 10;
                    addedXP[1] = 0;
                    config.FLV = 10;
                    config.FXP = 0;
                    oldXP[1] = 0;
                    newXP[1] = 0;
                    old[1] = false;
                }
                else
                {
                    sLevs[1] = n;
                    addedXP[1] = 0;
                    oldXP[1] = 0;
                    newXP[1] = 0;
                    old[1] = false;
                }
            }
            else if (args[0].ToLower() == "foraging")
            {
                Game1.player.foragingLevel.Value = n;
                Game1.player.experiencePoints[2] = GetDefStartXP(n);
                if (n < 10)
                {
                    sLevs[2] = 10;
                    addedXP[2] = 0;
                    config.FoLV = 10;
                    config.FoXP = 0;
                    oldXP[2] = 0;
                    newXP[2] = 0;
                    old[2] = false;
                }
                else
                {
                    sLevs[2] = n;
                    addedXP[2] = 0;
                    oldXP[2] = 0;
                    newXP[2] = 0;
                    old[2] = false;
                }
            }
            else if (args[0].ToLower() == "mining")
            {
                Game1.player.miningLevel.Value = n;
                Game1.player.experiencePoints[3] = GetDefStartXP(n);
                if (n < 10)
                {
                    sLevs[3] = 10;
                    addedXP[3] = 0;
                    config.MLV = 10;
                    config.MXP = 0;
                    oldXP[3] = 0;
                    newXP[3] = 0;
                    old[3] = false;
                }
                else
                {
                    sLevs[3] = n;
                    addedXP[3] = 0;
                    oldXP[3] = 0;
                    newXP[3] = 0;
                    old[3] = false;
                }
            }
            else if (args[0].ToLower() == "combat")
            {
                Game1.player.combatLevel.Value = n;
                Game1.player.experiencePoints[4] = GetDefStartXP(n);
                if (n < 10)
                {
                    sLevs[4] = 10;
                    addedXP[4] = 0;
                    config.CLV = 10;
                    config.CXP = 0;
                    oldXP[4] = 0;
                    newXP[4] = 0;
                    old[4] = false;
                }
                else
                {
                    sLevs[4] = n;
                    addedXP[4] = 0;
                    oldXP[4] = 0;
                    newXP[4] = 0;
                    old[4] = false;
                }
            }
            */

            this.Helper.Content.InvalidateCache("Data/Fish");
        }
        private void WmT(string command, string[] args)
        {
            if (!wm)
            {
                wm = true;
                Monitor.Log($"Overworld Monster Spawning -> ON.");
            }
            else
            {
                wm = false;
                Monitor.Log($"Overworld Monster Spawning -> OFF.");
            }
        }
        private void XpM(string command, string[] args)
        {
            if (args.Length > 1 && double.TryParse(args[1], out double x) && x > 0.0)
            {
                //xp_mod = x;
                Skill skill = skills.SingleOrDefault(sk => sk.name.Equals(args[0], StringComparison.OrdinalIgnoreCase));

                if (skill == null)
                    return;

                skill.xp_mod = x;
                Monitor.Log($"The XP modifier for {skill.name} was set to: {x}");
            }
            else
            {
                Monitor.Log($"Valid decimal not used; refer to help command.");
            }
        }
        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady || e.OldMenu == null)
                return;

            /*if (pres_comp)
            {
                if (e.OldMenu.GetType().FullName == "SkillPrestige.Menus.PrestigeMenu")
                {
                    //closing();
                    SetTimer(1100, 0);
                }
                else if (e.OldMenu.GetType().FullName == "StardewValley.Menus.GameMenu")
                {
                    //closing();
                    SetTimer(1100, 0);
                }
                else if (e.OldMenu.GetType().FullName == "SkillPrestige.Menus.SettingsMenu")
                {
                    //closing();
                    SetTimer(1100, 0);
                }
                else if (e.OldMenu.GetType().FullName == "SkillPrestige.Menus.Dialogs.WarningDialog")
                {
                    //closing();
                    SetTimer(1100, 0);
                }
            }*/
        }
        public void Closing()
        {

            /*if (Game1.player.farmingLevel.Value >= 10 && shLev[0])
            {
                Game1.player.farmingLevel.Value = origLevs[0];

            }
            else if (!shLev[0])
            {
                if (origLevs[0] - 10 >= 10)
                    sLevs[0] = origLevs[0] - 10;
                else
                    sLevs[0] = 10;
                Game1.player.farmingLevel.Value = origLevs[0] - 10;
                addedXP[0] = 0;
                oldXP[0] = 0;
                newXP[0] = 0;
                old[0] = false;

            }
            if (Game1.player.fishingLevel.Value >= 10 && shLev[1])
            {
                Game1.player.fishingLevel.Value = origLevs[1];

            }
            else if (!shLev[1])
            {

                if (origLevs[1] - 10 >= 10)
                    sLevs[1] = origLevs[1] - 10;
                else
                    sLevs[1] = 10;
                Game1.player.fishingLevel.Value = origLevs[1] - 10;
                addedXP[1] = 0;
                oldXP[1] = 0;
                newXP[1] = 0;
                old[1] = false;

            }
            if (Game1.player.foragingLevel.Value >= 10 && shLev[2])
            {
                Game1.player.foragingLevel.Value = origLevs[2];

            }
            else if (!shLev[2])
            {
                if (origLevs[2] - 10 >= 10)
                    sLevs[2] = origLevs[2] - 10;
                else
                    sLevs[2] = 10;
                Game1.player.foragingLevel.Value = origLevs[2] - 10;
                addedXP[2] = 0;
                oldXP[2] = 0;
                newXP[2] = 0;
                old[2] = false;

            }
            if (Game1.player.miningLevel.Value >= 10 && shLev[3])
            {
                Game1.player.miningLevel.Value = origLevs[3];

            }
            else if (!shLev[3])
            {
                if (origLevs[3] - 10 >= 10)
                    sLevs[3] = origLevs[3] - 10;
                else
                    sLevs[3] = 10;
                Game1.player.miningLevel.Value = origLevs[3] - 10;
                addedXP[3] = 0;
                oldXP[3] = 0;
                newXP[3] = 0;
                old[3] = false;

            }
            if (Game1.player.combatLevel.Value >= 10 && shLev[4])
            {
                Game1.player.combatLevel.Value = origLevs[4];

            }
            else if (!shLev[4])
            {
                if (origLevs[4] - 10 >= 10)
                    sLevs[4] = origLevs[4] - 10;
                else
                    sLevs[4] = 10;
                Game1.player.combatLevel.Value = origLevs[4] - 10;
                addedXP[4] = 0;
                oldXP[4] = 0;
                newXP[4] = 0;
                old[4] = false;

            }
            origLevs = new int[] { 0, 0, 0, 0, 0 };
            origExp = new int[] { 0, 0, 0, 0, 0 };
            pres_comp = false;
            shLev = new bool[] { true, true, true, true, true };
            */


        }

        public int[] defReqXPs = { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15001 };

        /*private int GetReqXP(int lev)
        {
            int exp = 0;

            if (lev < 10)
            {
                exp = defReqXPs[lev];
            }
            else
            {
                exp = (int)Math.Round((1000 * lev + (lev * lev * lev * 1.134)) * xp_mod);
            }

            return exp;
        }*/

        /*private int GetDefStartXP(int lev)
        {
            int exp;

            if (lev == 0)
            {
                exp = 0;
            }
            else if (lev > 0 && lev < 11)
            {
                exp = defReqXPs[lev - 1];
            }
            else
            {
                exp = 15001;
            }


            return exp;
        }*/

        private void ControlEvent_KeyPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            /*if (Game1.activeClickableMenu is GameMenu && e.Button == SButton.P && !pres_comp)
            {
                //Monitor.Log($"{Game1.player.name} pressed P--.");
                pres_comp = true;
                if (Game1.player.farmingLevel.Value > 10)
                {
                    origLevs[0] = Game1.player.farmingLevel.Value;
                    Game1.player.farmingLevel.Value = 10;

                }
                if (Game1.player.fishingLevel.Value > 10)
                {
                    origLevs[1] = Game1.player.fishingLevel.Value;
                    Game1.player.fishingLevel.Value = 10;

                }
                if (Game1.player.foragingLevel.Value > 10)
                {
                    origLevs[2] = Game1.player.foragingLevel.Value;
                    Game1.player.foragingLevel.Value = 10;

                }
                if (Game1.player.miningLevel.Value > 10)
                {
                    origLevs[3] = Game1.player.miningLevel.Value;
                    Game1.player.miningLevel.Value = 10;

                }
                if (Game1.player.combatLevel.Value > 10)
                {
                    origLevs[4] = Game1.player.combatLevel.Value;
                    Game1.player.combatLevel.Value = 10;

                }
            }*/
            /*
            if (e.Button.IsUseToolButton() && staminaC == -1.0f && oStamina == -1.0f)
            {
                Monitor.Log($"current stamina: {Game1.player.Stamina}");
                staminaC = Game1.player.Stamina;
                oStamina = staminaC;
                tbuttondown = true;
            }*/
        }
        private void GameEvents_OneSecondTick(object sender, OneSecondUpdateTickedEventArgs e)
        {

            if (!Context.IsWorldReady)
                return;


            if (e.IsMultipleOf(3600))
            {
                List<int> tmons = new List<int>();

                foreach (Monster mon in monsters)
                {
                    if (mon == null || mon.Health <= 0 || mon.currentLocation == null)
                    {
                        //monsters.Remove(mon);
                        tmons.Add(monsters.IndexOf(mon));
                    }
                }
                foreach (int i in tmons)
                {
                    monsters.RemoveAt(i);
                }


                /*for (int i = 0; i < skills.Count; i++)
                {
                    skillLevs[i] = skills[i].level;
                }*/
            }

            if (skills.Count > 4)
            {

                int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };

                for (int i = 0; i < temp.Length; i++)
                {
                    /*if (oldXP[i] == -1)
                    {
                        oldXP[i] = Game1.player.experiencePoints[i];
                    }*/

                    Skill skill = skills.SingleOrDefault(sk => sk.key == i);
                    if (skill.xp != Game1.player.experiencePoints[i])
                    {
                        //Skill skill = skills.SingleOrDefault(sk => sk.key == i);
                        //skill.xp = skill.xp + (Game1.player.experiencePoints[i] - oldXP[i]);
                        skill.xp = Game1.player.experiencePoints[i];
                        oldXP[i] = -1;
                    }
                }
            }


            /*for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] < 10 && dxp[i] != Game1.player.experiencePoints[i])
                {
                    EXPEventArgs args = new EXPEventArgs { key = i, xp = (Game1.player.experiencePoints[i] - dxp[i]) };
                    LEE.RaiseEvent(args);
                    dxp[i] = Game1.player.experiencePoints[i];
                }

                if (temp[i] >= 10 && !old[i])
                {
                    oldXP[i] = Game1.player.experiencePoints[i];
                    old[i] = true;
                }
                else if (old[i])
                {
                    newXP[i] = Game1.player.experiencePoints[i];

                    if (newXP[i] - oldXP[i] > 0 && !pres_comp)
                    {
                        old[i] = false;
                        AddFishingXP(newXP[i] - oldXP[i], i);
                    }

                }
            }*/

            /*for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i] >= 10 && !old[i])
                {
                    oldXP[i] = Game1.player.experiencePoints[i];
                    old[i] = true;
                }
                else if (old[i])
                {
                    newXP[i] = Game1.player.experiencePoints[i];

                    if (newXP[i] - oldXP[i] > 0 && !pres_comp)
                    {
                        old[i] = false;
                        AddFishingXP(newXP[i] - oldXP[i], i);
                    }

                }
            }*/



            /*if (pres_comp)
            {

                for (int i = 0; i < temp.Length; i++)
                {
                    if (temp[i] >= 10 && !olev[i])
                    {
                        oldLevs[i] = temp[i];
                        olev[i] = true;

                    }
                    else if (olev[i])
                    {
                        newLevs[i] = temp[i];

                        if (newLevs[i] - oldLevs[i] < 0)
                        {
                            olev[i] = false;
                            if (newLevs[i] - oldLevs[i] == -10 && oldLevs[i] == 10)
                            {
                                shLev[i] = false;
                            }
                        }
                    }
                }
            }*/

            //bool isdark = this.Helper.Reflection.GetField<Netcode.NetBool>(Game1.player.currentLocation, "isLightingDark").GetValue();

            if (!no_mons && wm && Game1.player.currentLocation.IsOutdoors && Game1.activeClickableMenu == null && rand.NextDouble() <= S_R())
            {

                //Monitor.Log($"Light level: {Game1.player.currentLocation}");

                Vector2 loc = Game1.player.currentLocation.getRandomTile();
                while (!(Game1.player.currentLocation.isTileLocationTotallyClearAndPlaceable(loc)))
                {
                    loc = Game1.player.currentLocation.getRandomTile();
                }

                int tier = rand.Next(0, 8);

                Monster m = GetMonster(tier, loc * (float)Game1.tileSize);

                if (tier == 8)
                {
                    tier = 5;
                    m.resilience.Value += 20;
                    m.Slipperiness += rand.Next(10) + 5;
                    m.coinsToDrop.Value = rand.Next(10) * 50;
                    m.startGlowing(new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)), true, 1.0f);
                    m.Health *= 1 + (rand.Next(Game1.player.CombatLevel / 2, Game1.player.CombatLevel));

                    var data = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
                    //Item item = new StardewValley.Object(rand.Next(data.Count), 1);

                    m.objectsToDrop.Add(rand.Next(data.Count));
                    m.displayName += ": LE BOSS";
                }
                else
                {
                    tier = 1;
                }

                m.DamageToFarmer = (int)(m.DamageToFarmer / 1.5) + (int)(Game1.player.combatLevel.Value / 3);
                //m.Health = (int)(m.Health / 1.5) + ((Game1.player.combatLevel.Value / 2) * (m.Health / 10));
                //m.Health = m.Health * (int)Math.Round(Game1.player.combatLevel.Value * 0.1 * (rand.NextDouble() + rand.NextDouble()));
                m.Health *= 1 + (Game1.player.CombatLevel / 5);
                m.focusedOnFarmers = true;
                m.wildernessFarmMonster = true;
                m.Speed += rand.Next((int)Math.Round((Game1.player.combatLevel.Value / 5.0)));
                m.resilience.Set(m.resilience.Value + (Game1.player.combatLevel.Value / 10));
                m.experienceGained.Value += (int)(m.Health / 100.0) + ((10 + (Game1.player.combatLevel.Value * 2)) * tier);

                IList<NPC> characters = Game1.currentLocation.characters;
                characters.Add((NPC)m);

                total_m++;

                if (tier == 5)
                    Game1.chatBox.addMessage($"A boss has spawned in your current location!", Color.DarkRed);

                monsters.Add(m);
            }

        }
        public double S_R()
        {
            if (Game1.player.combatLevel.Value == 0)
            {
                return 0.0;
            }

            if (s_mod != -1.0)
            {
                return s_mod;
            }
            else if (API.overSR != -1.0)
            {
                return API.overSR;
            }

            if (Game1.isDarkOut() || Game1.isRaining)
            {
                return (0.01 + (Game1.player.combatLevel.Value * 0.0001)) * 1.5;
            }

            return (0.01 + (Game1.player.combatLevel.Value * 0.0001));

        }
        private void GameEvents_QuarterSecondTick(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.UsingTool && initialtooluse == false)
            {
                oStamina = Game1.player.Stamina;
                initialtooluse = true;
            }
            else if (!Game1.player.UsingTool && initialtooluse == true)
            {
                if (Game1.player.Stamina > oStamina)
                {
                    Game1.player.Stamina = Math.Max(oStamina - 0.5f, 0.0f);
                }
                else
                {

                }

                oStamina = 0.0f;
                initialtooluse = false;

            }


            if (e.IsMultipleOf(8)) // && Game1.player.FishingLevel > 10)
            {
                if (Game1.activeClickableMenu is BobberBar && !firstFade)
                {
                    //Monitor.Log($"Fishing Level:  {Game1.player.fishingLevel.Value}");
                    //Monitor.Log($"Modified Fishing Level:  {Game1.player.FishingLevel}");
                    //Monitor.Log($"Buffs: {this.Helper.Reflection.GetField<string>(Game1.buffsDisplay, "buffs").GetValue()}");
                    int bobberBonus = 0;
                    Tool tool = Game1.player.CurrentTool;
                    bool beginnersRod;


                    beginnersRod = tool != null && tool is FishingRod && (int)(NetFieldBase<int, NetInt>)tool.upgradeLevel == 1;

                    //Monitor.Log($"{tool.Name}");
                    //Monitor.Log($"{tool.attachmentSlots()}");
                    /*for(int i = 0; i < tool.attachmentSlots(); i++)
                    {
                        
                        StardewValley.Object attachment = tool.attachments[i];
                        Monitor.Log($"{attachment.name}");
                        if (attachment.name == "Cork Bobber")
                        {
                            bobberBonus = 24;
                        }
                    }*/


                    foreach (var attachment in tool.attachments.Where(n => n != null))
                    {
                        if (attachment.name == "Cork Bobber")
                        {
                            bobberBonus = 24;
                        }
                    }
                    //Item check = this.Helper.Reflection.
                    if (Game1.player.FishingLevel > 99)
                        bobberBonus += 8;
                    else if (Game1.player.FishingLevel > 74)
                        bobberBonus += 6;
                    else if (Game1.player.FishingLevel > 49)
                        bobberBonus += 4;
                    else if (Game1.player.FishingLevel > 24)
                        bobberBonus += 2;

                    int bobberBarSize;

                    if (!(this.Helper.ModRegistry.IsLoaded("DevinLematty.ExtremeFishingOverhaul")))
                    {
                        if (beginnersRod)
                            bobberBarSize = 80 + (5 * 9);
                        else if (Game1.player.FishingLevel < 11)
                            bobberBarSize = 80 + bobberBonus + (int)(Game1.player.FishingLevel * 9);
                        else
                            bobberBarSize = 165 + bobberBonus + (int)(Game1.player.FishingLevel * (0.5 + (rand.NextDouble() / 2.0)));
                    }
                    else
                    {
                        if (beginnersRod)
                            bobberBarSize = 80 + (5 * 7);
                        else if (Game1.player.FishingLevel < 11)
                            bobberBarSize = 80 + bobberBonus + (int)(Game1.player.FishingLevel * 7);
                        else if (Game1.player.FishingLevel > 10 && Game1.player.FishingLevel < 20)
                            bobberBarSize = 150 + bobberBonus + (int)(Game1.player.FishingLevel);
                        else
                            bobberBarSize = 170 + bobberBonus + (int)(Game1.player.FishingLevel * 0.8 * (0.5 + (rand.NextDouble() / 2.0)));
                    }


                    firstFade = true;
                    //Monitor.Log($"{this.Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "bobberBarHeight").GetValue()} -SIZE.");
                    this.Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "bobberBarHeight").SetValue(bobberBarSize);
                    this.Helper.Reflection.GetField<float>(Game1.activeClickableMenu, "bobberBarPos").SetValue((float)(568 - bobberBarSize));


                }
                else if (!(Game1.activeClickableMenu is BobberBar) && firstFade)
                {
                    firstFade = false;
                    //Monitor.Log($"Fade set false. -SIZE.");
                }
                else if (Game1.activeClickableMenu is BobberBar && firstFade)
                {
                    bool bobberInBar = this.Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "bobberInBar").GetValue();
                    if (!bobberInBar)
                    {
                        float dist = this.Helper.Reflection.GetField<float>(Game1.activeClickableMenu, "distanceFromCatching").GetValue();
                        this.Helper.Reflection.GetField<float>(Game1.activeClickableMenu, "distanceFromCatching").SetValue(dist + ((float)(Game1.player.FishingLevel - 10) / 22000.0f));

                    }
                }
            }

            if (e.IsMultipleOf(15))
            {
                //if (Game1.currentLocation.debris.Count > 0)
                //Monitor.Log($"Current debs 0: {Game1.currentLocation.debris[0].item.DisplayName}");
                //Monitor.Log($"gameloc debris count {tempDebris.Count}/{Game1.currentLocation.debris.Count}");
                /*    if (tempDebris.Count != Game1.currentLocation.debris.Count)
                    {
                        GameLocation cl = Game1.currentLocation;
                        int diff = cl.debris.Count - tempDebris.Count;

                        //Monitor.Log($"LE debris diff: {diff}, gameloc debris count {tempDebris.Count}/{cl.debris.Count}");

                        if (diff > 0)
                        {
                            int deb_count = cl.debris.Count;

                            for (int i = deb_count - diff; i < deb_count; i++)
                            {
                                Item item = cl.debris[i].item;
                                if (item == null)
                                    continue;

                                int cat = item.Category;

                                if (!item.HasBeenInInventory)
                                {
                                    if ((cat == -2 || cat == -12 || cat == -15) && ShouldDup(3))
                                    {
                                        Game1.createItemDebris(item.getOne(), Game1.player.getStandingPosition(), rand.Next(4));
                                    }
                                    else if ((cat == -28) && ShouldDup(4))
                                    {
                                        Game1.createItemDebris(item.getOne(), Game1.player.getStandingPosition(), rand.Next(4));
                                    }
                                }
                            }
                        }

                        tempDebris = new List<Debris>(Game1.currentLocation.debris.ToList());
                    }
                */

            }

        }


        //public static double[] dupRates = { 0.002, 0.002, 0.002, 0.002, 0.002 };

        public static bool ShouldDup(int index)
        {
            double drate = 0.002;

            if (index == 0 || index == 2)
                drate = 0.002 / 2.0;

            //int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };

            if (rand.NextDouble() <= (skillLevs[index] * drate))
            {
                return true;
            }

            return false;
        }
        /*public static bool ShouldDup(int index, int index2)
        {
            int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };

            if (rand.NextDouble() <= (((skillLevs[index] + skillLevs[index2])/2.0) * ((dupRates[index] + dupRates[index2])/2.0)))
            {
                return true;
            }

            return false;
        }*/

        private void TimeEvent_AfterDayStarted(object sender, EventArgs e)
        {

            //List<HoeDirt> list = new List<HoeDirt>();
            Farm farm = Game1.getFarm();
            double gchance = Game1.player.FarmingLevel * 0.0002;
            double pchance = Game1.player.FarmingLevel * 0.001;
            foreach (Vector2 key in farm.terrainFeatures.Keys)
            {
                if (farm.terrainFeatures[key] is HoeDirt tf && tf.crop != null && rand.NextDouble() < gchance)
                {
                    tf.crop.growCompletely();
                    //tf.crop.minHarvest.Value = rand.Next(5, 100);
                    //Monitor.Log("LE: hit gchance!");
                    //tf.crop.tintColor.Value = Color.Lavender;
                }
                else if (farm.terrainFeatures[key] is HoeDirt tf2 && tf2.crop != null && rand.NextDouble() < pchance)
                {

                    tf2.crop.currentPhase.Value = Math.Min(tf2.crop.currentPhase.Value + 1, tf2.crop.phaseDays.Count - 1);
                    //tf2.crop.tintColor.Value = Color.DarkBlue;
                }
                /*if (farm.terrainFeatures[key] is HoeDirt tf3 && tf3.crop != null && tf3.crop.fullyGrown.Value)
                {
                    tf3.crop.minHarvest.Value = 1 + rand.Next((int)(Game1.player.FarmingLevel * 0.1));
                    //tf3.crop.
                }*/
            }
            //return Utility.GetRandom<HoeDirt>(list);


            if (!mpload && this.Helper.ModRegistry.IsLoaded("f1r3w477.Level_Extender"))
            {
                mpmod = this.Helper.ModRegistry.GetApi<MPModApi>("f1r3w477.Level_Extender");
                mpload = true;
                SetTimer(1000, 1);
            }
            else if (mpload)
            {
                SetTimer(1000, 1);
            }
            no_mons = false;
        }

        public void Rem_mons()
        {

            no_mons = true;

            //Monitor.Log("Removed Monsters.");

            int x = 0;
            int y;
            foreach (GameLocation location in Game1.locations)
            {
                y = location.characters.Count;

                location.characters.Filter(f => !(f.IsMonster));

                /*foreach (var ch in location.characters)
                {
                    if(ch.IsMonster)
                    location.characters.Remove(ch);
                }*/


                x += (y - location.characters.Count);

            }

            Monitor.Log($"Removed | {x} | / | {total_m} | monsters.");

            total_m = 0;

        }

        private Monster GetMonster(int x, Vector2 loc)
        {
            Monster m;
            switch (x)
            {
                case 0:
                    m = new DustSpirit(loc);
                    break;
                case 1:
                    m = new Grub(loc);
                    break;
                case 2:
                    m = new Skeleton(loc);
                    break;
                case 3:
                    m = new RockCrab(loc);
                    break;
                case 4:
                    m = new Ghost(loc);
                    break;
                case 5:
                    m = new GreenSlime(loc);
                    break;
                case 6:
                    m = new RockGolem(loc);
                    break;
                case 7:
                    m = new ShadowBrute(loc);
                    break;
                case 8:
                    int y = rand.Next(1, 6);

                    //m = new Monster();

                    if (y == 1)
                        m = new RockCrab(loc, "Iridium Crab");
                    else if (y == 2)
                        m = new Ghost(loc, "Carbon Ghost");
                    else if (y == 3)
                        m = new LavaCrab(loc);
                    //else if (y == 4)
                    //m = new Bat(loc, Math.Max(Game1.player.CombatLevel * 5, 50));
                    else if (y == 4)
                        m = new GreenSlime(loc, Math.Max(Game1.player.CombatLevel * 5, 50));
                    else if (y == 5)
                        m = new BigSlime(loc, Math.Max(Game1.player.CombatLevel * 5, 50));
                    else
                        m = new Mummy(loc);

                    break;

                default:
                    m = new Monster();
                    break;
            }

            return m;
        }

        Timer shouldDraw;

        private void SaveEvents_AfterLoad(object sender, SaveLoadedEventArgs e)
        {

            

            SetTimer(2000, 2);

            try
            {

                Monitor.Log("Starting load for LE");

                var config_t = this.Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json") ?? new ModData();

                int[] sdlevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
                string[] sdnames = { "Farming", "Fishing", "Foraging", "Mining", "Combat" };

                int[] cats0 = { -16, -74, -75, -79, -80, -81 };
                int[] cats1 = { -4 };
                int[] cats2 = cats0;
                int[] cats3 = { -2, -12, -15 };
                int[] cats4 = { -28, -29, -95, -96, -98 };

                List<int[]> cats = new List<int[]>();

                cats.Add(cats0);
                cats.Add(cats1);
                cats.Add(cats2);
                cats.Add(cats3);
                cats.Add(cats4);

                int count = 0;

                foreach (string str in config_t.skills)
                {
                    Monitor.Log($"skill load - {str}");
                    //Skill sk = JsonConvert.DeserializeObject<Skill>(str);
                    string[] vals = str.Split(',');
                    Skill sk = new Skill(vals[0], int.Parse(vals[1]), int.Parse(vals[2]), LE, null, double.Parse(vals[3]), cats[count]);
                    skills.Add(sk);
                    snames.Add(sk.name);
                    categories.Add(sk.cats);
                    skillLevs.Add(sk.level);
                    count++;
                }

                for (int i = count; i < 5; i++)
                {
                    Monitor.Log($"adding skills - {i}, dxp: {Game1.player.experiencePoints[i]}");
                    Skill sk = new Skill(sdnames[i], sdlevs[i], Game1.player.experiencePoints[i], LE, null, 1, cats[i]);
                    skills.Add(sk);
                    snames.Add(sk.name);
                    categories.Add(sk.cats);
                    skillLevs.Add(sk.level);
                }


                wm = config_t.WorldMonsters;
                xp_mod = config_t.Xp_modifier;

                config = config_t;

                /*for (int i = 0; i < 5; i++)
                {
                    dxp[i] = Game1.player.experiencePoints[i];
                }

                //object[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
                var config_t = this.Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json") ?? new ModData();
                addedXP[0] = config_t.FaXP;
                sLevs[0] = config_t.FaLV;
                addedXP[1] = config_t.FXP;
                sLevs[1] = config_t.FLV;
                addedXP[2] = config_t.FoXP;
                sLevs[2] = config_t.FoLV;
                addedXP[3] = config_t.MXP;
                sLevs[3] = config_t.MLV;
                addedXP[4] = config_t.CXP;
                sLevs[4] = config_t.CLV;
                wm = config_t.WorldMonsters;
                xp_mod = config_t.Xp_modifier;

                config = config_t;

                if (Game1.player.farmingLevel.Value >= 10)
                {
                    Game1.player.farmingLevel.Value = sLevs[0];

                    Monitor.Log($"{Game1.player.Name} loaded farming level {Game1.player.farmingLevel.Value}!");
                }
                if (Game1.player.fishingLevel.Value >= 10)
                {
                    Game1.player.fishingLevel.Value = sLevs[1];

                    Monitor.Log($"{Game1.player.Name} loaded fishing level {Game1.player.fishingLevel.Value}!");
                }
                if (Game1.player.foragingLevel.Value >= 10)
                {
                    Game1.player.foragingLevel.Value = sLevs[2];

                    Monitor.Log($"{Game1.player.Name} loaded foraging level {Game1.player.foragingLevel.Value}!");
                }
                if (Game1.player.miningLevel.Value >= 10)
                {
                    Game1.player.miningLevel.Value = sLevs[3];

                    Monitor.Log($"{Game1.player.Name} loaded mining level {Game1.player.miningLevel.Value}!");
                }
                if (Game1.player.combatLevel.Value >= 10)
                {
                    Game1.player.combatLevel.Value = sLevs[4];

                    Monitor.Log($"{Game1.player.Name} loaded combat level {Game1.player.combatLevel.Value}!");
                }*/

                //config = this.Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json") ?? new ModData();
            }
            catch (Exception ex)
            {
                Monitor.Log($"LE failed loading skills, mod will not start: {ex.Message}", LogLevel.Trace);
                System.Environment.Exit(1);
            }


            this.Helper.Content.InvalidateCache("Data/Fish");
        }
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {
            config.skills = new List<string>();

            foreach (Skill skill in skills)
            {
                //serializer.Converters.Add(new JavaScriptDateTimeConverter());
                //config.skills = new List<string>();

                config.skills.Add($"{skill.name},{skill.level},{skill.xp},{skill.xp_mod}");
                
            }
            /*if (Game1.player.farmingLevel.Value >= 10)
            {

                Game1.player.farmingLevel.Value = sLevs[0];
                config.FaXP = addedXP[0];
                config.FaLV = sLevs[0];

                Monitor.Log($"{Game1.player.Name} saved farming level {Game1.player.farmingLevel.Value}!");
            }
            if (Game1.player.fishingLevel.Value >= 10)
            {

                Game1.player.fishingLevel.Value = sLevs[1];
                config.FXP = addedXP[1];
                config.FLV = sLevs[1];

                Monitor.Log($"{Game1.player.Name} saved fishing level {Game1.player.fishingLevel.Value}!");
            }
            if (Game1.player.foragingLevel.Value >= 10)
            {

                Game1.player.foragingLevel.Value = sLevs[2];
                config.FoXP = addedXP[2];
                config.FoLV = sLevs[2];

                Monitor.Log($"{Game1.player.Name} saved foraging level {Game1.player.foragingLevel.Value}!");
            }
            if (Game1.player.miningLevel.Value >= 10)
            {

                Game1.player.miningLevel.Value = sLevs[3];
                config.MXP = addedXP[3];
                config.MLV = sLevs[3];

                Monitor.Log($"{Game1.player.Name} saved mining level {Game1.player.miningLevel.Value}!");
            }
            if (Game1.player.combatLevel.Value >= 10)
            {

                Game1.player.combatLevel.Value = sLevs[4];
                config.CXP = addedXP[4];
                config.CLV = sLevs[4];

                Monitor.Log($"{Game1.player.Name} saved combat level {Game1.player.combatLevel.Value}!");
            }
            config.WorldMonsters = wm;
            config.Xp_modifier = xp_mod;*/

            //this.Helper.Data.WriteJsonFile<ModData>($"data/{Constants.SaveFolderName}.json", config);

            config.WorldMonsters = wm;
            config.Xp_modifier = xp_mod;
            

            this.Helper.Data.WriteJsonFile<ModData>($"data/{Constants.SaveFolderName}.json", config);

            if (!no_mons)
            {
                Rem_mons();
            }

        }
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            /*oldXP = new int[] { 0, 0, 0, 0, 0 };
            newXP = new int[] { 0, 0, 0, 0, 0 };
            old = new bool[] { false, false, false, false, false };
            addedXP.Values = new int[] { 0, 0, 0, 0, 0 };
            sLevs = new int[] { 0, 0, 0, 0, 0 };
            max = new int[] { 100, 100, 100, 100, 100 };
            firstFade = false;
            config = new ModData();

            origLevs = new int[] { 0, 0, 0, 0, 0 };
            origExp = new int[] { 0, 0, 0, 0, 0 };

            wm = new bool();
            pres_comp = false;
            oldLevs = new int[] { 0, 0, 0, 0, 0 };
            newLevs = new int[] { 0, 0, 0, 0, 0 };
            olev = new bool[] { false, false, false, false, false };
            shLev = new bool[] { true, true, true, true, true };*/
            wm = new bool();
            pres_comp = false;
            firstFade = false;
            config = new ModData();
            xp_mod = 1.0;

            skills = new List<Skill>();
            snames = new List<string>();
            categories = new List<int[]>();
        }
        /*private void AddFishingXP(int xp, int i)
        {
            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            Monitor.Log($"Adding XP: {xp * mpMult}");
            addedXP[i] += (int)(xp * mpMult);
            if (addedXP[i] >= GetReqXP(temp[i]) && sLevs[i] < max[i])
            {
                int leftover = Math.Max(addedXP[i] - GetReqXP(temp[i]), 0);
                addedXP[i] = leftover;
                sLevs[i] += 1;

                if (i == 0)
                {
                    Game1.player.farmingLevel.Value = sLevs[i];
                }
                else if (i == 1)
                {
                    Game1.player.fishingLevel.Value = sLevs[i];
                }
                else if (i == 2)
                {
                    Game1.player.foragingLevel.Value = sLevs[i];
                }
                else if (i == 3)
                {
                    Game1.player.miningLevel.Value = sLevs[i];
                }
                else if (i == 4)
                {
                    Game1.player.combatLevel.Value = sLevs[i];
                }

                //Monitor.Log($"{Game1.player.Name} leveled {i}(index) to {sLevs[i]}!");
                Game1.chatBox.addMessage($"You leveled up {snames[i]} to level {sLevs[i]}!", Color.ForestGreen);
            }
            else
            {
                if (i == 0)
                {
                    Game1.player.farmingLevel.Value = sLevs[i];
                }
                else if (i == 1)
                {
                    Game1.player.fishingLevel.Value = sLevs[i];
                }
                else if (i == 2)
                {
                    Game1.player.foragingLevel.Value = sLevs[i];
                }
                else if (i == 3)
                {
                    Game1.player.miningLevel.Value = sLevs[i];
                }
                else if (i == 4)
                {
                    Game1.player.combatLevel.Value = sLevs[i];
                }
                //Monitor.Log($"You loaded {snames[i]} level {sLevs[i]}!");
            }
        }*/

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Closing();
        }

        public int[] GetCurXP()
        {
            return addedXP.Values;
        }

        /*public int[] GetReqXP()
        {
            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            int[] xTemp = { 0, 0, 0, 0, 0 };

            for (int i = 0; i < temp.Length; i++)
            {
                xTemp[i] = GetReqXP(temp[i]);
            }

            return xTemp;
        }*/


    }

    public class LEEvents
    {
        public event EventHandler<EXPEventArgs> OnXPChanged;

        public void RaiseEvent(EXPEventArgs args)
        {
            if (OnXPChanged != null)
            {
                { OnXPChanged(this, args); }
            }
        }
    }

    public class EXP
    {
        private int[] axp;
        LEEvents LEE;
        EXPEventArgs args;

        public EXP(LEEvents lee)
        {
            axp = new int[] { 0, 0, 0, 0, 0 };
            LEE = lee;
            args = new EXPEventArgs();
        }

        public int this[int key]
        {
            get { return axp[key]; }

            set
            {
                if (axp[key] != value)
                {
                    args.key = key;
                    //args.xp = value - axp[key];
                    axp[key] = value;

                    LEE.RaiseEvent(args);
                }

            }
        }

        public int Length
        {
            get { return axp.Length; }
        }

        public int[] Values
        {
            get { return axp; }
            set
            {

                for (int i = 0; i < axp.Length; i++)
                {
                    if (axp[i] != value[i])
                    {
                        args.key = i;
                        //args.xp = value[i] - axp[i];
                        axp[i] = value[i];


                        //LEE.RaiseEvent();
                        //break;
                    }
                }
                axp = value;
            }
        }

        //dummy events for levels below 10

    }

    public class EXPEventArgs : EventArgs
    {
        public int key { get; set; }
    }

    public class XPBar
    {
        public Skill skill;
        public Timer timer;
        public DateTime time;

        public double ych = 0;
        public bool startmove = false;
        public int movedir = 0;


        public XPBar(Skill skill)
        {
            this.skill = skill;
            this.timer = new Timer(5000);
            timer.Elapsed += delegate { skill.LE.EndXPBar(skill.key); };

            timer.AutoReset = false;
            timer.Enabled = true;

            time = DateTime.Now;

        }

        public int ychi
        {
            get => (int)Math.Round(ych);
            set { ychi = value; }
        }

    }

    public class Skill
    {
        
        public string name;
        public int key;
        public bool lvlbyxp = false;
        private int Level;
        public int level
        {
            get { return Level; }
            set
            {
                //LE.Monitor.Log($"LE Level: {level}, XP: {xp}, KEY: {key}");
                int rxp = getReqXP();
                Level = value;

                if (key < 5)
                {
                    LE.Monitor.Log($"LE Level: {level}, XP: {xp}, KEY: {key}");

                    if (key == 0)
                        Game1.player.FarmingLevel = level;
                    else if (key == 1)
                        Game1.player.FishingLevel = level;
                    else if (key == 2)
                        Game1.player.ForagingLevel = level;
                    else if (key == 3)
                        Game1.player.MiningLevel = level;
                    else if (key == 4)
                        Game1.player.CombatLevel = level;


                    if (level < 10)
                    {
                        Game1.player.experiencePoints[key] = GetDefStartXP();

                    }
                    else if (lvlbyxp)
                    {
                        LE.Monitor.Log($"{xp} - {rxp}");
                        Game1.player.experiencePoints[key] = xp - rxp;
                    }
                    else
                    {
                        Game1.player.experiencePoints[key] = 0;
                    }


                    /*if (!lvlbyxp) {
                        if (level > 9)
                        {
                            xp = 0;
                        }
                        else
                        {

                            xp = Game1.player.experiencePoints[key];
                        }
                    }*/
                }
                else
                {
                    xp = 0;
                }

                lvlbyxp = false;
            }
        }
        public int xpc;
        public EXPEventArgs args;
        public ModEntry LE;
        public int[] xp_table;
        public double xp_mod;
        private int XP;
        public int xp
        {
            get { return XP; }
            set
            {
                if (xp != value)
                {
                    xpc = value - xp;
                    XP = value;
                    checkForLevelUp();
                    LE.LEE.RaiseEvent(args);
                }
            }

        }

        public int[] cats;

        public Skill(string name, int level, int xp, ModEntry LE, int[] xp_table = null, double xp_mod = 1.0, int[] cats = null)
        {
            this.LE = LE;
            LE.Monitor.Log("got to skill 1");            
            this.name = name;
            this.key = LE.skills.Count;
            args = new EXPEventArgs();
            args.key = key;
            this.xp_table = xp_table ?? new int[0];
            this.xp_mod = xp_mod;
            this.cats = cats ?? new int[0];
            LE.Monitor.Log("got to skill 2");
            this.level = level;
            LE.Monitor.Log("got to skill 2");
            Game1.player.experiencePoints[key] = xp;
            LE.Monitor.Log("got to skill 3");

            LE.Monitor.Log("got to skill 4");
        }
        public void checkForLevelUp()
        {
            int reqxp = getReqXP();

            if (xp >= reqxp)
            {
                lvlbyxp = true;
                level += 1;

                /*int newxp;

                if (level < 9)
                    newxp = xp;
                else
                    newxp = reqxp - xp;

                level += 1;
                xp = newxp;*/
            }

        }

        public int getReqXP()
        {
            if (xp_table.Length > level)
                return xp_table[level];

            int exp = 0;

            if (level < 10 && key < 5)
            {
                exp = LE.defReqXPs[level];
            }
            else
            {
                exp = (int)Math.Round((1000 * level + (level * level * level * 1.134)) * xp_mod);
            }

            return exp;

        }

        private int GetDefStartXP()
        {
            if (key > 4)
                return 0;

            int exp;

            if (level == 0)
            {
                exp = 0;
            }
            else if (level > 0 && level < 11)
            {
                exp = LE.defReqXPs[level - 1];
            }
            else
            {
                exp = 15001;
            }


            return exp;
        }

        public int StartXP()
        {
            int xp = 0;

            if ((level > 0 && level < 10) && key < 5)
            {
                xp = LE.defReqXPs[level - 1];
            }

            return xp;

        }

        
    }

}
