using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class RequestService
    {
        private readonly InsuranceContext _context;

        public RequestService(InsuranceContext context) => _context = context;

        /// <summary>
        /// Gets a read-only list of all claims in the system.
        /// </summary>
        public IReadOnlyList<Request> Requests => _context.Database.Requests;

        /// <summary>
        /// Retrieves a specific insurance request by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the request.</param>
        /// <returns>The found <see cref="Request"/> object.</returns>
        /// <exception cref="EntityNotFoundException">Thrown if the request with the specified ID is not found.</exception>
        public Request Get(int id) =>
            _context.Database.Requests.FirstOrDefault(r => r.Id == id)
            ?? throw new EntityNotFoundException("Запит", id);

        /// <summary>
        /// Retrieves a list of all requests created by a specific client.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client.</param>
        /// <returns>A list of requests (<see cref="List{Request}"/>) belonging to the client.</returns>
        public List<Request> GetByClient(int clientId) =>
            _context.Database.Requests.Where(r => r.ClientId == clientId).ToList();

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

            if (!_context.Database.Clients.Any(c => c.Id == request.ClientId))
                throw new EntityNotFoundException("Клієнт", request.ClientId);
            if (request.DesiredCoverage <= 0)
                throw new ValidationException("Бажана сума покриття має бути більшою за нуль!");
            if (request.TermMonths <= 0)
                throw new ValidationException("Строк має бути більшим за нуль місяців!");

            request.Id = _context.Database.NextRequestId++;
            request.Status = RequestStatus.New;
            request.CreatedAt = DateTime.Now;

            _context.Database.Requests.Add(request);
            _context.Save();
        }

        /// <summary>
        /// Changes the status of the specified request to "Processed" (<see cref="RequestStatus.Processed"/>).
        /// </summary>
        /// <param name="id">The unique identifier of the request.</param>
        /// <exception cref="EntityNotFoundException">Thrown if the request is not found.</exception>
        /// <exception cref="ValidationException">Thrown if the request is already processed.</exception>
        public void MarkProcessed(int id)
        {
            var request = Get(id);
            if (request.Status == RequestStatus.Processed)
                throw new ValidationException("Запит уже оброблено!");

            request.Status = RequestStatus.Processed;
            _context.Save();
        }

        /// <summary>
        /// Executes an algorithm to automatically match active policies meeting the request criteria.
        /// </summary>
        /// <param name="requestId">The unique identifier of the request for which matching is performed.</param>
        /// <returns>A list of insurance policies (<see cref="List{Policy}"/>) matching the request conditions.</returns>
        public List<Policy> Match(int requestId)
        {
            var r = Get(requestId);

            return _context.Database.Policies.Where(p =>
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
