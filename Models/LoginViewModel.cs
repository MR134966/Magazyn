using System.ComponentModel.DataAnnotations;

namespace Proejkt_magazyn.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Adres e-mail jest wymagany.")]
    [EmailAddress(ErrorMessage = "Niepoprawny format adresu e-mail.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [DataType(DataType.Password)]
    public string Haslo { get; set; } = string.Empty;
}
