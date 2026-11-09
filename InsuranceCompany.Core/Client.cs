using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceCompany.Core
{
    internal class Client
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ContactInfo { get; set; }
        public ClientType Type { get; set; }
        public int PoliciesCount { get; set; }
        public decimal TotalPayouts { get; set; }
    }
}
