using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using Verse.AI;
using System.Reflection;

namespace ArmorRocket
{
    [StaticConstructorOnStartup]
    [HarmonyDebug]
    public static class onLoad
    {
        static onLoad()
        {
            Harmony harmony = new Harmony("rimworld.mod.nutmeg.ArmorRocket");
            Harmony.DEBUG = true;
            harmony.PatchAll();
            StaticTranspilerClass.onLoad();
            //call static onload func here for any weird calls needed

        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap))]
    [HarmonyPatch("AddHumanlikeOrders")]
    public static class FloatMenuMakerMap_AddHumanLikeOrders_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
        {
            var lineList = new List<CodeInstruction>(lines);

            List<CodeInstruction> myInstructs = new List<CodeInstruction>();

            //load pawn onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_1, null));
            //load clickcell onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));
            //load opts onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarga, 2));

            //call jobArmorRocketAssign(Pawn pawn, IntVec3 clickCell, ref List<FloatMenuOption> opts)
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "jobArmorRocketAssign"));

            lineList.InsertRange(lineList.Count - 1, myInstructs);

            return lineList;
        }
    }
    [HarmonyPatch(typeof(Pawn_DraftController))]
    [HarmonyPatch("set_Drafted")]
    public static class PawnDraftController_setDrafted_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
        {
            var lineList = new List<CodeInstruction>(lines);

            int adjustPoint = 0;

            List<CodeInstruction> myInstructs = new List<CodeInstruction>();

            //load draftcontroller onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));

            //call ldfld pawn
            while (!lineList[adjustPoint].ToString().Contains("Pawn_DraftController::pawn") && adjustPoint < lineList.Count)
            {
                adjustPoint++;
            }

            myInstructs.Add(new CodeInstruction(lineList[adjustPoint].opcode, lineList[adjustPoint].operand));

            //call draftedRocketLaunch(Pawn pawn)
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "draftedRocketLaunch"));

            while (!lineList[adjustPoint].ToString().Contains("LordUtility") && adjustPoint < lineList.Count)
            {
                adjustPoint++;
            }
            adjustPoint -= 2;

            lineList.InsertRange(adjustPoint, myInstructs);

            return lineList;
        }
    }
    [HarmonyPatch(typeof(PathGrid))]
    [HarmonyPatch("CalculatedCostAt")]
    public static class PathGrid_CalculatedCostAt_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
        {
            var lineList = new List<CodeInstruction>(lines);

            List<CodeInstruction> myInstructs = new List<CodeInstruction>();
            Map mine = new Map();
            FieldInfo pathGridMap = typeof(PathGrid).GetField("map", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo mapCompField = typeof(Map).GetField("components", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //load map.components
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathgrid
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, pathGridMap));//load map
            myInstructs.Add(new CodeInstruction(OpCodes.Ldflda, mapCompField));//load component field

            //load pathgrid
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathgrid

            //load intvec3 c
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_1, null));//load intvec3 c

            //heavyRoofGridorNull
            //call roofType(ref List<MapComponent> comps, PathGrid grid, IntVec3 c)
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "roofType"));

            //stloc.0 lock the int into first local variable thats an int
            myInstructs.Add(new CodeInstruction(OpCodes.Stloc_0, null));//locked

            //load it back out
            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_0, null));//loaded

            //load 0 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_0, null));//load 0

            //check if equal to 0 jump to
            myInstructs.Add(new CodeInstruction(OpCodes.Beq, null));//compare
            int jf1 = myInstructs.Count - 1;

            //reload number
            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_0, null));//loaded

            //load 1 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_1, null));//load 1

            //check if equal to 1 jump to
            myInstructs.Add(new CodeInstruction(OpCodes.Beq, null));//compare
            int jf2 = myInstructs.Count - 1;

            //return 10000 if equal to two
            //load 10000 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4, 10000));//load 10000
            myInstructs.Add(new CodeInstruction(OpCodes.Ret, null));//return

            //return 1 if equalt to 1
            myInstructs.Add(new CodeInstruction(OpCodes.Ldloc_0, null));//load 1
            int jt2 = myInstructs.Count - 1;
            myInstructs.Add(new CodeInstruction(OpCodes.Ret, null));//return


            //add jump logic for if statements, jf1 jumps to jt1 which is start of normal code, jf2 jumps to jt2
            Label jt1l = il.DefineLabel();

            Label jt2l = il.DefineLabel();

            lineList[0].labels.Add(jt1l);
            myInstructs[jf1].operand = jt1l;

            myInstructs[jt2].labels.Add(jt2l);
            myInstructs[jf2].operand = jt2l;

            lineList.InsertRange(0, myInstructs);

            return lineList;
        }
    }
    [HarmonyPatch(typeof(PathFinder), "FindPath", new Type[] {typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning)})]
    public static class PathFinder_FindPath2_Patch
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> lines, ILGenerator il)
        {
            var lineList = new List<CodeInstruction>(lines);

            List<CodeInstruction> myInstructs = new List<CodeInstruction>();

            int adjustPoint = 0;

            //for adjusting pathingcontext assignment

            while (!(lineList[adjustPoint].ToString().Contains("ldarg.0") && lineList[adjustPoint + 1].ToString().Contains("ldarg.0") && lineList[adjustPoint + 2].ToString().Contains("map") && lineList[adjustPoint + 3].ToString().Contains("pathing")))
            {
                adjustPoint++;
            }

            FieldInfo traverseParmMode = typeof(TraverseParms).GetField("mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //load traverseParms.mode
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_3, null));//load traverseparms
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, traverseParmMode));//load mode

            //load 7 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_7, null));// 7

            //check if mode == 7 and perfom jump
            myInstructs.Add(new CodeInstruction(OpCodes.Bne_Un, null));//compare and jump
            int jf1 = myInstructs.Count - 1;

            FieldInfo pathFinderMap = typeof(PathFinder).GetField("map", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo mapCompField = typeof(Map).GetField("components", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //load this to store in later 
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder

            //load this.map.mapcomponents
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, pathFinderMap));//load map
            myInstructs.Add(new CodeInstruction(OpCodes.Ldflda, mapCompField));//load components

            //call retrieveHeavyRoof
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "retrieveHeavyRoof"));

            FieldInfo pathFinderPathingContext = typeof(PathFinder).GetField("pathingContext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //store heavyRoof into pathingcontext
            myInstructs.Add(new CodeInstruction(OpCodes.Stfld, pathFinderPathingContext));

            //jump past normal logic
            myInstructs.Add(new CodeInstruction(OpCodes.Br, null));
            int jf2 = myInstructs.Count - 1 ;

            //add jumps
            int jt1 = adjustPoint;
            int jt2 = adjustPoint;

            //find jt2
            while (!(lineList[jt2].ToString().Contains("ldarg.0") && lineList[jt2 + 1].ToString().Contains("ldarg.0") && lineList[jt2 + 2].ToString().Contains("pathingContext")))
            {
                jt2++;
            }

            Label jt1l = il.DefineLabel();

            lineList[jt1].labels.Add(jt1l);
            myInstructs[jf1].operand = jt1l;

            Label jt2l = il.DefineLabel();

            lineList[jt2].labels.Add(jt2l);
            myInstructs[jf2].operand = jt2l;

            lineList.InsertRange(adjustPoint, myInstructs);

            //for adjusting edifice grid assignment

            while (!(lineList[adjustPoint].ToString().Contains("ldarg.0") && lineList[adjustPoint + 1].ToString().Contains("ldarg.0") && lineList[adjustPoint + 2].ToString().Contains("map") && lineList[adjustPoint + 3].ToString().Contains("edifice")))
            {
                adjustPoint++;
            }

            myInstructs.Clear();

            //load traverseParms.mode
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_3, null));//load traverseparms
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, traverseParmMode));//load mode

            //load 7 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_7, null));// 7

            //check if mode == 7 and perfom jump
            myInstructs.Add(new CodeInstruction(OpCodes.Bne_Un, null));//compare and jump
            int jf3 = myInstructs.Count - 1;

            //load this to store in later 
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder

            //load this.map.mapcomponents
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, pathFinderMap));//load map
            myInstructs.Add(new CodeInstruction(OpCodes.Ldflda, mapCompField));//load components

            //call retrieveEdifice
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "retrieveEdifice"));

            FieldInfo pathFinderEdificeGrid = typeof(PathFinder).GetField("edificeGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //store edificegrid into pathingcontext
            myInstructs.Add(new CodeInstruction(OpCodes.Stfld, pathFinderEdificeGrid));

            //jump past normal logic
            myInstructs.Add(new CodeInstruction(OpCodes.Br, null));
            int jf4 = myInstructs.Count - 1;

            //add jumps
            int jt3 = adjustPoint;
            int jt4 = adjustPoint;

            while (!(lineList[jt4].ToString().Contains("ldarg.0") && lineList[jt4 + 1].ToString().Contains("ldarg.0") && lineList[jt4 + 2].ToString().Contains("map") && lineList[jt4 + 3].ToString().Contains("blueprint")))
            {
                jt4++;
            }

            Label jt3l = il.DefineLabel();
            Label jt4l = il.DefineLabel();

            lineList[jt3].labels.Add(jt3l);
            myInstructs[jf3].operand = jt3l;

            lineList[jt4].labels.Add(jt4l);
            myInstructs[jf4].operand = jt4l;

            lineList.InsertRange(adjustPoint, myInstructs);

            ////for adjusting blueprintgrid assignment

            adjustPoint = jt4;

            while (!(lineList[adjustPoint].ToString().Contains("ldarg.0") && lineList[adjustPoint + 1].ToString().Contains("ldarg.0") && lineList[adjustPoint + 2].ToString().Contains("map") && lineList[adjustPoint + 3].ToString().Contains("blueprint")))
            {
                adjustPoint++;
            }

            myInstructs.Clear();

            //load traverseParms.mode
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_3, null));//load traverseparms
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, traverseParmMode));//load mode

            //load 7 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_7, null));// 7

            //check if mode == 7 and perfom jump
            myInstructs.Add(new CodeInstruction(OpCodes.Bne_Un, null));//compare and jump
            int jf5 = myInstructs.Count - 1;

            //load this to store in later 
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder

            //load this.map.mapcomponents
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_0, null));//load pathfinder
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, pathFinderMap));//load map
            myInstructs.Add(new CodeInstruction(OpCodes.Ldflda, mapCompField));//load components

            //call retrieveBlue
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "retrieveBlue"));

            FieldInfo pathFinderBlieprintGrid = typeof(PathFinder).GetField("blueprintGrid", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            //store blueprintgrid into pathingcontext
            myInstructs.Add(new CodeInstruction(OpCodes.Stfld, pathFinderBlieprintGrid));

            //jump past normal logic
            myInstructs.Add(new CodeInstruction(OpCodes.Br, null));
            int jf6 = myInstructs.Count - 1;

            //add jumps

            int jt5 = adjustPoint;
            int jt6 = adjustPoint;

            while (!(lineList[jt6].ToString().Contains("ldarga.s") && lineList[jt6 + 1].ToString().Contains("Cell") && lineList[jt6 + 2].ToString().Contains("x")))
            {
                jt6++;
            }

            Label jt5l = il.DefineLabel();
            Label jt6l = il.DefineLabel();

            lineList[jt5].labels.Add(jt5l);
            myInstructs[jf5].operand = jt5l;

            lineList[jt6].labels.Add(jt6l);
            myInstructs[jf6].operand = jt6l;

            Verse.Log.Warning("Inserting at: " + adjustPoint);
            for (int i = adjustPoint - 3; i < adjustPoint + 4; i++)
            {
                Verse.Log.Warning(lineList[i].ToString());
            }

            lineList.InsertRange(adjustPoint, myInstructs);

            ////adjust walkablefast logic to always continue if traversparams = our custom params

            while (!(lineList[adjustPoint].ToString().Contains("ldloc.s 13")))
            {
                adjustPoint++;
            }

            myInstructs.Clear();

            //load traverseParms.mode
            myInstructs.Add(new CodeInstruction(OpCodes.Ldarg_3, null));//load traverseparms
            myInstructs.Add(new CodeInstruction(OpCodes.Ldfld, traverseParmMode));//load mode

            //load 7 onto stack
            myInstructs.Add(new CodeInstruction(OpCodes.Ldc_I4_7, null));// 7

            //check if mode == 7 and perfom jump
            myInstructs.Add(new CodeInstruction(OpCodes.Bne_Un, null));//compare and jump
            int jf7 = myInstructs.Count - 1;

            //jump to loop start if true
            myInstructs.Add(new CodeInstruction(OpCodes.Br_S, null));//compare and jump
            int jf8 = myInstructs.Count - 1;

            //add jumps
            int jt7 = adjustPoint;
            int jt8 = adjustPoint;

            while (!lineList[jt8].ToString().Contains("br.s"))
            {
                jt8++;
            }

            Label jt7l = il.DefineLabel();
            Label jt8l = (Label)lineList[jt8].operand;

            lineList[jt7].labels.Add(jt7l);
            myInstructs[jf7].operand = jt7l;

            myInstructs[jf8].operand = jt8l;

            lineList.InsertRange(adjustPoint, myInstructs);

            //this is the testing for why its returning no path found without any error logs

            adjustPoint = 3;

            while (!(lineList[adjustPoint - 3].ToString().Contains("ldarg") && lineList[adjustPoint - 2].ToString().Contains("EndSample") && lineList[adjustPoint - 1].ToString().Contains("get_NotFound") && lineList[adjustPoint].ToString().Contains("ret")))
            {
                adjustPoint++;
            }

            myInstructs.Clear();
            
            //adding 
            myInstructs.Add(CodeInstruction.Call(typeof(StaticTranspilerClass), "logHere"));

            return lineList;
        }
    }
}
