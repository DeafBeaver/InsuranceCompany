using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using InsuranceSystem.Core.Entities;
using InsuranceSystem.BLL.Persistence;

namespace InsuranceSystem.BLL.Persistence
{
    /// <summary>
    /// Container class for holding all insurance-related data, including clients, policies, agents, claims, payments, and requests.
    /// </summary>
    public class DatabaseContainer
    {
        public int NextClientId { get; set; } = 1;
        public int NextPolicyId { get; set; } = 1;
        public int NextAgentId { get; set; } = 1;
        public int NextClaimId { get; set; } = 1;
        public int NextPaymentId { get; set; } = 1;
        public int NextRequestId { get; set; } = 1;
        public List<Client> Clients { get; set; } = new();
        public List<Policy> Policies { get; set; } = new();
        public List<Agent> Agents { get; set; } = new();
        public List<Claim> Claims { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public List<Request> Requests { get; set; } = new();
    }

    /// <summary>
    /// Json-based implementation of the IStorage interface, providing methods to save and load insurance data to and from a JSON file.
    /// </summary>
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
