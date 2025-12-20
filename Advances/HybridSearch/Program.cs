using Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0070

Console.WriteLine("=== Semantic Kernel 混合检索（Hybrid Search）与 RRF 重排序示例 ===\n");

// 创建Embedding生成器(本案例使用的是ollama本地运行的嵌入模型nomic-embed-text)
// 确保已安装ollama并下载nomic-embed-text模型,并且ollama客户端运行中
var embeddingGenerator = Settings.CreateEmbeddingGenerator();

// 创建Qdrant客户端（连接到本地Qdrant服务器）先启动docker容器
// docker run -d -p 6333:6333 -p 6334:6334 --name qdrant-hybridsearch qdrant/qdrant
var qdrantClient = new QdrantClient("localhost", 6334, https: false);
// 创建 Qdrant 向量存储
var vectorStore = new QdrantVectorStore(qdrantClient, ownsClient: false);
// 初始化知识库数据
await InitializeKnowledgeBase(vectorStore, embeddingGenerator);
// 示例 1: 基础混合检索
await Example1_BasicHybridSearch(vectorStore, embeddingGenerator);
// 示例 2: 对比纯向量搜索 vs 混合检索
await Example2_VectorVsHybridSearch(vectorStore, embeddingGenerator);
// 示例 3: 带过滤器的混合检索
await Example3_HybridSearchWithFilter(vectorStore, embeddingGenerator);
// 示例 4: 分页支持
await Example4_HybridSearchPaging(vectorStore, embeddingGenerator);
// 示例 5: RRF 评分解析
await Example5_UnderstandingRRFScore(vectorStore, embeddingGenerator);
Console.WriteLine("\n按任意键退出...");
Console.ReadKey();
/// <summary>
/// 初始化知识库数据
/// </summary>
static async Task InitializeKnowledgeBase(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("📚 正在初始化知识库数据...\n");

    // 获取集合（如果不存在会自动创建）
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

/// <summary>
/// 示例 1: 基础混合检索
/// </summary>
static async Task Example1_BasicHybridSearch(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("示例 1: 基础混合检索（向量搜索 + 关键词搜索）");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");

    // 转换为混合检索接口
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;

    // 查询：搜索关于"搜索"的技术
    string query = "如何实现高效的搜索功能";
    Console.WriteLine($"🔍 查询: {query}");

    // 生成查询向量
    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

    // 执行混合检索：结合向量搜索和关键词"搜索"、"检索"
    var results = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "搜索", "检索" },
        top: 3
    ).ToListAsync();

    Console.WriteLine($"\n📊 混合检索结果（Top 3）:\n");

    int index = 1;
    foreach (var result in results)
    {
        Console.WriteLine($"  {index}. {result.Record.Title}");
        Console.WriteLine($"     分类: {result.Record.Category}");
        Console.WriteLine($"     RRF 评分: {result.Score:F4}");
        Console.WriteLine($"     标签: {string.Join(", ", result.Record.Tags)}");
        Console.WriteLine($"     摘要: {result.Record.Content[..Math.Min(50, result.Record.Content.Length)]}...");
        Console.WriteLine();
        index++;
    }
}

/// <summary>
/// 示例 2: 对比纯向量搜索 vs 混合检索
/// </summary>
static async Task Example2_VectorVsHybridSearch(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("示例 2: 对比纯向量搜索 vs 混合检索");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;

    // 查询：包含特定术语"RRF"
    string query = "RRF 算法的工作原理";
    Console.WriteLine($"🔍 查询: {query}");
    Console.WriteLine("   特点: 包含专有术语 'RRF'\n");

    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

    // 1. 纯向量搜索（仅语义相似度）
    Console.WriteLine("📍 方法 1: 纯向量搜索（语义匹配）");
    var vectorResults = await collection.SearchAsync(
        queryEmbedding.Vector,
        top: 3
    ).ToListAsync();

    Console.WriteLine("   结果:");
    int vectorIndex = 1;
    foreach (var result in vectorResults)
    {
        Console.WriteLine($"   {vectorIndex}. {result.Record.Title} (评分: {result.Score:F4})");
        vectorIndex++;
    }

    // 2. 混合检索（语义 + 关键词"RRF"）
    Console.WriteLine("\n📍 方法 2: 混合检索（语义 + 关键词 'RRF'）");
    var hybridResults = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "RRF" },
        top: 3
    ).ToListAsync();

    Console.WriteLine("   结果:");
    int hybridIndex = 1;
    foreach (var result in hybridResults)
    {
        Console.WriteLine($"   {hybridIndex}. {result.Record.Title} (评分: {result.Score:F4})");
        hybridIndex++;
    }

    Console.WriteLine("\n💡 对比分析:");
    Console.WriteLine("   - 纯向量搜索: 依赖语义理解，可能忽略精确关键词");
    Console.WriteLine("   - 混合检索: 结合语义和关键词，更准确地找到包含 'RRF' 的文档");
    Console.WriteLine();
}

/// <summary>
/// 示例 3: 带过滤器的混合检索
/// </summary>
static async Task Example3_HybridSearchWithFilter(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("示例 3: 带过滤器的混合检索");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;

    string query = "搜索技术";
    Console.WriteLine($"🔍 查询: {query}");
    Console.WriteLine($"🔧 过滤器: Category == '算法'\n");

    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

    // 使用过滤器限制只搜索"算法"分类的文档
    var results = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "搜索", "算法" },
        top: 5,
        new HybridSearchOptions<TechDocument>
        {
            Filter = doc => doc.Category == "算法"  // 预过滤
        }
    ).ToListAsync();

    Console.WriteLine($"📊 过滤后的混合检索结果:\n");

    int index = 1;
    foreach (var result in results)
    {
        Console.WriteLine($"  {index}. {result.Record.Title}");
        Console.WriteLine($"     分类: {result.Record.Category} ✓");
        Console.WriteLine($"     RRF 评分: {result.Score:F4}");
        Console.WriteLine();
        index++;
    }

    Console.WriteLine("💡 过滤器优势:");
    Console.WriteLine("   - 减少搜索空间，提高性能");
    Console.WriteLine("   - 确保结果符合业务规则（如权限、分类等）");
    Console.WriteLine("   - 过滤在检索前执行，不影响评分");
    Console.WriteLine();
}

/// <summary>
/// 示例 4: 分页支持
/// </summary>
static async Task Example4_HybridSearchPaging(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("示例 4: 分页支持");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;

    string query = "AI 技术";
    Console.WriteLine($"🔍 查询: {query}");
    Console.WriteLine($"📄 分页设置: Top=3, Skip=0/3/6\n");

    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

    // 第一页
    Console.WriteLine("📖 第 1 页 (Skip=0, Top=3):");
    var page1 = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "AI", "技术" },
        top: 3,
        new HybridSearchOptions<TechDocument> { Skip = 0 }
    ).ToListAsync();

    int page1Index = 1;
    foreach (var result in page1)
    {
        Console.WriteLine($"   {page1Index}. {result.Record.Title} (评分: {result.Score:F4})");
        page1Index++;
    }

    // 第二页
    Console.WriteLine("\n📖 第 2 页 (Skip=3, Top=3):");
    var page2 = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "AI", "技术" },
        top: 3,
        new HybridSearchOptions<TechDocument> { Skip = 3 }
    ).ToListAsync();

    int page2Index = 4;
    foreach (var result in page2)
    {
        Console.WriteLine($"   {page2Index}. {result.Record.Title} (评分: {result.Score:F4})");
        page2Index++;
    }

    // 第三页
    Console.WriteLine("\n📖 第 3 页 (Skip=6, Top=3):");
    var page3 = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "AI", "技术" },
        top: 3,
        new HybridSearchOptions<TechDocument> { Skip = 6 }
    ).ToListAsync();

    int page3Index = 7;
    foreach (var result in page3)
    {
        Console.WriteLine($"   {page3Index}. {result.Record.Title} (评分: {result.Score:F4})");
        page3Index++;
    }

    Console.WriteLine("\n💡 分页实现:");
    Console.WriteLine("   - Skip: 跳过前 N 个结果");
    Console.WriteLine("   - Top: 返回的结果数量");
    Console.WriteLine("   - 评分排序在分页前完成，确保一致性");
    Console.WriteLine();
}

/// <summary>
/// 示例 5: RRF 评分解析
/// </summary>
static async Task Example5_UnderstandingRRFScore(
    QdrantVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
{
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine("示例 5: RRF（Reciprocal Rank Fusion）评分解析");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

    var collection = vectorStore.GetCollection<Guid, TechDocument>("tech_docs");
    var hybridSearchCollection = (IKeywordHybridSearchable<TechDocument>)collection;

    string query = "混合检索";
    Console.WriteLine($"🔍 查询: {query}\n");

    var queryEmbedding = await embeddingGenerator.GenerateAsync(query);

    // 1. 纯向量搜索
    Console.WriteLine("📍 第 1 步: 纯向量搜索排名");
    var vectorResults = await collection.SearchAsync(
        queryEmbedding.Vector,
        top: 5
    ).ToListAsync();

    int vectorRank = 1;
    foreach (var result in vectorResults)
    {
        Console.WriteLine($"   排名 {vectorRank}: {result.Record.Title,-35} (向量评分: {result.Score:F4})");
        vectorRank++;
    }

    // 2. 混合检索
    Console.WriteLine("\n📍 第 2 步: 混合检索（向量 + 关键词 '混合'、'检索'）");
    var hybridResults = await hybridSearchCollection.HybridSearchAsync(
        queryEmbedding.Vector,
        keywords: new[] { "混合", "检索" },
        top: 5
    ).ToListAsync();

    int hybridRank = 1;
    foreach (var result in hybridResults)
    {
        Console.WriteLine($"   排名 {hybridRank}: {result.Record.Title,-35} (RRF 评分: {result.Score:F4})");
        hybridRank++;
    }

    Console.WriteLine("\n📐 RRF 算法原理:");
    Console.WriteLine("   公式: RRF_score = Σ weight / (k + rank)");
    Console.WriteLine("   - k: 常量，通常为 60");
    Console.WriteLine("   - rank: 文档在各检索系统中的排名");
    Console.WriteLine("   - weight: 各检索系统的权重（如 0.1 向量 + 0.9 全文）");
    Console.WriteLine();

    Console.WriteLine("📊 评分融合过程（假设）:");
    Console.WriteLine("   文档 A:");
    Console.WriteLine("     - 向量搜索排名 1: 0.1 / (60 + 1) ≈ 0.0016");
    Console.WriteLine("     - 关键词搜索排名 1: 0.9 / (60 + 1) ≈ 0.0148");
    Console.WriteLine("     - RRF 总分: 0.0164");
    Console.WriteLine();
    Console.WriteLine("   文档 B:");
    Console.WriteLine("     - 向量搜索排名 5: 0.1 / (60 + 5) ≈ 0.0015");
    Console.WriteLine("     - 关键词搜索未出现: 0");
    Console.WriteLine("     - RRF 总分: 0.0015");
    Console.WriteLine();

    Console.WriteLine("💡 RRF 优势:");
    Console.WriteLine("   ✓ 排名归一化: 不依赖绝对评分值");
    Console.WriteLine("   ✓ 跨尺度融合: 向量相似度和 BM25 评分可公平合并");
    Console.WriteLine("   ✓ 鲁棒性强: 对单一检索系统的异常值不敏感");
    Console.WriteLine("   ✓ 工业标准: Cosmos DB、Qdrant、Weaviate 等原生支持");
    Console.WriteLine();
}

/// <summary>
/// 技术文档数据模型
/// </summary>
public class TechDocument
{
    /// <summary>
    /// 文档 ID
    /// </summary>
    [VectorStoreKey]
    public required Guid Id { get; set; }

    /// <summary>
    /// 文档标题
    /// </summary>
    [VectorStoreData]
    public required string Title { get; set; }

    /// <summary>
    /// 文档内容（用于全文搜索和向量化）
    /// </summary>
    [VectorStoreData(IsFullTextIndexed = true)]  // 标记为全文搜索字段
    public required string Content { get; set; }

    /// <summary>
    /// 分类（用于过滤）
    /// </summary>
    [VectorStoreData(IsIndexed = true)]  // 标记为可过滤字段
    public required string Category { get; set; }

    /// <summary>
    /// 标签列表
    /// </summary>
    [VectorStoreData]
    public required List<string> Tags { get; set; }

    /// <summary>
    /// 向量嵌入（用于向量搜索）
    /// </summary>
    [VectorStoreVector(768)]  // 768 维向量（nomic-embed-text 模型）
    public ReadOnlyMemory<float>? Vector { get; set; }
}
