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
}
