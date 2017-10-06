using System;
using System.Web.Mvc;
using Bridge.Domain.Recruiting;
using Bridge.Web.Models;
using Newtonsoft.Json;
using Bridge.Web.Utility;
using Bridge.Utility;
using Bridge.Services.Recruiting;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace Bridge.Web.Controllers
{
    public class AdministrationController : ApplicationControllerBase
    {
        private readonly ISetupRepository _SetupRepository;
        private readonly IHiringService HiringService;
        private readonly IJobRepository JobRepository;
        private readonly ICandidateRepository CandidateRepository;

        public AdministrationController(
             ISetupRepository setupRepository,
             IHiringService hiringService,
             IJobRepository jobRepository,
             ICandidateRepository candidateRepository)
        {
            _SetupRepository = setupRepository;
            HiringService = hiringService;
            JobRepository = jobRepository;
            CandidateRepository = candidateRepository;
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult RecruitmentCycle()
        {
            var recruitmentCycle = _SetupRepository.GetRecruitmentCycle();

            var model = new RecruitmentCyclesModel(recruitmentCycle);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult RecruitmentCycle(String qqFile) // ajaxUpload does not readily take JSON as a return type
        {
            var stream = Request.InputStream;
            var ruleFile = qqFile;
            var nextRecruitmentCycle = Request.QueryString["nextRecruitmentCycle"];
            if (String.IsNullOrEmpty(nextRecruitmentCycle))
                nextRecruitmentCycle = Request.Form["nextRecruitmentCycle"];
            var uploadedFile = RecruitmentCycleRules.GetRecruitmentCycleRulesFile(nextRecruitmentCycle);

            if (String.IsNullOrEmpty(Request["qqfile"]))
            {
                // IE
                if (Request.Files.Count > 0)
                {
                    var postedFile = Request.Files[0];
                    ruleFile = System.IO.Path.GetFileName(postedFile.FileName);
                    stream = postedFile.InputStream;
                }
                else
                {
                    return Json(new { success = false, errorMessage = "No file was received" }, "text/html");
                }
            }

            if (!ruleFile.EndsWith(".txt"))
            {
                return Json(new { success = false, errorMessage = "Invalid file type. Expected a (*.txt) file" }, "text/html");
            }

            if (stream != null && stream.Length > 0)
            {
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int) stream.Length);

                try
                {
                    var tempFileName = System.IO.Path.GetTempFileName();
                    System.IO.File.WriteAllBytes(tempFileName, buffer);
                    String configurationJson;
                    using (var configFileReader = new System.IO.StreamReader(tempFileName))
                    {
                        configurationJson = configFileReader.ReadToEnd();
                    }

                    var scoringRules = JsonConvert.DeserializeObject<RecruitmentCycleScoringRules>(configurationJson);
                    if (scoringRules == null)
                    {
                        return Json(new { success = false, errorMessage = "File contents could not be converted to scoring rules" }, "text/html");
                    }
                    else
                    {
                        var path = System.IO.Path.Combine(Server.MapPath(AppContext.CandidateScoringConfigFolder), uploadedFile);
                        System.IO.File.WriteAllBytes(path, buffer);
                    }
                }
                catch (Exception)
                {
                    return Json(new { success = false, errorMessage = "File contents could not be converted to scoring rules" }, "text/html");
                }
            }
            else
            {
                return Json(new { success = false, errorMessage = "No file was received" }, "text/html");
            }

            return Json(new { success = true, fileName = uploadedFile }, "text/html");
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonResult NextRecruitmentCycle(String recruitmentCycle)
        {
            try
            {
                _SetupRepository.CreateRecruitmentCycle(recruitmentCycle);

                return Json(recruitmentCycle);
            }
            catch (Exception)
            {
                return Json("Could not create next recruitment cycle");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public JsonResult ActivateRecruitmentCycle(String recruitmentCycle)
        {
            try
            {
                var referenceRecruitmentCycle = AppContext.RecruitmentCycle;
                AppContext.ActivateRecruitmentCycle(_SetupRepository, recruitmentCycle);
                HiringService.CloneWageGradesIfMissing(referenceRecruitmentCycle, recruitmentCycle);

                return Json(recruitmentCycle);
            }
            catch (Exception)
            {
                return Json("Could not activate next recruitment cycle");
            }
        }


        [Authorize(Roles = "Administrator")]
        public FileContentResult ScoringRuleFile(string recruitmentCycle)
        {
            var fileName = RecruitmentCycleRules.GetRecruitmentCycleRulesFile(recruitmentCycle);

            try
            {
                var path = System.IO.Path.Combine(Server.MapPath(AppContext.CandidateScoringConfigFolder), fileName);

                byte[] content = System.IO.File.ReadAllBytes(path);
                return File(content, "application/txt", fileName);
            }
            catch (Exception)
            {
                return File(new byte[0], "application/txt", fileName);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult WageGrades(string candidateType, string wageCategoryID)
        {
            var recruitmentCycle = AppContext.RecruitmentCycle;

            candidateType = (candidateType == "" || candidateType == "All") ? null : candidateType;
            wageCategoryID = (wageCategoryID == "" || wageCategoryID == "All") ? null : wageCategoryID;

            var newWageGrades = JobRepository.GetWageGrades(recruitmentCycle, candidateType, wageCategoryID);

            var model = new WageGradesModel(newWageGrades, HiringService);
            return View("~/Views/Administration/CloneWageGrades.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public ActionResult WageGrade(string candidateType, long wageCategoryID)
        {
            var wageGradeToEdit = JobRepository.GetWageGrade(AppContext.RecruitmentCycle, wageCategoryID, candidateType);
            var model = new WageGradeModel(wageGradeToEdit);
            return View("~/Views/Administration/WageGrade.cshtml", model);

        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ActionResult EditWageGrade(WageGrade wageGrade)
        {

            if (ModelState.IsValid)
            {
                HiringService.UpdateWageGrade(wageGrade);
                ModelState.Clear();
            }

            return RedirectToAction("WageGrades");
            /*var model = new WageGradeModel(wageGrade);
            return View("~/Views/Hiring/CloneWageGrades.cshtml", model);*/
        }


        [HttpGet]
        public ActionResult NewPositions(string SchoolID, string ContractType)
        {
            string recruitmentCycle = AppContext.RecruitmentCycle;
            var targetSchools = CandidateRepository.GetTargetAcademies();
            var contractTypes = this.JobRepository.GetAllContractTypes();

            var vacancies = this.JobRepository.GetPositions(recruitmentCycle)
                .Where(r =>
                    r.AcademyID == SchoolID
                    && r.ContractType.Name == ContractType)
                ;

            var jobPositions = this.JobRepository.GetPositionTypes();

            var model = new NewPositionViewModel(targetSchools, contractTypes, vacancies, jobPositions, SchoolID, ContractType);

            return View(model);
        }

        private void ParseDate(String stringDate, out DateTime parsedDate)
        {
            parsedDate = DateTime.MinValue;
            if (String.IsNullOrEmpty(stringDate)) return;

            DateTime.TryParseExact(stringDate, "d/MMM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            if (parsedDate == DateTime.MinValue)
            {
                DateTime.TryParseExact(stringDate, "d-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
            }
        }

        [HttpPost]
        public ActionResult CreateNewPositions(
            string SchoolID,
            string ContractType,
            string[] ClassID,
            int[] RequiredPositions,
            string RecruitmentCycle,
            string[] ClassificationDisplayText,
            int[] NoOfExistingPositions,
            int[] PositionTypeID,
            CandidateType[] CandidateType,
            string[] StartDate,
            string[] EndDate
            )
        {
            DateTime startDate;
            DateTime endDate;

            var requestedChanges = new List<NewPositionsSpecification>();
            for (int i = 0; i < ClassID.Length; i++)
            {
                ParseDate(StartDate[i], out startDate);
                ParseDate(EndDate[i], out endDate);

                requestedChanges.Add(new NewPositionsSpecification
                {
                    ClassID = ClassID[i],
                    RecruitmentCycle = RecruitmentCycle,
                    ClassificationDisplayText = ClassificationDisplayText[i],
                    SchoolID = SchoolID,
                    RequiredPositions = RequiredPositions[i],
                    NoOfExistingPositions = NoOfExistingPositions[i],
                    PositionTypeID = PositionTypeID[i],
                    CandidateType = CandidateType[i],
                    ContractType = ContractType,
                    StartDate = startDate.Date == DateTime.MinValue ? null : (DateTime?) startDate.Date,
                    EndDate = endDate.Date == DateTime.MinValue ? null : (DateTime?) endDate.Date,
                });

            }
            var NewPositionsToCreate = requestedChanges.Where(r => r.GetExtraPositionsNeeded != 0);

            if (!NewPositionsToCreate.Any())
            {
                return Content("<h2>No new positions to create</h2>"
                + @"<a href=""/Hiring/HiringDashboard"">Back To Hiring</a>"
                );
            }
            else
            {
                var NewPositions = new List<Position>();

                var CreatedPositions = new List<Position>();

                foreach (var item in NewPositionsToCreate)
                {

                    var lastSerialValue = this.JobRepository.GetPositionsLastSerialValue() + 1;
                    for (int i = 0; i < item.GetExtraPositionsNeeded; i++)
                    {
                        NewPositions.Add(
                            new Position
                            {
                                Academy = null,
                                AcademyID = item.SchoolID,
                                Classification = item.ClassID,
                                ContractType = (ContractType) (this.JobRepository.GetAllContractTypes().Where(r => r.Name == ContractType)).FirstOrDefault(),
                                SerialNumber = (lastSerialValue + i).ToString("0000") + "_" + SchoolID + "-" + item.ClassID + "-" + ContractType,
                                StartDate = item.StartDate,
                                EndDate = item.EndDate,
                                RecruitmentCycle = RecruitmentCycle,
                                PositionTypeID = item.PositionTypeID,
                                PositionStatus = Position.POSITION_STATUS_OPEN,
                            });
                    }

                    CreatedPositions.AddRange(this.JobRepository.AddPosition(NewPositions));

                    NewPositions.RemoveAll(r => true);
                }

                var createdPositionsModel = from newV in CreatedPositions
                                            from clf in NewPositionsToCreate
                                            where
                                            newV.AcademyID == clf.SchoolID
                                            && newV.Classification == clf.ClassID
                                            && newV.ContractType.Name == clf.ContractType
                                            select new NewPositionsSpecification
                                            {
                                                ClassID = newV.Classification,
                                                ClassificationDisplayText = clf.ClassificationDisplayText,
                                                ContractType = newV.ContractType.Name,
                                                PositionTypeID = newV.PositionTypeID,
                                                RecruitmentCycle = newV.RecruitmentCycle,
                                                RequiredPositions = clf.RequiredPositions,
                                                SchoolID = newV.AcademyID,
                                                IsActive = clf.IsActive,
                                                CandidateType = clf.CandidateType,
                                                SerialNumber = newV.SerialNumber,
                                                StartDate = newV.StartDate,
                                                EndDate = newV.EndDate,
                                            };

                ViewBag.CreatedPositionsModel = createdPositionsModel;
                return View("~/Views/Administration/NewPositionsList.cshtml", createdPositionsModel);
            }
        }

        [HttpPost]
        public ActionResult DeletePositions(List<string> ToDelete, string schoolID, string contractType)
        {

            ModelState.Clear();

            var positions = JobRepository.GetPositions(ToDelete.ToArray());
            var unassignedPositions = positions.Where(p => p.CandidateID == null);

            var assignedPositions = positions.Count() - unassignedPositions.Count();

            string ErrorMsg = null;

            switch (assignedPositions)
            {
                case 1:
                    ErrorMsg = "(" + assignedPositions + ") position could not be deleted beacuse they are assigned to candidate -  please unassign first";
                    break;
                default:
                    ErrorMsg = "(" + assignedPositions + ") positions could not be deleted because they are assigned to candidates -  please unassign first";
                    break;
                case 0:
                    break;
            }

            try
            {
                this.JobRepository.DeletePositions(unassignedPositions);
                return this.RedirectToAction("ViewPositions", new { SchoolID = schoolID, ContractType = contractType, ErrorMsg = ErrorMsg });
            }
            catch (Exception )
            {
                return Content("<b>Delete operation unsuccessful</b>");
            }
        }

        [HttpGet]
        public ActionResult ViewPositions(string SchoolID, string ContractType, string ErrorMsg, string AssignmentStatus = null)
        {

            IEnumerable<TargetAcademy> targetSchools = CandidateRepository.GetTargetAcademies();
            var contractTypes = this.JobRepository.GetAllContractTypes();

            var candidates = this.CandidateRepository.GetCandidates(AppContext.RecruitmentCycle);

            bool boolAssignmentStatus = false;
            bool.TryParse(AssignmentStatus, out boolAssignmentStatus);

            var positions = this.JobRepository.GetPositions(AppContext.RecruitmentCycle)
                .Where(r => (String.IsNullOrWhiteSpace(SchoolID) ? true : r.AcademyID == SchoolID)
                 && (String.IsNullOrWhiteSpace(ContractType) ? true : r.ContractType.Name == ContractType)
                 && (String.IsNullOrWhiteSpace(AssignmentStatus) ? true : boolAssignmentStatus == true ? r.CandidateID != null : r.CandidateID == null)
                 );

            var jobPositions = this.JobRepository.GetPositionTypes();

            ViewData.ModelState.AddModelError("AssignedPositionsDelete", ErrorMsg);

            ViewBag.AssignmentStatus = AssignmentStatus;

            var model = new PositionsViewModel(targetSchools, contractTypes, positions, jobPositions, candidates, SchoolID, ContractType);

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DataUtilities()
        {
            var ssis_Packages = new List<SSISPackageViewModel>
            {
              
                    new SSISPackageViewModel{
                        PackageID = 1,
                        PackageName = "Candidate's list Update",
                        PathName = @"~\SSIS Packages\CandidatesMigration.dtsx",
                        List = false,
                        LastOutcome = "Not run on this session",
                        Description = "Pulls newly activated candidates from Access Database",
                        ScriptType = ScriptType.SSISPackage,
                        Precedence = 30,
                    },
                  
                      new SSISPackageViewModel{
                        PackageID = 2,
                        PackageName = "Target School Update",
                        PathName = @"~\SSIS Packages\PopulateTargetAreas.dtsx",
                        List = false,
                        LastOutcome = "Not run on this session",
                        Description = "Fetch active Target Schools from Access",
                        ScriptType = ScriptType.SSISPackage,
                        Precedence = 20,
                    },
                   
                    new SSISPackageViewModel{
                        PackageID = 3,
                        PackageName = "Target School Update",
                        PathName = "MigrationMergeTargetAreas",
                        List = true,
                        LastOutcome = null,
                        Description = "Fetch active Target Schools from Access database",
                        ScriptType = ScriptType.StoredProcedure,
                        Precedence = 20,
                    },

                    new SSISPackageViewModel{
                        PackageID = 4,
                        PackageName = "Candidates Details Update",
                        PathName = "MigrationMergeCandidates",
                        List = true,
                        LastOutcome = null,
                        Description = "Update Candidate's master details",
                        ScriptType = ScriptType.StoredProcedure,
                        Precedence = 30,
                    },

                    new SSISPackageViewModel{
                        PackageID = 5,
                        PackageName = "Recruitment Scores Update",
                        PathName = "MigrationMergeRecruitmentScores",
                        List = true,
                        LastOutcome = null,
                        Description = "Update Candidate's scores during Recruitment",
                        ScriptType = ScriptType.StoredProcedure,
                        Precedence = 100,
                    },


                    new SSISPackageViewModel{
                        PackageID = 6,
                        PackageName = "Training Sites Update",
                        PathName = "MigrationMergeTrainingSites",
                        List = true,
                        LastOutcome = null,
                        Description = "Initialize new Training Sites",
                        ScriptType = ScriptType.StoredProcedure,
                        Precedence = 10,
                    },
               //return null
            };

            var model = from ssisPkg in ssis_Packages
                        where ssisPkg.List == true
                        orderby ssisPkg.Precedence
                        select ssisPkg;

            ViewBag.SSISPackageViewModel = model;

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public ViewResult Execute_SSIS_Package(SSISPackageViewModel package)
        {

            ModelState.Clear();

            var model = new List<SSISPackageViewModel> { package };

            IEnumerable<DataRoutineFeedback> fb;

            switch (package.ScriptType)
            {
                case ScriptType.StoredProcedure:
                    switch (package.PathName)
                    {
                        case "MigrationMergeTargetAreas":
                            fb = this.CandidateRepository.MigrationMergeTargetAreas();
                            model.FirstOrDefault().DataRoutineFeedback = fb;
                            model.FirstOrDefault().LastOutcome = fb.FirstOrDefault().ErrorNumber.HasValue ? "Failed" : "Successful";
                            if (fb.FirstOrDefault().ErrorNumber.HasValue) ModelState.AddModelError("SPError", fb.FirstOrDefault().ErrorMessage);
                            break;

                        case "MigrationMergeCandidates":
                            fb = this.CandidateRepository.MigrationMergeCandidates();
                            model.FirstOrDefault().DataRoutineFeedback = fb;
                            model.FirstOrDefault().LastOutcome = fb.FirstOrDefault().ErrorNumber.HasValue ? "Failed" : "Successful";
                            if (fb.FirstOrDefault().ErrorNumber.HasValue) ModelState.AddModelError("SPError", fb.FirstOrDefault().ErrorMessage);
                            break;

                        case "MigrationMergeRecruitmentScores":
                            fb = this.CandidateRepository.MigrationMergeRecruitmentScores();
                            model.FirstOrDefault().DataRoutineFeedback = fb;
                            model.FirstOrDefault().LastOutcome = fb.FirstOrDefault().ErrorNumber.HasValue ? "Failed" : "Successful";
                            if (fb.FirstOrDefault().ErrorNumber.HasValue) ModelState.AddModelError("SPError", fb.FirstOrDefault().ErrorMessage);
                            break;

                        case "MigrationMergeTrainingSites":
                            fb = this.CandidateRepository.MigrationMergeTrainingSites();
                            model.FirstOrDefault().DataRoutineFeedback = fb;
                            model.FirstOrDefault().LastOutcome = fb.FirstOrDefault().ErrorNumber.HasValue ? "Failed" : "Successful";
                            if (fb.FirstOrDefault().ErrorNumber.HasValue) ModelState.AddModelError("SPError", fb.FirstOrDefault().ErrorMessage);
                            break;

                        default:
                            break;
                    }


                    break;
                case ScriptType.SSISPackage:
                    var Pkg = new SSIS();
                    string appPath = System.Web.HttpContext.Current.Server.MapPath(package.PathName);

                    if (Pkg.ExecPackage(appPath))
                    {
                        package.LastOutcome = "Success";
                    }
                    else
                    {
                        package.LastOutcome = "Failure";
                    }


                    break;
                case ScriptType.Code:

                    break;
                default:
                    break;
            }


            package.LastExecution = DateTime.Now;





            //package.LastOutcome = fb.FirstOrDefault().ErrorNumber.HasValue ? fb.FirstOrDefault().ErrorMessage : "Successful";

            //if (model.FirstOrDefault().DataRoutineFeedback == null) model.FirstOrDefault().DataRoutineFeedback = new List<DataRoutineFeedback>();
            return View("DataUtilities", model);

        }
    }
}
