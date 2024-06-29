using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmorRacks;
using RimWorld;
using UnityEngine;
using Verse;
using ArmorRocket.ModSetting;

namespace ArmorRocket
{
    public class ArmorRocketMod : Mod 
    {
        public ArmorRocketModSettings Settings;

        public ArmorRocketMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<ArmorRocketModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            //IL_0008: Unknown result type (might be due to invalid IL or missing references)
            //IL_0092: Unknown result type (might be due to invalid IL or missing references)
            //IL_00c6: Unknown result type (might be due to invalid IL or missing references)
            //lol just so you know how I did this

/*
 * public int rocketSpeed = 10;

public bool noMend = false;
public bool noMech = false;
public bool noDisplay = false;

public bool nosound = false;
*/
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("Toggle Mod Sound", ref Settings.nosound);
            listingStandard.GapLine();
            listingStandard.Label("Rocket Speed", -1f);
            string buffer = Settings.rocketSpeed.ToString();
            listingStandard.IntEntry(ref Settings.rocketSpeed, ref buffer);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Armor Rocket";
        }
    }

}
