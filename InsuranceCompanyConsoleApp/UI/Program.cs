using System.Text;
using InsuranceSystem.BLL.Persistence;
using InsuranceSystem.BLL.Services;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.ConsoleUI
{
    internal partial class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            InsuranceSystemManager app;
            try
            {
                app = new InsuranceSystemManager(new JsonStorage("insurance_data.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося завантажити дані: {ex.Message}");
                return;
            }

            Menu("=== СИСТЕМА УПРАВЛІННЯ СТРАХОВОЮ КОМПАНІЄЮ ===", "Вихід",
                ("Клієнти",          Sub(() => ClientsMenu(app.Clients, app.Agents))),
                ("Поліси",           Sub(() => PoliciesMenu(app.Policies, app.Clients))),
                ("Агенти",           Sub(() => AgentsMenu(app.Agents, app.Clients))),
                ("Страхові випадки", Sub(() => ClaimsMenu(app.Claims, app.Policies))),
                ("Статистика",       Sub(() => StatisticsMenu(app.Statistics))),
                ("Запити клієнтів", Sub(() => RequestsMenu(app.Requests, app.Clients))));
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
