using MedicalSystem.Data;

namespace MedicalSystem.Ui;

public static class DoctorList
{
    public static void Show(MedicalContext db)
    {
        Console.WriteLine();
        Console.WriteLine("--- LIJEČNICI (definirani pri prvom pokretanju, bez CRUD-a) ---");
        foreach (var d in db.Doctors.OrderBy(d => d.LastName).ThenBy(d => d.FirstName))
            Console.WriteLine($"  [{d.Id}] {d}");
    }
}
