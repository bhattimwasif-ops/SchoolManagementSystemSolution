using Hangfire.Dashboard;


public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        bool isAuth= httpContext.User.Identity?.IsAuthenticated ?? false; // Allow only authenticated users
        if (isAuth)
            return httpContext.User.IsInRole("Admin");
        return false; // Deny access for unauthenticated users
    }
}