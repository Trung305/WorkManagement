using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;

namespace WorkManagement.Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Channel)
                .HasMaxLength(20);

            builder.Property(n => n.IsRead)
                .HasDefaultValue(false);

            builder.Property(n => n.IsSent)
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Relationships
            builder.HasOne(n => n.Task)
                .WithMany(t => t.Notifications)
                .HasForeignKey(n => n.TaskId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
