using FlowTrack.Iam.Domain;
using FlowTrack.Shared;
using FlowTrack.Shared.Domain;

namespace FlowTrack.Iam.Services;

[Service]
public sealed class AuthCookieSetter(
    IHttpContextAccessor contextAccessor,
    IDateTimeProvider dateTimeProvider
)
{
    public void SetAuthCookies(SigninSuccess signinSuccess)
    {
        var context =
            contextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext available");

        var now = dateTimeProvider.Now;

        context.Response.Cookies.Append(
            "ACCESS_TOKEN",
            signinSuccess.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = now.AddMinutes(60),
            }
        );

        context.Response.Cookies.Append(
            "REFRESH_TOKEN",
            signinSuccess.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = now.AddMinutes(60 * 24 * 30),
            }
        );
    }
}
