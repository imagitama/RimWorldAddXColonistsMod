using HarmonyLib;
using Verse;

namespace RimWorldAddXColonistsMod
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            Logger.LogMessage("Patching...");
            var harmony = new Harmony("imagitama.addxcolonists");
            harmony.PatchAll();
        }
    }
}
