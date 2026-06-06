using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Konto zostało zablokowane.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Haslo, false, false);

            if (result.Succeeded)
            {
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                }
                
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Kierownik"))
                    {
                        return RedirectToAction("Index", "Klient");
                    }
                    
                    if (roles.Contains("Pracownik"))
                    {
                        return RedirectToAction("ListaZlecen", "Pracownik");
                    }

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }
            
                return RedirectToAction("Index", "Home");
            }
            
            ModelState.AddModelError(string.Empty, "Złe dane logowania. Spróbuj ponownie.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email, string noweHaslo)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(noweHaslo))
        {
            TempData["Error"] = "Wypełnij wszystkie pola.";
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        
        if (user != null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, noweHaslo);

            if (result.Succeeded)
            {
                TempData["ResetMessage"] = "Hasło zostało pomyślnie zmienione! Możesz się teraz zalogować.";
                return RedirectToAction(nameof(Login));
            }
            else
            {
                TempData["Error"] = "Nie udało się zmienić hasła.";
            }
        }
        else
        {
            TempData["Error"] = "Konto z takim adresem e-mail nie istnieje w systemie.";
        }
        
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}