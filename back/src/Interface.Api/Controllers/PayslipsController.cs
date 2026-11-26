using Interface.Application.Features.Payroll;
using Interface.Application.Features.Payroll.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/payslip")]
[Produces("application/json")]
public sealed class PayslipsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Calculates the monthly payslip. HR can read any employee's payslip; an employee only their own.
    /// </summary>
    /// <param name="employeeId">The employee.</param>
    /// <param name="month">Payroll month as yyyy-MM, e.g. 2026-03.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet]
    [ProducesResponseType<PayslipDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<PayslipDto> GetPayslip(Guid employeeId, [FromQuery] string month, CancellationToken cancellationToken) =>
        sender.Send(new GetPayslipQuery(employeeId, month), cancellationToken);
}
