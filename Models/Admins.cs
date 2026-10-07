using Microsoft.AspNetCore.Identity;

namespace ChurchManagementMVC.Models
{
    public class Admins : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}