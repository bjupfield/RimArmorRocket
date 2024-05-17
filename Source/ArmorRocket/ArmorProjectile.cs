using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.Noise;
using System.Reflection;

namespace RimWorld
{
    public class ArmorProjectile : Projectile
    {
        static float xtoeat = 3.893f;
        static float ytoeat = 1.648f;
        static Type pawnPathType = typeof(PawnPath);
        static FieldInfo pawnPathNodes = typeof(PawnPath).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
        public override Vector3 DrawPos => base.DrawPos;

        Vector3 myPos;

        public override Vector3 ExactPosition => myPos;

        public override Material DrawMat => base.DrawMat;//add a function that takes the current orientation and checks if it has changed enough to draw differently for htis

        ThingWithComps bracelet;

        IntVec3 cellTarget;

        bool xyTraversal = false;

        List<Apparel> armors;

        float targetHeight;

        float zmultiplier;

        float xymultiplier;

        float t;

        float xyTickMultiplier;

        float ascensionTickMult;

        Vector2 previousMovementCalc;

        List<IntVec3> xyPath;

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

            t = 0;
            targetHeight = float.MaxValue;
            previousMovementCalc = Vector2.zero;
            cellTarget = intendedTarget.Cell;//used to check in tick to see if pawns postion has changed
            xyPath = new List<IntVec3>();
            myPos = new Vector3(launcher.Position.x, launcher.Position.y, launcher.Position.z);

            flightCalc();

            xyTickCount = 0;
            ascensioTickCount = 0;
            totalxyTraversed = 0;
            printData();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //need to save location and all relevent data here
        }
        public override void Tick()
        {
            printData();
            this.ticksToImpact--;
            if (ticksToImpact <= -150)
            {
                this.Destroy();
            }
            //do not call base, we are completely overridding base
            if (ticksToImpact % 5 == 0)
                targetPositionUpdated();
            if (!xyTraversal)
            {
                ascension();
            }
            else
            {
                xyTraverse();
            }
            targetReached();

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
            //warning this is not finished but we are just using it as a test

            //stealing pathfinding logic
            TraverseParms parms = TraverseParms.For((TraverseMode)7, Danger.Deadly, false, false, false);
            PawnPath createdPath = this.Map.pathFinder.FindPath(this.intendedTarget.Cell, this.ExactPosition.ToIntVec3(), parms);
            
            //converting path
            xyPath = ((List<IntVec3>)pawnPathNodes.GetValue(createdPath)).ListFullCopy();

            int i = 0 ;
            while (i < xyPath.Count - 2)
            {
                IntVec3 term1 = (xyPath[i] - xyPath[i + 1]);
                IntVec3 term2 = xyPath[i + 1] - xyPath[i + 2];
                if (term1.x / term1.Magnitude == term2.x / term2.Magnitude && term1.y / term1.Magnitude == term2.y / term2.Magnitude)
                {
                    xyPath.Remove(xyPath[i + 1]);
                }
                else
                {
                    i++;
                }
            }

            totalxyTraversal = 0;
            i = 0;
            while (i < xyPath.Count - 1)
            {
                IntVec3 term1 = xyPath[i] - xyPath[i+1];
                totalxyTraversal += Mathf.Sqrt((term1.x * term1.x) + (term1.z * term1.z));
                i++;
            }
            createdPath.Dispose();
            xymultiplier = totalxyTraversal / xtoeat;
            zmultiplier = xymultiplier;

            calculatethetickmultiplier();
            if (ExactPosition.z > targetHeight)
            {
                //recaculate zmultiplier
                zmultiplier = Position.z / ytoeat;
            }
            else
            {
                calculateAscensionTick();
                if(ticksToImpact == 0)
                    ticksToImpact = xyTickCount + ascensionTick;
            }
            Verse.Log.Warning("TotalCost: " + totalxyTraversal);
            foreach (IntVec3 x in xyPath)
            {
                Verse.Log.Warning(x.ToString());
            }
            Verse.Log.Warning("Hello: " + intendedTarget.Cell);
            //end of warning

        }
        void calculatethetickmultiplier()
        {
            float xyminMultiplier = (15 / xymultiplier) / 60;//setting max speed to 15 blocks persec
            float xymaxMultiplier = (5 / xymultiplier) / 60;//setting min speed to 5 blocks persec
            xyTickMultiplier = Mathf.Min(xymaxMultiplier, Mathf.Max(xyminMultiplier, (totalxyTraversal / 250 * 15) / 60));
            xyTickCount = Mathf.CeilToInt(ytoeat / xyTickMultiplier);
            //this will need to be redone latter
        }
        void calculateAscensionTick()
        {
            ascensionTickMult = Mathf.Min(15, Mathf.Max(5, ((totalxyTraversal / 2) / 125 * 15 ))) / 60;
            targetHeight = xymultiplier * .5f;
            ascensionTick = Mathf.CeilToInt(targetHeight / ascensionTickMult);
        }
        void ascension()
        {
            myPos = ExactPosition + Vector3.up * ascensionTickMult;
            Verse.Log.Warning((Vector3.up * ascensionTickMult).ToString());
            if(ExactPosition.y >= targetHeight)
            {
                Verse.Log.Warning("Reached or Exceeded Target height: " + ExactPosition.ToString());
                xyTraversal = true;
            }
            ascensionTick++;
            return;
        }
        void xyTraverse()
        {
            t += xyTickMultiplier;
            Vector2 toMove = new Vector2(Mathf.Pow(t, (float)Math.E), .5f - ((t * t) / 2) + Mathf.Sqrt(t / 10)) - previousMovementCalc;
            previousMovementCalc += toMove;
            totalxyTraversal -= toMove.x;
            totalxyTraversed += toMove.x;

            float xdistance = toMove.x;
            Verse.Log.Warning("Moving: " + xdistance + " Y Movement: " + toMove.y);
            Verse.Log.Warning("XYPathInd: " + xypathInd + " || XyPathCount: " + xyPath.Count);
            while (xdistance >= 0 && xypathInd < xyPath.Count)
            {
                Verse.Log.Warning("1");
                IntVec2 term1 = new IntVec2(xyPath[xypathInd + 1].x, xyPath[xypathInd + 1].z);
                xdistance -= new Vector2(ExactPosition.x - term1.x, ExactPosition.z - term1.z).magnitude;
                Verse.Log.Warning(term1.ToString());
                Verse.Log.Warning("Calculated Xdistance is: " + xdistance);
                if (xdistance >= 0)
                {
                    myPos = new Vector3(term1.x, ExactPosition.y, term1.z);
                    xypathInd++;
                }
                else
                {
                    Vector3 distancechange = new Vector3(term1.x - ExactPosition.x , 0, term1.z - ExactPosition.z).normalized;
                    myPos = new Vector3(term1.x + distancechange.x * xdistance, myPos.y, term1.z + distancechange.z * xdistance);
                }
                //Verse.Log.Warning("My pos After: " + myPos);
            }
            myPos.y = Mathf.Max(.5f, myPos.y + toMove.y);
 
            xyTickCount++;

        }
        void targetReached()
        {
            if ((ExactPosition.ToIntVec3() - cellTarget).Magnitude < 1.5)
            {
                //actually put assign armor logic here
                Verse.Log.Warning("TargetReached ExactPositionDiff: " + (ExactPosition.ToIntVec3() - cellTarget));
                this.Destroy();
            }
        }
        void printData()
        {
            Verse.Log.Warning("Curr Tick: " + this.ticksToImpact + " || CurrPosition: " + this.ExactPosition + " || Target Location" + cellTarget + " || Curr Rotation: " + this.ExactRotation + " || ThingId" + this.ThingID + " || Curr Graphic: " + this.DefaultGraphic);
            Verse.Log.Warning("ZMultiplier: " + zmultiplier + " || xMultiplier: " + xymultiplier + " || ascesiontickmult: " + ascensionTickMult + " || XyMult: " + xyTickMultiplier + " || TargetHeight: " + targetHeight);
        }
    }
}
