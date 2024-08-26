using Microsoft.AspNetCore.Authorization;

namespace PolicyAuthWebAPI.CustomRequirement
{
    public class MinimunAgeRequirement(int Age) : IAuthorizationRequirement
    {
    }
}
