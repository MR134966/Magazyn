using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proejkt_magazyn.Data;
using System.Globalization;

namespace Proejkt_magazyn.Controllers;

[Authorize(Roles = "Admin")]
public class StatystykiController : Controller
{
    private readonly MagazynDbContext _context;

    public StatystykiController(MagazynDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var produkty = await _context.Produkty.AsNoTracking().ToListAsync();

        var sumaSztukNaStanie = produkty.Sum(p => p.StanMagazynowy);
        var sumaWartosci = produkty.Sum(p => p.CenaBrutto * p.StanMagazynowy);
        var iloscProduktow = produkty.Count;
        var iloscPromocji = produkty.Count(p => p.CzyGazetka);
        var braki = produkty.Count(p => p.StanMagazynowy == 0);
        var niskieStany = produkty.Count(p => p.StanMagazynowy > 0 && p.StanMagazynowy < 10);

        var stanyPoAlejkach = produkty
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Aleja) ? "Brak alejki" : p.Aleja)
            .Select(g => new { Aleja = g.Key, Sztuk = g.Sum(p => p.StanMagazynowy) })
            .OrderByDescending(g => g.Sztuk)
            .ToList();

        var kultura = CultureInfo.GetCultureInfo("pl-PL");

        ViewBag.SumaSztukNaStanie = sumaSztukNaStanie;
        ViewBag.SumaWartosci = sumaWartosci.ToString("C", kultura);
        ViewBag.IloscProduktow = iloscProduktow;
        ViewBag.IloscPromocji = iloscPromocji;
        ViewBag.Braki = braki;
        ViewBag.NiskieStany = niskieStany;
        ViewBag.SredniStan = iloscProduktow > 0 ? (sumaSztukNaStanie / (double)iloscProduktow).ToString("0.0", kultura) : "0";

        ViewBag.AlejkiLabels = string.Join(",", stanyPoAlejkach.Select(k => $"'{k.Aleja.Replace("'", "\\'")}'"));
        ViewBag.AlejkiStany = string.Join(",", stanyPoAlejkach.Select(k => k.Sztuk));

        var promocjaNaStanie = produkty.Where(p => p.CzyGazetka).Sum(p => p.StanMagazynowy);
        var pozaPromocja = sumaSztukNaStanie - promocjaNaStanie;
        ViewBag.StanPozaPromocja = pozaPromocja;
        ViewBag.StanPromocja = promocjaNaStanie;

        return View(produkty);
    }
}
