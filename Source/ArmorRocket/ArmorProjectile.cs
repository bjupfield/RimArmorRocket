using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;

namespace ArmorRocket
{
    public class ArmorProjectile : Projectile
    {
        public override Vector3 DrawPos => base.DrawPos;

        public override Vector3 ExactPosition => base.ExactPosition;

        public override Material DrawMat => base.DrawMat;//add a function that takes the current orientation and checks if it has changed enough to draw differently for htis

        ThingWithComps bracelet;

        IntVec3 cellTarget;

        bool xyTraversal = false;

        List<Apparel> armors;

        float targetHeight;

        float zmultiplier;

        float xymultiplier;

        float theta = (float)((Math.PI * 3) / 2);//3pi/2-2pi

        float thetaTickMultiplier;

        Vector2 previousThetaMovementCalc;

        List<Vector2> xyPath;

        int xypathInd = 0;

        float totalxyTraversal;//this is our total current xypath distance that we use for our multiplier and theta calc
        float totalxyTraversed = 0;//this is the total distance traveled in xy direction
        int ascensionTick;//total ticks needed in ascension
        int ascensioTickCount = 0;//current ascension tick
        int xyTickCount = 0;//current xy traversal tick

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            /*reminder of things that exist on base that we will use

            this.intendedTarget; => target
            this.origin; => origin
            this.ticksToImpact; =? tickstoimpact//going to use as total tickcount
            this.launcher; => armorstand
            this.intendedTarget.cell; => targetcell
            this.exactposition; => exactposition

             */
            bracelet = ((ThingWithComps)launcher).GetComp<CompArmorRocket>().targetBracelet;
            //todo
            //set armorlist here
            //"despanw armors from stand

            previousThetaMovementCalc = Vector2.zero;
            cellTarget = this.intendedTarget.Cell;//used to check in tick to see if pawns postion has changed

            flightCalc();

        }
        public override void ExposeData()
        {
            base.ExposeData();
            //need to save location and all relevent data here
        }
        public override void Tick()
        {
            //do not call base, we are completely overridding base

            this.ticksToImpact++;
        }
        void targetPositionUpdated()
        {
            IntVec3 updated = cellTarget - this.intendedTarget.Cell;
            if (updated != IntVec3.Zero)//target has been updated 
            { 
                if (xyTraversal)
                {
                    inFlightRecalc();
                    return;
                }
                flightCalc();
            }
            return;
        }
        void inFlightRecalc()
        {
            //todo recalc when in xytraversal
        }
        void flightCalc()
        {
            //todo baseflightcalc
        }
    }
}
