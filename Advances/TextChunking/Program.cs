using Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Text;

namespace TextChunking;

// TextChunker 用于将长文本拆分为语义完整的块，是 RAG（检索增强生成）的关键步骤
// 两阶段分块：先分行（保证句子完整性）→ 再分段（控制块大小)
// 完整 RAG 流程：分块 → 向量化 → 存储 → 搜索
class Program
{
    static async Task Main(string[] args)
    {

        // 【1】Markdown文本分块,使用SplitMarkDownLines和SplitMarkdownParagraphs专用分法
        // 分隔符优先级不同：标点符号优先于换行符,尽量保留Markdown结构完整性（标题、列表、代码块等）
        const string markdownText = """
            # Semantic Kernel 介绍
            ## 什么是 Semantic Kernel？
            Semantic Kernel 是微软推出的轻量级 SDK，让你能够轻松地将**大语言模型 (LLM)** 集成到应用程序中。
            ### 核心特性
            - **插件系统**: 可扩展的插件架构
            - **提示模板**: 支持 Handlebars、Liquid 等模板引擎
            - **函数调用**: 自动函数调用和编排
            - **RAG 支持**: 内置向量存储和检索功能
            ## 快速开始
            ```csharp
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4", "api-key")
                .Build();
            ```
            """;

        // 使用 Markdown 专用的分块方法
        // 与纯文本的区别：分隔符优先级不同，更适合 Markdown 的语法结构
        var markdownLines = TextChunker.SplitMarkDownLines(markdownText, maxTokensPerLine: 30);
        var markdownParagraphs = TextChunker.SplitMarkdownParagraphs(markdownLines, maxTokensPerParagraph: 50);
        Console.WriteLine($"分割结果: {markdownParagraphs.Count} 个块\n");
        for (int i = 0; i < markdownParagraphs.Count; i++)
        {
            Console.WriteLine($"块 {i + 1}:");
            Console.WriteLine($"{markdownParagraphs[i]}\n");
            Console.WriteLine(new string('-', 60));
        }
        // 【2】完整 RAG 流程（分块 → 向量化 → 存储 → 搜索）
        const string ragText = """
            Semantic Kernel 是一个开源 SDK，它让开发者能够轻松地将大语言模型集成到应用中。
            它支持多种 AI 服务，包括 OpenAI、Azure OpenAI、Hugging Face 等。
            核心组件包括：Kernel（内核）、Plugins（插件）、Memory（内存）、Planners（规划器）。
            通过这些组件，开发者可以构建强大的 AI 应用。
            TextChunker 是 Semantic Kernel 中的文本分块工具，用于将长文本拆分为语义完整的块。
            它支持纯文本和 Markdown 格式，可以自定义 Token 计数器，并支持块之间的重叠。
            向量存储是 RAG 的核心组件，Semantic Kernel 支持多种向量数据库。
            包括 Qdrant、Chroma、Pinecone、InMemory 等，可以根据需求选择合适的存储方案。
            """;
        // 步骤 1：文本分块
        Console.WriteLine("📝 步骤 1：文本分块");
        Console.WriteLine(new string('─', 60));
        // 使用Tiktoken可以精确计数，默认的效果不够准确
        var ragTokenizer = TiktokenTokenizer.CreateForModel("gpt-4");
        // 两阶段分块：先分行，再分段
        var ragLines = TextChunker.SplitPlainTextLines(
            ragText,
            maxTokensPerLine: 30,
            tokenCounter: t => ragTokenizer.CountTokens(t));
        // 使用重叠提高检索效果
        var ragChunks = TextChunker.SplitPlainTextParagraphs(
            ragLines,
            maxTokensPerParagraph: 50,
            overlapTokens: 10,  // 块之间有10个Token的重叠
            tokenCounter: t => ragTokenizer.CountTokens(t));
        Console.WriteLine($"✅ 原始文本分割为 {ragChunks.Count} 个块");
        for (int i = 0; i < ragChunks.Count; i++)
        {
            var tokenCount = ragTokenizer.CountTokens(ragChunks[i]);
            Console.WriteLine($"块 {i + 1}: {tokenCount} tokens");
        }
        Console.WriteLine();

        // 创建 Embedding Generator（向量生成器）
        var embeddingGenerator = Settings.CreateEmbeddingGenerator();
        // 创建InMemory内存向量存储,生产环境推荐使用：Qdrant、Chroma、Pinecone 等
        var vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator });
        // 获取或创建集合（类似数据库中的表）
        var collection = vectorStore.GetCollection<int, ChunkDataModel>("text_chunks");
        await collection.EnsureCollectionExistsAsync();
        // 遍历每个文本块，生成向量并存储
        for (int i = 0; i < ragChunks.Count; i++)
        {
            // 创建数据模型
            // ChunkDataModel 使用 [VectorStoreVector] 特性标注
            // Content 字段会自动转换为向量（通过 Embedding => Content）
            var record = new ChunkDataModel
            {
                Key = i + 1,
                ChunkIndex = i + 1,
                Content = ragChunks[i],
                TokenCount = ragTokenizer.CountTokens(ragChunks[i])
            };
            // Upsert：插入或更新记录
            // VectorStore 会自动调用 EmbeddingGenerator 生成向量
            await collection.UpsertAsync(record);
            Console.WriteLine($"   ✅ 块 {i + 1} 已存储并向量化");
        }
        Console.WriteLine($"\n💡 共存储了 {ragChunks.Count} 个文本块到向量数据库\n");
        // 将查询文本转换为向量
        var queryEmbedding = await embeddingGenerator.GenerateAsync("支持哪些向量数据库？");
        // 执行向量搜索,SearchAsync使用余弦相似度计算查询向量与所有存储向量的相似度
        var searchResults = await collection.SearchAsync(
            queryEmbedding.Vector,
            top: 2).ToListAsync();
        Console.WriteLine($"找到 {searchResults.Count} 个最相关的文本块:\n");
        // 显示搜索结果
        for (int i = 0; i < searchResults.Count; i++)
        {
            var result = searchResults[i];
            Console.WriteLine($"   [{i + 1}] 相似度: {result.Score:F4}");
            Console.WriteLine($"       块索引: {result.Record.ChunkIndex}");
            Console.WriteLine($"       Token 数: {result.Record.TokenCount}");
            // 显示内容预览（前60个字符）
            var preview = result.Record.Content.Length > 60
                ? result.Record.Content.Substring(0, 60) + "..."
                : result.Record.Content;
            Console.WriteLine($"       内容预览: {preview}");
            Console.WriteLine();
        }
    }

}

// 分块数据模型 - 用于向量存储
public sealed class ChunkDataModel
{
    [VectorStoreKey]
    public int Key { get; init; }

    [VectorStoreData]
    public int ChunkIndex { get; init; }

    [VectorStoreData]
    public string Content { get; init; } = string.Empty;

    [VectorStoreData]
    public int TokenCount { get; init; }

    [VectorStoreVector(768)]  // 向量字段：Content 会自动转换成 768 维向量用于语义搜索
    public string Embedding => Content;
}
