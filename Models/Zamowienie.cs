using System.ComponentModel.DataAnnotations;

namespace Proejkt_magazyn.Models;

public class Zamowienie
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string NazwaKlienta { get; set; } = string.Empty;
    
    public DateTime DataZlozenia { get; set; } = DateTime.Now;

    public List<PozycjaZamowienia> Pozycje { get; set; } = new();
}