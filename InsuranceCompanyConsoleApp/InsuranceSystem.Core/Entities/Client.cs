using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.Core.Entities
{
    public class Client
    {
        private string _fullName = string.Empty;

        public int Id { get; set; }

        public string FullName
        {
            get => _fullName;
            set => _fullName = !string.IsNullOrWhiteSpace(value)
                ? value.Trim()
                : throw new ValidationException("ПІБ клієнта не може бути порожнім!");
        }

        public string ContactInfo { get; set; } = string.Empty;
        public ClientType Type { get; set; }
        public int? AgentId { get; set; }
    }
}
