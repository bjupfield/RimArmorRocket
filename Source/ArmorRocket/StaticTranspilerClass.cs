using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ArmorRocket
{
    public static class StaticTranspilerClass
    {

        public static JobDef JobLinkBracelet;
        public static void onLoad()
        {
            JobLinkBracelet = DefDatabase<JobDef>.AllDefsListForReading.Find(a =>
            {
                return a.defName == "ArmorRocket";
            });
            Verse.Log.Warning("Job is: " + JobLinkBracelet.ToString());
        }
        //Pawn pawn, IntVec3 clickCell, ref List<FloatMenuOption> opts
        public static void jobArmorRocketAssign(Pawn pawn, Vector3 clickCell, ref List<FloatMenuOption> opts)
        {
            Apparel bracelet = null;
            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (worn.HasComp<CompTargetBracelet>())
                {
                    bracelet = worn;
                }
            }
            if (bracelet == null)
            {
                return;
            }
            foreach (Thing thing40 in IntVec3.FromVector3(clickCell).GetThingList(pawn.Map))
            {
                //Verse.Log.Warning(thing40.ToString());
                if (thing40.HasComp<CompArmorRocket>())
                {
                    //Verse.Log.Warning("Check fired");
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Link Bracelet", delegate
                    {
                        thing40.SetForbidden(value: false, warnOnFail: false);
                        Verse.AI.Job job23 = JobMaker.MakeJob(JobLinkBracelet, thing40);
                        job23.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job23, Verse.AI.JobTag.Misc);

                    }, MenuOptionPriority.High), pawn, thing40));
                }
            }
        }
        public static void draftedRocketLaunch(Pawn pawn)
        {
            Apparel bracelet = null;
            foreach (Apparel worn in pawn.apparel.WornApparel)
            {
                if (worn.HasComp<CompTargetBracelet>())
                {
                    bracelet = worn;
                }
            }
            if (bracelet != null)
            {
                bracelet.GetComp<CompTargetBracelet>().notifyArmorRocket(pawn);
            }
        }
        public static int roofType(ref List<MapComponent> comps, PathGrid grid, IntVec3 c)
        {
            int? b = ((MapComponentHeavyRoofGrid)comps.Find(d => d.GetType() == typeof(MapComponentHeavyRoofGrid)))?.roofType(grid, c);
            if (b == null)
                return 0;
            else 
            {
                if(b != 0)
                Verse.Log.Warning("Num: "+ b);
                return (int)b; 
            }
        }
        public static BlueprintGrid retrieveBlue(ref List<MapComponent> comps)
        {
            Verse.Log.Warning("In retrieve blue");
            //the line immediatly below is what is failing
            foreach(MapComponent b in comps) 
            { 
                
            }
            return ((MapComponentHeavyRoofGrid)comps.Find(c => c.GetType() == typeof(MapComponentHeavyRoofGrid)))?.retrieveBlue();
        }
        public static EdificeGrid retrieveEdifice(ref List<MapComponent> comps)
        {
            Verse.Log.Warning("In retrieve edifice");
            return ((MapComponentHeavyRoofGrid)comps.Find(c => c.GetType() == typeof(MapComponentHeavyRoofGrid)))?.retrieveEdifice();
        }
        public static PathingContext retrieveHeavyRoof(ref List<MapComponent> comps)
        {
            Verse.Log.Warning("In retrieve heavroof");
            return ((MapComponentHeavyRoofGrid)comps.Find(c => c.GetType() == typeof(MapComponentHeavyRoofGrid)))?.retrieveContext();
        }
    }
}
