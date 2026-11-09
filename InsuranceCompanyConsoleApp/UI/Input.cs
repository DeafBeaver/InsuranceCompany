using System.Globalization;
using InsuranceSystem.Core.Exceptions;

namespace InsuranceSystem.ConsoleUI
{
    // Хелпери вводу. Правило: порожній ввід (Enter) = null = "пропустити",
    // а неправильний ввід = ValidationException (нічого не ігнорується мовчки).
    internal static class Input
    {
        public static string? Text(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        public static int? Int(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return null;
            return int.TryParse(s, out var n) ? n : throw new ValidationException("Невірний формат числа.");
        }

        public static decimal? Decimal(string prompt)
        {
            Console.Write(prompt);
            var s = Console.ReadLine()?.Trim().Replace(',', '.');
            if (string.IsNullOrWhiteSpace(s)) return null;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                ? d : throw new ValidationException("Невірний формат числа.");
        }

        // Підписи беруться з enum, тому розсинхрону з кодом бути не може
        public static T? Choice<T>(string title) where T : struct, Enum
        {
            Console.WriteLine(title);
            foreach (var v in Enum.GetValues<T>())
                Console.WriteLine($"  {Convert.ToInt32(v)} - {v}");
            Console.Write("Вибір: ");

            var s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (int.TryParse(s, out var n) && Enum.IsDefined(typeof(T), n))
                return (T)Enum.ToObject(typeof(T), n);
            throw new ValidationException("Невірний вибір.");
        }

        // Обов'язкові значення
        public static string RequireText(string prompt) =>
            Text(prompt) ?? throw new ValidationException("Поле не може бути порожнім.");
        public static int RequireInt(string prompt) =>
            Int(prompt) ?? throw new ValidationException("Поле не може бути порожнім.");
        public static decimal RequireDecimal(string prompt) =>
            Decimal(prompt) ?? throw new ValidationException("Поле не може бути порожнім.");
        public static T RequireChoice<T>(string title) where T : struct, Enum =>
            Choice<T>(title) ?? throw new ValidationException("Потрібно обрати значення.");
    }
}
