# 📶 WiFi QR Code Generator

A .NET 8 console application that generates static QR codes for WiFi networks with WPA/WPA2 security.

## Features

- ✅ WPA and WPA2 security support
- ✅ Password input masked with `*`
- ✅ Special character escaping (`;`, `,`, `"`, `\`, `:`)
- ✅ Saves QR code as a **PNG file**
- ✅ Renders QR code directly in the **terminal** (ASCII art)
- ✅ Hidden network support
- ✅ Timestamped output filename

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Usage

```bash
# Restore packages and run
dotnet run

# Or build first, then run the executable
dotnet build -c Release
./bin/Release/net8.0/WifiQrGenerator
```

## Example Session

```
╔══════════════════════════════════════╗
║     📶  WiFi QR Code Generator       ║
║        WPA / WPA2 Support            ║
╚══════════════════════════════════════╝

📡 Enter WiFi Network Name (SSID): MyHomeNetwork
🔑 Enter WiFi Password: **********

🔒 Security Type:
   1. WPA/WPA2 (recommended)
   2. WPA2 only
   Choose [1/2] (default: 1): 1

👁️  Is this a hidden network? [y/N]: n

⏳ Generating QR Code...

  ██████████████  ██  ██████████████
  ...

✅ QR Code saved to: /path/to/wifi_qr_MyHomeNetwork_20250301_120000.png

📋 WiFi String (for manual testing):
   WIFI:T:WPA;S:MyHomeNetwork;P:mypassword;H:false;;
```

## WiFi QR String Format

The standard format used (compatible with Android & iOS):
```
WIFI:T:<security>;S:<ssid>;P:<password>;H:<hidden>;;
```

| Field    | Values              |
|----------|---------------------|
| T        | WPA, WPA2, WEP, nopass |
| S        | Your network name   |
| P        | Your password       |
| H        | true / false        |

## Package Used

| Package  | Purpose               |
|----------|-----------------------|
| [QRCoder](https://github.com/codebude/QRCoder) v1.6.0 | QR code generation (no native deps) |
