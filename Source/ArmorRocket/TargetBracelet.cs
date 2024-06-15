using ArmorRocket;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ArmorRocket
{
    public class CompTargetBracelet : ThingComp
    {
        ArmorRocket.ArmorRocketThing armorRocket;
        String displayString;
        public void notifyArmorRocket(Verse.Pawn pawn)//called throguht the pawn_draftcontroller
        {
            armorRocket.launchArmor(pawn);
        }
        void linkedArmorRocketDestroyed()
        {

        }
        void linkedArmorRocketNameChanged()
        {

        }
        public void linkArmorRocket(Thing armorRocket)
        {
            this.armorRocket = (ArmorRocket.ArmorRocketThing)armorRocket;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref armorRocket, "connectedStation");
            Scribe_Values.Look(ref displayString, "displayString");
        }
    }
    public class CompProperties_TargetBracelet : CompProperties
    {
        public CompProperties_TargetBracelet() 
        {
            compClass = typeof(CompTargetBracelet);
        }
    }
}
