#if NETCOREAPP2_1
using BeetleX.Tracks;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeetleX.EFCore.Extension
{
#if NETCOREAPP2_1
    public class XDBContext : DbContext
    {
        public XDBContext() : base() { }
        public XDBContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            using (var tack = CodeTrackFactory.Track("Save", CodeTrackLevel.Function, null, "EFCore", "Configuring"))
            {
                base.OnConfiguring(optionsBuilder);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            using (var tack = CodeTrackFactory.Track("Save", CodeTrackLevel.Function, null, "EFCore", "ModelCreating"))
            {
                base.OnModelCreating(modelBuilder);
            }
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            using (var tack = CodeTrackFactory.Track("Save", CodeTrackLevel.Function, null, "EFCore", "DBContext"))
            {
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            using (var tack = CodeTrackFactory.Track("Save Async", CodeTrackLevel.Function, null, "EFCore", "DBContext"))
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }
    }
#endif
}
