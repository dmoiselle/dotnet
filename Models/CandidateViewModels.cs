using System;
using Bridge.Services.Recruiting;
using Bridge.Domain.Recruiting;
using Bridge.Services;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Bridge.Web.Utility;

namespace Bridge.Web.Models
{
    public class CandidateViewModel
    {
        public string RecruitmentCycle { get; set; }

        public readonly IEnumerable<int> WeekNumbers;

        public string TrainingClassID { get; set; }

        public string RecruitmentCyle { get; set; }

        public Candidate CurrentCandidate { get; set; }

        public CandidateModel CandidateModel { get; set; }
         
        public string selectedStatus { get; set; }

       

        [Display(Name = "Candidate Status:")]
        public readonly IEnumerable<SelectListItem> CandidateStatus;

        public IEnumerable<DataRow> CandidatePerformanceGroupedByAssessmentType { get; set; }
        
        public CandidateViewModel(
            Candidate theCandidate, 
            TargetAcademy originalAcademy, 
            Position position,
            IScoringService ScoringService , 
            RecruitmentCycleScoringRules recruitmentCycleScoringRules
            
            ) 
        {
            CurrentCandidate  = theCandidate;
            RecruitmentCyle = AppContext.RecruitmentCycle;
            TrainingClassID = theCandidate.TrainingClassID;
            WeekNumbers = recruitmentCycleScoringRules.CandidateTypeRules.SelectMany(c =>
               c.Weeks.Select(week => week.WeekNumber)
           ).Distinct();
            CandidatePerformanceGroupedByAssessmentType = theCandidate.GetPerformanceForCandidateGroupedByAssessmentType(WeekNumbers.Count(), ScoringService.GetAssessmentConfiguration(theCandidate.CandidateType));
            CandidateModel = theCandidate.MapToModel(ScoringService);
            CandidateModel.OriginalAcademy = originalAcademy.LocationDetails;
            CandidateModel.AssignedPosition = position;
            CandidateModel.RelocationStatus = position == null ? false : (position.AcademyID != originalAcademy.TargetAreaID);
            CandidateStatus = new[] {
            
                new SelectListItem { Text = "In Progress", Value = "InProgress" },
                new SelectListItem { Text = "Blacklisted", Value = "Blacklisted" },
                new SelectListItem { Text = "Dropped Out", Value = "DroppedOut" },
                new SelectListItem { Text = "Dismissed", Value = "Dismissed" },
                new SelectListItem { Text = "New", Value = "New" },
           };

           selectedStatus = theCandidate.CurrentCandidateStatus.ToString();
       
        }

        public CandidateViewModel(
            Candidate theCandidate
            
            ) {
            CurrentCandidate = theCandidate;
            RecruitmentCyle = AppContext.RecruitmentCycle;
            TrainingClassID = theCandidate.TrainingClassID;
            
            CandidateStatus = new[] {
            
                new SelectListItem { Text = "In Progress", Value = "InProgress"},
                new SelectListItem { Text = "Blacklisted", Value = "Blacklisted" },
                new SelectListItem { Text = "Dropped Out", Value = "DroppedOut" },
                new SelectListItem { Text = "Dismissed", Value = "Dismissed" },
                new SelectListItem { Text = "New", Value = "New"},
           };

        }
               
    }
    
    public class AssessmentViewModel
    {
        public AssessmentType Assessment_Type { get; set; }
        
    }
    
    public class CandidateHistoryEntryModel{
        public CandidateHistoryEntryModel(Candidate candidate)
        {
            currentStatus =  candidate.CurrentCandidateStatus;
            candidateID = candidate.CandidateID;
            candidateNationalID = candidate.NationalID;
            CandidateStatuses = new[]{
                new SelectListItem {
                    Text = CandidateStatus.New.ToString(),
                    Value = CandidateStatus.New.ToString()
                },
                new SelectListItem {
                    Text = CandidateStatus.Blacklisted.ToString(),
                    Value = CandidateStatus.Blacklisted.ToString()
                },
                new SelectListItem {
                    Text = CandidateStatus.Dismissed.ToString(),
                    Value = CandidateStatus.Dismissed.ToString()
                },
                new SelectListItem {
                    Text = CandidateStatus.DroppedOut.ToString(),
                    Value = CandidateStatus.DroppedOut.ToString()
                },              
            };
        }
        public CandidateStatus currentStatus { get; set; }
        public long candidateID {get; set; }
        public string candidateNationalID { get; set; }


        [Required(ErrorMessage = "Please select a status")]
        public string selectedStatus { get; set; }
         
        [Required]
        [Display(Name = "Change Candidate Status:")]
        public readonly IEnumerable<SelectListItem> CandidateStatuses;

        [Display(Name = "Give a reason why you are changing the candidate's status:")]
        [Required]
        public string Notes { get; set; }

    }

}