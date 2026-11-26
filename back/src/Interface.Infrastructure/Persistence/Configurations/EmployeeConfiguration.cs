using Interface.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Interface.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public const int MoneyPrecision = 18;
    public const int MoneyScale = 2;

    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(employee => employee.Id);
        builder.Property(employee => employee.Id).ValueGeneratedNever();

        builder.Property(employee => employee.FullName).HasMaxLength(Employee.FullNameMaxLength).IsRequired();
        builder.Property(employee => employee.Email).HasMaxLength(Employee.EmailMaxLength).IsRequired();
        builder.Property(employee => employee.JobTitle).HasMaxLength(Employee.JobTitleMaxLength).IsRequired();
        builder.Property(employee => employee.Department).HasMaxLength(Employee.DepartmentMaxLength).IsRequired();
        builder.Property(employee => employee.BaseMonthlySalary).HasPrecision(MoneyPrecision, MoneyScale);
        builder.Property(employee => employee.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(employee => employee.Email).IsUnique();
        builder.HasIndex(employee => employee.Department);
        builder.HasIndex(employee => employee.FullName);

        builder.Ignore(employee => employee.IsActive);

        // Allowances and deductions are always loaded and saved together with their employee,
        // so they are stored as JSON columns instead of separate tables.
        builder.OwnsMany(employee => employee.Allowances, allowance => ConfigurePayComponent(allowance, "Allowances"));
        builder.OwnsMany(employee => employee.Deductions, deduction => ConfigurePayComponent(deduction, "Deductions"));
        builder.Navigation(employee => employee.Allowances).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(employee => employee.Deductions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigurePayComponent(OwnedNavigationBuilder<Employee, PayComponent> component, string columnName)
    {
        component.ToJson(columnName);
        component.Property(item => item.Name).IsRequired();
        component.Property(item => item.Amount).HasPrecision(MoneyPrecision, MoneyScale);
    }
}
