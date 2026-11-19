using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem.BLL.Services
{
    public class ClientService
    {
        private readonly InsuranceDbContext _context;

        public ClientService(InsuranceDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Client> Get() => _context.Clients.ToList();

        public Client GetById(int id) =>
            _context.Clients.Find(id) ?? throw new EntityNotFoundException("Клієнт", id);

        public void Add(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (string.IsNullOrWhiteSpace(client.FullName))
                throw new ValidationException("ПІБ клієнта не може бути порожнім!");

            client.AgentId = null;
            _context.Clients.Add(client);
            _context.SaveChanges();
        }

        public void Update(int id, string? fullName, string? contactInfo, ClientType? type)
        {
            var client = GetById(id);

            if (fullName != null) client.FullName = fullName;
            if (contactInfo != null) client.ContactInfo = contactInfo;
            if (type.HasValue) client.Type = type.Value;

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var client = GetById(id);

            if (_context.Policies.Any(p => p.ClientId == id))
                throw new ValidationException("Не можна видалити клієнта, у якого є поліси!");

            _context.Clients.Remove(client);
            _context.SaveChanges();
        }

        public void SetAgent(int clientId, int? agentId)
        {
            var client = GetById(clientId);

            if (agentId.HasValue)
            {
                var agent = _context.Agents.Find(agentId.Value)
                    ?? throw new EntityNotFoundException("Агент", agentId.Value);
                client.AgentId = agentId.Value;
            }
            else
            {
                client.AgentId = null;
            }

            _context.SaveChanges();
        }

        public int CountPolicies(int clientId)
        {
            return _context.Policies.Count(p => p.ClientId == clientId);
        }

        public decimal TotalPayouts(int clientId)
        {
            var policyIds = _context.Policies
                .Where(p => p.ClientId == clientId)
                .Select(p => p.Id)
                .ToHashSet();

            return _context.Payments
                .Where(p => p.Type == PaymentType.Payout && policyIds.Contains(p.PolicyId))
                .Sum(p => (decimal?)p.Amount) ?? 0;
        }
    }
}