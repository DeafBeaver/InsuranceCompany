using InsuranceSystem.BLL.Service;
using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void StatisticsMenu(StatisticService stats) => Menu("Статистика",
            ("Загальна статистика", () => ShowSummary(stats)),
            ("Страхові випадки за період", () => ShowClaimsForPeriod(stats)),
            ("Ефективність агентів", () => ShowAgentStats(stats)),
            ("Статистика за типами страхування", () => ShowTypeStats(stats)),
            ("Платежі", () => ShowPayments(stats)));

        static void ShowSummary(StatisticService stats)
        {
            Console.WriteLine("\n--- ЗАГАЛЬНА СТАТИСТИКА ---");
            Console.WriteLine($"Активних полісів: {stats.GetActivePolicies().Count}");
            Console.WriteLine($"Всього виплат: {stats.TotalPayouts()} грн");
            Console.WriteLine($"Дохід компанії (внески - виплати): {stats.GetCompanyIncome()} грн");
        }

        static void ShowClaimsForPeriod(StatisticService stats)
        {
            var start = Input.RequireDate("Початок періоду (дд.мм.рррр): ");
            var end = Input.RequireDate("Кінець періоду (дд.мм.рррр): ");

            var claims = stats.GetClaimsForPeriod(start, end);
            Console.WriteLine($"\nСтрахових випадків за період: {claims.Count}");
            foreach (var c in claims) PrintClaim(c);
        }

        static void PrintClaim(Claim c) =>
            Console.WriteLine(
                $"[ID: {c.Id}] Поліс ID: {c.PolicyId} | Статус: {c.Status} | " +
                $"Дата: {c.Date:dd.MM.yyyy} | Виплата: {c.PayoutAmount} грн | {c.Description}");

        static void ShowAgentStats(StatisticService stats)
        {
            Console.WriteLine("\n--- ЕФЕКТИВНІСТЬ АГЕНТІВ ---");
            var list = stats.GetAllAgentsStats();
            if (list.Count == 0) { Console.WriteLine("Агентів немає."); return; }

            foreach (var a in list)
                Console.WriteLine($"[ID: {a.AgentId}] {a.Name} | Полісів: {a.PoliciesCount} | Комісія: {a.Commission:0.##} грн");
        }

        static void ShowTypeStats(StatisticService stats)
        {
            Console.WriteLine("\n--- СТАТИСТИКА ЗА ТИПАМИ ---");
            foreach (var t in stats.GetTypeStats())
                Console.WriteLine(
                    $"{t.Type}: всього {t.Total} | активних {t.Active} | " +
                    $"завершених {t.Completed} | середня ціна {t.AverageCost:0.##} грн");
        }

        static void ShowPayments(StatisticService stats)
        {
            Console.WriteLine("\n--- ПЛАТЕЖІ ---");
            if (stats.GetPayments().Count == 0) { Console.WriteLine("Платежів немає."); return; }

            foreach (var p in stats.GetPayments())
            {
                string sign = p.Type == PaymentType.Contribution ? "+" : "-";
                Console.WriteLine($"[ID: {p.Id}] {p.Date:dd.MM.yyyy HH:mm} | {p.Type} | Поліс ID: {p.PolicyId} | {sign}{p.Amount} грн");
            }
        }
    }
}