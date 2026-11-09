using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class ClientService
    {
        private readonly InsuranceContext _context;

        public ClientService(InsuranceContext context) => _context = context;

        public IReadOnlyList<Client> Clients => _context.Database.Clients;

        public Client Get(int id) =>
            _context.Database.Clients.FirstOrDefault(c => c.Id == id)
            ?? throw new EntityNotFoundException("Клієнт", id);

        public void Add(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            if (string.IsNullOrWhiteSpace(client.FullName))
                throw new ValidationException("ПІБ клієнта не може бути порожнім!");

            client.Id = _context.Database.NextClientId++;
            client.AgentId = null;
            _context.Database.Clients.Add(client);
            _context.Save();
        }

        public void Update(int id, string? fullName, string? contactInfo, ClientType? type)
        {
            var client = Get(id);

            if (fullName != null) client.FullName = fullName;
            if (contactInfo != null) client.ContactInfo = contactInfo;
            if (type.HasValue) client.Type = type.Value;

            _context.Save();
        }

        public void Delete(int id)
        {
            var client = Get(id);

            if (_context.Database.Policies.Any(p => p.ClientId == id))
                throw new ValidationException("Не можна видалити клієнта, у якого є поліси!");

            _context.Database.Clients.Remove(client);
            _context.Save();
        }

        public void SetAgent(int clientId, int? agentId)
        {
            var client = Get(clientId);

            if (agentId.HasValue && !_context.Database.Agents.Any(a => a.Id == agentId.Value))
                throw new EntityNotFoundException("Агент", agentId.Value);

            client.AgentId = agentId;
            _context.Save();
        }

        public int CountPolicies(int clientId) =>
            _context.Database.Policies.Count(p => p.ClientId == clientId);

        public decimal TotalPayouts(int clientId)
        {
            var policyIds = _context.Database.Policies
                .Where(p => p.ClientId == clientId)
                .Select(p => p.Id)
                .ToHashSet();

            return _context.Database.Claims
                .Where(c => c.Status == ClaimStatus.Done && policyIds.Contains(c.PolicyId))
                .Sum(c => c.PayoutAmount);
        }
    }
}
