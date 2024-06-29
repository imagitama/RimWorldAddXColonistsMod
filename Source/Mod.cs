using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("imagitama.addxcolonists");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(WorldInterface), "WorldInterfaceOnGUI")]
    public static class WorldInterfaceOnGUI_Patch
    {
        public static bool isEnabled = false;
        public static int colonistCount = 0;
        public static bool autoAssignBestSkills = true;
        public static bool basicsHighestPrio = true;
        public static bool doctorsFirst = true;
        public static bool cooksFirst = true;

        public static void Postfix()
        {
            if (Find.WorldInterface.selector.selectedTile >= 0 && !Find.PlaySettings.useWorkPriorities)
            {
                Rect rect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
                Rect buttonRect = new Rect(rect.width - 150f, 50f, 140f, 30f);
                if (Widgets.ButtonText(buttonRect, "Add X Colonists"))
                {
                    Find.WindowStack.Add(new Dialog_OverrideColonistCount());
                }
            }
        }
    }

    public class Dialog_OverrideColonistCount : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 300f);

        public Dialog_OverrideColonistCount()
        {
            closeOnClickedOutside = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Widgets.Label(new Rect(inRect.x, inRect.y, 200, 30), WorldInterfaceOnGUI_Patch.isEnabled ? "Mod Enabled" : "Mod Disabled");

            listingStandard.Gap(35f);

            listingStandard.Label("Number of Colonists:");

            string colonistCountBuffer = WorldInterfaceOnGUI_Patch.colonistCount.ToString();
            colonistCountBuffer = listingStandard.TextEntry(colonistCountBuffer);

            if (int.TryParse(colonistCountBuffer, out int parsedColonistCount))
            {
                WorldInterfaceOnGUI_Patch.colonistCount = parsedColonistCount;

                WorldInterfaceOnGUI_Patch.isEnabled = parsedColonistCount > 0;
            }

            listingStandard.CheckboxLabeled("Assign highest prio to best skills", ref WorldInterfaceOnGUI_Patch.autoAssignBestSkills, "Looks at their skills and assigns 1 to whatever is best");

            listingStandard.CheckboxLabeled("  Firefighting, patient, basics all highest prio", ref WorldInterfaceOnGUI_Patch.basicsHighestPrio, "Also Haul+ and Rescue");

            listingStandard.CheckboxLabeled("  Doctors highest prio over firefighting", ref WorldInterfaceOnGUI_Patch.doctorsFirst, "With so many pawns they tend to get hurt quickly");

            listingStandard.CheckboxLabeled("  Cooks highest prio", ref WorldInterfaceOnGUI_Patch.cooksFirst, "With so many pawns food is issue number 1");


            listingStandard.End();
        }
    }

    [HarmonyPatch(typeof(Page_ConfigureStartingPawns), "PostOpen")]
    public static class Page_ConfigureStartingPawns_PostOpen_Patch
    {
        public static void Postfix()
        {
            if (!WorldInterfaceOnGUI_Patch.isEnabled)
            {
                return;
            }

            int desiredColonistCount = WorldInterfaceOnGUI_Patch.colonistCount;

            Log.Message($"Desired colonist count: {desiredColonistCount}");

            if (desiredColonistCount == 0)
            {
                Log.Message("It is 0, ignoring");
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

            Log.Message("Finished adding colonists");
        }
    }

    [HarmonyPatch(typeof(Map))]
    [HarmonyPatch("FinalizeInit")]
    public static class Map_FinalizeInit_Patch
    {
        public static void Postfix(Map __instance)
        {
            if (!WorldInterfaceOnGUI_Patch.isEnabled)
            {
                return;
            }
            if (!WorldInterfaceOnGUI_Patch.autoAssignBestSkills)
            {
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
                        if (workTypeDef.defName == "Firefighter") {
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

    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public static class Game_FinalizeInit_Patch
    {
        static void Postfix()
        {
            if (!WorldInterfaceOnGUI_Patch.isEnabled)
            {
                return;
            }

            Log.Message("Switching to work priorities");
            Current.Game.playSettings.useWorkPriorities = true;
        }
    }
}
