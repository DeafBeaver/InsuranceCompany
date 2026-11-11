using System;
using System.Collections.Generic;
using System.Text;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.Core.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }
        public int PolicyId { get; set; }
    }
}
