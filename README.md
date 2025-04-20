# Password Generator CLI

A cross-platform command-line tool for generating secure passwords.

## Example

![example](Example/pwgen.gif)

## Installation

### Prerequisites
- .NET 9.0 or later

### Install from NuGet
```bash
dotnet tool install --global PasswordGenCLI
```

### Manual Installation
1. Clone the repository
2. Run in the project directory:
```bash
dotnet pack
dotnet tool install --global --add-source ./nupkg PasswordGenCLI
```

## Usage

### NEW FEATURES

Init local secure storage:
```bash
pwgen init
```

Add new entry to storage:
```bash
pwgen new -s GitHub -l example@gmail.com
```

Read password from storage
```bash
pwgen read --service GitHub
#if there are several entries for the same service they will be shown with logins to choose which password you want to copy
```

Read option for clipboard copy to set the timeout (in seconds). After expiration the password will be cleared from the clipboard.
```bash
pwgen read -s GitHub -t 30
# copied password will be cleared from the clipboard after 30 second
```
There are also `update` and `delete` commands to update or delete entry from the storage
```bash
pwgen update -s GitHub -l example@gmail.com

pwgen delete -s GitHub -l example@gmail.com
```

### BASE FEATURES
Show all entries
```bash
pwgen read --list 
```


Generate a password with default settings (14 characters):
```bash
pwgen
```

Specify password length:
```bash
pwgen -l 16
pwgen --length 20
```

Include special characters:
```bash
pwgen -u
pwgen --use-symbols
```

Custom special characters:
```bash
pwgen -s "#$%" -u
pwgen --symbols "@!%" --use-symbols
```

Show help:
```bash
pwgen --help
```

## Uninstallation
```bash
dotnet tool uninstall -g PasswordGenCLI
```

## Platform Support
- Windows
- macOS
- Linux

## License
MIT
