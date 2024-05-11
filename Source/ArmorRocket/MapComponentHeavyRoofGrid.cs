using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace ArmorRocket
{
    public class MapComponentHeavyRoofGrid : CustomMapComponent
    {
        PathingContext heavyRoof;
        public MapComponentHeavyRoofGrid(Map map)
        : base(map)
        {
            heavyRoof = new PathingContext(map, new PathGrid(map, true));
            heavyRoof.pathGrid.RecalculateAllPerceivedPathCosts();//this is really all we have to do lol
        }
        public int roofType(PathGrid path, IntVec3 c)//used in transpiler
        {
            //return 0 if not our pathgridder, return 1 if not a heavy roof, return 2 if a heavy roof
            if (path == heavyRoof.pathGrid)
            {
                if (map.roofGrid.RoofAt(c).isThickRoof)
                    return 2;
                else return 1;
            }

            return 0;
        }
        //no exposedata function is used as it is not used in the pathgrid or pathcomponents, this item will be reloaded on every map load
    }
}
