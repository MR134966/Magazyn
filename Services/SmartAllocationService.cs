using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Services;

public class PoradaBHP
{
    public int PozycjaId { get; set; }
    public string InstrukcjaUlozenia { get; set; } = string.Empty;
    public string SprzetDedykowany { get; set; } = string.Empty;
    public bool AlertWysokosci { get; set; }
}

public class WynikOptymalizacji
{
    public double SzacowanyDystansMetry { get; set; }
    public List<PozycjaZamowienia> Trasa { get; set; } = new();
    public Dictionary<int, PoradaBHP> InstrukcjeDlaTowarow { get; set; } = new();
}

public class SmartAllocationService
{
    public WynikOptymalizacji AnalizujZamowienie(Zamowienie zamowienie)
    {
        var wynik = new WynikOptymalizacji();
        var doPrzetworzenia = zamowienie.Pozycje.ToList(); 
        
        int aktualnyX = 5;
        int aktualnyY = 0;

        while (doPrzetworzenia.Any())
        {
            var najblizszy = doPrzetworzenia
                .OrderBy(p => ObliczDystansWiezowy(aktualnyX, aktualnyY, p.Produkt.PozycjaX, p.Produkt.PozycjaY))
                .First();

            double dystans = ObliczDystansWiezowy(aktualnyX, aktualnyY, najblizszy.Produkt.PozycjaX, najblizszy.Produkt.PozycjaY);
            wynik.SzacowanyDystansMetry += (dystans * 2.5);

            wynik.Trasa.Add(najblizszy);
            doPrzetworzenia.Remove(najblizszy);
            
            aktualnyX = najblizszy.Produkt.PozycjaX;
            aktualnyY = najblizszy.Produkt.PozycjaY;
        }

        wynik.SzacowanyDystansMetry += ObliczDystansWiezowy(aktualnyX, aktualnyY, 5, 0) * 2.5;

        foreach (var poz in wynik.Trasa)
        {
            var porada = new PoradaBHP { PozycjaId = poz.Id };
            
            if (poz.Produkt.CzyDelikatne || poz.Produkt.WagaKg < 5) 
            {
                porada.InstrukcjaUlozenia = "Delikatne — ułóż z górą";
            } 
            else if (poz.Produkt.WagaKg >= 15) 
            {
                porada.InstrukcjaUlozenia = "Ciężkie — dół palety";
            } 
            else 
            {
                porada.InstrukcjaUlozenia = "Standard — środek palety";
            }

            porada.SprzetDedykowany = poz.Produkt.Poziom >= 3 ? "Podest/Drabina robocza" : "Z poziomu 0";
            porada.AlertWysokosci = poz.Produkt.Poziom >= 3;
            wynik.InstrukcjeDlaTowarow[poz.Id] = porada;
        }

        return wynik;
    }

    private double ObliczDystansWiezowy(int x1, int y1, int x2, int y2)
    {
        if (y1 == y2) 
        {
            return Math.Abs(x1 - x2);
        }
        return Math.Abs(x1 - 5) + Math.Abs(y1 - y2) + Math.Abs(x2 - 5);
    }
}