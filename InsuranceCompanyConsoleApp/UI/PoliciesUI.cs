using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void PoliciesMenu(PolicyService policies, ClientService clients) => Menu("Меню полісів",
            ("Додати поліс",           () => AddPolicy(policies, clients)),
            ("Переглянути поліси",     () => ShowPolicies(policies)),
            ("Оновити поліс",          () => UpdatePolicy(policies)),
            ("Змінити статус поліса",  () => ChangePolicyStatus(policies)),
            ("Видалити поліс",         () => DeletePolicy(policies)),
            ("Знайти поліс",           () => SearchPolicies(policies)));

        static void PrintPolicy(Policy p) =>
            Console.WriteLine(
                $"[ID: {p.Id}] Клієнт ID: {p.ClientId} | {p.Type} | Статус: {p.Status} | " +
                $"Покриття: {p.CoverageAmount} грн | Вартість: {p.Cost} грн | " +
                $"{p.StartDate:dd.MM.yyyy} - {p.EndDate:dd.MM.yyyy}");

        static void ShowPolicies(PolicyService policies)
        {
            Console.WriteLine("\n--- СПИСОК ПОЛІСІВ ---");
            if (policies.Policies.Count == 0) { Console.WriteLine("Полісів немає."); return; }
            foreach (var p in policies.Policies) PrintPolicy(p);
        }

        static void AddPolicy(PolicyService policies, ClientService clients)
        {
            var type = Input.RequireChoice<PolicyType>("Тип поліса:");
            decimal coverage = Input.RequireDecimal("Сума покриття (грн): ");
            int months = Input.Int("Термін, місяців [Enter = 12]: ") ?? 12;
            ListClients(clients);
            int clientId = Input.RequireInt("ID клієнта: ");

            var start = DateTime.Now;
            var policy = new Policy
            {
                Type = type,
                CoverageAmount = coverage,
                StartDate = start,
                EndDate = start.AddMonths(months),
                ClientId = clientId
            };
            policies.Add(policy);
            Console.WriteLine($"Поліс створено! ID: {policy.Id}, вартість: {policy.Cost} грн");
        }

        static void UpdatePolicy(PolicyService policies)
        {
            ShowPolicies(policies);
            int id = Input.RequireInt("ID поліса: ");
            var type = Input.Choice<PolicyType>("Новий тип [Enter - пропустити]:");
            var coverage = Input.Decimal("Нова сума покриття [Enter - пропустити]: ");

            policies.Update(id, type: type, coverageAmount: coverage);
            Console.WriteLine("Поліс оновлено! Вартість перераховано.");
        }

        static void ChangePolicyStatus(PolicyService policies)
        {
            ShowPolicies(policies);
            int id = Input.RequireInt("ID поліса: ");
            var status = Input.RequireChoice<PolicyStatus>("Новий статус:");

            policies.ChangeStatus(id, status);
            Console.WriteLine("Статус оновлено!");
        }

        static void DeletePolicy(PolicyService policies)
        {
            ShowPolicies(policies);
            policies.Delete(Input.RequireInt("ID поліса для видалення: "));
            Console.WriteLine("Поліс видалено!");
        }

        static void SearchPolicies(PolicyService policies)
        {
            Console.WriteLine("\n--- ПОШУК ПОЛІСІВ (Enter - пропустити параметр) ---");
            var clientId = Input.Int("ID клієнта: ");
            var type     = Input.Choice<PolicyType>("Тип:");
            var status   = Input.Choice<PolicyStatus>("Статус:");
            var minCost  = Input.Decimal("Мінімальна вартість: ");
            var maxCost  = Input.Decimal("Максимальна вартість: ");

            var results = policies.Search(type, clientId, status, minCost, maxCost);

            Console.WriteLine($"\nЗнайдено записів: {results.Count}");
            foreach (var p in results) PrintPolicy(p);
        }
    }
}
