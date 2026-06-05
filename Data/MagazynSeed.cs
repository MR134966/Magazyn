using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proejkt_magazyn.Models;

namespace Proejkt_magazyn.Data;

public static class MagazynSeed
{
    public static async Task ZainicjujAsync(MagazynDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();
        await UsunNieuzywaneKolumnyJesliZostalyAsync(context);

        await UpewnijRoleAsync(roleManager);
        await MigrujOperatorowDoPracownikowAsync(userManager, roleManager);

        if (!await context.Produkty.AnyAsync())
        {
            context.Produkty.AddRange(UtworzProduktyStartowe());
            await context.SaveChangesAsync();
        }

        if (!await context.Zamowienia.AnyAsync(z => z.NazwaKlienta == "Sklep #442"))
        {
            var produkty = await context.Produkty.OrderBy(p => p.Id).Take(3).ToListAsync();
            if (produkty.Count >= 2)
            {
                var zamowienie = new Zamowienie { NazwaKlienta = "Sklep #442" };
                zamowienie.Pozycje.Add(new PozycjaZamowienia { ProduktId = produkty[0].Id, IloscSztuk = 5 });
                zamowienie.Pozycje.Add(new PozycjaZamowienia { ProduktId = produkty[1].Id, IloscSztuk = 3 });
                if (produkty.Count > 2)
                    zamowienie.Pozycje.Add(new PozycjaZamowienia { ProduktId = produkty[2].Id, IloscSztuk = 2 });

                context.Zamowienia.Add(zamowienie);
                await context.SaveChangesAsync();
            }
        }

        await UpewnijUzytkownikaAsync(userManager, "michal@magazyn.pl", "Haslo123!", "Michał", "Kowalski",
            "Pracownik magazynu", "Pracownik", numerSkanera: "SKN-007");

        await UpewnijUzytkownikaAsync(userManager, "klient@abc.pl", "Haslo123!", "Jan", "Nowak",
            "Kierownik sklepu", "Kierownik", sklep: "Sklep #442");

        await UpewnijUzytkownikaAsync(userManager, "admin@wms.pl", "Admin123!", "Główny", "Administrator",
            "Admin", "Admin");
    }

    private static List<Produkt> UtworzProduktyStartowe()
    {
        return
        [
            TworzProdukt("5901001001001", "Mleko UHT 1L", 1.0, 4.99m, 6.14m, false, false, "GRO", 1, 1, 1, 50, 1),
            TworzProdukt("5901001001002", "Chleb pszenny 500g", 0.5, 3.49m, 4.29m, false, false, "GRO", 2, 1, 2, 40, 1),
            TworzProdukt("5901001001003", "Kawa ziarnista 1kg", 1.0, 45.99m, 56.57m, true, false, "GRO", 4, 1, 3, 35, 1),

            TworzProdukt("5902002002001", "Szampon 400ml", 0.4, 12.99m, 15.98m, true, false, "BEA", 1, 3, 1, 60, 1),
            TworzProdukt("5902002002002", "Pasta do zębów", 0.15, 7.49m, 9.21m, false, false, "BEA", 3, 3, 2, 55, 1),
            TworzProdukt("5902002002003", "Krem nawilżający", 0.25, 18.99m, 23.36m, true, false, "BEA", 4, 3, 3, 58, 1),

            TworzProdukt("5903003003001", "Koszulka bawełniana M", 0.3, 29.99m, 36.89m, false, false, "APP", 1, 5, 1, 45, 1),
            TworzProdukt("5903003003002", "Bluza z kapturem L", 0.6, 79.99m, 98.39m, false, true, "APP", 3, 5, 2, 50, 1),
            TworzProdukt("5903003003003", "Spodnie jeansowe", 0.8, 119.99m, 147.59m, false, false, "APP", 4, 5, 3, 30, 1),

            TworzProdukt("5904004004001", "Krzesło biurowe", 8.0, 249.99m, 307.49m, false, true, "FUR", 0, 7, 1, 70, 1),
            TworzProdukt("5904004004002", "Biurko gamingowe", 15.0, 499.99m, 614.99m, false, true, "FUR", 2, 7, 2, 20, 1),
            TworzProdukt("5904004004003", "Szafka nocna", 10.0, 149.99m, 184.49m, true, true, "FUR", 4, 7, 3, 25, 1),

            TworzProdukt("5909009009001", "Płyn do mycia podłóg 2L", 2.0, 12.99m, 15.98m, false, false, "MAG", 1, 9, 1, 80, 1),
            TworzProdukt("5909009009002", "Ręczniki papierowe 8 rolek", 0.8, 14.99m, 18.44m, true, false, "MAG", 3, 9, 2, 100, 1),
            TworzProdukt("5909009009003", "Proszek do prania 5kg", 5.0, 39.99m, 49.19m, false, false, "MAG", 4, 9, 3, 60, 1),

            TworzProdukt("5905005005001", "Laptop ASUS 15\"", 2.2, 2999.99m, 3689.99m, false, true, "LAP", 7, 1, 1, 80, 1),
            TworzProdukt("5905005005002", "MacBook Air M2", 1.2, 4999.99m, 6149.99m, false, true, "LAP", 9, 1, 2, 15, 1),
            TworzProdukt("5905005005003", "Myszka bezprzewodowa", 0.2, 99.99m, 122.99m, true, true, "LAP", 11, 1, 3, 120, 1),

            TworzProdukt("5906006006001", "Smartfon Samsung 128GB", 0.2, 1999.99m, 2459.99m, true, true, "SMA", 7, 3, 1, 85, 1),
            TworzProdukt("5906006006002", "iPhone 15 Pro", 0.2, 4500.00m, 5535.00m, false, true, "SMA", 10, 3, 2, 40, 1),
            TworzProdukt("5906006006003", "Ładowarka USB-C 20W", 0.1, 49.99m, 61.49m, false, false, "SMA", 11, 3, 3, 200, 1),

            TworzProdukt("5907007007001", "Piłka do siatkówki Mikasa", 0.4, 149.99m, 184.49m, false, false, "SPO", 8, 5, 1, 65, 1),
            TworzProdukt("5907007007002", "Mata do jogi", 0.8, 59.99m, 73.79m, true, false, "SPO", 9, 5, 2, 90, 1),
            TworzProdukt("5907007007003", "Hantle bitumiczne 2x5kg", 10.0, 89.99m, 110.69m, false, false, "SPO", 11, 5, 3, 40, 1),

            TworzProdukt("5908008008001", "Olej silnikowy 5W30 1L", 1.0, 34.99m, 43.04m, false, false, "VEH", 7, 7, 1, 75, 1),
            TworzProdukt("5908008008002", "Płyn do spryskiwaczy 5L", 5.0, 19.99m, 24.59m, true, false, "VEH", 9, 7, 2, 150, 1),
            TworzProdukt("5908008008003", "Wycieraczki samochodowe", 0.5, 49.99m, 61.49m, false, true, "VEH", 11, 7, 3, 60, 1),

            TworzProdukt("5909009009011", "Ekspres do kawy", 3.5, 399.99m, 491.99m, true, true, "AGD", 8, 9, 1, 90, 1),
            TworzProdukt("5909009009012", "Odkurzacz pionowy", 2.5, 599.99m, 737.99m, false, true, "AGD", 10, 9, 2, 45, 1),
            TworzProdukt("5909009009013", "Czajnik elektryczny", 1.2, 99.99m, 122.99m, false, true, "AGD", 11, 9, 3, 80, 1)
        ];
    }

    private static Produkt TworzProdukt(string ean, string nazwa, double waga, decimal netto, decimal brutto,
        bool gazetka, bool delikatne, string aleja, int x, int y, int regal, int stan, int poziom)
    {
        return new Produkt
        {
            KodKreskowy = ean,
            Nazwa = nazwa,
            WagaKg = waga,
            CenaNetto = netto,
            CenaBrutto = brutto,
            CzyGazetka = gazetka,
            CzyDelikatne = delikatne,
            Aleja = aleja,
            PozycjaX = x,
            PozycjaY = y,
            Regal = regal,
            StanMagazynowy = stan,
            Poziom = poziom
        };
    }

    private static async Task UsunNieuzywaneKolumnyJesliZostalyAsync(MagazynDbContext context)
    {
        if (await KolumnaIstniejeAsync(context, "Produkty", "CzyChemia"))
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Produkty DROP COLUMN CzyChemia");

        if (await KolumnaIstniejeAsync(context, "Zamowienia", "Status"))
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Zamowienia DROP COLUMN Status");
    }

    private static async Task<bool> KolumnaIstniejeAsync(MagazynDbContext context, string tabela, string kolumna)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tabela}')";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(1);
            if (string.Equals(name, kolumna, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task UpewnijRoleAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var rola in new[] { "Pracownik", "Kierownik", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(rola))
                await roleManager.CreateAsync(new IdentityRole(rola));
        }
    }

    private static async Task MigrujOperatorowDoPracownikowAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Operator"))
            return;

        foreach (var user in await userManager.GetUsersInRoleAsync("Operator"))
        {
            await userManager.RemoveFromRoleAsync(user, "Operator");
            if (!await userManager.IsInRoleAsync(user, "Pracownik"))
                await userManager.AddToRoleAsync(user, "Pracownik");
        }
    }

    private static async Task UpewnijUzytkownikaAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string haslo,
        string imie,
        string nazwisko,
        string stanowisko,
        string rola,
        string? sklep = null,
        string? numerSkanera = null)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Imie = imie,
                Nazwisko = nazwisko,
                Stanowisko = stanowisko,
                Sklep = sklep ?? string.Empty,
                NumerSkanera = numerSkanera ?? string.Empty
            };
            await userManager.CreateAsync(user, haslo);
        }

        if (!await userManager.IsInRoleAsync(user, rola))
            await userManager.AddToRoleAsync(user, rola);
    }
}
