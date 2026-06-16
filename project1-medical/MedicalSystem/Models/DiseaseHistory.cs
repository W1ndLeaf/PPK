namespace MedicalSystem.Models;

public class DiseaseHistory
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public required string DiseaseName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }   // null = bolest još traje

    public List<Prescription> Prescriptions { get; set; } = [];
}
