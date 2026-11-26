using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Payroll;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Application.Features.Payroll.Queries;
using Interface.Domain.Employees;
using Interface.Domain.Payroll;
using Interface.Domain.Users;
using Interface.UnitTests.TestData;
using NSubstitute;

namespace Interface.UnitTests.Application;

public sealed class GetPayslipQueryHandlerTests
{
    private readonly IEmployeeRepository _employees = Substitute.For<IEmployeeRepository>();
    private readonly IPayrollPolicyProvider _policyProvider = Substitute.For<IPayrollPolicyProvider>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Employee _employee = new EmployeeBuilder().WithBaseSalary(20_000m).Build();

    public GetPayslipQueryHandlerTests()
    {
        _policyProvider.GetCurrentPolicy().Returns(TestPolicies.Standard());
        _employees.GetByIdAsync(_employee.Id, Arg.Any<CancellationToken>()).Returns(_employee);
    }

    private GetPayslipQueryHandler CreateHandler() => new(
        _employees,
        _policyProvider,
        new PayrollCalculator(new ProgressiveTaxCalculator(), new SocialInsuranceCalculator()),
        _currentUser,
        MapperFactory.Create());

    private void SignInAs(string role, Guid? employeeId = null)
    {
        _currentUser.Role.Returns(role);
        _currentUser.IsHr.Returns(role == UserRole.Hr);
        _currentUser.EmployeeId.Returns(employeeId);
    }

    [Fact]
    public async Task Hr_can_view_any_payslip()
    {
        SignInAs(UserRole.Hr);

        var payslip = await CreateHandler().Handle(new GetPayslipQuery(_employee.Id, "2026-03"), CancellationToken.None);

        Assert.Equal("2026-03", payslip.Month);
        Assert.Equal(20_000m, payslip.GrossPay);
    }

    [Fact]
    public async Task Employee_can_view_their_own_payslip()
    {
        SignInAs(UserRole.Employee, _employee.Id);

        var payslip = await CreateHandler().Handle(new GetPayslipQuery(_employee.Id, "2026-03"), CancellationToken.None);

        Assert.Equal(_employee.Id, payslip.EmployeeId);
    }

    [Fact]
    public async Task Employee_cannot_view_someone_elses_payslip_and_nothing_is_loaded()
    {
        SignInAs(UserRole.Employee, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => CreateHandler().Handle(new GetPayslipQuery(_employee.Id, "2026-03"), CancellationToken.None));

        await _employees.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Unknown_employee_returns_not_found()
    {
        SignInAs(UserRole.Hr);

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateHandler().Handle(new GetPayslipQuery(Guid.NewGuid(), "2026-03"), CancellationToken.None));
    }

    [Theory]
    [InlineData("2026-03", true)]
    [InlineData("2026-13", false)]
    [InlineData("March", false)]
    [InlineData("", false)]
    public void Month_must_be_yyyy_MM(string month, bool isValid)
    {
        var result = new GetPayslipQueryValidator().Validate(new GetPayslipQuery(Guid.NewGuid(), month));

        Assert.Equal(isValid, result.IsValid);
    }
}
