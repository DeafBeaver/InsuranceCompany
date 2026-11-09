using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceCompany.Core
{
    internal class Policy
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public PolicyType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CoverageAmount { get; set; }
        public decimal Cost { get; set; }
        public PolicyStatus Status { get; set; }
        public int ClientId { get; set; }
    }
}
