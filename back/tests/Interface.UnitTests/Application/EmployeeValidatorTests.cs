using FluentValidation.TestHelper;
using Interface.Application.Features.Employees;
using Interface.Application.Features.Employees.Commands;
using Interface.Domain.Employees;
using Microsoft.Extensions.Time.Testing;

namespace Interface.UnitTests.Application;

public sealed class EmployeeValidatorTests
{
    private static readonly DateOnly Today = new(2026, 3, 15);

    private readonly CreateEmployeeCommandValidator _validator =
        new(new FakeTimeProvider(new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero)));

    private static CreateEmployeeCommand ValidCommand() => new()
    {
        FullName = "Sara Ali",
        Email = "sara.ali@interface.local",
        JobTitle = "Software Engineer",
        Department = "Engineering",
        HireDate = new DateOnly(2023, 1, 1),
        BaseMonthlySalary = 25_000m,
        Status = EmployeeStatus.Active,
        Allowances = [new PayComponentDto { Name = "Transport", Amount = 1_500m }]
    };

    [Fact]
    public void A_complete_valid_employee_passes()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Full_name_is_required(string fullName)
    {
        _validator.TestValidate(ValidCommand() with { FullName = fullName })
            .ShouldHaveValidationErrorFor(command => command.FullName);
    }

    [Fact]
    public void Full_name_may_be_exactly_100_characters_but_not_101()
    {
        _validator.TestValidate(ValidCommand() with { FullName = new string('a', 100) })
            .ShouldNotHaveValidationErrorFor(command => command.FullName);
        _validator.TestValidate(ValidCommand() with { FullName = new string('a', 101) })
            .ShouldHaveValidationErrorFor(command => command.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-at.com")]
    public void Email_must_be_present_and_valid(string email)
    {
        _validator.TestValidate(ValidCommand() with { Email = email })
            .ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact]
    public void Job_title_and_department_are_required()
    {
        var result = _validator.TestValidate(ValidCommand() with { JobTitle = "", Department = "" });

        result.ShouldHaveValidationErrorFor(command => command.JobTitle);
        result.ShouldHaveValidationErrorFor(command => command.Department);
    }

    [Fact]
    public void Hire_date_today_is_allowed()
    {
        _validator.TestValidate(ValidCommand() with { HireDate = Today })
            .ShouldNotHaveValidationErrorFor(command => command.HireDate);
    }

    [Fact]
    public void Hire_date_in_the_future_is_rejected()
    {
        _validator.TestValidate(ValidCommand() with { HireDate = Today.AddDays(1) })
            .ShouldHaveValidationErrorFor(command => command.HireDate)
            .WithErrorMessage("Hire date cannot be in the future.");
    }

    [Fact]
    public void Missing_hire_date_is_rejected()
    {
        _validator.TestValidate(ValidCommand() with { HireDate = default })
            .ShouldHaveValidationErrorFor(command => command.HireDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Base_salary_must_be_greater_than_zero(decimal salary)
    {
        _validator.TestValidate(ValidCommand() with { BaseMonthlySalary = salary })
            .ShouldHaveValidationErrorFor(command => command.BaseMonthlySalary);
    }

    [Fact]
    public void Smallest_positive_salary_is_allowed()
    {
        _validator.TestValidate(ValidCommand() with { BaseMonthlySalary = 0.01m })
            .ShouldNotHaveValidationErrorFor(command => command.BaseMonthlySalary);
    }

    [Fact]
    public void Base_salary_with_more_than_two_decimals_is_rejected()
    {
        _validator.TestValidate(ValidCommand() with { BaseMonthlySalary = 100.005m })
            .ShouldHaveValidationErrorFor(command => command.BaseMonthlySalary);
    }

    [Fact]
    public void Status_must_be_active_or_inactive()
    {
        _validator.TestValidate(ValidCommand() with { Status = (EmployeeStatus)99 })
            .ShouldHaveValidationErrorFor(command => command.Status);
    }

    [Fact]
    public void Allowances_need_a_name_and_a_non_negative_amount()
    {
        var command = ValidCommand() with { Allowances = [new PayComponentDto { Name = "", Amount = -5m }] };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Allowances[0].Name");
        result.ShouldHaveValidationErrorFor("Allowances[0].Amount");
    }

    [Fact]
    public void A_zero_allowance_is_allowed()
    {
        _validator.TestValidate(ValidCommand() with { Allowances = [new PayComponentDto { Name = "Bonus", Amount = 0m }] })
            .ShouldNotHaveAnyValidationErrors();
    }
}
