using System;
using System.Collections.Generic;
using InsuranceCompany.Core;

namespace InsuranceCompany.BLL.Services
{
    public class InsuranceCompany
    {
        private readonly JsonStorage _storage;
        private readonly DatabaseContainer _db;

        public InsuranceManager(string filePath = "insurance_data.json")
        {
            _storage = new JsonStorage(filePath);
            _db = _storage.Load();
        }

        public void AddClient(Client client)
        {
            client.Id = _db.Clients.Any() ? _db.Clients.Max(c => c.Id) + 1 : 1;
            _db.Clients.Add(client);
            SaveChanges();
        }
    }
}
