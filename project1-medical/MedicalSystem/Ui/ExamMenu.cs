using MedicalSystem.Data;
using MedicalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Ui;

public static class ExamMenu
{
    public static void Run(MedicalContext db)
    {
        var patient = Pickers.PickPatient(db);
        if (patient is null) return;

        while (true)
        {
            var items = db.SpecialistExams
                .Include(e => e.Doctor)   // eager: podaci o specijalistu uz pregled
                .Where(e => e.PatientId == patient.Id)
                .OrderBy(e => e.ScheduledAt)
                .ToList();

            Console.WriteLine();
            Console.WriteLine($"--- SPECIJALISTIČKI PREGLEDI: {patient.FirstName} {patient.LastName} ---");
            if (items.Count == 0) Console.WriteLine("(prazno)");
            foreach (var e in items)
                Console.WriteLine($"  [{e.Id}] {e.Type,-6} {e.ScheduledAt:dd.MM.yyyy. HH:mm} — {e.Doctor}");
            Console.WriteLine("1) Zakaži  2) Promijeni termin  3) Otkaži (obriši)  0) Natrag");

            switch (ConsoleInput.ReadRequired("Odabir"))
            {
                case "1":
                {
                    Console.WriteLine("Vrsta pregleda:");
                    var type = ConsoleInput.ReadEnum<ExamType>("Vrsta");
                    var doctor = Pickers.PickDoctor(db);
                    if (doctor is null) break;

                    db.SpecialistExams.Add(new SpecialistExam
                    {
                        PatientId = patient.Id,
                        DoctorId = doctor.Id,
                        Type = type,
                        ScheduledAt = ConsoleInput.ReadDateTime("Termin"),
                    });
                    db.SaveChanges();
                    Console.WriteLine("Zakazano.");
                    break;
                }

                case "2":
                {
                    var id = ConsoleInput.ReadInt("ID pregleda");
                    var e = items.FirstOrDefault(x => x.Id == id);
                    if (e is null) { Console.WriteLine("Ne postoji."); break; }
                    e.ScheduledAt = ConsoleInput.ReadDateTime($"Novi termin [{e.ScheduledAt:dd.MM.yyyy. HH:mm}]");
                    db.SaveChanges();
                    Console.WriteLine("Spremljeno.");
                    break;
                }

                case "3":
                {
                    var id = ConsoleInput.ReadInt("ID pregleda");
                    var e = items.FirstOrDefault(x => x.Id == id);
                    if (e is null) { Console.WriteLine("Ne postoji."); break; }
                    if (ConsoleInput.Confirm($"Otkazati {e.Type} {e.ScheduledAt:dd.MM.yyyy. HH:mm}?"))
                    {
                        db.SpecialistExams.Remove(e);
                        db.SaveChanges();
                        Console.WriteLine("Otkazano.");
                    }
                    break;
                }

                case "0": return;
                default: Console.WriteLine("Nepoznat odabir."); break;
            }
        }
    }
}
