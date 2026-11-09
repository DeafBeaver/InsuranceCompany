using System;
using System.Collections.Generic;
using System.Text;

namespace InsuranceSystem.BLL.Persistence
{
    public interface IStorage
    {
        public void Save(DatabaseContainer data);
        public DatabaseContainer Load();
    }
}
