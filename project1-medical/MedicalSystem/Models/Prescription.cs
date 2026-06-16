namespace MedicalSystem.Models;

public class Prescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    // Lijek se može (ne mora) vezati uz konkretno stanje iz povijesti bolesti
    public int? DiseaseHistoryId { get; set; }
    public DiseaseHistory? DiseaseHistory { get; set; }

    public required string MedicationName { get; set; }
    public required string Dose { get; set; }        // npr. "500 mg", "2 tablete", "10 jedinica"
    public required string Frequency { get; set; }   // npr. "3x dnevno", "svaki drugi dan"
}
