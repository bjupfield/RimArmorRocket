using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;
using Verse.Sound;

namespace ArmorRocket
{
    public class JobDriver_ArmorRocket : JobDriver//copied from jobdriver_takeinventory
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 10, job.count, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            //path to armorrocket
            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                pawn.pather.StartPath(base.TargetThingA, PathEndMode.ClosestTouch);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return toil;
            //turn towards armorrocket and "link" the bracelet
            Toil toil2 = Toils_General.Wait(20);
            toil2.WithProgressBarToilDelay(TargetIndex.A);
            toil2.tickAction = delegate
            {
                pawn.rotationTracker.FaceTarget(base.TargetThingA);
            };
            toil2.AddFinishAction(delegate
            {
                Thing ArmorRocket = TargetA.Thing;
                if (ArmorRocket is ThingWithComps)
                {
                    ArmorRocketThing link = (ArmorRocketThing)ArmorRocket;
                    if (link == null)
                    {
                        pawn.jobs.curDriver.ReadyForNextToil();
                    }
                    else
                    {
                        RimWorld.Apparel bracelet = null;
                        foreach (RimWorld.Apparel clothing in pawn.apparel.WornApparel)
                        {
                            if (clothing.HasComp<CompTargetBracelet>()) bracelet = clothing;
                        }
                        if (bracelet != null)
                        {
                            toil2.PlaySoundAtEnd(SoundDefOf.EnergyShield_Reset);
                            bracelet.GetComp<CompTargetBracelet>().linkArmorRocket(ArmorRocket);
                            /******************************************** readd later
                            link.linkTargetBracelet(bracelet);
                            *******************************************/
                        }
                        else
                        {
                            pawn.jobs.curDriver.ReadyForNextToil();
                        }
                    }
                }
            });
            toil2.handlingFacing = true;
            toil2.PlaySoundAtEnd(SoundDefOf.EnergyShield_Reset);
            yield return toil2;
        }
    }

}
