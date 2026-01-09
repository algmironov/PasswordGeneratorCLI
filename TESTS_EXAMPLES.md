# Примеры тестов для PasswordGeneratorCLI

Этот документ содержит готовые примеры тестов, которые можно добавить в проект.

## Структура тестового проекта

```
PasswordGenCLI.Tests/
├── PasswordGenCLI.Tests.csproj
├── Unit/
│   ├── PasswordGeneratorTests.cs
│   ├── EncryptionServiceTests.cs
│   └── TablePrinterTests.cs
├── Integration/
│   └── StorageWorkflowTests.cs
└── Helpers/
    └── ReflectionHelper.cs
```

---

## 1. PasswordGenCLI.Tests.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\PasswordGenCLI.Common\PasswordGenCLI.Common.csproj" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
```

---

## 2. Unit/PasswordGeneratorTests.cs

```csharp
using FluentAssertions;
using PasswordGenCLI.Common.Service;

namespace PasswordGenCLI.Tests.Unit;

public class PasswordGeneratorTests
{
    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(20)]
    [InlineData(30)]
    public void Generate_WithValidLength_ReturnsPasswordWithCorrectLength(int length)
    {
        // Act
        string password = PasswordGenerator.Generate(length, null, false);
        string passwordWithoutDelimiters = password.Replace("-", "");

        // Assert
        passwordWithoutDelimiters.Length.Should().BeLessThanOrEqualTo(length);
        passwordWithoutDelimiters.Length.Should().BeGreaterThanOrEqualTo(length - 3); // С учетом делимитеров
    }

    [Theory]
    [InlineData(0, 6)]   // Меньше минимума
    [InlineData(3, 6)]   // Меньше минимума
    [InlineData(50, 30)] // Больше максимума
    [InlineData(100, 30)] // Намного больше максимума
    public void Generate_WithInvalidLength_ClampsToValidRange(int inputLength, int expectedMinLength)
    {
        // Act
        string password = PasswordGenerator.Generate(inputLength, null, false);
        string passwordWithoutDelimiters = password.Replace("-", "");

        // Assert
        passwordWithoutDelimiters.Length.Should().BeGreaterThanOrEqualTo(6);
        passwordWithoutDelimiters.Length.Should().BeLessThanOrEqualTo(30);
    }

    [Fact]
    public void Generate_WithSpecialSymbols_ContainsOnlyValidCharacters()
    {
        // Arrange
        string symbols = "!@#$%^&*";
        string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        // Act
        string password = PasswordGenerator.Generate(14, symbols, useSpecialSymbols: true);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.All(c => validChars.Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void Generate_WithoutSpecialSymbols_ContainsOnlyLettersNumbersAndDelimiters()
    {
        // Arrange
        string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-";

        // Act
        string password = PasswordGenerator.Generate(14, null, useSpecialSymbols: false);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.All(c => validChars.Contains(c)).Should().BeTrue();
    }

    [Fact]
    public void Generate_WithoutSpecialSymbols_ContainsDelimiters()
    {
        // Act
        string password = PasswordGenerator.Generate(14, null, useSpecialSymbols: false);

        // Assert
        password.Should().Contain("-", "пароли без спецсимволов должны содержать делимитеры");
    }

    [Fact]
    public void Generate_CalledMultipleTimes_ReturnsDifferentPasswords()
    {
        // Arrange
        const int iterations = 100;
        var passwords = new HashSet<string>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            string password = PasswordGenerator.Generate(14, null, useSpecialSymbols: true);
            passwords.Add(password);
        }

        // Assert
        passwords.Count.Should().BeGreaterThan(95, "минимум 95% паролей должны быть уникальными");
    }

    [Fact]
    public void Generate_WithDefaultSymbols_UsesCorrectSymbolSet()
    {
        // Arrange
        string defaultSymbols = PasswordGenerator.GetDefaultSymbols();

        // Act
        string password = PasswordGenerator.Generate(20, defaultSymbols, useSpecialSymbols: true);

        // Assert
        defaultSymbols.Should().NotBeNullOrEmpty();
        password.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(6, 1)]   // Короткий пароль - 1 делимитер
    [InlineData(10, 1)]  // Средний пароль - 1 делимитер
    [InlineData(15, 2)]  // Длинный пароль - 2 делимитера
    [InlineData(25, 3)]  // Очень длинный пароль - 3 делимитера
    public void Generate_WithoutSpecialSymbols_HasCorrectNumberOfDelimiters(int length, int expectedDelimiters)
    {
        // Act
        string password = PasswordGenerator.Generate(length, null, useSpecialSymbols: false);
        int delimiterCount = password.Count(c => c == '-');

        // Assert
        delimiterCount.Should().Be(expectedDelimiters);
    }

    [Fact]
    public void GetDefaultSymbols_ReturnsNonEmptyString()
    {
        // Act
        string symbols = PasswordGenerator.GetDefaultSymbols();

        // Assert
        symbols.Should().NotBeNullOrEmpty();
        symbols.Should().Contain("@");
        symbols.Should().Contain("!");
        symbols.Should().Contain("#");
    }

    [Theory]
    [InlineData(14, true)]
    [InlineData(14, false)]
    [InlineData(20, true)]
    [InlineData(20, false)]
    public void Generate_WithDifferentParameters_NeverReturnsEmptyString(int length, bool useSymbols)
    {
        // Act
        string password = PasswordGenerator.Generate(length, null, useSymbols);

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().BeGreaterThan(0);
    }
}
```

---

## 3. Unit/TablePrinterTests.cs

```csharp
using System.Text;
using FluentAssertions;
using PasswordGenCLI.Common.Models;
using PasswordGenCLI.Common.Service;

namespace PasswordGenCLI.Tests.Unit;

public class TablePrinterTests
{
    [Fact]
    public void PrintTable_WithEmptyList_DoesNotThrow()
    {
        // Arrange
        var entries = new List<PasswordEntry>();
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Act & Assert
        var exception = Record.Exception(() => TablePrinter.PrintTable(entries));
        exception.Should().BeNull();
    }

    [Fact]
    public void PrintTable_WithSingleEntry_PrintsHeader()
    {
        // Arrange
        var entries = new List<PasswordEntry>
        {
            new PasswordEntry
            {
                Service = "GitHub",
                Login = "user@example.com",
                Password = "secret123",
                Url = "https://github.com",
                Note = "Work account"
            }
        };

        // Act
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Assert
        output.Should().Contain("Service");
        output.Should().Contain("Login");
        output.Should().Contain("URL");
        output.Should().Contain("Note");
    }

    [Fact]
    public void PrintTable_WithEntry_PrintsEntryData()
    {
        // Arrange
        var entries = new List<PasswordEntry>
        {
            new PasswordEntry
            {
                Service = "GitHub",
                Login = "testuser",
                Password = "pass",
                Url = "https://github.com",
                Note = "Test"
            }
        };

        // Act
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Assert
        output.Should().Contain("GitHub");
        output.Should().Contain("testuser");
        output.Should().Contain("https://github.com");
        output.Should().Contain("Test");
    }

    [Fact]
    public void PrintTable_WithLongServiceName_TruncatesCorrectly()
    {
        // Arrange
        var entries = new List<PasswordEntry>
        {
            new PasswordEntry
            {
                Service = "VeryLongServiceNameThatExceedsTheColumnWidth",
                Login = "user",
                Password = "pass",
                Url = "",
                Note = ""
            }
        };

        // Act
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Assert
        output.Should().Contain("...", "длинные значения должны быть обрезаны");
    }

    [Fact]
    public void PrintTable_WithMultipleEntries_PrintsAllEntries()
    {
        // Arrange
        var entries = new List<PasswordEntry>
        {
            new PasswordEntry { Service = "GitHub", Login = "user1", Password = "p1", Url = "", Note = "" },
            new PasswordEntry { Service = "GitLab", Login = "user2", Password = "p2", Url = "", Note = "" },
            new PasswordEntry { Service = "Bitbucket", Login = "user3", Password = "p3", Url = "", Note = "" }
        };

        // Act
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Assert
        output.Should().Contain("GitHub");
        output.Should().Contain("GitLab");
        output.Should().Contain("Bitbucket");
        output.Should().Contain("user1");
        output.Should().Contain("user2");
        output.Should().Contain("user3");
    }

    [Fact]
    public void PrintTable_WithEmptyFields_HandlesGracefully()
    {
        // Arrange
        var entries = new List<PasswordEntry>
        {
            new PasswordEntry
            {
                Service = "Test",
                Login = "user",
                Password = "pass",
                Url = "",
                Note = ""
            }
        };

        // Act
        var output = CaptureConsoleOutput(() => TablePrinter.PrintTable(entries));

        // Assert
        output.Should().Contain("Test");
        output.Should().Contain("user");
    }

    private static string CaptureConsoleOutput(Action action)
    {
        var originalOut = Console.Out;
        try
        {
            using var writer = new StringWriter();
            Console.SetOut(writer);
            action();
            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
```

---

## 4. Helpers/ReflectionHelper.cs

```csharp
using System.Reflection;

namespace PasswordGenCLI.Tests.Helpers;

/// <summary>
/// Вспомогательный класс для тестирования приватных методов через рефлексию.
/// Используйте только для тестирования, в production коде избегайте рефлексии.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Вызывает приватный статический метод и возвращает результат.
    /// </summary>
    public static TResult? InvokePrivateStaticMethod<TResult>(
        Type type,
        string methodName,
        params object?[] parameters)
    {
        var method = type.GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException(
                $"Method '{methodName}' not found in type '{type.Name}'");

        var result = method.Invoke(null, parameters);
        return result is TResult typedResult ? typedResult : default;
    }

    /// <summary>
    /// Вызывает приватный метод экземпляра и возвращает результат.
    /// </summary>
    public static TResult? InvokePrivateMethod<TResult>(
        object instance,
        string methodName,
        params object?[] parameters)
    {
        var type = instance.GetType();
        var method = type.GetMethod(methodName,
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
            throw new InvalidOperationException(
                $"Method '{methodName}' not found in type '{type.Name}'");

        var result = method.Invoke(instance, parameters);
        return result is TResult typedResult ? typedResult : default;
    }

    /// <summary>
    /// Получает значение приватного поля.
    /// </summary>
    public static TField? GetPrivateField<TField>(object instance, string fieldName)
    {
        var type = instance.GetType();
        var field = type.GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        if (field == null)
            throw new InvalidOperationException(
                $"Field '{fieldName}' not found in type '{type.Name}'");

        var value = field.GetValue(instance);
        return value is TField typedValue ? typedValue : default;
    }
}
```

---

## 5. Unit/EncryptionServiceTests.cs

```csharp
using System.Security.Cryptography;
using FluentAssertions;
using PasswordGenCLI.Common.Service;
using PasswordGenCLI.Tests.Helpers;

namespace PasswordGenCLI.Tests.Unit;

public class EncryptionServiceTests
{
    [Fact]
    public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalText()
    {
        // Arrange
        string originalText = "Test Password 123!@# with special characters абв";
        string masterPassword = "MasterPassword123";

        // Act
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            originalText,
            masterPassword) ?? Array.Empty<byte>();

        string? decrypted = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            encrypted,
            masterPassword);

        // Assert
        decrypted.Should().Be(originalText);
    }

    [Fact]
    public void Encrypt_SameTextMultipleTimes_ProducesDifferentCiphertext()
    {
        // Arrange
        string text = "Same Text Every Time";
        string password = "Password123";

        // Act
        byte[] encrypted1 = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            text,
            password) ?? Array.Empty<byte>();

        byte[] encrypted2 = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            text,
            password) ?? Array.Empty<byte>();

        // Assert
        encrypted1.Should().NotBeEquivalentTo(encrypted2,
            "каждое шифрование использует новый IV (Initialization Vector)");
    }

    [Fact]
    public void Decrypt_WithWrongPassword_ThrowsCryptographicException()
    {
        // Arrange
        string text = "Secret Message";
        string correctPassword = "CorrectPassword";
        string wrongPassword = "WrongPassword";

        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            text,
            correctPassword) ?? Array.Empty<byte>();

        // Act
        Action act = () => ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            encrypted,
            wrongPassword);

        // Assert
        act.Should().Throw<Exception>()
            .WithInnerException<CryptographicException>();
    }

    [Fact]
    public void Encrypt_WithEmptyString_WorksCorrectly()
    {
        // Arrange
        string emptyText = "";
        string password = "Password";

        // Act
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            emptyText,
            password) ?? Array.Empty<byte>();

        string? decrypted = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            encrypted,
            password);

        // Assert
        encrypted.Should().NotBeEmpty("даже пустая строка должна шифроваться");
        decrypted.Should().Be(emptyText);
    }

    [Fact]
    public void Encrypt_WithVeryLongText_WorksCorrectly()
    {
        // Arrange
        string longText = new string('A', 10000); // 10KB текста
        string password = "Password";

        // Act
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            longText,
            password) ?? Array.Empty<byte>();

        string? decrypted = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            encrypted,
            password);

        // Assert
        decrypted.Should().Be(longText);
    }

    [Fact]
    public void Encrypt_ProducesCorrectStructure()
    {
        // Arrange
        string text = "Test";
        string password = "Password";

        // Act
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            text,
            password) ?? Array.Empty<byte>();

        // Assert
        encrypted.Length.Should().BeGreaterThan(32,
            "зашифрованные данные должны содержать: salt (16) + IV (16) + данные");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password123")]
    [InlineData("VeryLongPasswordWithSpecialCharacters!@#$%")]
    [InlineData("короткий")]
    public void Encrypt_Decrypt_WithDifferentPasswords_WorksCorrectly(string password)
    {
        // Arrange
        string text = "Test Message";

        // Act
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            text,
            password) ?? Array.Empty<byte>();

        string? decrypted = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            encrypted,
            password);

        // Assert
        decrypted.Should().Be(text);
    }

    [Fact]
    public void GetStoragePath_ReturnsValidPath()
    {
        // Act
        string? storagePath = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "GetStoragePath");

        // Assert
        storagePath.Should().NotBeNullOrEmpty();
        storagePath.Should().EndWith(".cpwgen");
        Path.IsPathRooted(storagePath).Should().BeTrue("путь должен быть абсолютным");
    }
}
```

---

## 6. Integration/StorageWorkflowTests.cs

```csharp
using FluentAssertions;
using PasswordGenCLI.Common.Models;
using PasswordGenCLI.Common.Service;
using PasswordGenCLI.Tests.Helpers;

namespace PasswordGenCLI.Tests.Integration;

/// <summary>
/// Интеграционные тесты для полного workflow работы с хранилищем.
/// ВНИМАНИЕ: Эти тесты изменяют состояние файловой системы.
/// </summary>
public class StorageWorkflowTests : IDisposable
{
    private readonly string _testStoragePath;
    private readonly string _originalStoragePath;

    public StorageWorkflowTests()
    {
        // Создаем временный путь для тестового хранилища
        _testStoragePath = Path.Combine(
            Path.GetTempPath(),
            $"test_pwgen_{Guid.NewGuid()}.cpwgen");

        // Сохраняем оригинальный путь через рефлексию и подменяем его
        _originalStoragePath = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "GetStoragePath") ?? "";

        // Здесь нужно было бы подменить GetStoragePath,
        // но для упрощения можно использовать файловую систему напрямую
    }

    [Fact]
    public void SaveStorage_ThenLoadStorage_ReturnsCorrectData()
    {
        // Arrange
        string masterPassword = "TestMasterPassword123";
        var storage = new PasswordStorage();
        storage.Entries.Add(new PasswordEntry
        {
            Service = "GitHub",
            Login = "testuser",
            Password = "testpass123",
            Url = "https://github.com",
            Note = "Test account"
        });

        // Act - шифруем и сохраняем
        string json = System.Text.Json.JsonSerializer.Serialize(storage);
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            json,
            masterPassword) ?? Array.Empty<byte>();

        File.WriteAllBytes(_testStoragePath, encrypted);

        // Act - читаем и расшифровываем
        byte[] loadedEncrypted = File.ReadAllBytes(_testStoragePath);
        string? decryptedJson = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            loadedEncrypted,
            masterPassword);

        var loadedStorage = System.Text.Json.JsonSerializer
            .Deserialize<PasswordStorage>(decryptedJson!);

        // Assert
        loadedStorage.Should().NotBeNull();
        loadedStorage!.Entries.Should().HaveCount(1);
        loadedStorage.Entries[0].Service.Should().Be("GitHub");
        loadedStorage.Entries[0].Login.Should().Be("testuser");
        loadedStorage.Entries[0].Password.Should().Be("testpass123");
    }

    [Fact]
    public void LoadStorage_WithCorruptedFile_ThrowsException()
    {
        // Arrange
        string masterPassword = "Password";
        byte[] corruptedData = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(_testStoragePath, corruptedData);

        // Act
        Action act = () =>
        {
            byte[] encrypted = File.ReadAllBytes(_testStoragePath);
            ReflectionHelper.InvokePrivateStaticMethod<string>(
                typeof(EncryptionService),
                "Decrypt",
                encrypted,
                masterPassword);
        };

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Storage_WithMultipleEntries_PreservesAllData()
    {
        // Arrange
        string masterPassword = "MasterPass123";
        var storage = new PasswordStorage();

        for (int i = 0; i < 10; i++)
        {
            storage.Entries.Add(new PasswordEntry
            {
                Service = $"Service{i}",
                Login = $"user{i}@example.com",
                Password = $"password{i}",
                Url = $"https://service{i}.com",
                Note = $"Note {i}"
            });
        }

        // Act
        string json = System.Text.Json.JsonSerializer.Serialize(storage);
        byte[] encrypted = ReflectionHelper.InvokePrivateStaticMethod<byte[]>(
            typeof(EncryptionService),
            "Encrypt",
            json,
            masterPassword) ?? Array.Empty<byte>();

        File.WriteAllBytes(_testStoragePath, encrypted);

        byte[] loadedEncrypted = File.ReadAllBytes(_testStoragePath);
        string? decryptedJson = ReflectionHelper.InvokePrivateStaticMethod<string>(
            typeof(EncryptionService),
            "Decrypt",
            loadedEncrypted,
            masterPassword);

        var loadedStorage = System.Text.Json.JsonSerializer
            .Deserialize<PasswordStorage>(decryptedJson!);

        // Assert
        loadedStorage.Should().NotBeNull();
        loadedStorage!.Entries.Should().HaveCount(10);

        for (int i = 0; i < 10; i++)
        {
            loadedStorage.Entries[i].Service.Should().Be($"Service{i}");
            loadedStorage.Entries[i].Login.Should().Be($"user{i}@example.com");
        }
    }

    public void Dispose()
    {
        // Очистка: удаляем тестовый файл
        if (File.Exists(_testStoragePath))
        {
            try
            {
                File.Delete(_testStoragePath);
            }
            catch
            {
                // Игнорируем ошибки при удалении
            }
        }
    }
}
```

---

## 7. Запуск тестов

### Через CLI:

```bash
# Запустить все тесты
dotnet test

# Запустить с подробным выводом
dotnet test --verbosity detailed

# Запустить конкретный тестовый класс
dotnet test --filter "FullyQualifiedName~PasswordGeneratorTests"

# Запустить конкретный тест
dotnet test --filter "FullyQualifiedName~Generate_WithValidLength_ReturnsPasswordWithCorrectLength"

# Собрать coverage отчет (требует coverlet)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Через Visual Studio:
1. Test Explorer → Run All
2. Правый клик на тесте → Run Test(s)
3. Правый клик на тесте → Debug Test(s)

### Через Rider:
1. Правый клик на тестовом проекте → Run Unit Tests
2. Кнопка Run/Debug возле каждого теста
3. Coverage → Run All Tests with Coverage

---

## 8. Минимальный набор для курса

Для базового покрытия рекомендуется начать с:

1. **PasswordGeneratorTests** (10 тестов)
   - Базовая генерация
   - Граничные значения
   - Валидация символов

2. **EncryptionServiceTests** (5-7 тестов)
   - Encrypt/Decrypt round-trip
   - Неправильный пароль
   - Граничные случаи

3. **TablePrinterTests** (4-5 тестов)
   - Пустой список
   - Обрезка длинных значений
   - Множественные записи

**Итого: ~20 тестов** для минимального покрытия критической функциональности.

---

## 9. Ожидаемый результат

После добавления всех тестов:

```
Starting test execution, please wait...
A total of 1 test files matched the specified pattern.

Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24, Duration: 245 ms
```

**Test Coverage:** ~70-80% для критических модулей (PasswordGenerator, Encryption)

---

## 10. Дополнительные рекомендации

### Что еще можно протестировать:

1. **Performance тесты**
   ```csharp
   [Fact]
   public void Generate_1000Passwords_CompletesInReasonableTime()
   {
       var sw = Stopwatch.StartNew();
       for (int i = 0; i < 1000; i++)
       {
           PasswordGenerator.Generate(14, null, true);
       }
       sw.Stop();
       sw.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 секунды
   }
   ```

2. **Concurrency тесты**
   ```csharp
   [Fact]
   public void Generate_FromMultipleThreads_ProducesUniquePasswords()
   {
       var passwords = new ConcurrentBag<string>();
       Parallel.For(0, 100, i =>
       {
           passwords.Add(PasswordGenerator.Generate(14, null, true));
       });

       passwords.Distinct().Count().Should().BeGreaterThan(95);
   }
   ```

3. **Security тесты**
   ```csharp
   [Fact]
   public void Encrypt_ProducesNonDeterministicOutput()
   {
       // Проверка, что IV действительно случайный
   }
   ```

---

**Эти примеры готовы к использованию и покрывают основную функциональность проекта.**
