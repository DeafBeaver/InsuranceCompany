using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceCompany.Core
{
    internal class Agent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }
    }
}
