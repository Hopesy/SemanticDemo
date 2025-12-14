using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Common;

namespace KernelArgumentsChat;

/// <summary>
/// 04 - 使用 Kernel 参数创建聊天体验
/// 演示如何通过参数管理对话历史，构建简单的聊天机器人
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Kernel 参数聊天演示 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();

            // 定义聊天机器人的提示模板
            const string chatPrompt = @"
ChatBot 可以与你讨论任何话题。
如果它不知道答案，它会明确说明'我不知道'。
{{$history}}
用户: {{$userInput}}
ChatBot:";

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5
            };

            // 创建提示词版的聊天函数,为了复用
            var chatFunction = kernel.CreateFunctionFromPrompt(chatPrompt, executionSettings);
            // 初始化对话历史和参数
            var history = "";
            var arguments = new KernelArguments
            {
                //初始化索引器的写法
                ["history"] = history
            };

            Console.WriteLine("聊天机器人已启动！输入 'exit' 或 'quit' 退出\n");
            Console.WriteLine("提示：这是一个简单的演示，展示如何使用参数管理对话历史\n");
            Console.WriteLine("---开始对话---\n");
            // 预设几个示例对话
            var demoConversations = new[]
            {
                "你好，我想找一些关于中国历史的书籍推荐",
                "我对唐朝特别感兴趣，有什么推荐吗？",
                "这本书主要讲什么内容？",
                "谢谢你的推荐！"
            };

            // 运行演示对话
            foreach (var userInput in demoConversations)
            {
                // 显示用户输入
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"用户: {userInput}");
                Console.ResetColor();
                // 设置用户输入参数
                arguments["userInput"] = userInput;
                // 调用聊天函数
                var botAnswer = await chatFunction.InvokeAsync(kernel, arguments);
                // 显示机器人回复
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"ChatBot: {botAnswer}");
                Console.ResetColor();
                Console.WriteLine();
                // 更新历史记录
                history += $"\n用户: {userInput}\nChatBot: {botAnswer}\n";
                arguments["history"] = history;
                // 添加延迟，让输出更自然
                await Task.Delay(1000);
            }

            Console.WriteLine("---演示对话结束---\n");
            // 显示完整的对话历史
            Console.WriteLine("【完整对话历史】");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine(history);
            Console.WriteLine(new string('=', 60));
            // 交互式聊天
            Console.WriteLine("\n现在你可以自己与机器人对话了！\n");
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("你: ");
                Console.ResetColor();

                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                // 设置用户输入
                arguments["userInput"] = userInput;

                // 获取机器人回复
                var answer = await chatFunction.InvokeAsync(kernel, arguments);

                // 显示回复
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"ChatBot: {answer}");
                Console.ResetColor();
                Console.WriteLine();

                // 更新历史
                history += $"\n用户: {userInput}\nChatBot: {answer}\n";
                arguments["history"] = history;
            }

            Console.WriteLine("\n✅ 聊天结束!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}
