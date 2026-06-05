using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Proejkt_magazyn.Data;
using Proejkt_magazyn.Services;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Controllers;

[Authorize(Roles = "Pracownik")]
public class PracownikController : Controller
{
    private readonly MagazynDbContext _context;
    private readonly SmartAllocationService _aiService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PracownikController(MagazynDbContext context, SmartAllocationService aiService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _aiService = aiService;
        _userManager = userManager;
    }

    public async Task<IActionResult> ListaZlecen()
    {
        var zlecenia = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .Where(z => z.Pozycje.Any(p => !p.CzyZebrane))
            .ToListAsync();
        
        if (!zlecenia.Any())
        {
            return View("BrakZadan");
        }

        return View(zlecenia);
    }

    public async Task<IActionResult> Index(int id, bool kontynuacja = false)
    {
        var zamowienie = await _context.Zamowienia
            .Include(z => z.Pozycje)
            .ThenInclude(p => p.Produkt)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zamowienie == null) return RedirectToAction(nameof(ListaZlecen));

        var plan = _aiService.AnalizujZamowienie(zamowienie);
        ViewBag.ZamowienieId = id;
        ViewBag.Kontynuacja = kontynuacja;
        return View(plan);
    }

    [HttpPost]
    public async Task<IActionResult> ZeskanujTowar(int pozycjaId, int zamowienieId, double dystansDoDodania)
    {
        var pozycja = await _context.PozycjeZamowien.FindAsync(pozycjaId);
        if (pozycja != null)
        {
            pozycja.CzyZebrane = true;

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.DystansDzisiaj += dystansDoDodania;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { id = zamowienieId, kontynuacja = true });
    }
}