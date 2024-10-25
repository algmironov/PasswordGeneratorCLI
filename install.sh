#!/bin/bash

# Determining colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Installing Password Generator CLI tool...${NC}"

# Checking if .NET SDK is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}Error: .NET SDK is not installed!${NC}"
    echo "Please install .NET SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Determining OS
case "$(uname -s)" in
    Darwin)
        echo 'macOS detected'
        ;;
    Linux)
        echo 'Linux detected'
        ;;
    CYGWIN*|MINGW*|MSYS*)
        echo 'Windows detected'
        ;;
    *)
        echo -e "${RED}Unsupported operating system${NC}"
        exit 1
        ;;
esac

# Remove old version if installed
dotnet tool uninstall -g PasswordGenCLI 2>/dev/null

# Creating package
echo -e "${YELLOW}Creating package...${NC}"
dotnet pack

# Installing tool globally
echo -e "${YELLOW}Installing tool...${NC}"
dotnet tool install --global --add-source ./nupkg PasswordGenCLI

if [ $? -eq 0 ]; then
    echo -e "${GREEN}\nInstallation successful!${NC}"
    echo "You can now use 'pwgen' command from any terminal"
    echo "Try 'pwgen --help' for usage information"
else
    echo -e "${RED}\nInstallation failed!${NC}"
    exit 1
fi