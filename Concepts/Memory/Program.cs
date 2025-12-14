#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Common;

namespace Concepts.Memory;

/// <summary>
/// 记忆系统核心概念
/// 演示如何使用嵌入和向量存储实现语义记忆
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 记忆系统核心概念 ===\n");

        try
        {
            // 创建 Kernel（包含 Embedding 服务）
            var kernel = Settings.CreateKernelBuilderWithEmbedding().Build();

            // ===== 示例 1: 文本嵌入生成 =====
            await Example1_TextEmbedding(kernel);

            // ===== 示例 2: 语义记忆存储 =====
            await Example2_SemanticMemory(kernel);

            // ===== 示例 3: 语义搜索 =====
            await Example3_SemanticSearch(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
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
    /// 示例 1: 文本嵌入生成
    /// </summary>
    static async Task Example1_TextEmbedding(Kernel kernel)
    {
        Console.WriteLine("【示例 1】文本嵌入生成\n");

        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // 生成文本的向量表示
        var text = "Semantic Kernel 是一个强大的 AI 编排框架";
        var embedding = await embeddingService.GenerateEmbeddingAsync(text);

        Console.WriteLine($"文本: {text}");
        Console.WriteLine($"嵌入维度: {embedding.Length}");
        Console.WriteLine($"前 5 个值: [{string.Join(", ", embedding.Span.Slice(0, Math.Min(5, embedding.Length)).ToArray().Select(v => v.ToString("F4")))}...]\n");
    }

    /// <summary>
    /// 示例 2: 语义记忆存储（概念演示）
    /// </summary>
    static async Task Example2_SemanticMemory(Kernel kernel)
    {
        Console.WriteLine("【示例 2】语义记忆存储（概念演示）\n");

        Console.WriteLine("语义记忆存储的核心概念:");
        Console.WriteLine("1. 将文本转换为向量（嵌入）");
        Console.WriteLine("2. 存储向量到内存或数据库");
        Console.WriteLine("3. 通过向量相似度检索相关信息\n");

        Console.WriteLine("示例知识库:");
        var knowledge = new[]
        {
            "Semantic Kernel 是微软开发的 AI 编排框架",
            "Semantic Kernel 支持多种 AI 模型和服务",
            "Semantic Kernel 可以轻松集成到 .NET 应用中",
            "C# 是一种现代的面向对象编程语言",
            "C# 支持异步编程和 LINQ 查询"
        };

        foreach (var item in knowledge)
        {
            Console.WriteLine($"  • {item}");
        }

        Console.WriteLine("\n✅ 知识已准备好进行向量化和存储\n");
        await Task.CompletedTask;
    }

    /// <summary>
    /// 示例 3: 语义搜索（概念演示）
    /// </summary>
    static async Task Example3_SemanticSearch(Kernel kernel)
    {
        Console.WriteLine("【示例 3】语义搜索（概念演示）\n");

        Console.WriteLine("知识库内容:");
        var knowledge = new Dictionary<string, string>
        {
            ["1"] = "北京是中国的首都，有着悠久的历史",
            ["2"] = "上海是中国最大的城市，是重要的金融中心",
            ["3"] = "深圳是中国的科技创新中心，毗邻香港",
            ["4"] = "杭州以西湖闻名，是阿里巴巴的总部所在地"
        };

        foreach (var item in knowledge)
        {
            Console.WriteLine($"  [{item.Key}] {item.Value}");
        }

        Console.WriteLine("\n搜索查询: '中国的金融中心在哪里？'\n");

        Console.WriteLine("语义搜索流程:");
        Console.WriteLine("1. 将查询转换为向量");
        Console.WriteLine("2. 计算与知识库中每条记录的相似度");
        Console.WriteLine("3. 返回最相关的结果\n");

        Console.WriteLine("模拟搜索结果:");
        Console.WriteLine("结果 1:");
        Console.WriteLine($"  内容: {knowledge["2"]}");
        Console.WriteLine("  相关度: 0.8542\n");

        Console.WriteLine("结果 2:");
        Console.WriteLine($"  内容: {knowledge["1"]}");
        Console.WriteLine("  相关度: 0.7123\n");

        await Task.CompletedTask;
    }
}
