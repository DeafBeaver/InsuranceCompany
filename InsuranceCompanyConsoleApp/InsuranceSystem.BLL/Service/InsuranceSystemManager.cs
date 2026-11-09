using InsuranceSystem.BLL.Persistence;

namespace InsuranceSystem.BLL.Services
{
    public class InsuranceSystemManager
    {
        public ClientService Clients { get; }
        public PolicyService Policies { get; }
        public ClaimService Claims { get; }
        public AgentService Agents { get; }

        public InsuranceSystemManager(IStorage storage)
        {
            var ctx = new InsuranceContext(storage);
            Clients = new ClientService(ctx);
            Policies = new PolicyService(ctx);
            Claims = new ClaimService(ctx);
            Agents = new AgentService(ctx);

            Policies.RefreshStatuses();
        }

    }
}
