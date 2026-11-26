using AutoMapper;
using Interface.Api.Authentication;
using Interface.Api.Contracts;
using Interface.Application.Common.Models;
using Interface.Application.Features.Employees;
using Interface.Application.Features.Employees.Commands;
using Interface.Application.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Interface.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize(Policy = AuthorizationPolicies.HrOnly)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
public sealed class EmployeesController(ISender sender, IMapper mapper) : ControllerBase
{
    /// <summary>Lists employees, paged. Search matches name or email; department and status filter exactly.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<EmployeeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<PagedResult<EmployeeDto>> GetEmployees(
        [FromQuery] GetEmployeesQuery query,
        CancellationToken cancellationToken) =>
        sender.Send(query, cancellationToken);

    /// <summary>Lists the distinct departments in use.</summary>
    [HttpGet("departments")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<string>> GetDepartments(CancellationToken cancellationToken) =>
        sender.Send(new GetDepartmentsQuery(), cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<EmployeeDto> GetEmployee(Guid id, CancellationToken cancellationToken) =>
        sender.Send(new GetEmployeeByIdQuery(id), cancellationToken);

    [HttpPost]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        EmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee = await sender.Send(mapper.Map<CreateEmployeeCommand>(request), cancellationToken);
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<EmployeeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<EmployeeDto> UpdateEmployee(Guid id, EmployeeRequest request, CancellationToken cancellationToken) =>
        sender.Send(mapper.Map<UpdateEmployeeCommand>(request) with { Id = id }, cancellationToken);

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }
}
