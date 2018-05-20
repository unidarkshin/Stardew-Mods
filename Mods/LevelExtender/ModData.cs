namespace LevelExtender {
    public class ModData
    {
        public int fXP { get; set; }
        public int fLV { get; set; }
        public int faXP { get; set; }
        public int faLV { get; set; }
        public int mXP { get; set; }
        public int mLV { get; set; }
        public int cXP { get; set; }
        public int cLV { get; set; }
        public int foXP { get; set; }
        public int foLV { get; set; }
        public bool worldMonsters { get; set; }
        public double xp_modifier { get; set; }
        public ModData()
        {
            this.fXP = 0;
            this.fLV = 10;
            this.faXP = 0;
            this.faLV = 10;
            this.mXP = 0;
            this.mLV = 10;
            this.cXP = 0;
            this.cLV = 10;
            this.foXP = 0;
            this.foLV = 10;
            this.worldMonsters = false;
            this.xp_modifier = 1.0;
        }
    }
}