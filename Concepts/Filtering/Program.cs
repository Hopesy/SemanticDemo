#pragma warning disable SKEXP0001

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Common;

namespace Concepts.Filtering;

/// <summary>
/// 过滤器核心概念
/// 演示如何使用过滤器拦截和修改函数调用
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 过滤器核心概念 ===\n");

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

            // ===== 示例 1: 函数调用过滤器 =====
            await Example1_FunctionFilter(builder);

            // ===== 示例 2: 提示渲染过滤器 =====
            await Example2_PromptFilter(builder);

            // ===== 示例 3: 自动重试过滤器 =====
            await Example3_RetryFilter(builder);

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
    /// 示例 1: 函数调用过滤器
    /// </summary>
    static async Task Example1_FunctionFilter(IKernelBuilder builder)
    {
        Console.WriteLine("【示例 1】函数调用过滤器\n");

        // 添加函数调用过滤器
        builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();

        var kernel = builder.Build();
        kernel.ImportPluginFromType<MathPlugin>("Math");

        // 调用函数（过滤器会记录调用信息）
        var result = await kernel.InvokeAsync("Math", "Add", new() { ["a"] = "10", ["b"] = "20" });
        Console.WriteLine($"结果: {result}\n");
    }

    /// <summary>
    /// 示例 2: 提示渲染过滤器
    /// </summary>
    static async Task Example2_PromptFilter(IKernelBuilder builder)
    {
        Console.WriteLine("【示例 2】提示渲染过滤器\n");

        // 添加提示渲染过滤器
        builder.Services.AddSingleton<IPromptRenderFilter, PromptLoggingFilter>();

        var kernel = builder.Build();

        var result = await kernel.InvokePromptAsync("请用一句话介绍 {{$topic}}", new() { ["topic"] = "过滤器" });
        Console.WriteLine($"结果: {result}\n");
    }

    /// <summary>
    /// 示例 3: 自动重试过滤器
    /// </summary>
    static async Task Example3_RetryFilter(IKernelBuilder builder)
    {
        Console.WriteLine("【示例 3】自动重试过滤器\n");

        // 添加重试过滤器
        builder.Services.AddSingleton<IFunctionInvocationFilter, RetryFilter>();

        var kernel = builder.Build();
        kernel.ImportPluginFromType<UnreliablePlugin>("Unreliable");

        try
        {
            // 这个函数可能会失败，但重试过滤器会自动重试
            var result = await kernel.InvokeAsync("Unreliable", "GetData");
            Console.WriteLine($"结果: {result}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"最终失败: {ex.Message}\n");
        }
    }
}

/// <summary>
/// 日志过滤器 - 记录函数调用
/// </summary>
public class LoggingFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Console.WriteLine($"[过滤器] 调用前: {context.Function.PluginName}.{context.Function.Name}");
        Console.WriteLine($"[过滤器] 参数: {string.Join(", ", context.Arguments.Select(a => $"{a.Key}={a.Value}"))}");

        // 调用实际函数
        await next(context);

        Console.WriteLine($"[过滤器] 调用后: 结果 = {context.Result}");
    }
}

/// <summary>
/// 提示日志过滤器 - 记录提示渲染
/// </summary>
public class PromptLoggingFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        Console.WriteLine($"[过滤器] 渲染前 - 函数: {context.Function.Name}");

        // 执行渲染
        await next(context);

        Console.WriteLine($"[过滤器] 渲染后的提示:\n{context.RenderedPrompt}\n");
    }
}

/// <summary>
/// 重试过滤器 - 自动重试失败的函数调用
/// </summary>
public class RetryFilter : IFunctionInvocationFilter
{
    private const int MaxRetries = 3;

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        int attempt = 0;
        Exception? lastException = null;

        while (attempt < MaxRetries)
        {
            attempt++;
            try
            {
                Console.WriteLine($"[重试过滤器] 尝试 {attempt}/{MaxRetries}");
                await next(context);
                Console.WriteLine($"[重试过滤器] 成功!");
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine($"[重试过滤器] 失败: {ex.Message}");

                if (attempt < MaxRetries)
                {
                    await Task.Delay(1000 * attempt); // 指数退避
                }
            }
        }

        Console.WriteLine($"[重试过滤器] 所有重试都失败");
        throw lastException!;
    }
}

/// <summary>
/// 数学插件
/// </summary>
public class MathPlugin
{
    [KernelFunction, Description("两个数相加")]
    public int Add([Description("第一个数")] int a, [Description("第二个数")] int b)
    {
        return a + b;
    }
}

/// <summary>
/// 不可靠的插件 - 用于演示重试
/// </summary>
public class UnreliablePlugin
{
    private static int _callCount = 0;

    [KernelFunction, Description("获取数据（可能失败）")]
    public string GetData()
    {
        _callCount++;

        // 前两次调用失败，第三次成功
        if (_callCount < 3)
        {
            throw new InvalidOperationException($"模拟失败（第 {_callCount} 次调用）");
        }

        return "成功获取数据！";
    }
}
