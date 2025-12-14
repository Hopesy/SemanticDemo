#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Common;

namespace Concepts.Streaming;

/// <summary>
/// 流式输出核心概念
/// 演示如何使用流式 API 实时获取 AI 响应
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 流式输出核心概念 ===\n");

        try
        {
            var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

            var builder = Kernel.CreateBuilder();
            if (useAzureOpenAI)
            {
                builder.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
            }
            else
            {
                builder.AddOpenAIChatCompletion(model, apiKey, orgId);
            }
            var kernel = builder.Build();

            await Example1_BasicStreaming(kernel);
            await Example2_StreamingChat(kernel);
            await Example3_StreamingWithMetadata(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    static async Task Example1_BasicStreaming(Kernel kernel)
    {
        Console.WriteLine("【示例 1】基础流式输出\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("请写一首关于春天的短诗。");

        Console.WriteLine("用户: 请写一首关于春天的短诗。");
        Console.Write("助手: ");

        await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(history))
        {
            Console.Write(update.Content);
            await Task.Delay(20);
        }

        Console.WriteLine("\n");
    }

    static async Task Example2_StreamingChat(Kernel kernel)
    {
        Console.WriteLine("【示例 2】流式聊天\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("你是一位友好的助手。");
        history.AddUserMessage("给我讲一个关于勇气的小故事。");

        Console.WriteLine("用户: 给我讲一个关于勇气的小故事。");
        Console.Write("助手: ");

        string fullResponse = "";
        await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(history))
        {
            Console.Write(update.Content);
            fullResponse += update.Content;
            await Task.Delay(20);
        }

        Console.WriteLine($"\n\n完整响应长度: {fullResponse.Length} 字符\n");
    }

    static async Task Example3_StreamingWithMetadata(Kernel kernel)
    {
        Console.WriteLine("【示例 3】流式输出with元数据\n");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("解释一下什么是人工智能。");

        Console.WriteLine("用户: 解释一下什么是人工智能。");
        Console.Write("助手: ");

        int chunkCount = 0;
        await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(history))
        {
            Console.Write(update.Content);
            chunkCount++;
            await Task.Delay(20);
        }

        Console.WriteLine($"\n\n收到的数据块数量: {chunkCount}\n");
    }
}
