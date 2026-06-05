using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Controllers;

[Authorize]
public class UstawieniaController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UstawieniaController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new UstawieniaViewModel
        {
            Imie = user.Imie,
            Nazwisko = user.Nazwisko,
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ZapiszProfil(UstawieniaViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Imie = model.Imie;
        user.Nazwisko = model.Nazwisko;
        
        
        if (user.Email != model.Email)
        {
            await _userManager.SetEmailAsync(user, model.Email);
            await _userManager.SetUserNameAsync(user, model.Email); 
        }

        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user); // Odświeża sesję, by u góry zaktualizowało się imię

        TempData["StatusMessage"] = "Profil został pomyślnie zaktualizowany!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ZmienHaslo(UstawieniaViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!string.IsNullOrEmpty(model.AktualneHaslo) && !string.IsNullOrEmpty(model.NoweHaslo))
        {
            var result = await _userManager.ChangePasswordAsync(user, model.AktualneHaslo, model.NoweHaslo);
            
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "Błąd: Aktualne hasło jest niepoprawne lub nowe hasło nie spełnia wymagań.";
                return RedirectToAction(nameof(Index));
            }
            
            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Hasło zostało pomyślnie zmienione!";
        }

        return RedirectToAction(nameof(Index));
    }
}