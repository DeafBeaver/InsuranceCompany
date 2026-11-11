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

        public IReadOnlyList<Claim> Claims => _context.Database.Claims;

        public Claim Get(int id) =>
            _context.Database.Claims.FirstOrDefault(c => c.Id == id)
            ?? throw new EntityNotFoundException("Страховий випадок", id);

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

        public void ChangeStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = Get(claimId);

            if ((int)newStatus != (int)claim.Status + 1)
                throw new ValidationException(
                    $"Неможливо змінити статус з {claim.Status} на {newStatus}.");

            claim.Status = newStatus;
            if (newStatus == ClaimStatus.Done)
            {
                _context.Database.Payments.Add(new Payment
                {
                    Id = _context.Database.NextPaymentId++,
                    Date = DateTime.Now,
                    Amount = claim.PayoutAmount,
                    Type = PaymentType.Payout,
                    PolicyId = claim.PolicyId
                });
            }
            _context.Save();
        }
    }
}
