#pragma warning disable SKEXP0001, SKEXP0050

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Common;

namespace Concepts.TextSearch;

/// <summary>
/// TextSearch 插件核心概念
/// 演示如何使用 ITextSearch 接口创建标准化的搜索插件
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
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();

            // ===== 示例 1: 基础 TextSearch 使用 =====
            await Example1_BasicTextSearch();

            // ===== 示例 2: TextSearch 插件与自动函数调用 =====
            await Example2_TextSearchWithFunctionCalling(kernel);

            // ===== 示例 3: 元数据过滤 =====
            await Example3_TextSearchWithFiltering();

            // ===== 示例 4: 分页支持 =====
            await Example4_TextSearchWithPagination();

            // ===== 示例 5: RAG 场景 - 搜索增强生成 =====
            await Example5_RAGWithTextSearch(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
            Console.WriteLine("\n💡 提示:");
            Console.WriteLine("   - TextSearch 提供了统一的搜索抽象");
            Console.WriteLine("   - 可以基于 Bing、Google、VectorStore 等实现");
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
    /// 示例 1: 基础 TextSearch 使用
    /// </summary>
    static async Task Example1_BasicTextSearch()
    {
        Console.WriteLine("【示例 1】基础 TextSearch 使用\n");

        // 创建一个基于内存的 TextSearch 实现
        var textSearch = new InMemoryTextSearch();

        // 1. 简单搜索 - 返回字符串结果
        Console.WriteLine("1. 简单搜索 (SearchAsync):");
        var query = "Semantic Kernel";
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
    }

    /// <summary>
    /// 示例 2: TextSearch 插件与自动函数调用
    /// </summary>
    static async Task Example2_TextSearchWithFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 2】TextSearch 插件与自动函数调用\n");

        // 创建 TextSearch 实例
        var textSearch = new InMemoryTextSearch();

        // 将 TextSearch 转换为 Kernel 插件
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
    /// 示例 3: 元数据过滤
    /// </summary>
    static async Task Example3_TextSearchWithFiltering()
    {
        Console.WriteLine("【示例 3】元数据过滤\n");

        var textSearch = new InMemoryTextSearch();

        // 使用元数据过滤 - 只搜索特定类别
        var filter = new TextSearchFilter().Equality("category", "tutorial");
        var options = new TextSearchOptions { Filter = filter, Top = 3 };

        Console.WriteLine("搜索条件: category = 'tutorial'\n");
        var results = await textSearch.GetTextSearchResultsAsync("Semantic Kernel", options);

        await foreach (var result in results.Results)
        {
            Console.WriteLine($"   [{result.Name}]");
            Console.WriteLine($"   {result.Value}");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// 示例 4: 分页支持
    /// </summary>
    static async Task Example4_TextSearchWithPagination()
    {
        Console.WriteLine("【示例 4】分页支持 (Top/Skip)\n");

        var textSearch = new InMemoryTextSearch();

        // 第一页: Top=2, Skip=0
        Console.WriteLine("第 1 页 (Top=2, Skip=0):");
        var page1 = await textSearch.GetTextSearchResultsAsync(
            "Semantic Kernel",
            new TextSearchOptions { Top = 2, Skip = 0 });

        await foreach (var result in page1.Results)
        {
            Console.WriteLine($"   - {result.Name}");
        }

        // 第二页: Top=2, Skip=2
        Console.WriteLine("\n第 2 页 (Top=2, Skip=2):");
        var page2 = await textSearch.GetTextSearchResultsAsync(
            "Semantic Kernel",
            new TextSearchOptions { Top = 2, Skip = 2 });

        await foreach (var result in page2.Results)
        {
            Console.WriteLine($"   - {result.Name}");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// 示例 5: RAG 场景 - 搜索增强生成
    /// </summary>
    static async Task Example5_RAGWithTextSearch(Kernel kernel)
    {
        Console.WriteLine("【示例 5】RAG 场景 - 搜索增强生成\n");

        var textSearch = new InMemoryTextSearch();

        // 创建带引用的搜索插件
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
/// 基于内存的 TextSearch 实现
/// 模拟真实的搜索服务（如 Bing、Google、VectorStore）
/// </summary>
public class InMemoryTextSearch : ITextSearch
{
    // 模拟知识库数据
    private readonly List<KnowledgeItem> _knowledgeBase = new()
    {
        new("Semantic Kernel 简介",
            "Semantic Kernel 是微软开发的开源 AI 编排框架，用于将大语言模型集成到应用程序中。",
            "https://learn.microsoft.com/semantic-kernel/overview",
            "overview"),

        new("插件系统",
            "Semantic Kernel 的插件系统允许你将自定义功能封装为可重用的组件，LLM 可以自动调用这些插件。",
            "https://learn.microsoft.com/semantic-kernel/concepts/plugins",
            "tutorial"),

        new("提示模板",
            "提示模板支持参数化和动态内容生成，可以使用 Handlebars 或 Liquid 语法。",
            "https://learn.microsoft.com/semantic-kernel/prompts/templates",
            "tutorial"),

        new("函数调用",
            "通过 FunctionChoiceBehavior.Auto() 启用自动函数调用，LLM 会自动决定何时调用哪些函数。",
            "https://learn.microsoft.com/semantic-kernel/concepts/function-calling",
            "tutorial"),

        new("RAG 检索增强生成",
            "使用 TextSearch 插件可以轻松实现 RAG，将外部知识库集成到 LLM 的回答中。",
            "https://learn.microsoft.com/semantic-kernel/concepts/rag",
            "advanced"),

        new("向量存储",
            "Semantic Kernel 支持多种向量数据库，如 Qdrant、Chroma、Pinecone 等。",
            "https://learn.microsoft.com/semantic-kernel/concepts/vector-stores",
            "advanced"),
    };

    /// <summary>
    /// 简单搜索 - 返回字符串结果
    /// </summary>
    public Task<KernelSearchResults<string>> SearchAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        var results = PerformSearch(query, searchOptions)
            .Select(item => $"{item.Title}: {item.Content}");

        return Task.FromResult(
            new KernelSearchResults<string>(results.ToAsyncEnumerable()));
    }

    /// <summary>
    /// 结构化搜索 - 返回 TextSearchResult
    /// </summary>
    public Task<KernelSearchResults<TextSearchResult>> GetTextSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        var results = PerformSearch(query, searchOptions)
            .Select(item => new TextSearchResult(item.Content)
            {
                Name = item.Title,
                Link = item.Link
            });

        return Task.FromResult(
            new KernelSearchResults<TextSearchResult>(results.ToAsyncEnumerable()));
    }

    /// <summary>
    /// 原始对象搜索 - 返回 KnowledgeItem
    /// </summary>
    public Task<KernelSearchResults<object>> GetSearchResultsAsync(
        string query,
        TextSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        var results = PerformSearch(query, searchOptions)
            .Cast<object>();

        return Task.FromResult(
            new KernelSearchResults<object>(results.ToAsyncEnumerable()));
    }

    /// <summary>
    /// 执行搜索逻辑（支持过滤和分页）
    /// </summary>
    private IEnumerable<KnowledgeItem> PerformSearch(string query, TextSearchOptions? options)
    {
        var results = _knowledgeBase.AsEnumerable();

        // 简单的关键词匹配
        if (!string.IsNullOrWhiteSpace(query))
        {
            results = results.Where(item =>
                item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                item.Content.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        // 元数据过滤
        if (options?.Filter != null)
        {
            // 简化实现：只支持 category 过滤
            var filterValue = ExtractFilterValue(options.Filter);
            if (!string.IsNullOrEmpty(filterValue))
            {
                results = results.Where(item => item.Category == filterValue);
            }
        }

        // 分页
        if (options?.Skip > 0)
        {
            results = results.Skip(options.Skip);
        }

        if (options?.Top > 0)
        {
            results = results.Take(options.Top);
        }

        return results.ToList();
    }

    /// <summary>
    /// 从过滤器中提取值（简化实现）
    /// </summary>
    private string? ExtractFilterValue(TextSearchFilter filter)
    {
        // 这是一个简化的实现
        // 实际应该解析 filter 的 FilterClauses
        return "tutorial"; // 示例中硬编码
    }

    /// <summary>
    /// 知识库条目
    /// </summary>
    private record KnowledgeItem(string Title, string Content, string Link, string Category);
}

/// <summary>
/// TextSearch 扩展方法
/// 将 ITextSearch 转换为 KernelPlugin
/// </summary>
public static class TextSearchExtensions
{
    /// <summary>
    /// 创建简单搜索插件
    /// </summary>
    public static KernelPlugin CreateWithSearch(
        this ITextSearch textSearch,
        string pluginName)
    {
        var function = KernelFunctionFactory.CreateFromMethod(
            async ([Description("搜索查询")] string query) =>
            {
                var results = await textSearch.SearchAsync(query, new TextSearchOptions { Top = 3 });
                var resultList = new List<string>();
                await foreach (var result in results.Results)
                {
                    resultList.Add(result);
                }
                return string.Join("\n\n", resultList);
            },
            functionName: "Search",
            description: "在知识库中搜索相关信息");

        return KernelPluginFactory.CreateFromFunctions(pluginName, functions: [function]);
    }

    /// <summary>
    /// 创建带引用的搜索插件
    /// </summary>
    public static KernelPlugin CreateWithGetTextSearchResults(
        this ITextSearch textSearch,
        string pluginName)
    {
        var function = KernelFunctionFactory.CreateFromMethod(
            async ([Description("搜索查询")] string query) =>
            {
                var results = await textSearch.GetTextSearchResultsAsync(
                    query,
                    new TextSearchOptions { Top = 3 });

                var resultList = new List<TextSearchResult>();
                await foreach (var result in results.Results)
                {
                    resultList.Add(result);
                }
                return resultList;
            },
            functionName: "GetTextSearchResults",
            description: "在知识库中搜索相关信息并返回详细结果");

        return KernelPluginFactory.CreateFromFunctions(pluginName, functions: [function]);
    }
}
