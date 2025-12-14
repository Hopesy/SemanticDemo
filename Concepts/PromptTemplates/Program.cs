#pragma warning disable SKEXP0001

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using Common;

namespace Concepts.PromptTemplates;

/// <summary>
/// 提示模板核心概念
/// 演示如何使用不同的模板语法创建动态提示
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 提示模板核心概念 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();

            // ===== 示例 1: 基础模板语法 =====
            await Example1_BasicTemplate(kernel);

            // ===== 示例 2: 调用插件函数 =====
            await Example2_TemplateWithPlugin(kernel);

            // ===== 示例 3: Handlebars 模板 =====
            await Example3_HandlebarsTemplate(kernel);

            // ===== 示例 4: 模板渲染 =====
            await Example4_TemplateRendering(kernel);

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
    /// 示例 1: 基础模板语法
    /// </summary>
    static async Task Example1_BasicTemplate(Kernel kernel)
    {
        Console.WriteLine("【示例 1】基础模板语法\n");

        // 使用 {{$variable}} 语法
        string template = """
            你好，{{$name}}！

            欢迎来到 {{$company}}。
            今天是 {{$date}}，祝你有美好的一天！
            """;

        var arguments = new KernelArguments
        {
            ["name"] = "张三",
            ["company"] = "Semantic Kernel 学习中心",
            ["date"] = DateTime.Now.ToString("yyyy年MM月dd日")
        };

        var result = await kernel.InvokePromptAsync(template, arguments);
        Console.WriteLine($"结果:\n{result}\n");
    }

    /// <summary>
    /// 示例 2: 在模板中调用插件函数
    /// </summary>
    static async Task Example2_TemplateWithPlugin(Kernel kernel)
    {
        Console.WriteLine("【示例 2】在模板中调用插件函数\n");

        // 导入时间插件
        kernel.ImportPluginFromType<TimePlugin>("time");

        // 模板中调用插件函数
        string template = """
            当前日期: {{time.Date}}
            当前时间: {{time.Time}}

            请用 JSON 格式回答以下问题：
            1. 现在是上午、下午还是晚上？
            2. 今天是工作日还是周末？
            """;

        var result = await kernel.InvokePromptAsync(
            template,
            new(new OpenAIPromptExecutionSettings { MaxTokens = 200 }));

        Console.WriteLine($"结果:\n{result}\n");
    }

    /// <summary>
    /// 示例 3: Handlebars 模板（支持循环和条件）
    /// </summary>
    static async Task Example3_HandlebarsTemplate(Kernel kernel)
    {
        Console.WriteLine("【示例 3】Handlebars 模板\n");

        // Handlebars 模板支持循环、条件等高级语法
        string template = """
            <message role="system">
            你是一位专业的客服代表。

            客户信息:
            姓名: {{customer.name}}
            会员等级: {{customer.level}}
            积分: {{customer.points}}
            </message>

            {{#each history}}
            <message role="{{role}}">
            {{content}}
            </message>
            {{/each}}
            """;

        var arguments = new KernelArguments
        {
            ["customer"] = new
            {
                name = "李四",
                level = "黄金会员",
                points = 1500
            },
            ["history"] = new[]
            {
                new { role = "user", content = "我的会员等级是什么？" }
            }
        };

        // 创建 Handlebars 模板
        var templateFactory = new HandlebarsPromptTemplateFactory();
        var promptConfig = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars",
            Name = "CustomerServicePrompt",
            InputVariables =
            [
                new() { Name = "customer", AllowDangerouslySetContent = true },
                new() { Name = "history", AllowDangerouslySetContent = true }
            ]
        };

        var function = kernel.CreateFunctionFromPrompt(promptConfig, templateFactory);
        var result = await kernel.InvokeAsync(function, arguments);

        Console.WriteLine($"结果:\n{result}\n");
    }

    /// <summary>
    /// 示例 4: 模板渲染（查看最终发送给 AI 的提示）
    /// </summary>
    static async Task Example4_TemplateRendering(Kernel kernel)
    {
        Console.WriteLine("【示例 4】模板渲染\n");

        string template = """
            请为以下产品写一段 {{$length}} 字的营销文案：

            产品名称: {{$product}}
            目标用户: {{$audience}}
            核心卖点: {{$features}}
            """;

        var arguments = new KernelArguments
        {
            ["length"] = "100",
            ["product"] = "智能手表",
            ["audience"] = "运动爱好者",
            ["features"] = "心率监测、GPS定位、50米防水"
        };

        // 先渲染模板，查看最终的提示
        var promptTemplateFactory = new KernelPromptTemplateFactory();
        var promptTemplate = promptTemplateFactory.Create(new PromptTemplateConfig(template));
        var renderedPrompt = await promptTemplate.RenderAsync(kernel, arguments);

        Console.WriteLine("渲染后的提示:");
        Console.WriteLine("─────────────────────────────────");
        Console.WriteLine(renderedPrompt);
        Console.WriteLine("─────────────────────────────────\n");

        // 执行提示
        var result = await kernel.InvokePromptAsync(template, arguments);
        Console.WriteLine($"AI 生成的文案:\n{result}\n");
    }
}

/// <summary>
/// 时间插件 - 提供日期和时间信息
/// </summary>
public class TimePlugin
{
    [KernelFunction, Description("获取当前日期")]
    public string Date()
    {
        return DateTime.Now.ToString("yyyy年MM月dd日 dddd");
    }

    [KernelFunction, Description("获取当前时间")]
    public string Time()
    {
        return DateTime.Now.ToString("HH:mm:ss");
    }

    [KernelFunction, Description("获取当前日期时间")]
    public string Now()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
