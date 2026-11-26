using Interface.Domain.Employees;
using Interface.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interface.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Email).HasMaxLength(Employee.EmailMaxLength).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(Employee.FullNameMaxLength).IsRequired();
        builder.Property(user => user.Role).HasMaxLength(20).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(512);

        builder.HasIndex(user => user.Email).IsUnique();

        // Deleting an employee also removes the login that belongs to them.
        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(user => user.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
