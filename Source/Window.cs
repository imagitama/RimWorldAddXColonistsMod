using UnityEngine;
using Verse;

namespace RimWorldAddXColonistsMod
{
    public class Dialog_OverrideColonistCount : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 400f);

        public Dialog_OverrideColonistCount()
        {
            Logger.LogMessage("Show window");
            closeOnClickedOutside = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            Widgets.Label(new Rect(inRect.x, inRect.y, 200, 30), WorldInterfaceOnGUI_Patch.isEnabled ? "Mod Enabled" : "Mod Disabled (0 colonists)");

            listingStandard.Gap(35f);

            listingStandard.Label("Number of Colonists:");

            string colonistCountBuffer = WorldInterfaceOnGUI_Patch.colonistCount.ToString();
            colonistCountBuffer = listingStandard.TextEntry(colonistCountBuffer);

            if (int.TryParse(colonistCountBuffer, out int parsedColonistCount))
            {
                WorldInterfaceOnGUI_Patch.isEnabled = parsedColonistCount > 0;
                WorldInterfaceOnGUI_Patch.colonistCount = parsedColonistCount;
            }

            listingStandard.Gap(10f);

            listingStandard.CheckboxLabeled("Assign highest prio to best skills", ref WorldInterfaceOnGUI_Patch.autoAssignBestSkills, "Looks at their skills and assigns 1 to whatever is best");

            listingStandard.CheckboxLabeled("  Firefighting, patient, basics all highest prio", ref WorldInterfaceOnGUI_Patch.basicsHighestPrio, "Includes mods \"Allow Tool Haul+\" and \"Rescuing Job\"");

            listingStandard.CheckboxLabeled("  Doctors highest prio over firefighting", ref WorldInterfaceOnGUI_Patch.doctorBeforeFirefighter, "Sets firefighting to prio 2");

            listingStandard.CheckboxLabeled("  Cooks highest prio", ref WorldInterfaceOnGUI_Patch.cookBeforeFirefighter, "With so many pawns food is issue number 1");

            listingStandard.Gap(10f);

            listingStandard.CheckboxLabeled("Change default behavior from flee to attack back", ref WorldInterfaceOnGUI_Patch.attackBackInsteadFlee, "Less micro with massive battles");

            listingStandard.CheckboxLabeled("  Ignore for doctors", ref WorldInterfaceOnGUI_Patch.attackBackExceptDoctors);

            listingStandard.End();
        }
    }
}
