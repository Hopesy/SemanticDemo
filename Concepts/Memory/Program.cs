#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Common;

namespace Concepts.Memory;

/// <summary>
/// 记忆系统核心概念 - 使用 InMemory VectorStore 实现真实的向量存储
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 记忆系统核心概念 (InMemory VectorStore) ===\n");

        try
        {
            // 创建 Kernel（用于 Chat 服务）
            var kernel = Settings.CreateKernelBuilder().Build();

            // 创建 IEmbeddingGenerator（使用最新的 Microsoft.Extensions.AI API）
            var embeddingGenerator = Settings.CreateEmbeddingGenerator();

            // 创建 InMemory VectorStore
            var vectorStore = new InMemoryVectorStore();

            // ===== 示例 1: 文本嵌入生成 =====
            await Example1_TextEmbedding(embeddingGenerator);

            // ===== 示例 2: 真实的语义记忆存储 =====
            await Example2_RealSemanticMemory(vectorStore, embeddingGenerator);

            // ===== 示例 3: 真实的语义搜索 =====
            await Example3_RealSemanticSearch(vectorStore, embeddingGenerator);

            // ===== 示例 4: 多类别知识检索 =====
            await Example4_MultiCategoryRetrieval(vectorStore, embeddingGenerator);

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
    static async Task Example1_TextEmbedding(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 1】文本嵌入生成\n");

        // 生成文本的向量表示
        var text = "Semantic Kernel 是一个强大的 AI 编排框架";
        var embeddingResult = await embeddingGenerator.GenerateAsync(text);
        var embedding = embeddingResult.Vector;

        Console.WriteLine($"文本: {text}");
        Console.WriteLine($"嵌入维度: {embedding.Length}");
        Console.WriteLine($"前 5 个值: [{string.Join(", ", embedding.Span.Slice(0, Math.Min(5, embedding.Length)).ToArray().Select(v => v.ToString("F4")))}...]\n");
    }

    /// <summary>
    /// 示例 2: 真实的语义记忆存储 (使用 InMemory VectorStore)
    /// </summary>
    static async Task Example2_RealSemanticMemory(
        InMemoryVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 2】真实的语义记忆存储 (InMemory VectorStore)\n");

        // 获取或创建知识库集合
        var collection = vectorStore.GetCollection<string, KnowledgeItem>("tech_knowledge");
        await collection.EnsureCollectionExistsAsync();

        // 准备知识库数据
        var knowledgeData = new[]
        {
            ("SK001", "Semantic Kernel 是微软开发的 AI 编排框架", "Semantic Kernel"),
            ("SK002", "Semantic Kernel 支持多种 AI 模型和服务", "Semantic Kernel"),
            ("SK003", "Semantic Kernel 可以轻松集成到 .NET 应用中", "Semantic Kernel"),
            ("CS001", "C# 是一种现代的面向对象编程语言", "C#"),
            ("CS002", "C# 支持异步编程和 LINQ 查询", "C#")
        };

        Console.WriteLine("正在将知识存储到 InMemory VectorStore...\n");

        var items = new List<KnowledgeItem>();
        foreach (var (id, text, category) in knowledgeData)
        {
            // 生成文本的向量嵌入
            var embeddingResult = await embeddingGenerator.GenerateAsync(text);

            var item = new KnowledgeItem
            {
                Key = id,
                Text = text,
                Category = category,
                Vector = embeddingResult.Vector
            };

            items.Add(item);
            Console.WriteLine($"  ✅ 已准备: [{id}] {text}");
        }

        // 批量存储到 InMemory VectorStore
        await collection.UpsertAsync(items);

        Console.WriteLine($"\n✅ 共存储 {knowledgeData.Length} 条知识到内存向量数据库\n");
    }

    /// <summary>
    /// 示例 3: 真实的语义搜索 (使用 InMemory VectorStore)
    /// </summary>
    static async Task Example3_RealSemanticSearch(
        InMemoryVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 3】真实的语义搜索 (InMemory VectorStore)\n");

        // 获取城市知识库集合
        var collection = vectorStore.GetCollection<string, CityInfo>("city_knowledge");
        await collection.EnsureCollectionExistsAsync();

        // 准备城市知识数据
        var cityData = new[]
        {
            ("BJ", "北京是中国的首都，有着悠久的历史", "北京"),
            ("SH", "上海是中国最大的城市，是重要的金融中心", "上海"),
            ("SZ", "深圳是中国的科技创新中心，毗邻香港", "深圳"),
            ("HZ", "杭州以西湖闻名，是阿里巴巴的总部所在地", "杭州")
        };

        Console.WriteLine("正在构建城市知识库...\n");

        var cities = new List<CityInfo>();
        foreach (var (id, description, name) in cityData)
        {
            var embeddingResult = await embeddingGenerator.GenerateAsync(description);

            var city = new CityInfo
            {
                Key = id,
                Name = name,
                Description = description,
                Vector = embeddingResult.Vector
            };

            cities.Add(city);
            Console.WriteLine($"  [{id}] {name}: {description}");
        }

        await collection.UpsertAsync(cities);
        Console.WriteLine("\n✅ 城市知识库已构建完成\n");

        // 执行语义搜索
        var query = "中国的金融中心在哪里？";
        Console.WriteLine($"🔍 用户查询: {query}\n");

        // 生成查询向量
        var queryEmbeddingResult = await embeddingGenerator.GenerateAsync(query);

        // 使用 VectorStore 进行向量搜索
        Console.WriteLine("正在执行语义搜索...\n");
        var searchResults = await collection.SearchAsync(
            queryEmbeddingResult.Vector,
            top: 3).ToListAsync();

        Console.WriteLine("📊 搜索结果:\n");
        int rank = 1;
        foreach (var result in searchResults)
        {
            Console.WriteLine($"结果 {rank}:");
            Console.WriteLine($"  城市: {result.Record.Name}");
            Console.WriteLine($"  描述: {result.Record.Description}");
            Console.WriteLine($"  相似度: {result.Score:F4}");
            Console.WriteLine();
            rank++;
        }
    }

    /// <summary>
    /// 示例 4: 多类别知识检索
    /// </summary>
    static async Task Example4_MultiCategoryRetrieval(
        InMemoryVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 4】多类别知识检索\n");

        // 获取技术知识库集合（之前在示例2中创建的）
        var collection = vectorStore.GetCollection<string, KnowledgeItem>("tech_knowledge");

        // 测试不同的查询
        var queries = new[]
        {
            "什么是 AI 框架？",
            "C# 有什么特性？",
            "如何在 .NET 中使用 AI？"
        };

        foreach (var query in queries)
        {
            Console.WriteLine($"🔍 查询: {query}");

            // 生成查询向量
            var queryEmbeddingResult = await embeddingGenerator.GenerateAsync(query);

            // 执行向量搜索
            var results = await collection.SearchAsync(
                queryEmbeddingResult.Vector,
                top: 2).ToListAsync();

            Console.WriteLine("   最相关的知识:\n");
            foreach (var result in results)
            {
                Console.WriteLine($"   [{result.Record.Key}] {result.Record.Category}");
                Console.WriteLine($"   {result.Record.Text}");
                Console.WriteLine($"   相似度: {result.Score:F4}\n");
            }

            Console.WriteLine("─────────────────────────────────────\n");
        }
    }
}

// ==================== VectorStore 数据模型 ====================

/// <summary>
/// 知识条目 (VectorStore 数据模型)
/// </summary>
public class KnowledgeItem
{
    [VectorStoreKey]
    public string Key { get; set; } = string.Empty;

    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    [VectorStoreData]
    public string Category { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

/// <summary>
/// 城市信息 (VectorStore 数据模型)
/// </summary>
public class CityInfo
{
    [VectorStoreKey]
    public string Key { get; set; } = string.Empty;

    [VectorStoreData]
    public string Name { get; set; } = string.Empty;

    [VectorStoreData]
    public string Description { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
