using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                        // TODO: Find inbuilt type for patient/doctor/cook (not inside WorkTypeDefOf)
                        if ((workTypeDef == WorkTypeDefOf.Firefighter || workTypeDef.defName == "Patient" || workTypeDef.defName == "Flicker" || workTypeDef.defName == "HaulUrgentlyDesignation" || workTypeDef.defName == "DoctorRescue"))
                        {
                            if (pawn.health.DisabledWorkTypes.Contains(workTypeDef))
                            {
                                break;
                            }
                            
                            pawn.workSettings.SetPriority(workTypeDef, 1);
                        }
                    }
                }

                var relevantWorkTypes = new List<WorkTypeDef>();

                foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                {
                    if (workTypeDef.relevantSkills.Contains(bestSkill.def) && !pawn.health.DisabledWorkTypes.Contains(workTypeDef))
                    {
                        relevantWorkTypes.Add(workTypeDef);
                    }
                }

                // resolve issue where pawns will never craft because another work is always first
                if (relevantWorkTypes.Count > 0)
                {
                    var random = new System.Random();
                    var selectedWorkType = relevantWorkTypes[random.Next(relevantWorkTypes.Count)];
                    pawn.workSettings.SetPriority(selectedWorkType, 1);

                    if (selectedWorkType.defName == "Doctor")
                    {
                        isDoctor = true;
                    }

                    if (selectedWorkType.defName == "Cooking")
                    {
                        isCook = true;
                    }
                }

                if (isDoctor && WorldInterfaceOnGUI_Patch.doctorBeforeFirefighter)
                {
                    foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (workTypeDef == WorkTypeDefOf.Firefighter && !pawn.health.DisabledWorkTypes.Contains(workTypeDef))
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 2);
                            break;
                        }
                    }
                }

                // food is issue number 1 for 100+ colonies
                if (isCook && WorldInterfaceOnGUI_Patch.cookBeforeFirefighter)
                {
                    foreach (var workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                    {
                        if (workTypeDef.defName != "Cooking" && pawn.workSettings.GetPriority(workTypeDef) == 1 && workTypeDef == WorkTypeDefOf.Firefighter && !pawn.health.DisabledWorkTypes.Contains(workTypeDef))
                        {
                            pawn.workSettings.SetPriority(workTypeDef, 2);
                            break;
                        }
                    }
                }

                // less micro for massive battles
                if (WorldInterfaceOnGUI_Patch.attackBackInsteadFlee)
                {
                    if (WorldInterfaceOnGUI_Patch.attackBackExceptDoctors && isDoctor)
                    {

                    }
                    else
                    {
                        if (pawn.Faction == Faction.OfPlayer && pawn.playerSettings != null)
                        {
                            pawn.playerSettings.hostilityResponse = HostilityResponseMode.Attack;
                        }
                    }
                }
            }
        }
    }
}
