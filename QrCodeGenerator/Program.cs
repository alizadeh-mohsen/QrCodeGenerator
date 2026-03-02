using QRCoder;
using System.Text.Json;

namespace WifiQrGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Clear();
        //PrintBanner();

        //var config = GetWifiConfig();
        var wifis = await LoadWifiConfigsAsync("Wifi.json");
        string wifiString;
        foreach (var config in wifis)
        {
            wifiString = BuildWifiString(config);
            SaveQrAsPng(wifiString, config);
        }
        // Save as PNG
        //string pngPath = SaveQrAsPng(wifiString, config.Ssid);

        //// Also render in terminal
        //PrintQrToConsole(wifiString);

        //Console.ForegroundColor = ConsoleColor.Green;
        //Console.WriteLine($"\n✅ QR Code saved to: {Path.GetFullPath(pngPath)}");
        //Console.ResetColor();

        //Console.WriteLine("\n📋 WiFi String (for manual testing):");
        //Console.ForegroundColor = ConsoleColor.DarkGray;
        //Console.WriteLine($"   {wifiString}");
        //Console.ResetColor();

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║     📶  WiFi QR Code Generator       ║");
        Console.WriteLine("║        WPA / WPA2 Support            ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    static WifiConfig GetWifiConfig()
    {
        var config = new WifiConfig();

        // SSID
        Console.Write("📡 Enter WiFi Network Name (SSID): ");
        config.Ssid = ReadRequired("SSID");

        // Password
        Console.Write("🔑 Enter WiFi Password: ");
        config.Password = ReadPassword();

        // Security type
        Console.WriteLine("\n🔒 Security Type:");
        Console.WriteLine("   1. WPA/WPA2 (recommended)");
        Console.WriteLine("   2. WPA2 only");
        Console.Write("   Choose [1/2] (default: 1): ");

        string secChoice = Console.ReadLine()?.Trim() ?? "1";
        config.SecurityType = secChoice == "2" ? "WPA2" : "WPA";

        // Hidden network
        Console.Write("\n👁️  Is this a hidden network? [y/N]: ");
        string hidden = Console.ReadLine()?.Trim().ToLower() ?? "n";
        config.Hidden = hidden == "y" || hidden == "yes";

        return config;
    }

    static string ReadRequired(string fieldName)
    {
        string value;
        do
        {
            value = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(value))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"   {fieldName} cannot be empty. Try again: ");
                Console.ResetColor();
            }
        } while (string.IsNullOrEmpty(value));
        return value;
    }

    static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
            else if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    /// <summary>
    /// Builds the WiFi QR string per the standard format:
    /// WIFI:T:WPA;S:MyNetwork;P:MyPassword;H:false;;
    /// </summary>
    static string BuildWifiString(WifiConfig config)
    {
        string ssid = EscapeWifiString(config.Ssid);
        string password = EscapeWifiString(config.Password);
        string hidden = config.Hidden ? "true" : "false";

        return $"WIFI:T:{config.SecurityType};S:{ssid};P:{password};H:{hidden};;";
    }

    /// <summary>
    /// Escapes special characters in SSID/password for the WiFi QR string format.
    /// Characters that must be escaped: \ ; , " :
    /// </summary>
    static string EscapeWifiString(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\"", "\\\"")
            .Replace(":", "\\:");
    }

    static void SaveQrAsPng(string wifiString, WifiConfig config)
    {
        using var qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(wifiString, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);
        byte[] qrCodeBytes = qrCode.GetGraphic(10);

        File.WriteAllBytes($@"c:/Temp/{config.FileName}.png", qrCodeBytes);
    }

    static void PrintQrToConsole(string wifiString)
    {
        using var qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(wifiString, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new AsciiQRCode(qrCodeData);
        string asciiQr = qrCode.GetGraphic(1, "██", "  ");

        Console.ForegroundColor = ConsoleColor.White;
        foreach (string line in asciiQr.Split('\n'))
            Console.WriteLine("  " + line);
        Console.ResetColor();
    }

    static async Task<List<WifiConfig>> LoadWifiConfigsAsync(string fileName = "wifi.json")
    {
        // Project root path
        string root = AppContext.BaseDirectory;

        // If running from /bin/Debug/... go up to project root
        string projectRoot = Directory.GetParent(root)!.Parent!.Parent!.Parent!.FullName;

        string jsonFilePath = Path.Combine(projectRoot, fileName);
        //C:\Users\offic\source\repos\alizadeh-mohsen\QrCodeGenerator\QrCodeGenerator\bin\Debug\net8.0
        if (!File.Exists(jsonFilePath))
            return new List<WifiConfig>();

        string json = await File.ReadAllTextAsync(jsonFilePath);

        var items = JsonSerializer.Deserialize<List<WifiConfig>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return items ?? new List<WifiConfig>();
    }


}

record WifiConfig
{
    public string Ssid { get; set; } = "";
    public string Password { get; set; } = "";
    public string SecurityType { get; set; } = "WPA";
    public string FileName { get; set; } = "";
    public bool Hidden { get; set; } = false;
}
