using MedicalSystem.Data;
using MedicalSystem.Ui;
using Microsoft.EntityFrameworkCore;

try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { /* preusmjeren izlaz */ }

using var db = new MedicalContext();

Console.WriteLine("Provjera baze i primjena migracija...");
db.Database.Migrate();   // izvrši migracije koje još nisu primijenjene (prvi put: kreira shemu)

FirstRunSetup.EnsureDoctors(db);
MainMenu.Run(db);

Console.WriteLine("Doviđenja!");
