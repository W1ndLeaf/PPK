using MedicalSystem.Data;
using MedicalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Ui;

public static class PrescriptionMenu
{
    public static void Run(MedicalContext db)
    {
        var patient = Pickers.PickPatient(db);
        if (patient is null) return;

        while (true)
        {
            var items = db.Prescriptions
                .Include(r => r.DiseaseHistory)   // eager: naziv bolesti uz lijek
                .Where(r => r.PatientId == patient.Id)
                .OrderBy(r => r.MedicationName)
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"--- LIJEKOVI: {patient.FirstName} {patient.LastName} ---");
            if (items.Count == 0) Console.WriteLine("(prazno)");
            foreach (var r in items)
            {
                var forDisease = r.DiseaseHistory is null ? "" : $" (za: {r.DiseaseHistory.DiseaseName})";
                Console.WriteLine($"  [{r.Id}] {r.MedicationName}, {r.Dose}, {r.Frequency}{forDisease}");
            }
            Console.WriteLine("1) Dodaj  2) Uredi  3) Obriši  0) Natrag");

            switch (ConsoleInput.ReadRequired("Odabir"))
            {
                case "1":
                {
                    int? diseaseId = null;
                    var diseases = db.DiseaseHistories.Where(d => d.PatientId == patient.Id).ToList();
                    if (diseases.Count > 0 && ConsoleInput.Confirm("Povezati lijek s bolešću iz povijesti?"))
                    {
                        foreach (var d in diseases)
                            Console.WriteLine($"  [{d.Id}] {d.DiseaseName}");
                        var id = ConsoleInput.ReadInt("ID bolesti (0 = bez veze)");
                        if (diseases.Any(x => x.Id == id)) diseaseId = id;
                    }

                    db.Prescriptions.Add(new Prescription
                    {
                        PatientId = patient.Id,
                        DiseaseHistoryId = diseaseId,
                        MedicationName = ConsoleInput.ReadRequired("Naziv lijeka"),
                        Dose = ConsoleInput.ReadRequired("Doza (npr. 500 mg, 2 tablete)"),
                        Frequency = ConsoleInput.ReadRequired("Učestalost (npr. 3x dnevno)"),
                    });
                    db.SaveChanges();
                    Console.WriteLine("Spremljeno.");
                    break;
                }

                case "2":
                {
                    var id = ConsoleInput.ReadInt("ID recepta");
                    var r = items.FirstOrDefault(x => x.Id == id);
                    if (r is null) { Console.WriteLine("Ne postoji."); break; }
                    r.MedicationName = ConsoleInput.ReadOptional($"Lijek [{r.MedicationName}]") ?? r.MedicationName;
                    r.Dose = ConsoleInput.ReadOptional($"Doza [{r.Dose}]") ?? r.Dose;
                    r.Frequency = ConsoleInput.ReadOptional($"Učestalost [{r.Frequency}]") ?? r.Frequency;
                    db.SaveChanges();
                    Console.WriteLine("Spremljeno.");
                    break;
                }

                case "3":
                {
                    var id = ConsoleInput.ReadInt("ID recepta");
                    var r = items.FirstOrDefault(x => x.Id == id);
                    if (r is null) { Console.WriteLine("Ne postoji."); break; }
                    if (ConsoleInput.Confirm($"Obrisati {r.MedicationName}?"))
                    {
                        db.Prescriptions.Remove(r);
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
}
