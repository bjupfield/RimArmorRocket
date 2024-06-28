using ArmorRacks.Things;
using ArmorRocket.Defs;
using ArmorRocket.ThingDefs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using ArmorRacks.Drawers;

namespace ArmorRocket.ThingComps
{
    public class CompArmorRocket : ThingComp
    {
        static int consumeRate = 15;
        private CompProperties_ArmorRocket Props => (CompProperties_ArmorRocket)props;
        public List<string> assignedThingsID;
        Apparel targetBracelet;
        Pawn target;
        CompRefuelable fuel => parent.GetComp<CompRefuelable>();
        CompPowerTrader power => parent.GetComp<CompPowerTrader>();
        public EffecterDef flight;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            flight = ((CompProperties_ArmorRocket)props).flight;
            assignedThingsID = new List<string>();

            ArmorRack rack = (ArmorRack)parent;

            rack.ContentsDrawer = new MechanizedArmorRackContentsDrawer(rack);
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption option in base.CompFloatMenuOptions(selPawn))
            {
                yield return option;
            }
            if (selPawn.apparel.WornApparel.Find(a =>
            {
                return a.HasComp<CompTargetBracelet>();
            }) != null && power.PowerOn)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Link Bracelet", delegate
                {
                    this.parent.SetForbidden(value: false, warnOnFail: false);
                    Job job23 = JobMaker.MakeJob((JobDef)ArmorRocketJobDefOf.ArmorRocket_LinkBracelet, this.parent);
                    job23.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job23, Verse.AI.JobTag.Misc);

                }, MenuOptionPriority.High), selPawn, this.parent);
            }

        }
        public void linkTargetBracelet(Apparel targetBracelet)
        {
            if(this.targetBracelet != null)
            {
                this.targetBracelet.GetComp<CompTargetBracelet>().armorRocketDestroyed();
            }
            this.targetBracelet = targetBracelet;
            target = this.targetBracelet.Wearer;
            ArmorRack rack = (ArmorRack)parent;
            rack.BodyTypeDef = targetBracelet.Wearer.story.bodyType;
            assignedThingsID = new List<string>();
        }
        public void launchArmor()
        {
            ArmorRack rack = (ArmorRack)parent;
            if (rack != null)
            {
                assignedThingsID = new List<string>();


                ArmorProjectile launchThis = (ArmorProjectile)GenSpawn.Spawn(ArmorRocketThingDefOf.ArmorRocketProjectile, rack.Position, rack.Map);
                {

                    if (rack.GetStoredApparel().Count > 0)
                    {
                        foreach (Thing t in rack.GetStoredApparel())
                        {
                            if(ApparelUtility.HasPartsToWear(targetBracelet.Wearer, t.def))
                            {
                                assignedThingsID.Add(t.ThingID);
                            }
                        }
                    }
                    if (rack.GetStoredWeapon() != null && !targetBracelet.Wearer.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        assignedThingsID.Add(rack.GetStoredWeapon().ThingID);
                    }
                }

                fuel.ConsumeFuel(consumeRate);

                LocalTargetInfo d = new LocalTargetInfo(targetBracelet);
                launchThis.Launch(rack, rack.DrawPos, rack.Position, d, new ProjectileHitFlags());
            }
        }
        public void braceletDestroyed()
        {
            targetBracelet = null;
            target = null;
        }
        public bool canLaunch()
        {
            if (fuel.HasFuel)
            {
                if (power.PowerOn) {
                    ArmorRack rack = (ArmorRack)parent;
                    if (rack != null)
                    {
                        if (rack.GetStoredApparel().Count > 0 || rack.GetStoredWeapon() != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public bool hasAssigned()
        {
            if (assignedThingsID.Count > 0)
            {
                foreach (String b in assignedThingsID)
                {
                    if (targetBracelet.Wearer.apparel.WornApparel.Find(c=> { return c.ThingID == b; }) != null || (targetBracelet.Wearer.equipment.Primary != null && targetBracelet.Wearer.equipment.Primary.ThingID  == b)) 
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public bool hasPower()
        {
            return power.PowerOn;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref flight, "flight");
            Scribe_Collections.Look(ref assignedThingsID, "assignedThings", LookMode.Value);
            Scribe_References.Look(ref targetBracelet, "target");
            Scribe_References.Look(ref parent, "parent");

        }
    }
    public class CompProperties_ArmorRocket : CompProperties
    {
        public EffecterDef flight;
        public CompProperties_ArmorRocket()
        {
            compClass = typeof(CompArmorRocket);
        }
    }

}