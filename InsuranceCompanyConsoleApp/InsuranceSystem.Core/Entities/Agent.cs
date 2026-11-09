using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.Core.Entities
{
    public class Agent
    {
        private string _name = string.Empty;
        private decimal _commissionRate;

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set => _name = !string.IsNullOrWhiteSpace(value)
                ? value.Trim()
                : throw new ValidationException("Ім'я агента не може бути порожнім!");
        }

        public decimal CommissionRate
        {
            get => _commissionRate;
            set => _commissionRate = value >= 0 && value <= 1
                ? value
                : throw new ValidationException("Комісія має бути від 0 до 1 (наприклад, 0.15)!");
        }
    }
}
