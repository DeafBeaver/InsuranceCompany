using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.BLL.Persistence;

namespace InsuranceSystem.BLL.Persistence
{
    public class DatabaseContainer
    {
        public int NextClientId { get; set; } = 1;
        public int NextPolicyId { get; set; } = 1;
        public int NextAgentId { get; set; } = 1;
        public int NextClaimId { get; set; } = 1;
        public List<Client> Clients { get; set; } = new();
        public List<Policy> Policies { get; set; } = new();
        public List<Agent> Agents { get; set; } = new();
        public List<Claim> Claims { get; set; } = new();
    }

    public class JsonStorage : IStorage
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
