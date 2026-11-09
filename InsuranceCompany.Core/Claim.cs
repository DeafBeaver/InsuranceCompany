using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceCompany.Core
{
    internal class Claim
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PayoutAmount { get; set; }
        public ClaimStatus Status { get; set; }
        public int PolicyId { get; set; }
    }
}
