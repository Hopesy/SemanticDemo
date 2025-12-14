using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Common;

namespace SemanticFunctionInline;

/// <summary>
/// 03 - 内联运行语义函数
/// 演示如何在代码中直接定义和运行语义函数
/// 内联方式——提示词直接写在 C# 代码里；外部文件.txt或.yaml便于管理、非开发者可编辑
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 内联语义函数演示 ===\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();
            // ===== 示例 1: 简单的总结函数 =====
            Console.WriteLine("【示例 1】创建总结函数\n");
            string summarizePrompt = """
            {{$input}}
            请用一句话总结上面的内容。
            """;

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 2000, //输出长度
                Temperature = 0.2,// 创意程度
                TopP = 0.5 // 词汇多样性
            };
            // 将语义函数创建成方法是为了复用，如果只调用一次，可以直接使用InvokePromptAsync
            var summaryFunction = kernel.CreateFunctionFromPrompt(summarizePrompt, executionSettings);
            // 测试文本
            var input = """
            Semantic Kernel 是微软开发的一个开源 SDK，它允许开发者将大型语言模型（LLM）
            与传统编程语言集成。通过 Semantic Kernel，你可以轻松地创建 AI 代理、
            编排复杂的 AI 工作流，并将 AI 功能无缝集成到你的应用程序中。
            它支持多种 AI 服务提供商，包括 OpenAI、Azure OpenAI 等。
            """;
            Console.WriteLine("原文:");
            Console.WriteLine(input);
            Console.WriteLine("\n总结:");
            // KernelArguments内部实现了索引器，可以像字典一样使用以及初始化
            // KernelArguments 的简化实现（概念性）
            //public class KernelArguments
            //{
            //   // 内部用字典存储
            //   private Dictionary<string, object?> _arguments = new();

            //   // 字符串索引器 - key 可以是任意字符串
            //   public object? this[string key]
            //   {
            //     get => _arguments[key];
            //     set => _arguments[key] = value;
            //   }
            //};
            var summaryResult = await kernel.InvokeAsync(summaryFunction, new() { ["input"] = input });
            Console.WriteLine(summaryResult);

            // ===== 示例 2: 更简洁的方式 =====
            Console.WriteLine("\n\n【示例 2】使用 InvokePromptAsync 简化调用\n");
            string simplePrompt = """
            {{$input}}
            用 5 个词总结上面的内容。
            """;
            var textToSummarize = """
            机器人三定律：
            1) 机器人不得伤害人类，或因不作为使人类受到伤害。
            2) 机器人必须服从人类的命令，除非这些命令与第一定律冲突。
            3) 机器人必须保护自己的存在，只要这种保护不与第一或第二定律冲突。
            """;
            Console.WriteLine("原文:");
            Console.WriteLine(textToSummarize);
            Console.WriteLine("\n5 词总结:");
            //Kernel 就是帮你做"字符串模板替换 + 调用 AI"这两件事
            var result = await kernel.InvokePromptAsync(simplePrompt, new() { ["input"] = textToSummarize });
            Console.WriteLine(result);
            // ===== 示例 3: 多参数函数 =====
            Console.WriteLine("\n\n【示例 3】使用多个参数\n");
            string translationPrompt = """
            将以下 {{$sourceLanguage}} 文本翻译成 {{$targetLanguage}}:
            {{$text}}
            只返回翻译结果，不要添加任何解释。
            """;
            var translationArgs = new KernelArguments
            {
                ["sourceLanguage"] = "中文",
                ["targetLanguage"] = "英文",
                ["text"] = "今天天气真好，我们去公园散步吧。"
            };
            Console.WriteLine($"原文 ({translationArgs["sourceLanguage"]}): {translationArgs["text"]}");
            Console.WriteLine($"\n翻译 ({translationArgs["targetLanguage"]}):");
            var translationResult = await kernel.InvokePromptAsync(translationPrompt, translationArgs);
            Console.WriteLine(translationResult);
            // ===== 示例 4: 创意生成 =====
            Console.WriteLine("\n\n【示例 4】创意内容生成\n");
            string creativePrompt = """
            主题: {{$topic}}
            风格: {{$style}}
            请根据上述主题和风格，创作一首四行小诗。
            """;
            var creativeArgs = new KernelArguments
            {
                ["topic"] = "春天",
                ["style"] = "轻松愉快"
            };
            Console.WriteLine($"主题: {creativeArgs["topic"]}");
            Console.WriteLine($"风格: {creativeArgs["style"]}");
            Console.WriteLine("\n生成的诗歌:");

            var creativeResult = await kernel.InvokePromptAsync(creativePrompt, creativeArgs);
            Console.WriteLine(creativeResult);

            Console.WriteLine("\n\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }
}
