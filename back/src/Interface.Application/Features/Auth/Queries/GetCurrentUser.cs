using AutoMapper;
using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using MediatR;

namespace Interface.Application.Features.Auth.Queries;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    IUserRepository users,
    IMapper mapper) : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId)
            throw new AuthenticationFailedException("You are not signed in.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new AuthenticationFailedException("Your account no longer exists.");

        return mapper.Map<CurrentUserDto>(user);
    }
}
