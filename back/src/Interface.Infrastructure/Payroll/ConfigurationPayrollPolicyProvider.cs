using Interface.Application.Abstractions.Payroll;
using Interface.Domain.Payroll;
using Microsoft.Extensions.Options;

namespace Interface.Infrastructure.Payroll;

/// <summary>Reads the payroll policy from configuration; picks up changes to appsettings without a restart.</summary>
internal sealed class ConfigurationPayrollPolicyProvider(IOptionsMonitor<PayrollOptions> options) : IPayrollPolicyProvider
{
    public PayrollPolicy GetCurrentPolicy() => options.CurrentValue.ToPolicy();
}
