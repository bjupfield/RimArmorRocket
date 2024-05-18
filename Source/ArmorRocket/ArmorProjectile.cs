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

        float xymultiplier;

        float xyAdder;

        float t;

        float ascensionTickMult;

        float previousMovementCalc;

        List<IntVec3> xyPath;

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
            previousMovementCalc = 0;
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

            ticksToImpact--;
            if (ticksToImpact <= -100) this.Destroy();

        }
        void targetPositionUpdated()
        {
            IntVec3 updated = cellTarget - this.intendedTarget.Cell;
            if (updated != IntVec3.Zero)//target has been updated 
            {
                Verse.Log.Warning("Position CHanged");
                if (xyTraversal)
                {
                    Verse.Log.Warning("This is Occuring?");
                    inFlightRecalc();
                    return;
                }
                flightCalc();
                cellTarget = this.intendedTarget.Cell;
            }
            return;
        }
        void inFlightRecalc()
        {
            IntVec2 a = (cellTarget - ExactPosition.ToIntVec3()).ToIntVec2;
            IntVec2 b = (intendedTarget.Cell - ExactPosition.ToIntVec3()).ToIntVec2;

            ticksToImpact = 0;

            if (!(Mathf.Acos((a.x * b.x + b.z * a.z) / a.Magnitude * b.Magnitude) >= Mathf.PI / 2))
            {//the pawn hasnt reversed direction

                //find the point in the current xpath that is closest to the new target position obviously choose the first one thats distance to the target is less than the next one
                int i = 0;
                while(i < xyPath.Count - 1)
                {
                    if ((xyPath[i] - intendedTarget.Cell).SqrMagnitude <= (xyPath[i + 1] - intendedTarget.Cell).SqrMagnitude)
                        break;
                    i++;
                }
                foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
                Verse.Log.Warning("Achieved I:" + i);
                while(i < xyPath.Count - 1)
                {
                    xyPath.Pop();
                }
                Verse.Log.Warning("This Path Has been choosen");
                foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
                xyPathFinder(xyPath.Last(), intendedTarget.Cell);
                Verse.Log.Warning("post reassignment");
                foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
                calculatethetickmultiplier(0);
            }
            else
            {
                xyPath = new List<IntVec3>();
                xyPathFinder(this.ExactPosition.ToIntVec3(), intendedTarget.Cell);
                t = 0;
                calculatethetickmultiplier(-previousMovementCalc);
            }

            cellTarget = this.intendedTarget.Cell;
            ticksToImpact += xyTickCount;
        }
        void flightCalc()
        {
            //this appears to be finished

            //determining if flight is under mountain or not

            totalxyTraversal = 0;

            xyPath = new List<IntVec3>();

            xyPathFinder(this.ExactPosition.ToIntVec3(), intendedTarget.Cell);

            calculatethetickmultiplier(0);
            calculateAscensionTick();
            ticksToImpact = xyTickCount + ascensionTick;

            //end of warning

        }
        void calculatethetickmultiplier(float add)
        {
            xyAdder = add;
            float b = totalxyTraversal - totalxyTraversed;
            xymultiplier = (b) / 250f;
            xyTickCount = (int)b * 8;
            //warning this is not the actual tick count... I just couldnt solve the equation x^3 * b / d + x * c = b - f...if you can plz tell the author... th
            //this should be a rough overestimate
        }
        void calculateAscensionTick()
        {
            ascensionTickMult = 4f / 80f; //10 takes 80 ticks, 1.333 secs
            targetHeight = 4;
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
            t += 1f/60f;
            float toMove = Mathf.Min(Mathf.Pow(t, 1.5f) * xymultiplier + xyAdder, .25f);
            previousMovementCalc = toMove;
            totalxyTraversed += toMove;

            float xdistance = toMove;

            //Verse.Log.Warning("Calculated Xdistance is: " + xdistance + " || T: " + t + " || XyMult: " + xymultiplier);
            while (xdistance >= 0 && xyPath.Count > 1)
            {
                //Verse.Log.Warning("1");
                IntVec2 term1 = new IntVec2(xyPath[1].x, xyPath[1].z);
                xdistance -= new Vector2(ExactPosition.x - term1.x, ExactPosition.z - term1.z).magnitude;
                //Verse.Log.Warning(term1.ToString());
                if (xdistance >= 0)
                {
                    myPos = new Vector3(term1.x, ExactPosition.y, term1.z);
                    xyPath.Remove(xyPath[0]);
                }
                else
                {
                    Vector3 distancechange = new Vector3(term1.x - ExactPosition.x , 0, term1.z - ExactPosition.z).normalized;
                    myPos = new Vector3(term1.x + distancechange.x * xdistance, myPos.y, term1.z + distancechange.z * xdistance);
                }
                //Verse.Log.Warning("My pos After: " + myPos);
            }
 
            xyTickCount++;

        }
        void targetReached()
        {
            if ((ExactPosition.ToIntVec3() - intendedTarget.Cell).Magnitude < 1.5)
            {
                //actually put assign armor logic here
                Verse.Log.Warning("TargetReached ExactPositionDiff: " + (ExactPosition.ToIntVec3() - cellTarget));
                this.Destroy();
            }
        }
        void xyPathFinder(IntVec3 startCell, IntVec3 endCell)
        {
            int i;
            int count = xyPath.Count;

            if (this.Map.roofGrid.RoofAt(endCell.x, endCell.z)?.isThickRoof == true)
            {//thick roof
                TraverseParms doorPasser = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false);
                PawnPath underPath = this.Map.pathFinder.FindPath(endCell, startCell, doorPasser);
                xyPath.InsertRange(count, ((List<IntVec3>)pawnPathNodes.GetValue(underPath)).ListFullCopy());
                i = xyPath.Count - 1;
                while (this.Map.roofGrid.RoofAt(xyPath[i].x, xyPath[i].z)?.isThickRoof == true)
                {
                    i--;
                }
                for (int j = count; j < i; j++)
                {
                    xyPath.Remove(xyPath[count]);
                }
            }

            //stealing pathfinding logic
            TraverseParms parms = TraverseParms.For((TraverseMode)7, Danger.Deadly, false, false, false);
            PawnPath createdPath = this.Map.pathFinder.FindPath((xyPath.Count > count ? xyPath[count] : endCell), startCell, parms);

            //converting path
            xyPath.InsertRange(count, ((List<IntVec3>)pawnPathNodes.GetValue(createdPath)));

            i = 0;
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

            i = 0;
            while (i < xyPath.Count - 1)
            {
                IntVec3 term1 = xyPath[i] - xyPath[i + 1];
                totalxyTraversal += Mathf.Sqrt((term1.x * term1.x) + (term1.z * term1.z));
                i++;
            }
        }
        void printData()
        {
            //Verse.Log.Warning("Curr Tick: " + this.ticksToImpact + " || CurrPosition: " + this.ExactPosition + " || Target Location" + cellTarget + " || Curr Rotation: " + this.ExactRotation + " || ThingId" + this.ThingID + " || Curr Graphic: " + this.DefaultGraphic);
            //Verse.Log.Warning("xMultiplier: " + xymultiplier + " || ascesiontickmult: " + ascensionTickMult +  " || TargetHeight: " + targetHeight);
        }
    }
}
