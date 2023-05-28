using Alert_server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Alert_server.Data
{
    public class ApplicatioinContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(@"Server=vehicleplus.cloud;Port=5435;Database=TCU;User Id=postgres;Password=postgres;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
