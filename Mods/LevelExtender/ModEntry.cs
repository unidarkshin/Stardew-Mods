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

        int skillCount = 5;

        List<XPBar> xpBars = new List<XPBar>();

        public ModEntry()
        {
            instance = this;
            LE = this;
            LEE = new LEEvents();
            addedXP = new EXP(LEE);
            total_m = 0;
            s_mod = -1.0;
            mpload = false;
            mpMult = 1.0;

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
                    int x = (val - rand.Next(0, (int)(Game1.player.fishingLevel.Value / 4)));
                    if (x < 1)
                        x = rand.Next(1, val);
                    fields[1] = x.ToString();
                    data[pair.Key] = string.Join("/", fields);
                }
            }
        }


        public override void Entry(IModHelper helper)
        {
            Initialize(instance.Monitor);

            /*var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.Patch(
                    original: AccessTools.Method(typeof(StardewValley.Tools.Axe), "DoFunction"), //nameof(this.Helper.Reflection.GetMethod(typeof(StardewValley.Tools.FishingRod), "doPullFishFromWater"))),
                    prefix: new HarmonyMethod(typeof(ModEntry), nameof(this.WCDF))
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

            //LEE.OnXPChanged += LEE;

            helper.ConsoleCommands.Add("xp", "Displays the xp table for your current levels.", this.XPT);
            helper.ConsoleCommands.Add("lev", "Sets the player's level: lev <type> <number>", this.SetLev);
            helper.ConsoleCommands.Add("wm_toggle", "Toggles monster spawning: wm_toggle", this.WmT);
            helper.ConsoleCommands.Add("xp_m", "Changes the xp modifier for levels 10 and after: xp_m <decimal 0.0 -> ANY> : 1.0 is default.", this.XpM);
            helper.ConsoleCommands.Add("spawn_modifier", "Forcefully changes monster spawn rate to specified decimal value: spawn_modifier <decimal(percent)> : -1.0 to not have any effect.", this.SM);
            helper.ConsoleCommands.Add("xp_table", "Tells the players current XP above or at level 10.", this.TellXP);
            helper.ConsoleCommands.Add("set_xp", "Sets your current XP for a given skill: set_xp <skill> <XP: int 0 -> ANY>", this.SetXP);
            //helper.ConsoleCommands.Add("LE_cmds", "Displays the xp table for your current levels.", this.XPT);

            this.Helper.Content.InvalidateCache("Data/Fish");
            LEE.OnXPChanged += this.OnXPChanged;


        }

        //int yChange = 0;
        //bool startMove = false;

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

                    string[] skills = { "F a r m i n g", "F i s h i n g", "F o r a g i n g", "M i n i n g", "C o m b a t" };
                    int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
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


                    int key = xpBars[i].key;
                    int xp = xpBars[i].xpc;
                    int lev = skillLevs[key];
                    int startXP = StartXP(lev);
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

                    if (lev < 10)
                    {
                        curXP = Game1.player.experiencePoints[key];
                    }
                    else
                    {
                        curXP = addedXP[key];
                    }

                    int maxXP = GetReqXP(lev);

                    if (startXP > 0)
                    {
                        maxXP = maxXP - startXP;
                        curXP = curXP - startXP;
                        startXP = 0;
                    }

                    int iWidth = 198;
                    double mod = iWidth / (maxXP * 1.0);
                    int bar2w = (int)Math.Round(xp * mod) + 1;
                    int bar1w = (int)Math.Round(curXP * mod) - bar2w;


                    /*if (i == 0 && xpBars[i].startmove && deltaTime <= 50)
                    {
                        xpBars[i].ych = 0;
                        xpBars[i].startmove = false;
                    }
                    else if (i == 0 && !xpBars[i].startmove && (deltaTime > 3950 && deltaTime < 4000))
                    {
                        xpBars[i].ych = 0;
                        xpBars[i].startmove = true;
                        Monitor.Log("startmove!");
                    }
                    else if ( && deltaTime >= 4000)
                    {
                        if (!shouldmove)
                            shouldmove = true;

                        xpBars[i].ych = (int)Math.Round((deltaTime - 4000) / 15.625);
                        Monitor.Log($"yChange Value: {xpBars[i].ych} -> {deltaTime}");
                    }

                    if (deltaTime >= 4970)
                    {
                        xpBars[i].ych = 0;
                        xpBars[i].startmove = false;
                        Monitor.Log("endmove!");
                    }*/


                    if (i == 0 && xpBars[i].ych < 0)
                    {
                        double ms = (DateTime.Now - otime).TotalMilliseconds;
                        double addv = (xpBars[i].ych + (ms / 15.625));
                        xpBars[i].ych = (addv >= 0 ? 0 : addv);
                        Monitor.Log($"NEG yChange Value: {xpBars[i].ych} -> {deltaTime}");
                    }
                    else if (i == 0 && deltaTime >= 4000)
                    {
                        double addv = (deltaTime - 4000) / 15.625;
                        xpBars[i].ych = (addv >= 64 ? 64 : addv);
                        Monitor.Log($"yChange Value: {xpBars[i].ych} -> {deltaTime}");
                    }
                    

                    xpBars[i].ych = xpBars[0].ych;



                    /*if (deltaTime >= 4970)
                    {
                        xpBars[i].ych = 0;
                        //xpBars[i].startmove = false;
                        Monitor.Log("endmove!");
                    }*/

                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX - 7, startY + (barSep * i) - 7 - xpBars[i].ychi, 214, 64), Color.DarkRed * transp);
                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX - 5, startY + (barSep * i) - 5 - xpBars[i].ychi, 210, 60), new Color(210, 173, 85) * transp);
                    Game1.spriteBatch.DrawString(Game1.smallFont, $"{skills[key]}", new Vector2(startX + 35 - ((skills[key].Length - 13) * 4), startY + (barSep * i) - xpBars[i].ychi), Color.Black * transp, 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 3), SpriteEffects.None, 0.5f);
                    Game1.spriteBatch.DrawString(Game1.smallFont, $"{skills[key]}", new Vector2(startX + 35 + 1 - ((skills[key].Length - 13) * 4), startY + (barSep * i) + 1 - xpBars[i].ychi), Color.Black * transp, 0.0f, Vector2.Zero, (float)(Game1.pixelZoom / 3), SpriteEffects.None, 0.5f);

                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX, startY + (barSep * i) + sep - xpBars[i].ychi, 200, 20), Color.Black * transp);
                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX + 1, startY + (barSep * i) + sep + 1 - xpBars[i].ychi, bar1w, 18), Color.SeaGreen * transp);
                    Game1.spriteBatch.Draw(Game1.staminaRect, new Rectangle(startX + 1 + bar1w, startY + (barSep * i) + sep + 1 - xpBars[i].ychi, bar2w, 18), Color.Turquoise * transp);

                    Vector2 mPos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
                    Vector2 bCenter = new Vector2(startX + (200 / 2), startY + (barSep * i) + sep + (20 / 2) - xpBars[i].ychi);
                    float dist = Vector2.Distance(mPos, bCenter);

                    if (dist <= 250f)
                    {
                        if (lev < 10)
                        {
                            curXP = Game1.player.experiencePoints[key];
                        }
                        else
                        {
                            curXP = addedXP[key];
                        }

                        maxXP = GetReqXP(lev);

                        float f = Math.Min(25f / dist, 1.0f);

                        string xpt = $"{curXP} / {maxXP}";

                        Game1.spriteBatch.DrawString(Game1.smallFont, xpt, new Vector2(startX + 1 + (198 / 2) - (xpt.Length * 4), startY + (barSep * i) + sep + 1 - xpBars[i].ychi), Color.White * f * (transp + 0.05f), 0.0f, Vector2.Zero, (Game1.pixelZoom / 6f), SpriteEffects.None, 0.5f);
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

        public int StartXP(int lev)
        {
            int xp = 0;

            if (lev > 0 && lev < 10)
            {
                xp = defReqXPs[lev - 1];
            }

            return xp;
        }

        private void SetXP(string command, string[] arg)
        {
            if (!Context.IsWorldReady || arg.Length < 2 || !int.TryParse(arg[1], out int xp))
                return;

            int[] skillLevs = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };

            if (arg[0].ToLower() == "farming")
            {
                if (skillLevs[0] < 10)
                    Game1.player.experiencePoints[0] = xp;
                else
                    addedXP[0] = xp;
            }
            else if (arg[0].ToLower() == "fishing")
            {
                if (skillLevs[1] < 10)
                    Game1.player.experiencePoints[1] = xp;
                else
                    addedXP[1] = xp;
            }
            else if (arg[0].ToLower() == "foraging")
            {
                if (skillLevs[2] < 10)
                    Game1.player.experiencePoints[2] = xp;
                else
                    addedXP[2] = xp;
            }
            else if (arg[0].ToLower() == "mining")
            {
                if (skillLevs[3] < 10)
                    Game1.player.experiencePoints[3] = xp;
                else
                    addedXP[3] = xp;
            }
            else if (arg[0].ToLower() == "combat")
            {
                if (skillLevs[4] < 10)
                    Game1.player.experiencePoints[4] = xp;
                else
                    addedXP[4] = xp;
            }
            else
            {
                Monitor.Log("ERROR - setXP: Invalid skill name.");
            }
        }

        public static bool WCDF(GameLocation location, int x, int y, int power, Farmer who)
        {

            try
            {
                Monitor.Log(who.Name);



                return true; // don't run original logic
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(WCDF)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
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

            /*if (e.Button.IsUseToolButton() && (staminaC != -1.0f))
            {
                //sChangeRelease = true;
                //if(Game1.player.Stamina > staminaC)
                //trueChange += Game1.player.Stamina - staminaC;

                staminaC = -1.0f;
                tbuttondown = false;
            }*/

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
                //showXPBar = true;
                int count = xpBarTimers.Count;

                //xpBarTimers.Add(new System.Timers.Timer(time));
                // Hook up the Elapsed event for the timer. 
                //xpBarTimers[count].Elapsed += EndXPBar;

                //xpBarTimers[count].AutoReset = false;
                //xpBarTimers[count].Enabled = true;

                //xpBarStartTime.Add(DateTime.Now);

            }



        }

        public void EndXPBar(int key)
        {

            //xpBars[0] = null;
            //xpBars.RemoveAt(0);
            //pushElementsToZero(xpBars);

            var item = xpBars.SingleOrDefault(x => x.key == key);
            double yval = item.ych;

            xpBars.Remove(item);

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

            string[] skills = { "Farming", "Fishing", "Foraging", "Mining", "Combat" };
            int[] skillLevs = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            for (int i = 0; i < 5; i++)
            {
                double xpn = Math.Round((1000 * skillLevs[i] + (skillLevs[i] * skillLevs[i] * skillLevs[i] * 0.33)) * xp_mod);
                Monitor.Log($"{skills[i]} | {skillLevs[i]} | {addedXP[i]} | {xpn}", LogLevel.Info);
                //Monitor.Log($"     {i} | {Math.Round((1000 * i + (i * i * i * 0.33)) * xp_mod)}");
            }
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
            if (e.xp < 0 || e.xp > 10000)
                return;

            Monitor.Log($"XP Changed: index {e.key}, EXP {e.xp}");

            bool exists = false;

            for (int i = 0; i < xpBars.Count; i++)
            {
                if (xpBars[i].key == e.key)
                {
                    xpBars[i].timer.Stop();
                    xpBars[i].timer.Start();
                    xpBars[i].time = DateTime.Now;
                    xpBars[i].xpc = e.xp;
                    exists = true;

                    /*if (i == 0)
                    {
                        //xpBars[i].ych = -1 * xpBars[i].ych;
                        xpBars[i].movedir = -1;
                        Monitor.Log("reset on xpchange");
                    }*/
                    double val = xpBars[i].ych * -1;

                    //Monitor.Log($"xpchanged, val: {val}");

                    setYchVals(val);


                    sortByTime();

                    break;
                    
                    
                }
            }

            if (!exists)
            {
                xpBars.Add(new XPBar(e.key, e.xp, LE));
            }

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
            foreach  (var bar in xpBars)
            {
                bar.ych = val;
            }
        }

        private void TellXP(string command, string[] args)
        {

            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
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
                    Monitor.Log($"NL: {Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod)}");
                else
                    Monitor.Log($"XP: Empty.");
            }
            for (int i = 0; i < 5; i++)
            {
                Monitor.Log($"Current XP - Default: {Game1.player.experiencePoints[i]}.");
            }

        }
        private void TellXP2(string command, string[] args)
        {

            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
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
                    Monitor.Log($"NL: {Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod)}");
                else
                    Monitor.Log($"XP: Empty.");
            }
            for (int i = 0; i < 5; i++)
            {
                Monitor.Log($"Current XP - Default: {Game1.player.experiencePoints[i]}.");
            }

        }
        private void SetLev(string command, string[] args)
        {
            if (args[0] == null || args[1] == null || !int.TryParse(args[1], out int n))
            {
                Monitor.Log($"Function Failed!");
                return;
            }
            n = int.Parse(args[1]);
            if (n < 1 || n > 100)
            {
                Monitor.Log($"Function Failed!");
                return;
            }

            if (args[0].ToLower() == "farming")
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
            this.Helper.Content.InvalidateCache("Data/Fish");
        }
        private void WmT(string command, string[] args)
        {
            if (!wm)
            {
                wm = true;
                Monitor.Log($"Monsters -> ON.");
            }
            else
            {
                wm = false;
                Monitor.Log($"Monsters -> OFF.");
            }
        }
        private void XpM(string command, string[] args)
        {
            if (double.TryParse(args[0], out double x) && x > 0.0)
            {
                xp_mod = x;
                Monitor.Log($"Modifier set to: {x}");
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

            if (pres_comp)
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
            }
        }
        public void Closing()
        {

            if (Game1.player.farmingLevel.Value >= 10 && shLev[0])
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
        }

        int[] defReqXPs = { 100, 380, 770, 1300, 2150, 3300, 4800, 6900, 10000, 15001 };

        private int GetReqXP(int lev)
        {
            int exp = 0;

            if (lev < 10)
            {
                exp = defReqXPs[lev];
            }
            else
            {
                exp = (int)Math.Round((1000 * lev + (lev * lev * lev * 0.33)) * xp_mod);
            }

            return exp;
        }

        private int GetDefStartXP(int lev)
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
        }

        private void ControlEvent_KeyPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.activeClickableMenu is GameMenu && e.Button == SButton.P && !pres_comp)
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
            }
            /*
            if (e.Button.IsUseToolButton() && staminaC == -1.0f && oStamina == -1.0f)
            {
                Monitor.Log($"current stamina: {Game1.player.Stamina}");
                staminaC = Game1.player.Stamina;
                oStamina = staminaC;
                tbuttondown = true;
            }*/
        }
        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            if (Context.IsWorldReady) // save is loaded
            {


                for (int i = 0; i < temp.Length; i++)
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
                }

                for (int i = 0; i < temp.Length; i++)
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
                }


            }
            if (Context.IsWorldReady && pres_comp)
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
            }

            //bool isdark = this.Helper.Reflection.GetField<Netcode.NetBool>(Game1.player.currentLocation, "isLightingDark").GetValue();

            if (Context.IsWorldReady && !no_mons && wm && Game1.player.currentLocation.IsOutdoors && Game1.activeClickableMenu == null && rand.NextDouble() <= S_R())
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
                m.Health = m.Health * (int)Math.Round(Game1.player.combatLevel.Value * 0.1 * (rand.NextDouble() + rand.NextDouble()));
                m.focusedOnFarmers = true;
                m.wildernessFarmMonster = true;
                m.Speed += rand.Next((int)Math.Round((Game1.player.combatLevel.Value / 5.0)));
                m.resilience.Set(m.resilience.Value + (Game1.player.combatLevel.Value / 10));
                m.ExperienceGained += ((10 + (Game1.player.combatLevel.Value * 2)) * tier);

                IList<NPC> characters = Game1.currentLocation.characters;
                characters.Add((NPC)m);

                total_m++;
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
                return (0.0075 + (Game1.player.combatLevel.Value * 0.0001)) * 1.5;
            }

            return (0.0075 + (Game1.player.combatLevel.Value * 0.0001));

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
                        if (Game1.player.FishingLevel < 11)
                            bobberBarSize = 80 + bobberBonus + (int)(Game1.player.FishingLevel * 9);
                        else
                            bobberBarSize = 165 + bobberBonus + (int)(Game1.player.FishingLevel * (0.5 + (rand.NextDouble() / 2.0)));
                    }
                    else
                    {
                        //bobberBarSize = 80 + bobberBonus + (int)(Game1.player.FishingLevel * 1.6);
                        if (Game1.player.FishingLevel < 11)
                            bobberBarSize = 80 + bobberBonus + (int)(Game1.player.FishingLevel * 7);
                        else if(Game1.player.FishingLevel > 10 && Game1.player.FishingLevel < 20)
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

        }
        private void TimeEvent_AfterDayStarted(object sender, EventArgs e)
        {
            //List<HoeDirt> list = new List<HoeDirt>();
            Farm farm = Game1.getFarm();
            double gchance = Game1.player.FarmingLevel * 0.001;
            foreach (Vector2 key in farm.terrainFeatures.Keys)
            {
                if (farm.terrainFeatures[key] is HoeDirt terrainFeature && terrainFeature.crop != null && rand.NextDouble() < gchance)
                    (farm.terrainFeatures[key] as HoeDirt).crop.growCompletely();
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
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
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
            }

            this.Helper.Content.InvalidateCache("Data/Fish");
        }
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {

            if (Game1.player.farmingLevel.Value >= 10)
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
            config.Xp_modifier = xp_mod;

            this.Helper.Data.WriteJsonFile<ModData>($"data/{Constants.SaveFolderName}.json", config);



            if (!no_mons)
            {
                Rem_mons();
            }

        }
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            oldXP = new int[] { 0, 0, 0, 0, 0 };
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
            shLev = new bool[] { true, true, true, true, true };
            xp_mod = 1.0;
        }
        private void AddFishingXP(int xp, int i)
        {
            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            Monitor.Log($"Adding XP: {xp * mpMult}");
            addedXP[i] += (int)(xp * mpMult);
            if ((addedXP[i] >= Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod)) && sLevs[i] < max[i])
            {
                addedXP[i] = 0;
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

                Monitor.Log($"{Game1.player.Name} leveled {i}(index) to {sLevs[i]}!");
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
                //Monitor.Log($"{Game1.player.Name} retained {i}(index) at {sLevs[i]}.");
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Closing();
        }

        public int[] GetCurXP()
        {
            return addedXP.Values;
        }

        public int[] GetReqXP()
        {
            int[] temp = { Game1.player.farmingLevel.Value, Game1.player.fishingLevel.Value, Game1.player.foragingLevel.Value, Game1.player.miningLevel.Value, Game1.player.combatLevel.Value };
            int[] xTemp = { 0, 0, 0, 0, 0 };

            for (int i = 0; i < temp.Length; i++)
            {
                xTemp[i] = (int)(Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod));
            }

            return xTemp;
        }


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
                    args.xp = value - axp[key];
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
                        args.xp = value[i] - axp[i];
                        axp[i] = value[i];

                        LEE.RaiseEvent(args);
                        //LEE.RaiseEvent();
                        break;
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
        public int xp { get; set; }
    }

    public class XPBar
    {
        public int key;
        public int xpc;
        public Timer timer;
        public DateTime time;

        public double ych = 0;
        public bool startmove = false;
        public int movedir = 0;


        public XPBar(int key, int xpc, ModEntry LE)
        {
            this.key = key;
            this.xpc = xpc;

            this.timer = new Timer(5000);
            timer.Elapsed += delegate { LE.EndXPBar(key); };

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
}

