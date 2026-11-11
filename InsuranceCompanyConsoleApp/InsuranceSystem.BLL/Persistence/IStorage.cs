using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.BLL.Persistence
{
    /// <summary>
    /// Interface for storage operations, providing methods to save and load insurance data.
    /// </summary>
    public interface IStorage
    {
        public void Save(DatabaseContainer data);
        public DatabaseContainer Load();
    }
}
