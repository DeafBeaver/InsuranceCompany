using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using InsuranceSystem.Domain.Entities;

namespace InsuranceSystem.BLL.Persistence
{
    public class DatabaseContainer
    {
        public List<Client> Clients { get; set; } = new();
        public List<Policy> Policies { get; set; } = new();
        public List<Agent> Agents { get; set; } = new();
        public List<Claim> Claims { get; set; } = new();
    }

    public class JsonStorage
    {
        private readonly string _filePath;

        public JsonStorage(string filePath = "insurance_data.json")
        {
            _filePath = filePath;
        }
        public DatabaseContainer Load()
        {
            if (!File.Exists(_filePath))
                return new DatabaseContainer();

            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<DatabaseContainer>(json) ?? new DatabaseContainer();
        }
        public void Save(DatabaseContainer data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_filePath, json);
        }
    }
}
