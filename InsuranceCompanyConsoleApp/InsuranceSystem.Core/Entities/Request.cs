using InsuranceSystem.Core.Enum;
using InsuranceSystem.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.Core.Entities
{
    public class Request
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public PolicyType Type { get; set; }
        public decimal DesiredCoverage { get; set; }
        public int TermMonths { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
