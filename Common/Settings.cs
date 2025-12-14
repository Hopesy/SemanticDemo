using System.Text.Json;

namespace Common;

/// <summary>
/// AI 服务配置类
/// </summary>
public class Settings
{
    private const string ConfigFile = "appsettings.json";

    public string Type { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string OrgId { get; set; } = string.Empty;

    /// <summary>
    /// 从配置文件加载设置
    /// </summary>
    public static (bool useAzureOpenAI, string model, string azureEndpoint, string apiKey, string orgId) LoadFromFile()
    {
        // 查找配置文件（向上查找多级目录）
        var currentDir = Directory.GetCurrentDirectory();
        string? configPath = FindConfigFile(currentDir);

        if (configPath == null)
        {
            throw new FileNotFoundException(
                $"未找到配置文件 {ConfigFile}。请在项目根目录或解决方案根目录创建该文件。\n" +
                $"当前目录: {currentDir}");
        }

        var json = File.ReadAllText(configPath);
        var settings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (settings == null)
        {
            throw new InvalidOperationException("配置文件格式错误");
        }

        bool useAzureOpenAI = settings.Type.Equals("azure", StringComparison.OrdinalIgnoreCase);

        return (
            useAzureOpenAI,
            settings.Model,
            settings.Endpoint,
            settings.ApiKey,
            settings.OrgId
        );
    }

    /// <summary>
    /// 向上查找配置文件
    /// </summary>
    private static string? FindConfigFile(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            var configPath = Path.Combine(dir.FullName, ConfigFile);
            if (File.Exists(configPath))
            {
                return configPath;
            }
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>
    /// 创建示例配置文件
    /// </summary>
    public static void CreateSampleConfig(string directory)
    {
        var openAiSample = new Settings
        {
            Type = "openai",
            Model = "gpt-4o-mini",
            ApiKey = "your-openai-api-key",
            OrgId = ""
        };

        var azureSample = new Settings
        {
            Type = "azure",
            Model = "your-deployment-name",
            Endpoint = "https://your-resource-name.openai.azure.com/",
            ApiKey = "your-azure-openai-key"
        };

        var openAiPath = Path.Combine(directory, "appsettings.openai.json");
        var azurePath = Path.Combine(directory, "appsettings.azure.json");

        File.WriteAllText(openAiPath, JsonSerializer.Serialize(openAiSample, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        File.WriteAllText(azurePath, JsonSerializer.Serialize(azureSample, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        Console.WriteLine($"已创建示例配置文件:");
        Console.WriteLine($"  - {openAiPath}");
        Console.WriteLine($"  - {azurePath}");
        Console.WriteLine($"\n请根据你的 AI 服务选择一个，重命名为 appsettings.json 并填入真实的 API 密钥。");
    }
}
