using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bridge.Domain.Recruiting;
using Bridge.Services.Recruiting;
using Bridge.Data.Recruiting;
using Bridge.Web.Models;
using Bridge.Web.Utility;
using System.Web.Security;

namespace Bridge.Web.Controllers
{
    public class HiringController : ApplicationControllerBase
    {

        private readonly ICandidateRepository CandidateRepository;
        private readonly IScoringRulesRepository ScoringRulesRepository;
        private readonly IScoringService ScoringService;
        private readonly IHiringService HiringService;
        private readonly IJobRepository JobRepository;
        
        private const string DEFAULT_CONTRACT_TYPE = "All";
        private const string DEFAULT_POSITION = "All";

        public HiringController(
             ICandidateRepository candidateRepository,
             IScoringRulesRepository scoringRulesRepository,
             IScoringService scoringService,
             IHiringService hiringService,
             IJobRepository jobRepository
            )
        {
            CandidateRepository = candidateRepository;
            ScoringRulesRepository = scoringRulesRepository;
            ScoringService = scoringService;
            HiringService = hiringService;
            JobRepository = jobRepository;

        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Offers()
        {
            string position = DEFAULT_POSITION;
            string contractType = DEFAULT_CONTRACT_TYPE;

            IEnumerable<PositionType> jobPositions = JobRepository.GetPositionTypes(AppContext.RecruitmentCycle);
            IEnumerable<ContractType> contractTypes = JobRepository.GetContractTypes(AppContext.RecruitmentCycle);
            OffersModel model = new OffersModel(AppContext.RecruitmentCycle, jobPositions, contractTypes,
                                                position, contractType);
            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractLetters()
        {
            
            OfferStatusesModel model = new OfferStatusesModel(this.CandidateRepository.GetTargetAcademies(),
                ScoringRulesRepository.GetRecruitmentCycleScoringRules());

            return View(model);
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult OffersExcelForRelocatedCandidates(string recruitmentCycle,
                                        string trainingSiteID,
                                        string position,
                                        string contractType) {
            if (recruitmentCycle == null) {
                recruitmentCycle = AppContext.RecruitmentCycle;
            }

            if (position == null) {
                position = DEFAULT_POSITION;
            }

            if (contractType == null) {
                contractType = DEFAULT_CONTRACT_TYPE;
            }

            int positionID = int.Parse(position);
            int contractTypeID = int.Parse(contractType);


            IEnumerable<WageCategory> wageCategories = JobRepository.GetWageCategories(AppContext.RecruitmentCycle);
            IEnumerable<ContractStaffCalendar> staffCalendars = JobRepository.GetContractStaffCalendars();

            var candidates = CandidateRepository.GetRelocatedCandidatesWithOffers(
                                         recruitmentCycle,
                                         trainingSiteID,
                                         positionID,
                                         contractTypeID);
            var positions = JobRepository.GetPositions(recruitmentCycle);
            var candidatePositions = from c in candidates
                                     join
                                         v in positions on c.CandidateID equals v.CandidateID
                                     select new {
                                         Position = v,
                                         Candidate = c
                                     };

            var targetSchools = this.CandidateRepository.GetTargetAcademies();


            var offers = from cp in candidatePositions
                         join cal in staffCalendars on new { Academy = cp.Position.AcademyID, Type = cp.Position.ContractType.Name } equals new { Academy = cal.SchoolID, Type = cal.ContractType }
                         into setCal
                         //using defaults if school calendar is missing  (default = null-staffCalendars.schoolID)
                         from cal4all in setCal.DefaultIfEmpty(staffCalendars.FirstOrDefault(r => r.SchoolID == null && r.ContractType == cp.Position.ContractType.Name))
                         join wc in wageCategories on cp.Position.Academy.CategoryID equals wc.CategoryID
                         join tSch in targetSchools on cp.Position.AcademyID equals tSch.TargetAreaID
                         select new {
                             Candidate = cp.Candidate,
                             Position = cp.Position,
                             Calendar = cal4all,
                             WageCategory = wc,
                             WageGrade = wc.WageGrades.First(g => g.WageCategoryID == wc.CategoryID && g.CandidateType == cp.Candidate.CandidateType.ToString()),
                             TargetAcademy = tSch,
                         };

            if (offers.ToList().Any()) {

                var rows = from o in offers
                           orderby o.Candidate.TrainingClassID, o.Candidate.FullName
                           select new {
                               ID_Number = o.Candidate.NationalID,
                               NationalID = o.Candidate.NationalID.Split("-".ToCharArray())[0],
                               Hiring_Target = o.Position.Academy.TargetArea + " - " + recruitmentCycle, // school
                               Wage_Category = o.WageCategory.CategoryID,
                               CandidateType = o.Candidate.CandidateType,
                               FullName = UtilityFunctions.ToTitleCase(o.Candidate.FullName),
                               First_Name = o.Candidate.First_Name,
                               Middle_Name = o.Candidate.Middle_Name,
                               Last_Name = o.Candidate.Last_Name,
                               Phone_Number1 = o.Candidate.PhoneNumber1,
                               Phone_Number2 = o.Candidate.PhoneNumber2,
                               Mpesa_Number = o.Candidate.MpesaNumber,
                               Mpesa_Name = o.Candidate.MpesaName,
                               PositionID = o.Position.PositionType.PositionTypeID,
                               Grade = o.Position.Classification, // position
                               Contract_Type = o.Position.ContractType.Name,
                               Contract_Start_Date = o.Position.StartDate.HasValue ? o.Position.StartDate.GetValueOrDefault().ToLongDateString() : null, // position
                               Contract_End_Date = o.Position.EndDate.HasValue ? o.Position.EndDate.GetValueOrDefault().ToLongDateString() : null,
                               End_of_Probation = o.Position.StartDate.HasValue ? o.Position.StartDate.GetValueOrDefault().AddMonths(o.Position.ContractType.ProbationPeriod).ToLongDateString() : null, // calculate - start + duration
                               Probation_Period = o.Position.ContractType.ProbationPeriod,
                               Net_Pay = o.WageGrade.NetMonthlyRate,
                               Date_Of_Birth = o.Candidate.DateofBirth == DateTime.MinValue ? "" : o.Candidate.DateofBirth.ToLongDateString(),
                               Age = (DateTime.Now.Subtract(o.Candidate.DateofBirth).Days / 365) > 100 ? "" : (DateTime.Now.Subtract(o.Candidate.DateofBirth).Days / 365).ToString(),
                               Regular_Months = o.WageGrade.RegularMonths,
                               Regular_Months_Total = (o.WageGrade.RegularMonths * 9).ToString("#"),
                               Holiday_Months = o.WageGrade.HolidayMonths,
                               Total_for_April = o.WageGrade.AprilTotal,
                               Total_for_August = o.WageGrade.AugustTotal,
                               Total_for_December = o.WageGrade.DecemberTotal,
                               Total_Annual_Amount = o.WageGrade.AnnualTotal,
                               Basic_Salary = o.WageGrade.MonthlyRate,
                               NHIF = o.WageGrade.NHIF,
                               NSSF = o.WageGrade.NSSF,
                               Community_School = o.Position.Academy.TargetArea,
                               ReportToAcademy = o.Calendar == null ? null : o.Calendar.ReportToAcademy.ToLongDateString(),
                               ADMISSION_WEEK = o.Calendar == null ? "" : o.Calendar.AdmissionWeekforCal,
                               TERM_1_for_Cal = o.Calendar == null ? "" : o.Calendar.Term1forCal,
                               Contract_PeriodStr = o.Position.ContractType.Duration,
                               Training_Site = o.Candidate.TrainingSiteID,
                               Room = o.Candidate.TrainingClassID,
                               Serial = o.Position.SerialNumber + "-" + o.WageCategory.CategoryID,
                               PositionLabel = o.Position.PositionType.Title,
                               Agreement = o.Position.PositionType.AgreementText, // job position
                               ContactStr = o.Candidate.PhoneNumber1 +
                                 ((!String.IsNullOrEmpty(o.Candidate.PhoneNumber1) && !String.IsNullOrEmpty(o.Candidate.PhoneNumber2))
                                 ? "," : "") + o.Candidate.PhoneNumber2,

                               IsTemporary = o.Position.ContractType.IsTemporary,
                               LocationDetails = o.Position.Academy.SchoolCode + " - " + o.Position.Academy.TargetArea + ", " + o.TargetAcademy.Town,
                               Sex = (o.Candidate.Gender == "M" ? "Male" : "Female"),
                               OfferedPosition = o.Position.ContractType.Name,
                               SchoolID = o.Position.AcademyID,
                               SM_Monthly_Rate = o.WageGrade.MonthlyRate,
                               SM_Net_Monthly_Rate = o.WageGrade.NetMonthlyRate,
                               Two_Weeks_Rate = o.WageGrade.TwoWeeksRate,
                               Home_Training_Site = o.Candidate.TrainingSiteID,
                               New_Training_Site = o.Candidate.TrainingSiteID,
                               SchoolName = o.Position.Academy.TargetArea,
                               Offered_Pos_BaseType = ((o.Candidate.CandidateType == CandidateType.AcademyManager)
                                                         ? "S" : "T"),
                               AM_Compensation_Table_Image = "AM Comp Table" + o.WageCategory.CategoryID + ".jpg",
                           };
                return this.Excel(rows, "Offers_" + recruitmentCycle + "_" + trainingSiteID + "_" + position + "_" + contractType + ".xlsx");
            } else {

                return Content("<h2> No positions data to extract on spreadsheet </h2>");
            }

        }



        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult OffersExcel(string recruitmentCycle,
                                        string trainingSiteID,
                                        string position,
                                        string contractType)
        {
            if (recruitmentCycle == null)
            {
                recruitmentCycle = AppContext.RecruitmentCycle;
            }

            if (position == null)
            {
                position = DEFAULT_POSITION;
            }

            if (contractType == null)
            {
                contractType = DEFAULT_CONTRACT_TYPE;
            }

            int positionID = int.Parse(position);
            int contractTypeID = int.Parse(contractType);


            IEnumerable<WageCategory> wageCategories = JobRepository.GetWageCategories(AppContext.RecruitmentCycle);
            IEnumerable<ContractStaffCalendar> staffCalendars = JobRepository.GetContractStaffCalendars();

            var candidates = CandidateRepository.GetCandidatesWithOffers(
                                         recruitmentCycle,
                                         trainingSiteID,
                                         positionID,
                                         contractTypeID);
            var positions = JobRepository.GetPositions(recruitmentCycle);
            var candidatePositions = from c in candidates
                                     join
                                         v in positions on c.CandidateID equals v.CandidateID
                                     select new
                                     {
                                         Position = v,
                                         Candidate = c
                                     };

            var targetSchools = this.CandidateRepository.GetTargetAcademies();


            var offers = from cp in candidatePositions
                         join cal in staffCalendars on new { Academy = cp.Position.AcademyID, Type = cp.Position.ContractType.Name } equals new { Academy = cal.SchoolID, Type = cal.ContractType }
                         into setCal
                         //using defaults if school calendar is missing  (default = null-staffCalendars.schoolID)
                         from cal4all in setCal.DefaultIfEmpty(staffCalendars.FirstOrDefault(r => r.SchoolID == null && r.ContractType == cp.Position.ContractType.Name))
                         join wc in wageCategories on cp.Position.Academy.CategoryID equals wc.CategoryID
                         join tSch in targetSchools on cp.Position.AcademyID equals tSch.TargetAreaID
                         select new
                         {
                             Candidate = cp.Candidate,
                             Position = cp.Position,
                             Calendar = cal4all,
                             WageCategory = wc,
                             WageGrade = wc.WageGrades.First(g => g.WageCategoryID == wc.CategoryID && g.CandidateType == cp.Candidate.CandidateType.ToString()),
                             TargetAcademy = tSch,
                         };

            if (offers.ToList().Any())
            {

                var rows = from o in offers
                           orderby o.Candidate.TrainingClassID, o.Candidate.FullName
                           select new
                           {
                               ID_Number = o.Candidate.NationalID,
                               NationalID = o.Candidate.NationalID.Split("-".ToCharArray())[0],
                               Hiring_Target = o.Position.Academy.TargetArea + " - " + recruitmentCycle, // school
                               Wage_Category = o.WageCategory.CategoryID,
                               CandidateType = o.Candidate.CandidateType,
                               FullName = UtilityFunctions.ToTitleCase(o.Candidate.FullName),
                               First_Name = o.Candidate.First_Name,
                               Middle_Name = o.Candidate.Middle_Name,
                               Last_Name = o.Candidate.Last_Name,
                               Phone_Number1 = o.Candidate.PhoneNumber1,
                               Phone_Number2 = o.Candidate.PhoneNumber2,
                               Mpesa_Number = o.Candidate.MpesaNumber,
                               Mpesa_Name = o.Candidate.MpesaName,
                               PositionID = o.Position.PositionType.PositionTypeID,
                               Grade = o.Position.Classification, // position
                               Contract_Type = o.Position.ContractType.Name,
                               Contract_Start_Date = o.Position.StartDate.HasValue ? o.Position.StartDate.GetValueOrDefault().ToLongDateString() : null, // position
                               Contract_End_Date = o.Position.EndDate.HasValue ? o.Position.EndDate.GetValueOrDefault().ToLongDateString() : null,
                               End_of_Probation = o.Position.StartDate.HasValue ? o.Position.StartDate.GetValueOrDefault().AddMonths(o.Position.ContractType.ProbationPeriod).ToLongDateString() : null, // calculate - start + duration
                               Probation_Period = o.Position.ContractType.ProbationPeriod,
                               Net_Pay = o.WageGrade.NetMonthlyRate.ToString("#"),
                               Date_Of_Birth = o.Candidate.DateofBirth == DateTime.MinValue ? "" : o.Candidate.DateofBirth.ToLongDateString(),
                               Age = (DateTime.Now.Subtract(o.Candidate.DateofBirth).Days / 365) > 100 ? "" : (DateTime.Now.Subtract(o.Candidate.DateofBirth).Days / 365).ToString(),
                               Regular_Months = o.WageGrade.RegularMonths.ToString("#"),
                               Regular_Months_Total = (o.WageGrade.RegularMonths * 9).ToString("#"),
                               Holiday_Months = o.WageGrade.HolidayMonths.ToString("#"),
                               Total_for_April = o.WageGrade.AprilTotal.ToString("#"),
                               Total_for_August = o.WageGrade.AugustTotal.ToString("#"),
                               Total_for_December = o.WageGrade.DecemberTotal.ToString("#"),
                               Total_Annual_Amount = o.WageGrade.AnnualTotal.ToString("#"),
                               Basic_Salary = o.WageGrade.MonthlyRate.ToString("#"),
                               NHIF = o.WageGrade.NHIF,
                               NSSF = o.WageGrade.NSSF,
                               Community_School = o.Position.Academy.TargetArea,
                               ReportToAcademy = o.Calendar == null ? null : o.Calendar.ReportToAcademy.ToLongDateString(),
                               ADMISSION_WEEK = o.Calendar == null ? "" : o.Calendar.AdmissionWeekforCal,
                               TERM_1_for_Cal = o.Calendar == null ? "" : o.Calendar.Term1forCal,
                               Contract_PeriodStr = o.Position.ContractType.Duration,
                               Training_Site = o.Candidate.TrainingSiteID,
                               Room = o.Candidate.TrainingClassID,
                               Serial = o.Position.SerialNumber + "-" + o.WageCategory.CategoryID,
                               PositionLabel = o.Position.PositionType.Title,
                               Agreement = o.Position.PositionType.AgreementText, // job position
                               ContactStr = o.Candidate.PhoneNumber1 +
                                 ((!String.IsNullOrEmpty(o.Candidate.PhoneNumber1) && !String.IsNullOrEmpty(o.Candidate.PhoneNumber2))
                                 ? "," : "") + o.Candidate.PhoneNumber2,

                               IsTemporary = o.Position.ContractType.IsTemporary,
                               LocationDetails = o.Position.Academy.SchoolCode + " - " + o.Position.Academy.TargetArea + ", " + o.TargetAcademy.Town,
                               Sex = (o.Candidate.Gender == "M" ? "Male" : "Female"),
                               OfferedPosition = o.Position.ContractType.Name,
                               SchoolID = o.Position.AcademyID,
                               SM_Monthly_Rate = o.WageGrade.MonthlyRate.ToString("#"),
                               SM_Net_Monthly_Rate = o.WageGrade.NetMonthlyRate.ToString("#"),
                               Two_Weeks_Rate = o.WageGrade.TwoWeeksRate.ToString("#"),
                               Home_Training_Site = o.Candidate.TrainingSiteID,
                               New_Training_Site = o.Candidate.TrainingSiteID,
                               SchoolName = o.Position.Academy.TargetArea,
                               Offered_Pos_BaseType = ((o.Candidate.CandidateType == CandidateType.AcademyManager)
                                                         ? "S" : "T"),
                               AM_Compensation_Table_Image = "AM Comp Table" + o.WageCategory.CategoryID + ".jpg",
                           };

                return this.Excel(rows, "Offers_" + recruitmentCycle + "_" + trainingSiteID + "_" + position + "_" + contractType + ".xlsx");

            }
            else
            {

                return Content("<h2> No positions data to extract on spreadsheet </h2>");
            }

        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractLettersExcel(OfferStatusesModel osm)
        {
            if (osm.RecruitmentCycle == null)
            {
                osm.RecruitmentCycle = AppContext.RecruitmentCycle;
            }

            IEnumerable<WageCategory> wageCategories = JobRepository.GetWageCategories(AppContext.RecruitmentCycle);

            IEnumerable<ContractStaffCalendar> staffCalendars = JobRepository.GetContractStaffCalendars();

            var candidatesWithOffers = CandidateRepository.GetCandidatesWithOffers(osm.RecruitmentCycle);

            var targetSchools = this.CandidateRepository.GetTargetAcademies();

            CandidateType typeofCandidate = (CandidateType)Enum.Parse(typeof(CandidateType), osm.CandidateType);

            HiringStatus candidateHiringStatus = (HiringStatus)Enum.Parse(typeof(HiringStatus), osm.OfferStatus);

            var activeCandidates = CandidateRepository.GetCandidates(osm.RecruitmentCycle, osm.TargetAcademy, typeofCandidate);

            var letterCandidates = from cds in activeCandidates.Except(candidatesWithOffers)
                                   join tSchs in targetSchools on cds.Academy.TargetAreaID equals tSchs.TargetAreaID
                                   //into sc
                                   join wc in wageCategories on tSchs.CategoryID equals wc.CategoryID
                                   where (
                                   cds.CurrentCandidateStatus == CandidateStatus.New
                                   && cds.HiringStatus == candidateHiringStatus)

                                   select new
                                   {
                                       Candidate = cds,
                                       WageCategory = wc,
                                       TargetSchools = tSchs
                                   };



            if (letterCandidates.Any())
            {
                var rows = from lc in letterCandidates
                           orderby lc.Candidate.TrainingClassID, lc.Candidate.FullName
                           select new
                           {
                               ID_Number = lc.Candidate.NationalID,
                               NationalID = lc.Candidate.NationalID.Split("-".ToCharArray())[0],
                               Wage_Category = lc.WageCategory.CategoryID,
                               CandidateType = lc.Candidate.CandidateType,
                               FullName = UtilityFunctions.ToTitleCase(lc.Candidate.FullName),
                               First_Name = lc.Candidate.First_Name,
                               Middle_Name = lc.Candidate.Middle_Name,
                               Last_Name = lc.Candidate.Last_Name,
                               Phone_Number1 = lc.Candidate.PhoneNumber1,
                               Phone_Number2 = lc.Candidate.PhoneNumber2,
                               Mpesa_Number = lc.Candidate.MpesaNumber,
                               Mpesa_Name = lc.Candidate.MpesaName,
                               PositionID = lc.Candidate.CandidateID,
                               Backup_Teachers_Daily_Rates = lc.WageCategory.BackupTeachersDailyRates.ToString("#"),
                               Backup_Academy_Manager_Daily_Rates = lc.WageCategory.BackupAcademyManagerDailyRates.ToString("#"),
                               Date_Of_Birth = lc.Candidate.DateofBirth == DateTime.MinValue ? "" : lc.Candidate.DateofBirth.ToLongDateString(),
                               Age = lc.Candidate.DateofBirth == DateTime.MinValue ? "" : (DateTime.Now.Subtract(lc.Candidate.DateofBirth).Days / 365).ToString(),
                               Serial = lc.Candidate.CandidateID + "-" + lc.WageCategory.CategoryID.ToString(),
                               ContactStr = lc.Candidate.PhoneNumber1 +
                                 ((!String.IsNullOrEmpty(lc.Candidate.PhoneNumber1) && !String.IsNullOrEmpty(lc.Candidate.PhoneNumber2))
                                 ? "," : "") + lc.Candidate.PhoneNumber2,
                               Room = lc.Candidate.TrainingClassID,
                               LocationDetails = lc.TargetSchools.SchoolCode + " - " + lc.TargetSchools.TargetArea + ", " + lc.TargetSchools.Town,
                               Sex = (lc.Candidate.Gender == "M" ? "Male" : "Female"),
                               SchoolID = lc.TargetSchools.SchoolCode,
                               Home_Training_Site = lc.Candidate.TrainingSiteID,
                               SchoolName = lc.TargetSchools.TargetArea,
                               OfferedPosition = lc.Candidate.HiringStatus,
                               Offered_Pos_BaseType = ((lc.Candidate.CandidateType == CandidateType.AcademyManager)
                                                         ? "S" : "T")
                           };

                return this.Excel(rows, osm.OfferStatus + " Letter_" + osm.RecruitmentCycle + "_" + osm.TrainingSite + "_" + osm.TargetAcademy + ".xlsx");

            }
            else
            {
                return Content("<h2> No " + osm.CandidateType + " " + osm.OfferStatus + " data to extract on spreadsheet </h2>");
            }
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractLettersExcelForRelocatedCandidates(OfferStatusesModel osm) {
            if (osm.RecruitmentCycle == null) {
                osm.RecruitmentCycle = AppContext.RecruitmentCycle;
            }

            IEnumerable<WageCategory> wageCategories = JobRepository.GetWageCategories(AppContext.RecruitmentCycle);

            IEnumerable<ContractStaffCalendar> staffCalendars = JobRepository.GetContractStaffCalendars();

            var candidatesWithOffers = CandidateRepository.GetRelocatedCandidatesWithOffers(osm.RecruitmentCycle);

            var targetSchools = this.CandidateRepository.GetTargetAcademies();

            CandidateType typeofCandidate = (CandidateType)Enum.Parse(typeof(CandidateType), osm.CandidateType);

            HiringStatus candidateHiringStatus = (HiringStatus)Enum.Parse(typeof(HiringStatus), osm.OfferStatus);

            var activeCandidates = CandidateRepository.GetCandidates(osm.RecruitmentCycle, osm.TargetAcademy, typeofCandidate);

            var letterCandidates = from cds in candidatesWithOffers
                                   join tSchs in targetSchools on cds.Academy.TargetAreaID equals tSchs.TargetAreaID
                                   //into sc
                                   join wc in wageCategories on tSchs.CategoryID equals wc.CategoryID
                                   where (
                                   cds.CurrentCandidateStatus == CandidateStatus.New
                                   && cds.HiringStatus == candidateHiringStatus)

                                   select new {
                                       Candidate = cds,
                                       WageCategory = wc,
                                       TargetSchools = tSchs
                                   };



            if (letterCandidates.Any()) {
                var rows = from lc in letterCandidates
                           orderby lc.Candidate.TrainingClassID, lc.Candidate.FullName
                           select new {
                               ID_Number = lc.Candidate.NationalID,
                               NationalID = lc.Candidate.NationalID.Split("-".ToCharArray())[0],
                               Wage_Category = lc.WageCategory.CategoryID,
                               CandidateType = lc.Candidate.CandidateType,
                               FullName = UtilityFunctions.ToTitleCase(lc.Candidate.FullName),
                               First_Name = lc.Candidate.First_Name,
                               Middle_Name = lc.Candidate.Middle_Name,
                               Last_Name = lc.Candidate.Last_Name,
                               Phone_Number1 = lc.Candidate.PhoneNumber1,
                               Phone_Number2 = lc.Candidate.PhoneNumber2,
                               Mpesa_Number = lc.Candidate.MpesaNumber,
                               Mpesa_Name = lc.Candidate.MpesaName,
                               PositionID = lc.Candidate.CandidateID,
                               Backup_Teachers_Daily_Rates = lc.WageCategory.BackupTeachersDailyRates.ToString("#"),
                               Backup_Academy_Manager_Daily_Rates = lc.WageCategory.BackupAcademyManagerDailyRates.ToString("#"),
                               Date_Of_Birth = lc.Candidate.DateofBirth == DateTime.MinValue ? "" : lc.Candidate.DateofBirth.ToLongDateString(),
                               Age = lc.Candidate.DateofBirth == DateTime.MinValue ? "" : (DateTime.Now.Subtract(lc.Candidate.DateofBirth).Days / 365).ToString(),
                               Serial = lc.Candidate.CandidateID + "-" + lc.WageCategory.CategoryID.ToString(),
                               ContactStr = lc.Candidate.PhoneNumber1 +
                                 ((!String.IsNullOrEmpty(lc.Candidate.PhoneNumber1) && !String.IsNullOrEmpty(lc.Candidate.PhoneNumber2))
                                 ? "," : "") + lc.Candidate.PhoneNumber2,
                               Room = lc.Candidate.TrainingClassID,
                               LocationDetails = lc.TargetSchools.SchoolCode + " - " + lc.TargetSchools.TargetArea + ", " + lc.TargetSchools.Town,
                               Sex = (lc.Candidate.Gender == "M" ? "Male" : "Female"),
                               SchoolID = lc.TargetSchools.SchoolCode,
                               Home_Training_Site = lc.Candidate.TrainingSiteID,
                               SchoolName = lc.TargetSchools.TargetArea,
                               OfferedPosition = lc.Candidate.HiringStatus,
                               Offered_Pos_BaseType = ((lc.Candidate.CandidateType == CandidateType.AcademyManager)
                                                         ? "S" : "T")
                           };

                return this.Excel(rows, osm.OfferStatus + " Letter_" + osm.RecruitmentCycle + "_" + osm.TrainingSite + "_" + osm.TargetAcademy + ".xlsx");

            } else {
                return Content("<h2> No " + osm.CandidateType + " " + osm.OfferStatus + " data to extract on spreadsheet </h2>");
            }
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ListDataDumpItems()
        {
            var model = new DataDumpModel().DataDumpItems
                .Where(r => r.Show == true);
            return View("~/Views/Hiring/DataDumpItems.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ExcelSystemDataDump(DataDumpItem ddi)
        {

            ModelState.Clear();

            var model = new DataDumpModel().DataDumpItems;

            model.Where(r => r.ID == ddi.ID).FirstOrDefault().LastOutcome = null;
            model.Where(r => r.ID == ddi.ID).FirstOrDefault().LastRun = DateTime.Now;


            switch (ddi.Name)
            {
                case "All Hired Candidates":


                    IEnumerable<WageCategory> wageCategories = JobRepository.GetWageCategories(AppContext.RecruitmentCycle);
                    IEnumerable<ContractStaffCalendar> staffCalendars = JobRepository.GetContractStaffCalendars();

                    var candidates = CandidateRepository.GetCandidatesWithOffers(ddi.RecruitmentCycles)
                        //.Where(r => r.AssignedVacancy.PositionStatus == "Accepted")
                        ;
                    var vacancies = JobRepository.GetPositions(ddi.RecruitmentCycles);
                    var candidatePositions = from c in candidates
                                             join
                                                 v in vacancies on c.CandidateID equals v.CandidateID
                                             select new
                                             {
                                                 Vacancy = v,
                                                 Candidate = c
                                             };

                    var targetSchools = this.CandidateRepository.GetTargetAcademies();

                    var contractStatuses = this.JobRepository.GetContractStatuses(ddi.RecruitmentCycles);


                    var offers = from cv in candidatePositions
                                 join wc in wageCategories on cv.Vacancy.Academy.CategoryID equals wc.CategoryID
                                 join tSch in targetSchools on cv.Vacancy.AcademyID equals tSch.TargetAreaID
                                 //where cs.Status == ContractStatusType.Accepted
                                 select new
                                 {
                                     Candidate = cv.Candidate,
                                     Vacancy = cv.Vacancy,
                                     WageCategory = wc,
                                     WageGrade = wc.WageGrades.First(g => g.ContractType == cv.Vacancy.ContractType.Name && g.CandidateType == cv.Candidate.CandidateType.ToString()),
                                     TargetAcademy = tSch,
                                 };



                    if (offers.ToList().Any())
                    {

                        var rows = from o in offers
                                   orderby o.Candidate.FullName
                                   select new
                                   {
                                       ID_Number = o.Candidate.NationalID,
                                       NationalID = o.Candidate.NationalID.Split("-".ToCharArray())[0],

                                       FullName = UtilityFunctions.ToTitleCase(o.Candidate.FullName),

                                       MobilePhoneNo = o.Candidate.MpesaNumber != null ? "254" + o.Candidate.MpesaNumber.ToString() : null,

                                       Standard = o.Vacancy.Classification,
                                       Contract_Type = o.Vacancy.ContractType.Name,
                                       Employment_Date = o.Vacancy.StartDate.HasValue ? o.Vacancy.StartDate.GetValueOrDefault().ToShortDateString() : null, // position

                                       Date_Of_Birth = o.Candidate.DateofBirth == DateTime.MinValue ? "" : o.Candidate.DateofBirth.ToShortDateString(),

                                       Basic_Salary = o.WageGrade.TwoWeeksRate == 0 ? o.WageGrade.MonthlyRate.ToString("#") : o.WageGrade.TwoWeeksRate.ToString("#"),
                                       NHIF = o.WageGrade.NHIF,
                                       NSSF = o.WageGrade.NSSF,
                                       Community_School = o.Vacancy.Academy.TargetArea,
                                       Serial = o.Vacancy.SerialNumber + "-" + o.WageCategory.CategoryID,
                                       JobTitle = o.Vacancy.PositionType.Title,

                                       LocationDetails = o.Vacancy.Academy.SchoolCode + " - " + o.Vacancy.Academy.TargetArea + ", " + o.TargetAcademy.Town,
                                       Gender = (o.Candidate.Gender == "M" ? "Male" : "Female"),
                                       SchoolCode = o.Vacancy.AcademyID,
                                       BaseSchoolCode = o.Vacancy.AcademyID,

                                       SchoolName = o.Vacancy.Academy.TargetArea,

                                   };
                        return this.Excel(rows, "Export_to_HR_System(" + ddi.Name + "_" + ddi.RecruitmentCycles + ").xlsx");
                    }
                    else
                    {

                        //model.Where(r => r.ID == ddi.ID).FirstOrDefault().LastOutcome = "Data not available";
                        //model.Where(r => r.ID == ddi.ID).FirstOrDefault().LastRun = DateTime.Now;

                        //return View("~/Views/Hiring/DataDumpItems.cshtml", model);

                        return Content("<h2> Data not available for (" + ddi.Name + ") </h2>");

                    }

                default:
                    return Content("<h2> Data not available for (" + ddi.Name + ") </h2>");

                //           return View("~/Views/Hiring/DataDumpItems.cshtml", model);

            }
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractStatus()
        {
            string recruitmentCycle = AppContext.RecruitmentCycle;
            var candidates = CandidateRepository.GetCandidates(recruitmentCycle, "ALL", null);
            var contractStatuses = JobRepository.GetContractStatuses(recruitmentCycle);
            var targetSchools = CandidateRepository.GetTargetAcademies();
            var model = new CandidatesContractStatusModel(candidates, contractStatuses, targetSchools, ScoringRulesRepository.GetRecruitmentCycleScoringRules());
            return View("~/Views/Hiring/ContractStatuses.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public JsonResult ChangeContractStatus(long? candidateID, string serialNumber, string contractStatus, string comment)
        {
            if (!candidateID.HasValue)
            {
                return Json("Invalid candidate ID");
            }
            ContractStatusType typeOfContractStatus = (ContractStatusType)Enum.Parse(typeof(ContractStatusType), contractStatus);

            var status = JobRepository.UpdateContractStatus(serialNumber, candidateID.Value, typeOfContractStatus, comment);

            return Json(serialNumber);
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractStatuses(string academy, string candidateType)
        {
            string recruitmentCycle = AppContext.RecruitmentCycle;
            CandidateType? typeOfCandidate;
            if (candidateType != "All")
                typeOfCandidate = (CandidateType)Enum.Parse(typeof(CandidateType), candidateType);
            else
                typeOfCandidate = null;

            var candidates = CandidateRepository.GetCandidates(recruitmentCycle, academy, typeOfCandidate);
            var contractStatuses = JobRepository.GetContractStatuses(recruitmentCycle);
            var targetSchools = CandidateRepository.GetTargetAcademies();

            var model = new CandidatesContractStatusModel(candidates, contractStatuses, targetSchools, ScoringRulesRepository.GetRecruitmentCycleScoringRules());
            return View("~/Views/Hiring/ContractStatuses.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult ContractStatusTypes(string currentStatus)
        {
            var statusTypes = from ContractStatusType type in Enum.GetValues(typeof(ContractStatusType))
                              select new SelectListItem { Text = type.ToString(), Value = type.ToString(), Selected = (type.ToString() == currentStatus) };

            return PartialView("~/Views/Hiring/ContractStatusTypes.cshtml", statusTypes);
        }


        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult GetAvailableJobPositions(string NationalID)
        {
            try
            {
                var candidate = CandidateRepository.GetCandidateByID(NationalID);
                if (candidate == null)
                {
                    throw new CandidateNotFoundException(NationalID);
                }
                var availablePositions = JobRepository.GetPositionsForAcademy(candidate.CandidateType, AppContext.RecruitmentCycle, candidate.Academy.TargetAreaID)
                    .Where(v => !v.CandidateID.HasValue
                       && v.PositionType.CandidateType == candidate.CandidateType
                       && candidate.HiringStatus == HiringStatus.Available
                       && candidate.CurrentCandidateStatus == CandidateStatus.New
                       )
                       .Select(x => new { x.SerialNumber, x.PositionType.Title });

                List<string> pieces = new List<string>();
                foreach (var position in availablePositions)
                {
                    var aPiece = "\"" + position.SerialNumber + "\":\"" + position.SerialNumber + " " + position.Title + "\"";
                    pieces.Add(aPiece);
                }
                if (pieces.Count() > 0)
                {
                    return Content("{" + String.Join(",", pieces) + "}");
                }
                return Content("");
            }
            catch (CandidateNotFoundException)
            {
                return Content("");
            }
        }

        [HttpPost]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public JsonResult AssignPosition(string serialNumber, string nationalID, decimal? relocationAmount)
        {
            try
            {
                var candidate = CandidateRepository.GetCandidateByID(nationalID);
                if (candidate == null)
                    throw new CandidateNotFoundException(nationalID);

                var position = JobRepository.GetPosition(serialNumber);
                if (position == null)
                    throw new UnknownPositionException(serialNumber);

                HiringService.AssignPosition(position, candidate, relocationAmount);
                return Json(serialNumber);
            }
            catch (CandidateNotFoundException ex)
            {
                this.Logger.Warn(ex.Message, ex);
                return Json(ex.Message);
            }
            catch (PositionAssignmentException ex)
            {
                this.Logger.Warn(ex.Message, ex);
                return Json(ex.Message);
            }
            catch (Exception ex)
            {
                this.Logger.Fatal(ex.Message, ex);
                return Json(String.Format("Unexpected error: Position ({0}) could be assigned", serialNumber));
            }
        }

        [HttpPost]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public JsonResult UnassignPosition(string serialNumber)
        {
            try
            {
                HiringService.UnassignPosition(serialNumber);
                return Json(serialNumber);
            }
            catch (Exception ex)
            {
                this.Logger.Fatal(ex.Message, ex);
                return Json(String.Format("Unexpected error: Position ({0}) could not be unassigned", serialNumber));
            }
        }

        [HttpGet]
        public ActionResult HiringStatuses(string NationalID)
        {
            string preSelectedStatus = "";
            if (false == String.IsNullOrEmpty(NationalID))
            {
                try
                {
                    var candidate = CandidateRepository.GetCandidateByID(NationalID);
                    preSelectedStatus = candidate == null ? "" : candidate.HiringStatus.ToString();
                }
                catch (CandidateNotFoundException ex)  //fail silently
                {
                    this.Logger.Warn("CandidateNotFound", ex);
                }
            }
            var status = new List<string>();
            foreach (var item in Enum.GetValues(typeof(HiringStatus)))
            {
                if (item.ToString() == preSelectedStatus)
                {
                    status.Add("\"selected\":\"" + preSelectedStatus + "\"");
                }
                status.Add("\"" + item + "\":\"" + item + "\"");

            }
            return Content("{" + string.Join(",", status) + "}");
        }

        [HttpPost]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public JsonResult ChangeHiringStatus(long candidateID, HiringStatus hiringStatus)
        {
            try
            {
                var success = HiringService.ChangeHiringStatus(candidateID, hiringStatus);
                return Json(candidateID);
            }
            catch (CandidateNotFoundException ex)
            {
                this.Logger.Warn("Candidate not Found", ex);
                return Json(ex.Message);
            }
            catch (InvalidCandidateStatusForHiringException ex)
            {
                this.Logger.Warn("InvalidCandidateStatusForHiringException", ex);
                return Json(ex.Message);
            }
            catch (HiringStatusConstraintViolationOnVacancyException ex)
            {
                this.Logger.Warn("HiringStatusConstraintViolationOnVacancyException", ex);
                return Json(ex.Message);
            }
        }        

        [HttpGet]
        [Authorize(Roles = "TrainingManager,TrainingFacilitator,Administrator")]
        public ActionResult Candidates(CandidateFilter candidateFilter)
        {
            // default to Teacher if candidateFilter.CandidateType is not set
            if (!candidateFilter.CandidateType.HasValue)
                candidateFilter.CandidateType = CandidateType.Teacher;

            var candidates = CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, null, candidateFilter.TargetAcademy, null, candidateFilter.CandidateType, candidateFilter.HiringStatus);
            var candidateIds = candidates.Select(c => c.CandidateID).ToArray();

            var contractStatuses = JobRepository.GetContractStatuses(candidateIds);

            var model = new HiringCandidatesModel(ScoringService, ScoringRulesRepository, candidates, contractStatuses, candidateFilter);

            return View("~/Views/Hiring/Candidates.cshtml", model);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Contracts(string candidateType, string contractType)
        {
            candidateType = candidateType == "All" ? null : candidateType;
            contractType = contractType == "All" ? null : contractType;

            var model = new CandidatesContractTypesModel(
                this.ScoringRulesRepository.GetRecruitmentCycleScoringRules(),
                this.JobRepository, this.CandidateRepository,
                candidateType,
                contractType
                );

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Letters(string candidateType) {

            candidateType = candidateType == "All" ? null : candidateType;

            if (candidateType == null) {
                var candidates = this.CandidateRepository.GetCandidates(AppContext.RecruitmentCycle);
                var model = new CandidatesLettersModel(candidates);
                return View("~/Views/Hiring/Letters.cshtml", model);
            } else {
                var typeOfCandidate = (CandidateType)Enum.Parse(typeof(CandidateType), candidateType);
                var candidates = this.CandidateRepository.GetCandidates(AppContext.RecruitmentCycle, null, null, typeOfCandidate);
                var model = new CandidatesLettersModel(candidates);
                return View("~/Views/Hiring/Letters.cshtml", model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult HiringDashboard(string ContractType, string Candidates_CandidateType, string Contracts_CandidateType)
        {
            var positions = this.JobRepository.GetPositions(AppContext.RecruitmentCycle);

            var candidates = this.CandidateRepository.GetCandidates(AppContext.RecruitmentCycle);
            var contracts = JobRepository.GetContractStatuses(candidates.Select(c => c.CandidateID).ToArray());

            var model = new HiringDashboardModel(ScoringService, ScoringRulesRepository, candidates, positions, contracts);

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult Positions(PositionFilter positionFilter)
        {
            var positions = JobRepository.GetPositions(AppContext.RecruitmentCycle, positionFilter.Academy, positionFilter.CandidateType, positionFilter.PositionStatus);
            var candidateIds = positions.Where(p => p.CandidateID.HasValue).Select(p => p.CandidateID.Value).ToArray();
            var candidates = CandidateRepository.GetCandidates(candidateIds);

            var contractStatuses = JobRepository.GetContractStatuses(candidateIds, positions.Select(p => p.SerialNumber).ToArray());

            var model = new PositionsModel(positionFilter, candidates, positions, contractStatuses, ScoringService, ScoringRulesRepository.GetRecruitmentCycleScoringRules());

            return View("~/Views/Hiring/Positions.cshtml", model);
        }



        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult NextPosition(PositionFilter positionFilter)
        {
            string recruitmentCycle = AppContext.RecruitmentCycle;

            var positions = JobRepository.GetPositions(recruitmentCycle, positionFilter.Academy, positionFilter.CandidateType, Position.POSITION_STATUS_OPEN); //NOTE: We can only assign the next OPEN position

            var nextPosition = positions.OrderBy(p => p.SerialNumber).FirstOrDefault();

            if (nextPosition != null)
            {
                positionFilter.SerialNumber = nextPosition.SerialNumber;
                return RedirectToAction("PositionAssignment", positionFilter);
            }
            return RedirectToAction("Positions", positionFilter);
        }

        [HttpGet]
        [Authorize(Roles = "TrainingManager,Administrator")]
        public ActionResult PositionAssignment(PositionFilter positionAssignmentFilter)
        {
            var position = JobRepository.GetPositions(new[] { positionAssignmentFilter.SerialNumber }).FirstOrDefault();
            var candidates = CandidateRepository.GetCandidatesWithoutPositions(AppContext.RecruitmentCycle, position.PositionType.CandidateType);

            //TODO: Handle scenario when there are no available candidates
            var model = new PositionAssignmentModel(ScoringService, ScoringRulesRepository, position, candidates);
            model.PositionFilter = positionAssignmentFilter;

            return View(model);
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



    }


}
