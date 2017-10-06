using Bridge.Domain.Recruiting;
using Bridge.Services.Recruiting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bridge.Web.Utility;

namespace Bridge.Web.Models
{

    public class CandidatesModel
    {
        public CandidatesModel(IEnumerable<Candidate> candidates, IEnumerable<TrainingSite> trainingSites, IEnumerable<string> trainingClasses, IScoringService scoringService, RecruitmentCycleScoringRules recruitmentCycleScoringRules)
        {

            Candidates = candidates.Select(candidate => candidate.MapToModel(scoringService, this.AssessmentTypesTolist));

            TrainingSites = trainingSites.Select(ts => new SelectListItem { Text = ts.Site, Value = ts.TrainingSiteID });

            TrainingClasses = trainingClasses.Select(tc => new SelectListItem { Text = tc, Value = tc });

            this.StarRating = new List<SelectListItem>(){ 
                new SelectListItem{Text = "5-Star", Value = "5"},
                new SelectListItem{Text = "4-Star", Value = "4"},
                new SelectListItem{Text = "3-Star", Value = "3"},
                new SelectListItem{Text = "2-Star", Value = "2"},
                new SelectListItem{Text = "1-Star", Value = "1"},
                new SelectListItem{Text = "0-Star", Value = "0"},
            };

            this.HiringStatuses = from hs in Enum.GetNames(typeof(HiringStatus)).AsEnumerable()
                                  select new SelectListItem { Text = hs.ToString(), Value = hs.ToString() };

            TargetSchools = scoringService.GetTargetAcademies()
                .OrderBy(o => o.TargetArea).Select(t => new SelectListItem { Text = t.LocationDetails, Value = t.TargetAreaID });

            WeekNumbers = recruitmentCycleScoringRules.CandidateTypeRules.SelectMany(c =>
                c.Weeks.Select(week => week.WeekNumber)
            ).Distinct();

            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString() }
            );

            this.AssessmentTypesTolist = scoringService.GetlistofActiveAccessments(
                candidates.Any() ? candidates.FirstOrDefault().CandidateType : CandidateType.Teacher);
        }

        public readonly IEnumerable<CandidateModel> Candidates;

        public IEnumerable<AssessmentType> AssessmentTypesTolist;

        public string CandidateStatus { get; set; }

        public string RecruitmentCycle { get; set; }

        [Display(Name = "Weeks:")]
        public readonly IEnumerable<int> WeekNumbers;

        [Display(Name = "Assessment Types:")]
        public readonly IEnumerable<string> AssessmentTypes;

        [Display(Name = "Training Classes:")]
        public readonly IEnumerable<SelectListItem> TrainingClasses;

        [Display(Name = "Training Sites:")]
        public readonly IEnumerable<SelectListItem> TrainingSites;

        [Display(Name = "Star Rating:")]
        public readonly IEnumerable<SelectListItem> StarRating;

        [Display(Name = "School")]
        public readonly IEnumerable<SelectListItem> TargetSchools;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Hiring Status:")]
        public readonly IEnumerable<SelectListItem> HiringStatuses;



    }

    public class AssessmentsDisplayModel
    {
        public AssessmentType AssessmentType { get; set; }
        public decimal? Score { get; set; }
    }

    public class CandidateModel
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public CandidateType CandidateType { get; set; }
        public decimal? SummaryScore { get; set; }
        public IEnumerable<WeeklyAssessmentModel> WeeklyAssessments { get; set; }
        public IDictionary<AssessmentType, decimal> AssessmentAverages { get; set; }
        public int? StarGrade { get; set; }
        public string NationalID { get; set; }
        public string CandidateStatus { get; set; }
        public string OriginalAcademy { get; set; }
        public string TrainingSiteID { get; set; }
        public string TrainingClassID { get; set; }
        public DateTime DateofBirth { get; set; }
        public string MpesaNumber { get; set; }
        public string MpesaName { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public HiringStatus HiringStatus { get; set; }
        public ContractStatusType AcceptanceStatus { get; set; }
        public ContractStatus ContractAcceptance { get; set; }
        public bool RelocationStatus { get; set; }
        public string SerialNumber { get; set; }
        public TargetAcademy AssignedAcademy { get; set; }
        public Position AssignedPosition { get; set; }
        public List<CandidateHistory> CandidateHistories { get; set; }
        public RecommendedPosition? FacilitatorRecommendedPosition { get; set; }
        public IEnumerable<AssessmentsDisplayModel> AssessmentDisplayModel { get; set; }

        public string PrettyFacilitatorRecommendedPosition
        {
            get
            {
                return RecommendedPositionToString(FacilitatorRecommendedPosition ?? RecommendedPosition.None, CandidateType);
            }
        }

        public static string RecommendedPositionToString(RecommendedPosition recClass, CandidateType candidateType)
        {
            switch (recClass)
            {
                case RecommendedPosition.None:
                    return "None";
                case RecommendedPosition.NotRecommended:
                    return "Not Recommended";
                case RecommendedPosition.ECD:
                    return "ECD";
                case RecommendedPosition.Class12:
                    return "Class 1/2";
                case RecommendedPosition.Class345:
                    return "Class 3/4/5";
                case RecommendedPosition.AcademyManagerMain:
                    return "AM Man";
                case RecommendedPosition.AcademyManagerSub:
                    return "AM Sub";
                default:
                    return "Not Recommended";
            }
        }

        public string FacilitatorRemarks { get; set; }

        public string FullName { get; set; }
    }

    public class WeeklyAssessmentModel
    {
        public decimal Score { get; set; }
        public int WeekNumber { get; set; }
    }

    public class AssessmentAverageModel
    {
        public decimal Score { get; set; }
        public AssessmentType AssessmentType { get; set; }
    }

    public class AssessmentEntryModel
    {

        [Required(ErrorMessage = "A Candidate Type must be selected")]
        public CandidateType CandidateType { get; set; }

        [Required(ErrorMessage = "A Target School must be selected")]
        public string TargetAcademy { get; set; }

        [Required]
        public AssessmentType AssessmentType { get; set; }

        public int? TypeNumber { get; set; }

        [Required]
        public int WeekNumber { get; set; }

        [Required]
        public decimal Score { get; set; }

        [Required]
        public string NationalID { get; set; }

    }

    public class AssessmentModel
    {
        public AssessmentModel() { }
        
        public AssessmentModel(decimal newScore)
        {
            Score = newScore;
        }

        public AssessmentModel(Candidate candidate, Assessment assessment)
        {

            NationalID = candidate.NationalID;
            Name = UtilityFunctions.ToTitleCase((String.IsNullOrWhiteSpace(candidate.Last_Name) ? null : candidate.Last_Name + ", ")
                    + (String.IsNullOrWhiteSpace(candidate.First_Name) ? null : candidate.First_Name + " ")
                    + (String.IsNullOrWhiteSpace(candidate.Middle_Name) ? null : candidate.Middle_Name));
            Score = assessment.RawTestScore;
            AssessmentID = assessment.AssessmentID;
            CandidateID = candidate.CandidateID;
        }

        public long AssessmentID { get; set; }
        public string NationalID { get; set; }
        public long CandidateID { get; set; }
        public string Name { get; set; }
        public decimal? Score { get; set; }

    }

    public class AssessmentsModel
    {
        public AssessmentsModel(RecruitmentCycleScoringRules recruitingCycleScoringRules)
        {
            CandidateTypes = recruitingCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem
                {
                    Text = candidateType.CandidateType.ToString(),
                    Value = candidateType.CandidateType.ToString(),
                }
            );

            RecruitingCycleScoringRules = recruitingCycleScoringRules;
        }

        public AssessmentsModel(IEnumerable<Candidate> Candidates, AssessmentType TypeOfAssessment, int? TypeNumber, int WeekNumber, RecruitmentCycleScoringRules recruitingCycleScoringRules)
        {

            CandidateTypes = recruitingCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem
                {
                    Text = candidateType.CandidateType.ToString(),
                    Value = candidateType.CandidateType.ToString(),
                }
            );

            RecruitingCycleScoringRules = recruitingCycleScoringRules;

            MapToModel(Candidates, TypeOfAssessment, TypeNumber, WeekNumber);
        }

        public readonly IEnumerable<SelectListItem> CandidateTypes;

        public IEnumerable<SelectListItem> TrainingClasses;

        [Display(Name = "Target School:")]
        public IEnumerable<SelectListItem> TargetSchools;

        public readonly RecruitmentCycleScoringRules RecruitingCycleScoringRules;

        public List<AssessmentModel> CandidateAssessments = new List<AssessmentModel>();

        private void MapToModel(IEnumerable<Candidate> Candidates, AssessmentType assessmentType, int? typeNumber, int weekNumber)
        {
            AssessmentModel assessmentModel;

            foreach (var candidate in Candidates)
            {
                assessmentModel = new AssessmentModel();

                assessmentModel.Name = candidate.FullName;
                assessmentModel.NationalID = candidate.NationalID;
                assessmentModel.CandidateID = candidate.CandidateID;

                if (candidate.Assessments.Where(a => a.AssessmentType == assessmentType && a.TypeNumber == typeNumber && a.WeekNumber == weekNumber).Count() > 0)
                {
                    assessmentModel.AssessmentID = candidate.Assessments.First(a => a.AssessmentType == assessmentType && a.TypeNumber == typeNumber && a.WeekNumber == weekNumber).AssessmentID;
                    assessmentModel.Score = candidate.Assessments.First(a => a.AssessmentType == assessmentType && a.TypeNumber == typeNumber && a.WeekNumber == weekNumber).RawTestScore;
                }
                CandidateAssessments.Add(assessmentModel);
            }
        }
    }

    public static class CandidatesModelMapper
    {
        public static CandidateModel MapToModel(this Candidate domain, IScoringService scoringService, IEnumerable<AssessmentType> listofActiveAssessments = null)
        {
            var weeklyScoringRuleSets = scoringService.GetAllWeeklyScoringRulesForCandidateType(domain.CandidateType);

            if (listofActiveAssessments == null) listofActiveAssessments = new List<AssessmentType>();

            var model = new CandidateModel
            {
                FirstName = domain.First_Name,
                MiddleName = domain.Middle_Name,
                LastName = domain.Last_Name,
                FullName = domain.FullName,
                CandidateType = domain.CandidateType,
                SummaryScore = scoringService.GetSummaryScore(weeklyScoringRuleSets, domain.Assessments, domain.CandidateType),
                NationalID = domain.NationalID,
                OriginalAcademy = domain.OriginalAcademyID,
                TrainingSiteID = domain.TrainingSiteID,
                TrainingClassID = domain.TrainingClassID,
                AssignedAcademy = domain.Academy,
                DateofBirth = domain.DateofBirth,
                MpesaNumber = domain.MpesaNumber,
                MpesaName = domain.MpesaName,
                PhoneNumber1 = domain.PhoneNumber1,
                PhoneNumber2 = domain.PhoneNumber2,
                HiringStatus = domain.HiringStatus,
                AcceptanceStatus = domain.AcceptanceStatus,
                AssessmentDisplayModel = (from ss in listofActiveAssessments.DefaultIfEmpty()
                                          join cS in domain.Assessments.DefaultIfEmpty(new Assessment())
                                          on ss equals cS.AssessmentType
                                          into candidateScores
                                          from cS1 in candidateScores.DefaultIfEmpty(

                                          new Assessment
                                          {
                                              AssessmentType = ss,
                                          }).OrderBy(r => r.AssessmentType)
                                          group cS1 by cS1.AssessmentType into avgs
                                          select new AssessmentsDisplayModel
                                          {
                                              AssessmentType = avgs.Key,
                                              Score = avgs.Average(
                                              r => r.WeekNumber == 0 ? 0 : (r.RawTestScore / scoringService.GetMaximumScoreForAssessment(domain.CandidateType, r.AssessmentType, r.WeekNumber, r.TypeNumber)) * 100
                                              ),

                                          }),




            };

            if (model.SummaryScore != null)
            {
                model.StarGrade = scoringService.GetStarForSummaryScore((decimal)model.SummaryScore, model.CandidateType);
            }

            model.WeeklyAssessments = domain.Assessments.Where(a => a.Source.ToLower() == Assessment.ASSESSMENT_SOURCE_TRAINING.ToLower()).Select(a =>
                new WeeklyAssessmentModel
                {
                    WeekNumber = a.WeekNumber,
                    Score = scoringService.GetWeeklyWeightedAverage(domain.Assessments, domain.CandidateType, a.WeekNumber),
                }
            );

            model.AssessmentAverages = scoringService.CalculateAssessmentAverages(domain.Assessments, domain.CandidateType);

            var recommendation = domain.FacilitatorRecommendations.FirstOrDefault();
            if (recommendation != null)
            {
                model.FacilitatorRecommendedPosition = recommendation.RecommendedPosition;
                model.FacilitatorRemarks = recommendation.Remarks;
            }

            return model;
        }

    }

    public enum ScriptType
    {
        StoredProcedure,
        SSISPackage,
        Code,
    }

    public class SSISPackageViewModel
    {
        public SSISPackageViewModel()
        {
            this.DataRoutineFeedback = new List<DataRoutineFeedback>() { new DataRoutineFeedback() };
        }
        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public string Description { get; set; }
        public string PathName { get; set; }
        public bool List { get; set; }
        public string LastOutcome { get; set; }
        public DateTime? LastExecution { get; set; }
        public IEnumerable<DataRoutineFeedback> DataRoutineFeedback { get; set; }
        public ScriptType ScriptType { get; set; }
        public int Precedence { get; set; }
    }

    public class FacilitatorRecommendationsModel
    {
        public FacilitatorRecommendationsModel(IEnumerable<Candidate> candidates, CandidateType typeOfCandidate,
                                       IEnumerable<TrainingSite> trainingSites, IEnumerable<string> trainingClasses,
                                       IScoringService scoringService, RecruitmentCycleScoringRules recruitmentCycleScoringRules)
        {

            Candidates = candidates.Select(candidate => candidate.MapToModel(scoringService));
            TrainingSites = trainingSites.Select(ts => new SelectListItem { Text = ts.Site, Value = ts.TrainingSiteID });
            TrainingClasses = trainingClasses.Select(tc => new SelectListItem { Text = tc, Value = tc }); ;
            WeekNumbers = recruitmentCycleScoringRules.CandidateTypeRules.SelectMany(c =>
                c.Weeks.Select(week => week.WeekNumber)
            ).Distinct();

            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString() }
            );

        }

        public readonly IEnumerable<CandidateModel> Candidates;

        public string RecruitmentCycle { get; set; }

        [Display(Name = "Weeks:")]
        public readonly IEnumerable<int> WeekNumbers;

        [Display(Name = "Training Sites:")]
        public readonly IEnumerable<SelectListItem> TrainingSites;

        [Display(Name = "Training Classes:")]
        public readonly IEnumerable<SelectListItem> TrainingClasses;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

    }
}
