using Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using System.Text.RegularExpressions;
/*
  1. 使用 LLM 智能提取关键词（提高准确率）
  2. 启用元数据过滤（如权限、分类、时间范围）
  3. 实现分页（避免一次返回大量结果）
  4. 缓存常见查询的关键词（减少 LLM 调用）
  5. 监控 RRF 评分分布（优化权重配置）
*/

Console.WriteLine("=== Semantic Kernel 混合检索（Hybrid Search）综合示例 ===\n");

// 创建 Kernel（用于 LLM 关键词提取）
var kernel = Settings.CreateKernelBuilder().Build();
// 创建 Embedding 生成器
var embeddingGenerator = Settings.CreateEmbeddingGenerator();
// 创建 Qdrant 客户端（连接到本地 Qdrant 服务器）
// 先启动 docker 容器：docker run -d -p 6333:6333 -p 6334:6334 --name qdrant-hybridsearch qdrant/qdrant
var qdrantClient = new QdrantClient("localhost", 6334, https: false);
var vectorStore = new QdrantVectorStore(qdrantClient, ownsClient: false);
// 初始化知识库数据
await InitializeKnowledgeBase(vectorStore, embeddingGenerator);
// 运行综合示例
await ComprehensiveHybridSearchExample(kernel, vectorStore, embeddingGenerator);
Console.WriteLine("\n按任意键退出...");
Console.ReadKey();
/// 初始化知识库数据
static async Task InitializeKnowledgeBase(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("正在初始化知识库数据...\n");
    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    await collection.EnsureCollectionExistsAsync();
    // 技术文档数据集
    var documents = new[]
    {
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Semantic Kernel 简介",
            Content = "Semantic Kernel 是微软开发的一个开源 SDK，用于将大型语言模型（LLM）集成到应用程序中。它提供了插件系统、内存管理、规划器等核心功能。",
            Category = "框架",
            Tags = ["AI", "SDK", "Microsoft"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "向量数据库概述",
            Content = "向量数据库是专门用于存储和检索向量嵌入（embeddings）的数据库系统。它支持高效的相似度搜索，广泛应用于语义搜索、推荐系统和 RAG 应用。",
            Category = "数据库",
            Tags = ["向量数据库", "Embeddings", "搜索"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "混合检索技术",
            Content = "混合检索（Hybrid Search）结合了向量搜索和关键词搜索的优势。向量搜索擅长语义理解，关键词搜索擅长精确匹配。通过 RRF 算法融合两者的结果。",
            Category = "搜索",
            Tags = ["Hybrid Search", "RRF", "检索"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "RAG 架构设计",
            Content = "检索增强生成（RAG）是一种将外部知识库与大型语言模型结合的架构模式。它通过检索相关文档来增强 LLM 的生成能力，减少幻觉问题。",
            Category = "架构",
            Tags = ["RAG", "LLM", "知识库"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "BM25 算法原理",
            Content = "BM25 是一种经典的文本相关性评分算法，基于词频（TF）和逆文档频率（IDF）。它在全文搜索引擎中广泛使用，例如 Elasticsearch 的默认评分算法。",
            Category = "算法",
            Tags = ["BM25", "TF-IDF", "全文搜索"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Reciprocal Rank Fusion 详解",
            Content = "RRF（倒数排名融合）是一种用于合并多个排序列表的算法。公式为 score = 1/(k+rank)，其中 k 通常取 60。它不依赖绝对评分值，仅依赖排名。",
            Category = "算法",
            Tags = ["RRF", "排序融合", "重排序"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Azure AI Search 服务",
            Content = "Azure AI Search 是微软提供的云搜索服务，支持全文搜索、向量搜索和混合检索。它内置了语义排序和 AI 增强功能。",
            Category = "服务",
            Tags = ["Azure", "搜索服务", "云服务"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Embedding 模型选择",
            Content = "Embedding 模型将文本转换为向量表示。常见的模型包括 OpenAI text-embedding-3-small、sentence-transformers、BGE 等。选择时需考虑维度、性能和成本。",
            Category = "模型",
            Tags = ["Embedding", "模型", "向量化"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Prompt Engineering 最佳实践",
            Content = "提示工程是优化 LLM 输出的关键技术。包括明确指令、提供示例、设置角色、使用思维链等技巧。好的提示可以显著提升输出质量。",
            Category = "技术",
            Tags = ["Prompt", "LLM", "优化"]
        },
        new TechDocument
        {
            Id = Guid.NewGuid(),
            Title = "Function Calling 机制",
            Content = "Function Calling 允许 LLM 调用外部函数或 API。模型会识别何时需要调用函数，并生成结构化的参数。这是构建 AI Agent 的核心能力。",
            Category = "技术",
            Tags = ["Function Calling", "Agent", "API"]
        }
    };

    // 为每个文档生成向量并插入
    foreach (var doc in documents)
    {
        var embedding = await embeddingGenerator.GenerateAsync(doc.Content);
        doc.Vector = embedding.Vector;
        await collection.UpsertAsync(doc);
    }

    Console.WriteLine($"✅ 已加载 {documents.Length} 个文档到知识库\n");
}

/// 综合混合检索示例 - 展示所有核心功能
static async Task ComprehensiveHybridSearchExample(Kernel kernel, QdrantVectorStore vectorStore, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("混合检索（Hybrid Search）综合演示");
    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;
    // 用户查询
    string query = "如何实现高效的搜索功能";
    Console.WriteLine($"🔍 用户查询: {query}\n");
    // 生成查询向量
    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);
    // ========== 1. 智能关键词提取 ==========
    Console.WriteLine("【步骤 1】智能关键词提取");
    var keywords = await ExtractKeywordsWithLLM(kernel, query);
    Console.WriteLine($"✓AI提取关键词: {string.Join(", ", keywords)}\n");
    // ========== 2. 对比三种搜索方式 ==========
    Console.WriteLine("【步骤 2】对比三种搜索方式\n");
    // 2.1 纯向量搜索
    Console.WriteLine("方式1：纯向量搜索（仅语义）");
    var vectorResults = await collection.SearchAsync(queryEmbedding.Vector, top: 3).ToListAsync();
    DisplayResults(vectorResults);

    // 2.2 纯关键词搜索（模拟全文搜索）
    Console.WriteLine("方式2：纯关键词搜索（仅精确匹配）");
    var keywordResults = await hybridSearchCollection.HybridSearchAsync(
        new ReadOnlyMemory<float>(new float[768]), // 空向量
        keywords: keywords,
        top: 3
    ).ToListAsync();
    DisplayResults(keywordResults);
    // 2.3 混合检索（推荐）
    //调用HybridSearchAsync并传入向量+关键词会自动应用RRF重排序
    //
    Console.WriteLine("方式3：混合检索（语义 + 精确匹配）⭐ 推荐");
    var hybridResults = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: keywords,
        top: 3
    ).ToListAsync();
    DisplayResults(hybridResults);
    Console.WriteLine("💡结论: 混合检索结合了语义理解和精确匹配，效果最好！\n");
    // ========== 3. 高级特性演示 ==========
    Console.WriteLine("【步骤 3】高级特性\n");
    // 3.1 带过滤器的混合检索
    Console.WriteLine("特性1-元数据过滤（只搜索'算法'分类）");
    var filteredResults = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "搜索", "算法" },
        top: 5,
        new HybridSearchOptions<TechDocument>
        {
            Filter = doc => doc.Category == "算法"
        }
    ).ToListAsync();
    Console.WriteLine($"✓找到 {filteredResults.Count} 个结果（仅'算法'分类）");
    foreach (var result in filteredResults)
    {
        Console.WriteLine($"  - {result.Record.Title} (分类: {result.Record.Category})");
    }
    // 3.2 分页支持
    Console.WriteLine("\n特性2-分页支持（Skip + Top）");
    var page1 = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: keywords,
        top: 2,
        new HybridSearchOptions<TechDocument> { Skip = 0, }
    ).ToListAsync();
    Console.WriteLine($"      ✓ 第 1 页（Top=2, Skip=0）: {page1[0].Record.Title}, {page1[1].Record.Title}");

    var page2 = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: keywords,
        top: 2,
        new HybridSearchOptions<TechDocument> { Skip = 2 }
    ).ToListAsync();
    Console.WriteLine($"      ✓ 第 2 页（Top=2, Skip=2）: {page2[0].Record.Title}, {page2[1].Record.Title}");

    // ========== 4. RRF 算法原理 ==========
    Console.WriteLine("\n【步骤 4】RRF 重排序算法原理\n");
    Console.WriteLine("   📐 公式: RRF_score = Σ weight / (k + rank)");
    Console.WriteLine("      - k: 常量（通常为 60）");
    Console.WriteLine("      - rank: 文档在各检索系统中的排名");
    Console.WriteLine("      - weight: 各检索系统的权重（如 0.1 向量 + 0.9 全文）\n");

    Console.WriteLine("   💡 RRF 优势:");
    Console.WriteLine("      ✓ 排名归一化: 不依赖绝对评分值");
    Console.WriteLine("      ✓ 跨尺度融合: 向量相似度和 BM25 评分可公平合并");
    Console.WriteLine("      ✓ 鲁棒性强: 对单一检索系统的异常值不敏感");
    Console.WriteLine("      ✓ 工业标准: Cosmos DB、Qdrant、Weaviate 等原生支持\n");

    // ========== 5. 最佳实践 ==========
    Console.WriteLine("【步骤 5】生产环境最佳实践\n");
    Console.WriteLine("   ✅ 推荐配置:");
    Console.WriteLine("      1. 使用 LLM 智能提取关键词（提高准确率）");
    Console.WriteLine("      2. 启用元数据过滤（如权限、分类、时间范围）");
    Console.WriteLine("      3. 实现分页（避免一次返回大量结果）");
    Console.WriteLine("      4. 缓存常见查询的关键词（减少 LLM 调用）");
    Console.WriteLine("      5. 监控 RRF 评分分布（优化权重配置）\n");

    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("✅ 混合检索综合演示完成！");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");
}

/// <summary>
/// 显示搜索结果的辅助方法
/// </summary>
static void DisplayResults<T>(List<VectorSearchResult<T>> results, string indent = "") where T : TechDocument
{
    for (int i = 0; i < results.Count; i++)
    {
        Console.WriteLine($"{indent}{i + 1}. {results[i].Record.Title} (评分: {results[i].Score:F4})");
    }
    Console.WriteLine();
}

#region 关键词提取辅助方法

/// <summary>
/// 使用 LLM 从查询中智能提取关键词
/// </summary>
static async Task<string[]> ExtractKeywordsWithLLM(Kernel kernel, string query)
{
    var prompt = $@"
从以下查询中提取 2-3 个最重要的关键词，用于全文搜索。
只返回关键词，用逗号分隔，不要有其他内容。

示例：
查询：如何实现高效的搜索功能
关键词：搜索,检索,高效

查询：{query}
关键词：";

    try
    {
        var result = await kernel.InvokePromptAsync(prompt);
        var keywords = result.ToString()
            .Split(new[] { ',', '，', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Trim())
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Take(3)
            .ToArray();

        return keywords.Length > 0 ? keywords : new[] { query };
    }
    catch
    {
        // 如果 LLM 调用失败，回退到简单提取
        return ExtractChineseKeywords(query);
    }
}

/// <summary>
/// 简单的中文关键词提取（基于正则表达式和停用词过滤）
/// </summary>
static string[] ExtractChineseKeywords(string query)
{
    var stopWords = new HashSet<string>
    {
        "的", "了", "在", "是", "我", "有", "和", "就", "不", "人", "都", "一",
        "一个", "上", "也", "很", "到", "说", "要", "去", "你", "会", "着", "没有",
        "看", "好", "自己", "这", "那", "如何", "怎么", "什么", "哪些", "为什么",
        "能", "可以", "或者", "但是", "然而", "因为", "所以", "实现", "功能"
    };

    var pattern = @"[\u4e00-\u9fa5]{2,4}";
    var matches = Regex.Matches(query, pattern);

    var keywords = matches
        .Select(m => m.Value)
        .Where(w => !stopWords.Contains(w))
        .Distinct()
        .Take(3)
        .ToArray();

    return keywords.Length > 0 ? keywords : new[] { query };
}

#endregion

/// <summary>
/// 技术文档数据模型
/// </summary>
public class TechDocument
{
    [VectorStoreKey]
    public required Guid Id { get; set; }

    [VectorStoreData]
    public required string Title { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]  // 标记为全文搜索字段
    public required string Content { get; set; }

    [VectorStoreData(IsIndexed = true)]  // 标记为可过滤字段
    public required string Category { get; set; }

    [VectorStoreData]
    public required List<string> Tags { get; set; }

    [VectorStoreVector(768)]  // 768 维向量（nomic-embed-text 模型）
    public ReadOnlyMemory<float>? Vector { get; set; }
}
