using Microsoft.AspNetCore.Authorization;

namespace WebApp_UnderTheHood.Authorization;

public class HrManagerProbationRequirement : IAuthorizationRequirement
{
    public HrManagerProbationRequirement(int probationMonths)
    {
        ProbationMonths = probationMonths;
    }

    public int ProbationMonths { get; }
}

public class HrManagerProbationRequirementHandler : AuthorizationHandler<HrManagerProbationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HrManagerProbationRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == "EmploymentDate"))
        {
            return Task.CompletedTask;
        }

        if (DateTime.TryParse(context.User.FindFirst(c => c.Type == "EmploymentDate")?.Value, out DateTime employmentDate))
        {
            var monthsEmployed = ((DateTime.UtcNow.Year - employmentDate.Year) * 12) + DateTime.UtcNow.Month - employmentDate.Month;

            if (monthsEmployed >= requirement.ProbationMonths)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
