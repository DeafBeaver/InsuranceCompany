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

        /// <summary>
        /// Retrieves a list of all policies that currently have an active status.
        /// </summary>
        /// <returns>A list of active <see cref="Policy"/> objects.</returns>
        public List<Policy> GetActivePolicies()
        {
            return _context.Database.Policies.FindAll(p => p.Status == Core.Enums.PolicyStatus.Active);
        }

        /// <summary>
        /// Calculates the total sum of all claim payouts issued across the system.
        /// </summary>
        /// <returns>The total payout amount as a <see cref="decimal"/> value.</returns>
        public decimal TotalPayouts() =>
            _context.Database.Payments.Where(p => p.Type == PaymentType.Payout).Sum(p => p.Amount);

        /// <summary>
        /// Calculates performance metrics and total earned commission for a specific agent.
        /// </summary>
        /// <param name="agentId">The unique identifier of the agent.</param>
        /// <returns>A tuple containing the total policies count and calculated commission.</returns>
        /// <exception cref="EntityNotFoundException">Thrown if the agent with the specified ID is not found.</exception>
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

        /// <summary>
        /// Represents statistical performance data for an insurance agent.
        /// </summary>
        /// <param name="AgentId">The unique identifier of the agent.</param>
        /// <param name="Name">The full name of the agent.</param>
        /// <param name="PoliciesCount">The total number of policies tied to the agent's clients.</param>
        /// <param name="Commission">The total commission earned by the agent.</param>
        public record AgentStats(int AgentId, string Name, int PoliciesCount, decimal Commission);

        /// <summary>
        /// Retrieves statistical summaries for all agents registered in the system.
        /// </summary>
        /// <returns>A list of <see cref="AgentStats"/> records representing agent efficiencies.</returns>
        public List<AgentStats> GetAllAgentsStats() =>
            _context.Database.Agents.Select(a =>
            {
                var (count, commission) = GetAgentEfficiency(a.Id);
                return new AgentStats(a.Id, a.Name, count, commission);
            }).ToList();

        /// <summary>
        /// Retrieves a list of all insurance claims registered within a specific date range.
        /// </summary>
        /// <param name="start">The start date of the period.</param>
        /// <param name="end">The end date of the period.</param>
        /// <returns>A list of claims (<see cref="List{Claim}"/>) within the given interval.</returns>
        /// <exception cref="ValidationException">Thrown if the end date is earlier than the start date.</exception>
        public List<Claim> GetClaimsForPeriod(DateTime start, DateTime end)
        {
            if (end.Date < start.Date)
                throw new ValidationException("Кінцева дата раніше за початкову!");
            var endExclusive = end.Date.AddDays(1);
            return _context.Database.Claims
                .Where(c => c.Date >= start.Date && c.Date < endExclusive).ToList();
        }

        /// <summary>
        /// Represents aggregated statistical data for a specific policy type.
        /// </summary>
        /// <param name="Type">The type of the policy.</param>
        /// <param name="Total">The total number of policies of this type.</param>
        /// <param name="Active">The number of active policies.</param>
        /// <param name="Completed">The number of completed policies.</param>
        /// <param name="AverageCost">The average cost of policies of this type.</param>
        public record TypeStats(PolicyType Type, int Total, int Active, int Completed, decimal AverageCost);

        /// <summary>
        /// Generates comprehensive statistical summaries grouped by policy types.
        /// </summary>
        /// <returns>A list of <see cref="TypeStats"/> records containing aggregated metrics per policy type.</returns>
        public List<TypeStats> GetTypeStats() =>
            Enum.GetValues<PolicyType>().Select(t =>
            {
                var list = _context.Database.Policies.Where(p => p.Type == t).ToList();
                return new TypeStats(t, list.Count,
                    list.Count(p => p.Status == PolicyStatus.Active),
                    list.Count(p => p.Status == PolicyStatus.Completed),
                    list.Count == 0 ? 0 : list.Average(p => p.Cost));
            }).ToList();

        /// <summary>
        /// Gets a read-only collection of all financial payments recorded in the system.
        /// </summary>
        public IReadOnlyList<Payment> GetPayments() => _context.Database.Payments;

        /// <summary>
        /// Calculates the net company income based on total contributions minus total payouts.
        /// </summary>
        /// <returns>The net income value as a <see cref="decimal"/>.</returns>
        public decimal GetCompanyIncome()
        {
            var payments = _context.Database.Payments;
            return payments.Where(p => p.Type == PaymentType.Contribution).Sum(p => p.Amount)
                 - payments.Where(p => p.Type == PaymentType.Payout).Sum(p => p.Amount);
        }
    }
}
