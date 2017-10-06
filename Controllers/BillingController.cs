using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Bridge.Web.Models;
using Bridge.Domain.Scheduling;
using Bridge.Services.Scheduling;
using Bridge.Utility.Dates;

namespace Bridge.Web.Controllers
{
    public class BillingController : ApplicationControllerBase
    {
        ISchedulingRepository _SchedulingRepository;
        
        public BillingController(ISchedulingRepository schedulingRepository)
        {
            _SchedulingRepository = schedulingRepository;
           
        }

        [Authorize(Roles = "Helpdesk,IT Support,Administrator")]
        public ActionResult Index(BillingSMSMessageFilter messageFilter)
        {
            var model = new BillingSMSMessagesModel();

            if (!messageFilter.DateStart.HasValue)
                messageFilter.DateStart = DateTime.Now.AddMonths(-1);
            if (!messageFilter.DateEnd.HasValue)
                messageFilter.DateEnd = DateTime.Now;

            if (!string.IsNullOrEmpty(messageFilter.PupilId) || !string.IsNullOrEmpty(messageFilter.PhoneNumber))
            {
                var messages = _SchedulingRepository.GetBillingSMSMessages(messageFilter.PupilId, messageFilter.PhoneNumber, new DateRange { StartDate = messageFilter.DateStart.Value, EndDate = messageFilter.DateEnd.Value });
                
                model = new BillingSMSMessagesModel(messages);
            }
            model.SMSMessageFilter = messageFilter;

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "IT Support,Administrator")]
        public ActionResult BillingSMSJobs()
        {
            var billingSMSJobs = _SchedulingRepository.GetBillingSMSJobs();

            var model = new BillingSMSJobsModel(billingSMSJobs);

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "IT Support,Administrator")]
        public ActionResult BillingSMSJob(BillingSMSJobType SMSJobType)
        {
            var billingSMSJob = _SchedulingRepository.GetBillingSMSJobByName(SMSJobType);

            var model = new BillingSMSJobModel(billingSMSJob);
            
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "IT Support,Administrator")]
        public ActionResult BillingSMSJob(BillingSMSJobModel billingSMSJobModel)
        {
            var billingSMSJob = new BillingSMSJob
            {
                JobID = billingSMSJobModel.JobId,
                SMSJobType = billingSMSJobModel.SMSJobType,
                Active = true,
                NextRun = billingSMSJobModel.NextRun,
                Status = JobStatus.Scheduled,
                SMSTemplate = billingSMSJobModel.SMSTemplate
            };

            _SchedulingRepository.UpdateBillingSMSJob(billingSMSJob);

            return RedirectToAction("BillingSMSJobs");
        }

        [HttpGet]
        [Authorize(Roles = "IT Support,Administrator")]
        public ActionResult NewBillingSMSJob()
        {
            var billingSMSJobs = _SchedulingRepository.GetBillingSMSJobs();
                        
            var model = new NewBillingSMSJobModel(billingSMSJobs);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "IT Support,Administrator")]
        public ActionResult NewBillingSMSJob(BillingSMSJobModel billingSMSJobModel)
        {
            if (ModelState.IsValid)
            {
                var billingSMSJob = new BillingSMSJob
                {
                    JobID = billingSMSJobModel.JobId,
                    SMSJobType = billingSMSJobModel.SMSJobType,
                    Active = true,
                    NextRun = billingSMSJobModel.NextRun,
                    Status = JobStatus.Scheduled,
                    SMSTemplate = billingSMSJobModel.SMSTemplate
                };

                _SchedulingRepository.CreateBillingSMSJob(billingSMSJob);
            }
            return RedirectToAction("BillingSMSJobs");
        }
    }
}
