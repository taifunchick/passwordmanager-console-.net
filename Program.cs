using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PasswordManager;

class Program
{
    private static string _dataFile = "passwords.dat";
    private static string _masterHash = "";
    private static List<PasswordEntry> _entries = new();
    private static bool _isAuthenticated = false;

    static void Main(string[] args)
    {
        Console.Title = "🔐 Password Manager";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════╗");
        Console.WriteLine("║       PASSWORD MANAGER            ║");
        Console.WriteLine("╚═══════════════════════════════════╝");
        Console.ResetColor();

        LoadMasterPassword();
        
        while (!_isAuthenticated)
        {
            if (string.IsNullOrEmpty(_masterHash))
            {
                SetupMasterPassword();
            }
            else
            {
                Login();
            }
        }

        LoadData();
        MainMenu();
    }

    static void SetupMasterPassword()
    {
        Console.WriteLine("\n First time setup - Create master password");
        Console.Write("Enter master password: ");
        var pass1 = ReadPassword();
        Console.Write("\nConfirm master password: ");
        var pass2 = ReadPassword();

        if (pass1 == pass2 && !string.IsNullOrEmpty(pass1))
        {
            _masterHash = HashPassword(pass1);
            File.WriteAllText("master.hash", _masterHash);
            Console.WriteLine("\n Master password created successfully!");
            _isAuthenticated = true;
        }
        else
        {
            Console.WriteLine("\n Passwords do not match or empty!");
        }
    }

    static void Login()
    {
        Console.WriteLine("\n Enter master password to continue");
        Console.Write("Password: ");
        var pass = ReadPassword();

        if (VerifyPassword(pass))
        {
            Console.WriteLine("\n Access granted!");
            _isAuthenticated = true;
        }
        else
        {
            Console.WriteLine("\n Wrong password!");
        }
    }

    static void LoadMasterPassword()
    {
        if (File.Exists("master.hash"))
        {
            _masterHash = File.ReadAllText("master.hash");
        }
    }

    static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    static bool VerifyPassword(string password)
    {
        return HashPassword(password) == _masterHash;
    }

    static string ReadPassword()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        return password.ToString();
    }

    static void LoadData()
    {
        if (File.Exists(_dataFile))
        {
            try
            {
                var encrypted = File.ReadAllBytes(_dataFile);
                var decrypted = Decrypt(encrypted);
                var json = Encoding.UTF8.GetString(decrypted);
                _entries = JsonSerializer.Deserialize<List<PasswordEntry>>(json) ?? new List<PasswordEntry>();
            }
            catch
            {
                _entries = new List<PasswordEntry>();
            }
        }
    }

    static void SaveData()
    {
        var json = JsonSerializer.Serialize(_entries);
        var bytes = Encoding.UTF8.GetBytes(json);
        var encrypted = Encrypt(bytes);
        File.WriteAllBytes(_dataFile, encrypted);
    }

    static byte[] Encrypt(byte[] data)
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        var key = ProtectedData.Protect(aes.Key, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes("key.dat", key);
        File.WriteAllBytes("iv.dat", aes.IV);

        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    static byte[] Decrypt(byte[] data)
    {
        if (!File.Exists("key.dat") || !File.Exists("iv.dat"))
            return Array.Empty<byte>();

        var key = ProtectedData.Unprotect(File.ReadAllBytes("key.dat"), null, DataProtectionScope.CurrentUser);
        var iv = File.ReadAllBytes("iv.dat");

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(data, 0, data.Length);
    }

    static void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════╗");
            Console.WriteLine("║       PASSWORD MANAGER            ║");
            Console.WriteLine("╠═══════════════════════════════════╣");
            Console.WriteLine("║                                   ║");
            Console.WriteLine("║  1.  List all passwords           ║");
            Console.WriteLine("║  2.  Add new password             ║");
            Console.WriteLine("║  3.  Search password              ║");
            Console.WriteLine("║  4.  Edit password                ║");
            Console.WriteLine("║  5.  Delete password              ║");
            Console.WriteLine("║  6.  Statistics                   ║");
            Console.WriteLine("║  7.  Export to CSV                ║");
            Console.WriteLine("║  8.  Exit                         ║");
            Console.WriteLine("║                                   ║");
            Console.WriteLine("╚═══════════════════════════════════╝");
            Console.ResetColor();
            Console.Write("\nChoose option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ListPasswords(); break;
                case "2": AddPassword(); break;
                case "3": SearchPassword(); break;
                case "4": EditPassword(); break;
                case "5": DeletePassword(); break;
                case "6": ShowStats(); break;
                case "7": ExportToCsv(); break;
                case "8": 
                    SaveData();
                    Console.WriteLine("\n Goodbye!");
                    return;
                default: Console.WriteLine("Invalid option!"); break;
            }
        }
    }

    static void ListPasswords()
    {
        Console.Clear();
        Console.WriteLine(" YOUR PASSWORDS\n");

        if (_entries.Count == 0)
        {
            Console.WriteLine("No passwords saved yet. Press any key...");
            Console.ReadKey();
            return;
        }

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            Console.WriteLine($"[{i + 1}] {entry.Service}");
            Console.WriteLine($"    Username: {entry.Username}");
            Console.WriteLine($"    Password: {new string('*', entry.Password.Length)}");
            if (!string.IsNullOrEmpty(entry.Url))
                Console.WriteLine($"    URL: {entry.Url}");
            Console.WriteLine($"    Added: {entry.CreatedAt:yyyy-MM-dd HH:mm}");
            Console.WriteLine();
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    static void AddPassword()
    {
        Console.Clear();
        Console.WriteLine(" ADD NEW PASSWORD\n");

        var entry = new PasswordEntry();

        Console.Write("Service (e.g., Google, GitHub, PornHub): ");
        entry.Service = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(entry.Service))
        {
            Console.WriteLine("Service name required!");
            Console.ReadKey();
            return;
        }

        Console.Write("Username/Email: ");
        entry.Username = Console.ReadLine()?.Trim();

        Console.Write("Generate random password? (y/n): ");
        var generate = Console.ReadLine()?.ToLower() == "y";

        if (generate)
        {
            entry.Password = GeneratePassword();
            Console.WriteLine($"Generated password: {entry.Password}");
        }
        else
        {
            Console.Write("Password: ");
            entry.Password = ReadPasswordString();
        }

        Console.Write("URL (optional): ");
        entry.Url = Console.ReadLine()?.Trim();

        Console.Write("Notes (optional): ");
        entry.Notes = Console.ReadLine()?.Trim();

        entry.CreatedAt = DateTime.Now;
        entry.UpdatedAt = DateTime.Now;

        _entries.Add(entry);
        SaveData();

        Console.WriteLine("\n Password added successfully!");
        Console.ReadKey();
    }

    static string ReadPasswordString()
    {
        var password = new StringBuilder();
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password.ToString();
    }

    static string GeneratePassword(int length = 16)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    static void SearchPassword()
    {
        Console.Clear();
        Console.Write("🔍 Search for service: ");
        var search = Console.ReadLine()?.ToLower();

        var results = _entries.Where(e => e.Service.ToLower().Contains(search ?? "")).ToList();

        if (results.Count == 0)
        {
            Console.WriteLine("No matches found!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"\nFound {results.Count} result(s):\n");

        foreach (var entry in results)
        {
            Console.WriteLine($" {entry.Service}");
            Console.WriteLine($"   Username: {entry.Username}");
            Console.WriteLine($"   Password: {entry.Password}");
            if (!string.IsNullOrEmpty(entry.Url))
                Console.WriteLine($"   URL: {entry.Url}");
            Console.WriteLine();
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }

    static void EditPassword()
    {
        ListPasswords();
        if (_entries.Count == 0) return;

        Console.Write("Select number to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > _entries.Count)
        {
            Console.WriteLine("Invalid selection!");
            Console.ReadKey();
            return;
        }

        var entry = _entries[index - 1];
        Console.Clear();
        Console.WriteLine($" EDITING: {entry.Service}\n");

        Console.Write($"Service [{entry.Service}]: ");
        var newService = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newService)) entry.Service = newService;

        Console.Write($"Username [{entry.Username}]: ");
        var newUsername = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newUsername)) entry.Username = newUsername;

        Console.Write("Change password? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            Console.Write("New password: ");
            entry.Password = ReadPasswordString();
        }

        Console.Write($"URL [{entry.Url}]: ");
        var newUrl = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newUrl)) entry.Url = newUrl;

        Console.Write($"Notes [{entry.Notes}]: ");
        var newNotes = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(newNotes)) entry.Notes = newNotes;

        entry.UpdatedAt = DateTime.Now;
        SaveData();

        Console.WriteLine("\n Password updated!");
        Console.ReadKey();
    }

    static void DeletePassword()
    {
        ListPasswords();
        if (_entries.Count == 0) return;

        Console.Write("Select number to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int index) || index < 1 || index > _entries.Count)
        {
            Console.WriteLine("Invalid selection!");
            Console.ReadKey();
            return;
        }

        var entry = _entries[index - 1];
        Console.Write($"Delete {entry.Service}? (y/n): ");
        
        if (Console.ReadLine()?.ToLower() == "y")
        {
            _entries.RemoveAt(index - 1);
            SaveData();
            Console.WriteLine(" Password deleted!");
        }
        else
        {
            Console.WriteLine("Cancelled");
        }

        Console.ReadKey();
    }

    static void ShowStats()
    {
        Console.Clear();
        Console.WriteLine(" STATISTICS\n");

        Console.WriteLine($"Total passwords: {_entries.Count}");
        Console.WriteLine($"Unique services: {_entries.Select(e => e.Service).Distinct().Count()}");
        
        var byService = _entries.GroupBy(e => e.Service[0].ToString().ToUpper())
            .OrderBy(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()}");
        
        Console.WriteLine($"\nBy first letter:\n{string.Join(", ", byService)}");

        Console.WriteLine($"\nLast added: {(_entries.Count > 0 ? _entries.Max(e => e.CreatedAt).ToString("yyyy-MM-dd HH:mm") : "None")}");

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    static void ExportToCsv()
    {
        if (_entries.Count == 0)
        {
            Console.WriteLine("No passwords to export!");
            Console.ReadKey();
            return;
        }

        var csv = new StringBuilder();
        csv.AppendLine("Service,Username,Password,URL,Notes,CreatedAt,UpdatedAt");

        foreach (var entry in _entries)
        {
            csv.AppendLine($"\"{entry.Service}\",\"{entry.Username}\",\"{entry.Password}\",\"{entry.Url}\",\"{entry.Notes}\",{entry.CreatedAt},{entry.UpdatedAt}");
        }

        var fileName = $"passwords_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        File.WriteAllText(fileName, csv.ToString(), Encoding.UTF8);
        
        Console.WriteLine($" Exported to {fileName}");
        Console.ReadKey();
    }
}

class PasswordEntry
{
    public string Service { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}