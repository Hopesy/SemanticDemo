#pragma warning disable SKEXP0010

using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace Common;

// AI服务配置类，按需创建
// Kernel → 负责聊天对话、插件调用等；-EmbeddingGenerator → 专门负责文本向量化
public class Settings
{
    private const string ConfigFile = "appsettings.json";

    // 聊天模型配置
    public ModelConfig ChatModel { get; set; } = new();

    // 向量模型配置（可选，如果为空则使用 ChatModel 配置）
    public ModelConfig? EmbeddingModel { get; set; }

    #region 辅助方法
    // 从配置文件加载设置
    private static Settings LoadSettings()
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

        return settings;
    }

    // 兼容旧代码的方法（返回 ChatModel 配置）
    public static (string model, string endpoint, string apiKey, string orgId) LoadFromFile()
    {
        var settings = LoadSettings();
        return (
            settings.ChatModel.Model,
            settings.ChatModel.Endpoint,
            settings.ChatModel.ApiKey,
            settings.ChatModel.OrgId
        );
    }
    // 查找配置文件（优先使用 Common 项目目录下的配置）
    private static string? FindConfigFile(string startDir)
    {
        // 获取 Settings.cs 所在的目录（即 Common 项目目录）
        var assemblyLocation = typeof(Settings).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        // 从程序集目录向上查找 Common 目录
        var dir = new DirectoryInfo(assemblyDir ?? startDir);
        while (dir != null)
        {
            // 检查是否存在 Common 子目录，如果当前就是 Common 目录或包含 Common 子目录
            var commonConfigPath = Path.Combine(dir.FullName, "Common", ConfigFile);
            if (File.Exists(commonConfigPath))
            {
                return commonConfigPath;
            }

            // 检查当前目录是否就是 Common 目录
            if (dir.Name.Equals("Common", StringComparison.OrdinalIgnoreCase))
            {
                var configPath = Path.Combine(dir.FullName, ConfigFile);
                if (File.Exists(configPath))
                {
                    return configPath;
                }
            }

            dir = dir.Parent;
        }

        return null;
    }
    //创建示例配置文件
    public static void CreateSampleConfig(string directory)
    {
        var openAiSample = new Settings
        {
            ChatModel = new ModelConfig
            {
                Model = "gpt-4o-mini",
                Endpoint = "",
                ApiKey = "your-openai-api-key",
                OrgId = ""
            },
            EmbeddingModel = new ModelConfig
            {
                Model = "text-embedding-ada-002",
                Endpoint = "",
                ApiKey = "your-openai-api-key",
                OrgId = "",
                Dimensions = 1536
            }
        };

        var deepseekSample = new Settings
        {
            ChatModel = new ModelConfig
            {
                Model = "deepseek-chat",
                Endpoint = "https://api.deepseek.com",
                ApiKey = "your-deepseek-api-key",
                OrgId = ""
            },
            // DeepSeek 不提供 Embedding 服务，需要使用其他服务
            EmbeddingModel = new ModelConfig
            {
                Model = "text-embedding-ada-002",
                Endpoint = "",
                ApiKey = "your-openai-api-key-for-embedding",
                OrgId = "",
                Dimensions = 1536
            }
        };

        var zhipuSample = new Settings
        {
            ChatModel = new ModelConfig
            {
                Model = "glm-4-flash",
                Endpoint = "https://open.bigmodel.cn/api/paas/v4",
                ApiKey = "your-zhipu-api-key",
                OrgId = ""
            },
            // 智谱 AI 的 Embedding 服务
            EmbeddingModel = new ModelConfig
            {
                Model = "embedding-2",
                Endpoint = "https://open.bigmodel.cn/api/paas/v4",
                ApiKey = "your-zhipu-api-key",
                OrgId = "",
                Dimensions = 1024
            }
        };

        var openAiPath = Path.Combine(directory, "appsettings.openai.json");
        var deepseekPath = Path.Combine(directory, "appsettings.deepseek.json");
        var zhipuPath = Path.Combine(directory, "appsettings.zhipu.json");

        File.WriteAllText(openAiPath, JsonSerializer.Serialize(openAiSample, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        File.WriteAllText(deepseekPath, JsonSerializer.Serialize(deepseekSample, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        File.WriteAllText(zhipuPath, JsonSerializer.Serialize(zhipuSample, new JsonSerializerOptions
        {
            WriteIndented = true
        }));

        Console.WriteLine($"已创建示例配置文件:");
        Console.WriteLine($"  - {openAiPath}");
        Console.WriteLine($"  - {deepseekPath}");
        Console.WriteLine($"  - {zhipuPath}");
        Console.WriteLine($"\n请根据你的 AI 服务选择一个，重命名为 appsettings.json 并填入真实的 API 密钥。");
    }
    #endregion

    //【1】创建配置好的Kernel Builder
    // 支持 OpenAI、DeepSeek 等所有兼容 OpenAI API 的服务
    public static IKernelBuilder CreateKernelBuilder()
    {
        var settings = LoadSettings();
        var chatModel = settings.ChatModel;

        var builder = Kernel.CreateBuilder();
        if (!string.IsNullOrEmpty(chatModel.Endpoint))
        {
            // 自定义端点（DeepSeek、本地模型等）
            var httpClient = new HttpClient { BaseAddress = new Uri(chatModel.Endpoint) };
            builder.AddOpenAIChatCompletion(chatModel.Model, chatModel.ApiKey, chatModel.OrgId, httpClient: httpClient);
        }
        else
        {
            // 标准 OpenAI
            builder.AddOpenAIChatCompletion(chatModel.Model, chatModel.ApiKey, chatModel.OrgId);
        }
        return builder;
    }
    //【2】创建IEmbeddingGenerator向量服务，用于文本嵌入功能如Memory、RAG
    //嵌入模型名称默认为和向量维度（1536适用于text-embedding-ada-002）
    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(string? embeddingModel = null,
        int? dimensions = null)
    {
        var settings = LoadSettings();

        // 确定使用的配置：优先使用 EmbeddingModel 配置，否则回退到 ChatModel 配置
        var modelConfig = settings.EmbeddingModel ?? settings.ChatModel;
        var useEmbeddingModel = embeddingModel ?? modelConfig.Model;

        // 确定向量维度：参数 > 配置 > 默认值(1536)
        var useDimensions = dimensions ?? modelConfig.Dimensions ?? 1536;

        if (!string.IsNullOrEmpty(modelConfig.Endpoint))
        {
            // 自定义端点（智谱、DeepSeek、本地模型等）
            var openaiClient = new OpenAIClient(new ApiKeyCredential(modelConfig.ApiKey), new OpenAIClientOptions { Endpoint = new Uri(modelConfig.Endpoint) });
            //这个IChatClient是Microsoft.Extensions.AI提供的，更加存粹只负责聊天，这个调用工具需要自己实现
            //ChatCompletion是semantic kernel提供的更高级封装，聊天时候能自动使用工具
            var embeddingClient = openaiClient.GetEmbeddingClient(useEmbeddingModel);
            return embeddingClient.AsIEmbeddingGenerator(useDimensions);
        }
        else
        {
            // 标准 OpenAI
            var openaiClient = new OpenAIClient(new ApiKeyCredential(modelConfig.ApiKey));
            var embeddingClient = openaiClient.GetEmbeddingClient(useEmbeddingModel);
            return embeddingClient.AsIEmbeddingGenerator(useDimensions);
        }
    }
}

/// <summary>
/// 模型配置
/// </summary>
public class ModelConfig
{
    public string Model { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string OrgId { get; set; } = string.Empty;

    // Embedding 模型的向量维度（仅用于 Embedding 模型）
    public int? Dimensions { get; set; }
}
