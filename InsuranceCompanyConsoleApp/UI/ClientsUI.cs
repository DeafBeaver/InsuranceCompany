using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void ClientsMenu(ClientService clients, AgentService agents) => Menu("Меню клієнтів",
            ("Додати клієнта",       () => AddClient(clients)),
            ("Переглянути клієнтів", () => ShowClients(clients, agents)),
            ("Оновити клієнта",      () => UpdateClient(clients)),
            ("Видалити клієнта",     () => DeleteClient(clients)));

        static void ListClients(ClientService clients)
        {
            Console.WriteLine("Клієнти:");
            foreach (var c in clients.Clients)
                Console.WriteLine($"  [ID: {c.Id}] {c.FullName}");
        }

        static void ShowClients(ClientService clients, AgentService agents)
        {
            Console.WriteLine("\n--- СПИСОК КЛІЄНТІВ ---");
            if (clients.Clients.Count == 0) { Console.WriteLine("Клієнтів немає."); return; }

            foreach (var c in clients.Clients)
            {
                string agent = c.AgentId.HasValue
                    ? agents.Agents.FirstOrDefault(a => a.Id == c.AgentId)?.Name ?? "?"
                    : "—";
                Console.WriteLine(
                    $"[ID: {c.Id}] {c.FullName} ({c.Type}) | Контакти: {c.ContactInfo} | " +
                    $"Полісів: {clients.CountPolicies(c.Id)} | Агент: {agent} | " +
                    $"Всього виплат: {clients.TotalPayouts(c.Id)} грн");
            }
        }

        static void AddClient(ClientService clients)
        {
            var client = new Client
            {
                FullName = Input.RequireText("ПІБ: "),
                ContactInfo = Input.Text("Контакти [Enter - пропустити]: ") ?? "",
                Type = Input.RequireChoice<ClientType>("Тип клієнта:")
            };
            clients.Add(client);
            Console.WriteLine($"Клієнта додано! ID: {client.Id}");
        }

        static void UpdateClient(ClientService clients)
        {
            ListClients(clients);
            int id = Input.RequireInt("ID клієнта: ");
            var name    = Input.Text("Нове ПІБ [Enter - пропустити]: ");
            var contact = Input.Text("Нові контакти [Enter - пропустити]: ");
            var type    = Input.Choice<ClientType>("Новий тип [Enter - пропустити]:");

            clients.Update(id, name, contact, type);
            Console.WriteLine("Клієнта оновлено!");
        }

        static void DeleteClient(ClientService clients)
        {
            ListClients(clients);
            clients.Delete(Input.RequireInt("ID клієнта для видалення: "));
            Console.WriteLine("Клієнта видалено!");
        }
    }
}
