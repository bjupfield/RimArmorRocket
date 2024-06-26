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
using ArmorRocket;
using ArmorRacks.Things;
using ArmorRocket.ThingComps;
using System.IO;

namespace ArmorRocket
{
    //public class ArmorProjectile : Projectile
    //{
    //    static float xtoeat = 3.893f;
    //    static FieldInfo pawnPathNodes = typeof(PawnPath).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
    //    public override Vector3 DrawPos => base.DrawPos;

    //    Vector3 myPos;

    //    public override Vector3 ExactPosition => myPos;

    //    public override Material DrawMat => base.DrawMat;//add a function that takes the current orientation and checks if it has changed enough to draw differently for htis

    //    ThingWithComps bracelet;

    //    IntVec3 cellTarget;

    //    bool xyTraversal = false;

    //    List<Apparel> armors;

    //    float targetHeight;

    //    float xymultiplier;

    //    float xyAdder;

    //    float t;

    //    float ascensionTickMult;

    //    float previousMovementCalc;

    //    List<IntVec3> xyPath;

    //    float totalxyTraversal;//this is our total current xypath distance that we use for our multiplier and theta calc
    //    float totalxyTraversed = 0;//this is the total distance traveled in xy direction
    //    int ascensionTick;//total ticks needed in ascension
    //    int ascensioTickCount = 0;//current ascension tick
    //    int xyTickCount = 0;//current xy traversal tick

    //    public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
    //    {
    //        base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
    //        /*reminder of things that exist on base that we will use

    //        this.intendedTarget; => target
    //        this.origin; => origin
    //        this.ticksToImpact; =? tickstoimpact//going to use as total tickcount
    //        this.launcher; => armorstand
    //        this.intendedTarget.cell; => targetcell
    //        this.exactposition; => exactposition

    //         */
    //        bracelet = ((ArmorRocket.ArmorRocketThing)launcher).targetBracelet;
    //        //todo
    //        //set armorlist here
    //        //"despanw armors from stand

    //        t = 0;
    //        targetHeight = float.MaxValue;
    //        previousMovementCalc = 0;
    //        cellTarget = intendedTarget.Cell;//used to check in tick to see if pawns postion has changed
    //        xyPath = new List<IntVec3>();
    //        myPos = new Vector3(launcher.Position.x, launcher.Position.y, launcher.Position.z);

    //        flightCalc();

    //        xyTickCount = 0;
    //        ascensioTickCount = 0;
    //        totalxyTraversed = 0;
    //        printData();
    //    }
    //    public override void ExposeData()
    //    {
    //        base.ExposeData();
    //        //need to save location and all relevent data here
    //    }
    //    public override void Tick()
    //    {
    //        printData();

    //        //do not call base, we are completely overridding base
    //        if (ticksToImpact % 5 == 0)
    //            targetPositionUpdated();
    //        if (!xyTraversal)
    //        {
    //            ascension();
    //        }
    //        else
    //        {
    //            xyTraverse();
    //        }
    //        targetReached();

    //        ticksToImpact--;
    //        if (ticksToImpact <= -100) this.Destroy();

    //    }
    //    void targetPositionUpdated()
    //    {
    //        IntVec3 updated = cellTarget - this.intendedTarget.Cell;
    //        if (updated != IntVec3.Zero)//target has been updated 
    //        {
    //            Verse.Log.Warning("Position CHanged");
    //            if (xyTraversal)
    //            {
    //                Verse.Log.Warning("This is Occuring?");
    //                inFlightRecalc();
    //                return;
    //            }
    //            flightCalc();
    //            cellTarget = this.intendedTarget.Cell;
    //        }
    //        return;
    //    }
    //    void inFlightRecalc()
    //    {
    //        IntVec2 a = (cellTarget - ExactPosition.ToIntVec3()).ToIntVec2;
    //        IntVec2 b = (intendedTarget.Cell - ExactPosition.ToIntVec3()).ToIntVec2;

    //        ticksToImpact = 0;

    //        if (!(Mathf.Acos((a.x * b.x + b.z * a.z) / a.Magnitude * b.Magnitude) >= Mathf.PI / 2))
    //        {//the pawn hasnt reversed direction

    //            //find the point in the current xpath that is closest to the new target position obviously choose the first one thats distance to the target is less than the next one
    //            int i = 0;
    //            while(i < xyPath.Count - 1)
    //            {
    //                if ((xyPath[i] - intendedTarget.Cell).SqrMagnitude <= (xyPath[i + 1] - intendedTarget.Cell).SqrMagnitude)
    //                    break;
    //                i++;
    //            }
    //            foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
    //            Verse.Log.Warning("Achieved I:" + i);
    //            while(i < xyPath.Count - 1)
    //            {
    //                xyPath.Pop();
    //            }
    //            Verse.Log.Warning("This Path Has been choosen");
    //            foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
    //            xyPathFinder(xyPath.Last(), intendedTarget.Cell);
    //            Verse.Log.Warning("post reassignment");
    //            foreach (IntVec3 v in xyPath) Verse.Log.Warning(v.ToString());
    //            calculatethetickmultiplier(0);
    //        }
    //        else
    //        {
    //            xyPath = new List<IntVec3>();
    //            xyPathFinder(this.ExactPosition.ToIntVec3(), intendedTarget.Cell);
    //            t = 0;
    //            calculatethetickmultiplier(-previousMovementCalc);
    //        }

    //        cellTarget = this.intendedTarget.Cell;
    //        ticksToImpact += xyTickCount;
    //    }
    //    void flightCalc()
    //    {
    //        //this appears to be finished

    //        //determining if flight is under mountain or not

    //        totalxyTraversal = 0;

    //        xyPath = new List<IntVec3>();

    //        xyPathFinder(this.ExactPosition.ToIntVec3(), intendedTarget.Cell);

    //        calculatethetickmultiplier(0);
    //        calculateAscensionTick();
    //        ticksToImpact = xyTickCount + ascensionTick;

    //        //end of warning

    //    }
    //    void calculatethetickmultiplier(float add)
    //    {
    //        xyAdder = add;
    //        float b = totalxyTraversal - totalxyTraversed;
    //        xymultiplier = (b) / 250f;
    //        xyTickCount = (int)b * 8;
    //        //warning this is not the actual tick count... I just couldnt solve the equation x^3 * b / d + x * c = b - f...if you can plz tell the author... th
    //        //this should be a rough overestimate
    //    }
    //    void calculateAscensionTick()
    //    {
    //        ascensionTickMult = 4f / 80f; //10 takes 80 ticks, 1.333 secs
    //        targetHeight = 4;
    //        ascensionTick = Mathf.CeilToInt(targetHeight / ascensionTickMult);
    //    }
    //    void ascension()
    //    {
    //        myPos = ExactPosition + Vector3.up * ascensionTickMult;
    //        Verse.Log.Warning((Vector3.up * ascensionTickMult).ToString());
    //        if(ExactPosition.y >= targetHeight)
    //        {
    //            Verse.Log.Warning("Reached or Exceeded Target height: " + ExactPosition.ToString());
    //            xyTraversal = true;
    //        }
    //        ascensionTick++;
    //        return;
    //    }
    //    void xyTraverse()
    //    {
    //        t += 1f/60f;
    //        float toMove = Mathf.Min(Mathf.Pow(t, 1.5f) * xymultiplier + xyAdder, .25f);
    //        previousMovementCalc = toMove;
    //        totalxyTraversed += toMove;

    //        float xdistance = toMove;

    //        //Verse.Log.Warning("Calculated Xdistance is: " + xdistance + " || T: " + t + " || XyMult: " + xymultiplier);
    //        while (xdistance >= 0 && xyPath.Count > 1)
    //        {
    //            //Verse.Log.Warning("1");
    //            IntVec2 term1 = new IntVec2(xyPath[1].x, xyPath[1].z);
    //            xdistance -= new Vector2(ExactPosition.x - term1.x, ExactPosition.z - term1.z).magnitude;
    //            //Verse.Log.Warning(term1.ToString());
    //            if (xdistance >= 0)
    //            {
    //                myPos = new Vector3(term1.x, ExactPosition.y, term1.z);
    //                xyPath.Remove(xyPath[0]);
    //            }
    //            else
    //            {
    //                Vector3 distancechange = new Vector3(term1.x - ExactPosition.x , 0, term1.z - ExactPosition.z).normalized;
    //                myPos = new Vector3(term1.x + distancechange.x * xdistance, myPos.y, term1.z + distancechange.z * xdistance);
    //            }
    //            //Verse.Log.Warning("My pos After: " + myPos);
    //        }

    //        xyTickCount++;

    //    }
    //    void targetReached()
    //    {
    //        if ((ExactPosition.ToIntVec3() - intendedTarget.Cell).Magnitude < 1.5)
    //        {
    //            //actually put assign armor logic here
    //            Verse.Log.Warning("TargetReached ExactPositionDiff: " + (ExactPosition.ToIntVec3() - cellTarget));
    //            this.Destroy();
    //        }
    //    }
    //    void xyPathFinder(IntVec3 startCell, IntVec3 endCell)
    //    {
    //        int i;
    //        int count = xyPath.Count;

    //        if (this.Map.roofGrid.RoofAt(endCell.x, endCell.z)?.isThickRoof == true)
    //        {//thick roof
    //            TraverseParms doorPasser = TraverseParms.For(TraverseMode.PassDoors, Danger.Deadly, false, false, false);
    //            PawnPath underPath = this.Map.pathFinder.FindPath(endCell, startCell, doorPasser);
    //            xyPath.InsertRange(count, ((List<IntVec3>)pawnPathNodes.GetValue(underPath)).ListFullCopy());
    //            i = xyPath.Count - 1;
    //            while (this.Map.roofGrid.RoofAt(xyPath[i].x, xyPath[i].z)?.isThickRoof == true)
    //            {
    //                i--;
    //            }
    //            for (int j = count; j < i; j++)
    //            {
    //                xyPath.Remove(xyPath[count]);
    //            }
    //        }

    //        //stealing pathfinding logic
    //        TraverseParms parms = TraverseParms.For((TraverseMode)7, Danger.Deadly, false, false, false);
    //        PawnPath createdPath = this.Map.pathFinder.FindPath((xyPath.Count > count ? xyPath[count] : endCell), startCell, parms);

    //        //converting path
    //        xyPath.InsertRange(count, ((List<IntVec3>)pawnPathNodes.GetValue(createdPath)));

    //        i = 0;
    //        while (i < xyPath.Count - 2)
    //        {
    //            IntVec3 term1 = (xyPath[i] - xyPath[i + 1]);
    //            IntVec3 term2 = xyPath[i + 1] - xyPath[i + 2];
    //            if (term1.x / term1.Magnitude == term2.x / term2.Magnitude && term1.y / term1.Magnitude == term2.y / term2.Magnitude)
    //            {
    //                xyPath.Remove(xyPath[i + 1]);
    //            }
    //            else
    //            {
    //                i++;
    //            }
    //        }

    //        i = 0;
    //        while (i < xyPath.Count - 1)
    //        {
    //            IntVec3 term1 = xyPath[i] - xyPath[i + 1];
    //            totalxyTraversal += Mathf.Sqrt((term1.x * term1.x) + (term1.z * term1.z));
    //            i++;
    //        }
    //    }
    //    void printData()
    //    {
    //        //Verse.Log.Warning("Curr Tick: " + this.ticksToImpact + " || CurrPosition: " + this.ExactPosition + " || Target Location" + cellTarget + " || Curr Rotation: " + this.ExactRotation + " || ThingId" + this.ThingID + " || Curr Graphic: " + this.DefaultGraphic);
    //        //Verse.Log.Warning("xMultiplier: " + xymultiplier + " || ascesiontickmult: " + ascensionTickMult +  " || TargetHeight: " + targetHeight);
    //    }
    //}
    public class ArmorProjectile : Projectile
    {
        static readonly IntVec3[] surround = new IntVec3[4] { new IntVec3(1, 0, 0), new IntVec3(-1, 0, 0), new IntVec3(0, 0, 1), new IntVec3(0, 0, -1) };
        static node fake = new node();
        static int maxInt = 10000;
        static float speed = 10;
        static FieldInfo pawnPathNodes = typeof(PawnPath).GetField("nodes", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo doorOpen = typeof(Building_Door).GetField("openInt", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo doorHold = typeof(Building_Door).GetField("holdOpenInt", BindingFlags.NonPublic | BindingFlags.Instance);
        struct node
        {
            public int index;
            public int parent;
            public IntVec3 value;
        }
        struct cellNpath
        {
            public IntVec3 cell;
            public bool type;//true for any heavy roofs or any subsequents cells that have roofs after cellnpath with type = true
        }
        private node[] nodeGrid;
        RoofGrid roof => Map.roofGrid;
        public override Vector3 DrawPos => base.DrawPos;

        public override Vector3 ExactPosition => myPos;

        public override Quaternion ExactRotation => Quaternion.Lerp(Quaternion.LookRotation(slerpRot), Quaternion.LookRotation(rotVec), curveDist);

        public Vector3 rotVec = new Vector3();
        public Vector3 slerpRot;

        public override Material DrawMat => base.DrawMat;//add a function that takes the current orientation and checks if it has changed enough to draw differently for htis

        Vector3 myPos;

        Apparel bracelet;

        IntVec3 cellTarget;

        public List<Thing> launchedThings;

        List<cellNpath> path;

        int numPathNode;

        /**************Bezier Curve Variables****************/
        float curveDist;//from 0 =< t =< 1 
        int previousCurvePosition;//which side of our line our control point will go on
        float curveMult; //bezier curves 0 =< t =< 1, curveMult = 1 / (totaldistance / speed)
        static float curveM = 2;
        static float curveMin = .3f;
        static float curveMax = 20f;
        static float curveL = 1 / 4f;
        Vector3 curvePoint0;
        Vector3 curvePoint1;
        Vector3 curveControlPoint;

        private node constructNode(int index, int parent, IntVec3 cell)
        {
            node ret = new node();
            ret.index = index;
            ret.parent = parent;
            ret.value = cell;
            return ret;
        }
        private cellNpath constructCellNPath(IntVec3 cell, bool type)
        {
            cellNpath ret = new cellNpath();
            ret.cell = cell;
            ret.type = type;
            return ret;
        }
        private bool heavyRoof(int cellIndex)
        {
            return roof.Roofed(cellIndex) && roof.RoofAt(cellIndex).isThickRoof;
        }
        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire = false, Thing equipment = null, ThingDef targetCoverDef = null)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            /*reminder of things that exist on base that we will use

            this.intendedTarget; => target
            this.origin; => origin
            this.ticksToImpact; =? tickstoimpact//going to use as total tickcount
            this.launcher; => armorstand
            this.intendedTarget.cell; => cellTarget
            this.exactposition; => exactposition

             */

            bracelet = (Apparel)intendedTarget.Thing;
            ArmorRack launcher1 = (ArmorRack)launcher;
            if (launcher1 == null || bracelet == null)
            {
                this.Destroy();
                return;
            }


            /*******************Add Launched Armor************************/
            launchedThings = new List<Thing>();
            if (launcher1.GetStoredApparel().Count > 0)
            {
                foreach (Thing t in launcher1.GetStoredApparel())
                {
                    if (ApparelUtility.HasPartsToWear(bracelet.Wearer, t.def))
                    {
                        launchedThings.Add(t);
                    }
                }
            }
            if (launcher1.GetStoredWeapon() != null && !bracelet.Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                launchedThings.Add(launcher1.GetStoredWeapon());
            }
            foreach(Thing t in launchedThings)
            {
                launcher1.InnerContainer.Remove(t);
            }

            /*******************Path To Target**************************/
            cellTarget = intendedTarget.Cell;
            path = new List<cellNpath>();
            myPos = new Vector3(launcher.Position.x, launcher.Position.y, launcher.Position.z);
            numPathNode = 0;
            nodeGrid = new node[this.Map.Size.x * this.Map.Size.z];
            previousCurvePosition = (int)Verse.Rand.Value;
            rotVec = new Vector3(0, 0, 1);
            slerpRot = rotVec;
            flightCalc();


            //printData();
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            foreach(Thing t in launchedThings)
            {
                GenDrop.TryDropSpawn(t, this.ExactPosition.ToIntVec3(), Map, ThingPlaceMode.Near, out Thing resultant);
                if(resultant == null) 
                {
                    Verse.Log.Error("Unable To Drop Item: " + t);
                }
            }
            base.Destroy(mode);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //need to save location and all relevent data here
        }
        public override void Tick()
        {
            //do not call base, we are completely overridding base

            /********************Check if Target Position Has Updated Change Path if Necessary************************/
            if (ticksToImpact % 5 == 0)
            {
                targetPositionUpdated();
            }

            if(traverse())
            {
                return;
            }

            targetReached();

            ticksToImpact--;
            if (ticksToImpact <= -100) this.Destroy();

        }
        private void flightCalc(bool start = true)
        {;

            IntVec3 curCell;
            IntVec3 targetCell = cellTarget;
            IntVec3 startCell;
            int indexTarget = Map.cellIndices.CellToIndex(targetCell);
            int curIndex;

            Verse.Log.Warning("TargetIndex: " + indexTarget);

            int pathCountOg = path.Count;

            if(pathCountOg == 0)
            {
                curCell = Position;
                curIndex = Map.cellIndices.CellToIndex(curCell);
            }
            else
            {
                curCell = path.Last().cell;
                curIndex = Map.cellIndices.CellToIndex(curCell);
            }
            startCell = curCell;
            {
                PriorityQueue<int, float> nodeCheck = new PriorityQueue<int, float>(4000, default);

                node curNode = constructNode(curIndex, curIndex, curCell);
                if (nodeGrid == null)
                {
                    nodeGrid = new node[this.Map.Size.x * this.Map.Size.z];
                }
                nodeGrid[curNode.index] = curNode;

                nodeCheck.Enqueue(curNode.index, 0);
                int maxIteration = 0;
                int mapSizeX = Map.Size.x;
                int mapSizeZ = Map.Size.z;

                int parentint = 0;

                while (maxIteration < maxInt)
                {
                    if (!nodeCheck.TryDequeue(out curIndex, out float priority))
                    {
                        break;
                    }
                    curNode = nodeGrid[curIndex];

                    for (int i = 0; i < 4; i++, maxIteration++)
                    {
                        curCell = Map.cellIndices.IndexToCell(curIndex) + surround[i];
                        int fakeIndex = Map.cellIndices.CellToIndex(curCell);

                        if (curCell.x < 0 || curCell.x > mapSizeX)
                        {
                            continue;
                        }
                        if (curCell.z < 0 || curCell.z > mapSizeZ)
                        {
                            continue;
                        }


                        if (roof.RoofAt(fakeIndex) != null && roof.RoofAt(fakeIndex).isThickRoof)
                        {
                            continue;
                        }

                        if (fakeIndex == curNode.index)
                        {
                            parentint++;
                            continue;
                        }

                        if (!nodeGrid[fakeIndex].Equals(default(node)))
                        {
                            continue;
                        }

                        /**************Found New Cell***************/
                        node newNode = constructNode(fakeIndex, curNode.index, curCell);

                        nodeGrid[newNode.index] = newNode;

                        /**************Cell is Target***************/
                        if (fakeIndex == indexTarget)
                        {
                            curNode = newNode;
                            break;
                        }

                        nodeCheck.Enqueue(newNode.index, (targetCell - newNode.value).Magnitude);//enque show deference to up and right, just don't know if equal priority messes it up

                    }
                    if (curNode.index == indexTarget)
                    {
                        break;
                    }

                }
                int totalD = 0;
                if (curNode.index != indexTarget)
                {
                    /*************PathFinder Could Not Find path without Heavy Roof*******************/
                    //implement rimworlds base pathfinder here
                    Verse.Log.Warning("Iteration Count: " + maxIteration);
                    TraverseParms doorPasser = TraverseParms.For(TraverseMode.PassAllDestroyablePlayerOwnedThings, Danger.Deadly, false, false, false);
                    PawnPath underPath = this.Map.pathFinder.FindPath(cellTarget, startCell, doorPasser);
                    List<IntVec3> mountainPrePath = ((List<IntVec3>)pawnPathNodes.GetValue(underPath)).ListFullCopy();
                    List<cellNpath> mountainPath = new List<cellNpath>();
                    foreach(IntVec3 vec in mountainPrePath)
                    {
                        mountainPath.Add(constructCellNPath(vec, true));
                    }
                    int loc = 0;

                    for (int i = mountainPath.Count - 1; i >= 0; i--)
                    {
                        int index = Map.cellIndices.CellToIndex(mountainPath[i].cell);
                        if (!nodeGrid[index].Equals(default(node)) && !roof.Roofed(index))
                        {
                            mountainPath.RemoveRange(0, i - 1);
                            break;
                        }
                    }
                    while (loc < mountainPath.Count - 2)
                    {
                        if (!pathSimplifier(ref mountainPath, loc))
                        {
                            loc++;
                        }
                    }
                    for (int i = 0; i < mountainPath.Count; i++)
                    {
                        path.Insert(path.Count, mountainPath[i]);
                        if (i + 1 < mountainPath.Count)
                        {
                            totalD += Math.Max((int)Math.Abs((mountainPath[i].cell - mountainPath[i + 1].cell).Magnitude), 50);
                        }

                    }
                    curNode = nodeGrid[nodeGrid[Map.cellIndices.CellToIndex(mountainPath[0].cell)].parent];
                    underPath.Dispose();

                }
                /*************Implement Path Reducer Here, Remove All Nodes that parent and child node can be traversed in a straight line without intersecting a heavyroof cell*******************/
                while (curNode.index != curNode.parent)
                {
                    path.Insert(pathCountOg, constructCellNPath(curNode.value, false));
                    curNode = nodeGrid[curNode.parent];
                    totalD += Math.Max((int)(curNode.value - path[pathCountOg].cell).Magnitude, 50);
                    pathSimplifier(ref path, pathCountOg);
                }

                int fakeNumNodePathCount = numPathNode + (pathCountOg == 0 ? 0 : 1);
                while (fakeNumNodePathCount + 2 < path.Count)
                {
                    if (!pathSimplifier(ref path, fakeNumNodePathCount)) 
                    { 
                        fakeNumNodePathCount++;
                    }
                }

                ticksToImpact += (int)(totalD * 2f / speed);

                if (start)
                {
                    bezierCurveCalc(ExactPosition, path[1].cell.ToVector3());
                }
                else
                {
                    Verse.Log.Warning("Hey New Path.Count: " + path.Count);
                    //bezierCurveCalc(path[numPathNode].ToVector3(), path[numPathNode + 1].ToVector3());
                }
            }

        }
        private bool pathSimplifier(ref List<cellNpath> path, int loc = 0)
        {
            if (path.Count > 2 + loc)
            {//remove nodes from path if unnecessary


                //z intercepts in terms of x integers
                if (!path[loc + 1].type)
                {
                    IntVec3 childChild = path[loc + 2].cell;
                    IntVec3 distance = path[loc].cell - path[loc + 2].cell;
                    int x;
                    for (x = 1; x < Math.Abs(distance.x); x++)
                    {
                        int xIntPos = (x * (Math.Abs(distance.x) / distance.x)) + path[loc + 2].cell.x;
                        int zPos = (int)((float)x * ((float)distance.z / (float)Math.Abs(distance.x)) + (float)path[loc + 2].cell.z);//z at position xIntPos, rounded for grid
                                                                                                                                //check this this x grid and grid to left for overhead roof
                        int left = Map.cellIndices.CellToIndex(new IntVec3(xIntPos - 1, 0, zPos));
                        int right = Map.cellIndices.CellToIndex(new IntVec3(xIntPos, 0, zPos));
                        if (heavyRoof(left) || heavyRoof(right))
                        {
                            break;
                        }

                    }
                    if (x < Math.Abs(distance.x))
                    {
                        return false;
                    }
                    //x intercepts in terms of z integers
                    int z;
                    for (z = 1; z < Math.Abs(distance.z); z++)
                    {
                        int zIntPos = (z * (Math.Abs(distance.z) / distance.z)) + path[loc + 2].cell.z;
                        int xPos = (int)(((float)z * (float)distance.x / (float)Math.Abs(distance.z)) + (float)path[loc + 2].cell.x);

                        int up = Map.cellIndices.CellToIndex(new IntVec3(xPos, 0, zIntPos - 1));
                        int down = Map.cellIndices.CellToIndex(new IntVec3(xPos, 0, zIntPos));
                        if (heavyRoof(up) || heavyRoof(down))
                        {
                            break;
                        }
                    }
                    if (z < Math.Abs(distance.z))
                    {
                        return false;
                    }
                    path.RemoveAt(loc + 1);

                    return true;
                }
            }
            return false;
        }
        private void bezierCurveCalc(Vector3 p0, Vector3 p1, bool isLinear = false)
        {
            curvePoint0 = p0;
            curvePoint1 = p1;
            curveDist = 0;

            float lineMult = (previousCurvePosition % 4 > 1 ? curveL : 1 - curveL);

            float x = curvePoint1.x - curvePoint0.x;
            float z = curvePoint1.z - curvePoint0.z;
            Vector3 inverseVec = new Vector3(z * (z < 0 ? -1 : -1 ), 0, x).normalized * (x < 0 ? -1 : 1);
            float mult = (previousCurvePosition % 2 > 0 ? 1 : -1) * Math.Max(curveMin, Math.Min(new Vector2(x, z).magnitude / curveM , curveMax)) * (isLinear ? 0 : 1);
            //change mult and x and z to change position of curvecontrolpoint
            curveControlPoint = curvePoint0 + new Vector3(x, 0, z) * lineMult + (inverseVec * mult);
            curveMult = Math.Abs(1 / ((p1 - p0).magnitude / speed));
            previousCurvePosition += 1;
        }
        private void bezierCalc(float distance)
        {
            /*****************Position Calc*****************/
            float tS = distance * distance;
            float tReverseS = (1 - distance) * (1 - distance);
            float x1 = curvePoint0.x - curveControlPoint.x;
            float x2 = curvePoint1.x - curveControlPoint.x;
            float x = ((x1) * tReverseS) + ((x2) * tS) + curveControlPoint.x;
            float z1 = curvePoint0.z - curveControlPoint.z;
            float z2 = curvePoint1.z - curveControlPoint.z;
            float z = ((z1) * tReverseS) + ((z2) * tS) + curveControlPoint.z;
            myPos = new Vector3(x + .5f, 1, z + .5f);
            int cellIndex = Map.cellIndices.CellToIndex(myPos.ToIntVec3());

            /*****************Oreintation Calc*****************/
            float xOrien = 2 * (1 - distance) * x1 + 2 * distance * x2;
            float zOrein = 2 * (1 - distance) * z1 + 2 * distance * z2;
            rotVec = new Vector3(xOrien, 0, zOrein);


            if ((path[numPathNode].type || path[numPathNode].type) && roof.Roofed(cellIndex))
            {
                foreach (Thing t in Map.thingGrid.ThingsAt(myPos.ToIntVec3()))
                {
                    if (t.def.IsDoor)
                    {
                        Building_Door tDoor = t as Building_Door;
                        doorOpen.SetValue(tDoor, true);
                        doorHold.SetValue(tDoor, true);
                    }
                };
            }
        }
        private void targetPositionUpdated()
        {
            if(cellTarget != intendedTarget.Cell)
            {
                //is this all?
                cellTarget = intendedTarget.Cell;
                Vector3 endVec = (path.Last().cell - path[path.Count - 2].cell).ToVector3();
                Vector3 newEndVec = cellTarget.ToVector3() - path.Last().cell.ToVector3();
                if (Math.Acos((endVec.x * newEndVec.x + endVec.z * newEndVec.z) / (endVec.magnitude * newEndVec.magnitude)) > Math.PI / 2) {
                    path.Clear();
                    flightCalc();
                    /*************************Custom Turn Bezier Curve****************************/
                    /*Relevant Vars: I have had very little sleep
                     previousNextNode
                     previousCurNode
                     curveDist
                     previousCurvePosition - 1 :3
                     curveControlPoint
                     curveMult
                     */
                    Vector3 controllVec = curveControlPoint - curvePoint0;//orignal vector
                    Vector3 breakAwayPoint = ExactPosition;//current vector, point we are breaking away from in bezier curve /*c(subscript)o*/
                    Vector3 newVec = path[1].cell.ToVector3() - breakAwayPoint;//vector created from "breakaway" point in original bezier curve and new target point
                    Vector3 vt = newVec * (1 - curveDist);
                    Vector3 b = breakAwayPoint * curveDist;
                    float thetaO = Mathf.Acos(curveControlPoint.x / newVec.magnitude) + (curveControlPoint.z < 0 ? 180 : 0);
                    float thetaV = Mathf.Acos(newVec.x / newVec.magnitude);
                    float thetaCheck = thetaV + (newVec.z < 0 ? 180 : 0) - thetaO;//check to see which side vector should be on fof new vec line
                    float theta4 = thetaV + 90 - (thetaCheck >= 0 && thetaCheck <= 180 ? 180 : 0);
                    float tAbs = (.5f - Mathf.Abs(curveDist - .5f));
                    Vector3 a = new Vector3(Mathf.Cos(theta4) * tAbs, 0, Mathf.Sin(theta4) * tAbs);
                    float mult = Math.Max(curveMin, Math.Min(newVec.magnitude / curveM, curveMax));

                    curvePoint0 = breakAwayPoint;
                    curvePoint1 = path[1].cell.ToVector3();
                    curveControlPoint = breakAwayPoint + vt + ((b + a) * mult);
                    curveDist = 0;
                    curveMult = Math.Abs(1 / (newVec.magnitude / speed));
                }
                else
                {
                    flightCalc();
                }

            }
        }
        private bool traverse()
        {
            float distance = curveMult / 60;//ticks a second

            curveDist += distance;
            if(curveDist >= 1)
            {
                curveDist--;
                curveDist = curveDist / curveMult;
                numPathNode++;
                if(numPathNode < path.Count - 1) 
                {
                    bezierCurveCalc(path[numPathNode].cell.ToVector3(), path[numPathNode + 1].cell.ToVector3(), path[numPathNode].type);
                    curveDist = curveDist * curveMult;
                    slerpRot = rotVec;
                }
                else
                {
                    Verse.Log.Warning("Path end reahced?");
                    myPos = bracelet.DrawPos;
                    targetReached(true);
                    return true;
                }
            }
            bezierCalc(curveDist);
            return false;
        }
        private void targetReached(bool reached = false)
        {
            if((ExactPosition - bracelet.DrawPos).magnitude < .5f || reached)
            {
                Verse.Log.Warning("Path End not Reached!");
                if(bracelet.Wearer != null)
                {
                    while(launchedThings.Count > 0)
                    {
                        if (launchedThings.First().def.IsApparel)
                        {
                            if (ApparelUtility.HasPartsToWear(bracelet.Wearer, launchedThings.First().def))
                            {
                                if (bracelet.Wearer.apparel.WornApparel.Count > 0)
                                {
                                    List<Apparel> refe = new List<Apparel>(bracelet.Wearer.apparel.WornApparel);
                                    foreach (Apparel a in refe)
                                    {
                                        if (!ApparelUtility.CanWearTogether(a.def, launchedThings.First().def, bracelet.Wearer.RaceProps.body))
                                        {
                                            bracelet.Wearer.apparel.TryDrop(a);
                                        }
                                    }
                                }
                                bracelet.Wearer.apparel.Wear((Apparel)launchedThings.Pop());
                            }

                        }
                        else if (launchedThings.First().def.IsWeapon)
                        {
                            if(bracelet.Wearer.equipment.Primary != null)
                            {
                                bracelet.Wearer.equipment.TryDropEquipment(bracelet.Wearer.equipment.Primary, out ThingWithComps result, Position);
                            }
                            bracelet.Wearer.equipment.AddEquipment((ThingWithComps)launchedThings.Pop());
                        }
                        else
                        {
                            Verse.Log.Error("Launched Things Contains Non-Apparel/Weapon Item");
                        }
                    }
                }
                this.Destroy();
            }
        }
    }
}
