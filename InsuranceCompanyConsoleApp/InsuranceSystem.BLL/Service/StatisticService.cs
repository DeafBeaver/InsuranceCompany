using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.BLL.Service
{
    public class StatisticService
    {
        private InsuranceContext _context;
        public StatisticService(InsuranceContext context)
        {
            _context = context;
        }
        public List<Policy> GetActivePolicies()
        {
            return _context.Database.Policies.FindAll(p => p.Status == Core.Enums.PolicyStatus.Active);
        }
        public decimal TotalPayouts() =>
            _context.Database.Payments.Where(p => p.Type == PaymentType.Payout).Sum(p => p.Amount);
        public (int PoliciesCount, decimal Commission) GetAgentEfficiency(int agentId)
        {
            var agent = _context.Database.Agents.FirstOrDefault(a => a.Id == agentId)
                ?? throw new EntityNotFoundException("Агент", agentId);

            var clientIds = _context.Database.Clients
                .Where(c => c.AgentId == agentId).Select(c => c.Id).ToHashSet();
            var policies = _context.Database.Policies
                .Where(p => clientIds.Contains(p.ClientId)).ToList();

            return (policies.Count, policies.Sum(p => p.Cost) * agent.CommissionRate);
        }
        public record AgentStats(int AgentId, string Name, int PoliciesCount, decimal Commission);

        public List<AgentStats> GetAllAgentsStats() =>
            _context.Database.Agents.Select(a =>
            {
                var (count, commission) = GetAgentEfficiency(a.Id);
                return new AgentStats(a.Id, a.Name, count, commission);
            }).ToList();

        public List<Claim> GetClaimsForPeriod(DateTime start, DateTime end)
        {
            if (end.Date < start.Date)
                throw new ValidationException("Кінцева дата раніше за початкову!");
            var endExclusive = end.Date.AddDays(1);
            return _context.Database.Claims
                .Where(c => c.Date >= start.Date && c.Date < endExclusive).ToList();
        }

        public record TypeStats(PolicyType Type, int Total, int Active, int Completed, decimal AverageCost);

        public List<TypeStats> GetTypeStats() =>
            Enum.GetValues<PolicyType>().Select(t =>
            {
                var list = _context.Database.Policies.Where(p => p.Type == t).ToList();
                return new TypeStats(t, list.Count,
                    list.Count(p => p.Status == PolicyStatus.Active),
                    list.Count(p => p.Status == PolicyStatus.Completed),
                    list.Count == 0 ? 0 : list.Average(p => p.Cost));
            }).ToList();
        public IReadOnlyList<Payment> GetPayments() => _context.Database.Payments;
        public decimal GetCompanyIncome()
        {
            var payments = _context.Database.Payments;
            return payments.Where(p => p.Type == PaymentType.Contribution).Sum(p => p.Amount)
                 - payments.Where(p => p.Type == PaymentType.Payout).Sum(p => p.Amount);
        }
    }
}
