#pragma warning disable SKEXP0001, SKEXP0110

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;
using System.Text.Json;
using Common;

namespace Concepts.Agents;

/// <summary>
/// 真实的多代理协作客服系统
/// 演示Agent Handoff - 多个专业化Agent之间的智能切换
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 智能客服系统 - 多代理协作 ===\n");
        Console.WriteLine("本系统包含 4 个专业化 Agent:");
        Console.WriteLine("  1. 分流Agent - 识别问题类型并路由");
        Console.WriteLine("  2. 订单状态Agent - 处理订单查询");
        Console.WriteLine("  3. 技术支持Agent - 处理技术问题");
        Console.WriteLine("  4. 退换货Agent - 处理退换货申请\n");

        try
        {
            // 创建 Kernel
            var kernel = Settings.CreateKernelBuilder().Build();

            // 添加订单查询插件
            kernel.ImportPluginFromType<OrderPlugin>("Order");

            // ===== 示例 1: 单Agent处理 vs 多Agent协作对比 =====
            await Example1_SingleVsMultiAgent(kernel);

            // ===== 示例 2: 复杂场景 - Agent切换 =====
            await Example2_AgentHandoff(kernel);

            // ===== 示例 3: 完整客服对话流程 =====
            await Example3_FullCustomerService(kernel);

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
    /// 示例 1: 单Agent vs 多Agent对比
    /// </summary>
    static async Task Example1_SingleVsMultiAgent(Kernel kernel)
    {
        Console.WriteLine("【示例 1】单Agent vs 多Agent对比\n");

        // 通用Agent - 什么都做,但不专业
        var genericAgent = new ChatCompletionAgent
        {
            Name = "通用客服",
            Instructions = "你是客服人员,回答各种问题。",
            Kernel = kernel
        };

        // 专业化的订单Agent
        var orderAgent = new ChatCompletionAgent
        {
            Name = "订单专员",
            Instructions = """
                你是订单状态查询专员。
                专注于处理订单相关问题:查询订单状态、物流信息、发货进度等。
                使用 Order 插件查询真实的订单数据。
                回答要专业、准确。
                """,
            Kernel = kernel
        };

        string question = "帮我查一下订单 ORD-2024-001 的状态";
        Console.WriteLine($"用户问题: {question}\n");

        // 通用Agent处理
        Console.WriteLine(">>> 通用Agent 处理:");
        var genericHistory = new ChatHistory();
        genericHistory.AddUserMessage(question);
        Console.Write("  回答: ");
        await foreach (var response in genericAgent.InvokeAsync(genericHistory))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");

        // 专业Agent处理
        Console.WriteLine(">>> 订单专员 处理:");
        var orderHistory = new ChatHistory();
        orderHistory.AddUserMessage(question);
        Console.Write("  回答: ");
        await foreach (var response in orderAgent.InvokeAsync(orderHistory))
        {
            Console.Write(response.Content);
        }
        Console.WriteLine("\n");
    }

    /// <summary>
    /// 示例 2: Agent Handoff - 智能切换
    /// </summary>
    static async Task Example2_AgentHandoff(Kernel kernel)
    {
        Console.WriteLine("【示例 2】Agent Handoff - 智能切换\n");

        // 创建专业化的Agent团队
        var triageAgent = CreateTriageAgent(kernel);
        var orderAgent = CreateOrderAgent(kernel);
        var technicalAgent = CreateTechnicalAgent(kernel);
        var returnAgent = CreateReturnAgent(kernel);

        var agents = new Dictionary<string, ChatCompletionAgent>
        {
            ["triage"] = triageAgent,
            ["order"] = orderAgent,
            ["technical"] = technicalAgent,
            ["return"] = returnAgent
        };

        // 模拟用户对话
        var userQuestions = new[]
        {
            "我的手表连不上手机蓝牙",
            "我想查询订单 ORD-2024-002 的物流",
            "这个产品可以退货吗？我买了3天"
        };

        foreach (var question in userQuestions)
        {
            Console.WriteLine($"💬 用户: {question}");

            // 步骤 1: 分流Agent识别问题类型
            var routingResult = await RouteQuestion(triageAgent, question);
            Console.WriteLine($"   🔀 分流结果: {routingResult.AgentType} ({routingResult.Reason})");

            // 步骤 2: 切换到专业Agent处理
            var targetAgent = agents[routingResult.AgentType];
            Console.WriteLine($"   👤 处理Agent: {targetAgent.Name}");

            var history = new ChatHistory();
            history.AddUserMessage(question);

            Console.Write("   🤖 回答: ");
            await foreach (var response in targetAgent.InvokeAsync(history))
            {
                Console.Write(response.Content);
            }
            Console.WriteLine("\n");
        }
    }

    /// <summary>
    /// 示例 3: 完整的客服对话流程
    /// </summary>
    static async Task Example3_FullCustomerService(Kernel kernel)
    {
        Console.WriteLine("【示例 3】完整客服对话流程\n");

        var triageAgent = CreateTriageAgent(kernel);
        var orderAgent = CreateOrderAgent(kernel);
        var technicalAgent = CreateTechnicalAgent(kernel);
        var returnAgent = CreateReturnAgent(kernel);

        var agents = new Dictionary<string, ChatCompletionAgent>
        {
            ["triage"] = triageAgent,
            ["order"] = orderAgent,
            ["technical"] = technicalAgent,
            ["return"] = returnAgent
        };

        // 模拟完整的客服对话
        Console.WriteLine("=== 客服对话开始 ===\n");

        var conversation = new[]
        {
            "你好,我买的智能手表还没收到",
            "订单号是 ORD-2024-003",
            "那预计什么时候能到？",
            "好的,收到后如果有问题可以退吗？"
        };

        ChatCompletionAgent? currentAgent = null;
        var sessionHistory = new ChatHistory();

        foreach (var userMessage in conversation)
        {
            Console.WriteLine($"💬 用户: {userMessage}");

            // 每次都通过分流Agent判断是否需要切换
            var routingResult = await RouteQuestion(triageAgent, userMessage, sessionHistory);

            if (currentAgent == null || routingResult.AgentType != GetAgentKey(currentAgent, agents))
            {
                currentAgent = agents[routingResult.AgentType];
                Console.WriteLine($"   🔀 切换到: {currentAgent.Name}");
            }

            sessionHistory.AddUserMessage(userMessage);

            Console.Write("   🤖 回答: ");
            await foreach (var response in currentAgent.InvokeAsync(sessionHistory))
            {
                Console.Write(response.Content);
                sessionHistory.Add(response);
            }
            Console.WriteLine("\n");
        }

        Console.WriteLine("=== 对话结束 ===\n");
    }

    // === 辅助方法 ===

    /// <summary>
    /// 创建分流Agent
    /// </summary>
    static ChatCompletionAgent CreateTriageAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "分流专员",
            Instructions = """
                你是客服分流专员,负责识别用户问题类型。

                问题分类:
                - order: 订单查询、物流、发货状态
                - technical: 技术问题、产品使用、故障排查
                - return: 退货、换货、售后服务
                - triage: 其他一般咨询

                请分析用户问题,返回 JSON 格式:
                {
                    "agent_type": "order|technical|return|triage",
                    "reason": "分类原因"
                }
                """,
            Kernel = kernel
        };
    }

    /// <summary>
    /// 创建订单Agent
    /// </summary>
    static ChatCompletionAgent CreateOrderAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "订单专员",
            Instructions = """
                你是订单状态查询专员。
                专注于处理订单相关问题:查询订单状态、物流信息、发货进度等。
                使用 Order 插件查询真实的订单数据。
                回答要专业、准确,并主动告知客户订单详细信息。
                """,
            Kernel = kernel
        };
    }

    /// <summary>
    /// 创建技术支持Agent
    /// </summary>
    static ChatCompletionAgent CreateTechnicalAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "技术支持",
            Instructions = """
                你是技术支持专员。
                专注于解决产品使用问题、技术故障、设备连接等。

                常见问题解决方案:
                - 蓝牙连接: 确保蓝牙已开启,尝试重启设备并重新配对
                - 充电问题: 检查充电线是否插好,尝试更换充电头
                - 数据同步: 确保APP版本最新,检查网络连接

                回答要详细、有步骤,方便用户操作。
                """,
            Kernel = kernel
        };
    }

    /// <summary>
    /// 创建退换货Agent
    /// </summary>
    static ChatCompletionAgent CreateReturnAgent(Kernel kernel)
    {
        return new ChatCompletionAgent
        {
            Name = "售后专员",
            Instructions = """
                你是售后服务专员。
                专注于处理退货、换货、保修等售后问题。

                退换货政策:
                - 7天无理由退货(商品未拆封)
                - 15天内质量问题免费换货
                - 1年免费保修
                - 退货需提供订单号和问题照片

                回答要明确政策,引导用户提供必要信息。
                """,
            Kernel = kernel
        };
    }

    /// <summary>
    /// 路由问题到合适的Agent
    /// </summary>
    static async Task<RoutingResult> RouteQuestion(
        ChatCompletionAgent triageAgent,
        string question,
        ChatHistory? context = null)
    {
        var history = context != null ? new ChatHistory(context) : new ChatHistory();
        history.AddUserMessage(question);

        string response = "";
        await foreach (var message in triageAgent.InvokeAsync(history))
        {
            response += message.Content;
        }

        // 解析JSON响应
        try
        {
            // 提取JSON部分
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}') + 1;
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = response.Substring(jsonStart, jsonEnd - jsonStart);
                var result = JsonSerializer.Deserialize<RoutingResult>(jsonStr, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result ?? new RoutingResult { AgentType = "triage", Reason = "默认分流" };
            }
        }
        catch
        {
            // JSON解析失败,回退到关键词匹配
        }

        // 回退: 基于关键词的简单路由
        if (question.Contains("订单") || question.Contains("物流") || question.Contains("发货"))
            return new RoutingResult { AgentType = "order", Reason = "包含订单关键词" };
        if (question.Contains("连接") || question.Contains("蓝牙") || question.Contains("故障") || question.Contains("充电"))
            return new RoutingResult { AgentType = "technical", Reason = "包含技术关键词" };
        if (question.Contains("退") || question.Contains("换") || question.Contains("保修"))
            return new RoutingResult { AgentType = "return", Reason = "包含售后关键词" };

        return new RoutingResult { AgentType = "triage", Reason = "一般咨询" };
    }

    /// <summary>
    /// 获取Agent对应的key
    /// </summary>
    static string GetAgentKey(ChatCompletionAgent agent, Dictionary<string, ChatCompletionAgent> agents)
    {
        foreach (var (key, value) in agents)
        {
            if (value == agent)
                return key;
        }
        return "triage";
    }
}

/// <summary>
/// 路由结果
/// </summary>
public class RoutingResult
{
    public string AgentType { get; set; } = "triage";
    public string Reason { get; set; } = "";
}

/// <summary>
/// 订单查询插件 (真实的后端API模拟)
/// </summary>
public class OrderPlugin
{
    // 模拟订单数据库
    private static readonly Dictionary<string, OrderInfo> OrderDatabase = new()
    {
        ["ORD-2024-001"] = new OrderInfo
        {
            OrderId = "ORD-2024-001",
            Status = "已发货",
            Product = "智能手表 Pro",
            Quantity = 1,
            TrackingNumber = "SF1234567890",
            EstimatedDelivery = "2024-12-18"
        },
        ["ORD-2024-002"] = new OrderInfo
        {
            OrderId = "ORD-2024-002",
            Status = "配送中",
            Product = "智能手表 标准版",
            Quantity = 2,
            TrackingNumber = "YT9876543210",
            EstimatedDelivery = "2024-12-17"
        },
        ["ORD-2024-003"] = new OrderInfo
        {
            OrderId = "ORD-2024-003",
            Status = "已发货",
            Product = "智能手表 运动版",
            Quantity = 1,
            TrackingNumber = "JD5555666677",
            EstimatedDelivery = "2024-12-19"
        }
    };

    [KernelFunction, Description("查询订单状态和物流信息")]
    public string QueryOrder([Description("订单号")] string orderId)
    {
        if (OrderDatabase.TryGetValue(orderId, out var order))
        {
            return $@"订单信息:
- 订单号: {order.OrderId}
- 商品: {order.Product}
- 数量: {order.Quantity}
- 状态: {order.Status}
- 快递单号: {order.TrackingNumber}
- 预计送达: {order.EstimatedDelivery}";
        }

        return $"未找到订单 {orderId},请检查订单号是否正确";
    }

    [KernelFunction, Description("查询物流详情")]
    public string QueryTracking([Description("快递单号")] string trackingNumber)
    {
        return $@"物流信息 [{trackingNumber}]:
- 2024-12-16 10:30 【北京分拨中心】已发出
- 2024-12-16 15:20 【北京转运中心】已到达
- 2024-12-17 08:00 【配送站点】派件中
- 预计今日送达";
    }
}

/// <summary>
/// 订单信息模型
/// </summary>
public class OrderInfo
{
    public string OrderId { get; set; } = "";
    public string Status { get; set; } = "";
    public string Product { get; set; } = "";
    public int Quantity { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string EstimatedDelivery { get; set; } = "";
}
