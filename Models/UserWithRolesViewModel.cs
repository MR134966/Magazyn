using Microsoft.AspNetCore.Identity;

namespace Proejkt_magazyn.Models;

public class UserWithRolesViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
    public bool CzyZablokowany { get; set; }
}