using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.BLL.Service.AuthApi;
using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Exceptions;
using SQLitePCL;
using System.Text;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Batteries.Init();

            var authClient = new AuthApiClient("https://localhost:7030/");

            Console.WriteLine("=== АВТОРИЗАЦІЯ В СИСТЕМІ ===");
            Console.Write("Введіть логін: ");
            string username = Console.ReadLine() ?? "";
            Console.Write("Введіть пароль: ");
            string password = Console.ReadLine() ?? "";

            Console.WriteLine("Виконується вхід...");
            var authResult = await authClient.LoginAsync(username, password);

            if (authResult == null)
            {
                Console.WriteLine("Помилка: невірний логін або пароль!");
                Pause();
                return;
            }

            Console.WriteLine($"Успіх! Вітаємо, ваша роль у системі: {authResult.Role}");
            Thread.Sleep(1500);

            InsuranceSystemManager app;
            try
            {
                app = new InsuranceSystemManager();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося ініціалізувати базу даних: {ex.Message}");
                return;
            }
            await RunMenuBasedOnRole(authResult.Role, authResult.Token, authClient, app);
        }

        static async Task RunMenuBasedOnRole(string role, string currentToken, AuthApiClient authClient, InsuranceSystemManager app)
        {
            switch (role)
            {
                case "Client":
                    Menu("=== МЕНЮ КЛІЄНТА ===", "Вихід",
                    ("Мої поліси", Sub(() => PoliciesMenu(app.Policies, app.Clients))),
                    ("Створити запит", Sub(() => RequestsMenu(app.Requests, app.Clients))));
                    break;
                case "Agent":
                    Menu("=== МЕНЮ АГЕНТА ===", "Вихід",
                    ("Клієнти", Sub(() => ClientsMenu(app.Clients, app.Agents))),
                    ("Поліси", Sub(() => PoliciesMenu(app.Policies, app.Clients))),
                    ("Агенти", Sub(() => AgentsMenu(app.Agents, app.Clients, app.Requests))),
                    ("Страхові випадки", Sub(() => ClaimsMenu(app.Claims, app.Policies))));
                    break;
                case "Manager":
                    Menu("=== МЕНЮ МЕНЕДЖЕРА ===", "Вихід",
                        ("Клієнти", Sub(() => ClientsMenu(app.Clients, app.Agents))),
                        ("Поліси", Sub(() => PoliciesMenu(app.Policies, app.Clients))),
                        ("Агенти", Sub(() => AgentsMenu(app.Agents, app.Clients, app.Requests))),
                        ("Страхові випадки", Sub(() => ClaimsMenu(app.Claims, app.Policies))),
                        ("Статистика", Sub(() => StatisticsMenu(app.Statistics))),
                        ("Запити клієнтів", Sub(() => RequestsMenu(app.Requests, app.Clients))));
                    break;
                case "Admin":
                case "Administrator":
                    {
                        bool running = true;
                        while (running)
                        {
                            Console.Clear();
                            Console.WriteLine("=== ПАНЕЛЬ КЕРУВАННЯ АДМІНІСТРАТОРА ===");
                            Console.WriteLine("1. Переглянути список користувачів");
                            Console.WriteLine("2. Зареєструвати нового користувача (клієнта/агента)");
                            Console.WriteLine("3. Змінити роль користувача");
                            Console.WriteLine("4. Перейти до страхової системи (меню даних)");
                            Console.WriteLine("0. Вихід");
                            Console.Write("Оберіть дію: ");

                            string choice = Console.ReadLine() ?? "";

                            switch (choice)
                            {
                                case "1":
                                    var users = await authClient.GetUsersAsync(currentToken);
                                    Console.WriteLine("\n--- Список користувачів у системі ---");
                                    if (users != null)
                                    {
                                        foreach (var u in users)
                                            Console.WriteLine($"Логін: {u.UserName} | Роль: {u.Role}");
                                    }
                                    Pause();
                                    break;

                                case "2":
                                    Console.Write("Введіть логін нового юзера: ");
                                    string newLogin = Console.ReadLine()!;
                                    Console.Write("Введіть пароль: ");
                                    string newPass = Console.ReadLine()!;
                                    Console.Write("Введіть роль (Client / Agent / Manager): ");
                                    string newRole = Console.ReadLine()!;

                                    bool regSuccess = await authClient.RegisterAsync(currentToken, newLogin, newPass, newRole);
                                    Console.WriteLine(regSuccess ? "Користувача успішно створено!" : "Помилка при створенні.");
                                    Pause();
                                    break;

                                case "3":
                                    Console.Write("Введіть логін існуючого юзера: ");
                                    string targetUser = Console.ReadLine()!;
                                    Console.Write("Введіть нову роль: ");
                                    string targetRole = Console.ReadLine()!;

                                    bool roleSuccess = await authClient.SetUserRoleAsync(currentToken, targetUser, targetRole);
                                    Console.WriteLine(roleSuccess ? "Роль успішно змінено!" : "Помилка при зміні ролі.");
                                    Pause();
                                    break;

                                case "4":
                                    Menu("=== МЕНЮ МЕНЕДЖЕРА ===", "Вихід",
                                        ("Клієнти", Sub(() => ClientsMenu(app.Clients, app.Agents))),
                                        ("Поліси", Sub(() => PoliciesMenu(app.Policies, app.Clients))),
                                        ("Агенти", Sub(() => AgentsMenu(app.Agents, app.Clients, app.Requests))),
                                        ("Страхові випадки", Sub(() => ClaimsMenu(app.Claims, app.Policies))),
                                        ("Статистика", Sub(() => StatisticsMenu(app.Statistics))),
                                        ("Запити клієнтів", Sub(() => RequestsMenu(app.Requests, app.Clients))));
                                    break;

                                case "0":
                                    running = false;
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        static void Run(Action action)
        {
            try { action(); }
            catch (DomainException ex) { Console.WriteLine($"Помилка: {ex.Message}"); }
            catch (Exception ex) { Console.WriteLine($"Неочікувана помилка: {ex.Message}"); }
        }

        private static bool _skipPause;

        static Action Sub(Action submenu) => () => { submenu(); _skipPause = true; };

        static void Pause()
        {
            Console.WriteLine("\nНатисніть будь-яку клавішу...");
            if (Console.IsInputRedirected) Console.ReadLine();
            else Console.ReadKey(true);
        }

        static void Menu(string title, params (string Label, Action Action)[] items) =>
            Menu(title, "Назад", items);

        static void Menu(string title, string backLabel, params (string Label, Action Action)[] items)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(title);
                for (int i = 0; i < items.Length; i++)
                    Console.WriteLine($"{i + 1}. {items[i].Label}");
                Console.WriteLine($"0. {backLabel}");
                Console.Write("Оберіть опцію: ");

                var choice = Console.ReadLine();
                if (choice == null || choice == "0") return;

                _skipPause = false;
                if (int.TryParse(choice, out int n) && n >= 1 && n <= items.Length)
                    Run(items[n - 1].Action);
                else
                    Console.WriteLine("Невірна опція.");

                if (_skipPause) _skipPause = false;
                else Pause();
            }
        }
    }
}