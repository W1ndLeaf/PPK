namespace MedicalSystem.Models;

public class Doctor
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Specialization { get; set; }

    public List<SpecialistExam> Exams { get; set; } = [];

    public override string ToString() => $"dr. {FirstName} {LastName} ({Specialization})";
}
