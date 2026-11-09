using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class AgentService
    {
        private readonly InsuranceContext _context;

        public AgentService(InsuranceContext context) => _context = context;

        public IReadOnlyList<Agent> Agents => _context.Database.Agents;

        public Agent Get(int id) =>
            _context.Database.Agents.FirstOrDefault(a => a.Id == id)
            ?? throw new EntityNotFoundException("Агент", id);

        public List<Client> GetClients(int agentId) =>
            _context.Database.Clients.Where(c => c.AgentId == agentId).ToList();

        public void Add(Agent agent)
        {
            ArgumentNullException.ThrowIfNull(agent);
            if (string.IsNullOrWhiteSpace(agent.Name))
                throw new ValidationException("Ім'я агента не може бути порожнім!");

            agent.Id = _context.Database.NextAgentId++;
            _context.Database.Agents.Add(agent);
            _context.Save();
        }

        public void Update(int id, string? name, decimal? commissionRate)
        {
            var agent = Get(id);

            if (name != null) agent.Name = name;
            if (commissionRate.HasValue) agent.CommissionRate = commissionRate.Value;

            _context.Save();
        }

        public void Delete(int id)
        {
            var agent = Get(id);

            foreach (var client in _context.Database.Clients.Where(c => c.AgentId == id))
                client.AgentId = null;

            _context.Database.Agents.Remove(agent);
            _context.Save();
        }
    }
}
