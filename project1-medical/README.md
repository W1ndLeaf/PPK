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
