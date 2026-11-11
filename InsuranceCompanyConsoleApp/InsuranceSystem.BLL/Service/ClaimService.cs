using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.BLL.Services
{
    public class ClaimService
    {
        private readonly InsuranceContext _context;

        public ClaimService(InsuranceContext context) => _context = context;
        /// <summary>
        /// Gets a read-only list of all claims in the system.
        /// </summary>
        public IReadOnlyList<Claim> Claims => _context.Database.Claims;

        /// <summary>
        /// Gets a specific claim by its ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="EntityNotFoundException"></exception>
        public Claim Get(int id) =>
            _context.Database.Claims.FirstOrDefault(c => c.Id == id)
            ?? throw new EntityNotFoundException("Страховий випадок", id);

        /// <summary>
        /// Adds a new claim to the system after validating its details against the associated policy.
        /// </summary>
        /// <param name="claim"></param>
        /// <exception cref="EntityNotFoundException"></exception>
        /// <exception cref="ValidationException"></exception>
        public void Add(Claim claim)
        {
            ArgumentNullException.ThrowIfNull(claim);

            var policy = _context.Database.Policies.FirstOrDefault(p => p.Id == claim.PolicyId)
                ?? throw new EntityNotFoundException("Поліс", claim.PolicyId);

            if (policy.Status != PolicyStatus.Active)
                throw new ValidationException("Страховий випадок можна зареєструвати лише за активним полісом!");
            if (claim.Date < policy.StartDate || claim.Date > policy.EndDate)
                throw new ValidationException("Дата випадку виходить за строк дії поліса!");
            if (string.IsNullOrWhiteSpace(claim.Description))
                throw new ValidationException("Опис випадку не може бути порожнім!");
            if (claim.PayoutAmount <= 0)
                throw new ValidationException("Сума виплати має бути більшою за нуль!");
            if (claim.PayoutAmount > policy.CoverageAmount)
                throw new ValidationException("Сума виплати перевищує суму покриття поліса!");

            claim.Id = _context.Database.NextClaimId++;
            claim.Status = ClaimStatus.New;
            _context.Database.Claims.Add(claim);
            _context.Save();
        }

        /// <summary>
        /// Dictionary for appropriate change status of claims. It defines the allowed transitions between different claim statuses.
        /// </summary>
        private static readonly Dictionary<ClaimStatus, ClaimStatus[]> AllowedTransitions = new()
        {
            [ClaimStatus.New] = new[] { ClaimStatus.Pending },
            [ClaimStatus.Pending] = new[] { ClaimStatus.Approved, ClaimStatus.Rejected },
            [ClaimStatus.Approved] = new[] { ClaimStatus.Rejected },
            [ClaimStatus.Paid] = Array.Empty<ClaimStatus>(),
            [ClaimStatus.Rejected] = Array.Empty<ClaimStatus>()
        };

        /// <summary>
        /// Changes the status of a claim.
        /// </summary>
        /// <param name="claimId"></param>
        /// <param name="newStatus"></param>
        /// <exception cref="ValidationException"></exception>
        public void ChangeStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = Get(claimId);

            if (!AllowedTransitions[claim.Status].Contains(newStatus))
                throw new ValidationException($"Неможливо змінити статус з {claim.Status} на {newStatus}.");

            claim.Status = newStatus;
            _context.Save();
        }
        /// <summary>
        /// Does a payout for an approved claim.
        /// </summary>
        /// <param name="claimId"></param>
        /// <exception cref="ValidationException"></exception>
        /// <exception cref="EntityNotFoundException"></exception>
        public void DoPayout(int claimId)
        {
            var claim = Get(claimId);
            if (claim.Status != ClaimStatus.Approved)
                throw new ValidationException("Виплата можлива лише для затверджених випадків.");

            var policy = _context.Database.Policies.FirstOrDefault(p => p.Id == claim.PolicyId)
                ?? throw new EntityNotFoundException("Поліс", claim.PolicyId);

            decimal alreadyPaid = _context.Database.Payments
                .Where(p => p.PolicyId == claim.PolicyId && p.Type == PaymentType.Payout)
                .Sum(p => p.Amount);

            decimal remaining = policy.CoverageAmount - alreadyPaid;
            if (claim.PayoutAmount > remaining)
                throw new ValidationException($"Сума виплати перевищує залишок покриття ({remaining} грн).");

            claim.Status = ClaimStatus.Paid;
            _context.Database.Payments.Add(new Payment
            {
                Id = _context.Database.NextPaymentId++,
                Date = DateTime.Now,
                Amount = claim.PayoutAmount,
                Type = PaymentType.Payout,
                PolicyId = claim.PolicyId
            });
            _context.Save();

        }
    }
}
