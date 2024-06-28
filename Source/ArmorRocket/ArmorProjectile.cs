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
using ArmorRacks.Drawers;
using Verse.Sound;

namespace ArmorRocket
{
    public class ArmorProjectile : Projectile
    {
        static readonly IntVec3[] surround = new IntVec3[4] { new IntVec3(1, 0, 0), new IntVec3(-1, 0, 0), new IntVec3(0, 0, 1), new IntVec3(0, 0, -1) };
        static readonly Vector3 offsetVec = new Vector3() { x = .5f, y = 0f, z = .5f };
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

        public List<Thing> launchedThings;

        List<cellNpath> path;

        IntVec3 cellTarget;

        int numPathNode;

        public override Vector3 DrawPos => myPos;

        public override Vector3 ExactPosition => myPos;

        public override Quaternion ExactRotation => Quaternion.Lerp(Quaternion.LookRotation(slerpRot), Quaternion.LookRotation(rotVec), 1 - ((1 - curveDist) * (1 - curveDist) * (1 - curveDist)));
        //public override Quaternion ExactRotation => Quaternion.LookRotation(rotVec);
        //public override Quaternion ExactRotation => rotVec;

        Material drawMat;
        public override Material DrawMat => drawMat;
        /**********Orientation Variables***********/
        public Vector3 rotVec;
        public Vector3 slerpRot;
        Vector3 previousPos;
        static float alt = 13;//this is used to draw above everything

        Vector3 myPos;

        Apparel bracelet;

        EffecterDef flight;

        Effecter flighter;

        /************Starting "Animation" Variables***********/

        ArmorRackContentsDrawer drawer;
        BodyTypeDef bodyTypeDef;
        int animationTicks = 0;
        static float fakeAlt = (float)AltitudeLayer.Building * .3846154f;


        /**************Bezier Curve Variables****************/
        float curveDist;//from 0 =< t =< 1 
        float curveMult; //bezier curves 0 =< t =< 1, curveMult = 1 / (totaldistance / speed)
        static float curveM = 2;
        static float curveMin = .3f;
        static float curveMax = 20f;
        static float curveL = (1 / 6f);
        Vector3 curvePoint0;
        Vector3 curvePoint1;
        Vector3 curveControlPoint;
        bool direction;
        bool direction2;

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
            ArmorRack rack = (ArmorRack)launcher;
            if (rack == null || bracelet == null)
            {
                this.Destroy();
                return;
            }
            CompArmorRocket comp = rack.GetComp<CompArmorRocket>();

            flight = comp.flight;
            flighter = flight.Spawn();

            bodyTypeDef = rack.BodyTypeDef;
            drawer = new ArmorRackContentsDrawer(rack);
            drawer.IsApparelResolved = true;

            /*******************Add Launched Armor************************/
            launchedThings = new List<Thing>();
            if (rack.GetStoredApparel().Count > 0)
            {
                foreach (Thing t in rack.GetStoredApparel())
                {
                    if (ApparelUtility.HasPartsToWear(bracelet.Wearer, t.def))
                    {
                        launchedThings.Add(t);
                        if (ApparelGraphicRecordGetter.TryGetGraphicApparel((Apparel)t, bodyTypeDef, out ApparelGraphicRecord rec))
                        {
                            drawer.ApparelGraphics.Add(rec);
                        }
                    }
                }
            }
            if (rack.GetStoredWeapon() != null && !bracelet.Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                launchedThings.Add(rack.GetStoredWeapon());
            }
            foreach(Thing t in launchedThings)
            {
                rack.InnerContainer.Remove(t);
            }

            /*******************Path To Target**************************/
            cellTarget = intendedTarget.Cell;
            path = new List<cellNpath>();
            numPathNode = 0;
            rotVec = new Vector3();
            direction = Verse.Rand.Bool;
            direction2 = Verse.Rand.Bool;

            myPos = rack.DrawPos;
            myPos.y = alt;

            if (rack.Rotation == Rot4.South)
            {

            }
            else if (rack.Rotation == Rot4.East)
            {

            }
            else if (rack.Rotation == Rot4.West)
            {

            }
            else if (rack.Rotation == Rot4.North)
            {

            }

            slerpRot = rotVec;
            previousPos = this.ExactPosition;
            flightCalc();

            this.DrawColor = rack.DrawColor;
            drawMat = MaterialPool.MatFrom("Nutmeg/ArmorRockets_Projectile", ShaderDatabase.DefaultShader, this.DrawColor, 0);

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

            if (animationTicks < 60)
            {
                animationTicks++;
            }
            else
            {

                /********************Check if Target Position Has Updated Change Path if Necessary************************/
                if (ticksToImpact % 5 == 0)
                {
                    targetPositionUpdated();
                }

                if (traverse())
                {
                    return;
                }

                targetReached();

                ticksToImpact--;
                if (ticksToImpact <= -100) this.Destroy();

            }

        }
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {

            //base.DrawAt(drawLoc, flip);
            if(animationTicks < 60)
            {
                Vector3 fake = drawLoc;
                fake.y = fakeAlt;


                int neg = animationTicks % 4;
                int direction = animationTicks % 2;

                fake.x += (float)direction / 20 * (neg > 1 ? -1 : 1);

                SoundStarter.PlayOneShot(SoundDefOf.Artillery_ShellLoaded, this);

                if (drawer != null && drawer.ApparelGraphics.Count > 0)
                {
                    drawer.DrawAt(fake);
                }
                else
                {
                    drawer = new ArmorRackContentsDrawer((ArmorRack)launcher);
                }
            }
            else
            {
                base.DrawAt(drawLoc, flip);
            }


        }
        private void flightCalc(bool start = true)
        {;

            IntVec3 curCell;
            IntVec3 targetCell = cellTarget;
            IntVec3 startCell;
            int indexTarget = Map.cellIndices.CellToIndex(targetCell);
            int curIndex;

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
                    if (path.Count == 1)//theres probably better fix for this but Im lazy
                    {
                        bezierCurveCalc(ExactPosition, path[0].cell.ToVector3() + offsetVec);
                    }
                    else
                    {
                        bezierCurveCalc(ExactPosition, path[1].cell.ToVector3() + offsetVec);
                    }
                }
                else
                {
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

            float lineMult = (direction2 ? curveL : 1 - curveL);

            float x = curvePoint1.x - curvePoint0.x;
            float z = curvePoint1.z - curvePoint0.z;
            Vector3 inverseVec = new Vector3(z * (z < 0 ? -1 : -1 ), 0, x).normalized * (x < 0 ? -1 : 1);
            float mult =  Math.Max(curveMin, Math.Min(new Vector2(x, z).magnitude / curveM , curveMax)) * (isLinear ? 0 : 1) * (direction ? -1 : 1);
            //change mult and x and z to change position of curvecontrolpoint
            curveControlPoint = curvePoint0 + new Vector3(x, 0, z) * lineMult + (inverseVec * mult);
            curveMult = Math.Abs(1 / ((p1 - p0).magnitude / speed));
            direction = !direction;
            if (direction)
            {
                direction2 = !direction2;
            }
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
            myPos = new Vector3(x, alt, z);
            int cellIndex = Map.cellIndices.CellToIndex(myPos.ToIntVec3());

            /*****************Oreintation Calc*****************/
            rotVec = (myPos - previousPos) * 200;
            previousPos = myPos;


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
                    float thetaO = Mathf.Acos(curveControlPoint.x / newVec.magnitude) + (curveControlPoint.z < 0 ? (float)Math.PI : 0);
                    float thetaV = Mathf.Acos(newVec.x / newVec.magnitude);
                    float thetaCheck = thetaV + (newVec.z < 0 ? (float)Math.PI : 0) - thetaO;//check to see which side vector should be on fof new vec line
                    float theta4 = thetaV + (float)(Math.PI / 2) - (thetaCheck >= 0 && thetaCheck <= Math.PI ? (float)Math.PI : 0);
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
                    bezierCurveCalc(path[numPathNode].cell.ToVector3() + offsetVec, path[numPathNode + 1].cell.ToVector3() + offsetVec, path[numPathNode].type);
                    curveDist = curveDist * curveMult;
                    slerpRot = rotVec;
                }
                else
                {
                    myPos = bracelet.DrawPos;
                    myPos.y = alt;
                    targetReached(true);
                    return true;
                }
            }
            bezierCalc(curveDist);
            flighter.Trigger(this, TargetInfo.Invalid);
            return false;
        }
        private void targetReached(bool reached = false)
        {
            if((ExactPosition - bracelet.DrawPos).magnitude < .5f || reached)
            {
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
