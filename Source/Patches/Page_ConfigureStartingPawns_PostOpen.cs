using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "PostOpen")]
    public static class Page_ConfigureStartingPawns_PostOpen_Patch
    {
        public static void Postfix()
        {
            if (!WorldInterfaceOnGUI_Patch.isEnabled)
            {
                Logger.LogMessage($"Disabled");
                return;
            }

            int desiredColonistCount = WorldInterfaceOnGUI_Patch.colonistCount;

            Logger.LogMessage($"Adding colonists: {desiredColonistCount}");

            if (desiredColonistCount == 0)
            {
                Logger.LogMessage("It is 0, ignoring");
                return;
            }

            GameInitData gameInitData = Find.GameInitData;

            gameInitData.startingPawnCount = desiredColonistCount;
            gameInitData.startingAndOptionalPawns = new List<Pawn>();

            for (int i = 0; i < WorldInterfaceOnGUI_Patch.colonistCount; i++)
            {
                Pawn pawn = StartingPawnUtility.NewGeneratedStartingPawn();
                gameInitData.startingAndOptionalPawns.Add(pawn);
            }

            Logger.LogMessage("Finished adding colonists");
        }
    }
}
