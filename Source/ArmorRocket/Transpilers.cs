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
}
