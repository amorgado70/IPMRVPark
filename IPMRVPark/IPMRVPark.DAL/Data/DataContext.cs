using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace IPMRVPark.Contracts.Data
{
    public class DataContext : DbContext
    {
        public DataContext() : base("ipmrvparkDbContext")
        {
        }
    }
}
