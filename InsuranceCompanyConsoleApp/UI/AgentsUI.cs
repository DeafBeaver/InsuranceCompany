using InsuranceSystem.Core.Exceptions;
using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void AgentsMenu(AgentService agents, ClientService clients, RequestService request) => Menu("Меню агентів",
            ("Додати агента", () => AddAgent(agents)),
            ("Переглянути агентів", () => ShowAgents(agents)),
            ("Оновити інформацію про агента", () => UpdateAgent(agents)),
            ("Видалити агента", () => DeleteAgent(agents)),
            ("Призначити клієнта агенту", () => AssignClient(agents, clients)),
            ("Відписати клієнта від агента", () => UnassignClient(clients)),
            ("Запити конкретного клієнта", () => ShowClientRequests(request, clients)),
            ("Створити поліс за запитом", () => ProcessRequest(request, agents, clients)),
            ("Підібрати поліси під запит", () => MatchRequest(request)));

        static void ShowAgents(AgentService agents)
        {
            Console.WriteLine("\n--- СПИСОК АГЕНТІВ ---");
            if (agents.Get().Count == 0) { Console.WriteLine("Агентів немає."); return; }

            foreach (var a in agents.Get())
            {
                Console.WriteLine($"[ID: {a.Id}] {a.Name} | Комісія: {a.CommissionRate * 100}%");
                foreach (var c in agents.GetAgentsClients(a.Id))
                    Console.WriteLine($"    - Клієнт [ID: {c.Id}] {c.FullName}");
            }
        }

        static void AddAgent(AgentService agents)
        {
            var agent = new Agent
            {
                Name = Input.RequireText("Ім'я агента: "),
                CommissionRate = Input.RequireDecimal("Комісія (0.0 - 1.0): "),
                Login = Input.Text("Логін у системі авторизації [Enter - пропустити]: ")
            };
            agents.Add(agent);
            Console.WriteLine($"Агента додано! ID: {agent.Id}");
        }

        static void UpdateAgent(AgentService agents)
        {
            ShowAgents(agents);
            int id = Input.RequireInt("ID агента: ");
            var name = Input.Text("Нове ім'я [Enter - пропустити]: ");
            var commission = Input.Decimal("Нова комісія 0.0 - 1.0 [Enter - пропустити]: ");

            agents.Update(id, name, commission);
            Console.WriteLine("Агента оновлено!");
        }

        static void DeleteAgent(AgentService agents)
        {
            ShowAgents(agents);
            agents.Delete(Input.RequireInt("ID агента для видалення: "));
            Console.WriteLine("Агента видалено! Його клієнтів відв'язано.");
        }

        static void AssignClient(AgentService agents, ClientService clients)
        {
            ListClients(clients);
            int clientId = Input.RequireInt("ID клієнта: ");
            ShowAgents(agents);
            int agentId = Input.RequireInt("ID агента: ");

            clients.SetAgent(clientId, agentId);
            Console.WriteLine("Клієнта призначено агенту!");
        }

        static void UnassignClient(ClientService clients)
        {
            ListClients(clients);
            clients.SetAgent(Input.RequireInt("ID клієнта: "), null);
            Console.WriteLine("Клієнта відв'язано від агента!");
        }

        static void ProcessRequest(RequestService requests, AgentService agents, ClientService clients)
        {
            ShowRequests(requests.Requests);

            int requestId = Input.RequireInt("ID запиту: ");

            ShowAgents(agents);

            int agentId = Input.RequireInt("ID агента якого закріпити: ");

            var client = clients.GetById(requests.Get(requestId).ClientId);

            if (client.AgentId == null)
            {
                Console.Write("За клієнтом не закріплено агента. Закріпити за цим агентом? (т/н): ");
                if (Console.ReadLine()?.Trim().ToLower() != "т")
                    throw new ValidationException("Скасовано: потрібен закріплений агент.");

                clients.SetAgent(client.Id, agentId);
            }


            requests.MakePolicyByRequest(requestId, agentId);
            Console.WriteLine($"Поліс створено по запиту {requestId}!");
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
    }
}
