using Microsoft.EntityFrameworkCore;
using PermissionAPI.Models.SpResults;

namespace PermissionAPI.Data;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configure keyless entity for SP result
        modelBuilder.Entity<UserPermissionResult>(entity =>
        {
            entity.HasNoKey();
            entity.ToView(null); // Not mapped to any table/view
        });
    }
}
