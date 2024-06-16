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
            Thing pawnWeapon = this.pawn.equipment.Primary;
            Thing standWeapon = rocket.assignedWeapon;
            if (standApparel.Count > 0)
            {
                List<Apparel> dropped = new List<Apparel>();
                Thing droppedWeapon = null;
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
                        Toil toil1 = Toils_General.Wait(20);
                        toil1.WithProgressBarToilDelay(TargetIndex.A);
                        toil1.tickAction = delegate
                        {
                            pawn.rotationTracker.FaceTarget(p);
                        };
                        toil1.AddFinishAction(delegate
                        {
                            Thing replace = ApparelUtility.GetApparelReplacedByNewApparel(pawn, p);
                            this.pawn.apparel.WornApparel.Remove((Apparel)replace);
                            GenDrop.TryDropSpawn(replace, rocket.Position, rocket.Map, ThingPlaceMode.Near, out replace);
                            dropped.Add((Apparel)replace);
                            rocket.InnerContainer.Remove(p);
                            pawn.apparel.Wear(p);
                        });
                        yield return toil1;
                    }
                }
                if(standWeapon != null)
                {
                    Toil toil4 = Toils_General.Wait(20);
                    toil4.WithProgressBarToilDelay(TargetIndex.A);
                    toil4.tickAction = delegate
                    {
                        pawn.rotationTracker.FaceTarget(TargetThingA);
                    };
                    if (pawnWeapon != null)
                    {
                        toil4.AddFinishAction(delegate
                        {
                            droppedWeapon = pawnWeapon;
                            this.pawn.equipment.Remove((ThingWithComps)pawnWeapon);
                            GenDrop.TryDropSpawn(droppedWeapon, rocket.Position, rocket.Map, ThingPlaceMode.Near, out droppedWeapon);
                        });
                    }
                    toil4.AddFinishAction(delegate 
                    { 
                        rocket.InnerContainer.Remove(standWeapon);
                        pawn.equipment.AddEquipment((ThingWithComps)standWeapon);
                    });
                    yield return toil4;
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
                if(droppedWeapon != null)
                {
                    Toil toil5 = Toils_General.Wait(20);
                    toil5.WithProgressBarToilDelay(TargetIndex.A);
                    toil5.tickAction = delegate
                    {
                        pawn.rotationTracker.FaceTarget(droppedWeapon);
                    };
                    toil5.AddFinishAction(delegate
                    {
                        rocket.InnerContainer.TryAdd(droppedWeapon);
                    });
                    yield return toil5;
                }
            }
            else if(equipedApparel.Count > 0 || pawnWeapon != null)
            {
                foreach(Apparel p in equipedApparel)
                {
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
                if(pawnWeapon != null)
                {
                    Toil toil7 = Toils_General.Wait(50);
                    toil7.WithProgressBarToilDelay(TargetIndex.A);
                    toil7.tickAction = delegate
                    {
                        pawn.rotationTracker.FaceTarget(TargetThingA);
                    };
                    toil7.AddFinishAction(delegate
                    {
                        pawn.equipment.Remove((ThingWithComps)pawnWeapon);
                        rocket.InnerContainer.TryAdd(pawnWeapon);
                    });
                    yield return toil7;
                }
            }
            else
            {
                pawn.jobs.curDriver.ReadyForNextToil();
            }
            

        }

    }
}
