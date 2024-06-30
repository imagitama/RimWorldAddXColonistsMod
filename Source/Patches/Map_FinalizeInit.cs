using HarmonyLib;
using System.Linq;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch("FinalizeInit")]
    public static class Map_FinalizeInit_Patch
    {
        public static void Postfix(Map __instance)
        {
            if (!WorldInterfaceOnGUI_Patch.isEnabled)
            {
                Logger.LogMessage($"Disabled");
                return;
            }
            if (!WorldInterfaceOnGUI_Patch.autoAssignBestSkills)
            {
                Logger.LogMessage($"Not auto assigning best skills");
                return;
            }

            foreach (var pawn in __instance.mapPawns.FreeColonists)
            {
                if (pawn.skills == null || !pawn.RaceProps.Humanlike || pawn.workSettings == null)
                    continue;

                var bestSkill = pawn.skills.skills
                    .OrderByDescending(s => s.Level)
                    .FirstOrDefault();

                if (bestSkill == null)
                    continue;

                bool isDoctor = false;
                bool isCook = false;

                // https://github.com/RimWorld-zh/RimWorld-Defs-Templates/blob/master/CoreDefsProcessed/WorkTypeDefs/WorkTypes.xml

                if (WorldInterfaceOnGUI_Patch.basicsHighestPrio)
                {
                    foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if ((workTypeDef.defName == "Firefighter" || workTypeDef.defName == "Patient" || workTypeDef.defName == "Flicker" || workTypeDef.defName == "Haul+" || workTypeDef.defName == "Rescue"))
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 1);
                        }
                    }
                }

                foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                {
                    if (workTypeDef.relevantSkills.Contains(bestSkill.def))
                    {
                        pawn.workSettings.SetPriority(workTypeDef, 1);

                        if (workTypeDef.defName == "Doctor")
                        {
                            isDoctor = true;
                        }

                        if (workTypeDef.defName == "Cooking")
                        {
                            isCook = true;
                        }

                        break;
                    }
                }

                if (isDoctor && WorldInterfaceOnGUI_Patch.doctorsFirst)
                {
                    foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (workTypeDef.defName == "Firefighter")
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 2);
                            break;
                        }
                    }
                }

                if (isCook && WorldInterfaceOnGUI_Patch.cooksFirst)
                {
                    foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (workTypeDef.defName != "Cooking" && pawn.workSettings.GetPriority(workTypeDef) == 1 && workTypeDef.defName != "Firefighting")
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 2);
                            break;
                        }
                    }
                }
            }
        }
    }
}
