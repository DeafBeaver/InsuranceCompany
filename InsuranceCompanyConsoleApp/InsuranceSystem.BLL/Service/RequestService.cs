using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class RequestService
    {
        private readonly InsuranceDbContext _context;

        public RequestService(InsuranceDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets a read-only list of all requests in the system.
        /// </summary>
        public IReadOnlyList<Request> Requests => _context.Requests.ToList();

        /// <summary>
        /// Retrieves a specific insurance request by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the request.</param>
        /// <returns>The found <see cref="Request"/> object.</returns>
        /// <exception cref="EntityNotFoundException">Thrown if the request with the specified ID is not found.</exception>
        public Request Get(int id) =>
            _context.Requests.FirstOrDefault(r => r.Id == id)
            ?? throw new EntityNotFoundException("Запит", id);

        /// <summary>
        /// Retrieves a list of all requests created by a specific client.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client.</param>
        /// <returns>A list of requests (<see cref="List{Request}"/>) belonging to the client.</returns>
        public List<Request> GetByClient(int clientId) =>
            _context.Requests.Where(r => r.ClientId == clientId).ToList();

        /// <summary>
        /// Adds a new insurance request to the system after passing validation checks.
        /// </summary>
        /// <param name="request">The request object to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if the passed <paramref name="request"/> object is null.</exception>
        /// <exception cref="EntityNotFoundException">Thrown if the client with the specified <see cref="Request.ClientId"/> does not exist.</exception>
        /// <exception cref="ValidationException">Thrown if the desired coverage amount or term is less than or equal to zero.</exception>
        public void Add(Request request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!_context.Clients.Any(c => c.Id == request.ClientId))
                throw new EntityNotFoundException("Клієнт", request.ClientId);
            if (request.DesiredCoverage <= 0)
                throw new ValidationException("Бажана сума покриття має бути більшою за нуль!");
            if (request.TermMonths <= 0)
                throw new ValidationException("Строк має бути більшим за нуль місяців!");

            request.Status = RequestStatus.New;
            request.CreatedAt = DateTime.Now;

            _context.Requests.Add(request);
            _context.SaveChanges();
        }

        /// <summary>
        /// Creates and saves a new insurance policy based on a client's request, 
        /// validating agent assignment and marking the request as processed.
        /// </summary>
        /// <param name="requestId">The unique identifier of the client's request.</param>
        /// <param name="agentId">The unique identifier of the agent processing the request.</param>
        /// <exception cref="EntityNotFoundException">
        /// Thrown if the request with <paramref name="requestId"/> or the associated client is not found.
        /// </exception>
        /// <exception cref="ValidationException">
        /// Thrown if the request has already been processed, the client has no assigned agent, 
        /// or the client is assigned to a different agent than <paramref name="agentId"/>.
        /// </exception>
        public void MakePolicyByRequest(int requestId, int agentId)
        {
            var request = Get(requestId);
            if (request.Status == RequestStatus.Processed)
                throw new ValidationException("Запит уже оброблено!");

            var client = _context.Clients.Find(request.ClientId) 
                ?? throw new EntityNotFoundException("Клієнт", request.ClientId);

            if (client.AgentId == null)
                throw new ValidationException("За клієнтом не закріплено агента.");
            if (client.AgentId != agentId)
                throw new ValidationException("Клієнт закріплений за іншим агентом.");

            var start = DateTime.Now;
            var policy = new Policy
            {
                Type = request.Type,
                CoverageAmount = request.DesiredCoverage,
                StartDate = start,
                EndDate = start.AddMonths(request.TermMonths),
                ClientId = request.ClientId,
                AgentId = agentId,
                RequestId = request.Id
            };

            _context.Policies.Add(policy);
            request.Status = RequestStatus.Processed;
            _context.SaveChanges();
        }

        /// <summary>
        /// Executes an algorithm to automatically match active policies meeting the request criteria.
        /// </summary>
        /// <param name="requestId">The unique identifier of the request for which matching is performed.</param>
        /// <returns>A list of insurance policies (<see cref="List{Policy}"/>) matching the request conditions.</returns>
        public List<Policy> Match(int requestId)
        {
            var r = Get(requestId);

            return _context.Policies.Where(p =>
                p.Status == PolicyStatus.Active &&
                p.Type == r.Type &&
                p.CoverageAmount >= r.DesiredCoverage &&
                (p.EndDate - p.StartDate).TotalDays >= r.TermMonths * 30
            ).ToList();
        }

        /// <summary>
        /// Calculates the estimated cost of an insurance policy based on the desired coverage from the request.
        /// </summary>
        /// <param name="requestId">The unique identifier of the request.</param>
        /// <returns>The estimated policy cost (<see cref="decimal"/> value).</returns>
        public decimal EstimateCost(int requestId) =>
            PolicyService.CalculateCost(Get(requestId).DesiredCoverage);
    }
}
