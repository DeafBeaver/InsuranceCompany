using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem.BLL.Services
{
    public class PolicyService
    {
        public const decimal CostRate = 0.05m;
        private readonly InsuranceDbContext _context;

        public PolicyService(InsuranceDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Policy> Get() => _context.Policies.ToList();

        public static decimal CalculateCost(decimal coverageAmount) => coverageAmount * CostRate;

        public Policy GetById(int id) =>
            _context.Policies.Find(id) ?? throw new EntityNotFoundException("Поліс", id);

        public void Add(Policy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            if (policy.CoverageAmount <= 0)
                throw new ValidationException("Сума покриття має бути більшою за нуль!");
            ValidateDates(policy.StartDate, policy.EndDate);

            if (!_context.Clients.Any(c => c.Id == policy.ClientId))
                throw new EntityNotFoundException("Клієнт", policy.ClientId);

            policy.Cost = CalculateCost(policy.CoverageAmount);
            policy.Status = PolicyStatus.Active;

            _context.Policies.Add(policy);

            //save before to generate policy id for payment
            _context.SaveChanges();

            _context.Payments.Add(new Payment
            {
                Date = DateTime.Now,
                Amount = policy.Cost,
                Type = PaymentType.Contribution,
                PolicyId = policy.Id
            });

            _context.SaveChanges();
        }

        public void Update(int id, PolicyType? type = null, DateTime? startDate = null,
                           DateTime? endDate = null, decimal? coverageAmount = null)
        {
            var policy = GetById(id);

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

            _context.SaveChanges();
        }

        public void ChangeStatus(int id, PolicyStatus newStatus)
        {
            GetById(id).Status = newStatus;
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var policy = GetById(id);

            if (_context.Claims.Any(c => c.PolicyId == id))
                throw new ValidationException("Не можна видалити поліс, за яким є страхові випадки!");

            var payments = _context.Payments.Where(p => p.PolicyId == id).ToList();
            _context.Payments.RemoveRange(payments);

            _context.Policies.Remove(policy);
            _context.SaveChanges();
        }

        public List<Policy> Search(PolicyType? type = null, int? clientId = null,
                                   PolicyStatus? status = null,
                                   decimal? minCost = null, decimal? maxCost = null)
        {
            return _context.Policies.Where(p =>
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

            var expired = _context.Policies
                .Where(p => p.Status != PolicyStatus.Completed && p.EndDate < now)
                .ToList();

            foreach (var p in expired)
                p.Status = PolicyStatus.Completed;

            if (expired.Count > 0)
                _context.SaveChanges();

            return expired.Count;
        }
    }
}