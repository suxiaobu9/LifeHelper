# LifeHelper

## User secret

```powershell
# 進入到 Server
cd Server

# 建立使用者的 user-secrets
dotnet user-secrets init --id 647e7628-55a7-4ce9-9cf5-a23436a78f88

# 設定 user-secrets 的 DB 連線資訊
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=;Database=;user id=;password="
dotnet user-secrets set "LineChatBot:ChannelAccessToken" ""
dotnet user-secrets set "LineChatBot:ChannelSecret" ""
dotnet user-secrets set "LIFF:LiffId" ""
dotnet user-secrets set "LIFF:ChannelId" ""
dotnet user-secrets set "LIFF:ChannelSecret" ""

```

## Tailwindcss

```powershell
# 進入到 Client
cd Client

npm install

```
