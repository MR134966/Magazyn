using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("RedirectUser");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult RedirectUser()    {
        if (User.IsInRole("Pracownik"))
        {
            return RedirectToAction("ListaZlecen", "Pracownik");
        }
        else if (User.IsInRole("Kierownik"))
        {
            return RedirectToAction("Index", "Klient");
        }
        else if (User.IsInRole("Admin"))
        {
            return RedirectToAction("Index", "Admin");
        }

        return RedirectToAction("Login", "Account");
    }
}