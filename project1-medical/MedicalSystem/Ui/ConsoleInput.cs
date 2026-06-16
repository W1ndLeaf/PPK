using System.Globalization;

namespace MedicalSystem.Ui;

// Pomoćne metode za čitanje validiranih vrijednosti s konzole.
// Svaka metoda ponavlja pitanje dok unos nije ispravan.
public static class ConsoleInput
{
    private static readonly string[] DateFormats = ["d.M.yyyy", "d.M.yyyy.", "yyyy-MM-dd"];
    private static readonly string[] DateTimeFormats = ["d.M.yyyy HH:mm", "d.M.yyyy. HH:mm", "yyyy-MM-dd HH:mm"];

    private static string ReadLineOrExit()
    {
        var line = Console.ReadLine();
        if (line is null)   // EOF (npr. preusmjeren ulaz) — uredno izađi
        {
            Console.WriteLine("(kraj ulaza)");
            Environment.Exit(0);
        }
        return line;
    }

    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt}: ");
            var s = ReadLineOrExit().Trim();
            if (s.Length > 0) return s;
            Console.WriteLine("  ! Unos je obavezan.");
        }
    }

    // Vraća null kad korisnik samo pritisne Enter (preskoči / zadrži postojeće).
    public static string? ReadOptional(string prompt)
    {
        Console.Write($"{prompt}: ");
        var s = ReadLineOrExit().Trim();
        return s.Length == 0 ? null : s;
    }

    public static string ReadOib(string prompt)
    {
        while (true)
        {
            var s = ReadRequired(prompt);
            if (s.Length == 11 && s.All(char.IsAsciiDigit)) return s;
            Console.WriteLine("  ! OIB mora imati točno 11 znamenki.");
        }
    }

    public static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            var s = ReadRequired($"{prompt} (npr. 15.3.1990)");
            if (DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            Console.WriteLine("  ! Neispravan datum.");
        }
    }

    public static DateOnly? ReadDateOptional(string prompt)
    {
        while (true)
        {
            var s = ReadOptional($"{prompt} (npr. 15.3.1990, Enter = preskoči)");
            if (s is null) return null;
            if (DateOnly.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            Console.WriteLine("  ! Neispravan datum.");
        }
    }

    public static DateTime ReadDateTime(string prompt)
    {
        while (true)
        {
            var s = ReadRequired($"{prompt} (npr. 15.3.2026 14:30)");
            if (DateTime.TryParseExact(s, DateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d;
            Console.WriteLine("  ! Neispravno vrijeme.");
        }
    }

    public static int ReadInt(string prompt)
    {
        while (true)
        {
            var s = ReadRequired(prompt);
            if (int.TryParse(s, out var i)) return i;
            Console.WriteLine("  ! Očekujem broj.");
        }
    }

    // Numerirani izbor nad vrijednostima enuma.
    public static T ReadEnum<T>(string prompt) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        for (var i = 0; i < values.Length; i++)
            Console.WriteLine($"  {i + 1}) {values[i]}");
        while (true)
        {
            var n = ReadInt(prompt);
            if (n >= 1 && n <= values.Length) return values[n - 1];
            Console.WriteLine($"  ! Odaberi 1-{values.Length}.");
        }
    }

    public static bool Confirm(string prompt)
    {
        Console.Write($"{prompt} (d/n): ");
        var s = ReadLineOrExit().Trim().ToLowerInvariant();
        return s is "d" or "da" or "y";
    }

    public static void Pause()
    {
        Console.Write("... Enter za nastavak ");
        ReadLineOrExit();
    }
}
