using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Infrastructure.Data.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Core.Entities.Task>
    {
        public void Configure(EntityTypeBuilder<Core.Entities.Task> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Relationships configured in UserConfiguration
        }
    }
}
