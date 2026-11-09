using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.Core.Entities
{
    public class Policy
    {
        private decimal _coverageAmount;

        public int Id { get; set; }
        public PolicyType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal CoverageAmount
        {
            get => _coverageAmount;
            set => _coverageAmount = value > 0
                ? value
                : throw new ValidationException("Сума покриття має бути більшою за нуль!");
        }

        public decimal Cost { get; set; }
        public PolicyStatus Status { get; set; }
        public int ClientId { get; set; }
    }
}
