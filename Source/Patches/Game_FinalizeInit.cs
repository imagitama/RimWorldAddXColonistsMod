using HarmonyLib;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public static class Game_FinalizeInit_Patch
    {
        public static void Postfix()
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
