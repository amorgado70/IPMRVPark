using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace IPMRVPark.DAL.Data
{
    class DataContext : DbContext
    {
        public DataContext() : base("ipmrvparkDbContext")
        {
        }
    }
}
