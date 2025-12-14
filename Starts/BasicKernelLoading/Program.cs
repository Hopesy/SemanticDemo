#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Common;

namespace BasicKernelLoading;

/// <summary>
/// Kernel 基础加载
/// 演示如何创建和使用 Kernel 的基本操作
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Kernel 基础加载 ===\n");

        try
        {
            var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

            // 创建 Kernel
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

            // ===== 示例 1: 基础提示调用 =====
            await Example1_BasicPrompt(kernel);

            // ===== 示例 2: 模板化提示 =====
            await Example2_TemplatedPrompt(kernel);

            // ===== 示例 3: 流式调用 =====
            await Example3_StreamingPrompt(kernel);

            // ===== 示例 4: 执行设置 =====
            await Example4_ExecutionSettings(kernel);

            // ===== 示例 5: JSON 格式输出 =====
            await Example5_JsonOutput(kernel);

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
    /// 示例 1: 基础提示调用
    /// </summary>
    static async Task Example1_BasicPrompt(Kernel kernel)
    {
        Console.WriteLine("【示例 1】基础提示调用\n");

        var prompt = "天空是什么颜色？";
        Console.WriteLine($"提示: {prompt}");

        var result = await kernel.InvokePromptAsync(prompt);
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 2: 模板化提示 - 使用参数
    /// </summary>
    static async Task Example2_TemplatedPrompt(Kernel kernel)
    {
        Console.WriteLine("【示例 2】模板化提示\n");

        // 使用 {{$变量名}} 语法
        var prompt = "{{$topic}}是什么颜色？";
        var arguments = new KernelArguments { { "topic", "大海" } };

        Console.WriteLine($"提示模板: {prompt}");
        Console.WriteLine($"参数: topic = {arguments["topic"]}");

        var result = await kernel.InvokePromptAsync(prompt, arguments);
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 3: 流式调用 - 实时显示生成过程
    /// </summary>
    static async Task Example3_StreamingPrompt(Kernel kernel)
    {
        Console.WriteLine("【示例 3】流式调用\n");

        var prompt = "{{$topic}}是什么颜色？请详细解释。";
        var arguments = new KernelArguments { { "topic", "草地" } };

        Console.WriteLine($"提示: {prompt.Replace("{{$topic}}", arguments["topic"]?.ToString())}");
        Console.WriteLine("流式回答: ");

        await foreach (var update in kernel.InvokePromptStreamingAsync(prompt, arguments))
        {
            Console.Write(update);
            await Task.Delay(20); // 打字机效果
        }

        Console.WriteLine("\n");
    }

    /// <summary>
    /// 示例 4: 执行设置 - 控制生成参数
    /// </summary>
    static async Task Example4_ExecutionSettings(Kernel kernel)
    {
        Console.WriteLine("【示例 4】执行设置\n");

        var prompt = "给我讲一个关于{{$topic}}的故事。";
        var arguments = new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
                MaxTokens = 200,
                Temperature = 0.7
            })
        {
            { "topic", "小狗" }
        };

        Console.WriteLine($"提示: {prompt.Replace("{{$topic}}", "小狗")}");
        Console.WriteLine("执行设置:");
        Console.WriteLine("  - MaxTokens: 200");
        Console.WriteLine("  - Temperature: 0.7");
        Console.WriteLine();

        var result = await kernel.InvokePromptAsync(prompt, arguments);
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 5: JSON 格式输出
    /// </summary>
    static async Task Example5_JsonOutput(Kernel kernel)
    {
        Console.WriteLine("【示例 5】JSON 格式输出\n");

        var prompt = "为{{$topic}}蛋糕创建一个食谱，以 JSON 格式返回，包含 ingredients（配料）和 steps（步骤）字段。";
        var arguments = new KernelArguments(
            new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object"
            })
        {
            { "topic", "巧克力" }
        };

        Console.WriteLine($"提示: 为巧克力蛋糕创建一个食谱（JSON 格式）");
        Console.WriteLine("执行设置: ResponseFormat = json_object");
        Console.WriteLine();

        var result = await kernel.InvokePromptAsync(prompt, arguments);
        Console.WriteLine("JSON 回答:");
        Console.WriteLine(result);
        Console.WriteLine();
    }
}
