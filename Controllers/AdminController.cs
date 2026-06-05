using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proejkt_magazyn.Data;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly MagazynDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(MagazynDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.SumaZamowien = await _context.Zamowienia.CountAsync();
        ViewBag.NiskieStany = await _context.Produkty.CountAsync(p => p.StanMagazynowy < 10);
        ViewBag.LiczbaPracownikow = (await _userManager.GetUsersInRoleAsync("Pracownik")).Count;
        ViewBag.LiczbaKierownikow = (await _userManager.GetUsersInRoleAsync("Kierownik")).Count;

        return View();
    }

    public async Task<IActionResult> Zamowienia()
    {
        var zamowienia = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .OrderByDescending(z => z.Id)
            .ToListAsync();

        return View(zamowienia);
    }

    public async Task<IActionResult> SzczegolyZamowienia(int id)
    {
        var zamowienie = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zamowienie == null) return NotFound();

        return View(zamowienie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZapiszPoprawkiZamowienia(int zamowienieId, IFormCollection form)
    {
        var zamowienie = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .FirstOrDefaultAsync(z => z.Id == zamowienieId);

        if (zamowienie == null) return NotFound();

        int zaktualizowano = 0;

        foreach (var pozycja in zamowienie.Pozycje)
        {
            if (int.TryParse(form[$"ilosc_{pozycja.Id}"], out int nowaIlosc) && nowaIlosc > 0)
            {
                if (pozycja.IloscSztuk != nowaIlosc)
                {
                    int roznica = nowaIlosc - pozycja.IloscSztuk;

                    
                    if (roznica > 0 && pozycja.Produkt.StanMagazynowy < roznica)
                    {
                        TempData["Error"] = $"Nie można zwiększyć pozycji '{pozycja.Produkt.Nazwa}' o {roznica} szt. Brak wystarczającej ilości w magazynie (dostępne: {pozycja.Produkt.StanMagazynowy} szt.).";
                        return RedirectToAction(nameof(SzczegolyZamowienia), new { id = zamowienieId });
                    }

                    
                    pozycja.Produkt.StanMagazynowy -= roznica;

                    pozycja.IloscSztuk = nowaIlosc;
                    pozycja.CzyZebrane = false;
                    zaktualizowano++;
                }
            }
        }

        if (zaktualizowano > 0)
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Pomyślnie zaktualizowano i cofnięto na skaner {zaktualizowano} pozycji.";
        }

        return RedirectToAction(nameof(SzczegolyZamowienia), new { id = zamowienieId });
    }

    public async Task<IActionResult> Pracownicy()
    {
        var uzytkownicy = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var modeleUzytkownikow = new List<UserWithRolesViewModel>();

        foreach (var user in uzytkownicy)
        {
            var role = await _userManager.GetRolesAsync(user);
            modeleUzytkownikow.Add(new UserWithRolesViewModel
            {
                User = user,
                Roles = role,
                CzyZablokowany = await CzyKontoZablokowane(user)
            });
        }

        ViewBag.NoweKonto = new DodajKontoViewModel();
        return View(modeleUzytkownikow);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DodajKonto(DodajKontoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nie udało się utworzyć konta. Sprawdź wprowadzone dane.";
            return RedirectToAction(nameof(Pracownicy));
        }

        if (model.TypKonta != "Pracownik" && model.TypKonta != "Kierownik")
        {
            TempData["Error"] = "Dozwolone typy konta: Pracownik lub Kierownik sklepu.";
            return RedirectToAction(nameof(Pracownicy));
        }

        if (model.TypKonta == "Kierownik" && string.IsNullOrWhiteSpace(model.Sklep))
        {
            TempData["Error"] = "Dla kierownika sklepu podaj nazwę sklepu.";
            return RedirectToAction(nameof(Pracownicy));
        }

        if (model.TypKonta == "Pracownik" && string.IsNullOrWhiteSpace(model.NumerSkanera))
        {
            TempData["Error"] = "Dla pracownika magazynu podaj numer skanera.";
            return RedirectToAction(nameof(Pracownicy));
        }

        var istniejacy = await _userManager.FindByEmailAsync(model.Email);
        if (istniejacy != null)
        {
            TempData["Error"] = "Konto z tym adresem e-mail już istnieje.";
            return RedirectToAction(nameof(Pracownicy));
        }

        var rola = model.TypKonta == "Kierownik" ? "Kierownik" : "Pracownik";
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true,
            Imie = model.Imie,
            Nazwisko = model.Nazwisko,
            Stanowisko = model.TypKonta == "Kierownik" ? "Kierownik sklepu" : "Pracownik magazynu",
            Sklep = model.Sklep ?? string.Empty,
            NumerSkanera = model.NumerSkanera ?? string.Empty
        };

        var wynik = await _userManager.CreateAsync(user, model.Haslo);
        if (!wynik.Succeeded)
        {
            TempData["Error"] = string.Join(" ", wynik.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Pracownicy));
        }

        await _userManager.AddToRoleAsync(user, rola);
        TempData["Success"] = $"Utworzono konto {user.Email} ({model.TypKonta}).";
        return RedirectToAction(nameof(Pracownicy));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZablokujKonto(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Pracownicy));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Nie można zablokować konta administratora.";
            return RedirectToAction(nameof(Pracownicy));
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        TempData["Success"] = $"Konto {user.Email} zostało zablokowane.";
        return RedirectToAction(nameof(Pracownicy));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OdblokujKonto(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Pracownicy));
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        TempData["Success"] = $"Konto {user.Email} zostało odblokowane.";
        return RedirectToAction(nameof(Pracownicy));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UsunKonto(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["Error"] = "Nie znaleziono użytkownika.";
            return RedirectToAction(nameof(Pracownicy));
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            TempData["Error"] = "Nie można usunąć konta administratora systemu.";
            return RedirectToAction(nameof(Pracownicy));
        }

        var result = await _userManager.DeleteAsync(user);
        
        if (result.Succeeded)
        {
            TempData["Success"] = $"Konto {user.Email} zostało trwale usunięte.";
        }
        else
        {
            TempData["Error"] = "Wystąpił błąd podczas usuwania konta.";
        }

        return RedirectToAction(nameof(Pracownicy));
    }

    public async Task<IActionResult> Dostawy()
    {
        var produkty = await _context.Produkty.OrderBy(p => p.StanMagazynowy).ToListAsync();
        return View(produkty);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZapiszDostawe(int produktId, int ilosc)
    {
        if (ilosc <= 0)
        {
            TempData["Error"] = "Ilość dostawy musi być większa od zera.";
            return RedirectToAction(nameof(Dostawy));
        }

        var produkt = await _context.Produkty.FirstOrDefaultAsync(p => p.Id == produktId);
        if (produkt == null)
        {
            TempData["Error"] = "Nie znaleziono produktu.";
            return RedirectToAction(nameof(Dostawy));
        }

        produkt.StanMagazynowy += ilosc;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Przyjęto dostawę: {produkt.Nazwa} (+{ilosc} szt.). Aktualny stan: {produkt.StanMagazynowy} szt.";
        return RedirectToAction(nameof(Dostawy));
    }

    private async Task<bool> CzyKontoZablokowane(ApplicationUser user)
    {
        return await _userManager.IsLockedOutAsync(user);
    }
}