using InsuranceSystem.BLL.Service;
using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.Core.Enums;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.ConsoleUI
{
    internal record Session(string Role, string Login, Client? Client, Agent? Agent);

    internal partial class Program
    {
        static void ClientRoleMenu(InsuranceSystemManager app, Session s)
        {
            if (s.Client == null)
            {
                Console.WriteLine("Вашого профілю клієнта ще не створено. Зверніться до агента.");
                Pause();
                return;
            }
            var me = s.Client;

            Menu($"=== МЕНЮ КЛІЄНТА: {me.FullName} ===", "Вихід",
                ("Мої поліси", () => ShowMyPolicies(app.Policies, me)),
                ("Мої страхові випадки", () => ShowMyClaims(app.Policies, app.Claims, me)),
                ("Мої запити", () => ShowRequests(app.Requests.GetByClient(me.Id))),
                ("Створити запит", () => CreateMyRequest(app.Requests, me)));
        }

        static void ShowMyPolicies(PolicyService policies, Client me)
        {
            Console.WriteLine("\n--- МОЇ ПОЛІСИ ---");
            var list = policies.Search(clientId: me.Id);
            if (list.Count == 0) { Console.WriteLine("Полісів немає."); return; }
            foreach (var p in list) PrintPolicy(p);
        }

        static void ShowMyClaims(PolicyService policies, ClaimService claims, Client me)
        {
            Console.WriteLine("\n--- МОЇ СТРАХОВІ ВИПАДКИ ---");
            var myPolicyIds = policies.Search(clientId: me.Id).Select(p => p.Id).ToHashSet();
            var list = claims.Get().Where(c => myPolicyIds.Contains(c.PolicyId)).ToList();
            if (list.Count == 0) { Console.WriteLine("Випадків немає."); return; }
            foreach (var c in list) PrintClaim(c);
        }

        static void CreateMyRequest(RequestService requests, Client me)
        {
            var request = new Request
            {
                ClientId = me.Id,
                Type = Input.RequireChoice<PolicyType>("Бажаний тип страхування:"),
                DesiredCoverage = Input.RequireDecimal("Бажана сума покриття (грн): "),
                TermMonths = Input.RequireInt("Строк (місяців): ")
            };
            requests.Add(request);
            Console.WriteLine($"Запит збережено! ID: {request.Id}");
        }

        static void AgentRoleMenu(InsuranceSystemManager app, Session s)
        {
            if (s.Agent == null)
            {
                Console.WriteLine("Вашого профілю агента ще не створено. Зверніться до менеджера.");
                Pause();
                return;
            }
            var me = s.Agent;

            Menu($"=== МЕНЮ АГЕНТА: {me.Name} ===", "Вихід",
                ("Мої клієнти", () => ShowMyClients(app.Agents, app.Clients, me)),
                ("Додати клієнта", () => AddMyClient(app.Clients, me)),
                ("Закріпити клієнта за собою", () => TakeClient(app.Clients, me)),
                ("Додати поліс", () => AddPolicy(app.Policies, app.Clients)),
                ("Мої продані поліси", () => ShowMySales(app.Policies, me)),
                ("Запити моїх клієнтів", () => ShowMyClientsRequests(app.Requests, app.Agents, me)),
                ("Створити поліс за запитом", () => ProcessMyRequest(app.Requests, app.Agents, me)),
                ("Зареєструвати страховий випадок", () => RegisterClaim(app.Claims, app.Policies)),
                ("Моя статистика", () => ShowMyStats(app.Statistics, me)));
        }

        static void ShowMyClients(AgentService agents, ClientService clients, Agent me)
        {
            Console.WriteLine("\n--- МОЇ КЛІЄНТИ ---");
            var list = agents.GetAgentsClients(me.Id);
            if (list.Count == 0) { Console.WriteLine("Клієнтів немає."); return; }
            foreach (var c in list)
                Console.WriteLine($"[ID: {c.Id}] {c.FullName} | Полісів: {clients.CountPolicies(c.Id)}");
        }

        static void AddMyClient(ClientService clients, Agent me)
        {
            var client = new Client
            {
                FullName = Input.RequireText("ПІБ: "),
                ContactInfo = Input.Text("Контакти [Enter - пропустити]: ") ?? "",
                Type = Input.RequireChoice<ClientType>("Тип клієнта:"),
                Login = Input.Text("Логін клієнта в системі авторизації [Enter - пропустити]: ")
            };
            clients.Add(client);
            clients.SetAgent(client.Id, me.Id);
            Console.WriteLine($"Клієнта додано та закріплено за вами! ID: {client.Id}");
        }

        static void TakeClient(ClientService clients, Agent me)
        {
            ListClients(clients);
            var client = clients.GetById(Input.RequireInt("ID клієнта: "));

            if (client.AgentId != null && client.AgentId != me.Id)
                throw new ValidationException("Клієнт уже закріплений за іншим агентом.");

            clients.SetAgent(client.Id, me.Id);
            Console.WriteLine("Клієнта закріплено за вами!");
        }

        static void ShowMySales(PolicyService policies, Agent me)
        {
            Console.WriteLine("\n--- МОЇ ПРОДАНІ ПОЛІСИ ---");
            var list = policies.Get().Where(p => p.AgentId == me.Id).ToList();
            if (list.Count == 0) { Console.WriteLine("Полісів немає."); return; }
            foreach (var p in list) PrintPolicy(p);
        }

        static void ShowMyClientsRequests(RequestService requests, AgentService agents, Agent me)
        {
            var clientIds = agents.GetAgentsClients(me.Id).Select(c => c.Id).ToHashSet();
            ShowRequests(requests.Requests.Where(r => clientIds.Contains(r.ClientId)).ToList());
        }

        static void ProcessMyRequest(RequestService requests, AgentService agents, Agent me)
        {
            ShowMyClientsRequests(requests, agents, me);
            int requestId = Input.RequireInt("ID запиту: ");

            requests.MakePolicyByRequest(requestId, me.Id);
            Console.WriteLine($"Поліс створено по запиту {requestId}!");
        }

        static void ShowMyStats(StatisticService stats, Agent me)
        {
            var (count, commission) = stats.GetAgentEfficiency(me.Id);
            Console.WriteLine("\n--- МОЯ СТАТИСТИКА ---");
            Console.WriteLine($"Продано полісів: {count}");
            Console.WriteLine($"Комісія: {commission:0.##} грн");
        }

        static void ManagerRoleMenu(InsuranceSystemManager app)
        {
            Menu("=== МЕНЮ МЕНЕДЖЕРА ===", "Вихід",
                ("Статистика", Sub(() => StatisticsMenu(app.Statistics))),
                ("Страхові випадки", Sub(() => ClaimsManagerMenu(app.Claims))),
                ("Усі поліси", () => ShowPolicies(app.Policies)),
                ("Усі клієнти", () => ShowClients(app.Clients, app.Agents)),
                ("Усі запити", () => ShowRequests(app.Requests.Requests)),
                ("Агенти", Sub(() => AgentsMenu(app.Agents, app.Clients, app.Requests))));
        }

        static void ClaimsManagerMenu(ClaimService claims) => Menu("Страхові випадки (менеджер)",
            ("Переглянути випадки", () => ShowClaims(claims)),
            ("Змінити статус (розгляд, рішення)", () => ChangeClaimStatus(claims)),
            ("Провести виплату", () => DoPayout(claims)));
    }
}
