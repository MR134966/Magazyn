using System.ComponentModel.DataAnnotations;

namespace Proejkt_magazyn.Models;

public class PozycjaZamowienia
{
    [Key]
    public int Id { get; set; }
    
    public int ZamowienieId { get; set; }
    public Zamowienie Zamowienie { get; set; }
    
    public int ProduktId { get; set; }
    public Produkt Produkt { get; set; }
    
    public int IloscSztuk { get; set; }
    
    public bool CzyZebrane { get; set; } = false; 
}