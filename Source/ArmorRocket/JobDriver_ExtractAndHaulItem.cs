using ArmorRacks.Jobs;
using ArmorRocket.ThingComps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace ArmorRocket
{
    public class JobDriver_ExtractAndHaulItem : JobDriver_BaseRackJob
    {
        bool passedToil1 = false;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(fail);
            return pawn.Reserve(TargetThingA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            foreach (Toil toil in base.MakeNewToils())
            {
                yield return toil;
            }
            ArmorRocketThing rocket = (ArmorRocketThing)this.TargetThingA;

            if (!fail())
            {

                Toil toil1 = Toils_General.Wait(20);
                toil1.AddFinishAction(delegate
                {
                    passedToil1 = true;
                    rocket.InnerContainer.assignAddRemove(TargetThingB);
                    rocket.InnerContainer.Remove(TargetThingB);
                    Thing thing = null;
                    GenDrop.TryDropSpawn(TargetThingB, rocket.Position, rocket.Map, ThingPlaceMode.Near, out thing);
                    TargetThingB = thing;
                });
                yield return toil1;


                Toil toil2 = Toils_Haul.StoreThingJob(TargetIndex.B);

                yield return toil2;
            }
        }
        private bool fail()
        {
            if(TargetThingB == null) 
            {
                Verse.Log.Warning("Failed");
                return true;
            }
            else
            {
                ArmorRocketThing rocket = (ArmorRocketThing)this.TargetThingA;
                if (!rocket.InnerContainer.InnerListForReading.Contains(TargetThingB) && !passedToil1)
                {
                    Verse.Log.Warning("failed");
                    return true;
                }
            }
            return false;
        }
    }
}
