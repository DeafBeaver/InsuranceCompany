using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void RequestsMenu(RequestService requests, ClientService clients) => Menu("Меню запитів клієнтів",
            ("Створити запит", () => CreateRequest(requests, clients)),
            ("Усі запити", () => ShowRequests(requests.Requests)),
            ("Запити конкретного клієнта", () => ShowClientRequests(requests, clients)),
            ("Підібрати поліси під запит", () => MatchRequest(requests)),
            ("Позначити запит оброблений", () => ProcessRequest(requests)));

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

        static void ShowClientRequests(RequestService requests, ClientService clients)
        {
            ListClients(clients);
            int clientId = Input.RequireInt("ID клієнта: ");
            ShowRequests(requests.GetByClient(clientId));
        }

        static void MatchRequest(RequestService requests)
        {
            ShowRequests(requests.Requests);
            int id = Input.RequireInt("ID запиту: ");

            var matches = requests.Match(id);
            Console.WriteLine($"\nПідходящих існуючих полісів: {matches.Count}");
            foreach (var p in matches) PrintPolicy(p);

            Console.WriteLine($"Орієнтовна вартість нового поліса за запитом: {requests.EstimateCost(id):0.##} грн");
        }

        static void ProcessRequest(RequestService requests)
        {
            ShowRequests(requests.Requests);
            requests.MarkProcessed(Input.RequireInt("ID запиту: "));
            Console.WriteLine("Запит позначено як оброблений!");
        }
    }
}
