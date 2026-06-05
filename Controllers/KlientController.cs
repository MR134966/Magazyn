using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proejkt_magazyn.Data;
using Proejkt_magazyn.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proejkt_magazyn.Controllers;

[Authorize(Roles = "Kierownik")]
public class KlientController : Controller
{
    private readonly MagazynDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public KlientController(MagazynDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var mojeZamowienia = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .Where(z => z.NazwaKlienta == user.Sklep)
            .ToListAsync();
        return View(mojeZamowienia);
    }

    public async Task<IActionResult> SzczegolyZamowienia(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var zamowienie = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .FirstOrDefaultAsync(z => z.Id == id && z.NazwaKlienta == user.Sklep);

        if (zamowienie == null) return NotFound();

        return View(zamowienie);
    }

    public async Task<IActionResult> Gazetka()
    {
        var wszystkieProdukty = await _context.Produkty.ToListAsync();
        return View(wszystkieProdukty);
    }

    [HttpPost]
    public async Task<IActionResult> ZlozZamowienie(int[] produktId, int[] ilosc)
    {
        if (produktId == null || ilosc == null || produktId.Length == 0)
        {
            return RedirectToAction(nameof(Gazetka));
        }

        var user = await _userManager.GetUserAsync(User);
        var noweZamowienie = new Zamowienie { NazwaKlienta = user.Sklep };

        var produktyWBazie = await _context.Produkty
            .Where(p => produktId.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        for (int i = 0; i < produktId.Length; i++)
        {
            if (ilosc[i] <= 0) continue;

            if (!produktyWBazie.TryGetValue(produktId[i], out var produkt))
            {
                TempData["Error"] = "Jeden z wybranych produktów nie istnieje.";
                return RedirectToAction(nameof(Gazetka));
            }

            if (produkt.StanMagazynowy < ilosc[i])
            {
                TempData["Error"] = $"Brak wystarczającej ilości produktu '{produkt.Nazwa}'. Maksymalnie możesz zamówić {produkt.StanMagazynowy} szt.";
                return RedirectToAction(nameof(Gazetka));
            }
        }

        for (int i = 0; i < produktId.Length; i++)
        {
            if (ilosc[i] > 0 && produktyWBazie.TryGetValue(produktId[i], out var produkt))
            {
                produkt.StanMagazynowy -= ilosc[i];
                
                noweZamowienie.Pozycje.Add(new PozycjaZamowienia 
                { 
                    ProduktId = produkt.Id, 
                    IloscSztuk = ilosc[i] 
                });
            }
        }

        if (noweZamowienie.Pozycje.Any())
        {
            _context.Zamowienia.Add(noweZamowienie);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Zamówienie zostało złożone pomyślnie!";
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> PobierzDokumentWz(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        
        var zamowienie = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .FirstOrDefaultAsync(z => z.Id == id && z.NazwaKlienta == user.Sklep);

        if (zamowienie == null) return NotFound();

        var dokument = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Dokument Wydania Zewnętrznego (WZ)").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Numer systemowy: #{zamowienie.Id:00000}").FontSize(14).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(5).Text($"Data wystawienia: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    });
                    
                   
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Text("Odbiorca:").SemiBold();
                    col.Item().Text(zamowienie.NazwaKlienta).FontSize(14);
                    col.Item().PaddingBottom(1, Unit.Centimetre);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn();
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("#").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Nazwa produktu").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Ilość").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Cena jedn.").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).AlignRight().Text("Wartość").SemiBold();
                        });

                        int i = 1;
                        decimal sumaCalkowita = 0;
                        foreach (var poz in zamowienie.Pozycje)
                        {
                            decimal wartoscPozycji = poz.IloscSztuk * poz.Produkt.CenaBrutto;
                            sumaCalkowita += wartoscPozycji;

                            table.Cell().PaddingVertical(5).Text(i.ToString());
                            table.Cell().PaddingVertical(5).Text(poz.Produkt.Nazwa);
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{poz.IloscSztuk} szt.");
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{poz.Produkt.CenaBrutto:0.00} zł");
                            table.Cell().PaddingVertical(5).AlignRight().Text($"{wartoscPozycji:0.00} zł").SemiBold();
                            i++;
                        }

                        table.Cell().ColumnSpan(4).AlignRight().PaddingTop(10).Text("Razem do zapłaty (brutto):").SemiBold().FontSize(14);
                        table.Cell().AlignRight().PaddingTop(10).Text($"{sumaCalkowita:0.00} zł").SemiBold().FontSize(14).FontColor(Colors.Green.Darken2);
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("SmartWMS | Strona ");
                    x.CurrentPageNumber();
                    x.Span(" z ");
                    x.TotalPages();
                });
            });
        });

        var pdfBytes = dokument.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"WZ_{zamowienie.Id:00000}_{DateTime.Now:yyyyMMdd}.pdf");
    }
}