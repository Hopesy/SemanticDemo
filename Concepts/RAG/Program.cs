#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0070

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Common;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text.Json;

namespace Concepts.RAG;

/// <summary>
/// 真实的 RAG (检索增强生成) 系统 + 语义缓存
/// 使用 Qdrant 向量数据库进行语义搜索和智能缓存
/// </summary>
class Program
{
    private const string CollectionName = "knowledge_base";
    private const string CacheCollectionName = "semantic_cache"; // 语义缓存集合
    private const string QdrantEndpoint = "http://localhost:6333";
    private const float CacheSimilarityThreshold = 0.95f; // 缓存命中阈值

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 真实的 RAG 系统 + 语义缓存 (使用 Qdrant 向量数据库) ===\n");
        Console.WriteLine("📋 前置条件:");
        Console.WriteLine("   请确保 Qdrant 向量数据库已启动:");
        Console.WriteLine("   docker run -p 6333:6333 qdrant/qdrant\n");
        try
        {
            // 1. 创建 Kernel(包含 Chat 和 Embedding 服务)
            // embedding模型怎么加载进去的？？？
            var kernel = Settings.CreateKernelBuilderWithEmbedding().Build();
            var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            // 2. 连接 Qdrant 向量数据库
            Console.WriteLine("📡 连接 Qdrant 向量数据库...");
            var qdrantClient = new QdrantClient(QdrantEndpoint);
            // 初始化知识库集合
            await InitializeCollection(qdrantClient, CollectionName, "知识库");
            // 初始化语义缓存集合
            await InitializeCollection(qdrantClient, CacheCollectionName, "语义缓存");
            // 创建缓存服务
            var cacheService = new SemanticCacheService(qdrantClient, embeddingService);
            // ===== 示例 1: 构建知识库 =====
            await Example1_BuildKnowledgeBase(qdrantClient, embeddingService);
            // ===== 示例 2: 语义搜索 =====
            await Example2_SemanticSearch(qdrantClient, embeddingService);
            // ===== 示例 3: RAG 问答(不使用缓存) =====
            await Example3_RealRAG(qdrantClient, embeddingService, chatService);
            // ===== 示例 4: RAG + 语义缓存 =====
            await Example4_RAGWithSemanticCache(qdrantClient, embeddingService, chatService, cacheService);
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
    /// 初始化集合(如果不存在则创建)
    /// </summary>
    static async Task InitializeCollection(QdrantClient client, string collectionName, string displayName)
    {
        try
        {
            await client.GetCollectionInfoAsync(collectionName);
            Console.WriteLine($"   ✅ 已连接到{displayName}集合: {collectionName}");
        }
        catch
        {
            Console.WriteLine($"   ⚠️  {displayName}集合 '{collectionName}' 不存在,正在创建...");
            await client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams
                {
                    Size = 1536, // OpenAI text-embedding-ada-002 的维度
                    Distance = Distance.Cosine
                }
            );
            Console.WriteLine($"   ✅ {displayName}集合创建成功");
        }
    }

    /// <summary>
    /// 示例 1: 构建知识库(存储真实的向量)
    /// </summary>
    static async Task Example1_BuildKnowledgeBase(
        QdrantClient client,
        ITextEmbeddingGenerationService embeddingService)
    {
        Console.WriteLine("\n【示例 1】构建知识库\n");

        // 企业产品知识库
        var knowledge = new Dictionary<string, string>
        {
            ["智能手表功能"] = "我们的智能手表支持心率监测、血氧检测、GPS定位、50米防水、NFC支付等功能。",
            ["电池续航"] = "智能手表在正常使用下电池续航可达7天,运动模式下约24小时,支持快速充电,充电30分钟可使用1天。",
            ["系统兼容性"] = "智能手表兼容 iOS 13.0 及以上和 Android 6.0 及以上系统,通过蓝牙 5.0 连接。",
            ["健康监测"] = "支持全天候心率监测、血氧饱和度测量、睡眠质量分析、压力监测和女性生理周期管理。",
            ["运动模式"] = "内置100+种运动模式,包括跑步、骑行、游泳、登山、瑜伽、球类运动等,并提供专业运动数据分析。",
            ["售后保修"] = "产品提供1年免费保修服务,支持7天无理由退换货,终身技术支持。非人为损坏免费维修。"
        };

        Console.WriteLine("正在生成向量并存储到 Qdrant...");

        var points = new List<PointStruct>();
        ulong id = 1;

        foreach (var (category, text) in knowledge)
        {
            // 生成文本的向量嵌入
            var embedding = await embeddingService.GenerateEmbeddingAsync(text);

            // 创建 Qdrant Point
            var point = new PointStruct
            {
                Id = new PointId { Num = id++ },
                Vectors = embedding.ToArray(),
                Payload =
                {
                    ["text"] = text,
                    ["category"] = category
                }
            };

            points.Add(point);
            Console.WriteLine($"   ✅ 已存储: {category}");
        }

        // 批量上传到 Qdrant
        await client.UpsertAsync(CollectionName, points);

        Console.WriteLine($"\n✅ 共存储 {knowledge.Count} 条知识到向量数据库\n");
    }

    /// <summary>
    /// 示例 2: 真实的语义搜索
    /// </summary>
    static async Task Example2_SemanticSearch(
        QdrantClient client,
        ITextEmbeddingGenerationService embeddingService)
    {
        Console.WriteLine("【示例 2】真实的语义搜索\n");

        string query = "这款手表能用多长时间?";
        Console.WriteLine($"🔍 用户查询: {query}\n");

        // 生成查询的向量
        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(query);

        // 向量搜索(返回最相似的 Top 3)
        Console.WriteLine("正在向量数据库中搜索...");
        var searchResults = await client.SearchAsync(
            collectionName: CollectionName,
            vector: queryEmbedding.ToArray(),
            limit: 3,
            payloadSelector: true
        );

        Console.WriteLine("\n📊 搜索结果:\n");
        int rank = 1;
        foreach (var result in searchResults)
        {
            var category = result.Payload["category"].StringValue;
            var text = result.Payload["text"].StringValue;
            var score = result.Score;

            Console.WriteLine($"结果 {rank}:");
            Console.WriteLine($"   类别: {category}");
            Console.WriteLine($"   内容: {text}");
            Console.WriteLine($"   相似度: {score:F4}");
            Console.WriteLine();
            rank++;
        }
    }

    /// <summary>
    /// 示例 3: 真实的 RAG 问答系统(不使用缓存)
    /// </summary>
    static async Task Example3_RealRAG(
        QdrantClient client,
        ITextEmbeddingGenerationService embeddingService,
        IChatCompletionService chatService)
    {
        Console.WriteLine("【示例 3】真实的 RAG 问答系统(不使用缓存)\n");

        string question = "我想买一款运动手表,有什么功能推荐?";
        Console.WriteLine($"💬 用户提问: {question}\n");

        var startTime = DateTime.Now;

        // 步骤 1: 检索相关知识
        Console.WriteLine("步骤 1: 从知识库检索相关信息...");
        var answer = await ExecuteRAG(client, embeddingService, chatService, question);

        var duration = (DateTime.Now - startTime).TotalMilliseconds;

        Console.WriteLine("🤖 AI 回答:");
        Console.WriteLine($"{answer}");
        Console.WriteLine($"\n⏱️  总耗时: {duration:F0} ms\n");
    }

    /// <summary>
    /// 示例 4: RAG + 语义缓存 (核心演示)
    /// </summary>
    static async Task Example4_RAGWithSemanticCache(
        QdrantClient client,
        ITextEmbeddingGenerationService embeddingService,
        IChatCompletionService chatService,
        SemanticCacheService cacheService)
    {
        Console.WriteLine("【示例 4】RAG + 语义缓存 (核心演示)\n");

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

                answer = await ExecuteRAG(client, embeddingService, chatService, question);
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

        var stats = await cacheService.GetCacheStatistics();

        Console.WriteLine("📊 缓存统计:");
        Console.WriteLine($"   - 总缓存条目数: {stats.TotalCacheEntries}");
        Console.WriteLine($"   - 缓存命中次数: {stats.CacheHits}");
        Console.WriteLine($"   - 缓存未命中次数: {stats.CacheMisses}");
        Console.WriteLine($"   - 命中率: {stats.HitRate:P2}");
        Console.WriteLine($"   - 节省成本: ~${stats.CostSaved:F4}");
        Console.WriteLine($"   - 平均缓存响应时间: {stats.AvgCacheResponseTime:F0} ms");
        Console.WriteLine($"   - 平均完整 RAG 响应时间: {stats.AvgFullRAGResponseTime:F0} ms");
        Console.WriteLine($"   - 性能提升: {stats.PerformanceImprovement:F1}x\n");
    }

    /// <summary>
    /// 执行完整的 RAG 流程(辅助方法)
    /// </summary>
    static async Task<string> ExecuteRAG(
        QdrantClient client,
        ITextEmbeddingGenerationService embeddingService,
        IChatCompletionService chatService,
        string question)
    {
        // 1. 检索相关知识
        var queryEmbedding = await embeddingService.GenerateEmbeddingAsync(question);

        var searchResults = await client.SearchAsync(
            collectionName: CollectionName,
            vector: queryEmbedding.ToArray(),
            limit: 3,
            payloadSelector: true
        );

        var relevantTexts = new List<string>();
        foreach (var result in searchResults)
        {
            var text = result.Payload["text"].StringValue;
            relevantTexts.Add(text);
        }

        // 2. 构建增强的提示词
        var context = string.Join("\n", relevantTexts.Select((t, i) => $"{i + 1}. {t}"));

        var history = new ChatHistory();
        history.AddSystemMessage(@"你是一位专业的智能手表产品顾问。
请根据提供的产品知识回答用户问题,回答要准确、专业、友好、简洁(控制在100字以内)。
如果知识库中没有相关信息,请诚实告知用户。");

        history.AddUserMessage($@"产品知识:
{context}

用户问题: {question}");

        // 3. 生成答案
        var answer = await chatService.GetChatMessageContentAsync(history);

        return answer.Content ?? "";
    }
}

// ==================== 语义缓存服务 ====================

/// <summary>
/// 语义缓存服务 - 使用向量相似度实现智能缓存
/// </summary>
public class SemanticCacheService
{
    private const string CacheCollectionName = "semantic_cache";
    private const float SimilarityThreshold = 0.95f; // 相似度阈值
    private const decimal CostPerRequest = 0.002m; // 每次 LLM 调用的估算成本

    private readonly QdrantClient _client;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private ulong _nextCacheId = 1;

    // 统计数据
    private int _cacheHits = 0;
    private int _cacheMisses = 0;
    private List<long> _cacheResponseTimes = new();
    private List<long> _fullRAGResponseTimes = new();

    public SemanticCacheService(QdrantClient client, ITextEmbeddingGenerationService embeddingService)
    {
        _client = client;
        _embeddingService = embeddingService;
    }

    /// <summary>
    /// 尝试从缓存获取答案
    /// </summary>
    public async Task<CachedAnswer?> TryGetCachedAnswer(string question)
    {
        var startTime = DateTime.Now;

        // 1. 生成问题的向量
        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);

        // 2. 在缓存集合中搜索相似问题
        var searchResults = await _client.SearchAsync(
            collectionName: CacheCollectionName,
            vector: questionEmbedding.ToArray(),
            limit: 1,
            payloadSelector: true,
            scoreThreshold: SimilarityThreshold // 只返回超过阈值的结果
        );

        var duration = (DateTime.Now - startTime).TotalMilliseconds;

        if (searchResults.Any())
        {
            // 缓存命中
            _cacheHits++;
            _cacheResponseTimes.Add((long)duration);

            var result = searchResults.First();
            return new CachedAnswer
            {
                OriginalQuestion = result.Payload["question"].StringValue,
                Answer = result.Payload["answer"].StringValue,
                Similarity = result.Score,
                CachedAt = DateTime.Parse(result.Payload["timestamp"].StringValue)
            };
        }

        // 缓存未命中
        _cacheMisses++;
        return null;
    }

    /// <summary>
    /// 将问答对存入缓存
    /// </summary>
    public async Task CacheAnswer(string question, string answer)
    {
        // 1. 生成问题的向量
        var questionEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);

        // 2. 创建缓存条目
        var cacheEntry = new PointStruct
        {
            Id = new PointId { Num = _nextCacheId++ },
            Vectors = questionEmbedding.ToArray(),
            Payload =
            {
                ["question"] = question,
                ["answer"] = answer,
                ["timestamp"] = DateTime.UtcNow.ToString("O")
            }
        };

        // 3. 存入缓存集合
        await _client.UpsertAsync(CacheCollectionName, new[] { cacheEntry });
    }

    /// <summary>
    /// 记录完整 RAG 响应时间
    /// </summary>
    public void RecordFullRAGResponseTime(long milliseconds)
    {
        _fullRAGResponseTimes.Add(milliseconds);
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    public async Task<CacheStatistics> GetCacheStatistics()
    {
        // 获取缓存条目总数
        var collectionInfo = await _client.GetCollectionInfoAsync(CacheCollectionName);
        var totalEntries = (int)collectionInfo.PointsCount;

        var totalRequests = _cacheHits + _cacheMisses;
        var hitRate = totalRequests > 0 ? (double)_cacheHits / totalRequests : 0;

        var avgCacheTime = _cacheResponseTimes.Any() ? _cacheResponseTimes.Average() : 0;
        var avgFullRAGTime = _fullRAGResponseTimes.Any() ? _fullRAGResponseTimes.Average() : 2000; // 默认估算

        var performanceImprovement = avgCacheTime > 0 ? avgFullRAGTime / avgCacheTime : 0;

        return new CacheStatistics
        {
            TotalCacheEntries = totalEntries,
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
