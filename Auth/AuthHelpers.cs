using System.Security.Claims;

namespace SupportDeskBE.Auth
{
    public static class AuthHelpers
    {
        public static string UserId(ClaimsPrincipal user) =>
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public static bool IsStaff(ClaimsPrincipal user) =>
            user.IsInRole("Agent") || user.IsInRole("Admin");
    }
}

