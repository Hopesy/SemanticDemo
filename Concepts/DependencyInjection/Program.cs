#pragma warning disable SKEXP0001

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using Common;

namespace Concepts.DependencyInjection;

/// <summary>
/// 依赖注入核心概念
/// 演示如何使用 DI 容器管理 Kernel 和服务
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 依赖注入核心概念 ===\n");

        try
        {
            // ===== 示例 1: 基础 DI 集成 =====
            await Example1_BasicDI();
            // ===== 示例 2: 注入自定义服务 =====
            await Example2_CustomService();
            // ===== 示例 3: 日志集成 =====
            await Example3_Logging();

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
    /// 示例 1: 基础依赖注入
    /// </summary>
    static async Task Example1_BasicDI()
    {
        Console.WriteLine("【示例 1】基础依赖注入\n");

        // 创建 DI 容器
        var services = new ServiceCollection();

        // 注册 Kernel（使用统一配置）
        ConfigureKernel(services.AddKernel());

        // 构建服务提供程序
        var serviceProvider = services.BuildServiceProvider();

        // 从 DI 容器获取 Kernel
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        var result = await kernel.InvokePromptAsync("用一句话介绍依赖注入的好处");
        Console.WriteLine($"结果: {result}\n");
    }

    /// <summary>
    /// 示例 2: 注入自定义服务
    /// </summary>
    static async Task Example2_CustomService()
    {
        Console.WriteLine("【示例 2】注入自定义服务\n");

        var services = new ServiceCollection();

        // 注册自定义服务
        services.AddSingleton<IGreetingService, GreetingService>();

        // 注册 Kernel 和插件
        var builder = services.AddKernel();
        ConfigureKernel(builder);

        // 注册插件（插件会自动从 DI 容器获取依赖）
        builder.Plugins.AddFromType<GreetingPlugin>();

        var serviceProvider = services.BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        // 调用插件（插件内部使用了注入的服务）
        var result = await kernel.InvokeAsync("GreetingPlugin", "GetGreeting", new() { ["name"] = "张三" });
        Console.WriteLine($"问候语: {result}\n");
    }

    /// <summary>
    /// 示例 3: 日志集成
    /// </summary>
    static async Task Example3_Logging()
    {
        Console.WriteLine("【示例 3】日志集成\n");

        var services = new ServiceCollection();

        // 添加日志
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // 注册 Kernel
        var kernelBuilder = services.AddKernel();
        ConfigureKernel(kernelBuilder);

        var serviceProvider = services.BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        Console.WriteLine("执行提示（查看日志输出）:\n");
        var result = await kernel.InvokePromptAsync("什么是 Semantic Kernel？");
        Console.WriteLine($"\n结果: {result}\n");
    }

    /// <summary>
    /// 配置 Kernel Builder（使用统一的 Settings 配置）
    /// </summary>
    static void ConfigureKernel(IKernelBuilder builder)
    {
        var (model, endpoint, apiKey, orgId) = Settings.LoadFromFile();

        if (!string.IsNullOrEmpty(endpoint))
        {
            // 自定义端点（DeepSeek、本地模型等）
            var httpClient = new HttpClient { BaseAddress = new Uri(endpoint) };
            builder.AddOpenAIChatCompletion(model, apiKey, orgId, httpClient: httpClient);
        }
        else
        {
            // 标准 OpenAI
            builder.AddOpenAIChatCompletion(model, apiKey, orgId);
        }
    }
}

/// <summary>
/// 问候服务接口
/// </summary>
public interface IGreetingService
{
    string GetGreeting(string name);
}

/// <summary>
/// 问候服务实现
/// </summary>
public class GreetingService : IGreetingService
{
    public string GetGreeting(string name)
    {
        var hour = DateTime.Now.Hour;
        var timeOfDay = hour < 12 ? "早上" : hour < 18 ? "下午" : "晚上";
        return $"{timeOfDay}好，{name}！";
    }
}

/// <summary>
/// 问候插件（使用依赖注入）
/// </summary>
public class GreetingPlugin
{
    private readonly IGreetingService _greetingService;

    // 通过构造函数注入依赖
    public GreetingPlugin(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [KernelFunction, Description("获取个性化问候语")]
    public string GetGreeting([Description("用户名")] string name)
    {
        return _greetingService.GetGreeting(name);
    }
}
