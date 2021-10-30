# CyDriveSDK

## Get Started

Create a CyDrive client before all:
```csharp
var client = new CyDriveClient(serverAddr, deviceId, account);
```

You should generate a 32-bit device ID, that's unique for each device of user.

For signing up/in, You need to pass `account`, which should contain `Email` and `Password` fields at least.

### Register new account
This would register a new account for the given email and password.
```csharp
client.RegisterAsync();
```

### Login
This would login, and the state will expire after 24 hours since the last time you accessed server.
```csharp
client.LoginAsync();
```

### Send and recv message
```csharp
client.OnMessage += (sender, e) => 
{
    Console.WriteLine(e.Message.Content);
}

client.SendText("hello CyDrive!", 
    peerId, 
    (bool ok) => Console.WriteLine("sended!"))
```

## Roadmap

| Module |   Features    |  State   |
| :----: | :-----------: | :------: |
| Client |   Register    |  stable  |
| Client |     Login     |  stable  |
| Client | Download file |  stable  |
| Client |  Upload file  |  stable  |
| Client |  GetFileList  |  stable  |