using MedicalSystem.Data;
using MedicalSystem.Models;

namespace MedicalSystem.Ui;

public static class DiseaseMenu
{
    public static void Run(MedicalContext db)
    {
        var patient = Pickers.PickPatient(db);
        if (patient is null) return;

        while (true)
        {
            var items = db.DiseaseHistories
                .Where(d => d.PatientId == patient.Id)
                .OrderByDescending(d => d.StartDate)
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"--- POVIJEST BOLESTI: {patient.FirstName} {patient.LastName} ---");
            if (items.Count == 0) Console.WriteLine("(prazno)");
            foreach (var d in items)
            {
                var until = d.EndDate is null ? "(još traje)" : $"do {d.EndDate:dd.MM.yyyy.}";
                Console.WriteLine($"  [{d.Id}] {d.DiseaseName}: od {d.StartDate:dd.MM.yyyy.} {until}");
            }
            Console.WriteLine("1) Dodaj  2) Uredi  3) Obriši  0) Natrag");

            switch (ConsoleInput.ReadRequired("Odabir"))
            {
                case "1":
                    db.DiseaseHistories.Add(new DiseaseHistory
                    {
                        PatientId = patient.Id,
                        DiseaseName = ConsoleInput.ReadRequired("Naziv bolesti"),
                        StartDate = ConsoleInput.ReadDate("Početak bolesti"),
                        EndDate = ConsoleInput.ReadDateOptional("Kraj bolesti"),
                    });
                    db.SaveChanges();
                    Console.WriteLine("Spremljeno.");
                    break;

                case "2":
                {
                    var d = PickById(items);
                    if (d is null) break;
                    d.DiseaseName = ConsoleInput.ReadOptional($"Naziv [{d.DiseaseName}]") ?? d.DiseaseName;
                    var currentEnd = d.EndDate?.ToString("dd.MM.yyyy.") ?? "još traje";
                    var newEnd = ConsoleInput.ReadDateOptional($"Kraj [{currentEnd}]");
                    if (newEnd is not null) d.EndDate = newEnd;
                    db.SaveChanges();
                    Console.WriteLine("Spremljeno.");
                    break;
                }

                case "3":
                {
                    var d = PickById(items);
                    if (d is null) break;
                    if (ConsoleInput.Confirm($"Obrisati '{d.DiseaseName}'?"))
                    {
                        db.DiseaseHistories.Remove(d);
                        db.SaveChanges();
                        Console.WriteLine("Obrisano.");
                    }
                    break;
                }

                case "0": return;
                default: Console.WriteLine("Nepoznat odabir."); break;
            }
        }
    }

    private static DiseaseHistory? PickById(List<DiseaseHistory> items)
    {
        var id = ConsoleInput.ReadInt("ID zapisa");
        var d = items.FirstOrDefault(x => x.Id == id);
        if (d is null) Console.WriteLine("Ne postoji zapis s tim ID-om.");
        return d;
    }
}
