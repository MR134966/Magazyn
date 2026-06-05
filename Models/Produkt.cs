namespace Proejkt_magazyn.Models;

public class Produkt
{
    public int Id { get; set; }
    public string KodKreskowy { get; set; } = string.Empty;
    public string Nazwa { get; set; } = string.Empty;
    public double WagaKg { get; set; }
    public bool CzyDelikatne { get; set; }
    
    public decimal CenaNetto { get; set; }
    public decimal CenaBrutto { get; set; }
    public bool CzyGazetka { get; set; }

    public string Aleja { get; set; } = string.Empty;
    public int Regal { get; set; }
    public int Poziom { get; set; }
    public int PozycjaX { get; set; }
    public int PozycjaY { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("stock")]
    public int StanMagazynowy { get; set; }
}

