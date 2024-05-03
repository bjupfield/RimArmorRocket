using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Verse.AI
{
    public class CompArmorRocket : ThingComp
    {
        public Apparel targetBracelet;
        Pawn target;
        String displayTarget;

        public void linkTargetBracelet(Thing targetBracelet)
        {
            this.targetBracelet = (Apparel)targetBracelet;
            target = this.targetBracelet.Wearer;
        }
        public void notifyTargetChanged()
        {

        }
        public void launchArmor(Verse.Pawn pawn)
        {
            Verse.Log.Warning("Armor \"Lauched\".");
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref targetBracelet, "connectedBracelet");
            Scribe_References.Look(ref target, "connectedPawn");
            Scribe_Values.Look(ref displayTarget, "displayString");
        }
    }
    public class CompProperties_ArmorRocket : CompProperties
    {
        public CompProperties_ArmorRocket() 
        {
            compClass = typeof(CompArmorRocket); 
        }
    }
}
