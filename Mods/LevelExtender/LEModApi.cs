using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI;

namespace LevelExtender
{
    public class LEModApi
    {
        public ModEntry ME;

        public LEModApi(ModEntry me)
        {
            ME = me;
        }

        public ModEntry GetMod()
        {
            return ME;
        }

        //This value will offset spawn-rate by the specified amount (1 second intervals)
        public double overSR = -1.0;

        public void Spawn_Rate(double osr)
        {
            overSR = osr;
        }

        /*public int[] CurrentXP()
        {
            return ME.GetCurXP();
        }

        public int[] RequiredXP()
        {
            return ME.GetReqXP();
        }*/

        public event EventHandler<EXPEventArgs> OnXPChanged
        {
            add => ME.LEE.OnXPChanged += value;
            remove => ME.LEE.OnXPChanged -= value;
        }

        public dynamic TalkToSkill(string[] args)
        {
            return ME.TalkToSkill(args);
        }
        public int initializeSkill(string name, int xp, double xp_mod, List<int> xp_table = null, int[] cats = null)
        {
            return ME.initializeSkill(name, xp, xp_mod, xp_table, cats);
        }
    }
}
