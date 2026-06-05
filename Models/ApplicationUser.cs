using Microsoft.AspNetCore.Identity;

namespace Proejkt_magazyn.Models;

public class ApplicationUser : IdentityUser
{
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public string Stanowisko { get; set; } = string.Empty; 
    
    
    public string Sklep { get; set; } = string.Empty; 
    
    
    public string NumerSkanera { get; set; } = string.Empty; 
    public double DystansDzisiaj { get; set; } = 0;
}