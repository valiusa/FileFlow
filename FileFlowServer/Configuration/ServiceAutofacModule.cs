using Autofac;
using Data;
using Microsoft.EntityFrameworkCore;

namespace FileFlowServer.Configuration
{
    public class ServiceAutofacModule : Module
    {
        private string connectionString;

        public ServiceAutofacModule()
        {

        }

        public ServiceAutofacModule(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Register DbContext
            builder.RegisterType<FileFlowDbContext>().WithParameter("options", DbContextOptionsFactory.Get(this.connectionString)).InstancePerLifetimeScope();

            
        }
    }

    public class DbContextOptionsFactory
    {
        public static DbContextOptions<FileFlowDbContext> Get(string connectionString)
        {
            var builder = new DbContextOptionsBuilder<FileFlowDbContext>();

            DbContextConfigurer.Configure(builder, connectionString);

            return builder.Options;
        }
    }

    public class DbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<FileFlowDbContext> builder,
            string connectionString)
        {
            builder.UseSqlServer(connectionString, o => o.UseCompatibilityLevel(120));
        }
    }
}
