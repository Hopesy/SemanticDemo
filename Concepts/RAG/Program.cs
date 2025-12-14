#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.ChatCompletion;
using Common;

namespace Concepts.RAG;

/// <summary>
/// RAG (检索增强生成) 核心概念
/// 演示如何结合向量搜索和 AI 生成实现 RAG
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== RAG (检索增强生成) 核心概念 ===\n");

        try
        {
            var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

            var builder = Kernel.CreateBuilder();
            if (useAzureOpenAI)
            {
                builder.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
                builder.AddAzureOpenAITextEmbeddingGeneration("text-embedding-ada-002", azureEndpoint, apiKey);
            }
            else
            {
                builder.AddOpenAIChatCompletion(model, apiKey, orgId);
                builder.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey, orgId);
            }

            var kernel = builder.Build();

            // ===== 示例 1: 基础 RAG 流程 =====
            await Example1_BasicRAG(kernel, apiKey, orgId, useAzureOpenAI, azureEndpoint);

            // ===== 示例 2: 带上下文的对话 =====
            await Example2_RAGWithContext(kernel, apiKey, orgId, useAzureOpenAI, azureEndpoint);

            // ===== 示例 3: 多文档 RAG =====
            await Example3_MultiDocumentRAG(kernel, apiKey, orgId, useAzureOpenAI, azureEndpoint);

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
    /// 示例 1: 基础 RAG 流程（概念演示）
    /// </summary>
    static async Task Example1_BasicRAG(Kernel kernel, string apiKey, string? orgId, bool useAzure, string? endpoint)
    {
        Console.WriteLine("【示例 1】基础 RAG 流程（概念演示）\n");

        // 1. 模拟知识库
        Console.WriteLine("步骤 1: 构建知识库...");
        var knowledge = new[]
        {
            "我们的智能手表支持心率监测、GPS定位和50米防水",
            "智能手表电池续航可达7天，支持快速充电",
            "智能手表兼容 iOS 和 Android 系统"
        };
        foreach (var item in knowledge)
        {
            Console.WriteLine($"  • {item}");
        }
        Console.WriteLine("✅ 知识库已构建\n");

        // 2. 用户提问
        string question = "这款手表的电池能用多久？";
        Console.WriteLine($"步骤 2: 用户提问\n问题: {question}\n");

        // 3. 模拟检索相关信息
        Console.WriteLine("步骤 3: 检索相关信息...");
        var context = knowledge[1]; // 模拟检索到最相关的信息
        Console.WriteLine($"  找到: {context} (相关度: 0.9123)\n");

        // 4. 生成答案
        Console.WriteLine("步骤 4: 生成答案...");
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("你是一位产品客服，请根据提供的产品信息回答用户问题。");
        history.AddUserMessage($"产品信息:\n{context}\n\n用户问题: {question}");

        var answer = await chatService.GetChatMessageContentAsync(history);
        Console.WriteLine($"答案: {answer.Content}\n");
    }

    /// <summary>
    /// 示例 2: 带上下文的对话（概念演示）
    /// </summary>
    static async Task Example2_RAGWithContext(Kernel kernel, string apiKey, string? orgId, bool useAzure, string? endpoint)
    {
        Console.WriteLine("【示例 2】带上下文的对话（概念演示）\n");

        // 模拟知识库
        var knowledge = new Dictionary<string, string>
        {
            ["1"] = "我们公司成立于2020年，总部位于北京",
            ["2"] = "公司主要业务是开发智能硬件产品",
            ["3"] = "公司目前有员工200人，其中研发人员占60%"
        };

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("你是公司的 AI 助手，请根据公司信息回答问题。");

        // 第一轮对话
        string question1 = "公司是什么时候成立的？";
        Console.WriteLine($"用户: {question1}");

        var context1 = knowledge["1"]; // 模拟检索
        history.AddUserMessage($"相关信息:\n{context1}\n\n问题: {question1}");

        var answer1 = await chatService.GetChatMessageContentAsync(history);
        history.AddAssistantMessage(answer1.Content!);
        Console.WriteLine($"助手: {answer1.Content}\n");

        // 第二轮对话（带上下文）
        string question2 = "有多少研发人员？";
        Console.WriteLine($"用户: {question2}");

        var context2 = knowledge["3"]; // 模拟检索
        history.AddUserMessage($"相关信息:\n{context2}\n\n问题: {question2}");

        var answer2 = await chatService.GetChatMessageContentAsync(history);
        Console.WriteLine($"助手: {answer2.Content}\n");
    }

    /// <summary>
    /// 示例 3: 多文档 RAG（概念演示）
    /// </summary>
    static async Task Example3_MultiDocumentRAG(Kernel kernel, string apiKey, string? orgId, bool useAzure, string? endpoint)
    {
        Console.WriteLine("【示例 3】多文档 RAG（概念演示）\n");

        // 模拟多文档知识库
        Console.WriteLine("构建多文档知识库...");
        var documents = new[]
        {
            "Semantic Kernel 是微软开发的 AI 编排框架",
            "Semantic Kernel 支持 OpenAI、Azure OpenAI 等多种模型",
            "要开始使用 Semantic Kernel，首先需要安装 NuGet 包",
            "创建 Kernel 实例是使用 Semantic Kernel 的第一步",
            "建议使用依赖注入来管理 Kernel 实例",
            "使用过滤器可以实现日志记录和错误处理"
        };
        Console.WriteLine($"✅ 已存储 {documents.Length} 个文档片段\n");

        // 复杂查询
        string question = "如何开始使用 Semantic Kernel？有什么最佳实践？";
        Console.WriteLine($"问题: {question}\n");

        Console.WriteLine("检索相关文档...");
        var relevantDocs = new[] { documents[2], documents[3], documents[4], documents[5] };
        for (int i = 0; i < relevantDocs.Length; i++)
        {
            Console.WriteLine($"  文档 {i + 1}: {relevantDocs[i]} (相关度: {0.9 - i * 0.1:F4})");
        }
        Console.WriteLine();

        // 生成综合答案
        var context = string.Join("\n", relevantDocs.Select((d, i) => $"{i + 1}. {d}"));
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("你是一位技术文档助手，请根据提供的文档信息综合回答用户问题。");
        history.AddUserMessage($"相关文档:\n{context}\n\n用户问题: {question}");

        var answer = await chatService.GetChatMessageContentAsync(history);
        Console.WriteLine($"综合答案:\n{answer.Content}\n");
    }

    /// <summary>
    /// 模拟创建知识库
    /// </summary>
    static Dictionary<string, List<string>> CreateMockKnowledgeBase()
    {
        return new Dictionary<string, List<string>>();
    }

    /// <summary>
    /// 模拟获取相关上下文
    /// </summary>
    static Task<string> GetRelevantContext(Dictionary<string, List<string>> kb, string collection, string query)
    {
        // 模拟返回相关内容
        return Task.FromResult("模拟的相关上下文信息");
    }
}
