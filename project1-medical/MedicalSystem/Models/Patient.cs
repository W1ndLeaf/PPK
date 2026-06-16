namespace MedicalSystem.Models;

public enum Sex { Male, Female }

public class Patient
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Oib { get; set; }              // 11 znamenki, UNIQUE
    public DateOnly DateOfBirth { get; set; }
    public Sex Sex { get; set; }
    public required string ResidenceAddress { get; set; } // adresa boravišta
    public required string DomicileAddress { get; set; }  // adresa prebivališta

    // Navigacijska svojstva (1:N) — EF ih puni kod eager loadinga (Include)
    public List<DiseaseHistory> Diseases { get; set; } = [];
    public List<Prescription> Prescriptions { get; set; } = [];
    public List<SpecialistExam> Exams { get; set; } = [];
}
