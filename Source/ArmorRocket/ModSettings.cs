using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ArmorRocket.ModSetting
{
    public class ArmorRocketModSettings : ModSettings
    {

        public int rocketSpeed = 10;

        public bool noMend = false;
        public bool noMech = false;
        public bool noDisplay = false;

        public bool nosound = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref rocketSpeed, "speed", 10);
            Scribe_Values.Look(ref nosound, "sound", defaultValue: false);
        }
    }
}
