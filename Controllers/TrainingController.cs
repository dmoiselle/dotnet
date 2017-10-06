using Bridge.Data.Recruiting;
using Bridge.Domain.Recruiting;
using Bridge.Services.Recruiting;
using Bridge.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using Bridge.Web.Utility;
using System.IO;

namespace Bridge.Web.Controllers
{
    public class TrainingController : ApplicationControllerBase
    {
        public TrainingController(
            ICandidateRepository candidateRepository,
            IJobRepository jobRepository,
            IScoringRulesRepository scoringRulesRepository,
            IScoringService scoringService)
        {
            CandidateRepository = candidateRepository;
            JobRepository = jobRepository;
            ScoringRulesRepository = scoringRulesRepository;
            ScoringService = scoringService;
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult Index()
        {
            return Candidates(null, null, CandidateType.Teacher);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult Candidates(string trainingSiteID, string trainingClassID, CandidateType? candidateType)
        {

            if (!candidateType.HasValue)
            {
                candidateType = CandidateType.Teacher;
            }

            try
            {
                var candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, trainingSiteID, trainingClassID, candidateType.Value);
                var trainingSites = CandidateRepository.GetTrainingSites();
                var trainingClasses = CandidateRepository.GetTrainingClasses(AppContext.RecruitmentCycle);
                var model = new CandidatesModel(candidates, trainingSites, trainingClasses, ScoringService, ScoringRulesRepository.GetRecruitmentCycleScoringRules());

                return View("~/Views/Training/Candidates.cshtml", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult Assessments(string candidateType, string trainingClass, int? typeNumber, int? weekNumber, string assessmentType)
        {

            if ((!String.IsNullOrEmpty(candidateType)) && (!String.IsNullOrEmpty(assessmentType)))
            {
                CandidateType typeOfCandidate = (CandidateType) Enum.Parse(typeof(CandidateType), candidateType);
                AssessmentType typeOfAssessment = (AssessmentType) Enum.Parse(typeof(AssessmentType), assessmentType);

                var candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, trainingClass, typeOfCandidate);
                var model = new AssessmentsModel(candidates, typeOfAssessment, typeNumber, (int) weekNumber, ScoringRulesRepository.GetRecruitmentCycleScoringRules());
                return View(model);
            }

            var defaultModel = new AssessmentsModel(ScoringRulesRepository.GetRecruitmentCycleScoringRules());

            var trainingClasses = CandidateRepository.GetTrainingClasses(AppContext.RecruitmentCycle);

            defaultModel.TrainingClasses = trainingClasses.Select(tc => new SelectListItem { Value = tc, Text = tc });

            return View(defaultModel);
        }

        [HttpPost]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public JsonResult Assessment(CandidateType candidateType, Assessment assessment)
        {
            try
            {
                var scoringRule = ScoringRulesRepository.GetScoringRuleForAssessment(candidateType, assessment.AssessmentType, assessment.WeekNumber, assessment.TypeNumber);
                if (assessment.RawTestScore > scoringRule.MaximumScore || assessment.RawTestScore < scoringRule.MinimumScore)
                {
                    throw new RawScoreOutOfRangeException(assessment.RawTestScore);
                }

                assessment.Source = Bridge.Domain.Recruiting.Assessment.ASSESSMENT_SOURCE_TRAINING;

                if (assessment.AssessmentID == 0)
                {
                    assessment = CandidateRepository.AddAssessments(new[] { assessment }).First();
                }
                else
                {
                    CandidateRepository.UpdateAssessmentRawTestScore(assessment.AssessmentID, assessment.RawTestScore);
                }

                return Json(new { success = true, AssessmentID = assessment.AssessmentID });

            }
            catch (RawScoreOutOfRangeException ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Unexpected error: Assessment score could not be updated" });
            }

        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult PartialAssessments(string trainingClass, string candidateType, int? typeNumber, int? weekNumber, string assessmentType)
        {
            if ((!String.IsNullOrEmpty(candidateType)) && (!String.IsNullOrEmpty(assessmentType)))
            {
                CandidateType typeOfCandidate = (CandidateType) Enum.Parse(typeof(CandidateType), candidateType);
                AssessmentType typeOfAssessment = (AssessmentType) Enum.Parse(typeof(AssessmentType), assessmentType);

                var candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, trainingClass, typeOfCandidate);

                var model = new AssessmentsModel(candidates, typeOfAssessment, typeNumber, (int) weekNumber, ScoringRulesRepository.GetRecruitmentCycleScoringRules());

                return PartialView(model);
            }

            var defaultModel = new AssessmentsModel(ScoringRulesRepository.GetRecruitmentCycleScoringRules());

            return PartialView(defaultModel);

        }

        public ActionResult AssessmentDelete(long id, string Name)
        {
            ViewBag.Name = Name;

            Assessment assessment = CandidateRepository.GetAssessment(id);
            return View(assessment);
        }

        [HttpPost, ActionName("AssessmentDelete")]
        public ActionResult AssessmentDeleteConfirmed(long id)
        {
            bool IsSuccessful = CandidateRepository.DeleteAssessment(id);
            if (IsSuccessful == true)
            {
                return RedirectToAction("Assessments");
            }
            else
            {
                return RedirectToAction("Assessments");
            }
        }

        [HttpPost, ActionName("CandidateStatusInsert")]
        public ActionResult CandidateStatusInsert(CandidateViewModel candidateView)
        {
            throw new NotImplementedException();
        }

        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Finalize()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CandidateProfile(string id)
        {

            try
            {
                var _candidate = CandidateRepository.GetCandidateByID(id);
                var _candidateID = _candidate.CandidateID;
                var position = JobRepository.GetPositions(new[] { _candidateID }).SingleOrDefault();
                var _candidateStatusList = CandidateRepository.GetCandidateHistoryByCandidateID(_candidateID).OrderByDescending(c => c.StatusDate);

                TargetAcademy originalAcademy = _candidate.Academy;

                if (!String.IsNullOrEmpty(_candidate.OriginalAcademyID))
                {
                    originalAcademy = CandidateRepository.GetTargetAcademy(_candidate.OriginalAcademyID);
                }

                var _model = new CandidateViewModel(_candidate, originalAcademy, position, ScoringService, ScoringRulesRepository.GetRecruitmentCycleScoringRules());
                return View("~/Views/Training/CandidateProfile.cshtml", _model);
            }
            catch (CandidateNotFoundException ex)
            {
                ModelState.AddModelError("CandidateNotFoundException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                Redirect(Request.UrlReferrer.ToString());
                return null;
            }
        }

        [HttpPost]
        public ActionResult UpdateCandidateStatus(string id, string CandidateID, string Notes, string CandidateStatusSelect)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CandidateStatus candidateStatus = (CandidateStatus) Enum.Parse(typeof(CandidateStatus), CandidateStatusSelect);
                    var candidate = CandidateRepository.GetCandidateByID(id);
                    var insertHistory = CandidateRepository.InsertCandidateStatus(candidate.CandidateID, candidateStatus, Notes, AppContext.RecruitmentCycle);

                    candidate = CandidateRepository.GetCandidateByID(id);
                    var position = JobRepository.GetPositions(new[] { candidate.CandidateID }).SingleOrDefault();
                    TargetAcademy originalAcademy = candidate.Academy;

                    if (!String.IsNullOrEmpty(candidate.OriginalAcademyID))
                    {
                        originalAcademy = CandidateRepository.GetTargetAcademy(candidate.OriginalAcademyID);
                    }

                    if (insertHistory == true)
                    {
                        var _model = new CandidateViewModel(candidate, originalAcademy, position, ScoringService, ScoringRulesRepository.GetRecruitmentCycleScoringRules());
                        return View("~/Views/Training/CandidateProfile.cshtml", _model);
                    }

                }
                catch (UnknownNationalIDException)
                {
                    //error handling here
                    throw;
                    //   return null;
                }
                catch (CandidateHistoryNotInsertedException ex)
                {
                    ModelState.AddModelError("CandidateHistoryNotInsertedException", ex.Message);
                    this.Logger.Warn(ex.Message, ex);
                    //Redirect(Request.UrlReferrer.ToString());
                    throw;
                    //return null;
                }

            }
            else
            {
                throw new NotImplementedException("model state is invalid for insert candidate score");
            }

            return null;
        }



        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult ChangeCandidateStatus(string id)
        {
            try
            {
                var candidate = CandidateRepository.GetCandidateByID(id);

                var _model = new CandidateHistoryEntryModel(candidate);
                //return PartialView(_model);
                return View(_model);
            }
            catch (CandidateNotFoundException ex)
            {
                ModelState.AddModelError("CandidateNotFoundException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                Redirect(Request.UrlReferrer.ToString());
                return null;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("InvalidOperationException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                Redirect(Request.UrlReferrer.ToString());
                return null;
            }
        }


        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult QuickEditAssessmentScore(long id, decimal value)
        {
            try
            {
                var assessment = ScoringService.GetAssesssmentUsingAssessmentID(id);
                var candidate = CandidateRepository.GetCandidateByCandidateID(assessment.CandidateID);
                bool success = ScoringService.UpdateAssessmentScore(candidate.CandidateType, id, value);
                if (success == true)
                {
                    var model = new AssessmentModel(value);
                    return PartialView(model);
                }
                else
                {
                    var model = new AssessmentModel();
                }
            }
            catch (UnknownAssessmentException ex)
            {
                ModelState.AddModelError("AssessmentIsUnknownException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
            }
            catch (RawScoreOutOfRangeException ex)
            {
                ModelState.AddModelError("RawScoreOutOfRangeException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
            }
            return PartialView();
        }

        [Authorize(Roles = "TrainingManager,TrainingFacilitator")]
        public ActionResult NewCandidate()
        {
            return null;
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult FacilitatorRecommendations(string trainingSiteID, string trainingClassID, CandidateType? candidateType)
        {

            if (!candidateType.HasValue)
            {
                candidateType = CandidateType.Teacher;
            }

            var candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, trainingSiteID, trainingClassID, candidateType.Value);
            var trainingSites = CandidateRepository.GetTrainingSites();
            var trainingClasses = CandidateRepository.GetTrainingClasses(AppContext.RecruitmentCycle);
            var model = new FacilitatorRecommendationsModel(candidates, candidateType.Value,
                                                    trainingSites, trainingClasses, ScoringService,
                                                    ScoringRulesRepository.GetRecruitmentCycleScoringRules());
            return View(model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult EditFacilitatorRecommendedPosition(string nationalID, string recommendedPosition)
        {
            try
            {
                RecommendedPosition recClass;
                if (!Enum.TryParse(recommendedPosition, out recClass))
                {
                    this.Logger.Warn("Invalid class " + recommendedPosition);
                    return Content("Update Error. Invalid recommendation class");
                }

                Candidate candidate = CandidateRepository.GetCandidateByID(nationalID);
                if (candidate == null)
                {
                    this.Logger.Warn("Candidate with nationalID " + nationalID + " not found");
                    return Content("Update Error. NationalID not found");
                }

                FacilitatorRecommendation rec = candidate.FacilitatorRecommendations.FirstOrDefault(r => r.RecruitmentCycleID == AppContext.RecruitmentCycle);

                if (rec == null)
                {
                    rec = new FacilitatorRecommendation()
                    {
                        RecruitmentCycleID = AppContext.RecruitmentCycle,
                        CandidateID = candidate.CandidateID,
                        RecommendedPosition = recClass,
                        Remarks = ""
                    };
                    CandidateRepository.InsertFacilitatorRecommendation(candidate, rec);
                }
                else
                {
                    rec.RecommendedPosition = recClass;
                    CandidateRepository.UpdateFacilitatorRecommendation(candidate, rec);
                }
                return Content(CandidateModel.RecommendedPositionToString(recClass, candidate.CandidateType)); // jEditable wants us to return same thing that was edited

            }
            catch (FacilitatorRecommendationNotFoundException ex) //? When does this get thrown?
            {
                ModelState.AddModelError("FacilitatorRecommendationNotFoundException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                return Content("Update Error. Facilitator recommendation not found");
            }
            catch (InvalidRecommendedPositionForCandidateTypeException ex)
            {
                ModelState.AddModelError("InvalidClassValidForCandidateTypeException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                return Content("Update Error. Invalid class for candidate");
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult EditFacilitatorRemarks(string nationalID, string remarks)
        {
            try
            {
                Candidate candidate = CandidateRepository.GetCandidateByID(nationalID);

                if (candidate == null)
                {
                    this.Logger.Warn("Candidate with nationalID " + nationalID + " not found");
                    return Content("Update Error. NationalID not found");
                }
                FacilitatorRecommendation rec = candidate.FacilitatorRecommendations.FirstOrDefault(r => r.RecruitmentCycleID == AppContext.RecruitmentCycle);

                if (rec == null)
                {
                    rec = new FacilitatorRecommendation()
                    {
                        RecruitmentCycleID = AppContext.RecruitmentCycle,
                        CandidateID = candidate.CandidateID,
                        RecommendedPosition = RecommendedPosition.None,
                        Remarks = remarks
                    };
                    CandidateRepository.InsertFacilitatorRecommendation(candidate, rec);
                }
                else
                {
                    rec.Remarks = remarks;
                    CandidateRepository.UpdateFacilitatorRecommendation(candidate, rec);
                }
                return Content(remarks); // jEditable wants us to return same thing that was edited

            }
            catch (FacilitatorRecommendationNotFoundException ex) //? When does this get thrown?
            {
                ModelState.AddModelError("FacilitatorRecommendationNotFoundException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                return Content("Update Error. Facilitator recommendation not found");
            }
            catch (InvalidRecommendedPositionForCandidateTypeException ex)
            {
                ModelState.AddModelError("InvalidClassValidForCandidateTypeException", ex.Message);
                this.Logger.Warn(ex.Message, ex);
                return Content("Update Error. Invalid class for candidate");
            }
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult ExcelTest()
        {
            IEnumerable<Candidate> candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, null, null, CandidateType.Teacher);
            var test = from candidate in candidates
                       select new
                       {
                           NationalID = candidate.NationalID,
                           Name = candidate.First_Name + " " + candidate.Middle_Name + " " + candidate.Last_Name,
                           Status = candidate.CurrentCandidateStatus,
                           Type = candidate.CandidateType,
                           Gender = candidate.Gender
                       };
            return this.Excel(test, "ExcelTest.xlsx");
        }

        private readonly ICandidateRepository CandidateRepository;
        private readonly IJobRepository JobRepository;
        private readonly IScoringRulesRepository ScoringRulesRepository;
        private readonly IScoringService ScoringService;

    }
}
