#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0070

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Common;
using Qdrant.Client;

namespace Concepts.RAG;
// 真实的 RAG (检索增强生成) 系统 + 语义缓存,使用 VectorStore 抽象层统一管理向量数据库
class Program
{
    private const string KnowledgeCollectionName = "knowledge_base";
    private const string CacheCollectionName = "semantic_cache";
    private const string QdrantEndpoint = "localhost";
    private const float CacheSimilarityThreshold = 0.95f;

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 真实的 RAG 系统 + 语义缓存 (使用 VectorStore 抽象层) ===\n");
        Console.WriteLine("📋 前置条件:");
        Console.WriteLine("请确保 Qdrant 向量数据库已启动:");
        Console.WriteLine("docker run -p 6333:6333 qdrant/qdrant\n");

        try
        {
            // 1. 创建Kernel和EmbeddingGenerator
            var kernel = Settings.CreateKernelBuilder().Build();
            // 【聊天服务】ChatCompletionService
            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            // 【向量服务】使用最新的 Microsoft.Extensions.AI API
            var embeddingGenerator = Settings.CreateEmbeddingGenerator();
            // 2. 连接 Qdrant 并创建 VectorStore
            Console.WriteLine("📡 连接 Qdrant 向量数据库...");
            var qdrantClient = new QdrantClient(QdrantEndpoint);
            var vectorStore = new QdrantVectorStore(qdrantClient, ownsClient: true);
            // 3. 获取集合 (使用 VectorStore 抽象)
            var knowledgeCollection = vectorStore.GetCollection<Guid, KnowledgeEntry>(KnowledgeCollectionName);
            var cacheCollection = vectorStore.GetCollection<Guid, CacheEntry>(CacheCollectionName);
            // 4. 初始化集合
            await knowledgeCollection.EnsureCollectionExistsAsync();
            await cacheCollection.EnsureCollectionExistsAsync();
            Console.WriteLine($"✅ 已连接到知识库集合: {KnowledgeCollectionName}");
            Console.WriteLine($"   ✅ 已连接到语义缓存集合: {CacheCollectionName}\n");

            // 创建缓存服务
            var cacheService = new SemanticCacheService(cacheCollection, embeddingGenerator);
            // ===== 示例 1: 构建知识库 =====
            await Example1_BuildKnowledgeBase(knowledgeCollection, embeddingGenerator);
            // ===== 示例 2: 语义搜索 =====
            await Example2_SemanticSearch(knowledgeCollection, embeddingGenerator);
            // ===== 示例 3: RAG 问答(不使用缓存) =====
            await Example3_RealRAG(knowledgeCollection, embeddingGenerator, chatService);
            // ===== 示例 4: RAG + 语义缓存 =====
            await Example4_RAGWithSemanticCache(knowledgeCollection, embeddingGenerator, chatService, cacheService);
            // ===== 示例 5: 缓存统计分析 =====
            await Example5_CacheAnalytics(cacheService);
            Console.WriteLine("\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
            if (ex.Message.Contains("Connection refused") || ex.Message.Contains("Unable to connect"))
            {
                Console.WriteLine("\n💡 提示: 请先启动 Qdrant:");
                Console.WriteLine("   docker run -p 6333:6333 qdrant/qdrant");
            }
            Console.WriteLine($"\n详细信息: {ex.StackTrace}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 示例 1: 构建知识库 (使用 VectorStore)
    /// </summary>
    static async Task Example1_BuildKnowledgeBase(
        VectorStoreCollection<Guid, KnowledgeEntry> collection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 1】构建知识库 (使用 VectorStore 抽象层)\n");
        // 企业产品知识库
        var knowledgeData = new Dictionary<string, string>
        {
            ["智能手表功能"] = "我们的智能手表支持心率监测、血氧检测、GPS定位、50米防水、NFC支付等功能。",
            ["电池续航"] = "智能手表在正常使用下电池续航可达7天,运动模式下约24小时,支持快速充电,充电30分钟可使用1天。",
            ["系统兼容性"] = "智能手表兼容 iOS 13.0 及以上和 Android 6.0 及以上系统,通过蓝牙 5.0 连接。",
            ["健康监测"] = "支持全天候心率监测、血氧饱和度测量、睡眠质量分析、压力监测和女性生理周期管理。",
            ["运动模式"] = "内置100+种运动模式,包括跑步、骑行、游泳、登山、瑜伽、球类运动等,并提供专业运动数据分析。",
            ["售后保修"] = "产品提供1年免费保修服务,支持7天无理由退换货,终身技术支持。非人为损坏免费维修。"
        };
        Console.WriteLine("正在生成向量并存储到 VectorStore...");
        var entries = new List<KnowledgeEntry>();
        foreach (var (category, text) in knowledgeData)
        {
            // 生成文本的向量嵌入
            var embeddingResult = await embeddingGenerator.GenerateAsync(text);
            // 创建 VectorStore 记录
            var entry = new KnowledgeEntry
            {
                Key = Guid.NewGuid(),
                Text = text,
                Category = category,
                Vector = embeddingResult.Vector
            };
            entries.Add(entry);
            Console.WriteLine($"   ✅ 已准备: {category}");
        }
        // 使用 VectorStore 统一接口批量存储
        await collection.UpsertAsync(entries);
        Console.WriteLine($"\n✅ 共存储 {knowledgeData.Count} 条知识到向量数据库\n");
    }

    /// <summary>
    /// 示例 2: 真实的语义搜索 (使用 VectorStore)
    /// </summary>
    static async Task Example2_SemanticSearch(
        VectorStoreCollection<Guid, KnowledgeEntry> collection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        Console.WriteLine("【示例 2】真实的语义搜索 (VectorStore)\n");
        string query = "这款手表能用多长时间?";
        Console.WriteLine($"🔍 用户查询: {query}\n");

        // 生成查询的向量
        var queryEmbeddingResult = await embeddingGenerator.GenerateAsync(query);
        // 使用 VectorStore 统一的向量搜索接口
        Console.WriteLine("正在向量数据库中搜索...");
        var searchResults = await collection.SearchAsync(
            queryEmbeddingResult.Vector,
            top: 3).ToListAsync();

        Console.WriteLine("\n📊 搜索结果:\n");
        int rank = 1;
        foreach (var result in searchResults)
        {
            Console.WriteLine($"结果 {rank}:");
            Console.WriteLine($"   类别: {result.Record.Category}");
            Console.WriteLine($"   内容: {result.Record.Text}");
            Console.WriteLine($"   相似度: {result.Score:F4}");
            Console.WriteLine();
            rank++;
        }
    }

    /// <summary>
    /// 示例 3: 真实的 RAG 问答系统 (使用 VectorStore)
    /// </summary>
    static async Task Example3_RealRAG(
        VectorStoreCollection<Guid, KnowledgeEntry> collection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IChatCompletionService chatService)
    {
        Console.WriteLine("【示例 3】真实的 RAG 问答系统 (VectorStore)\n");

        string question = "我想买一款运动手表,有什么功能推荐?";
        Console.WriteLine($"💬 用户提问: {question}\n");

        var startTime = DateTime.Now;

        // 使用 VectorStore 执行 RAG
        Console.WriteLine("步骤 1: 从知识库检索相关信息...");
        var answer = await ExecuteRAG(collection, embeddingGenerator, chatService, question);
        var duration = (DateTime.Now - startTime).TotalMilliseconds;
        Console.WriteLine("🤖 AI 回答:");
        Console.WriteLine($"{answer}");
        Console.WriteLine($"\n⏱️  总耗时: {duration:F0} ms\n");
    }
    /// <summary>
    /// 示例 4: RAG + 语义缓存 (使用 VectorStore)
    /// </summary>
    static async Task Example4_RAGWithSemanticCache(
        VectorStoreCollection<Guid, KnowledgeEntry> knowledgeCollection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IChatCompletionService chatService,
        SemanticCacheService cacheService)
    {
        Console.WriteLine("【示例 4】RAG + 语义缓存 (VectorStore)\n");

        var questions = new[]
        {
            "手表的电池能用多久?",                     // 第 1 次: Cache MISS
            "这个手表的续航时间怎么样?",               // 第 2 次: Cache HIT (语义相似)
            "电池续航表现如何?",                       // 第 3 次: Cache HIT
            "支持哪些运动?",                           // 第 4 次: Cache MISS
            "有哪些运动模式?",                         // 第 5 次: Cache HIT
        };

        foreach (var question in questions)
        {
            Console.WriteLine($"💬 用户提问: {question}");

            var startTime = DateTime.Now;

            // 尝试从缓存获取答案
            var cachedAnswer = await cacheService.TryGetCachedAnswer(question);

            string answer;
            if (cachedAnswer != null)
            {
                // 缓存命中
                answer = cachedAnswer.Answer;
                var duration = (DateTime.Now - startTime).TotalMilliseconds;

                Console.WriteLine($"🎯 缓存命中! (相似问题: \"{cachedAnswer.OriginalQuestion}\")");
                Console.WriteLine($"   相似度: {cachedAnswer.Similarity:F4}");
                Console.WriteLine($"🤖 AI 回答: {answer}");
                Console.WriteLine($"⏱️  响应时间: {duration:F0} ms (从缓存获取)\n");
                Console.WriteLine($"💰 成本节省: ~$0.002 (跳过 LLM 调用)\n");
            }
            else
            {
                // 缓存未命中,执行完整 RAG 流程
                Console.WriteLine($"❌ 缓存未命中,执行完整 RAG 流程...");

                answer = await ExecuteRAG(knowledgeCollection, embeddingGenerator, chatService, question);
                var duration = (DateTime.Now - startTime).TotalMilliseconds;

                Console.WriteLine($"🤖 AI 回答: {answer}");
                Console.WriteLine($"⏱️  响应时间: {duration:F0} ms (完整 RAG)\n");

                // 将结果存入缓存
                await cacheService.CacheAnswer(question, answer);
                Console.WriteLine($"✅ 已将答案存入语义缓存\n");
            }

            Console.WriteLine("─────────────────────────────────────\n");
        }
    }

    /// <summary>
    /// 示例 5: 缓存统计分析
    /// </summary>
    static async Task Example5_CacheAnalytics(SemanticCacheService cacheService)
    {
        Console.WriteLine("【示例 5】缓存统计分析\n");

        var stats = cacheService.GetCacheStatistics();

        Console.WriteLine("📊 缓存统计:");
        Console.WriteLine($"   - 总缓存条目数: {stats.TotalCacheEntries}");
        Console.WriteLine($"   - 缓存命中次数: {stats.CacheHits}");
        Console.WriteLine($"   - 缓存未命中次数: {stats.CacheMisses}");
        Console.WriteLine($"   - 命中率: {stats.HitRate:P2}");
        Console.WriteLine($"   - 节省成本: ~${stats.CostSaved:F4}");
        Console.WriteLine($"   - 平均缓存响应时间: {stats.AvgCacheResponseTime:F0} ms");
        Console.WriteLine($"   - 平均完整 RAG 响应时间: {stats.AvgFullRAGResponseTime:F0} ms");
        Console.WriteLine($"   - 性能提升: {stats.PerformanceImprovement:F1}x\n");

        await Task.CompletedTask;
    }

    /// <summary>
    /// 执行完整的 RAG 流程 (使用 VectorStore)
    /// </summary>
    static async Task<string> ExecuteRAG(
        VectorStoreCollection<Guid, KnowledgeEntry> collection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IChatCompletionService chatService,
        string question)
    {
        // 1. 生成查询向量并检索相关知识
        var queryEmbeddingResult = await embeddingGenerator.GenerateAsync(question);

        var searchResults = await collection.SearchAsync(
            queryEmbeddingResult.Vector,
            top: 3).ToListAsync();

        // 2. 收集检索结果
        var relevantTexts = searchResults.Select(r => r.Record.Text).ToList();

        // 3. 构建增强的提示词
        var context = string.Join("\n", relevantTexts.Select((t, i) => $"{i + 1}. {t}"));

        var history = new ChatHistory();
        history.AddSystemMessage(@"你是一位专业的智能手表产品顾问。
请根据提供的产品知识回答用户问题,回答要准确、专业、友好、简洁(控制在100字以内)。
如果知识库中没有相关信息,请诚实告知用户。");

        history.AddUserMessage($@"产品知识:
{context}

用户问题: {question}");

        // 4. 生成答案
        var answer = await chatService.GetChatMessageContentAsync(history);

        return answer.Content ?? "";
    }
}

// ==================== VectorStore 数据模型 ====================

/// <summary>
/// 知识库条目 (VectorStore 数据模型)
/// </summary>
public class KnowledgeEntry
{
    [VectorStoreKey]
    public Guid Key { get; set; }

    [VectorStoreData]
    public string Text { get; set; } = string.Empty;

    [VectorStoreData]
    public string Category { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

/// <summary>
/// 缓存条目 (VectorStore 数据模型)
/// </summary>
public class CacheEntry
{
    [VectorStoreKey]
    public Guid Key { get; set; }

    [VectorStoreData]
    public string Question { get; set; } = string.Empty;

    [VectorStoreData]
    public string Answer { get; set; } = string.Empty;

    [VectorStoreData]
    public string Timestamp { get; set; } = string.Empty;

    [VectorStoreVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> Vector { get; set; }
}

// ==================== 语义缓存服务 (使用 VectorStore) ====================

/// <summary>
/// 语义缓存服务 - 使用 VectorStore 抽象层
/// </summary>
public class SemanticCacheService
{
    private const float SimilarityThreshold = 0.95f;
    private const decimal CostPerRequest = 0.002m;
    private readonly VectorStoreCollection<Guid, CacheEntry> _cacheCollection;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    // 统计数据
    private int _cacheHits = 0;
    private int _cacheMisses = 0;
    private List<long> _cacheResponseTimes = new();
    private List<long> _fullRAGResponseTimes = new();
    public SemanticCacheService(
        VectorStoreCollection<Guid, CacheEntry> cacheCollection,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        _cacheCollection = cacheCollection;
        _embeddingGenerator = embeddingGenerator;
    }

    /// <summary>
    /// 尝试从缓存获取答案 (使用 VectorStore)
    /// </summary>
    public async Task<CachedAnswer?> TryGetCachedAnswer(string question)
    {
        var startTime = DateTime.Now;

        // 1. 生成问题的向量
        var questionEmbeddingResult = await _embeddingGenerator.GenerateAsync(question);

        // 2. 使用 VectorStore 搜索相似问题
        var searchResults = await _cacheCollection.SearchAsync(
            questionEmbeddingResult.Vector,
            top: 1).ToListAsync();

        var duration = (DateTime.Now - startTime).TotalMilliseconds;

        if (searchResults.Any())
        {
            var result = searchResults.First();

            // 检查相似度阈值
            if (result.Score >= SimilarityThreshold)
            {
                // 缓存命中
                _cacheHits++;
                _cacheResponseTimes.Add((long)duration);

                return new CachedAnswer
                {
                    OriginalQuestion = result.Record.Question,
                    Answer = result.Record.Answer,
                    Similarity = (float)result.Score,
                    CachedAt = DateTime.Parse(result.Record.Timestamp)
                };
            }
        }

        // 缓存未命中
        _cacheMisses++;
        return null;
    }

    /// <summary>
    /// 将问答对存入缓存 (使用 VectorStore)
    /// </summary>
    public async Task CacheAnswer(string question, string answer)
    {
        // 1. 生成问题的向量
        var questionEmbeddingResult = await _embeddingGenerator.GenerateAsync(question);

        // 2. 创建缓存条目
        var cacheEntry = new CacheEntry
        {
            Key = Guid.NewGuid(),
            Question = question,
            Answer = answer,
            Timestamp = DateTime.UtcNow.ToString("O"),
            Vector = questionEmbeddingResult.Vector
        };

        // 3. 使用 VectorStore 统一接口存储
        await _cacheCollection.UpsertAsync([cacheEntry]);
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    public CacheStatistics GetCacheStatistics()
    {
        var totalRequests = _cacheHits + _cacheMisses;
        var hitRate = totalRequests > 0 ? (double)_cacheHits / totalRequests : 0;

        var avgCacheTime = _cacheResponseTimes.Any() ? _cacheResponseTimes.Average() : 0;
        var avgFullRAGTime = _fullRAGResponseTimes.Any() ? _fullRAGResponseTimes.Average() : 2000;

        var performanceImprovement = avgCacheTime > 0 ? avgFullRAGTime / avgCacheTime : 0;

        return new CacheStatistics
        {
            TotalCacheEntries = _cacheHits,
            CacheHits = _cacheHits,
            CacheMisses = _cacheMisses,
            HitRate = hitRate,
            CostSaved = _cacheHits * CostPerRequest,
            AvgCacheResponseTime = avgCacheTime,
            AvgFullRAGResponseTime = avgFullRAGTime,
            PerformanceImprovement = performanceImprovement
        };
    }
}

// ==================== 数据模型 ====================

public record CachedAnswer
{
    public required string OriginalQuestion { get; init; }
    public required string Answer { get; init; }
    public float Similarity { get; init; }
    public DateTime CachedAt { get; init; }
}

public record CacheStatistics
{
    public int TotalCacheEntries { get; init; }
    public int CacheHits { get; init; }
    public int CacheMisses { get; init; }
    public double HitRate { get; init; }
    public decimal CostSaved { get; init; }
    public double AvgCacheResponseTime { get; init; }
    public double AvgFullRAGResponseTime { get; init; }
    public double PerformanceImprovement { get; init; }
}
