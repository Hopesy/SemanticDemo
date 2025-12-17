#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Common;

namespace Concepts.Plugins;

/// <summary>
/// 插件系统核心概念
/// 演示如何创建、导入和使用插件
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 插件系统核心概念 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();
            // ===== 示例 1: 创建和导入原生插件 =====
            await Example1_NativePlugin(kernel);
            // ===== 示例 2: 内联函数插件 =====
            await Example2_InlineFunction(kernel);
            // ===== 示例 3: 列出所有插件和函数 =====
            await Example3_ListPlugins(kernel);
            // ===== 示例 4: AI 自动调用插件 =====
            await Example4_AutoFunctionCalling(kernel);
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
    /// 示例 1: 创建和导入原生插件
    /// </summary>
    static async Task Example1_NativePlugin(Kernel kernel)
    {
        Console.WriteLine("【示例 1】创建和导入原生插件\n");
        // 导入插件
        kernel.ImportPluginFromType<MathPlugin>("Math");
        kernel.ImportPluginFromType<TextPlugin>("Text");
        // 直接调用插件函数
        var result1 = await kernel.InvokeAsync("Math", "Add", new() { ["a"] = "10", ["b"] = "20" });
        Console.WriteLine($"Math.Add(10, 20) = {result1}");
        var result2 = await kernel.InvokeAsync("Text", "Uppercase", new() { ["text"] = "hello world" });
        Console.WriteLine($"Text.Uppercase('hello world') = {result2}");
        var result3 = await kernel.InvokeAsync("Math", "Multiply", new() { ["a"] = "5", ["b"] = "6" });
        Console.WriteLine($"Math.Multiply(5, 6) = {result3}\n");
    }

    /// <summary>
    /// 示例 2: 内联函数插件
    /// </summary>
    static async Task Example2_InlineFunction(Kernel kernel)
    {
        Console.WriteLine("【示例 2】内联函数插件\n");

        // 创建内联函数
        var jokeFunction = kernel.CreateFunctionFromPrompt(
            "讲一个关于 {{$topic}} 的笑话",
            new OpenAIPromptExecutionSettings() { MaxTokens = 200 },
            functionName: "TellJoke",
            description: "讲一个笑话");
        var result = await kernel.InvokeAsync(jokeFunction, new() { ["topic"] = "程序员" });
        Console.WriteLine($"笑话: {result}\n");
    }
    /// <summary>
    /// 示例 3: 列出所有插件和函数
    /// </summary>
    static async Task Example3_ListPlugins(Kernel kernel)
    {
        Console.WriteLine("【示例 3】列出所有插件和函数\n");
        var functions = kernel.Plugins.GetFunctionsMetadata();
        Console.WriteLine($"共有 {functions.Count()} 个函数:\n");
        foreach (var func in functions)
        {
            Console.WriteLine($"插件: {func.PluginName}");
            Console.WriteLine($"  函数: {func.Name}");
            Console.WriteLine($"  描述: {func.Description}");

            if (func.Parameters.Count > 0)
            {
                Console.WriteLine("  参数:");
                foreach (var param in func.Parameters)
                {
                    Console.WriteLine($"    - {param.Name}: {param.Description}");
                }
            }
            Console.WriteLine();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 示例 4: AI 自动调用插件
    /// </summary>
    static async Task Example4_AutoFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 4】AI 自动调用插件\n");

        // 启用自动函数调用
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        // AI 会自动识别需要调用哪些插件函数
        var result = await kernel.InvokePromptAsync(
            "请计算 15 加 25 的结果，然后将结果转换为大写文本",
            new(executionSettings));
        Console.WriteLine($"结果: {result}\n");
    }
}

/// <summary>
/// 数学插件 - 提供基础数学运算
/// </summary>
public class MathPlugin
{
    [KernelFunction, Description("两个数相加")]
    public int Add(
        [Description("第一个数")] int a,
        [Description("第二个数")] int b)
    {
        return a + b;
    }

    [KernelFunction, Description("两个数相减")]
    public int Subtract(
        [Description("被减数")] int a,
        [Description("减数")] int b)
    {
        return a - b;
    }

    [KernelFunction, Description("两个数相乘")]
    public int Multiply(
        [Description("第一个数")] int a,
        [Description("第二个数")] int b)
    {
        return a * b;
    }

    [KernelFunction, Description("两个数相除")]
    public double Divide(
        [Description("被除数")] double a,
        [Description("除数")] double b)
    {
        if (b == 0)
        {
            throw new ArgumentException("除数不能为零");
        }
        return a / b;
    }
}

/// <summary>
/// 文本插件 - 提供文本处理功能
/// </summary>
public class TextPlugin
{
    [KernelFunction, Description("将文本转换为大写")]
    public string Uppercase([Description("要转换的文本")] string text)
    {
        return text.ToUpper();
    }

    [KernelFunction, Description("将文本转换为小写")]
    public string Lowercase([Description("要转换的文本")] string text)
    {
        return text.ToLower();
    }

    [KernelFunction, Description("获取文本长度")]
    public int Length([Description("要计算长度的文本")] string text)
    {
        return text.Length;
    }

    [KernelFunction, Description("反转文本")]
    public string Reverse([Description("要反转的文本")] string text)
    {
        char[] charArray = text.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    [KernelFunction, Description("连接两个文本")]
    public string Concat(
        [Description("第一个文本")] string text1,
        [Description("第二个文本")] string text2)
    {
        return text1 + text2;
    }
}
