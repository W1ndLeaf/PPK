using MedicalSystem.Data;
using MedicalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Ui;

public static class PatientMenu
{
    public static void Run(MedicalContext db)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("--- PACIJENTI ---");
            Console.WriteLine("1) Ispis svih");
            Console.WriteLine("2) Detalji (s povezanim podacima — eager loading)");
            Console.WriteLine("3) Dodaj");
            Console.WriteLine("4) Uredi");
            Console.WriteLine("5) Obriši");
            Console.WriteLine("0) Natrag");

            switch (ConsoleInput.ReadRequired("Odabir"))
            {
                case "1": ListAll(db); break;
                case "2": Details(db); break;
                case "3": Add(db); break;
                case "4": Edit(db); break;
                case "5": Delete(db); break;
                case "0": return;
                default: Console.WriteLine("Nepoznat odabir."); break;
            }
        }
    }

    private static string SexLabel(Sex s) => s == Sex.Male ? "M" : "Ž";

    private static void ListAll(MedicalContext db)
    {
        var patients = db.Patients.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
        if (patients.Count == 0) Console.WriteLine("(nema pacijenata)");
        foreach (var p in patients)
            Console.WriteLine($"[{p.Id}] {p.LastName}, {p.FirstName} | OIB {p.Oib} | rođen {p.DateOfBirth:dd.MM.yyyy.} | {SexLabel(p.Sex)}");
    }

    private static void Details(MedicalContext db)
    {
        var picked = Pickers.PickPatient(db);
        if (picked is null) return;

        // EAGER LOADING: Include/ThenInclude dohvaća povezane podatke odmah,
        // u istom upitu (JOIN), umjesto naknadno po potrebi (lazy).
        var p = db.Patients
            .Include(x => x.Diseases).ThenInclude(d => d.Prescriptions)
            .Include(x => x.Prescriptions)
            .Include(x => x.Exams).ThenInclude(e => e.Doctor)
            .First(x => x.Id == picked.Id);

        Console.WriteLine();
        Console.WriteLine($"== {p.FirstName} {p.LastName} ==");
        Console.WriteLine($"OIB: {p.Oib} | rođen: {p.DateOfBirth:dd.MM.yyyy.} | spol: {SexLabel(p.Sex)}");
        Console.WriteLine($"Adresa boravišta:   {p.ResidenceAddress}");
        Console.WriteLine($"Adresa prebivališta: {p.DomicileAddress}");

        Console.WriteLine("Povijest bolesti:");
        if (p.Diseases.Count == 0) Console.WriteLine("  (prazno)");
        foreach (var d in p.Diseases.OrderByDescending(d => d.StartDate))
        {
            var until = d.EndDate is null ? "još traje" : $"do {d.EndDate:dd.MM.yyyy.}";
            Console.WriteLine($"  [{d.Id}] {d.DiseaseName}: od {d.StartDate:dd.MM.yyyy.}, {until}");
        }

        Console.WriteLine("Lijekovi:");
        if (p.Prescriptions.Count == 0) Console.WriteLine("  (prazno)");
        foreach (var r in p.Prescriptions)
        {
            var forDisease = r.DiseaseHistory is null ? "" : $" (za: {r.DiseaseHistory.DiseaseName})";
            Console.WriteLine($"  [{r.Id}] {r.MedicationName}, {r.Dose}, {r.Frequency}{forDisease}");
        }

        Console.WriteLine("Pregledi:");
        if (p.Exams.Count == 0) Console.WriteLine("  (prazno)");
        foreach (var e in p.Exams.OrderBy(e => e.ScheduledAt))
            Console.WriteLine($"  [{e.Id}] {e.Type} — {e.ScheduledAt:dd.MM.yyyy. HH:mm} kod {e.Doctor}");

        ConsoleInput.Pause();
    }

    private static void Add(MedicalContext db)
    {
        var oib = ConsoleInput.ReadOib("OIB (11 znamenki)");
        if (db.Patients.Any(p => p.Oib == oib))   // UNIQUE provjera i na razini baze (indeks)
        {
            Console.WriteLine("! Pacijent s tim OIB-om već postoji.");
            return;
        }

        var p = new Patient
        {
            Oib = oib,
            FirstName = ConsoleInput.ReadRequired("Ime"),
            LastName = ConsoleInput.ReadRequired("Prezime"),
            DateOfBirth = ConsoleInput.ReadDate("Datum rođenja"),
            Sex = ReadSex(),
            ResidenceAddress = ConsoleInput.ReadRequired("Adresa boravišta"),
            DomicileAddress = ConsoleInput.ReadRequired("Adresa prebivališta"),
        };
        db.Patients.Add(p);
        db.SaveChanges();   // INSERT
        Console.WriteLine($"Spremljeno (ID {p.Id}).");
    }

    private static Sex ReadSex()
    {
        while (true)
        {
            var s = ConsoleInput.ReadRequired("Spol (m/ž)").ToLowerInvariant();
            if (s == "m") return Sex.Male;
            if (s is "ž" or "z") return Sex.Female;
            Console.WriteLine("  ! Upiši m ili ž.");
        }
    }

    private static void Edit(MedicalContext db)
    {
        var p = Pickers.PickPatient(db);
        if (p is null) return;

        Console.WriteLine("Enter = zadrži postojeću vrijednost.");
        p.FirstName = ConsoleInput.ReadOptional($"Ime [{p.FirstName}]") ?? p.FirstName;
        p.LastName = ConsoleInput.ReadOptional($"Prezime [{p.LastName}]") ?? p.LastName;
        p.ResidenceAddress = ConsoleInput.ReadOptional($"Adresa boravišta [{p.ResidenceAddress}]") ?? p.ResidenceAddress;
        p.DomicileAddress = ConsoleInput.ReadOptional($"Adresa prebivališta [{p.DomicileAddress}]") ?? p.DomicileAddress;

        // CHANGE TRACKING: context uspoređuje trenutno stanje sa snapshotom
        // pa UPDATE sadrži samo stvarno promijenjene stupce.
        db.SaveChanges();
        Console.WriteLine("Spremljeno.");
    }

    private static void Delete(MedicalContext db)
    {
        var p = Pickers.PickPatient(db);
        if (p is null) return;
        if (!ConsoleInput.Confirm($"Obrisati pacijenta {p.FirstName} {p.LastName} i SVE povezane zapise (kaskadno)?"))
            return;

        db.Patients.Remove(p);
        db.SaveChanges();   // DELETE — povezani zapisi padaju kaskadno (ON DELETE CASCADE)
        Console.WriteLine("Obrisano.");
    }
}
