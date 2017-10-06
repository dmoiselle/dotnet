using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bridge.Domain.Scheduling;
using Bridge.Services.Scheduling;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Bridge.Domain.Notifications;

namespace Bridge.Web.Models
{
    public class NewBillingSMSJobModel : BillingSMSJobModel
    {
        public NewBillingSMSJobModel(IEnumerable<BillingSMSJob> billingSMSJobs)
        {
            var existingSMSJobTypes = billingSMSJobs.Select(bsj => bsj.SMSJobType);

            var jobTypes = (BillingSMSJobType[])Enum.GetValues(typeof(BillingSMSJobType));

            SMSJobTypes = jobTypes.Where(sjt => !existingSMSJobTypes.Contains(sjt))
                .Select(sjt => new SelectListItem { Value = sjt.ToString(), Text = SMSJobTypeDescriptions[sjt] });
        }

        public IEnumerable<SelectListItem> SMSJobTypes { get; set; }
    }

    public class BillingSMSJobModel
    {
        public BillingSMSJobModel()
        { }

        protected static readonly Dictionary<BillingSMSJobType, string> SMSJobTypeDescriptions = new Dictionary<BillingSMSJobType, string> {
             {BillingSMSJobType.FirstMonthInitialBill, "First Month Initial Bill"},
             {BillingSMSJobType.FirstMonthPaymentReminder, "First Month Payment Reminder"},
             {BillingSMSJobType.FirstMonthNotAISPaymentReminder, "First Month Not AIS Payment Reminder"},
             {BillingSMSJobType.FirstMonthNotAISExtremePaymentReminder, "First Month Not AIS Extreme Payment Reminder"},
             {BillingSMSJobType.RegularMonthBill, "Regular Month Bill"},
             {BillingSMSJobType.PaymentReminder, "Payment Reminder"},
             {BillingSMSJobType.NotAISPaymentReminder, "Not AIS Payment Reminder"},
             {BillingSMSJobType.NotAISExtremePaymentReminder, "Not AIS Extreme Payment Reminder"}
        };

        public BillingSMSJobModel (BillingSMSJob billingSMSJob)
        {
            JobId = billingSMSJob.JobID;
            SMSJobType = billingSMSJob.SMSJobType;
            NextRun = billingSMSJob.NextRun;
            SMSTemplate = billingSMSJob.SMSTemplate;
            LastRun = billingSMSJob.LastRun;
            Status = billingSMSJob.Status.ToString();
            DisplayText = SMSJobTypeDescriptions[billingSMSJob.SMSJobType];
        }
        public int JobId { get; set; }
        [Required(ErrorMessage = "SMS job type is required")]
        public BillingSMSJobType SMSJobType { get; set; }
        [Required(ErrorMessage = "Date scheduled is required")]
        public DateTime NextRun { get; set; }
        [Required(ErrorMessage = "An SMS template is required")]
        public string SMSTemplate { get; set; }
        public DateTime LastRun { get; set; }
        public string Status { get; set; }
        public string DisplayText { get; set; }
    }

    public class BillingSMSJobsModel
    {
        public BillingSMSJobsModel(IEnumerable<BillingSMSJob> billingSMSJobs)
        {
            BillingSMSJobs = billingSMSJobs.Select(bsj => new BillingSMSJobModel(bsj));

            var SMSJobTypes = (BillingSMSJobType[])Enum.GetValues(typeof(BillingSMSJobType));

            ShowNewSMSJobLink = SMSJobTypes.Length != BillingSMSJobs.Count();
        }

        public IEnumerable<BillingSMSJobModel> BillingSMSJobs { get; set; }
        public bool ShowNewSMSJobLink { get; set; }
    }

    public class BillingSMSMessageModel
    {
        public string PupilId { get; set; }
        public string PhoneNumber { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; }
    }

    public class BillingSMSMessagesModel
    {
        public BillingSMSMessagesModel() {}

        public BillingSMSMessagesModel(IEnumerable<SMSMessage> messages)
        {
            SMSMessages = messages.Select(m => new BillingSMSMessageModel
            {
                PupilId = m.MessageRef,
                PhoneNumber = m.PhoneNumber,
                MessageType = m.MessageType,
                Message = m.Message,
                DateSent = m.DateSent
            });
        }

        public IEnumerable<BillingSMSMessageModel> SMSMessages { get; set; }

        public BillingSMSMessageFilter SMSMessageFilter { get; set; }
    }

    public class BillingSMSMessageFilter
    {
        public string PupilId { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}