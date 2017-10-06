using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bridge.Web.Utility
{
    public class UtilityFunctions
    {
        public static string ToTitleCase(string mText)
        {
            string rText = "";
            try
            {
                System.Globalization.CultureInfo cultureInfo =
    System.Threading.Thread.CurrentThread.CurrentCulture;
                System.Globalization.TextInfo TextInfo = cultureInfo.TextInfo;
                rText = TextInfo.ToTitleCase(mText.ToLower());
            }
            catch
            {
                rText = mText;
            }
            return rText;
        }
    }

    public static class RecruitmentCycleRules
    {
        public static string GetRecruitmentCycleRulesFile(String recruitmentCycleID)
        {
            return string.Format("{0}_CandidateScoringRules.txt", recruitmentCycleID.Replace(" ", "_"));
        }
    }
}