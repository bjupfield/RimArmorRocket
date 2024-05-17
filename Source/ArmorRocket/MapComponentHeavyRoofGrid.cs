using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorld
{
    public class MapComponentHeavyRoofGrid : CustomMapComponent
    {
        static FieldInfo innerArray = typeof(EdificeGrid).GetField("innerArray", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        PathingContext heavyRoof;

        BlueprintGrid blueprintGrid;
        public MapComponentHeavyRoofGrid(Map map)
        : base(map)
        {
            heavyRoof = new PathingContext(map, new PathGrid(map, true));
            blueprintGrid = null;
            this.map = map;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            heavyRoof.pathGrid.RecalculateAllPerceivedPathCosts();//this is really all we have to do lol
        }
        public int roofType(PathGrid path, IntVec3 c)//used in transpiler
        {
            //return 0 if not our pathgridder, return 1 if not a heavy roof, return 2 if a heavy roof
            if (path == heavyRoof.pathGrid)
            {
                if (map.roofGrid.RoofAt(c) != null && map.roofGrid.RoofAt(c).isThickRoof)
                {
                    return 2;
                }
                else return 1;
            }
            //if (map.roofGrid.RoofAt(c).isThickRoof)
            //    return 2;
            //else return 1;
            return 0;
        }
        public BlueprintGrid retrieveBlue()
        {
            return this.blueprintGrid;
        }

        public PathingContext retrieveContext()
        {
            return this.heavyRoof;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref heavyRoof, "heavyRoofMap", this);
            Scribe_Deep.Look(ref blueprintGrid, "fakeBlueprint", this);
        }
    }
}
