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

        /// <summary>
        /// Gets a read-only list of all clients in the system.
        /// </summary>
        public IReadOnlyList<Client> Clients => _context.Database.Clients;

        /// <summary>
        /// Gets a specific client by their ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public Client Get(int id) =>
            _context.Database.Clients.FirstOrDefault(c => c.Id == id)
            ?? throw new EntityNotFoundException("Клієнт", id);

        /// <summary>
        /// Adds a new client to the system.
        /// </summary>
        /// <param name="client"></param>
        /// <exception cref="ValidationException"></exception>
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

        /// <summary>
        /// Updates an existing client's information. Only non-null parameters will be updated.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fullName"></param>
        /// <param name="contactInfo"></param>
        /// <param name="type"></param>
        public void Update(int id, string? fullName, string? contactInfo, ClientType? type)
        {
            var client = Get(id);

            if (fullName != null) client.FullName = fullName;
            if (contactInfo != null) client.ContactInfo = contactInfo;
            if (type.HasValue) client.Type = type.Value;

            _context.Save();
        }

        /// <summary>
        /// Deletes a client from the system. This will also delete any associated requests.
        /// </summary>
        public void Delete(int id)
        {
            var client = Get(id);

            if (_context.Database.Policies.Any(p => p.ClientId == id))
                throw new ValidationException("Не можна видалити клієнта, у якого є поліси!");

            _context.Database.Clients.Remove(client);
            _context.Database.Requests.RemoveAll(r => r.ClientId == id);
            _context.Save();
        }

        /// <summary>
        /// Assigns agent to a client. If agentId is null, it will disassociate the client from any agent.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="agentId"></param>
        /// <exception cref="EntityNotFoundException"></exception>
        public void SetAgent(int clientId, int? agentId)
        {
            var client = Get(clientId);

            if (agentId.HasValue && !_context.Database.Agents.Any(a => a.Id == agentId.Value))
                throw new EntityNotFoundException("Агент", agentId.Value);

            client.AgentId = agentId;
            _context.Save();
        }

        /// <summary>
        /// Returns the number of policies associated with a specific client.
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public int CountPolicies(int clientId) =>
            _context.Database.Policies.Count(p => p.ClientId == clientId);

        /// <summary>
        /// Returns the total amount of payouts associated with a specific client.
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public decimal TotalPayouts(int clientId)
        {
            return _context.Database.Payments
                .Where(p => p.Type == PaymentType.Payout && p.PolicyId == clientId)
                .Sum(p => p.Amount);
        }
    }
}
