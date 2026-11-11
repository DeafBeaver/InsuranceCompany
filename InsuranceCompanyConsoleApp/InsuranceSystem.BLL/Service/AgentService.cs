using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class AgentService
    {
        private readonly InsuranceContext _context;

        public AgentService(InsuranceContext context) => _context = context;

        /// <summary>
        /// Gets a read-only collection of all Agents in the system.
        /// </summary>
        public IReadOnlyList<Agent> Agents => _context.Database.Agents;

        /// <summary>
        /// Gets an agent by their ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public Agent Get(int id) =>
            _context.Database.Agents.FirstOrDefault(a => a.Id == id)
            ?? throw new EntityNotFoundException("Агент", id);

        /// <summary>
        /// Gets a list of clients associated with a specific agent.
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public List<Client> GetClients(int agentId) =>
            _context.Database.Clients.Where(c => c.AgentId == agentId).ToList();

        /// <summary>
        /// Add a new agent to the system.
        /// </summary>
        /// <param name="agent"></param>
        /// <exception cref="ValidationException"></exception>
        public void Add(Agent agent)
        {
            ArgumentNullException.ThrowIfNull(agent);
            if (string.IsNullOrWhiteSpace(agent.Name))
                throw new ValidationException("Ім'я агента не може бути порожнім!");

            agent.Id = _context.Database.NextAgentId++;
            _context.Database.Agents.Add(agent);
            _context.Save();
        }
        /// <summary>
        /// Update an existing agent's information.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="commissionRate"></param>
        public void Update(int id, string? name, decimal? commissionRate)
        {
            var agent = Get(id);

            if (name != null) agent.Name = name;
            if (commissionRate.HasValue) agent.CommissionRate = commissionRate.Value;

            _context.Save();
        }
        /// <summary>
        /// Delete an agent from the system. This will also disassociate any clients linked to this agent.
        /// </summary>
        /// <param name="id"></param>
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
