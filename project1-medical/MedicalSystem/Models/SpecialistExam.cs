namespace MedicalSystem.Models;

public enum ExamType { CT, MR, ULTRA, EKG, ECHO, OKO, DERM, DENTA, MAMMO, EEG }

public class SpecialistExam
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int DoctorId { get; set; }       // liječnik specijalist koji obavlja pregled
    public Doctor? Doctor { get; set; }

    public ExamType Type { get; set; }
    public DateTime ScheduledAt { get; set; }
}
