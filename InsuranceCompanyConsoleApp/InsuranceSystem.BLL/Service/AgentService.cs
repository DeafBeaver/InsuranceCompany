using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem.BLL.Services
{
    public class AgentService
    {
        private readonly InsuranceDbContext _context;

        public AgentService(InsuranceDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Agent> Get() => _context.Agents.ToList();

        public List<Client> GetAgentsClients(int agentId) =>
            _context.Clients
                .Where(c => c.AgentId == agentId)
                .ToList();

        public Agent GetById(int id) =>
            _context.Agents.Find(id) ?? throw new EntityNotFoundException("Агент", id);

        public void Add(Agent agent)
        {
            ArgumentNullException.ThrowIfNull(agent);
            if (string.IsNullOrWhiteSpace(agent.Name))
                throw new ValidationException("Ім'я агента не може бути порожнім!");

            _context.Agents.Add(agent);
            _context.SaveChanges();
        }

        public void Update(int id, string? name, decimal? commissionRate)
        {
            var agent = GetById(id);

            if (name != null) agent.Name = name;
            if (commissionRate.HasValue) agent.CommissionRate = commissionRate.Value;

            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var agent = GetById(id);

            if (_context.Policies.Any(p => p.AgentId == id))
                throw new ValidationException("До агента прив'язані поліси.");

            var clientsToUpdate = _context.Clients.Where(c => c.AgentId == id).ToList();
            foreach (var client in clientsToUpdate)
            {
                client.AgentId = null;
            }

            _context.Agents.Remove(agent);
            _context.SaveChanges();
        }
    }
}