using MedicalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Data;

public class MedicalContext : DbContext
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DiseaseHistory> DiseaseHistories => Set<DiseaseHistory>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<SpecialistExam> SpecialistExams => Set<SpecialistExam>();

    public const string DefaultConnection =
        "Host=localhost;Port=5432;Database=medical;Username=medapp;Password=medapp_dev_pw";

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseNpgsql(ResolveConnectionString());
    }

    // Connection string priority: MEDICAL_DB env var -> connection.txt file (e.g. Supabase) -> local Docker.
    private static string ResolveConnectionString()
    {
        var env = Environment.GetEnvironmentVariable("MEDICAL_DB");
        if (!string.IsNullOrWhiteSpace(env)) return env;
        if (File.Exists("connection.txt"))
        {
            var fromFile = File.ReadAllText("connection.txt").Trim();
            if (fromFile.Length > 0) return fromFile;
        }
        return DefaultConnection;
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Doctor>(e =>
        {
            e.Property(d => d.FirstName).HasMaxLength(100);
            e.Property(d => d.LastName).HasMaxLength(100);
            e.Property(d => d.Specialization).HasMaxLength(100);
        });

        b.Entity<Patient>(e =>
        {
            e.Property(p => p.FirstName).HasMaxLength(100);
            e.Property(p => p.LastName).HasMaxLength(100);
            e.Property(p => p.Oib).HasMaxLength(11).IsFixedLength();  // CHAR(11)
            e.HasIndex(p => p.Oib).IsUnique();                        // UNIQUE indeks na OIB
            e.Property(p => p.Sex).HasConversion<string>().HasMaxLength(10);
            e.Property(p => p.ResidenceAddress).HasMaxLength(200);
            e.Property(p => p.DomicileAddress).HasMaxLength(200);
        });

        b.Entity<DiseaseHistory>(e =>
        {
            e.Property(d => d.DiseaseName).HasMaxLength(200);
            e.HasOne(d => d.Patient)
             .WithMany(p => p.Diseases)
             .HasForeignKey(d => d.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Prescription>(e =>
        {
            e.Property(r => r.MedicationName).HasMaxLength(200);
            e.Property(r => r.Dose).HasMaxLength(100);
            e.Property(r => r.Frequency).HasMaxLength(100);
            e.HasOne(r => r.Patient)
             .WithMany(p => p.Prescriptions)
             .HasForeignKey(r => r.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.DiseaseHistory)
             .WithMany(d => d.Prescriptions)
             .HasForeignKey(r => r.DiseaseHistoryId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<SpecialistExam>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(10);
            // bez vremenske zone: termin se upisuje i čita kao lokalno vrijeme
            e.Property(x => x.ScheduledAt).HasColumnType("timestamp without time zone");
            e.HasOne(x => x.Patient)
             .WithMany(p => p.Exams)
             .HasForeignKey(x => x.PatientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Doctor)
             .WithMany(d => d.Exams)
             .HasForeignKey(x => x.DoctorId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
