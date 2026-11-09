using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.Core.Entities
{
    public class Claim
    {
        private decimal _payoutAmount;

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;

        public decimal PayoutAmount
        {
            get => _payoutAmount;
            set => _payoutAmount = value >= 0
                ? value
                : throw new ValidationException("Сума виплати не може бути від'ємною!");
        }

        public ClaimStatus Status { get; set; }
        public int PolicyId { get; set; }
    }
}
