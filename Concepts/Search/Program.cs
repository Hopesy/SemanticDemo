#pragma warning disable SKEXP0001, SKEXP0050

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Common;

namespace Concepts.Search;

/// <summary>
/// 搜索功能核心概念
/// 演示如何集成 Web 搜索功能
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 搜索功能核心概念 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();

            // ===== 示例 1: 模拟搜索插件 =====
            await Example1_MockSearch(kernel);

            // ===== 示例 2: 搜索增强的对话 =====
            await Example2_SearchEnhancedChat(kernel);

            // ===== 示例 3: 多来源搜索 =====
            await Example3_MultiSourceSearch(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
            Console.WriteLine("\n💡 提示: 要使用真实的 Bing 搜索，需要配置 Bing Search API Key");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex.StackTrace}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 示例 1: 模拟搜索插件
    /// </summary>
    static async Task Example1_MockSearch(Kernel kernel)
    {
        Console.WriteLine("【示例 1】模拟搜索插件\n");

        // 导入模拟搜索插件
        kernel.ImportPluginFromType<MockSearchPlugin>("Search");

        // 使用搜索插件
        var result = await kernel.InvokeAsync("Search", "WebSearch", new() { ["query"] = "Semantic Kernel" });
        Console.WriteLine($"搜索查询: Semantic Kernel");
        Console.WriteLine($"搜索结果:\n{result}\n");
    }

    /// <summary>
    /// 示例 2: 搜索增强的对话
    /// </summary>
    static async Task Example2_SearchEnhancedChat(Kernel kernel)
    {
        Console.WriteLine("【示例 2】搜索增强的对话\n");

        // 导入搜索插件
        kernel.ImportPluginFromType<MockSearchPlugin>("Search");

        // 启用自动函数调用
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // AI 会自动调用搜索插件
        var question = "最新的 .NET 版本是什么？";
        Console.WriteLine($"用户问题: {question}\n");

        Console.WriteLine("AI 正在搜索并生成答案...");
        var result = await kernel.InvokePromptAsync(question, new(settings));
        Console.WriteLine($"\n答案: {result}\n");
    }

    /// <summary>
    /// 示例 3: 多来源搜索
    /// </summary>
    static async Task Example3_MultiSourceSearch(Kernel kernel)
    {
        Console.WriteLine("【示例 3】多来源搜索\n");

        // 导入多个搜索插件
        kernel.ImportPluginFromType<MockSearchPlugin>("WebSearch");
        kernel.ImportPluginFromType<MockNewsSearchPlugin>("NewsSearch");
        kernel.ImportPluginFromType<MockDocSearchPlugin>("DocSearch");

        Console.WriteLine("已加载的搜索插件:");
        foreach (var plugin in kernel.Plugins)
        {
            Console.WriteLine($"  - {plugin.Name}");
        }
        Console.WriteLine();

        // 搜索不同来源
        Console.WriteLine("1. Web 搜索:");
        var webResult = await kernel.InvokeAsync("WebSearch", "WebSearch", new() { ["query"] = "AI 技术" });
        Console.WriteLine($"   {webResult}\n");

        Console.WriteLine("2. 新闻搜索:");
        var newsResult = await kernel.InvokeAsync("NewsSearch", "NewsSearch", new() { ["query"] = "AI 技术" });
        Console.WriteLine($"   {newsResult}\n");

        Console.WriteLine("3. 文档搜索:");
        var docResult = await kernel.InvokeAsync("DocSearch", "DocSearch", new() { ["query"] = "AI 技术" });
        Console.WriteLine($"   {docResult}\n");
    }
}

/// <summary>
/// 模拟 Web 搜索插件
/// </summary>
public class MockSearchPlugin
{
    [KernelFunction, Description("在互联网上搜索信息")]
    public string WebSearch([Description("搜索查询")] string query)
    {
        // 模拟搜索结果
        return $"""
            搜索 "{query}" 的结果:

            1. Semantic Kernel 官方文档
               Semantic Kernel 是微软开发的开源 AI 编排框架...
               来源: learn.microsoft.com

            2. Semantic Kernel GitHub 仓库
               在 GitHub 上查看 Semantic Kernel 的源代码和示例...
               来源: github.com/microsoft/semantic-kernel

            3. Semantic Kernel 入门教程
               学习如何使用 Semantic Kernel 构建 AI 应用...
               来源: devblogs.microsoft.com
            """;
    }
}

/// <summary>
/// 模拟新闻搜索插件
/// </summary>
public class MockNewsSearchPlugin
{
    [KernelFunction, Description("搜索最新新闻")]
    public string NewsSearch([Description("搜索查询")] string query)
    {
        return $"""
            关于 "{query}" 的最新新闻:

            • 微软发布 Semantic Kernel 新版本
              时间: 2天前
              来源: TechCrunch

            • AI 编排框架市场分析报告
              时间: 1周前
              来源: Forbes
            """;
    }
}

/// <summary>
/// 模拟文档搜索插件
/// </summary>
public class MockDocSearchPlugin
{
    [KernelFunction, Description("在文档库中搜索")]
    public string DocSearch([Description("搜索查询")] string query)
    {
        return $"""
            在文档库中搜索 "{query}":

            📄 Semantic Kernel 架构设计文档
               章节: 核心概念
               更新: 2024-01-15

            📄 Semantic Kernel API 参考手册
               章节: Kernel 类
               更新: 2024-01-10
            """;
    }
}
