using QRCoder;
using System.Text.Json;

namespace WifiQrGenerator;



class Program
{
    const string Folder = @"c:/Temp";

    static async Task Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("Press any key to start...");
        Console.ReadKey();
        //PrintBanner();

        ClearTempFolder();
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

        Console.WriteLine("\n QR Codes generated in c:/Temp/ Press any key to exit...");
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

        File.WriteAllBytes($@"{Folder}/{config.FileName}.png", qrCodeBytes);
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

    static void ClearTempFolder(string folderPath = Folder)
    {
        try
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error clearing temp folder: {ex.Message}");
            Console.ResetColor();
        }
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


/*
 [
  {
    "fileName": "Central Camden One-Bedroom Apartment",
    "ssid": "BTB-CZFM63",
    "password": "CAKefn6MnPRTgp"
  },
  {
    "fileName": "Camden Central 4-Bedroom Flat",
    "ssid": "VM0750750",
    "password": "uttsR4tuzt7xthty"
  },
  {
    "fileName": "The Lambs Passage Executive Apartment",
    "ssid": "Smart-5G-Hub-8FHT",
    "password": "t7LCpnF7wAfG"
  },
  {
    "fileName": "Modern Executive 2BR Apartment Heart of City of London",
    "ssid": "BTB-76KZs9",
    "password": "W6hQUFHAH9CXbwex"
  },
  {
    "fileName": "Fitzrovia Apartments",
    "ssid": "London2025",
    "password": "102GreatTitchfieldSt!"
  },
  {
    "fileName": "Camden Town Studios",
    "ssid": "BTB-NXFMZ3",
    "password": "xdCUGcamfRGHv6"
  },
  {
    "fileName": "Stylish Camden Town Apartments",
    "ssid": "ALHN-2A2C",
    "password": "hxj3rUQukn"
  },
  {
    "fileName": "1-Min to Wembley Station, Walk to Stadium",
    "ssid": "Smart-5G-hub-9jxf",
    "password": "DkxRrL9JgKUQ"
  },
  {
    "fileName": "City 2BR Near Thames & Bank",
    "ssid": "BTB-G2ZXG5",
    "password": "wkP3i6iVE7wMxQkw"
  },
  {
    "fileName": "Camden Town Apartments",
    "ssid": "EE-Z8f6qc",
    "password": "V7FC7XV79NDXRCfX"
  },
  {
    "fileName": "Kings Cross Grand Stay",
    "ssid": "BTB-6MFMXM",
    "password": "EHk7YdmGMhdqyq"
  },
  {
    "fileName": "Camden Town 1-Min to Station Sleeps 5",
    "ssid": "EE-ZKCX7X",
    "password": "gJCaTakFUfUbm9"
  },
  {
    "fileName": "Camden apartments",
    "ssid": "VM5637A0",
    "password": "4hdNMcfytfya"
  },
  {
    "fileName": "Cosy Flat London",
    "ssid": "EE-5CCZPP",
    "password": "C6NyKVQNpXbdym"
  },
  {
    "fileName": "Kensington Apartments",
    "ssid": "KensingtonApartments",
    "password": "Kensingtonguest"
  },
  {
    "fileName": "Central London 2 Bedroom Penthouse",
    "ssid": "GP_E5_EA186",
    "password": "eBX5S>VM"
  }
]
 */