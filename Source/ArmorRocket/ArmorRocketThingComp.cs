using ArmorRacks.Things;
using ArmorRocket.Defs;
using ArmorRocket.ThingDefs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArmorRocket.ThingComps
{
    public class CompArmorRocket : ThingComp
    {
        public List<Thing> assignedThings;
        Apparel targetBracelet;
        Pawn target;
        CompRefuelable fuel;
        CompPowerTrader power;
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            assignedThings = new List<Thing>();
            fuel = parent.GetComp<CompRefuelable>();
            power = parent.GetComp<CompPowerTrader>();
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
            }) != null)
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
        }
        public void launchArmor()
        {
            ArmorRack rack = (ArmorRack)parent;
            if (rack != null)
            {
                ArmorProjectile launchThis = (ArmorProjectile)GenSpawn.Spawn(ArmorRocketThingDefOf.ArmorRocketProjectile, this.parent.Position, this.parent.Map);
                {
                    //insert all launch relevant assignments like armor...
                    if (rack.GetStoredApparel().Count > 0)
                    {
                        assignedThings.AddRange(rack.GetStoredApparel());
                    }
                    if(rack.GetStoredWeapon() != null)
                    {
                        assignedThings.Add(rack.GetStoredWeapon());
                    }
                }


                LocalTargetInfo d = new LocalTargetInfo(targetBracelet);
                launchThis.Launch(this.parent, parent.DrawPos, this.target.Position, d, new ProjectileHitFlags());
            }
        }
        public void braceletDestroyed()
        {
            targetBracelet = null;
            target = null;
        }
        public bool canLaunch()
        {
            if (fuel.IsFull)
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
            if (assignedThings.Count > 0)
            {
                foreach (Thing b in assignedThings)
                {
                    if(targetBracelet.Wearer.apparel.Contains(b) || targetBracelet.Wearer.equipment.Contains(b)) 
                    {
                        return true;
                    }
                }
            }
            return false;
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