using InsuranceSystem.BLL.Service;
using InsuranceSystem.BLL.Persistence;

namespace InsuranceSystem.BLL.Services
{
    public class InsuranceSystemManager
    {
        public ClientService Clients { get; }
        public PolicyService Policies { get; }
        public ClaimService Claims { get; }
        public AgentService Agents { get; }
        public StatisticService Statistics { get; }
        public RequestService Requests { get; }

        public InsuranceSystemManager(IStorage storage)
        {
            var ctx = new InsuranceContext(storage);
            Clients = new ClientService(ctx);
            Policies = new PolicyService(ctx);
            Claims = new ClaimService(ctx);
            Agents = new AgentService(ctx);
            Statistics = new StatisticService(ctx);
            Requests = new RequestService(ctx);

            Policies.RefreshStatuses();
        }

    }
}
