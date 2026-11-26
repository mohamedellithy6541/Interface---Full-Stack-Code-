using Interface.Application.Abstractions.Authentication;
using Interface.Application.Abstractions.Persistence;
using Interface.Application.Common.Exceptions;
using Interface.Application.Features.Auth.Commands;
using Interface.Domain.Users;
using NSubstitute;

namespace Interface.UnitTests.Application;

public sealed class AuthHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IGoogleTokenValidator _googleTokenValidator = Substitute.For<IGoogleTokenValidator>();
    private readonly User _hrUser = CreateHrUser();

    public AuthHandlerTests()
    {
        _tokenGenerator.Generate(Arg.Any<User>()).Returns(new AccessToken("jwt", DateTimeOffset.UnixEpoch));
        _users.GetByEmailAsync("hr@interface.local", Arg.Any<CancellationToken>()).Returns(_hrUser);
    }

    private static User CreateHrUser()
    {
        var user = User.Create("hr@interface.local", "HR", UserRole.Hr);
        user.SetPasswordHash("hash");
        return user;
    }

    private LoginCommandHandler LoginHandler() => new(_users, _passwordHasher, _tokenGenerator, MapperFactory.Create());

    private GoogleLoginCommandHandler GoogleHandler() =>
        new(_googleTokenValidator, _users, _tokenGenerator, MapperFactory.Create());

    [Fact]
    public async Task Login_with_the_right_password_returns_a_token_and_the_user()
    {
        _passwordHasher.Verify(_hrUser, "correct").Returns(true);

        var response = await LoginHandler().Handle(new LoginCommand("hr@interface.local", "correct"), CancellationToken.None);

        Assert.Equal("jwt", response.AccessToken);
        Assert.Equal(UserRole.Hr, response.User.Role);
    }

    [Fact]
    public async Task Login_with_a_wrong_password_fails()
    {
        _passwordHasher.Verify(_hrUser, "wrong").Returns(false);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => LoginHandler().Handle(new LoginCommand("hr@interface.local", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Login_with_an_unknown_email_fails_with_the_same_message()
    {
        var exception = await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => LoginHandler().Handle(new LoginCommand("nobody@interface.local", "x"), CancellationToken.None));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task Google_login_for_a_registered_verified_email_returns_a_token()
    {
        _googleTokenValidator.ValidateAsync("google-token", Arg.Any<CancellationToken>())
            .Returns(new ExternalUserInfo("hr@interface.local", "HR", EmailVerified: true));

        var response = await GoogleHandler().Handle(new GoogleLoginCommand("google-token"), CancellationToken.None);

        Assert.Equal("jwt", response.AccessToken);
    }

    [Fact]
    public async Task Google_login_with_an_invalid_token_fails()
    {
        _googleTokenValidator.ValidateAsync("bad", Arg.Any<CancellationToken>()).Returns((ExternalUserInfo?)null);

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => GoogleHandler().Handle(new GoogleLoginCommand("bad"), CancellationToken.None));
    }

    [Fact]
    public async Task Google_login_with_an_unverified_email_fails()
    {
        _googleTokenValidator.ValidateAsync("google-token", Arg.Any<CancellationToken>())
            .Returns(new ExternalUserInfo("hr@interface.local", "HR", EmailVerified: false));

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => GoogleHandler().Handle(new GoogleLoginCommand("google-token"), CancellationToken.None));
    }

    [Fact]
    public async Task Google_login_for_an_unregistered_email_fails()
    {
        _googleTokenValidator.ValidateAsync("google-token", Arg.Any<CancellationToken>())
            .Returns(new ExternalUserInfo("stranger@gmail.com", "Stranger", EmailVerified: true));

        await Assert.ThrowsAsync<AuthenticationFailedException>(
            () => GoogleHandler().Handle(new GoogleLoginCommand("google-token"), CancellationToken.None));
    }
}
