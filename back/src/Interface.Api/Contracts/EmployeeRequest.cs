using AutoMapper;
using Interface.Application.Features.Employees;
using Interface.Application.Features.Employees.Commands;
using Interface.Domain.Employees;

namespace Interface.Api.Contracts;

/// <summary>Request body for creating or updating an employee.</summary>
public sealed record EmployeeRequest
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public DateOnly HireDate { get; init; }
    public decimal BaseMonthlySalary { get; init; }
    public EmployeeStatus Status { get; init; } = EmployeeStatus.Active;
    public IReadOnlyList<PayComponentDto> Allowances { get; init; } = [];
    public IReadOnlyList<PayComponentDto> Deductions { get; init; } = [];
}

/// <summary>Maps HTTP contracts to application commands, keeping the web shape separate from the use cases.</summary>
public sealed class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<EmployeeRequest, CreateEmployeeCommand>();
        CreateMap<EmployeeRequest, UpdateEmployeeCommand>()
            .ForMember(command => command.Id, options => options.Ignore());
    }
}
