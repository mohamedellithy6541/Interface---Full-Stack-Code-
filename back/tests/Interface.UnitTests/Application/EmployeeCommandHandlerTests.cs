using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Application.Features.Employees;
using Interface.Application.Features.Employees.Commands;
using Interface.Domain.Employees;
using NSubstitute;

namespace Interface.UnitTests.Application;

public sealed class EmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _employees = Substitute.For<IEmployeeRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private static CreateEmployeeCommand NewEmployee() => new()
    {
        FullName = "  Sara Ali ",
        Email = "Sara.Ali@Interface.local",
        JobTitle = "Engineer",
        Department = "Engineering",
        HireDate = new DateOnly(2023, 1, 1),
        BaseMonthlySalary = 25_000m,
        Allowances = [new PayComponentDto { Name = "Transport", Amount = 1_500m }]
    };

    [Fact]
    public async Task Create_saves_the_employee_with_normalized_values()
    {
        var handler = new CreateEmployeeCommandHandler(_employees, _unitOfWork, MapperFactory.Create());

        var result = await handler.Handle(NewEmployee(), CancellationToken.None);

        _employees.Received(1).Add(Arg.Is<Employee>(employee =>
            employee.FullName == "Sara Ali"
            && employee.Email == "sara.ali@interface.local"
            && employee.Allowances.Single().Amount == 1_500m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("sara.ali@interface.local", result.Email);
    }

    [Fact]
    public async Task Create_with_an_email_already_in_use_returns_conflict_and_saves_nothing()
    {
        _employees.EmailExistsAsync("Sara.Ali@Interface.local", null, Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateEmployeeCommandHandler(_employees, _unitOfWork, MapperFactory.Create());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(NewEmployee(), CancellationToken.None));

        _employees.DidNotReceive().Add(Arg.Any<Employee>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_of_a_missing_employee_returns_not_found()
    {
        var handler = new UpdateEmployeeCommandHandler(_employees, _unitOfWork, MapperFactory.Create());
        var command = new UpdateEmployeeCommand { Id = Guid.NewGuid(), FullName = "X", Email = "x@y.z", BaseMonthlySalary = 1m };

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Update_may_keep_the_employees_own_email()
    {
        var existing = Employee.Create("Sara Ali", "sara@interface.local", "Engineer", "Engineering",
            new DateOnly(2023, 1, 1), 20_000m, EmployeeStatus.Active);
        _employees.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        var handler = new UpdateEmployeeCommandHandler(_employees, _unitOfWork, MapperFactory.Create());

        var result = await handler.Handle(new UpdateEmployeeCommand
        {
            Id = existing.Id,
            FullName = "Sara Ali",
            Email = "sara@interface.local",
            JobTitle = "Senior Engineer",
            Department = "Engineering",
            HireDate = new DateOnly(2023, 1, 1),
            BaseMonthlySalary = 30_000m,
            Status = EmployeeStatus.Inactive
        }, CancellationToken.None);

        await _employees.Received(1).EmailExistsAsync("sara@interface.local", existing.Id, Arg.Any<CancellationToken>());
        Assert.Equal("Senior Engineer", result.JobTitle);
        Assert.Equal(EmployeeStatus.Inactive, result.Status);
    }

    [Fact]
    public async Task Delete_of_a_missing_employee_returns_not_found()
    {
        var handler = new DeleteEmployeeCommandHandler(_employees, _unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteEmployeeCommand(Guid.NewGuid()), CancellationToken.None));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
