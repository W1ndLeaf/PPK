using MedicalSystem.Data;
using MedicalSystem.Models;

namespace MedicalSystem.Ui;

// Zajednički odabir entiteta po ID-u (ispiše popis pa traži ID).
public static class Pickers
{
    public static Patient? PickPatient(MedicalContext db)
    {
        var patients = db.Patients.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
        if (patients.Count == 0)
        {
            Console.WriteLine("Nema pacijenata u sustavu.");
            return null;
        }
        Console.WriteLine("-- Pacijenti --");
        foreach (var p in patients)
            Console.WriteLine($"  [{p.Id}] {p.LastName}, {p.FirstName} (OIB {p.Oib})");

        var id = ConsoleInput.ReadInt("ID pacijenta (0 = odustani)");
        if (id == 0) return null;
        var chosen = patients.FirstOrDefault(p => p.Id == id);
        if (chosen is null) Console.WriteLine("Ne postoji pacijent s tim ID-om.");
        return chosen;
    }

    public static Doctor? PickDoctor(MedicalContext db)
    {
        var doctors = db.Doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName).ToList();
        Console.WriteLine("-- Liječnici --");
        foreach (var d in doctors)
            Console.WriteLine($"  [{d.Id}] {d}");

        var id = ConsoleInput.ReadInt("ID liječnika (0 = odustani)");
        if (id == 0) return null;
        var chosen = doctors.FirstOrDefault(d => d.Id == id);
        if (chosen is null) Console.WriteLine("Ne postoji liječnik s tim ID-om.");
        return chosen;
    }
}
