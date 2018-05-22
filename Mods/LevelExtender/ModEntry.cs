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
        int[] addedXP = { 0, 0, 0, 0, 0 };
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

        bool no_mons = false;

        private LEModApi API;


        public ModEntry()
        {
            instance = this;
        }

        public override object GetApi()
        {
            return API = new LEModApi();
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Fish");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            asset
                .AsDictionary<int, string>()
                .Set((id, data) =>
                {
                    string[] fields = data.Split('/');
                    if (int.TryParse(fields[1], out int val))
                    {
                        int x = (val - rand.Next(0, (int)(Game1.player.FishingLevel / 4)));
                        if (x < 1)
                            x = rand.Next(1, val);
                        fields[1] = x.ToString();
                        return string.Join("/", fields);
                    }
                    else
                    {
                        return string.Join("/", fields);
                    }
                });
        }


        public override void Entry(IModHelper helper)
        {
            //instance = this;
            GameEvents.OneSecondTick += this.GameEvents_OneSecondTick;
            GameEvents.EighthUpdateTick += this.GameEvents_QuarterSecondTick;
            SaveEvents.AfterLoad += this.SaveEvents_AfterLoad;
            SaveEvents.BeforeSave += this.SaveEvents_BeforeSave;
            SaveEvents.AfterReturnToTitle += this.SaveEvents_AfterReturnToTitle;
            MenuEvents.MenuChanged += this.MenuEvents_MenuChanged;
            MenuEvents.MenuClosed += this.MenuEvents_MenuClosed;
            ControlEvents.KeyPressed += this.ControlEvent_KeyPressed;
            TimeEvents.AfterDayStarted += this.TimeEvent_AfterDayStarted;

            helper.ConsoleCommands.Add("xp", "Tells the players current XP above or at level 10.", this.tellXP);
            helper.ConsoleCommands.Add("lev", "Sets the player's level: lev <type> <number>", this.SetLev);
            helper.ConsoleCommands.Add("wm_toggle", "Toggles monster spawning: wm_toggle", this.wmT);
            helper.ConsoleCommands.Add("xp_m", "Changes the xp modifier for levels 10 and after: xp_m <decimal 0.0 -> ANY> : 1.0 is default.", this.xpM);
            //helper.ConsoleCommands.Add("warp", "Sets the player's level: lev <type> <number>", this.Warp);

            this.Helper.Content.InvalidateCache("Data/Fish");

        }

        private void tellXP(string command, string[] args)
        {

            int[] temp = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
            this.Monitor.Log($"XP: Farming  Fishing  Foraging  Mining  Combat |");
            for (int i = 0; i < addedXP.Length; i++)
            {
                if (temp[i] >= 10)
                    this.Monitor.Log($"XP: {addedXP[i]}");
                else
                    this.Monitor.Log($"XP: Empty.");
            }
            this.Monitor.Log($"\n");
            for (int i = 0; i < sLevs.Length; i++)
            {
                if (temp[i] >= 10)
                    this.Monitor.Log($"NL: {Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod)}");
                else
                    this.Monitor.Log($"XP: Empty.");
            }
            for (int i = 0; i < 5; i++)
            {
                this.Monitor.Log($"Current XP - Default: {Game1.player.experiencePoints[i]}.");
            }

        }
        private void SetLev(string command, string[] args)
        {
            int n;
            if (args[0] == null || args[1] == null || !int.TryParse(args[1], out n))
            {
                this.Monitor.Log($"Function Failed!");
                return;
            }
            n = int.Parse(args[1]);
            if (n < 1 || n > 100)
            {
                this.Monitor.Log($"Function Failed!");
                return;
            }

            if (args[0].ToLower() == "farming")
            {
                Game1.player.FarmingLevel = n;
                if (n < 10)
                {
                    sLevs[0] = 10;
                    addedXP[0] = 0;
                    config.faLV = 10;
                    config.faXP = 0;
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
                Game1.player.FishingLevel = n;
                if (n < 10)
                {
                    sLevs[1] = 10;
                    addedXP[1] = 0;
                    config.fLV = 10;
                    config.fXP = 0;
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
                Game1.player.ForagingLevel = n;
                if (n < 10)
                {
                    sLevs[2] = 10;
                    addedXP[2] = 0;
                    config.foLV = 10;
                    config.foXP = 0;
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
                Game1.player.MiningLevel = n;
                if (n < 10)
                {
                    sLevs[3] = 10;
                    addedXP[3] = 0;
                    config.mLV = 10;
                    config.mXP = 0;
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
                Game1.player.CombatLevel = n;
                if (n < 10)
                {
                    sLevs[4] = 10;
                    addedXP[4] = 0;
                    config.cLV = 10;
                    config.cXP = 0;
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
        private void wmT(string command, string[] args)
        {
            if (!wm)
            {
                wm = true;
                this.Monitor.Log($"Monsters -> ON.");
            }
            else
            {
                wm = false;
                this.Monitor.Log($"Monsters -> OFF.");
            }
        }
        private void xpM(string command, string[] args)
        {
            if (double.TryParse(args[0], out double x) && x > 0.0)
            {
                xp_mod = x;
                this.Monitor.Log($"Modifier set to: {x}");
            }
            else
            {
                this.Monitor.Log($"Valid decimal not used; refer to help command.");
            }
        }
        private void MenuEvents_MenuChanged(object sender, EventArgsClickableMenuChanged e)
        {

        }
        private void MenuEvents_MenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (pres_comp)
            {
                if (e.PriorMenu.GetType().FullName == "SkillPrestige.Menus.PrestigeMenu")
                {
                    //closing();
                    SetTimer();
                }
                else if (e.PriorMenu.GetType().FullName == "StardewValley.Menus.GameMenu")
                {
                    //closing();
                    SetTimer();
                }
                else if (e.PriorMenu.GetType().FullName == "SkillPrestige.Menus.SettingsMenu")
                {
                    //closing();
                    SetTimer();
                }
                else if (e.PriorMenu.GetType().FullName == "SkillPrestige.Menus.Dialogs.WarningDialog")
                {
                    //closing();
                    SetTimer();
                }
            }
        }
        public void closing()
        {

            if (Game1.player.FarmingLevel >= 10 && shLev[0])
            {
                Game1.player.FarmingLevel = origLevs[0];

            }
            else if (!shLev[0])
            {
                if (origLevs[0] - 10 >= 10)
                    sLevs[0] = origLevs[0] - 10;
                else
                    sLevs[0] = 10;
                Game1.player.FarmingLevel = origLevs[0] - 10;
                addedXP[0] = 0;
                oldXP[0] = 0;
                newXP[0] = 0;
                old[0] = false;

            }
            if (Game1.player.FishingLevel >= 10 && shLev[1])
            {
                Game1.player.FishingLevel = origLevs[1];

            }
            else if (!shLev[1])
            {

                if (origLevs[1] - 10 >= 10)
                    sLevs[1] = origLevs[1] - 10;
                else
                    sLevs[1] = 10;
                Game1.player.FishingLevel = origLevs[1] - 10;
                addedXP[1] = 0;
                oldXP[1] = 0;
                newXP[1] = 0;
                old[1] = false;

            }
            if (Game1.player.ForagingLevel >= 10 && shLev[2])
            {
                Game1.player.ForagingLevel = origLevs[2];

            }
            else if (!shLev[2])
            {
                if (origLevs[2] - 10 >= 10)
                    sLevs[2] = origLevs[2] - 10;
                else
                    sLevs[2] = 10;
                Game1.player.ForagingLevel = origLevs[2] - 10;
                addedXP[2] = 0;
                oldXP[2] = 0;
                newXP[2] = 0;
                old[2] = false;

            }
            if (Game1.player.MiningLevel >= 10 && shLev[3])
            {
                Game1.player.MiningLevel = origLevs[3];

            }
            else if (!shLev[3])
            {
                if (origLevs[3] - 10 >= 10)
                    sLevs[3] = origLevs[3] - 10;
                else
                    sLevs[3] = 10;
                Game1.player.MiningLevel = origLevs[3] - 10;
                addedXP[3] = 0;
                oldXP[3] = 0;
                newXP[3] = 0;
                old[3] = false;

            }
            if (Game1.player.CombatLevel >= 10 && shLev[4])
            {
                Game1.player.CombatLevel = origLevs[4];

            }
            else if (!shLev[4])
            {
                if (origLevs[4] - 10 >= 10)
                    sLevs[4] = origLevs[4] - 10;
                else
                    sLevs[4] = 10;
                Game1.player.CombatLevel = origLevs[4] - 10;
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
        private int getExp(int x)
        {
            switch (x)
            {
                case 0:
                    x = 0;
                    break;
                case 1:
                    x = 100;
                    break;
                case 2:
                    x = 380;
                    break;
                case 3:
                    x = 770;
                    break;
                case 4:
                    x = 1300;
                    break;
                case 5:
                    x = 2150;
                    break;
                case 6:
                    x = 3300;
                    break;
                case 7:
                    x = 4800;
                    break;
                case 8:
                    x = 6900;
                    break;
                case 9:
                    x = 10000;
                    break;
                default:
                    x = 15001;
                    break;
            }
            return x;
        }
        private void ControlEvent_KeyPressed(object sender, EventArgsKeyPressed e)
        {

            if (Game1.activeClickableMenu is GameMenu && e.KeyPressed == Microsoft.Xna.Framework.Input.Keys.P && !pres_comp)
            {
                //this.Monitor.Log($"{Game1.player.name} pressed P--.");
                pres_comp = true;
                if (Game1.player.FarmingLevel > 10)
                {
                    origLevs[0] = Game1.player.FarmingLevel;
                    Game1.player.FarmingLevel = 10;

                }
                if (Game1.player.FishingLevel > 10)
                {
                    origLevs[1] = Game1.player.FishingLevel;
                    Game1.player.FishingLevel = 10;

                }
                if (Game1.player.ForagingLevel > 10)
                {
                    origLevs[2] = Game1.player.ForagingLevel;
                    Game1.player.ForagingLevel = 10;

                }
                if (Game1.player.MiningLevel > 10)
                {
                    origLevs[3] = Game1.player.MiningLevel;
                    Game1.player.MiningLevel = 10;

                }
                if (Game1.player.CombatLevel > 10)
                {
                    origLevs[4] = Game1.player.CombatLevel;
                    Game1.player.CombatLevel = 10;

                }
            }
        }
        private void GameEvents_OneSecondTick(object sender, EventArgs e)
        {
            int[] temp = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
            if (Context.IsWorldReady) // save is loaded
            {

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
                            addFishingXP(newXP[i] - oldXP[i], i);
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
            if (Context.IsWorldReady && !no_mons && wm && Game1.player.currentLocation.IsOutdoors && Game1.activeClickableMenu == null && rand.NextDouble() <= s_r())
            {

                Vector2 loc = Game1.player.currentLocation.getRandomTile();
                while (!(Game1.player.currentLocation.isTileLocationTotallyClearAndPlaceable(loc)))
                {
                    loc = Game1.player.currentLocation.getRandomTile();
                }

                Monster m = getMonster(rand.Next(7), loc * (float)Game1.tileSize);
                m.DamageToFarmer = (int)(m.DamageToFarmer / 1.5) + (int)(Game1.player.CombatLevel / 3);
                m.Health = (int)(m.Health / 1.5) + ((Game1.player.CombatLevel / 2) * (m.Health / 10));
                m.focusedOnFarmers = true;
                m.wildernessFarmMonster = true;
                m.Speed += rand.Next(3 + Game1.player.CombatLevel);
                m.resilience.Set(m.resilience.Value + (Game1.player.CombatLevel / 10));
                m.ExperienceGained += (Game1.player.CombatLevel / 2);

                IList<NPC> characters = Game1.currentLocation.characters;
                characters.Add((NPC)m);


            }

        }
        public double s_r()
        {
            double x;

            if (API.overSR == -1.0)
            {
                if (Game1.player.CombatLevel == 0 && !Game1.isDarkOut())
                {
                    return 0.0;
                }

                if (Game1.isDarkOut() || Game1.isRaining)
                {
                    return (0.010 + (Game1.player.CombatLevel * 0.0001)) * 2;
                }

                return (0.010 + (Game1.player.CombatLevel * 0.0001));

            }
            else
            {
                if (Game1.player.CombatLevel == 0 && !Game1.isDarkOut())
                {
                    x = 0.0;
                }

                else if (Game1.isDarkOut() || Game1.isRaining)
                {
                    x = (0.010 + (Game1.player.CombatLevel * 0.0001)) * 2;
                }

                else
                {
                    x = (0.010 + (Game1.player.CombatLevel * 0.0001));
                }

            }

            return x + API.overSR;

        }
        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            if (Context.IsWorldReady && Game1.player.FishingLevel > 10) 
            {
                if (Game1.activeClickableMenu is BobberBar && !firstFade)
                {


                    firstFade = true;
                    this.Monitor.Log($"{this.Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "bobberBarHeight").GetValue()} -SIZE.");
                    this.Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "bobberBarHeight").SetValue(176);
                    this.Helper.Reflection.GetField<float>(Game1.activeClickableMenu, "bobberBarPos").SetValue((float)(568 - 176));
                    

                }
                else if (!(Game1.activeClickableMenu is BobberBar) && firstFade)
                {
                    firstFade = false;
                    //this.Monitor.Log($"Fade set false. -SIZE.");
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

            no_mons = false;
        }

        public void rem_mons()
        {

            no_mons = true;

            Monitor.Log("Removed Monsters.");
            for (int j = 0; j < Game1.locations.Count; j++)
            {
                //Game1.locations[j].cleanupBeforeSave();

                IList<NPC> characters = Game1.locations[j].characters;
                for (int i = 0; i < characters.Count; i++)
                {
                    if (characters[i] is Monster && (characters[i] as Monster).wildernessFarmMonster)
                    {
                        characters.RemoveAt(i);


                    }
                }
            }
        }

        private Monster getMonster(int x, Vector2 loc)
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
                default:
                    m = new Monster();
                    break;
            }

            return m;
        }
        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            //object[] temp = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
            var config_t = this.Helper.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json") ?? new ModData();
            addedXP[0] = config_t.faXP;
            sLevs[0] = config_t.faLV;
            addedXP[1] = config_t.fXP;
            sLevs[1] = config_t.fLV;
            addedXP[2] = config_t.foXP;
            sLevs[2] = config_t.foLV;
            addedXP[3] = config_t.mXP;
            sLevs[3] = config_t.mLV;
            addedXP[4] = config_t.cXP;
            sLevs[4] = config_t.cLV;
            wm = config_t.worldMonsters;
            xp_mod = config_t.xp_modifier;

            config = config_t;

            if (Game1.player.FarmingLevel >= 10)
            {
                Game1.player.FarmingLevel = sLevs[0];

                this.Monitor.Log($"{Game1.player.Name} loaded farming level {Game1.player.FarmingLevel}!");
            }
            if (Game1.player.FishingLevel >= 10)
            {
                Game1.player.FishingLevel = sLevs[1];

                this.Monitor.Log($"{Game1.player.Name} loaded fishing level {Game1.player.FishingLevel}!");
            }
            if (Game1.player.ForagingLevel >= 10)
            {
                Game1.player.ForagingLevel = sLevs[2];

                this.Monitor.Log($"{Game1.player.Name} loaded foraging level {Game1.player.ForagingLevel}!");
            }
            if (Game1.player.MiningLevel >= 10)
            {
                Game1.player.MiningLevel = sLevs[3];

                this.Monitor.Log($"{Game1.player.Name} loaded mining level {Game1.player.MiningLevel}!");
            }
            if (Game1.player.CombatLevel >= 10)
            {
                Game1.player.CombatLevel = sLevs[4];

                this.Monitor.Log($"{Game1.player.Name} loaded combat level {Game1.player.CombatLevel}!");
            }

            this.Helper.Content.InvalidateCache("Data/Fish");
        }
        private void SaveEvents_BeforeSave(object sender, EventArgs e)
        {

            if (Game1.player.FarmingLevel >= 10)
            {

                Game1.player.FarmingLevel = sLevs[0];
                config.faXP = addedXP[0];
                config.faLV = sLevs[0];

                this.Monitor.Log($"{Game1.player.Name} saved farming level {Game1.player.FarmingLevel}!");
            }
            if (Game1.player.FishingLevel >= 10)
            {

                Game1.player.FishingLevel = sLevs[1];
                config.fXP = addedXP[1];
                config.fLV = sLevs[1];

                this.Monitor.Log($"{Game1.player.Name} saved fishing level {Game1.player.FishingLevel}!");
            }
            if (Game1.player.ForagingLevel >= 10)
            {

                Game1.player.ForagingLevel = sLevs[2];
                config.foXP = addedXP[2];
                config.foLV = sLevs[2];

                this.Monitor.Log($"{Game1.player.Name} saved foraging level {Game1.player.ForagingLevel}!");
            }
            if (Game1.player.MiningLevel >= 10)
            {

                Game1.player.MiningLevel = sLevs[3];
                config.mXP = addedXP[3];
                config.mLV = sLevs[3];

                this.Monitor.Log($"{Game1.player.Name} saved mining level {Game1.player.MiningLevel}!");
            }
            if (Game1.player.CombatLevel >= 10)
            {

                Game1.player.CombatLevel = sLevs[4];
                config.cXP = addedXP[4];
                config.cLV = sLevs[4];

                this.Monitor.Log($"{Game1.player.Name} saved combat level {Game1.player.CombatLevel}!");
            }
            config.worldMonsters = wm;
            config.xp_modifier = xp_mod;

            this.Helper.WriteJsonFile<ModData>($"data/{Constants.SaveFolderName}.json", config);

            

            if (!no_mons)
            {
                rem_mons();
            }
        }
        private void SaveEvents_AfterReturnToTitle(object sender, EventArgs e)
        {
            oldXP = new int[] { 0, 0, 0, 0, 0 };
            newXP = new int[] { 0, 0, 0, 0, 0 };
            old = new bool[] { false, false, false, false, false };
            addedXP = new int[] { 0, 0, 0, 0, 0 };
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
        private void addFishingXP(int xp, int i)
        {
            int[] temp = { Game1.player.FarmingLevel, Game1.player.FishingLevel, Game1.player.ForagingLevel, Game1.player.MiningLevel, Game1.player.CombatLevel };
            this.Monitor.Log($"Adding XP: {xp}");
            addedXP[i] += xp;
            if ((addedXP[i] >= Math.Round((1000 * temp[i] + (temp[i] * temp[i] * temp[i] * 0.33)) * xp_mod)) && sLevs[i] < max[i])
            {
                addedXP[i] = 0;
                sLevs[i] += 1;

                if (i == 0)
                {
                    Game1.player.FarmingLevel = sLevs[i];
                }
                else if (i == 1)
                {
                    Game1.player.FishingLevel = sLevs[i];
                }
                else if (i == 2)
                {
                    Game1.player.ForagingLevel = sLevs[i];
                }
                else if (i == 3)
                {
                    Game1.player.MiningLevel = sLevs[i];
                }
                else if (i == 4)
                {
                    Game1.player.CombatLevel = sLevs[i];
                }

                this.Monitor.Log($"{Game1.player.Name} leveled {i}(index) to {sLevs[i]}!");
            }
            else
            {
                if (i == 0)
                {
                    Game1.player.FarmingLevel = sLevs[i];
                }
                else if (i == 1)
                {
                    Game1.player.FishingLevel = sLevs[i];
                }
                else if (i == 2)
                {
                    Game1.player.ForagingLevel = sLevs[i];
                }
                else if (i == 3)
                {
                    Game1.player.MiningLevel = sLevs[i];
                }
                else if (i == 4)
                {
                    Game1.player.CombatLevel = sLevs[i];
                }
                this.Monitor.Log($"{Game1.player.Name} retained {i}(index) at {sLevs[i]}.");
            }
        }
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1100);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.closing();
        }

        
    }


}

