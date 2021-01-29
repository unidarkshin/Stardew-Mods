using System.Collections.Generic;

namespace LevelExtender {
    public class ModData
    {

        public bool WorldMonsters { get; set; }

        public bool drawBars { get; set; }

        public bool drawExtraItemNotifications { get; set; }

        public List<string> skills { get; set; }

        public ModData()
        {

            this.WorldMonsters = false;

            this.drawBars = true;

            this.drawExtraItemNotifications = true;

            this.skills = new List<string>();
        }
    }
}