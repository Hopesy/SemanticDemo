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
            // ===== 示例 1: 多轮对话 =====
            await Example2_MultiTurnChat(kernel);
            // ===== 示例 2: 流式聊天 =====
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
    /// 示例 1: 多轮对话
    /// </summary>
    static async Task Example2_MultiTurnChat(Kernel kernel)
    {
        Console.WriteLine("【示例 2】多轮对话\n");
        //SK的抽象层。代码不需要关心底层是OpenAI、Azure还是Ollama，只要获取这个服务接口，后续的调用方式都是统一的
        //维护对话上下文的关键容器。它不仅存储文本，还存储消息的角色（User, Assistant, System）
        //缺点:一个纯粹的API连接器。它只负责把文本发给OpenAI，然后把文本收回来。它不知道你之前在kernel.Plugins.AddFromType<...>()里注册了什么
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        //LLM大语言模型本身是无状态的,为了让AI知道上下文,下次提问时必须手动将上一轮的回复和用户的新问题一起打包再次发送给模型
        //ChatHistory会无限增长,在多轮对话中如果对话过长，会超过模型的上下文窗口限制,需要实现一种机制(如滚动窗口或摘要)来移除旧消息。
        var history = new ChatHistory();
        // 添加系统消息定义 AI 的角色，通常作为历史记录的第一条,设定 AI 的“人设”或行为准则。
        history.AddSystemMessage("你是一位专业的 C# 编程导师，擅长用简单易懂的方式解释复杂概念。");
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
    /// 示例 2: 流式聊天
    /// </summary>
    static async Task Example4_StreamingChat(Kernel kernel)
    {
        Console.WriteLine("【示例 4】流式聊天\n");
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddUserMessage("请用三句话介绍 Semantic Kernel。");
        Console.WriteLine($"用户: 请用三句话介绍 Semantic Kernel。");
        Console.Write("助手: ");
        //对于长回复，流式传输能大幅降低用户的感知延迟
        //返回的是IAsyncEnumerable，必须配合await foreach使用，实现的关键是yield return
        //GetStreamingChatMessageContentsAsync只是构建了一个异步枚举器，并不会立即执行任何网络请求，只有foreach循环开始迭代时，才会真正触发对LLM服务的调用
        //GetStreamingChatMessageContentsAsync方法的内部逻辑会异步地读取这个HTTP响应流。它不是一次性读完，而是一行一行地监听
        //成功解析出一个文本块（update）后，它不会将其存储在列表中，而是会使用 yield return 关键字立即将这个值“产出”给调用者。
        //yield return会暂停GetStreamingChatMessageContentsAsync方法的执行，将控制权和update值返回给await foreach循环。
        //当循环准备好处理下一个项目时(本案例仅console)，GetStreamingChatMessageContentsAsync方法会从上次暂停的地方继续执行等待下一个SSE事件。
        //当LLM完成全部回答的生成后，它会发送一个特殊的终止信号（例如 data: [DONE])，然后关闭 HTTP 连接
        //SDK接收到这个信号或发现流已关闭时，InvokePromptStreamingAsync方法就会执行完毕，await foreach循环也随之自然结束
        await foreach (var update in chatService.GetStreamingChatMessageContentsAsync(history))
        {
            Console.Write(update.Content);
            await Task.Delay(20); // 打字机效果
        }
        Console.WriteLine("\n");
    }
}
