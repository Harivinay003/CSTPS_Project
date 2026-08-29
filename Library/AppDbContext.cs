using Microsoft.EntityFrameworkCore;
using VirtualEMS.Library;
using System.Configuration;

namespace VirtualEMS.DataServices
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
             : base(options)
        {
        }
        public DbSet<AlarmTag> AlarmTags { get; set; }
        public DbSet<AlarmParameter> AlarmParameters { get; set; }
        public DbSet<IODevice> IODevices { get; set; }
        public DbSet<FieldDevice> FieldDevices { get; set; }
        public DbSet<SerialDevice> SerialDevices { get; set; }
        public DbSet<SerialDeviceDriver> SerialDeviceDrivers { get; set; }
        public DbSet<SerialDeviceParameter> SerialDeviceParameters { get; set; }
        public DbSet<SerialDeviceReadBlock> SerialDeviceReadBlocks { get; set; }
        public DbSet<SerialDeviceRegister> SerialDeviceRegisters { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TrendTag>  TrendTags { get; set; }
        public DbSet<TrendParameter> TrendParameters { get; set; }
        public DbSet<Permits> Permits { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           if(!optionsBuilder.IsConfigured)
           {
                // Configure the context here if not already configured
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["ConfigDBConnString"].ConnectionString);
            }
        }
    }
}
