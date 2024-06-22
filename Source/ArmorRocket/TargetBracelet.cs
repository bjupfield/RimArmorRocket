using ArmorRocket;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using ArmorRocket.ThingComps;

namespace ArmorRocket
{
    public class CompTargetBracelet : ThingComp
    {
        public CompArmorRocket armorRocket;
        Apparel brac;
        String displayString;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            brac = (Apparel)this.parent;
        }
        public void notifyArmorRocket()//called throguht the pawn_draftcontroller
        {
            armorRocket.launchArmor();
        }
        public void armorRocketDestroyed()
        {
            armorRocket = null;
        }
        void linkedArmorRocketNameChanged()
        {

        }
        public void linkArmorRocket(CompArmorRocket armorRocket)
        {
            if (this.armorRocket != null) 
            {
                armorRocket.braceletDestroyed();
            }
            this.armorRocket = armorRocket;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            //actually do this
            //Scribe_References.Look(ref armorRocket, "connectedStation");
            Scribe_Values.Look(ref displayString, "displayString");
        }
        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach(Gizmo b in base.CompGetWornGizmosExtra())
            {
                yield return b;
            }
            if (armorRocket != null)
            {
                if (armorRocket.canLaunch()) {
                    LaunchPawn_Command launchPawn = new LaunchPawn_Command(this);
                    yield return launchPawn;
                }
                if (armorRocket.hasAssigned())
                {
                    //return assigned armor commnad here
                    ReturnArmor_Command returnArmor = new ReturnArmor_Command(this);
                    yield return returnArmor;
                }
            }

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
