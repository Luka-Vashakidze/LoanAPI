using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Data;

namespace Project.Tests
{
    public static class TestHelpers
    {
        public static LoanDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<LoanDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new LoanDbContext(options);
        }
    }
}
