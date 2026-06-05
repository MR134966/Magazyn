using System.ComponentModel.DataAnnotations;

namespace Proejkt_magazyn.Models;

public class UstawieniaViewModel
{
    [Required(ErrorMessage = "Imię jest wymagane.")]
    public string Imie { get; set; }

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    public string Nazwisko { get; set; }
    
    [Required(ErrorMessage = "Email jest wymagany.")]
    [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
    public string Email { get; set; }
    
    [DataType(DataType.Password)]
    public string? AktualneHaslo { get; set; }

    [DataType(DataType.Password)]
    public string? NoweHaslo { get; set; }

    [DataType(DataType.Password)]
    [Compare("NoweHaslo", ErrorMessage = "Hasła nie pasują do siebie.")]
    public string? PotwierdzNoweHaslo { get; set; }
}