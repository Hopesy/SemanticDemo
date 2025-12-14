using Microsoft.SemanticKernel;
using Common;

namespace GettingStarted;

/// <summary>
/// 00 - 快速入门
/// 演示如何快速开始使用 Semantic Kernel
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Semantic Kernel 快速入门 ===\n");

        try
        {
            // 步骤 1: 创建 Kernel
            Console.WriteLine("步骤 1: 创建 Kernel...");
            var kernel = Settings.CreateKernelBuilder().Build();
            Console.WriteLine("Kernel 创建成功!\n");

            // 步骤 2: 加载并运行插件
            Console.WriteLine("步骤 2: 加载插件...");

            // 查找 FunPlugin 目录
            var funPluginPath = FindPluginDirectory("FunPlugin");
            if (funPluginPath == null)
            {
                Console.WriteLine("警告: 未找到 FunPlugin 目录，跳过插件演示");
                Console.WriteLine("你可以从 Semantic Kernel 仓库复制 prompt_template_samples/FunPlugin 到解决方案根目录\n");
            }
            else
            {
                var funPluginFunctions = kernel.ImportPluginFromPromptDirectory(funPluginPath);
                Console.WriteLine($"已加载插件: FunPlugin\n");

                // 步骤 4: 调用函数
                Console.WriteLine("步骤 4: 调用 Joke 函数...");
                var arguments = new KernelArguments { ["input"] = "穿越到恐龙时代" };
                var result = await kernel.InvokeAsync(funPluginFunctions["Joke"], arguments);

                Console.WriteLine("\n--- AI 生成的笑话 ---");
                Console.WriteLine(result);
                Console.WriteLine("--- 结束 ---\n");
            }

            // 步骤 3: 直接调用提示
            Console.WriteLine("步骤 3: 直接调用提示...");
            var prompt = "用一句话介绍什么是 Semantic Kernel";
            var directResult = await kernel.InvokePromptAsync(prompt);
            Console.WriteLine($"\n问题: {prompt}");
            Console.WriteLine($"回答: {directResult}\n");

            Console.WriteLine("✅ 快速入门完成!");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"\n❌ 错误: {ex.Message}");
            Console.WriteLine("\n请按照以下步骤配置:");
            Console.WriteLine("1. 在解决方案根目录创建 appsettings.json");
            Console.WriteLine("2. 参考 appsettings.openai.json 或 appsettings.azure.json 示例");
            Console.WriteLine("3. 填入你的 API 密钥");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
            Console.WriteLine($"详细信息: {ex}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    /// <summary>
    /// 查找插件目录（向上查找多级）
    /// </summary>
    private static string? FindPluginDirectory(string pluginName)
    {
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDir != null)
        {
            // 查找 prompt_template_samples/PluginName
            var pluginPath = Path.Combine(currentDir.FullName, "prompt_template_samples", pluginName);
            if (Directory.Exists(pluginPath))
            {
                return pluginPath;
            }

            // 也尝试直接在根目录下查找
            pluginPath = Path.Combine(currentDir.FullName, pluginName);
            if (Directory.Exists(pluginPath))
            {
                return pluginPath;
            }

            currentDir = currentDir.Parent;
        }

        return null;
    }
}
