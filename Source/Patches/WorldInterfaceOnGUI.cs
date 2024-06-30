using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [HarmonyPatch(typeof(WorldInterface), "WorldInterfaceOnGUI")]
    public static class WorldInterfaceOnGUI_Patch
    {
        public static bool isEnabled = false;
        public static int colonistCount = 0;
        public static bool autoAssignBestSkills = true;
        public static bool basicsHighestPrio = true;
        public static bool doctorBeforeFirefighter = true;
        public static bool cookBeforeFirefighter = true;
        public static bool attackBackInsteadFlee = true;
        public static bool attackBackExceptDoctors = true;

        public static void Postfix()
        {
            if (Find.WorldInterface.selector.selectedTile >= 0 && !Find.PlaySettings.useWorkPriorities)
            {
                Rect rect = new Rect(0f, 0f, UI.screenWidth, UI.screenHeight);
                Rect buttonRect = new Rect(rect.width - 150f, 5f, 140f, 30f);
                if (Widgets.ButtonText(buttonRect, "Add X Colonists"))
                {
                    Find.WindowStack.Add(new Dialog_OverrideColonistCount());
                }
            }
        }
    }
}
