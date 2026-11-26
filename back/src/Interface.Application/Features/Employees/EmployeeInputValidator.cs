using FluentValidation;
using Interface.Domain.Employees;

namespace Interface.Application.Features.Employees;

/// <summary>The employee field rules from the specification, shared by create and update.</summary>
public abstract class EmployeeInputValidator<TInput> : AbstractValidator<TInput>
    where TInput : IEmployeeInput
{
    protected EmployeeInputValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(Employee.FullNameMaxLength)
            .WithMessage($"Full name must be at most {Employee.FullNameMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(Employee.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(Employee.JobTitleMaxLength);

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(Employee.DepartmentMaxLength);

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required.")
            .Must(hireDate => hireDate <= Today(timeProvider))
            .WithMessage("Hire date cannot be in the future.");

        RuleFor(x => x.BaseMonthlySalary)
            .GreaterThan(0).WithMessage("Base monthly salary must be greater than 0.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Base monthly salary can have at most 2 decimal places.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be Active or Inactive.");

        RuleFor(x => x.Allowances).NotNull();
        RuleForEach(x => x.Allowances).SetValidator(new PayComponentValidator());

        RuleFor(x => x.Deductions).NotNull();
        RuleForEach(x => x.Deductions).SetValidator(new PayComponentValidator());
    }

    private static DateOnly Today(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}

public sealed class PayComponentValidator : AbstractValidator<PayComponentDto>
{
    public const int NameMaxLength = 100;

    public PayComponentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(NameMaxLength);

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.")
            .PrecisionScale(18, 2, ignoreTrailingZeros: true)
            .WithMessage("Amount can have at most 2 decimal places.");
    }
}
