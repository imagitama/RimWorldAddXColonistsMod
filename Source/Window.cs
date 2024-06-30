using UnityEngine;
using Verse;

namespace RimWorldAddXColonistsMod
{
    public class Dialog_OverrideColonistCount : Window
    {
        public override Vector2 InitialSize => new Vector2(300f, 300f);

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

            Widgets.Label(new Rect(inRect.x, inRect.y, 200, 30), WorldInterfaceOnGUI_Patch.isEnabled ? "Mod Enabled" : "Mod Disabled");

            listingStandard.Gap(35f);

            listingStandard.Label("Number of Colonists:");

            string colonistCountBuffer = WorldInterfaceOnGUI_Patch.colonistCount.ToString();
            colonistCountBuffer = listingStandard.TextEntry(colonistCountBuffer);

            if (int.TryParse(colonistCountBuffer, out int parsedColonistCount))
            {
                WorldInterfaceOnGUI_Patch.isEnabled = parsedColonistCount > 0;
                WorldInterfaceOnGUI_Patch.colonistCount = parsedColonistCount;
            }

            listingStandard.CheckboxLabeled("Assign highest prio to best skills", ref WorldInterfaceOnGUI_Patch.autoAssignBestSkills, "Looks at their skills and assigns 1 to whatever is best");

            listingStandard.CheckboxLabeled("  Firefighting, patient, basics all highest prio", ref WorldInterfaceOnGUI_Patch.basicsHighestPrio, "Also Haul+ and Rescue");

            listingStandard.CheckboxLabeled("  Doctors highest prio over firefighting", ref WorldInterfaceOnGUI_Patch.doctorsFirst, "With so many pawns they tend to get hurt quickly");

            listingStandard.CheckboxLabeled("  Cooks highest prio", ref WorldInterfaceOnGUI_Patch.cooksFirst, "With so many pawns food is issue number 1");

            listingStandard.End();
        }
    }
}
