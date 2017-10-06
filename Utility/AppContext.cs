using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Bridge.Domain.Recruiting;
using System.Xml;

namespace Bridge.Web.Utility
{
    public static class AppContext
    {
        private static string _RecruitmentCycle;

        public static string RecruitmentCycle
        {
            get
            {
                return _RecruitmentCycle;
            }
            private set
            {
                _RecruitmentCycle = value;
            }
        }

        public static void ActivateRecruitmentCycle(ISetupRepository setupRepository, string recruitmentCycle)
        {
            //...
            try
            {
                setupRepository.ActivateRecruitmentCycle(recruitmentCycle);

                var fileName = HttpContext.Current.Server.MapPath("~/App_Data/RecruitmentCycles.xml");

                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(fileName);

                var current = xmlDoc.SelectSingleNode("//RecruitmentCycle/Current");

                current.InnerText = recruitmentCycle;
                xmlDoc.Save(fileName);

                RecruitmentCycle = recruitmentCycle;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string CandidateScoringConfigFolder { get { return "~/App_Data/"; } }

        static AppContext()
        {

            _RecruitmentCycle = GetRecruitmentCycle();

        }

        public static string GetRecruitmentCycle()
        {
            var fileName = HttpContext.Current.Server.MapPath("~/App_Data/RecruitmentCycles.xml");

            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(fileName);

            var current = xmlDoc.SelectSingleNode("//RecruitmentCycle/Current");

            return current.InnerText;
        }

        public static string CandidateScoringConfigFile
        {
            get
            {
                return CandidateScoringConfigFolder + RecruitmentCycleRules.GetRecruitmentCycleRulesFile(RecruitmentCycle);
            }
        }
    }

}