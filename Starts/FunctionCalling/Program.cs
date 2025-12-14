#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using Common;

namespace FunctionCalling;

/// <summary>
/// 02 函数调用基础教程
/// 演示如何添加插件并让 AI 自动调用函数
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 函数调用基础教程 ===\n");

        try
        {
            // 创建 Kernel 并添加插件
            var builder = Settings.CreateKernelBuilder();
            builder.Plugins.AddFromType<TimeInformation>();
            builder.Plugins.AddFromType<MathOperations>();
            var kernel = builder.Build();
            // ===== 示例 1: AI 可能产生幻觉 =====
            await Example1_WithoutPlugin(kernel);
            // ===== 示例 2: 直接调用插件 =====
            await Example2_DirectPluginCall(kernel);
            // ===== 示例 3: 自动函数调用 =====
            await Example3_AutoFunctionCalling(kernel);
            // ===== 示例 4: 复杂场景 =====
            await Example4_ComplexScenario(kernel);
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
    /// 示例 1: 没有插件时 AI 可能产生幻觉
    /// </summary>
    static async Task Example1_WithoutPlugin(Kernel kernel)
    {
        Console.WriteLine("【示例 1】没有插件时 AI 可能产生幻觉\n");

        var prompt = "距离春节还有多少天？";
        Console.WriteLine($"提示: {prompt}");
        Console.WriteLine("注意: AI 无法获取实时信息，可能会猜测或产生幻觉。\n");

        var result = await kernel.InvokePromptAsync(prompt);
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 2: 在提示中直接调用插件
    /// </summary>
    static async Task Example2_DirectPluginCall(Kernel kernel)
    {
        Console.WriteLine("【示例 2】在提示中直接调用插件\n");

        // 使用 {{PluginName.FunctionName}} 语法直接调用插件
        var prompt = "当前 UTC 时间是 {{TimeInformation.GetCurrentUtcTime}}。距离春节（2026年1月29日）还有多少天？";
        Console.WriteLine($"提示模板: {prompt}");
        Console.WriteLine("说明: 使用 {{{{PluginName.FunctionName}}}} 语法直接调用插件\n");

        var result = await kernel.InvokePromptAsync(prompt);
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 3: 自动函数调用 - AI 自动决定何时调用函数
    /// </summary>
    static async Task Example3_AutoFunctionCalling(Kernel kernel)
    {
        Console.WriteLine("【示例 3】自动函数调用\n");

        // 启用自动函数调用
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var prompt = "距离春节（2026年1月29日）还有多少天？请解释你的思考过程。";
        Console.WriteLine($"提示: {prompt}");
        Console.WriteLine("说明: AI 会自动调用 GetCurrentUtcTime 函数获取当前时间\n");

        var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(settings));
        Console.WriteLine($"回答: {result}\n");
    }

    /// <summary>
    /// 示例 4: 复杂场景 - 多步骤计算
    /// </summary>
    static async Task Example4_ComplexScenario(Kernel kernel)
    {
        Console.WriteLine("【示例 4】复杂场景 - 多步骤计算\n");

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var prompt = "如果我有 15 个苹果，给了朋友 7 个，然后又买了 12 个，现在我有多少个苹果？请使用数学函数计算。";
        Console.WriteLine($"提示: {prompt}");
        Console.WriteLine("说明: AI 会自动调用 Add 和 Subtract 函数进行计算\n");

        var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(settings));
        Console.WriteLine($"回答: {result}\n");
    }
}

/// <summary>
/// 时间信息插件
/// </summary>
public class TimeInformation
{
    [KernelFunction]
    [Description("获取当前 UTC 时间")]
    public string GetCurrentUtcTime()
    {
        return DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
    }

    [KernelFunction]
    [Description("获取当前本地时间")]
    public string GetCurrentLocalTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

/// <summary>
/// 数学运算插件
/// </summary>
public class MathOperations
{
    [KernelFunction]
    [Description("将两个数字相加")]
    public int Add(
        [Description("第一个数字")] int number1,
        [Description("第二个数字")] int number2)
    {
        Console.WriteLine($"  [函数调用] Add({number1}, {number2}) = {number1 + number2}");
        return number1 + number2;
    }

    [KernelFunction]
    [Description("从第一个数字中减去第二个数字")]
    public int Subtract(
        [Description("被减数")] int number1,
        [Description("减数")] int number2)
    {
        Console.WriteLine($"  [函数调用] Subtract({number1}, {number2}) = {number1 - number2}");
        return number1 - number2;
    }

    [KernelFunction]
    [Description("将两个数字相乘")]
    public int Multiply(
        [Description("第一个数字")] int number1,
        [Description("第二个数字")] int number2)
    {
        Console.WriteLine($"  [函数调用] Multiply({number1}, {number2}) = {number1 * number2}");
        return number1 * number2;
    }
}
