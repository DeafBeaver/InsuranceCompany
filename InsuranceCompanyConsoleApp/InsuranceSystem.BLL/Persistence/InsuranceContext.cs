using InsuranceSystem.BLL.Persistence;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Text;

namespace InsuranceSystem.BLL.Persistence
{
    /// <summary>
    /// Represents the context for managing insurance data.
    /// </summary>
    public class InsuranceContext
    {
        private readonly IStorage _storage;
        public DatabaseContainer Database { get; }
        public InsuranceContext(IStorage storage)
        {
            _storage = storage;
            Database = _storage.Load();
        }
        public void Save()
        {
            _storage.Save(Database);
        }
    }
}
