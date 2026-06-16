using MedicalSystem.Models;
using MedicalSystem.Ui;

namespace MedicalSystem.Data;

// Zadatak: liječnici se definiraju SAMO pri prvom pokretanju aplikacije (nema CRUD-a).
public static class FirstRunSetup
{
    private static readonly Doctor[] DemoDoctors =
    [
        new() { FirstName = "Ivana", LastName = "Horvat", Specialization = "radiologija" },
        new() { FirstName = "Marko", LastName = "Kovač", Specialization = "kardiologija" },
        new() { FirstName = "Ana", LastName = "Babić", Specialization = "oftalmologija" },
        new() { FirstName = "Petar", LastName = "Jurić", Specialization = "dermatologija" },
        new() { FirstName = "Maja", LastName = "Novak", Specialization = "neurologija" },
    ];

    public static void EnsureDoctors(MedicalContext db)
    {
        if (db.Doctors.Any()) return;   // nije prvo pokretanje — preskoči

        Console.WriteLine("=== PRVO POKRETANJE — UNOS LIJEČNIKA ===");
        Console.WriteLine("Liječnici se mogu definirati samo sada, pri prvom pokretanju.");
        Console.WriteLine();

        var added = 0;
        while (true)
        {
            var input = ConsoleInput.ReadOptional(added == 0
                ? "Ime liječnika ('d' = učitaj demo popis)"
                : "Ime liječnika (Enter = završi unos)");

            if (input is null)
            {
                if (added > 0) break;
                Console.WriteLine("  ! Potreban je barem jedan liječnik.");
                continue;
            }

            if (added == 0 && input.Equals("d", StringComparison.OrdinalIgnoreCase))
            {
                db.Doctors.AddRange(DemoDoctors);
                added = DemoDoctors.Length;
                break;
            }

            db.Doctors.Add(new Doctor
            {
                FirstName = input,
                LastName = ConsoleInput.ReadRequired("Prezime"),
                Specialization = ConsoleInput.ReadRequired("Specijalizacija"),
            });
            added++;
        }

        db.SaveChanges();
        Console.WriteLine($"Spremljeno liječnika: {added}.");
        Console.WriteLine();
    }
}
