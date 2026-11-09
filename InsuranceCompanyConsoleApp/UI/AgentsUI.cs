using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void AgentsMenu(AgentService agents, ClientService clients) => Menu("Меню агентів",
            ("Додати агента",                () => AddAgent(agents)),
            ("Переглянути агентів",          () => ShowAgents(agents)),
            ("Оновити інформацію про агента", () => UpdateAgent(agents)),
            ("Видалити агента",              () => DeleteAgent(agents)),
            ("Призначити клієнта агенту",    () => AssignClient(agents, clients)),
            ("Відписати клієнта від агента", () => UnassignClient(clients)));

        static void ShowAgents(AgentService agents)
        {
            Console.WriteLine("\n--- СПИСОК АГЕНТІВ ---");
            if (agents.Agents.Count == 0) { Console.WriteLine("Агентів немає."); return; }

            foreach (var a in agents.Agents)
            {
                Console.WriteLine($"[ID: {a.Id}] {a.Name} | Комісія: {a.CommissionRate * 100}%");
                foreach (var c in agents.GetClients(a.Id))
                    Console.WriteLine($"    - Клієнт [ID: {c.Id}] {c.FullName}");
            }
        }

        static void AddAgent(AgentService agents)
        {
            var agent = new Agent
            {
                Name = Input.RequireText("Ім'я агента: "),
                CommissionRate = Input.RequireDecimal("Комісія (0.0 - 1.0): ")
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
    }
}
