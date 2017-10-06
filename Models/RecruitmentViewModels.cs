using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bridge.Domain.Recruiting;
using Bridge.Web.Utility;

namespace Bridge.Web.Models
{
    public class RecruitmentCycleModel
    {
        public string RecruitmentCycle { get; set; }

        public DateTime DateActivated { get; set; }

        public bool RulesFileUploaded { get; set; }

        public string RulesFile { get; set; }       
    }

    public class RecruitmentCyclesModel
    {
        public RecruitmentCyclesModel(RecruitmentCycle recruitmentCycle)
        {
            CurrentRecruitmentCycle = new RecruitmentCycleModel
            {
                RecruitmentCycle = recruitmentCycle.RecruitmentCycleID,
                DateActivated = recruitmentCycle.DateActivated,
                RulesFile = RecruitmentCycleRules.GetRecruitmentCycleRulesFile(recruitmentCycle.RecruitmentCycleID),
                RulesFileUploaded = true,
            };

            if (!String.IsNullOrEmpty(recruitmentCycle.NextRecruitmentCycleID))
            {
                NextRecruitmentCycle = new RecruitmentCycleModel
                {
                    RecruitmentCycle = recruitmentCycle.NextRecruitmentCycleID,
                    RulesFile = RecruitmentCycleRules.GetRecruitmentCycleRulesFile(recruitmentCycle.NextRecruitmentCycleID)
                };

                NextRecruitmentCycle.RulesFileUploaded = System.IO.File.Exists(System.IO.Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/"), NextRecruitmentCycle.RulesFile));
            }
        }

        public RecruitmentCycleModel CurrentRecruitmentCycle { get; set; }

        public RecruitmentCycleModel NextRecruitmentCycle { get; set; }
    }
}