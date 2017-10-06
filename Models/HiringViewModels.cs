using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Bridge.Domain.Recruiting;
using Bridge.Services.Recruiting;
using Bridge.Web.Validators;
using Bridge.Web.Utility;

namespace Bridge.Web.Models
{
    public class HiringCandidatesModel
    {
        public HiringCandidatesModel(IScoringService scoringService, IScoringRulesRepository scoringRulesRepository, IEnumerable<Candidate> candidates, IEnumerable<ContractStatus> contractStatuses, CandidateFilter candidateFilter)
        {
            var recruitmentCycleScoringRules = scoringRulesRepository.GetRecruitmentCycleScoringRules();
            var weeklyScoringRuleSets = scoringService.GetAllWeeklyScoringRulesForCandidateType(candidateFilter.CandidateType.Value);
            var recruitmentAssessmentTypes = scoringRulesRepository.GetRecruitmentScoringRules(candidateFilter.CandidateType.Value).OrderBy(r => r.Order).Select(r => r.Type);
            var trainingAssessmentTypes = scoringRulesRepository.GetSummaryScoringRules(candidateFilter.CandidateType.Value).Rules.OrderBy(r => r.Order).Select(r => r.Type);

            CandidateTypes = ((CandidateType[])Enum.GetValues(typeof(CandidateType))).Select(candidateType =>
                new SelectListItem { Text = candidateType.ToString(), Value = candidateType.ToString(), Selected = (candidateType == candidateFilter.CandidateType) });

            TargetAcademies = scoringService.GetTargetAcademiesWithPositions(AppContext.RecruitmentCycle).OrderBy(o=>o.LocationDetails).Select(t=> new SelectListItem {Text =  t.LocationDetails, Value = t.TargetAreaID, Selected = t.TargetAreaID == candidateFilter.TargetAcademy });

            HiringStatuses = ((HiringStatus[])Enum.GetValues(typeof(HiringStatus))).Select(hs =>
                new SelectListItem { Text = hs.ToString(), Value = hs.ToString(), Selected = (hs == candidateFilter.HiringStatus) });

            RecruitmentAssessmentTypes = recruitmentAssessmentTypes.AsEnumerable();
            TrainingAssessmentTypes = trainingAssessmentTypes.AsEnumerable();

            StarRating = new List<SelectListItem>(){ 
                new SelectListItem{Text = "5-Star", Value = "5", Selected = (candidateFilter.StarRating == "5") },
                new SelectListItem{Text = "4-Star", Value = "4", Selected = (candidateFilter.StarRating == "4")},
                new SelectListItem{Text = "3-Star", Value = "3", Selected = (candidateFilter.StarRating == "3")},
                new SelectListItem{Text = "2-Star", Value = "2", Selected = (candidateFilter.StarRating == "2")},
                new SelectListItem{Text = "1-Star", Value = "1", Selected = (candidateFilter.StarRating == "1")},
                new SelectListItem{Text = "0-Star", Value = "0", Selected = (candidateFilter.StarRating == "0")},
            };
                
            var hiringCandidates = new List<HiringCandidateModel>();
            foreach (var candidate in candidates)
            {
                var summaryScore = scoringService.GetSummaryScore(weeklyScoringRuleSets, candidate.Assessments, candidate.CandidateType);

                int? starRating = null;
                if (summaryScore.HasValue)
                {
                    starRating = scoringService.GetStarForSummaryScore(summaryScore.Value, candidate.CandidateType);
                }

                if ((!String.IsNullOrEmpty(candidateFilter.StarRating) && !starRating.HasValue) || (!String.IsNullOrEmpty(candidateFilter.StarRating) && starRating.Value.ToString() != candidateFilter.StarRating)) continue;

                var contractStatus = contractStatuses.FirstOrDefault(cs => cs.CandidateID == candidate.CandidateID);               
                
                var facilitatorRec = candidate.FacilitatorRecommendations.Count() > 0 ? candidate.FacilitatorRecommendations.LastOrDefault() : null;
                String displayPosition = String.Empty, serialNumber = String.Empty;
                if (contractStatus != null)
                {
                    serialNumber = contractStatus.SerialNumber;
                    var classification = ExtractClassification(serialNumber);
                    
                    displayPosition = Bridge.Web.Models.Classification.Classifications.Single(c => c.ClassID == classification).DisplayText;
                }

                hiringCandidates.Add(new HiringCandidateModel
                {
                    AssignedAcademy = candidate.Academy.LocationDetails,
                    FullName = candidate.FullName,
                    CandidateID = candidate.CandidateID,
                    NationalID = candidate.NationalID,
                    FacilitatorRecommendedPosition = facilitatorRec != null ? facilitatorRec.RecommendedPosition.ToString() : String.Empty,
                    FacilitatorRemarks = facilitatorRec != null ? facilitatorRec.Remarks : null,
                    SummaryScore = summaryScore,
                    StarGrade = summaryScore.HasValue ? starRating : null,
                    AssessmentAverages = scoringService.CalculateAssessmentAverages(candidate.Assessments, candidate.CandidateType),
                    HiringStatus = candidate.HiringStatus,
                    CandidateStatus = candidate.CurrentCandidateStatus.ToString(),
                    SerialNumber = serialNumber,
                    DisplayPosition = displayPosition,
                    CandidateType = candidate.CandidateType,
                    AcceptanceStatus = contractStatus != null ? (ContractStatusType)contractStatus.Status : (ContractStatusType?)null
                });
            }

            Candidates = hiringCandidates;
        }

        private string ExtractClassification(string serialNumber)
        {
            var token = serialNumber.Substring(serialNumber.IndexOf("-") + 1);
            return token.Substring(0, token.IndexOf("-"));
        }

        public IEnumerable<ContractStatusType> ContractStatuses { get; set; }

        public IEnumerable<HiringCandidateModel> Candidates { get; set; }

        public IEnumerable<SelectListItem> CandidateTypes { get; set; }

        public CandidateFilter CandidateFilter { get; set; }

        public IEnumerable<SelectListItem> TargetAcademies { get; set; }

        public IEnumerable<SelectListItem> HiringStatuses { get; set; }

        public List<SelectListItem> StarRating { get; set; }

        public IEnumerable<AssessmentType> RecruitmentAssessmentTypes { get; set; }

        public IEnumerable<AssessmentType> TrainingAssessmentTypes { get; set; }
    }

    public class HiringCandidateModel
    {
        public string FullName { get; set; }
        public CandidateType CandidateType { get; set; }
        public decimal? SummaryScore { get; set; }
        public IDictionary<AssessmentType, decimal> AssessmentAverages { get; set; }
        public int? StarGrade { get; set; }
        public long CandidateID { get; set; }
        public string NationalID { get; set; }
        public string CandidateStatus { get; set; }
        public HiringStatus HiringStatus { get; set; }
        public ContractStatusType? AcceptanceStatus { get; set; }
        public String AssignedAcademy { get; set; }
        public String SerialNumber { get; set; }
        public string DisplayPosition { get; set; }
        public String FacilitatorRecommendedPosition { get; set; }
        public string FacilitatorRemarks { get; set; }
    }
    
    public class CandidateFilter
    {
        public CandidateType? CandidateType { get; set; }

        public HiringStatus? HiringStatus { get; set; }

        public string TargetAcademy { get; set; }

        public string StarRating { get; set; }
    }

    public class OfferStatusesModel
    {

        public OfferStatusesModel()
        {

        }
        public OfferStatusesModel(IEnumerable<TargetAcademy> targetSchool, RecruitmentCycleScoringRules recruitingCycleScoringRules)
        {

            OfferStatuses = new[] {
                        new SelectListItem { Text = "Backup", Value = "Backup" , Selected = true},
                        new SelectListItem { Text = "Rejected", Value = "Rejected" },
                        //new SelectListItem { Text = "Relocation", Value = "Relocation" },
                };

            TargetSchools = new[] {
                new SelectListItem {Text = "ALL", Value = "", Selected = true}
                };

            var tSch = from sch in targetSchool
                       orderby sch.TargetArea
                       select new SelectListItem { Text = sch.LocationDetails, Value = sch.TargetAreaID };
            TargetSchools = TargetSchools.Union(tSch);


            CandidateTypes = recruitingCycleScoringRules.CandidateTypeRules.Select(candidateType =>
            new SelectListItem
            {
                Text = candidateType.CandidateType.ToString(),
                Value = candidateType.CandidateType.ToString(),
            });
        }

        [Display(Name = "Recruitment Cycle:")]
        public string RecruitmentCycle { get; set; }

        [Display(Name = "Offer Status:")]
        public readonly IEnumerable<SelectListItem> OfferStatuses;

        [Display(Name = "School")]
        public readonly IEnumerable<SelectListItem> TargetSchools;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        public string TrainingSite { get; set; }
        public string TargetAcademy { get; set; }
        public string OfferStatus { get; set; }
        public string CandidateType { get; set; }

    }

    public class OffersModel
    {
        public OffersModel(string recruitmentCyle,
                            IEnumerable<PositionType> jobPositions,
                            IEnumerable<ContractType> contractTypes,
                            string selectedPositon,
                            string selectedContractType)
        {
            RecruitmentCycle = recruitmentCyle;
            Position = selectedPositon;
            ContractType = selectedContractType;

                     Positions = jobPositions
                            .Select(pos =>
                                        new SelectListItem { Text = pos.Title, Value = pos.PositionTypeID.ToString(), Selected = (pos.Title == selectedPositon) }
                                        );

            ContractTypes = contractTypes
                            .Select(contractType =>
                                        new SelectListItem { Text = contractType.Name, Value = contractType.ContractTypeID.ToString(), Selected = (contractType.Name == selectedContractType) }
                                        );


        }

        [Display(Name = "Recruitment Cycle:")]
        public readonly string RecruitmentCycle;

        public readonly string Position;
        public readonly string ContractType;

        [Display(Name = "Position:")]
        public readonly IEnumerable<SelectListItem> Positions;

        [Display(Name = "Contract Type:")]
        public readonly IEnumerable<SelectListItem> ContractTypes;


    }

    public class RelocatedCandidateModel
    {
        public string FullName { get; set; }
        public CandidateType CandidateType { get; set; }
        public decimal? SummaryScore { get; set; }
        public IEnumerable<WeeklyAssessmentModel> WeeklyAssessments { get; set; }
        public int? StarGrade { get; set; }
        public string NationalID { get; set; }
        public string OriginalAcademy { get; set; }
        public TargetAcademy AssignedAcademy { get; set; }
        [Required]
        public string AssignedVacancy { get; set; }
        public List<SelectListItem> AssignedPositions { get; set; }
        [Required]
        [StringCurrencyValidator("Invalid amount")]
        public string RelocationAmount { get; set; }
    }

    public class RelocatedCandidatesModel
    {
        public RelocatedCandidatesModel() { }

        public RelocatedCandidatesModel(IEnumerable<Candidate> candidates, IScoringService scoringService, RecruitmentCycleScoringRules recruitmentCycleScoringRules, IEnumerable<Position> availablePositions, IEnumerable<TargetAcademy> targetSchools)
        {

            var candidatesList = candidates.Select(candidate => candidate.MapToModel(scoringService)).OrderByDescending(c => c.SummaryScore);
            /*TargetSchools = scoringService.GetTargetAcademiesWithPositions()
                .OrderBy(o => o.TargetArea).Select(t => new SelectListItem { Text = t.LocationDetails, Value = t.TargetAreaID });*/
            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString(), Selected = (candidateType.CandidateType == CandidateType.Teacher) }
            );
            WeekNumbers = recruitmentCycleScoringRules.CandidateTypeRules.SelectMany(c =>
            c.Weeks.Select(week => week.WeekNumber)
            ).Distinct();
            AvailablePositions = availablePositions.Select(v => new SelectListItem { Text = v.SerialNumber });
            _candidates = MapToModel(candidatesList, availablePositions).ToList();

            TargetSchools = from s in targetSchools select new SelectListItem { Text = s.LocationDetails, Value = s.TargetAreaID };

        }

        public IEnumerable<RelocatedCandidateModel> Candidates
        {
            get
            {
                return _candidates;
            }
        }

        public string CandidateStatus { get; set; }

        public string RecruitmentCycle { get; set; }

        public List<Position> AssignedPositions { get; set; }

        [Display(Name = "School")]
        public readonly IEnumerable<SelectListItem> TargetSchools;

        [Display(Name = "Weeks:")]
        public readonly IEnumerable<int> WeekNumbers;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Available Positions")]
        public readonly IEnumerable<SelectListItem> AvailablePositions;

        public IEnumerable<RelocatedCandidateModel> MapToModel(IEnumerable<CandidateModel> candidates, IEnumerable<Position> availablePositions)
        {
            var relocatedCandidates = from c in candidates
                                      select new RelocatedCandidateModel
                                      {
                                          FullName = c.FullName,
                                          CandidateType = c.CandidateType,
                                          AssignedAcademy = c.AssignedAcademy,
                                          WeeklyAssessments = c.WeeklyAssessments,
                                          AssignedPositions = availablePositions.Select(
                                                a => new SelectListItem
                                                {
                                                    Text = a.SerialNumber,
                                                    Value = a.SerialNumber,
                                                }).ToList(),
                                          NationalID = c.NationalID,
                                          OriginalAcademy = c.OriginalAcademy,
                                          RelocationAmount = string.Empty,
                                          StarGrade = c.StarGrade,
                                          SummaryScore = c.SummaryScore
                                      };

            return relocatedCandidates;
        }

        internal void resetValuesForCandidate(string nationalId, string relocationAmount, string assignedVacancy)
        {
            var candidate = _candidates.First(c => c.NationalID == nationalId);
            candidate.RelocationAmount = relocationAmount;
            candidate.AssignedVacancy = assignedVacancy;
            if (candidate.AssignedPositions.Any(item => item.Value == assignedVacancy))
            {
                candidate.AssignedPositions.First(item => item.Value == assignedVacancy).Selected = true;
            }
        }

        private List<RelocatedCandidateModel> _candidates;
    }

    public class CandidateContractStatusModel
    {
        public string NationalID { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }
        public ContractStatusType Status { get; set; }
        public string Comments { get; set; }
        public string Location { get; set; }
        public CandidateType CandidateType { get; set; }
        public IEnumerable<SelectListItem> ContractStatuses;
    }

    public class CandidatesContractStatusModel
    {

        public CandidatesContractStatusModel(IEnumerable<Candidate> candidates, IEnumerable<ContractStatus> contractStatus, IEnumerable<TargetAcademy> targetSchools, RecruitmentCycleScoringRules recruitmentCycleScoringRules)
        {
            Candidates = MapToModel(candidates, contractStatus, targetSchools);

            TargetSchools = from t in targetSchools select new SelectListItem { Text = t.LocationDetails, Value = t.TargetAreaID };

            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString() }
            );

        }

        public readonly IEnumerable<CandidateContractStatusModel> Candidates;

        [Display(Name = "Academy")]
        public readonly IEnumerable<SelectListItem> TargetSchools;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> ContractTypes;

        public IEnumerable<CandidateContractStatusModel> MapToModel(IEnumerable<Candidate> candidates, IEnumerable<ContractStatus> contractStatus, IEnumerable<TargetAcademy> targetSchools)
        {
            var candidatesContractStatusModel = from c in candidates
                                                join cs in contractStatus on c.CandidateID equals cs.CandidateID
                                                join s in targetSchools on c.Academy.TargetAreaID equals s.TargetAreaID
                                                select new CandidateContractStatusModel
                                                {
                                                    FullName = c.FullName,
                                                    NationalID = c.NationalID,
                                                    SerialNumber = cs.SerialNumber,
                                                    Comments = cs.Comments,
                                                    Location = s.LocationDetails,
                                                    Status = cs.Status,
                                                    ContractStatuses = from ContractStatusType type in Enum.GetValues(typeof(ContractStatusType))
                                                                       select new SelectListItem { Text = type.ToString(), Value = type.ToString(), Selected = (cs.Status == type) },
                                                    CandidateType = c.CandidateType
                                                };

            return candidatesContractStatusModel;
        }

    }

    public class CandidateContractListItem
    {
        public CandidateType CandidateType { get; set; }
        public string ContractType { get; set; }
        public long NoOfPositions { get; set; }
        public long Offered { get; set; }
        public long Accepted { get; set; }
        public long Declined { get; set; }
        public string RecruitmentCycle { get; set; }
        public long PositionTypeID { get; set; }
        public int ContractTypeID { get; set; }
        public string ContractTypeDescription { get; set; }
    }

    public class CandidateLettersItem {
        public CandidateType CandidateType { get; set; }
        public long NoOfCandidates { get; set; }
        public HiringStatus HiringStatus { get; set; }
        public string RecruitmentCycle { get; set; }       
    }

    public class HiringDashboardModel{
        private IScoringService _scoringService;
        private IScoringRulesRepository _scoringRulesRepository;

        public HiringDashboardModel(IScoringService scoringService, IScoringRulesRepository scoringRulesRepository, IEnumerable<Candidate> candidates, IEnumerable<Position> positions, IEnumerable<ContractStatus> contracts)
        {
            _scoringService = scoringService;
            _scoringRulesRepository = scoringRulesRepository;

            InflatePositionStats(positions);
            InflateCandidateStats(candidates, contracts);
            InflateContractStats(positions, contracts);
        }

        private void InflatePositionStats(IEnumerable<Position> positions)
        {
            var assignedPositions = positions.Where(r => r.CandidateID != null);
            var unassignedPositions = positions.Where(r => r.CandidateID == null);

            TeacherPositionsStat = new PositionStat();

            TeacherPositionsStat.Total = positions.Count(p => p.PositionType.CandidateType == CandidateType.Teacher);
            TeacherPositionsStat.Filled = assignedPositions.Count(p => p.PositionType.CandidateType == CandidateType.Teacher);
            TeacherPositionsStat.Open = unassignedPositions.Count(p => p.PositionType.CandidateType == CandidateType.Teacher);

            AMPositionsStat = new PositionStat();

            AMPositionsStat.Total = positions.Count(p => p.PositionType.CandidateType == CandidateType.AcademyManager);
            AMPositionsStat.Filled = assignedPositions.Count(p => p.PositionType.CandidateType == CandidateType.AcademyManager);
            AMPositionsStat.Open = unassignedPositions.Count(p => p.PositionType.CandidateType == CandidateType.AcademyManager);
        }

        private void InflateCandidateStats(IEnumerable<Candidate> candidates, IEnumerable<ContractStatus> contracts)
        {
            var teacherCandidates = candidates.Where(c => c.CandidateType == CandidateType.Teacher);
            var managerCandidates = candidates.Where(c => c.CandidateType == CandidateType.AcademyManager);

            TeacherCandidatesStat = new CandidateStat();

            TeacherCandidatesStat.Total = teacherCandidates.Count();
            TeacherCandidatesStat.Assigned = teacherCandidates.Join(contracts, c => c.CandidateID, p => p.CandidateID, (c, p) => c).Count();
            TeacherCandidatesStat.Backup = teacherCandidates.Count(c => c.HiringStatus == HiringStatus.Backup);
            TeacherCandidatesStat.Rejected = teacherCandidates.Count(c => c.HiringStatus == HiringStatus.Rejected);
            TeacherCandidatesStat.Available = TeacherCandidatesStat.Total - (TeacherCandidatesStat.Assigned + TeacherCandidatesStat.Backup + TeacherCandidatesStat.Rejected);
            TeacherCandidatesStat.Relocated = teacherCandidates.Count(c => c.Academy.TargetAreaID != c.OriginalAcademyID && c.HiringStatus == HiringStatus.Assigned);

            AMCandidatesStat = new CandidateStat();

            AMCandidatesStat.Total = managerCandidates.Count();
            AMCandidatesStat.Assigned = managerCandidates.Join(contracts, c => c.CandidateID, p => p.CandidateID, (c, p) => c).Count();
            AMCandidatesStat.Backup = managerCandidates.Count(c => c.HiringStatus == HiringStatus.Backup);
            AMCandidatesStat.Rejected = managerCandidates.Count(c => c.HiringStatus == HiringStatus.Rejected);
            AMCandidatesStat.Available = AMCandidatesStat.Total - (AMCandidatesStat.Assigned + AMCandidatesStat.Backup + AMCandidatesStat.Rejected);
            AMCandidatesStat.Relocated = managerCandidates.Count(c => c.Academy.TargetAreaID != c.OriginalAcademyID && c.HiringStatus == HiringStatus.Assigned);

            InflateCandidateRatingStat(teacherCandidates, managerCandidates);
        }

        private void InflateCandidateRatingStat(IEnumerable<Candidate> teacherCandidates, IEnumerable<Candidate> managerCandidates)
        {
            // Resolve the subsets by Ids here - to avoid multiple execution while looping
            var teacherAssignedIds = teacherCandidates.Where(c => c.HiringStatus == HiringStatus.Assigned).Select(c => c.CandidateID).ToArray();
            var managerAssignedIds = managerCandidates.Where(c => c.HiringStatus == HiringStatus.Assigned).Select(c => c.CandidateID).ToArray();
            var teacherBackupIds = teacherCandidates.Where(c => c.HiringStatus == HiringStatus.Backup).Select(c => c.CandidateID).ToArray();
            var managerBackupIds = managerCandidates.Where(c => c.HiringStatus == HiringStatus.Backup).Select(c => c.CandidateID).ToArray();
            var teacherRejectedIds = teacherCandidates.Where(c => c.HiringStatus == HiringStatus.Rejected).Select(c => c.CandidateID).ToArray();
            var managerRejectedIds = managerCandidates.Where(c => c.HiringStatus == HiringStatus.Rejected).Select(c => c.CandidateID).ToArray();
            
            var dictWeeklyScoringRuleSets = new Dictionary<CandidateType, IEnumerable<WeeklyScoringRuleSet>>();
            dictWeeklyScoringRuleSets[CandidateType.Teacher] = _scoringService.GetAllWeeklyScoringRulesForCandidateType(CandidateType.Teacher);
            dictWeeklyScoringRuleSets[CandidateType.AcademyManager] = _scoringService.GetAllWeeklyScoringRulesForCandidateType(CandidateType.AcademyManager);

            TeacherCandidatesRatingStat = new Dictionary<int, CandidateStat>();
            AMCandidatesRatingStat = new Dictionary<int, CandidateStat>();

            var ratings = new List<int>() { 5, 4, 3, 2, 1, 0, -1 };

            var candidateRatings = new Dictionary<Candidate, int?>();
            foreach (var candidate in teacherCandidates.Union(managerCandidates))
            {
                var summaryScore = _scoringService.GetSummaryScore(dictWeeklyScoringRuleSets[candidate.CandidateType], candidate.Assessments, candidate.CandidateType);
                if (summaryScore.HasValue)
                {
                    candidateRatings.Add(candidate, _scoringService.GetStarForSummaryScore(summaryScore.Value, candidate.CandidateType));
                }
                else
                {
                    candidateRatings.Add(candidate, null);
                }
            }

            foreach (var star in ratings)
            {
                var starStat1 = new CandidateStat();
                starStat1.Total = candidateRatings.Count(cr => cr.Key.CandidateType == CandidateType.Teacher && cr.Value == (star == -1 ? (int?)null : star));
                starStat1.Assigned = candidateRatings.Count(cr => teacherAssignedIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat1.Backup = candidateRatings.Count(cr => teacherBackupIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat1.Rejected = candidateRatings.Count(cr => teacherRejectedIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat1.Available = starStat1.Total - (starStat1.Assigned + starStat1.Backup + starStat1.Rejected);

                TeacherCandidatesRatingStat.Add(star, starStat1);

                var starStat2 = new CandidateStat();
                starStat2.Total = candidateRatings.Count(cr => cr.Key.CandidateType == CandidateType.AcademyManager && cr.Value == (star == -1 ? (int?)null : star));
                starStat2.Assigned = candidateRatings.Count(cr => managerAssignedIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat2.Backup = candidateRatings.Count(cr => managerBackupIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat2.Rejected = candidateRatings.Count(cr => managerRejectedIds.Contains(cr.Key.CandidateID) && cr.Value == (star == -1 ? (int?)null : star));
                starStat2.Available = starStat2.Total - (starStat2.Assigned + starStat2.Backup + starStat2.Rejected);

                AMCandidatesRatingStat.Add(star, starStat2);
            }
        }

        private void InflateContractStats(IEnumerable<Position> positions, IEnumerable<ContractStatus> contracts)
        {
            var teacherPositions = positions.Join(contracts, p => p.SerialNumber, c => c.SerialNumber, (p, c) => p).Where(p => p.PositionType.CandidateType == CandidateType.Teacher);
            var managerPositions = positions.Join(contracts, p => p.SerialNumber, c => c.SerialNumber, (p, c) => p).Where(p => p.PositionType.CandidateType == CandidateType.AcademyManager);

            var teacherContractTypes = teacherPositions.Select(p => p.ContractType.Description).Distinct();
            var managerContractTypes = managerPositions.Select(p => p.ContractType.Description).Distinct();

            TeacherContractsStat = new Dictionary<string, ContractStat>();

            foreach (var contractType in teacherContractTypes)
            {
                var contractStat = new ContractStat();

                contractStat.Total = teacherPositions.Count(p => p.ContractType.Description == contractType);
                
                contractStat.Offered = teacherPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Offered), 
                                        p => p.SerialNumber, 
                                        c => c.SerialNumber, 
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);

                contractStat.Accepted = teacherPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Accepted),
                                        p => p.SerialNumber,
                                        c => c.SerialNumber,
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);
                
                contractStat.Declined= teacherPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Declined), 
                                        p => p.SerialNumber, 
                                        c => c.SerialNumber, 
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);

                TeacherContractsStat.Add(contractType, contractStat);
            
            }

            AMContractsStat = new Dictionary<string, ContractStat>();

            foreach (var contractType in managerContractTypes)
            {
                var contractStat = new ContractStat();

                contractStat.Total = managerPositions.Count(p => p.ContractType.Description == contractType);

                contractStat.Offered = managerPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Offered),
                                        p => p.SerialNumber,
                                        c => c.SerialNumber,
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);

                contractStat.Accepted = managerPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Accepted),
                                        p => p.SerialNumber,
                                        c => c.SerialNumber,
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);

                contractStat.Declined = managerPositions.Join(
                                        contracts.Where(c => c.Status == ContractStatusType.Declined),
                                        p => p.SerialNumber,
                                        c => c.SerialNumber,
                                        (p, c) => p).Count(p => p.ContractType.Description == contractType);

                AMContractsStat.Add(contractType, contractStat);
            }
        
        }

        public PositionStat TeacherPositionsStat { get; set; }
        public PositionStat AMPositionsStat { get; set; }
        public CandidateStat TeacherCandidatesStat { get; set; }
        public CandidateStat AMCandidatesStat { get; set; }        

        public IDictionary<int, CandidateStat> TeacherCandidatesRatingStat { get; set; }
        public IDictionary<int, CandidateStat> AMCandidatesRatingStat { get; set; }
         
        public IDictionary<string, ContractStat> TeacherContractsStat { get; set; }
        public IDictionary<string, ContractStat> AMContractsStat { get; set; }

        #region Embeddings
        public class PositionStat
        {
            public int Total { get; set; }
            public int Filled { get; set; }
            public int Open { get; set; }
        }

        public class CandidateStat
        {
            public int Total { get; set; }
            public int Assigned { get; set; }
            public int Available { get; set; }
            public int Backup { get; set; }
            public int Rejected { get; set; }
            public int Relocated { get; set; }
        }

        public class ContractStat
        {
            public int Total { get; set; }
            public int Offered { get; set; }
            public int Accepted { get; set; }
            public int Declined { get; set; }
        }

        public class LetterStat
        {
            public int Total { get; set; }
            public int Backup { get; set; }
            public int Rejected { get; set; }
        }
        #endregion
    }

    public class CandidatesLettersModel {
        public CandidatesLettersModel(IEnumerable<Candidate> candidates) {
            CandidateTypes = ((CandidateType[])Enum.GetValues(typeof(CandidateType))).Select(candidateType =>
                new SelectListItem { Text = candidateType.ToString(), Value = candidateType.ToString() });
            GetLettersStats(candidates);
            LettersListItems = from c in candidates

                               select new CandidateLettersItem {
                                   CandidateType = c.CandidateType,
                                   HiringStatus = c.HiringStatus

                               };
        }

        private void GetLettersStats(IEnumerable<Candidate> candidates) {

            var teacherCandidates = candidates.Where(c => c.CandidateType == CandidateType.Teacher);
            var managerCandidates = candidates.Where(c => c.CandidateType == CandidateType.AcademyManager);

            NumberOfLettersForBackupTeacherCandidates = teacherCandidates.Count(c => c.HiringStatus == HiringStatus.Backup);
            NumberOfLettersForRejectedTeacherCandidates = teacherCandidates.Count(c => c.HiringStatus == HiringStatus.Rejected);
            NumberOfLettersForRelocatedTeacherCandidates = teacherCandidates.Count(c => c.Academy.TargetAreaID != c.OriginalAcademyID && c.HiringStatus == HiringStatus.Assigned);

            NumberOfLettersForBackupAMCandidates = managerCandidates.Count(c => c.HiringStatus == HiringStatus.Backup);
            NumberOfLettersForRejectedAMCandidates = managerCandidates.Count(c => c.HiringStatus == HiringStatus.Rejected);
            NumberOfLettersForRelocatedAMCandidates = managerCandidates.Count(c => c.Academy.TargetAreaID != c.OriginalAcademyID && c.HiringStatus == HiringStatus.Assigned);

        }

        public int NumberOfLettersForBackupTeacherCandidates { get; set; }
        public int NumberOfLettersForRejectedTeacherCandidates { get; set; }
        public int NumberOfLettersForRelocatedTeacherCandidates { get; set; }

        public int NumberOfLettersForBackupAMCandidates { get; set; }
        public int NumberOfLettersForRejectedAMCandidates { get; set; }
        public int NumberOfLettersForRelocatedAMCandidates { get; set; }
         

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        public IEnumerable<CandidateLettersItem> LettersListItems;

        
    }

    public class CandidatesContractTypesModel
    {

        public CandidatesContractTypesModel(
            RecruitmentCycleScoringRules recruitmentCycleScoringRules,
            IJobRepository Jr,
            ICandidateRepository Cr,
            string CandidateType,
            string ContractType,
            string acceptanceStatus = null)
        {

            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString() }
            );

            ContractTypes = Jr.GetContractTypes(AppContext.RecruitmentCycle).Select(contractType =>
                new SelectListItem { Text = contractType.Description.ToString(), Value = contractType.Name.ToString() }
            );


            var vacancies = Jr.GetPositions(AppContext.RecruitmentCycle); //.Where(r => r.CandidateID != null);
            var jobPositions = Jr.GetPositionTypes(AppContext.RecruitmentCycle);
            var contractOfferStatuses = Jr.GetAllContractStatuses(AppContext.RecruitmentCycle);
            var contractTypesAll = Jr.GetAllContractTypes();


            ContractListItems = from jp in jobPositions
                                join v in vacancies
                                on jp.PositionTypeID equals v.PositionTypeID

                                join cTyp in contractTypesAll
                                on v.ContractType.Name equals cTyp.Name

                                join cos in contractOfferStatuses
                                on v.SerialNumber equals cos.SerialNumber

                                group cos by new { v.ContractType.Name, v.PositionTypeID, candidateType = jp.CandidateType, cTyp.ContractTypeID, cTyp.Description }

                                    into fn
                                    where
                                (String.IsNullOrWhiteSpace(CandidateType) ? true : fn.Key.candidateType.ToString() == CandidateType)
                                 && (String.IsNullOrWhiteSpace(ContractType) ? true : fn.Key.Name == ContractType)
                                    select new CandidateContractListItem
                                    {
                                        ContractType = fn.Key.Name,
                                        NoOfPositions = fn.Count(),
                                        CandidateType = fn.Key.candidateType,
                                        Offered = fn.Count(r => r.Status == ContractStatusType.Offered),
                                        Accepted = fn.Count(r => r.Status == ContractStatusType.Accepted),
                                        Declined = fn.Count(r => r.Status == ContractStatusType.Declined),
                                        RecruitmentCycle = AppContext.RecruitmentCycle,
                                        PositionTypeID = fn.Key.PositionTypeID,
                                        ContractTypeID = fn.Key.ContractTypeID,
                                        ContractTypeDescription = fn.Key.Description,
                                    };

        }

        public IEnumerable<CandidateContractListItem> ContractListItems;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Contract Type:")]
        public IEnumerable<SelectListItem> ContractTypes;

    }

    public class DataDumpItem
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Show { get; set; }
        public string RecruitmentCycles { get; set; }
        public DateTime? LastRun { get; set; }
        public string LastOutcome { get; set; }

    }

    public class DataDumpModel
    {
        public DataDumpModel()
        {

            var dataDumpItems = new List<DataDumpItem>{
                    new DataDumpItem{ ID = 1, Name = "All Hired Candidates", Description = null, RecruitmentCycles = AppContext.RecruitmentCycle , Show = true},
                    new DataDumpItem{ ID = 2, Name = "All Hired AMs", Description = null, RecruitmentCycles = AppContext.RecruitmentCycle , Show = false},
                
                };

            DataDumpItems = dataDumpItems.AsEnumerable();
        }

        public readonly IEnumerable<DataDumpItem> DataDumpItems;


    }

    public class Classification
    {
        static Classification()
        {
            Classifications = new List<Classification> {
                new Classification{ Seq = 1, ClassID = "AMM" , CandidateType = CandidateType.AcademyManager, DisplayText = "Academy Manager", IsActive = true },
                new Classification{ Seq = 2, ClassID = "AMS" , CandidateType = CandidateType.AcademyManager, DisplayText = "Academy Manager Substitute", IsActive = true },
                new Classification{ Seq = 3, ClassID = "AMA" , CandidateType = CandidateType.AcademyManager, DisplayText = "Academy Manager Assistant", IsActive = true },
                new Classification{ Seq = 4, ClassID = "BA" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Baby Class)", IsActive = true },
                new Classification{ Seq = 5, ClassID = "NS" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Nursery)", IsActive = true },
                new Classification{ Seq = 6, ClassID = "PU" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Pre-Unit)", IsActive = true },
                new Classification{ Seq = 7, ClassID = "C1" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 1)", IsActive = true },
                new Classification{ Seq = 8, ClassID = "C2" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 2)", IsActive = true },
                new Classification{ Seq = 9, ClassID = "C3" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 3)", IsActive = true },
                new Classification{ Seq = 10, ClassID = "C4" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 4)", IsActive = true },
                new Classification{ Seq = 11, ClassID = "C5" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 5)", IsActive = true },
                new Classification{ Seq = 12, ClassID = "C6" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 6)", IsActive = true },
                new Classification{ Seq = 13, ClassID = "C7" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 7)", IsActive = true },
                new Classification{ Seq = 14, ClassID = "C8" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(Class 8)", IsActive = true },
                new Classification{ Seq = 15, ClassID = "SUB" , CandidateType = CandidateType.Teacher, DisplayText = "Teacher(SUB)", IsActive = true }};
        }

        public Classification()
        {
            
        }

        public int Seq { get; set; }
        public static IEnumerable<Classification> Classifications { get; set; }
        public string ClassID { get; set; }
        [Editable(false)]
        public string DisplayText { get; set; }
        public bool IsActive { get; set; }
        public CandidateType CandidateType { get; set; }
    }

    public class PositionViewModel
    {

        public PositionViewModel()
        {
            this.Classifications = Bridge.Web.Models.Classification.Classifications;
        }

        public string SchoolID { get; set; }
        public string Classification { get; set; }
        public string ClassificationDisplayText { get; set; }
        public long? CandidateID { get; set; }
        public string RecruitmentCycle { get; set; }
        public CandidateType CandidateType { get; set; }
        public long JobPositionID { get; set; }
        public string ContractType { get; set; }
        public string SerialNumber { get; set; }
        public string NationalID { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public DateTime? EndDate { get; set; }
        public string PositionStatus { get; set; }

        public IEnumerable<Classification> Classifications;

    }

    public class PositionsViewModel
    {
        public PositionsViewModel(
             IEnumerable<TargetAcademy> targertShcools,
                IEnumerable<ContractType> contractTypes,
                IEnumerable<Position> vacancies,
                IEnumerable<PositionType> jobpositions,
                IEnumerable<Candidate> candidates,
               string SchoolID,
                string selectedContractType

            )
        {



            School = from s in targertShcools
                     select new SelectListItem { Text = s.LocationDetails, Value = s.SchoolCode };

            ContractType = from ct in contractTypes
                           select new SelectListItem { Text = ct.Description, Value = ct.Name };


            this.PositionViewModel = from v in vacancies
                                  join c in candidates
                                  on v.CandidateID equals c.CandidateID
                                  into vv
                                  from cc in vv.DefaultIfEmpty(new Candidate())
                                  join j in jobpositions
                                  on v.PositionTypeID equals j.PositionTypeID
                                  join vm in new PositionViewModel().Classifications
                                  on new { Classf = v.Classification, CdateType = j.CandidateType }
                                  equals new { Classf = vm.ClassID, CdateType = vm.CandidateType }


                                  select new PositionViewModel
                                  {
                                      CandidateID = v.CandidateID,
                                      CandidateType = j.CandidateType,
                                      Classification = v.Classification,
                                      ClassificationDisplayText = vm.DisplayText,
                                      ContractType = v.ContractType.Name,
                                      StartDate = v.StartDate,
                                      EndDate = v.EndDate,
                                      PositionStatus = v.PositionStatus,
                                      JobPositionID = v.PositionTypeID,
                                      RecruitmentCycle = v.RecruitmentCycle,
                                      SchoolID = v.AcademyID,
                                      SerialNumber = v.SerialNumber,
                                      NationalID = cc.NationalID
                                  };



        }


        public IEnumerable<PositionViewModel> PositionViewModel { get; set; }
        public IEnumerable<SelectListItem> School { get; set; }
        public IEnumerable<SelectListItem> ContractType { get; set; }
    }

    public class NewPositionsSpecification
    {

        public string SchoolID { get; set; }
        public string ClassID { get; set; }
        public string ClassificationDisplayText { get; set; }
        public int RequiredPositions { get; set; }
        public string RecruitmentCycle { get; set; }
        public bool IsActive { get; set; }
        public int NoOfExistingPositions { get; set; }
        public CandidateType CandidateType { get; set; }
        public long PositionTypeID { get; set; }
        public string ContractType { get; set; }
        public string SerialNumber { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public DateTime? EndDate { get; set; }

        public int GetExtraPositionsNeeded
        {
            get
            {

                if (this.RequiredPositions - this.NoOfExistingPositions > 0)
                {
                    return this.RequiredPositions - this.NoOfExistingPositions;
                }
                else
                {
                    return 0;

                }
            }
        }
    }

    public class NewPositionViewModel
    {
        public NewPositionViewModel(
            IEnumerable<TargetAcademy> targertShcools,
            IEnumerable<ContractType> contractTypes,
            IEnumerable<Position> positions,
            IEnumerable<PositionType> jobpositions,
            string SchoolID,
            string selectedContractType
            )
        {
            this.Classifications = Bridge.Web.Models.Classification.Classifications;

            School = from s in targertShcools
                     select new SelectListItem { Text = s.LocationDetails, Value = s.TargetAreaID };

            ContractType = from ct in contractTypes
                           select new SelectListItem { Text = ct.Description, Value = ct.Name };


            if (String.IsNullOrEmpty(SchoolID) == false && String.IsNullOrEmpty(selectedContractType) == false)
            {
                NewPositionsSpecification = from c in this.Classifications
                                            join v in positions
                                            on c.ClassID equals v.Classification
                                            into mv
                                            from vs in mv.DefaultIfEmpty(new Position
                                            {
                                                Academy = null,
                                                AcademyID = SchoolID,
                                                RecruitmentCycle = AppContext.RecruitmentCycle,
                                                SerialNumber = null,
                                                Classification = c.ClassID,
                                                PositionTypeID = jobpositions.FirstOrDefault(r => r.CandidateType == c.CandidateType).PositionTypeID
                                            })
                                            where vs.AcademyID == SchoolID && c.IsActive == true
                                            group vs by new
                                            {
                                                vs.AcademyID,
                                                c.ClassID,
                                                //vs.ContractType,
                                                vs.RecruitmentCycle,
                                                JobPositionID = vs.PositionTypeID,
                                                c.DisplayText,
                                                c.IsActive,
                                                c.CandidateType
                                            }
                                                into vg
                                                select new NewPositionsSpecification
                                                {
                                                    SchoolID = vg.Key.AcademyID,
                                                    ClassID = vg.Key.ClassID,
                                                    ClassificationDisplayText = vg.Key.DisplayText,
                                                    RecruitmentCycle = vg.Key.RecruitmentCycle,
                                                    RequiredPositions = vg.Count(r => r.Academy != null),
                                                    IsActive = vg.Key.IsActive,
                                                    NoOfExistingPositions = vg.Count(r => r.Academy != null),
                                                    PositionTypeID = vg.Key.JobPositionID,
                                                    ContractType = selectedContractType,
                                                    StartDate = vg.LastOrDefault().StartDate,
                                                    EndDate = vg.LastOrDefault().EndDate,
                                                    CandidateType = vg.Key.CandidateType
                                                };
            }

            else
            {
                this.NewPositionsSpecification = new List<NewPositionsSpecification>();
            }
        }

        public IEnumerable<Classification> Classifications { get; set; }
        public IEnumerable<SelectListItem> School { get; set; }
        public IEnumerable<SelectListItem> ContractType { get; set; }
        public IEnumerable<NewPositionsSpecification> NewPositionsSpecification { get; set; }
    }

    public class WageGradeModel
    {
        public WageGradeModel(WageGrade wageGrade)
        {
            WageCategoryID = wageGrade.WageCategoryID;
            WageGradeID = wageGrade.WageGradeID;
            ContractType = wageGrade.ContractType;
            CandidateType = wageGrade.CandidateType;
            RecruitmentCycle = wageGrade.RecruitmentCycle;
            MonthlyRate = wageGrade.MonthlyRate;
            NetMonthlyRate = wageGrade.NetMonthlyRate;
            MinimumWage = wageGrade.MinimumWage;
            TwoWeeksRate = wageGrade.TwoWeeksRate;
            RegularMonths = wageGrade.RegularMonths;
            HolidayMonths = wageGrade.HolidayMonths;
            AprilTotal = wageGrade.AprilTotal;
            AugustTotal = wageGrade.AugustTotal;
            DecemberTotal = wageGrade.DecemberTotal;
            AnnualTotal = wageGrade.AnnualTotal;
            NHIF = wageGrade.NHIF;
            NSSF = wageGrade.NSSF;
            DailyRate = wageGrade.DailyRate;
            Above21DaysDailyRate = wageGrade.Above21DaysDailyRate;
        }
        public int WageCategoryID;
        public int WageGradeID;
        public string ContractType;
        public string CandidateType;
        public string RecruitmentCycle;
        public decimal MonthlyRate;
        public decimal NetMonthlyRate;
        public decimal TwoWeeksRate;

        [Required]
        public decimal RegularMonths;

        public decimal HolidayMonths;

        [Required]
        public decimal AprilTotal;

        [Required]
        public decimal AugustTotal;

        [Required]
        public decimal DecemberTotal;

        public decimal MinimumWage;
        public decimal AnnualTotal;
        public decimal NHIF;
        public decimal NSSF;
        public decimal DailyRate;
        public decimal Above21DaysDailyRate;

    }

    public class ContractLetterModel
    {
        public CandidateType CandidateType { get; set; }
        public HiringStatus HiringStatus { get; set; }
        public long? SummaryCount { get; set; }

    }

    public class WageGradesModel
    {
        public WageGradesModel(IEnumerable<WageGrade> wageGrades, IHiringService hiringService)
        {
            WageGrades = wageGrades;
            CandidateTypes = ((CandidateType[])Enum.GetValues(typeof(CandidateType))).Select(candidateType =>
                new SelectListItem { Text = candidateType.ToString(), Value = candidateType.ToString() });
            WageCategories = hiringService.GetWageCategoryIDs(AppContext.RecruitmentCycle).Select(wageCategory =>
                new SelectListItem { Text = wageCategory.ToString(), Value = wageCategory.ToString() });


        }
        public IEnumerable<WageGrade> WageGrades;

        [Display(Name = "Candidate Type:")]
        public IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Wage Category:")]
        public IEnumerable<SelectListItem> WageCategories;
    }

    public class OfferedCandidateModel
    {

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string NationalID { get; set; }
        public string LastName { get; set; }
        public CandidateType CandidateType { get; set; }
        public decimal? SummaryScore { get; set; }
        public int? StarGrade { get; set; }
        public string FullName { get; set; }
        public string FacilitatorRemarks { get; set; }
        public string RecommendedPosition { get; set; }
    }

    public static class OfferedCandidatesModelMapper
    {
        public static OfferedCandidateModel MapToOfferedCandidateModel(this Candidate domain, IScoringService scoringService)
        {
            var weeklyScoringRuleSets = scoringService.GetAllWeeklyScoringRulesForCandidateType(domain.CandidateType);

            var model = new OfferedCandidateModel
             {
                 FirstName = domain.First_Name,
                 MiddleName = domain.Middle_Name,
                 LastName = domain.Last_Name,
                 NationalID = domain.NationalID,
                 CandidateType = domain.CandidateType,
                 FullName = domain.FullName,
                 SummaryScore = scoringService.GetSummaryScore(weeklyScoringRuleSets, domain.Assessments, domain.CandidateType),
             };

            if (model.SummaryScore != null)
            {
                model.StarGrade = scoringService.GetStarForSummaryScore((decimal)model.SummaryScore, model.CandidateType);
            }
            if (domain.FacilitatorRecommendations.Count() > 0)
            {
                var recommendation = domain.FacilitatorRecommendations.LastOrDefault();
                model.RecommendedPosition = recommendation.RecommendedPosition.ToString();
                model.FacilitatorRemarks = recommendation.Remarks;
            }

            return model;
        }

    }

    public class PositionsModel
    {

        public PositionsModel(PositionFilter positionFilter, IEnumerable<Candidate> candidates, IEnumerable<Position> positions, IEnumerable<ContractStatus> candidatesContractStatus, IScoringService scoringService, RecruitmentCycleScoringRules recruitmentCycleScoringRules)
        {
            List<PositionModel> positionModel = new List<PositionModel>();
            foreach (var pos in positions)
            {
                Candidate offeredCandidate = null;
                ContractStatus offeredCandidatesContractStatus = null;

                if (pos.CandidateID != null)
                {
                    offeredCandidate = candidates.SingleOrDefault(c => c.CandidateID == pos.CandidateID);
                    offeredCandidatesContractStatus = candidatesContractStatus.SingleOrDefault(cs => cs.SerialNumber == pos.SerialNumber && cs.CandidateID == pos.CandidateID);
                }

                var model = new PositionModel
                {

                    Academy = pos.Academy.LocationDetails,
                    CandidateType = pos.PositionType.CandidateType,
                    PositionStatus = pos.PositionStatus,
                    SerialNumber = pos.SerialNumber,
                    Candidate = (offeredCandidate == null) ? null : offeredCandidate.MapToOfferedCandidateModel(scoringService),
                    ContractAcceptanceStatus = (offeredCandidatesContractStatus == null) ? string.Empty : offeredCandidatesContractStatus.Status.ToString()
                };
                positionModel.Add(model);

            }

            Positions = positionModel;

            TargetAcademies = scoringService.GetTargetAcademiesWithPositions(AppContext.RecruitmentCycle)
                .OrderBy(o => o.TargetArea).Select(t => new SelectListItem { Text = t.LocationDetails, Value = t.TargetAreaID, Selected = t.TargetAreaID == positionFilter.Academy });

            CandidateTypes = recruitmentCycleScoringRules.CandidateTypeRules.Select(candidateType =>
                new SelectListItem { Text = candidateType.CandidateType.ToString(), Value = candidateType.CandidateType.ToString(), Selected = candidateType.CandidateType == positionFilter.CandidateType }
            );

            PositionStatus = new[] {
                new SelectListItem { Value = "", Text = "All", Selected = String.IsNullOrEmpty(positionFilter.PositionStatus) },
                new SelectListItem { Value = Position.POSITION_STATUS_OPEN, Text = "Open", Selected = positionFilter.PositionStatus == Position.POSITION_STATUS_OPEN },
                new SelectListItem { Value = Position.POSITION_STATUS_FILLED, Text = "Filled", Selected = positionFilter.PositionStatus == Position.POSITION_STATUS_FILLED },
            };

        }

        public readonly IEnumerable<PositionModel> Positions;

        [Display(Name = "Academy")]
        public readonly IEnumerable<SelectListItem> TargetAcademies;

        [Display(Name = "Candidate Type:")]
        public readonly IEnumerable<SelectListItem> CandidateTypes;

        [Display(Name = "Position Status:")]
        public readonly IEnumerable<SelectListItem> PositionStatus;


    }

    public class PositionModel
    {
        public string SerialNumber { get; set; }
        public string Academy { get; set; }
        public CandidateType CandidateType { get; set; }
        public OfferedCandidateModel Candidate { get; set; }
        public string ContractAcceptanceStatus { get; set; }
        public string PositionStatus { get; set; }
    }

    public class Assignee
    {
        public string NationalID { get; set; }
        public string FullName { get; set; }
        public decimal? SummaryScore { get; set; }
        public int? Rating { get; set; }
        public string Recommendation { get; set; }
        public string Remarks { get; set; }
        public string Academy { get; set; }
        public string AcademyID { get; set; }
        public IDictionary<AssessmentType, decimal> AssessmentAverages { get; set; }
    }

    public class PositionAssignmentModel
    {
        public PositionAssignmentModel(IScoringService scoringService, IScoringRulesRepository scoringRulesRepository, Position position, IEnumerable<Candidate> candidates)
        {
            var candidateType = candidates.First().CandidateType;
            var weeklyScoringRuleSets = scoringService.GetAllWeeklyScoringRulesForCandidateType(candidateType);

            Academy = position.Academy.LocationDetails;
            AcademyID = position.AcademyID;
            SerialNumber = position.SerialNumber;
            CandidateType = candidateType.ToString();
            Classification = Bridge.Web.Models.Classification.Classifications.Single(c => c.ClassID == position.Classification).DisplayText;            
            ContractType = position.ContractType.Description;
            Recommendations = new SelectList(((RecommendedPosition[])Enum.GetValues(typeof(RecommendedPosition))).Select(r => r.ToString()));
            RecruitmentAssessmentTypes = scoringRulesRepository.GetRecruitmentScoringRules(candidateType).OrderBy(r => r.Order).Select(r => r.Type);
            TrainingAssessmentTypes = scoringRulesRepository.GetSummaryScoringRules(candidateType).Rules.OrderBy(r => r.Order).Select(r => r.Type);

            var assignees = new List<Assignee>();
            candidates = candidates.Where(c => c.HiringStatus != HiringStatus.Backup && c.HiringStatus != HiringStatus.Rejected);
            foreach (var candidate in candidates)
            {
                var summaryScore = scoringService.GetSummaryScore(weeklyScoringRuleSets, candidate.Assessments, candidate.CandidateType);

                assignees.Add(new Assignee
                {
                    Academy = candidate.Academy == null ? null : candidate.Academy.LocationDetails,
                    FullName = candidate.FullName,
                    NationalID = candidate.NationalID,
                    Recommendation = candidate.FacilitatorRecommendations.Count() > 0 ? candidate.FacilitatorRecommendations.LastOrDefault().RecommendedPosition.ToString() : null,
                    Remarks = candidate.FacilitatorRecommendations.Count() > 0 ? candidate.FacilitatorRecommendations.LastOrDefault().Remarks : null,
                    AcademyID = candidate.Academy == null ? null : candidate.Academy.TargetAreaID,
                    SummaryScore = summaryScore,
                    Rating = summaryScore.HasValue ? scoringService.GetStarForSummaryScore(summaryScore.Value, candidate.CandidateType) : null,
                    AssessmentAverages = scoringService.CalculateAssessmentAverages(candidate.Assessments, candidate.CandidateType)
                });
            }

            Assignees = assignees;
        }

        public IEnumerable<Assignee> Assignees { get; set; }

        public string Academy { get; set; }

        public string AcademyID { get; set; }

        public string SerialNumber { get; set; }

        public string CandidateType { get; set; }

        public string Classification { get; set; }

        public string ContractType { get; set; }

        public IEnumerable<SelectListItem> Recommendations { get; set; }

        public IEnumerable<AssessmentType> RecruitmentAssessmentTypes { get; set; }

        public IEnumerable<AssessmentType> TrainingAssessmentTypes { get; set; }

        public PositionFilter PositionFilter { get; set; }
    }

    public class PositionFilter
    {
        public string SerialNumber { get; set; }
        public CandidateType? CandidateType { get; set; }
        public string PositionStatus { get; set; }
        public string Academy { get; set; }
    }
}