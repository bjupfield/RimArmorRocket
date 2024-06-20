using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ArmorRocket
{
    public static class UIAdjust
    {
        public static string capAll(string capName)
        {
            string lolYouCantAssignToStrings = "";
            lolYouCantAssignToStrings += capName[0];
            for(int i = 1; i < capName.Length; i++)
            {
                if (capName[i - 1] == ' ')
                {
                    lolYouCantAssignToStrings = capName[i].ToString().CapitalizeFirst();
                }
                else
                {
                    lolYouCantAssignToStrings += capName[i];
                }
            }
            return lolYouCantAssignToStrings;
        }
    }
}
