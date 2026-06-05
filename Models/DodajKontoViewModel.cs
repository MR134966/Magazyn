using System.ComponentModel.DataAnnotations;

namespace Proejkt_magazyn.Models;

public class DodajKontoViewModel
{
    [Required(ErrorMessage = "E-mail jest wymagany.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hasło jest wymagane.")]
    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    public string Haslo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Imię jest wymagane.")]
    public string Imie { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane.")]
    public string Nazwisko { get; set; } = string.Empty;

    [Required(ErrorMessage = "Wybierz typ konta.")]
    public string TypKonta { get; set; } = "Pracownik";

    public string? Sklep { get; set; }

    public string? NumerSkanera { get; set; }
}
