using AutoMapper;
using Interface.Domain.Payroll;

namespace Interface.Application.Features.Payroll;

public sealed record PayslipLineDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public sealed record PayslipDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string Month { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal BaseSalary { get; init; }
    public IReadOnlyList<PayslipLineDto> Allowances { get; init; } = [];
    public decimal GrossPay { get; init; }
    public decimal TaxableIncome { get; init; }
    public decimal IncomeTax { get; init; }
    public decimal SocialInsurance { get; init; }
    public IReadOnlyList<PayslipLineDto> OtherDeductions { get; init; } = [];
    public decimal TotalDeductions { get; init; }
    public decimal NetPay { get; init; }
}

public sealed class PayrollMappingProfile : Profile
{
    public PayrollMappingProfile()
    {
        CreateMap<PayslipLine, PayslipLineDto>();
        CreateMap<Payslip, PayslipDto>()
            .ForMember(dto => dto.Month, options => options.MapFrom(payslip => payslip.Period.ToString()));
    }
}
