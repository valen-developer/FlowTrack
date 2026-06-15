using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowTrack.WorkManagement.Workspaces.Infrastructure.Persistence
{
    internal class WorkspaceEntityConf : IEntityTypeConfiguration<WorkspaceEntity>
    {
        public void Configure(EntityTypeBuilder<WorkspaceEntity> builder)
        {
            builder.ToTable("workspaces");
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).IsRequired();
            builder.Property(w => w.OwnerId).IsRequired();

            builder.HasIndex(w => w.Name).IsUnique();
        }
    }
}
