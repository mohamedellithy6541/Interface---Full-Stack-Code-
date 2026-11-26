using AutoMapper;
using FluentValidation;
using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using MediatR;

namespace Interface.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator tokenGenerator,
    IMapper mapper) : IRequestHandler<LoginCommand, AuthResponse>
{
    // One message for every failure, so a caller cannot tell which emails are registered.
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);

        if (user?.PasswordHash is null || !passwordHasher.Verify(user, command.Password))
            throw new AuthenticationFailedException(InvalidCredentialsMessage);

        var token = tokenGenerator.Generate(user);
        return new AuthResponse
        {
            AccessToken = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = mapper.Map<CurrentUserDto>(user)
        };
    }
}
