#pragma warning disable SKEXP0001

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Common;

namespace Concepts.Filtering;

/// <summary>
/// 企业级 Filter + Telemetry 监控系统
/// 演示如何在生产环境中监控 AI 应用的性能、成本和质量
/// </summary>
class Program
{
    // 全局监控指标收集器
    private static readonly MetricsCollector Metrics = new();

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 企业级 Filter + Telemetry 监控系统 ===\n");

        try
        {
            // ===== 示例 1: 性能监控 =====
            await Example1_PerformanceMonitoring();

            // ===== 示例 2: 成本追踪 =====
            await Example2_CostTracking();

            // ===== 示例 3: 多层过滤器链 =====
            await Example3_FilterChain();

            // ===== 示例 4: 实时监控仪表板 =====
            Example4_MonitoringDashboard();

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
    /// 示例 1: 性能监控 - 追踪每个调用的执行时间和 Token 使用量
    /// </summary>
    static async Task Example1_PerformanceMonitoring()
    {
        Console.WriteLine("【示例 1】性能监控 - 追踪执行时间和 Token 使用\n");

        var builder = Settings.CreateKernelBuilder();
        builder.Services.AddSingleton<IFunctionInvocationFilter, PerformanceMonitoringFilter>();
        builder.Services.AddSingleton<MetricsCollector>(Metrics);

        // 添加结构化日志
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
            loggingBuilder.SetMinimumLevel(LogLevel.Information);
        });

        var kernel = builder.Build();

        // 测试多个函数调用
        var questions = new[]
        {
            "什么是机器学习?",
            "解释一下深度学习的原理",
            "AI 和机器学习的区别是什么?"
        };

        foreach (var question in questions)
        {
            var result = await kernel.InvokePromptAsync(question);
            Console.WriteLine($"问题: {question}");
            Console.WriteLine($"回答: {result}\n");
        }

        Console.WriteLine("性能统计:");
        Console.WriteLine($"- 平均响应时间: {Metrics.GetAverageResponseTime():F2} ms");
        Console.WriteLine($"- 最慢调用: {Metrics.GetMaxResponseTime():F2} ms");
        Console.WriteLine($"- 总 Token 使用: {Metrics.GetTotalTokens()}\n");
    }

    /// <summary>
    /// 示例 2: 成本追踪 - 实时计算 API 调用成本
    /// </summary>
    static async Task Example2_CostTracking()
    {
        Console.WriteLine("【示例 2】成本追踪 - 实时计算 API 成本\n");

        var builder = Settings.CreateKernelBuilder();
        builder.Services.AddSingleton<IFunctionInvocationFilter, CostTrackingFilter>();
        builder.Services.AddSingleton<MetricsCollector>(Metrics);

        var kernel = builder.Build();

        // 执行多个任务
        var tasks = new[]
        {
            "写一篇关于 AI 的 100 字短文",
            "总结量子计算的核心原理",
            "列出 5 个编程最佳实践"
        };

        foreach (var task in tasks)
        {
            Console.WriteLine($"任务: {task}");
            var result = await kernel.InvokePromptAsync(task);
            Console.WriteLine($"结果: {result.ToString().Substring(0, Math.Min(50, result.ToString().Length))}...\n");
        }

        Console.WriteLine("成本统计:");
        Console.WriteLine($"- 总成本: ${Metrics.GetTotalCost():F4}");
        Console.WriteLine($"- 输入 Token 成本: ${Metrics.GetInputCost():F4}");
        Console.WriteLine($"- 输出 Token 成本: ${Metrics.GetOutputCost():F4}\n");
    }

    /// <summary>
    /// 示例 3: 多层过滤器链 - 日志、性能、成本、重试全方位监控
    /// </summary>
    static async Task Example3_FilterChain()
    {
        Console.WriteLine("【示例 3】多层过滤器链 - 组合多个监控能力\n");

        var builder = Settings.CreateKernelBuilder();

        // 按顺序添加多个过滤器（执行顺序很重要）
        builder.Services.AddSingleton<IFunctionInvocationFilter, RequestLoggingFilter>();  // 1. 日志记录
        builder.Services.AddSingleton<IFunctionInvocationFilter, PerformanceMonitoringFilter>(); // 2. 性能监控
        builder.Services.AddSingleton<IFunctionInvocationFilter, CostTrackingFilter>(); // 3. 成本追踪
        builder.Services.AddSingleton<IFunctionInvocationFilter, AutoRetryFilter>(); // 4. 自动重试
        builder.Services.AddSingleton<IFunctionInvocationFilter, ErrorHandlingFilter>(); // 5. 错误处理

        builder.Services.AddSingleton<MetricsCollector>(Metrics);

        var kernel = builder.Build();
        kernel.ImportPluginFromType<BusinessPlugin>("Business");

        Console.WriteLine("调用业务函数 (会经过完整的过滤器链):");
        var result = await kernel.InvokeAsync("Business", "ProcessOrder", new()
        {
            ["orderId"] = "ORD-12345",
            ["amount"] = "1999.99"
        });

        Console.WriteLine($"\n最终结果: {result}\n");
    }

    /// <summary>
    /// 示例 4: 实时监控仪表板 - 展示聚合指标
    /// </summary>
    static void Example4_MonitoringDashboard()
    {
        Console.WriteLine("【示例 4】实时监控仪表板\n");

        var dashboard = Metrics.GenerateDashboard();
        Console.WriteLine(dashboard);
    }
}

// ==================== 过滤器实现 ====================

/// <summary>
/// 性能监控过滤器 - 追踪执行时间和 Token 使用
/// </summary>
public class PerformanceMonitoringFilter : IFunctionInvocationFilter
{
    private readonly MetricsCollector _metrics;
    private readonly ILogger<PerformanceMonitoringFilter>? _logger;

    public PerformanceMonitoringFilter(MetricsCollector metrics, ILogger<PerformanceMonitoringFilter>? logger = null)
    {
        _metrics = metrics;
        _logger = logger;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var stopwatch = Stopwatch.StartNew();
        var functionName = $"{context.Function.PluginName}.{context.Function.Name}";

        try
        {
            await next(context);
            stopwatch.Stop();

            // 提取 Token 使用信息
            var usage = ExtractTokenUsage(context);

            // 记录性能指标
            _metrics.RecordPerformance(functionName, stopwatch.ElapsedMilliseconds, usage);

            _logger?.LogInformation(
                "函数 {FunctionName} 执行完成 - 耗时: {Duration}ms, Tokens: {Tokens}",
                functionName, stopwatch.ElapsedMilliseconds, usage.TotalTokens);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _metrics.RecordError(functionName, ex);
            throw;
        }
    }

    private TokenUsage ExtractTokenUsage(FunctionInvocationContext context)
    {
        if (context.Result?.Metadata?.TryGetValue("Usage", out var usageObj) == true)
        {
            if (usageObj is Dictionary<string, object> usageDict)
            {
                return new TokenUsage
                {
                    PromptTokens = GetInt(usageDict, "PromptTokens"),
                    CompletionTokens = GetInt(usageDict, "CompletionTokens"),
                    TotalTokens = GetInt(usageDict, "TotalTokens")
                };
            }
        }
        return new TokenUsage();
    }

    private int GetInt(Dictionary<string, object> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && value is int intValue ? intValue : 0;
    }
}

/// <summary>
/// 成本追踪过滤器 - 计算 API 调用成本
/// </summary>
public class CostTrackingFilter : IFunctionInvocationFilter
{
    private readonly MetricsCollector _metrics;

    // GPT-4 定价 (2025年价格示例)
    private const decimal INPUT_COST_PER_1K = 0.03m;   // $0.03 / 1K tokens
    private const decimal OUTPUT_COST_PER_1K = 0.06m;  // $0.06 / 1K tokens

    public CostTrackingFilter(MetricsCollector metrics)
    {
        _metrics = metrics;
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        await next(context);

        // 提取 Token 使用信息
        if (context.Result?.Metadata?.TryGetValue("Usage", out var usageObj) == true)
        {
            if (usageObj is Dictionary<string, object> usageDict)
            {
                var promptTokens = GetInt(usageDict, "PromptTokens");
                var completionTokens = GetInt(usageDict, "CompletionTokens");

                // 计算成本
                var inputCost = (promptTokens / 1000m) * INPUT_COST_PER_1K;
                var outputCost = (completionTokens / 1000m) * OUTPUT_COST_PER_1K;
                var totalCost = inputCost + outputCost;

                // 记录成本
                _metrics.RecordCost(inputCost, outputCost, totalCost);

                Console.WriteLine($"💰 成本: 输入 ${inputCost:F4} + 输出 ${outputCost:F4} = 总计 ${totalCost:F4}");
            }
        }
    }

    private int GetInt(Dictionary<string, object> dict, string key)
    {
        return dict.TryGetValue(key, out var value) && value is int intValue ? intValue : 0;
    }
}

/// <summary>
/// 请求日志过滤器 - 记录详细的请求和响应
/// </summary>
public class RequestLoggingFilter : IFunctionInvocationFilter
{
    private static int _requestId = 0;

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        var requestId = Interlocked.Increment(ref _requestId);
        var functionName = $"{context.Function.PluginName}.{context.Function.Name}";

        // 记录请求
        Console.WriteLine($"📝 [请求 {requestId}] 调用: {functionName}");
        Console.WriteLine($"   参数: {JsonSerializer.Serialize(context.Arguments)}");

        await next(context);

        // 记录响应
        var resultPreview = context.Result?.ToString()?.Substring(0, Math.Min(100, context.Result.ToString()?.Length ?? 0));
        Console.WriteLine($"✅ [请求 {requestId}] 完成: {resultPreview}...\n");
    }
}

/// <summary>
/// 自动重试过滤器 - 处理瞬时故障
/// </summary>
public class AutoRetryFilter : IFunctionInvocationFilter
{
    private const int MAX_RETRIES = 3;
    private readonly int[] _retryDelays = { 1000, 2000, 4000 }; // 指数退避

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                await next(context);
                if (attempt > 0)
                {
                    Console.WriteLine($"🔄 重试成功 (第 {attempt + 1} 次尝试)");
                }
                return;
            }
            catch (HttpRequestException ex) when (attempt < MAX_RETRIES - 1)
            {
                lastException = ex;
                var delay = _retryDelays[attempt];
                Console.WriteLine($"⚠️  网络错误,{delay}ms 后重试... (尝试 {attempt + 1}/{MAX_RETRIES})");
                await Task.Delay(delay);
            }
            catch (Exception)
            {
                // 非网络错误不重试
                throw;
            }
        }

        throw lastException!;
    }
}

/// <summary>
/// 错误处理过滤器 - 统一异常处理和告警
/// </summary>
public class ErrorHandlingFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // 记录错误详情
            var errorLog = new
            {
                Timestamp = DateTime.UtcNow,
                Function = $"{context.Function.PluginName}.{context.Function.Name}",
                ErrorType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.StackTrace
            };

            Console.WriteLine($"❌ 错误详情:\n{JsonSerializer.Serialize(errorLog, new JsonSerializerOptions { WriteIndented = true })}");

            // 在生产环境中，这里应该发送告警 (如钉钉、企业微信、PagerDuty)
            // await SendAlertAsync(errorLog);

            throw; // 重新抛出异常
        }
    }
}

// ==================== 指标收集器 ====================

/// <summary>
/// 指标收集器 - 聚合所有监控数据
/// </summary>
public class MetricsCollector
{
    private readonly List<PerformanceMetric> _performanceMetrics = new();
    private readonly List<CostMetric> _costMetrics = new();
    private readonly List<ErrorMetric> _errorMetrics = new();
    private readonly object _lock = new();

    public void RecordPerformance(string functionName, long durationMs, TokenUsage usage)
    {
        lock (_lock)
        {
            _performanceMetrics.Add(new PerformanceMetric
            {
                FunctionName = functionName,
                DurationMs = durationMs,
                Usage = usage,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public void RecordCost(decimal inputCost, decimal outputCost, decimal totalCost)
    {
        lock (_lock)
        {
            _costMetrics.Add(new CostMetric
            {
                InputCost = inputCost,
                OutputCost = outputCost,
                TotalCost = totalCost,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public void RecordError(string functionName, Exception ex)
    {
        lock (_lock)
        {
            _errorMetrics.Add(new ErrorMetric
            {
                FunctionName = functionName,
                ErrorType = ex.GetType().Name,
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    public double GetAverageResponseTime() =>
        _performanceMetrics.Any() ? _performanceMetrics.Average(m => m.DurationMs) : 0;

    public long GetMaxResponseTime() =>
        _performanceMetrics.Any() ? _performanceMetrics.Max(m => m.DurationMs) : 0;

    public int GetTotalTokens() =>
        _performanceMetrics.Sum(m => m.Usage.TotalTokens);

    public decimal GetTotalCost() =>
        _costMetrics.Sum(m => m.TotalCost);

    public decimal GetInputCost() =>
        _costMetrics.Sum(m => m.InputCost);

    public decimal GetOutputCost() =>
        _costMetrics.Sum(m => m.OutputCost);

    public string GenerateDashboard()
    {
        lock (_lock)
        {
            var totalRequests = _performanceMetrics.Count;
            var totalErrors = _errorMetrics.Count;
            var successRate = totalRequests > 0 ? ((totalRequests - totalErrors) / (double)totalRequests * 100) : 0;

            return $@"
╔══════════════════════════════════════════════════════════╗
║            实时监控仪表板 (Real-time Dashboard)           ║
╚══════════════════════════════════════════════════════════╝

📊 请求统计
   - 总请求数: {totalRequests}
   - 成功请求: {totalRequests - totalErrors}
   - 失败请求: {totalErrors}
   - 成功率: {successRate:F2}%

⏱️  性能指标
   - 平均响应时间: {GetAverageResponseTime():F2} ms
   - 最慢响应时间: {GetMaxResponseTime()} ms
   - P95 响应时间: {GetPercentile(95):F2} ms
   - P99 响应时间: {GetPercentile(99):F2} ms

🪙 Token 使用
   - 总 Token 数: {GetTotalTokens():N0}
   - 平均每次请求: {(totalRequests > 0 ? GetTotalTokens() / totalRequests : 0):F0} tokens

💰 成本追踪
   - 总成本: ${GetTotalCost():F4}
   - 输入成本: ${GetInputCost():F4}
   - 输出成本: ${GetOutputCost():F4}
   - 平均每次请求: ${(totalRequests > 0 ? GetTotalCost() / totalRequests : 0):F4}

❌ 错误统计
{GetErrorSummary()}

⏰ 最后更新: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
";
        }
    }

    private double GetPercentile(int percentile)
    {
        if (!_performanceMetrics.Any()) return 0;

        var sorted = _performanceMetrics.Select(m => (double)m.DurationMs).OrderBy(x => x).ToList();
        int index = (int)Math.Ceiling(sorted.Count * percentile / 100.0) - 1;
        return sorted[Math.Max(0, index)];
    }

    private string GetErrorSummary()
    {
        if (!_errorMetrics.Any())
            return "   - 无错误";

        var errorGroups = _errorMetrics
            .GroupBy(e => e.ErrorType)
            .Select(g => $"   - {g.Key}: {g.Count()} 次");

        return string.Join("\n", errorGroups);
    }
}

// ==================== 数据模型 ====================

public record TokenUsage
{
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
}

public record PerformanceMetric
{
    public required string FunctionName { get; init; }
    public long DurationMs { get; init; }
    public TokenUsage Usage { get; init; } = new();
    public DateTime Timestamp { get; init; }
}

public record CostMetric
{
    public decimal InputCost { get; init; }
    public decimal OutputCost { get; init; }
    public decimal TotalCost { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ErrorMetric
{
    public required string FunctionName { get; init; }
    public required string ErrorType { get; init; }
    public required string Message { get; init; }
    public DateTime Timestamp { get; init; }
}

// ==================== 测试插件 ====================

public class BusinessPlugin
{
    [KernelFunction, Description("处理订单")]
    public async Task<string> ProcessOrder(
        [Description("订单号")] string orderId,
        [Description("金额")] string amount)
    {
        // 模拟业务处理
        await Task.Delay(Random.Shared.Next(100, 500));

        return $"订单 {orderId} 已处理，金额: ¥{amount}";
    }
}
