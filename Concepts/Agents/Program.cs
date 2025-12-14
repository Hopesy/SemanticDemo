#pragma warning disable SKEXP0001, SKEXP0110

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using Common;

namespace Concepts.Agents;

/// <summary>
/// Agent 核心概念
/// 演示如何创建和使用 AI Agent
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Agent 核心概念 ===\n");

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

            // ===== 示例 1: 创建基础 Agent =====
            await Example1_BasicAgent(kernel);

            // ===== 示例 2: 带插件的 Agent =====
            await Example2_AgentWithPlugins(kernel);

            // ===== 示例 3: 多轮对话 Agent =====
            await Example3_ConversationalAgent(kernel);

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
    /// 示例 1: 创建基础 Agent
    /// </summary>
    static async Task Example1_BasicAgent(Kernel kernel)
    {
        Console.WriteLine("【示例 1】创建基础 Agent\n");

        // 创建一个简单的 Agent
        ChatCompletionAgent agent = new()
        {
            Name = "助手",
            Instructions = "你是一位友好的助手，用简洁的语言回答问题。",
            Kernel = kernel
        };

        Console.WriteLine($"Agent 名称: {agent.Name}");
        Console.WriteLine($"Agent 指令: {agent.Instructions}\n");

        // 与 Agent 交互
        var history = new ChatHistory();
        history.AddUserMessage("请用一句话介绍你自己");
        Console.WriteLine($"用户: 请用一句话介绍你自己");

        Console.Write("助手: ");
        await foreach (var response in agent.InvokeAsync(history))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");
    }

    /// <summary>
    /// 示例 2: 带插件的 Agent
    /// </summary>
    static async Task Example2_AgentWithPlugins(Kernel kernel)
    {
        Console.WriteLine("【示例 2】带插件的 Agent\n");

        // 添加插件到 Kernel
        kernel.ImportPluginFromType<WeatherPlugin>("Weather");
        kernel.ImportPluginFromType<CalculatorPlugin>("Calculator");

        // 创建带插件的 Agent
        ChatCompletionAgent agent = new()
        {
            Name = "智能助手",
            Instructions = """
                你是一位智能助手，可以查询天气和进行计算。
                当用户询问天气时，使用 Weather 插件。
                当用户需要计算时，使用 Calculator 插件。
                """,
            Kernel = kernel
        };

        Console.WriteLine("Agent 已加载以下插件:");
        foreach (var plugin in kernel.Plugins)
        {
            Console.WriteLine($"  - {plugin.Name}");
        }
        Console.WriteLine();

        // 测试天气查询
        var weatherHistory = new ChatHistory();
        weatherHistory.AddUserMessage("北京今天天气怎么样？");
        Console.WriteLine($"用户: 北京今天天气怎么样？");
        Console.Write("助手: ");

        await foreach (var response in agent.InvokeAsync(weatherHistory))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");

        // 测试计算
        var calcHistory = new ChatHistory();
        calcHistory.AddUserMessage("帮我计算 25 乘以 4");
        Console.WriteLine($"用户: 帮我计算 25 乘以 4");
        Console.Write("助手: ");

        await foreach (var response in agent.InvokeAsync(calcHistory))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");
    }

    /// <summary>
    /// 示例 3: 多轮对话 Agent
    /// </summary>
    static async Task Example3_ConversationalAgent(Kernel kernel)
    {
        Console.WriteLine("【示例 3】多轮对话 Agent\n");

        ChatCompletionAgent agent = new()
        {
            Name = "对话助手",
            Instructions = "你是一位记忆力很好的助手，能记住之前的对话内容。",
            Kernel = kernel
        };

        // 创建对话历史
        var history = new ChatHistory();

        // 第一轮对话
        var message1 = new ChatMessageContent(AuthorRole.User, "我叫张三，我喜欢编程");
        history.Add(message1);
        Console.WriteLine($"用户: {message1.Content}");

        Console.Write("助手: ");
        await foreach (var response in agent.InvokeAsync(history))
        {
            Console.Write(response.Content);
            history.Add(response);
        }
        Console.WriteLine("\n");

        // 第二轮对话（测试记忆）
        var message2 = new ChatMessageContent(AuthorRole.User, "你还记得我的名字吗？");
        history.Add(message2);
        Console.WriteLine($"用户: {message2.Content}");

        Console.Write("助手: ");
        await foreach (var response in agent.InvokeAsync(history))
        {
            Console.Write(response.Content);
            history.Add(response);
        }
        Console.WriteLine("\n");

        // 第三轮对话
        var message3 = new ChatMessageContent(AuthorRole.User, "我喜欢什么？");
        history.Add(message3);
        Console.WriteLine($"用户: {message3.Content}");

        Console.Write("助手: ");
        await foreach (var response in agent.InvokeAsync(history))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");
    }
}

/// <summary>
/// 天气插件
/// </summary>
public class WeatherPlugin
{
    [KernelFunction, Description("获取指定城市的天气信息")]
    public string GetWeather([Description("城市名称")] string city)
    {
        // 模拟天气数据
        var random = new Random();
        var temperature = random.Next(15, 30);
        var conditions = new[] { "晴天", "多云", "小雨" };
        var condition = conditions[random.Next(conditions.Length)];

        return $"{city}今天{condition}，温度{temperature}°C";
    }
}

/// <summary>
/// 计算器插件
/// </summary>
public class CalculatorPlugin
{
    [KernelFunction, Description("两个数相加")]
    public int Add([Description("第一个数")] int a, [Description("第二个数")] int b)
    {
        return a + b;
    }

    [KernelFunction, Description("两个数相乘")]
    public int Multiply([Description("第一个数")] int a, [Description("第二个数")] int b)
    {
        return a * b;
    }

    [KernelFunction, Description("两个数相减")]
    public int Subtract([Description("被减数")] int a, [Description("减数")] int b)
    {
        return a - b;
    }
}
