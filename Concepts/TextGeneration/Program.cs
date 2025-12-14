#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Common;

namespace Concepts.TextGeneration;

/// <summary>
/// 文本生成核心概念
/// 演示如何控制 AI 文本生成的参数和行为
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 文本生成核心概念 ===\n");

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

            // ===== 示例 1: 控制输出长度 =====
            await Example1_MaxTokens(kernel);

            // ===== 示例 2: 控制创造性（Temperature）=====
            await Example2_Temperature(kernel);

            // ===== 示例 3: Top P 采样 =====
            await Example3_TopP(kernel);

            // ===== 示例 4: 停止序列 =====
            await Example4_StopSequences(kernel);

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
    /// 示例 1: 控制输出长度（MaxTokens）
    /// </summary>
    static async Task Example1_MaxTokens(Kernel kernel)
    {
        Console.WriteLine("【示例 1】控制输出长度\n");

        string prompt = "请介绍一下人工智能的发展历史";

        // 短输出
        var settings1 = new OpenAIPromptExecutionSettings { MaxTokens = 50 };
        var result1 = await kernel.InvokePromptAsync(prompt, new(settings1));
        Console.WriteLine($"短输出 (MaxTokens=50):\n{result1}\n");

        // 长输出
        var settings2 = new OpenAIPromptExecutionSettings { MaxTokens = 200 };
        var result2 = await kernel.InvokePromptAsync(prompt, new(settings2));
        Console.WriteLine($"长输出 (MaxTokens=200):\n{result2}\n");
    }

    /// <summary>
    /// 示例 2: 控制创造性（Temperature）
    /// Temperature 范围: 0.0 - 2.0
    /// - 0.0: 最确定性，输出最一致
    /// - 1.0: 平衡
    /// - 2.0: 最随机，最有创造性
    /// </summary>
    static async Task Example2_Temperature(Kernel kernel)
    {
        Console.WriteLine("【示例 2】控制创造性（Temperature）\n");

        string prompt = "给我的咖啡店起一个有创意的名字";

        // 低温度 - 保守
        var settings1 = new OpenAIPromptExecutionSettings { Temperature = 0.0, MaxTokens = 50 };
        var result1 = await kernel.InvokePromptAsync(prompt, new(settings1));
        Console.WriteLine($"保守模式 (Temperature=0.0):\n{result1}\n");

        // 高温度 - 创造性
        var settings2 = new OpenAIPromptExecutionSettings { Temperature = 1.5, MaxTokens = 50 };
        var result2 = await kernel.InvokePromptAsync(prompt, new(settings2));
        Console.WriteLine($"创造模式 (Temperature=1.5):\n{result2}\n");
    }

    /// <summary>
    /// 示例 3: Top P 采样
    /// TopP 范围: 0.0 - 1.0
    /// - 0.1: 只考虑概率最高的 10% 的词
    /// - 0.9: 考虑概率最高的 90% 的词
    /// </summary>
    static async Task Example3_TopP(Kernel kernel)
    {
        Console.WriteLine("【示例 3】Top P 采样\n");

        string prompt = "完成这句话：今天天气真";

        // 低 TopP - 更确定
        var settings1 = new OpenAIPromptExecutionSettings { TopP = 0.1, MaxTokens = 30 };
        var result1 = await kernel.InvokePromptAsync(prompt, new(settings1));
        Console.WriteLine($"确定模式 (TopP=0.1):\n{result1}\n");

        // 高 TopP - 更多样
        var settings2 = new OpenAIPromptExecutionSettings { TopP = 0.9, MaxTokens = 30 };
        var result2 = await kernel.InvokePromptAsync(prompt, new(settings2));
        Console.WriteLine($"多样模式 (TopP=0.9):\n{result2}\n");
    }

    /// <summary>
    /// 示例 4: 停止序列
    /// 当遇到指定的字符串时停止生成
    /// </summary>
    static async Task Example4_StopSequences(Kernel kernel)
    {
        Console.WriteLine("【示例 4】停止序列\n");

        string prompt = "列出三种水果：\n1.";

        // 使用停止序列在第三项后停止
        var settings = new OpenAIPromptExecutionSettings
        {
            MaxTokens = 100,
            StopSequences = new[] { "4." }  // 遇到 "4." 时停止
        };

        var result = await kernel.InvokePromptAsync(prompt, new(settings));
        Console.WriteLine($"结果（在第4项前停止）:\n{result}\n");
    }
}
