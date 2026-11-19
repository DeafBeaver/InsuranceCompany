using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InsuranceSystem.BLL.Services
{
    public class ClaimService
    {
        private readonly InsuranceDbContext _context;

        public ClaimService(InsuranceDbContext context)
        {
            _context = context;
        }

        public IReadOnlyList<Claim> Get() => _context.Claims.ToList();

        public Claim GetById(int id) =>
            _context.Claims.Find(id) ?? throw new EntityNotFoundException("Страховий випадок", id);

        public void Add(Claim claim)
        {
            ArgumentNullException.ThrowIfNull(claim);

            var policy = _context.Policies.Find(claim.PolicyId)
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

            claim.Status = ClaimStatus.New;
            _context.Claims.Add(claim);
            _context.SaveChanges();
        }

        private static readonly Dictionary<ClaimStatus, ClaimStatus[]> AllowedTransitions = new()
        {
            [ClaimStatus.New] = new[] { ClaimStatus.Pending },
            [ClaimStatus.Pending] = new[] { ClaimStatus.Approved, ClaimStatus.Rejected },
            [ClaimStatus.Approved] = new[] { ClaimStatus.Rejected },
            [ClaimStatus.Paid] = Array.Empty<ClaimStatus>(),
            [ClaimStatus.Rejected] = Array.Empty<ClaimStatus>()
        };

        public void ChangeStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = GetById(claimId);

            if (!AllowedTransitions[claim.Status].Contains(newStatus))
                throw new ValidationException($"Неможливо змінити статус з {claim.Status} на {newStatus}.");

            claim.Status = newStatus;
            _context.SaveChanges();
        }

        public void DoPayout(int claimId)
        {
            var claim = GetById(claimId);
            if (claim.Status != ClaimStatus.Approved)
                throw new ValidationException("Виплата можлива лише для затверджених випадків.");

            var policy = _context.Policies.Find(claim.PolicyId)
                ?? throw new EntityNotFoundException("Поліс", claim.PolicyId);

            decimal alreadyPaid = _context.Payments
                .Where(p => p.PolicyId == claim.PolicyId && p.Type == PaymentType.Payout)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            decimal remaining = policy.CoverageAmount - alreadyPaid;
            if (claim.PayoutAmount > remaining)
                throw new ValidationException($"Сума виплати перевищує залишок покриття ({remaining} грн).");

            claim.Status = ClaimStatus.Paid;

            _context.Payments.Add(new Payment
            {
                Date = DateTime.Now,
                Amount = claim.PayoutAmount,
                Type = PaymentType.Payout,
                PolicyId = claim.PolicyId
            });

            _context.SaveChanges();
        }
    }
}