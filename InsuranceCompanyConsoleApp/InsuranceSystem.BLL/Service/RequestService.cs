using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enum;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class RequestService
    {
        private readonly InsuranceContext _context;

        public RequestService(InsuranceContext context) => _context = context;

        public IReadOnlyList<Request> Requests => _context.Database.Requests;

        public Request Get(int id) =>
            _context.Database.Requests.FirstOrDefault(r => r.Id == id)
            ?? throw new EntityNotFoundException("Запит", id);

        public List<Request> GetByClient(int clientId) =>
            _context.Database.Requests.Where(r => r.ClientId == clientId).ToList();

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

        public void MarkProcessed(int id)
        {
            var request = Get(id);
            if (request.Status == RequestStatus.Processed)
                throw new ValidationException("Запит уже оброблено!");

            request.Status = RequestStatus.Processed;
            _context.Save();
        }
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

        public decimal EstimateCost(int requestId) =>
            PolicyService.CalculateCost(Get(requestId).DesiredCoverage);
    }
}
