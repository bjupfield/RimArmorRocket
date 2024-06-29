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
            List<Apparel> standApparel = new List<Apparel>(rocket.GetStoredApparel());
            Thing pawnWeapon = this.pawn.equipment.Primary;
            Thing standWeapon = rocket.assignedWeapon;
            if (standApparel.Count > 0 || standWeapon != null)
            {
                List<Thing> dropped = new List<Thing>();
                foreach (Apparel p in standApparel)
                {
                    if (ApparelUtility.HasPartsToWear(pawn, p.def))
                    {
                        if (pawn.apparel.CanWearWithoutDroppingAnything(p.def))
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
                            Thing replace = ApparelUtility.GetApparelReplacedByNewApparel(pawn, p);
                            toil1.AddFinishAction(delegate
                            {
                                replace = ApparelUtility.GetApparelReplacedByNewApparel(pawn, p);
                                
                                GenDrop.TryDropSpawn(replace, rocket.Position, rocket.Map, ThingPlaceMode.Near, out replace);
                                
                                rocket.InnerContainer.Remove(p);
                                pawn.apparel.Wear(p);
                            });
                            dropped.Add(replace);
                            yield return toil1;
                        }
                    }
                }


                if (standWeapon != null)
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
                            this.pawn.equipment.Remove((ThingWithComps)pawnWeapon);
                            GenDrop.TryDropSpawn(pawnWeapon, rocket.Position, rocket.Map, ThingPlaceMode.Near, out pawnWeapon);
                        });
                        dropped.Add(pawnWeapon);
                    }
                    toil4.AddFinishAction(delegate 
                    {
                        rocket.InnerContainer.Remove(standWeapon);
                        pawn.equipment.AddEquipment((ThingWithComps)standWeapon);
                    });
                    yield return toil4;
                }


                foreach (Thing p in dropped)
                {
                    if (rocket.fakeAccepts(p))
                    {
                        Toil toil2 = Toils_General.Wait(20);
                        toil2.WithProgressBarToilDelay(TargetIndex.A);
                        toil2.tickAction = delegate
                        {
                            pawn.rotationTracker.FaceTarget(p);
                        };
                        toil2.AddFinishAction(delegate
                        {
                            p.DeSpawn();
                            rocket.InnerContainer.TryAdd(p);
                        });
                        yield return toil2;
                    }
                }
            }


            else if(equipedApparel.Count > 0 || pawnWeapon != null)
            {
                foreach(Apparel p in equipedApparel)
                {
                    if (rocket.fakeAccepts(p))
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
