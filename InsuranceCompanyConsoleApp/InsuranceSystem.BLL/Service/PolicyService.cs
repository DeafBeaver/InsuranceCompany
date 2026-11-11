using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class PolicyService
    {
        public const decimal CostRate = 0.05m;

        private readonly InsuranceContext _context;

        public PolicyService(InsuranceContext context) => _context = context;

        public IReadOnlyList<Policy> Policies => _context.Database.Policies;

        public static decimal CalculateCost(decimal coverageAmount) => coverageAmount * CostRate;

        public Policy Get(int id) =>
            _context.Database.Policies.FirstOrDefault(p => p.Id == id)
            ?? throw new EntityNotFoundException("Поліс", id);

        public void Add(Policy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            if (policy.CoverageAmount <= 0)
                throw new ValidationException("Сума покриття має бути більшою за нуль!");
            ValidateDates(policy.StartDate, policy.EndDate);

            if (!_context.Database.Clients.Any(c => c.Id == policy.ClientId))
                throw new EntityNotFoundException("Клієнт", policy.ClientId);

            policy.Id = _context.Database.NextPolicyId++;
            policy.Cost = CalculateCost(policy.CoverageAmount);
            policy.Status = PolicyStatus.Active;

            _context.Database.Policies.Add(policy);
            _context.Database.Payments.Add(new Payment
            {
                Id = _context.Database.NextPaymentId++,
                Date = DateTime.Now,
                Amount = policy.Cost,
                Type = PaymentType.Contribution,
                PolicyId = policy.Id
            });
            _context.Save();
        }
        public void Update(int id, PolicyType? type = null, DateTime? startDate = null,
                           DateTime? endDate = null, decimal? coverageAmount = null)
        {
            var policy = Get(id);

            var newStart = startDate ?? policy.StartDate;
            var newEnd = endDate ?? policy.EndDate;
            ValidateDates(newStart, newEnd);

            if (coverageAmount.HasValue && coverageAmount.Value <= 0)
                throw new ValidationException("Сума покриття має бути більшою за нуль!");

            if (type.HasValue) policy.Type = type.Value;
            policy.StartDate = newStart;
            policy.EndDate = newEnd;
            if (coverageAmount.HasValue) policy.CoverageAmount = coverageAmount.Value;
            policy.Cost = CalculateCost(policy.CoverageAmount);

            _context.Save();
        }

        public void ChangeStatus(int id, PolicyStatus newStatus)
        {
            Get(id).Status = newStatus;
            _context.Save();
        }

        public void Delete(int id)
        {
            var policy = Get(id);

            if (_context.Database.Claims.Any(c => c.PolicyId == id))
                throw new ValidationException("Не можна видалити поліс, за яким є страхові випадки!");

            _context.Database.Policies.Remove(policy);
            _context.Database.Payments.RemoveAll(p => p.PolicyId == id);
            _context.Save();
        }

        public List<Policy> Search(PolicyType? type = null, int? clientId = null,
                                   PolicyStatus? status = null,
                                   decimal? minCost = null, decimal? maxCost = null)
        {
            return _context.Database.Policies.Where(p =>
                (!type.HasValue || p.Type == type.Value) &&
                (!clientId.HasValue || p.ClientId == clientId.Value) &&
                (!status.HasValue || p.Status == status.Value) &&
                (!minCost.HasValue || p.Cost >= minCost.Value) &&
                (!maxCost.HasValue || p.Cost <= maxCost.Value)
            ).ToList();
        }

        private static void ValidateDates(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ValidationException("Дата завершення має бути пізнішою за дату початку!");
        }
        public int RefreshStatuses()
        {
            var now = DateTime.Now;

            var expired = _context.Database.Policies
                .Where(p => p.Status != PolicyStatus.Completed && p.EndDate < now)
                .ToList();

            foreach (var p in expired)
                p.Status = PolicyStatus.Completed;

            if (expired.Count > 0)
                _context.Save();

            return expired.Count;
        }
    }
}
