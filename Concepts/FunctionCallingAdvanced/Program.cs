#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Common;

namespace Concepts.FunctionCallingAdvanced;

/// <summary>
/// 高级函数调用概念
/// 演示函数调用的高级特性和控制
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 高级函数调用概念 ===\n");

        try
        {
            var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

            var builder = Kernel.CreateBuilder();
            if (useAzureOpenAI)
            {
                builder.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
            }
            else
            {
                builder.AddOpenAIChatCompletion(model, apiKey, orgId);
            }
            var kernel = builder.Build();

            // 导入插件
            kernel.ImportPluginFromType<WeatherPlugin>("Weather");
            kernel.ImportPluginFromType<CurrencyPlugin>("Currency");

            // ===== 示例 1: 自动函数调用 =====
            await Example1_AutoFunctionCalling(kernel);

            // ===== 示例 2: 必须调用函数 =====
            await Example2_RequiredFunctionCalling(kernel);

            // ===== 示例 3: 禁用函数调用 =====
            await Example3_NoFunctionCalling(kernel);

            // ===== 示例 4: 多步骤函数调用 =====
            await Example4_MultiStepFunctionCalling(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 示例 1: 自动函数调用（AI 自动决定是否调用）
    /// </summary>
    static async Task Example1_AutoFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 1】自动函数调用\n");

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await kernel.InvokePromptAsync(
            "北京今天的天气怎么样？",
            new(settings));

        Console.WriteLine($"结果: {result}\n");
    }

    /// <summary>
    /// 示例 2: 必须调用函数（强制 AI 调用函数）
    /// </summary>
    static async Task Example2_RequiredFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 2】必须调用函数\n");

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
        };

        var result = await kernel.InvokePromptAsync(
            "告诉我上海的天气",
            new(settings));

        Console.WriteLine($"结果: {result}\n");
    }

    /// <summary>
    /// 示例 3: 禁用函数调用（AI 不会调用任何函数）
    /// </summary>
    static async Task Example3_NoFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 3】禁用函数调用\n");

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.None()
        };

        var result = await kernel.InvokePromptAsync(
            "深圳今天的天气怎么样？",
            new(settings));

        Console.WriteLine($"结果（没有调用函数，可能会产生幻觉）: {result}\n");
    }

    /// <summary>
    /// 示例 4: 多步骤函数调用（AI 连续调用多个函数）
    /// </summary>
    static async Task Example4_MultiStepFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 4】多步骤函数调用\n");

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await kernel.InvokePromptAsync(
            "北京今天的天气怎么样？如果温度超过 25 度，请将 100 美元转换为人民币",
            new(settings));

        Console.WriteLine($"结果: {result}\n");
    }
}

/// <summary>
/// 天气插件 - 提供天气信息
/// </summary>
public class WeatherPlugin
{
    [KernelFunction, Description("获取指定城市的天气信息")]
    public string GetWeather([Description("城市名称")] string city)
    {
        // 模拟天气数据
        var random = new Random();
        var temperature = random.Next(15, 35);
        var conditions = new[] { "晴天", "多云", "小雨", "阴天" };
        var condition = conditions[random.Next(conditions.Length)];

        return $"{city}的天气: {condition}，温度 {temperature}°C";
    }

    [KernelFunction, Description("获取指定城市的温度")]
    public int GetTemperature([Description("城市名称")] string city)
    {
        // 模拟温度数据
        var random = new Random();
        return random.Next(15, 35);
    }
}

/// <summary>
/// 货币插件 - 提供货币转换
/// </summary>
public class CurrencyPlugin
{
    [KernelFunction, Description("将美元转换为人民币")]
    public string ConvertUsdToCny([Description("美元金额")] double usd)
    {
        double rate = 7.2;  // 模拟汇率
        double cny = usd * rate;
        return $"{usd} 美元 = {cny:F2} 人民币（汇率: {rate}）";
    }

    [KernelFunction, Description("将人民币转换为美元")]
    public string ConvertCnyToUsd([Description("人民币金额")] double cny)
    {
        double rate = 7.2;  // 模拟汇率
        double usd = cny / rate;
        return $"{cny} 人民币 = {usd:F2} 美元（汇率: {rate}）";
    }
}
