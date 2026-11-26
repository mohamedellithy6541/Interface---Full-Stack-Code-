using FluentValidation;
using Interface.Application.Common.Behaviors;
using Interface.Application.Features.Employees.Commands;

namespace Interface.UnitTests.Application;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Invalid_request_throws_and_never_reaches_the_handler()
    {
        var behavior = new ValidationBehavior<DeleteEmployeeCommand, MediatR.Unit>([new DeleteEmployeeCommandValidator()]);
        var handlerCalled = false;

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new DeleteEmployeeCommand(Guid.Empty),
            _ => { handlerCalled = true; return Task.FromResult(MediatR.Unit.Value); },
            CancellationToken.None));

        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task Valid_request_is_passed_to_the_handler()
    {
        var behavior = new ValidationBehavior<DeleteEmployeeCommand, MediatR.Unit>([new DeleteEmployeeCommandValidator()]);
        var handlerCalled = false;

        await behavior.Handle(
            new DeleteEmployeeCommand(Guid.NewGuid()),
            _ => { handlerCalled = true; return Task.FromResult(MediatR.Unit.Value); },
            CancellationToken.None);

        Assert.True(handlerCalled);
    }
}
