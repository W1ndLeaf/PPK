# Medicinski sustav

C# (.NET 8) + Entity Framework Core + PostgreSQL.

## Entiteti
- Pacijent (ime, prezime, OIB, datum rođenja, spol, adresa boravišta, adresa prebivališta)
- Liječnik (ime, prezime, specijalizacija) – unose se samo pri prvom pokretanju
- Povijest bolesti
- Lijek (naziv, doza, učestalost)
- Specijalistički pregled (vrsta, termin, liječnik)

## Pokretanje
Baza:

    docker compose up -d

Aplikacija:

    cd MedicalSystem
    dotnet run

PostgreSQL je na localhost:5432 (baza `medical`, korisnik `medapp`), pgAdmin na http://localhost:5050.
EF Core pri pokretanju primijeni migraciju i kreira tablice.

## Cloud baza (Supabase)
Connection string se čita ovim redoslijedom: varijabla okruženja `MEDICAL_DB`, zatim datoteka `connection.txt`, inače lokalni Docker.

Za Supabase: u projektu otvori Project Settings -> Database -> Connection string (URI) i spremi ga u `MedicalSystem/connection.txt`, npr.:

    Host=db.<ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<lozinka>;SSL Mode=Require;Trust Server Certificate=true

Pri pokretanju aplikacije EF Core primijeni migraciju i kreira tablice u Supabase bazi. (`connection.txt` je u .gitignore.)
