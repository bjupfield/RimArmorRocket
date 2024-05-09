﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.Noise;

namespace RimWorld
{
    public class ArmorProjectile : Projectile
    {
        static float xtoeat = 3.893f;
        static float ytoeat = 1.648f;
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

        Vector2 previousThetaMovementCalc;

        List<IntVec2> xyPath;

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
            previousThetaMovementCalc = Vector2.zero;
            cellTarget = bracelet.Position;//used to check in tick to see if pawns postion has changed
            xyPath = new List<IntVec2>();

            flightCalc();

            xyTickCount = 0;
            ascensioTickCount = 0;
            totalxyTraversed = 0;
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
            if(ticksToImpact % 5 == 0)
                targetPositionUpdated();
            if (xyTraversal)
            {
                ascension();
            }
            else
            {
                xyTraverse();
            }
            targetReached();

            this.ticksToImpact--;
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
            xyPath.Add(new IntVec2((int)this.ExactPosition.x, (int)this.ExactPosition.z));
            xyPath.Add(cellTarget.ToIntVec2);
            totalxyTraversal = (xyPath[1] - xyPath[0]).Magnitude;
            xymultiplier = totalxyTraversal / xtoeat;
            zmultiplier = xymultiplier;

            calculatethetickmultiplier();
            if(Position.z > targetHeight)
            {
                //recaculate zmultiplier
                zmultiplier = Position.z / ytoeat; 
            }
            else
            {
                calculateAscensionTick();
                ticksToImpact = xyTickCount + ascensionTick;
            }
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
            ascensionTickMult = Mathf.Min(15, Mathf.Max(5, ((totalxyTraversal / 2) / 125 * 15 ) / 60));
            targetHeight = xymultiplier * .5f;
            ascensionTick = Mathf.CeilToInt(targetHeight / ascensionTickMult);
        }
        void ascension()
        {
            myPos = ExactPosition + Vector3.up * ascensionTickMult;
            if(ExactPosition.z > targetHeight)
            {
                xyTraversal = true;
            }
            ascensionTick++;
            return;
        }
        void xyTraverse()
        {
            t += xyTickMultiplier;
            Vector2 toMove = new Vector2(Mathf.Pow(t, (float)Math.E), .5f - ((t * t) / 2) + Mathf.Sqrt(t / 10));
            totalxyTraversal -= toMove.x;
            totalxyTraversed += toMove.x;

            float xdistance = toMove.x;
            while(xdistance >= 0 && xypathInd < xyPath.Count)
            {
                xdistance -= new Vector2(ExactPosition.x - xyPath[xypathInd + 1].x, ExactPosition.z - xyPath[xypathInd + 1].z).magnitude;
                if (xdistance >= 0)
                {
                    myPos = new Vector3(xyPath[xypathInd + 1].x, xyPath[xypathInd + 1].z, ExactPosition.z);
                    xypathInd++;   
                }
                else
                {
                    Vector3 distancechange = new Vector3(ExactPosition.x - xyPath[xypathInd + 1].x, 0, ExactPosition.z - xyPath[xypathInd + 1].z).normalized;
                    myPos += distancechange * xdistance;
                }
            }
            myPos.y = -toMove.y;
            xyTickCount++;

        }
        void targetReached()
        {
            if ((ExactPosition.ToIntVec3() - cellTarget).Magnitude < 1.5)
            {
                //actually put assign armor logic here
                Verse.Log.Warning("TargetReached");
                this.Destroy();
            }
        }
        void printData()
        {
            Verse.Log.Warning("Curr Tick: " + this.ticksToImpact + " || CurrPosition: " + this.ExactPosition + " || Target Location" + cellTarget + " || Curr Rotation: " + this.ExactRotation + " || ThingId" + this.ThingID + " || Curr Graphic: " + this.DefaultGraphic);
        }
    }
}