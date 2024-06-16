using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using ArmorRacks.Jobs;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace ArmorRocket
{
    public class JobDriver_StoreSwapArmor : JobDriver_BaseRackJob
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            foreach(Toil toil in base.MakeNewToils())
            {
                yield return toil;
            }
            ArmorRocketThing rocket = (ArmorRocketThing)this.TargetThingA;
            List<Apparel> equipedApparel = new List<Apparel>(this.pawn.apparel.WornApparel);
            List<Apparel> standApparel = new List<Apparel>(rocket.assignedArmor);
            if (standApparel.Count > 0)
            {
                Verse.Log.Warning("Stand");
                List<Apparel> dropped = new List<Apparel>();
                foreach (Apparel p in standApparel)
                {
                    if (ApparelUtility.HasPartsToWear(pawn, p.def) && pawn.apparel.CanWearWithoutDroppingAnything(p.def))
                    {
                        Toil toil1 = Toils_General.Wait(20);
                        toil1.WithProgressBarToilDelay(TargetIndex.A);
                        toil1.tickAction = delegate
                        {
                            pawn.rotationTracker.FaceTarget(p);
                        };
                        toil1.AddFinishAction(delegate
                        {
                            rocket.InnerContainer.Remove(p);
                            pawn.apparel.Wear(p);
                        });
                        yield return toil1;
                    }
                    else
                    {
                        Thing replace = ApparelUtility.GetApparelReplacedByNewApparel(pawn, p);
                        GenDrop.TryDropSpawn(replace, rocket.Position, rocket.Map, ThingPlaceMode.Near, out replace);
                        dropped.Add((Apparel)replace);
                    }
                }
                foreach(Apparel p in dropped)
                {
                    if (rocket.Accepts(p))
                    {
                        Toil toil2 = Toils_General.Wait(20);
                        toil2.WithProgressBarToilDelay(TargetIndex.A);
                        toil2.tickAction = delegate
                        {
                            pawn.rotationTracker.FaceTarget(p);
                        };
                        toil2.AddFinishAction(delegate
                        {
                            rocket.InnerContainer.TryAdd(p);
                        });
                        yield return toil2;
                    }
                }
            }
            else if(equipedApparel.Count > 0)
            {
                Verse.Log.Warning("Equip");
                if (rocket.HasComp<CompArmorRocket>())
                {
                    Verse.Log.Warning("comp exist");
                }
                else
                {
                    Verse.Log.Warning("Comp does not exist");
                }
                foreach(Apparel p in equipedApparel)
                {
                    Verse.Log.Warning("1");
                    if (rocket.Accepts(p))
                    {
                        Toil toil3 = Toils_General.Wait(50);
                        toil3.WithProgressBarToilDelay(TargetIndex.A);
                        toil3.tickAction = delegate
                        {
                            pawn.rotationTracker.FaceTarget(p);
                        };
                        toil3.AddFinishAction(delegate
                        {
                            pawn.apparel.Remove(p);
                            rocket.InnerContainer.TryAdd(p);
                        });
                        yield return toil3;
                    }
                }
            }
            else
            {
                pawn.jobs.curDriver.ReadyForNextToil();
            }
            

        }

    }
}
