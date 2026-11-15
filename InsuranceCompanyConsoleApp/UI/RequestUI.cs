using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void RequestsMenu(RequestService requests, ClientService clients) => Menu("Меню запитів клієнтів",
            ("Створити запит", () => CreateRequest(requests, clients)),
            ("Усі запити", () => ShowRequests(requests.Requests)));
        static void ShowRequests(IReadOnlyList<Request> list)
        {
            Console.WriteLine("\n--- ЗАПИТИ ---");
            if (list.Count == 0) { Console.WriteLine("Запитів немає."); return; }

            foreach (var r in list)
                Console.WriteLine(
                    $"[ID: {r.Id}] Клієнт ID: {r.ClientId} | {r.Type} | " +
                    $"Покриття: {r.DesiredCoverage} грн | Строк: {r.TermMonths} міс. | " +
                    $"Статус: {r.Status} | {r.CreatedAt:dd.MM.yyyy}");
        }

        static void CreateRequest(RequestService requests, ClientService clients)
        {
            ListClients(clients);
            var request = new Request
            {
                ClientId = Input.RequireInt("ID клієнта: "),
                Type = Input.RequireChoice<PolicyType>("Бажаний тип страхування:"),
                DesiredCoverage = Input.RequireDecimal("Бажана сума покриття (грн): "),
                TermMonths = Input.RequireInt("Строк (місяців): ")
            };
            requests.Add(request);
            Console.WriteLine($"Запит збережено! ID: {request.Id}");
        }

    }
}
