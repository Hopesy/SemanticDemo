#pragma warning disable SKEXP0001, SKEXP0050, SKEXP0020

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using Common;

namespace Concepts.TextSearch;

/// <summary>
/// TextSearch 插件核心概念
/// 演示如何使用官方的 VectorStoreTextSearch 创建标准化的搜索插件
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== TextSearch 插件核心概念 ===\n");
        Console.WriteLine("📚 TextSearch 是 Semantic Kernel 中用于 RAG 的标准化搜索模块");
        Console.WriteLine("   - 将检索封装为标准 SK 插件");
        Console.WriteLine("   - LLM 自动决定何时搜索知识库");
        Console.WriteLine("   - 支持元数据过滤、分页等高级功能\n");

        try
        {
            // 创建 Kernel 和 Embedding Generator
            var kernel = Settings.CreateKernelBuilder().Build();
            var embeddingGenerator = Settings.CreateEmbeddingGenerator();

            // 初始化向量存储和知识库
            Console.WriteLine("正在初始化向量存储和知识库...");
            var (vectorStore, collection) = await InitializeVectorStoreAsync(embeddingGenerator);
            Console.WriteLine("✅ 知识库初始化完成\n");

            // 创建 TextSearch 实例（官方实现）
            // VectorStoreTextSearch 会自动使用 DataModel 上的特性标注来映射结果
            // - [TextSearchResultName] -> TextSearchResult.Name
            // - [TextSearchResultValue] -> TextSearchResult.Value
            // - [TextSearchResultLink] -> TextSearchResult.Link
            // - [VectorStoreVector] -> 自动向量化搜索
            var textSearch = new VectorStoreTextSearch<DataModel>(collection);

            // ===== 示例 1: 基础 TextSearch 使用（包含过滤和分页） =====
            await Example1_BasicTextSearch(textSearch);

            // ===== 示例 2: TextSearch 插件与自动函数调用 =====
            await Example2_TextSearchWithFunctionCalling(kernel, textSearch);

            // ===== 示例 3: RAG 场景 - 搜索增强生成 =====
            await Example3_RAGWithTextSearch(kernel, textSearch);

            Console.WriteLine("\n✅ 所有示例完成!");
            Console.WriteLine("\n💡 提示:");
            Console.WriteLine("   - TextSearch 提供了统一的搜索抽象");
            Console.WriteLine("   - 官方提供了 VectorStoreTextSearch 实现");
            Console.WriteLine("   - 支持自动函数调用，LLM 决定何时搜索");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex.StackTrace}");
        }

        Console.WriteLine("\n程序执行完成!");
    }

    /// <summary>
    /// 初始化向量存储和知识库
    /// </summary>
    static async Task<(InMemoryVectorStore, VectorStoreCollection<Guid, DataModel>)> InitializeVectorStoreAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
    {
        // 创建 InMemory 向量存储
        var vectorStore = new InMemoryVectorStore(new() { EmbeddingGenerator = embeddingGenerator });

        // 获取集合
        var collection = vectorStore.GetCollection<Guid, DataModel>("knowledge_base");
        await collection.EnsureCollectionExistsAsync();

        // 知识库数据
        var knowledgeData = new[]
        {
            ("Semantic Kernel 简介", "Semantic Kernel 是微软开发的开源 AI 编排框架，用于将大语言模型集成到应用程序中。", "https://learn.microsoft.com/semantic-kernel/overview", "overview"),
            ("插件系统", "Semantic Kernel 的插件系统允许你将自定义功能封装为可重用的组件，LLM 可以自动调用这些插件。", "https://learn.microsoft.com/semantic-kernel/concepts/plugins", "tutorial"),
            ("提示模板", "提示模板支持参数化和动态内容生成，可以使用 Handlebars 或 Liquid 语法。", "https://learn.microsoft.com/semantic-kernel/prompts/templates", "tutorial"),
            ("函数调用", "通过 FunctionChoiceBehavior.Auto() 启用自动函数调用，LLM 会自动决定何时调用哪些函数。", "https://learn.microsoft.com/semantic-kernel/concepts/function-calling", "tutorial"),
            ("RAG 检索增强生成", "使用 TextSearch 插件可以轻松实现 RAG，将外部知识库集成到 LLM 的回答中。", "https://learn.microsoft.com/semantic-kernel/concepts/rag", "advanced"),
            ("向量存储", "Semantic Kernel 支持多种向量数据库，如 Qdrant、Chroma、Pinecone、InMemory 等。", "https://learn.microsoft.com/semantic-kernel/concepts/vector-stores", "advanced"),
        };

        // 插入数据并生成向量
        foreach (var (title, content, link, category) in knowledgeData)
        {
            var embedding = await embeddingGenerator.GenerateAsync(content);
            var record = new DataModel
            {
                Key = Guid.NewGuid(),
                Title = title,
                Content = content,
                Link = link,
                Category = category
            };
            await collection.UpsertAsync(record);
        }

        return (vectorStore, collection);
    }

    /// <summary>
    /// 示例 1: 基础 TextSearch 使用（包含过滤和分页）
    /// </summary>
    static async Task Example1_BasicTextSearch(ITextSearch textSearch)
    {
        Console.WriteLine("【示例 1】基础 TextSearch 使用\n");

        var query = "Semantic Kernel";

        // 1. 简单搜索 - 返回字符串结果
        Console.WriteLine("1. 简单搜索 (SearchAsync):");
        var searchResults = await textSearch.SearchAsync(query, new TextSearchOptions { Top = 2 });

        await foreach (var result in searchResults.Results)
        {
            Console.WriteLine($"   {result}");
        }

        // 2. 结构化搜索 - 返回 TextSearchResult
        Console.WriteLine("\n2. 结构化搜索 (GetTextSearchResultsAsync):");
        var textResults = await textSearch.GetTextSearchResultsAsync(query, new TextSearchOptions { Top = 2 });

        await foreach (var result in textResults.Results)
        {
            Console.WriteLine($"   标题: {result.Name}");
            Console.WriteLine($"   内容: {result.Value}");
            Console.WriteLine($"   链接: {result.Link}");
            Console.WriteLine();
        }

        // 3. 元数据过滤 - 只搜索特定类别
        Console.WriteLine("3. 元数据过滤 (Filter):");
        var filter = new TextSearchFilter().Equality("Category", "tutorial");
        var filterOptions = new TextSearchOptions { Filter = filter, Top = 3 };
        Console.WriteLine("   搜索条件: Category = 'tutorial'\n");

        var filteredResults = await textSearch.GetTextSearchResultsAsync(query, filterOptions);
        await foreach (var result in filteredResults.Results)
        {
            Console.WriteLine($"   [{result.Name}]");
            Console.WriteLine($"   {result.Value}");
            Console.WriteLine();
        }

        // 4. 分页支持 - Top/Skip
        Console.WriteLine("4. 分页支持 (Top/Skip):");

        // 第一页
        Console.WriteLine("   第 1 页 (Top=2, Skip=0):");
        var page1 = await textSearch.GetTextSearchResultsAsync(
            query,
            new TextSearchOptions { Top = 2, Skip = 0 });

        await foreach (var result in page1.Results)
        {
            Console.WriteLine($"      - {result.Name}");
        }

        // 第二页
        Console.WriteLine("\n   第 2 页 (Top=2, Skip=2):");
        var page2 = await textSearch.GetTextSearchResultsAsync(
            query,
            new TextSearchOptions { Top = 2, Skip = 2 });

        await foreach (var result in page2.Results)
        {
            Console.WriteLine($"      - {result.Name}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// 示例 2: TextSearch 插件与自动函数调用
    /// </summary>
    static async Task Example2_TextSearchWithFunctionCalling(Kernel kernel, ITextSearch textSearch)
    {
        Console.WriteLine("【示例 2】TextSearch 插件与自动函数调用\n");

        // 将 TextSearch 转换为 Kernel 插件（使用官方扩展方法）
        var searchPlugin = textSearch.CreateWithSearch("KnowledgeBase");
        kernel.Plugins.Add(searchPlugin);

        // 启用自动函数调用
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        // LLM 会自动决定是否需要搜索
        var question = "Semantic Kernel 支持哪些功能？";
        Console.WriteLine($"用户问题: {question}\n");
        Console.WriteLine("AI 正在分析问题并自动搜索知识库...\n");

        var result = await kernel.InvokePromptAsync(question, new(settings));
        Console.WriteLine($"AI 回答:\n{result}\n");

        // 移除插件以避免影响后续示例
        kernel.Plugins.Remove(searchPlugin);
    }

    /// <summary>
    /// 示例 3: RAG 场景 - 搜索增强生成
    /// </summary>
    static async Task Example3_RAGWithTextSearch(Kernel kernel, ITextSearch textSearch)
    {
        Console.WriteLine("【示例 3】RAG 场景 - 搜索增强生成\n");

        // 创建带引用的搜索插件（使用官方扩展方法）
        var searchPlugin = textSearch.CreateWithGetTextSearchResults("Search");
        kernel.Plugins.Add(searchPlugin);

        // 使用 Handlebars 模板格式化搜索结果
        var promptTemplate = """
            请根据以下知识库内容回答问题。

            知识库内容:
            {{#each (Search-GetTextSearchResults query)}}
            ---
            标题: {{Name}}
            内容: {{Value}}
            来源: {{Link}}
            {{/each}}
            ---

            问题: {{query}}

            要求: 请在回答中引用相关来源链接。
            """;

        var arguments = new KernelArguments
        {
            ["query"] = "如何使用 Semantic Kernel 的插件系统？"
        };

        Console.WriteLine($"问题: {arguments["query"]}\n");
        Console.WriteLine("正在搜索知识库并生成答案...\n");

        var result = await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory());

        Console.WriteLine($"AI 回答:\n{result}\n");

        // 清理
        kernel.Plugins.Remove(searchPlugin);
    }
}

/// <summary>
/// 数据模型 - 用于向量存储
/// </summary>
/// <remarks>
/// 使用特性标注来指定如何映射到 TextSearchResult
/// </remarks>
public sealed class DataModel
{
    [VectorStoreKey]
    public Guid Key { get; init; }

    [VectorStoreData]
    [TextSearchResultName]
    public string Title { get; init; } = string.Empty;

    [VectorStoreData]
    [TextSearchResultValue]
    public string Content { get; init; } = string.Empty;

    [VectorStoreData]
    [TextSearchResultLink]
    public string Link { get; init; } = string.Empty;

    [VectorStoreData(IsIndexed = true)]
    public string Category { get; init; } = string.Empty;

    [VectorStoreVector(1536)]
    public string Embedding => Content;
}
