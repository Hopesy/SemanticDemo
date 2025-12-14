#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Common;

namespace Concepts.ChatCompletion;

/// <summary>
/// ChatCompletion 核心概念
/// 演示如何使用 ChatCompletion 服务进行对话
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ChatCompletion 核心概念 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();
            // ===== 示例 1: 基础聊天 =====
            await Example1_BasicChat(kernel);
            // ===== 示例 2: 多轮对话 =====
            await Example2_MultiTurnChat(kernel);
            // ===== 示例 3: 系统消息 =====
            await Example3_SystemMessage(kernel);
            // ===== 示例 4: 流式聊天 =====
            await Example4_StreamingChat(kernel);
            Console.WriteLine("\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
    /// <summary>
    /// 示例 1: 基础聊天
    /// </summary>
    static async Task Example1_BasicChat(Kernel kernel)
    {
        Console.WriteLine("【示例 1】基础聊天\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("你好！请用一句话介绍你自己。");

        var result = await chatService.GetChatMessageContentAsync(history);
        Console.WriteLine($"用户: {history[0].Content}");
        Console.WriteLine($"助手: {result.Content}\n");
    }

    /// <summary>
    /// 示例 2: 多轮对话
    /// </summary>
    static async Task Example2_MultiTurnChat(Kernel kernel)
    {
        Console.WriteLine("【示例 2】多轮对话\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();

        // 第一轮
        history.AddUserMessage("我想学习 C# 编程");
        var response1 = await chatService.GetChatMessageContentAsync(history);
        history.AddAssistantMessage(response1.Content!);
        Console.WriteLine($"用户: 我想学习 C# 编程");
        Console.WriteLine($"助手: {response1.Content}\n");

        // 第二轮
        history.AddUserMessage("从哪里开始比较好？");
        var response2 = await chatService.GetChatMessageContentAsync(history);
        history.AddAssistantMessage(response2.Content!);
        Console.WriteLine($"用户: 从哪里开始比较好？");
        Console.WriteLine($"助手: {response2.Content}\n");
    }

    /// <summary>
    /// 示例 3: 系统消息
    /// </summary>
    static async Task Example3_SystemMessage(Kernel kernel)
    {
        Console.WriteLine("【示例 3】系统消息\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();

        // 添加系统消息定义 AI 的角色
        history.AddSystemMessage("你是一位专业的 C# 编程导师，擅长用简单易懂的方式解释复杂概念。");
        history.AddUserMessage("什么是委托？");

        var result = await chatService.GetChatMessageContentAsync(history);
        Console.WriteLine($"系统: 你是一位专业的 C# 编程导师...");
        Console.WriteLine($"用户: 什么是委托？");
        Console.WriteLine($"助手: {result.Content}\n");
    }

    /// <summary>
    /// 示例 4: 流式聊天
    /// </summary>
    static async Task Example4_StreamingChat(Kernel kernel)
    {
        Console.WriteLine("【示例 4】流式聊天\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("请用三句话介绍 Semantic Kernel。");

        Console.WriteLine($"用户: 请用三句话介绍 Semantic Kernel。");
        Console.Write("助手: ");

        await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(history))
        {
            Console.Write(update.Content);
            await Task.Delay(20); // 打字机效果
        }

        Console.WriteLine("\n");
    }
}
