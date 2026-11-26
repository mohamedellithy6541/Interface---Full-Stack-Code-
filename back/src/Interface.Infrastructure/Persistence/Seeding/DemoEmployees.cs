using Interface.Domain.Employees;

namespace Interface.Infrastructure.Persistence.Seeding;

internal static class DemoEmployees
{
    public static List<Employee> Create(DateOnly today) =>
    [
        Employee.Create("Sara Ali", "sara.ali@interface.local", "Software Engineer", "Engineering",
            today.AddYears(-3), 25_000m, EmployeeStatus.Active,
            [new PayComponent("Transport", 1_500m), new PayComponent("Housing", 3_000m)],
            [new PayComponent("Medical plan", 400m)]),
        Employee.Create("Omar Hassan", "omar.hassan@interface.local", "Senior Software Engineer", "Engineering",
            today.AddYears(-5), 40_000m, EmployeeStatus.Active,
            [new PayComponent("Transport", 1_500m)]),
        Employee.Create("Mona Youssef", "mona.youssef@interface.local", "QA Engineer", "Engineering",
            today.AddMonths(-8), 18_000m, EmployeeStatus.Active),
        Employee.Create("Karim Adel", "karim.adel@interface.local", "Engineering Manager", "Engineering",
            today.AddYears(-7), 60_000m, EmployeeStatus.Active,
            [new PayComponent("Housing", 5_000m), new PayComponent("Phone", 500m)]),
        Employee.Create("Nour Ibrahim", "nour.ibrahim@interface.local", "HR Specialist", "Human Resources",
            today.AddYears(-2), 15_000m, EmployeeStatus.Active,
            [new PayComponent("Transport", 1_000m)]),
        Employee.Create("Ahmed Mostafa", "ahmed.mostafa@interface.local", "HR Manager", "Human Resources",
            today.AddYears(-6), 35_000m, EmployeeStatus.Active),
        Employee.Create("Laila Samir", "laila.samir@interface.local", "Accountant", "Finance",
            today.AddYears(-4), 20_000m, EmployeeStatus.Active,
            [new PayComponent("Transport", 1_000m)],
            [new PayComponent("Loan repayment", 2_000m)]),
        Employee.Create("Youssef Tarek", "youssef.tarek@interface.local", "Financial Analyst", "Finance",
            today.AddYears(-1), 22_000m, EmployeeStatus.Active),
        Employee.Create("Hana Mahmoud", "hana.mahmoud@interface.local", "Sales Representative", "Sales",
            today.AddMonths(-14), 12_000m, EmployeeStatus.Active,
            [new PayComponent("Transport", 800m)]),
        Employee.Create("Mostafa Gamal", "mostafa.gamal@interface.local", "Sales Manager", "Sales",
            today.AddYears(-5), 45_000m, EmployeeStatus.Active,
            [new PayComponent("Car", 4_000m)]),
        Employee.Create("Dina Fathy", "dina.fathy@interface.local", "Marketing Specialist", "Marketing",
            today.AddYears(-2), 16_000m, EmployeeStatus.Active),
        Employee.Create("Tamer Nabil", "tamer.nabil@interface.local", "Office Assistant", "Operations",
            today.AddYears(-3), 8_000m, EmployeeStatus.Inactive)
    ];
}
