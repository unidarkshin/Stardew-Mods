using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace ExtremeFishingOverhaul
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor
    {

        public static Mod instance;
        public static Random rnd;

        String[] fType = { "mixed", "dart", "floater", "sinker" };
        String[] names = { "Jaguar", "Loveless", "Horny", "Chinese", "Japanese", "Korean", "Frightened", "American", "Sweedish", "Iranian", "Polish", "Bloodied", "Decaying", "Dead", "Dying", "Eager", "Kawaii", "Russian", "German", "Noodle", "Floppy", "Disk", "Bubble", "Fruit", "Stinky", "Taco", "Knife", "Hawk", "Night", "Sky", "Lizard", "Clown", "Floob", "Peruvian", "Forgotten", "Cranky", "Loopy", "Irresponsible", "Fish", "Dog", "Cat", "Mother", "Father", "Transvestite", "Llama", "Giraffe", "Expert", "Happy", "Sad", "Real", "Fake", "Cheating", "Freaky", "Legendary", "Common", "Rare", "Ultimate", "Original", "Freaky", "Slimy", "Rude", "Discriminatory", "Anorexic", "Enslaved", "Ghost", "Bird", "Squish", "Helium", "Metal", "Wood", "Stone", "Borking", "Skinny", "Janky", "Swaggy", "Oblivious", "Boring", "Raping", "Racist", "Fat", "Astral", "Elysian", "Celestial", "Angelic", "Immortal", "Obese", "Janky", "Strange", "Eerie", "Fishy", "Star", "Stringy", "Politically Correct", "Cougar", "BBW", "Creamy", "Moist", "Rookie", "Undead", "Listless", "Horny", "Bland", "Seasoned", "Careful", "Belligerent", "Uncanny", "Light", "Flakey", "Flaming", "Copious", "Arrogant", "Scaley", "Hallowed", "Sacred", "Small", "Excited", "Smothered", "Lava", "Molten", "Drake", "Sandy", "Ice", "Chocolate", "Slippery", "Shadey", "Elemental", "Expermental", "Air", "Fire", "Nature", "Earth", "Tree", "Mutant", "Defecating", "Physics Defying", "Passionate", "Cute", "Sexual", "Lonely", "Anxious", "Terrible", "Impressive", "Mentally", "Dangerous", "Intelligent", "Lucky", "Dramatic", "Embarrassed", "Conscious", "Wonderful", "Wonder", "Unique", "Freezing", "Beautiful", "Enchanted", "Ghostly" };
        String[] n_post = { "Fish", "Ray", "Angelfish", "Moonfish", "Eel", "Sucker", "Dragonfish", "Minnou", "Angler", "Prowfish" };


        List<string> diffs = new List<string>();
        List<string> fIDS = new List<string>();
        List<string> type = new List<string>();
        List<string> levRS = new List<string>();
        List<bool> legend;
        List<bool> legend2;

        int MaxFish;
        int maxFL;
        int minFL;
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        /// 

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(@"Data\Fish") || asset.AssetNameEquals(@"Data\ObjectInformation") || asset.AssetNameEquals(@"Data\Locations") || asset.AssetNameEquals(@"Maps\springobjects");
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {



            if (asset.AssetNameEquals(@"Data\Fish"))
            {


                for (int i = 0; i < MaxFish; i++)
                {
                    double spawn;

                    if (legend[i] == true)
                    {
                        spawn = 0.2;
                    }
                    else if (legend2[i] == true)
                    {
                        spawn = 0.1;
                    }
                    else
                    {
                        spawn = (rnd.Next(3, 5 - Math.Max(0, (int)(Math.Round((double)(int.Parse(levRS[i]) / 50))))) / 10);
                    }



                    asset
                            .AsDictionary<int, string>()
                            .Set
                            (int.Parse(fIDS[i]), $"Fish {fIDS[i]}/{diffs[i]}/{type[i]}/1/{rnd.Next(10, 100).ToString()}/600 2600/spring/both/1 .1 1 .1/{rnd.Next(3, 6).ToString()}/{spawn.ToString()}/0.5/{levRS[i]}");


                }



            }
            else if (asset.AssetNameEquals(@"Data\ObjectInformation"))
            {
                //this.Monitor.Log("2nd");
                bool rare = false;
                bool rare2 = false;



                for (int i = 0; i < MaxFish; i++)
                {
                    double y = rnd.NextDouble();
                    string x = "";
                    if (y < 0.5)
                    {
                        x = names[rnd.Next(0, names.Length - 1)] + " " + n_post[rnd.Next(0, n_post.Length - 1)];
                    }
                    else if (y < 0.8)
                    {
                        x = names[rnd.Next(0, names.Length - 1)] + " " + names[rnd.Next(0, names.Length - 1)] + " " + n_post[rnd.Next(0, n_post.Length - 1)];
                    }
                    else
                    {
                        x = names[rnd.Next(0, names.Length - 1)] + " " + names[rnd.Next(0, names.Length - 1)] + " " + names[rnd.Next(0, names.Length - 1)] + " " + n_post[rnd.Next(0, n_post.Length - 1)];
                    }

                    if (x.ToLower().Contains("elysian") || x.ToLower().Contains("angelic") || x.ToLower().Contains("immortal"))
                    {
                        rare = true;
                        x = x.ToUpper();
                        legend[i] = true;

                        //Monitor.Log("Celestial F Created.");
                    }
                    else if (x.ToLower().Contains("celestial") || x.ToLower().Contains("astral"))
                    {
                        rare2 = true;
                        x = x.ToUpper();
                        legend2[i] = true;

                        //Monitor.Log("Celestial2 F Created.");
                    }

                    int id = 804 + i;
                    int levR = rnd.Next(minFL, maxFL);
                    int diff;
                    string ft;

                    if (rare)
                    {
                        diff = rnd.Next(85, 150);
                        ft = fType[rnd.Next(0, 1)];
                    }
                    else if (rare2)
                    {
                        diff = rnd.Next(135, 250);
                        ft = fType[rnd.Next(0, 1)];
                    }
                    else
                    {
                        diff = rnd.Next(20, 120);
                        ft = fType[rnd.Next(0, fType.Length - 1)];
                    }

                    diff = (int)(diff * (((levR + 100.0) / 250.0) + 0.30));
                    int price = (int)(((diff * diff) / 40.0) * (((levR + 8) * (levR + 8)) / (100.0 * ((levR + 1) / 2.0))));


                    int food = 0 + (int)((diff / 20.0) * rnd.Next(-1 * (int)(levR + 10 / 5.0), (int)(levR + 10 / 5.0)));

                    //this.Monitor.Log($"Levs: {levR}");

                    if (ft == "mixed")
                    {
                        price = (int)(price * 1.5);
                    }
                    else if (ft == "dart")
                    {
                        price = (int)(price * 1.25);
                    }
                    else if (ft == "floater")
                    {
                        price = (int)(price * 0.8);
                    }
                    else
                    {
                        price = (int)(price * 0.6);
                    }

                    //"Astral", "Elysian", "Celestial", "Angelic", "Immortal",
                    if (rare)
                    {
                        price *= rnd.Next(2, 4);
                        food *= rnd.Next(2, 4);
                    }
                    else if (rare2)
                    {
                        price *= rnd.Next(10, 20);
                        food *= rnd.Next(10, 20);
                    }

                    asset
                            .AsDictionary<int, string>()
                            .Set
                            (id, $"{x}/{price.ToString()}/{food.ToString()}/Fish -4/{x}/Price: {price.ToString()}\nDifficulty: {diff.ToString()}\nLevel Requirement: {levR.ToString()}\nFish Type: {ft}/ Day Night^Spring Fall");

                    fIDS.Add(id.ToString());
                    diffs.Add(diff.ToString());
                    type.Add(ft);
                    levRS.Add(levR.ToString());

                    rare = false;
                    rare2 = false;
                }





            }
            else if (asset.AssetNameEquals(@"Data\Locations"))
            {

                asset
                .AsDictionary<string, string>()
                .Set((id, data) =>
                {
                    
                    string[] fields = data.Split('/');
                    

                    for (int i = 0; i < fIDS.Count; i++)
                    {
                        if (rnd.NextDouble() < 0.33)
                        {
                            fields[4] += " " + fIDS[i] + " -1";
                            //this.Monitor.Log($"|||||-----> {fIDS[i]}");
                        }
                        if (rnd.NextDouble() < 0.33)
                        {
                            fields[5] += " " + fIDS[i] + " -1";
                            //this.Monitor.Log($"|||||-----> {fIDS[i]}");
                        }
                        if (rnd.NextDouble() < 0.33)
                        {
                            fields[6] += " " + fIDS[i] + " -1";
                            //this.Monitor.Log($"|||||-----> {fIDS[i]}");
                        }
                        if (rnd.NextDouble() < 0.33)
                        {
                            fields[7] += " " + fIDS[i] + " -1";
                            //this.Monitor.Log($"|||||-----> {fIDS[i]}");
                        }


                    }

                    return string.Join("/", fields);
  
                });

            }
            else if (asset.AssetNameEquals(@"Maps\springobjects"))
            {
                var texture = this.Helper.Content.Load<Texture2D>(@"assets\springobjects.xnb", ContentSource.ModFolder);
                asset.AsImage().ReplaceWith(texture);
            }
        }


        public override void Entry(IModHelper helper)
        {

            ModConfig config = helper.ReadConfig<ModConfig>();
            rnd = new Random(config.seed);
            Monitor.Log("Seed loaded.");

            int x = 10;
            if (this.Helper.ModRegistry.IsLoaded("Devin_Lematty.Level_Extender") && !config.maxLevOverride)
            {
                x = 100;

                config.maxFishingLevel = x;

                this.Helper.WriteConfig<ModConfig>(config);
            }


            maxFL = config.maxFishingLevel;
            minFL = config.minFishingLevel;
            MaxFish = config.maxFish;

            legend = new List<bool>(new bool[MaxFish]);
            legend2 = new List<bool>(new bool[MaxFish]);

        }


    }
}
