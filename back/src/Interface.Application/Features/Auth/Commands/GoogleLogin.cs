using AutoMapper;
using FluentValidation;
using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using MediatR;

namespace Interface.Application.Features.Auth.Commands;

/// <summary>
/// Signs in with a Google ID token obtained by the front end from Google Identity Services.
/// The token is verified on the server, then exchanged for this API's own JWT.
/// Only Google accounts whose email matches an existing user may sign in.
/// </summary>
public sealed record GoogleLoginCommand(string IdToken) : IRequest<AuthResponse>;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken).NotEmpty();
    }
}

public sealed class GoogleLoginCommandHandler(
    IGoogleTokenValidator googleTokenValidator,
    IUserRepository users,
    IJwtTokenGenerator tokenGenerator,
    IMapper mapper) : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(GoogleLoginCommand command, CancellationToken cancellationToken)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(command.IdToken, cancellationToken)
                         ?? throw new AuthenticationFailedException("The Google sign-in token is invalid or has expired.");

        if (!googleUser.EmailVerified)
            throw new AuthenticationFailedException("The Google account email address is not verified.");

        var user = await users.GetByEmailAsync(googleUser.Email, cancellationToken)
                   ?? throw new AuthenticationFailedException(
                       $"No account is registered for {googleUser.Email}. Ask HR to create one.");

        var token = tokenGenerator.Generate(user);
        return new AuthResponse
        {
            AccessToken = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            User = mapper.Map<CurrentUserDto>(user)
        };
    }
}
