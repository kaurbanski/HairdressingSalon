using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebApplication3.Models.Validation.Attributes;

namespace WebApplication3.Models
{
    public class Visit
    {
        private Visit()
        {

        }

        [Required]
        public int Id { get; private set; }

        public DateTime StartDate { get; private set; }
        [NotMapped]
        public int Time { get; private set; } = Constants.Constants.VISIT_TIME;
        [Range(0, double.MaxValue)]
        public decimal Cost { get; private set; }
        [Range(0, double.MaxValue)]
        public decimal CostAfterDiscount { get; private set; }

        [Required]
        public int ServiceId { get; private set; }
        [Required]
        public int VisitTermId { get; private set; }
        [Required]
        public string CustomerId { get; private set; }

        public virtual Service Service { get; private set; }
        public virtual VisitTerm VisitTerm { get; private set; }
        public virtual Customer Customer { get; private set; }

        public void SetCost(decimal cost)
        {
            Cost = cost;
        }

        public void SetCostAfterDiscount(decimal costAfterDiscount)
        {
            CostAfterDiscount = costAfterDiscount;
        }

        public void SetServiceId(int serviceId)
        {
            ServiceId = serviceId;
        }

        public static Visit Create(int serviceId, int visitTermId, string customerId, DateTime startDate)
        {
            return new Visit
            {
                ServiceId = serviceId,
                VisitTermId = visitTermId,
                CustomerId = customerId,
                StartDate = startDate
            };
        }
        public static Visit Create(int id, int serviceId, int visitTermId, string customerId, DateTime startDate)
        {
            return new Visit
            {
                ServiceId = serviceId,
                VisitTermId = visitTermId,
                CustomerId = customerId,
                StartDate = startDate,
                Id = id
            };
        }
    }
}