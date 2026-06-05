using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Data;


public class MagazynDbContext : IdentityDbContext<ApplicationUser> 
{
    public MagazynDbContext(DbContextOptions<MagazynDbContext> options) : base(options)
    {
    }

    public DbSet<Produkt> Produkty { get; set; }
    public DbSet<Zamowienie> Zamowienia { get; set; }
    public DbSet<PozycjaZamowienia> PozycjeZamowien { get; set; }
}