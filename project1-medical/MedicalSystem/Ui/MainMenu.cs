using MedicalSystem.Data;

namespace MedicalSystem.Ui;

public static class MainMenu
{
    public static void Run(MedicalContext db)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== MEDICINSKI SUSTAV ===");
            Console.WriteLine("1) Pacijenti");
            Console.WriteLine("2) Povijest bolesti");
            Console.WriteLine("3) Lijekovi (recepti)");
            Console.WriteLine("4) Specijalistički pregledi");
            Console.WriteLine("5) Popis liječnika");
            Console.WriteLine("0) Izlaz");

            switch (ConsoleInput.ReadRequired("Odabir"))
            {
                case "1": PatientMenu.Run(db); break;
                case "2": DiseaseMenu.Run(db); break;
                case "3": PrescriptionMenu.Run(db); break;
                case "4": ExamMenu.Run(db); break;
                case "5": DoctorList.Show(db); break;
                case "0": return;
                default: Console.WriteLine("Nepoznat odabir."); break;
            }
        }
    }
}
