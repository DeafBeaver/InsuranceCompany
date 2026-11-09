using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void ClaimsMenu(ClaimService claims, PolicyService policies) => Menu("Меню страхових випадків",
            ("Зареєструвати страховий випадок", () => RegisterClaim(claims, policies)),
            ("Переглянути страхові випадки",    () => ShowClaims(claims)),
            ("Змінити статус випадку",          () => ChangeClaimStatus(claims)));

        static void ShowClaims(ClaimService claims)
        {
            Console.WriteLine("\n--- СТРАХОВІ ВИПАДКИ ---");
            if (claims.Claims.Count == 0) { Console.WriteLine("Випадків немає."); return; }

            foreach (var c in claims.Claims)
                Console.WriteLine(
                    $"[ID: {c.Id}] Поліс ID: {c.PolicyId} | Статус: {c.Status} | " +
                    $"Дата: {c.Date:dd.MM.yyyy} | Виплата: {c.PayoutAmount} грн | {c.Description}");
        }

        static void RegisterClaim(ClaimService claims, PolicyService policies)
        {
            Console.WriteLine("Активні поліси:");
            foreach (var p in policies.Policies.Where(p => p.Status == PolicyStatus.Active))
                PrintPolicy(p);

            var claim = new Claim
            {
                PolicyId = Input.RequireInt("ID поліса: "),
                Description = Input.RequireText("Опис страхової події: "),
                PayoutAmount = Input.RequireDecimal("Сума виплати (грн): "),
                Date = DateTime.Now
            };
            claims.Add(claim);
            Console.WriteLine($"Страховий випадок зареєстровано! ID: {claim.Id}");
        }

        static void ChangeClaimStatus(ClaimService claims)
        {
            ShowClaims(claims);
            int id = Input.RequireInt("ID випадку: ");
            var status = Input.RequireChoice<ClaimStatus>("Новий статус:");

            claims.ChangeStatus(id, status);
            Console.WriteLine("Статус оновлено!");
        }
    }
}
