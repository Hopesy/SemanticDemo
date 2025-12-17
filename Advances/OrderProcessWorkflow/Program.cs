#pragma warning disable SKEXP0001, SKEXP0110

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using Common;

namespace Concepts.OrderProcessWorkflow;

/// <summary>
/// 真实的电商订单处理工作流
/// 演示类似 Process Framework 的事件驱动、状态管理、步骤编排
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== 电商订单处理工作流 ===\n");
        Console.WriteLine("本系统演示完整的订单处理流程:");
        Console.WriteLine("  1. 创建订单");
        Console.WriteLine("  2. 库存检查");
        Console.WriteLine("  3. 支付处理");
        Console.WriteLine("  4. [并行] 发货准备 & 发票生成");
        Console.WriteLine("  5. 物流追踪");
        Console.WriteLine("  6. 订单完成\n");

        try
        {
            var kernel = Settings.CreateKernelBuilder().Build();

            // ===== 示例 1: 简单订单流程 =====
            await Example1_SimpleOrderFlow();

            // ===== 示例 2: 复杂订单流程(带条件分支) =====
            await Example2_ComplexOrderFlow(kernel);

            // ===== 示例 3: 并行处理(扇出/扇入) =====
            await Example3_ParallelProcessing();

            // ===== 示例 4: 错误处理和重试 =====
            await Example4_ErrorHandlingAndRetry();

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
    /// 示例 1: 简单的线性订单流程
    /// </summary>
    static async Task Example1_SimpleOrderFlow()
    {
        Console.WriteLine("【示例 1】简单订单流程\n");

        // 创建订单
        var order = new Order
        {
            OrderId = "ORD-001",
            ProductName = "智能手表 Pro",
            Quantity = 1,
            Price = 2999.00m,
            CustomerName = "张三",
            ShippingAddress = "北京市朝阳区xxx路xxx号"
        };

        Console.WriteLine($"📦 创建订单: {order.OrderId}");
        Console.WriteLine($"   商品: {order.ProductName} x {order.Quantity}");
        Console.WriteLine($"   金额: ¥{order.Price * order.Quantity}\n");

        // 工作流
        var workflow = new OrderWorkflow();

        // 步骤 1: 库存检查
        await workflow.CheckInventory(order);

        // 步骤 2: 处理支付
        await workflow.ProcessPayment(order);

        // 步骤 3: 准备发货
        await workflow.PrepareShipment(order);

        // 步骤 4: 发货
        await workflow.Ship(order);

        // 步骤 5: 完成
        await workflow.Complete(order);

        Console.WriteLine($"\n✅ 订单 {order.OrderId} 处理完成! 状态: {order.Status}\n");
    }

    /// <summary>
    /// 示例 2: 复杂订单流程(带 AI 判断和条件分支)
    /// </summary>
    static async Task Example2_ComplexOrderFlow(Kernel kernel)
    {
        Console.WriteLine("【示例 2】复杂订单流程(带 AI 风险评估)\n");

        var order = new Order
        {
            OrderId = "ORD-002",
            ProductName = "iPhone 15 Pro Max",
            Quantity = 5, // 大量购买
            Price = 9999.00m,
            CustomerName = "李四",
            ShippingAddress = "异地地址",
            CustomerEmail = "lisi@example.com"
        };

        Console.WriteLine($"📦 创建订单: {order.OrderId}");
        Console.WriteLine($"   商品: {order.ProductName} x {order.Quantity}");
        Console.WriteLine($"   金额: ¥{order.Price * order.Quantity} (高价值订单)\n");

        var workflow = new OrderWorkflow();

        // 步骤 1: 库存检查
        await workflow.CheckInventory(order);

        // 步骤 2: AI 风险评估
        var riskScore = await workflow.AssessRiskWithAI(kernel, order);
        Console.WriteLine($"   🎯 AI 风险评分: {riskScore}/100\n");

        // 条件分支: 根据风险评分决定流程
        if (riskScore > 70)
        {
            Console.WriteLine("   ⚠️  高风险订单,需要人工审核\n");
            await workflow.RequestManualReview(order);

            // 模拟人工审核
            Console.Write("   👤 人工审核中");
            for (int i = 0; i < 3; i++)
            {
                await Task.Delay(500);
                Console.Write(".");
            }
            Console.WriteLine(" 审核通过!\n");

            order.Notes = "人工审核通过";
        }
        else
        {
            Console.WriteLine("   ✅ 风险评估通过,自动处理\n");
        }

        // 步骤 3: 处理支付
        await workflow.ProcessPayment(order);

        // 步骤 4: 准备发货
        await workflow.PrepareShipment(order);

        // 步骤 5: 发货
        await workflow.Ship(order);

        // 步骤 6: 完成
        await workflow.Complete(order);

        Console.WriteLine($"\n✅ 订单 {order.OrderId} 处理完成!\n");
    }

    /// <summary>
    /// 示例 3: 并行处理(扇出/扇入模式)
    /// </summary>
    static async Task Example3_ParallelProcessing()
    {
        Console.WriteLine("【示例 3】并行处理(扇出/扇入)\n");

        var order = new Order
        {
            OrderId = "ORD-003",
            ProductName = "MacBook Pro",
            Quantity = 1,
            Price = 15999.00m,
            CustomerName = "王五",
            ShippingAddress = "上海市浦东新区xxx路xxx号"
        };

        Console.WriteLine($"📦 订单: {order.OrderId}\n");

        var workflow = new OrderWorkflow();

        // 前置步骤
        await workflow.CheckInventory(order);
        await workflow.ProcessPayment(order);

        // 并行处理: 同时进行发货准备、发票生成、通知发送
        Console.WriteLine("   ⚡ 启动并行处理...\n");

        var tasks = new List<Task>
        {
            Task.Run(async () =>
            {
                Console.WriteLine("   📋 [任务1] 开始准备发货...");
                await Task.Delay(1000);
                order.ShippingLabel = "SF1234567890";
                Console.WriteLine("   ✅ [任务1] 发货准备完成\n");
            }),
            Task.Run(async () =>
            {
                Console.WriteLine("   💰 [任务2] 开始生成发票...");
                await Task.Delay(800);
                order.InvoiceNumber = "INV-2024-001";
                Console.WriteLine("   ✅ [任务2] 发票生成完成\n");
            }),
            Task.Run(async () =>
            {
                Console.WriteLine("   📧 [任务3] 开始发送通知...");
                await Task.Delay(600);
                Console.WriteLine("   ✅ [任务3] 通知发送完成\n");
            })
        };

        // 等待所有并行任务完成(扇入)
        await Task.WhenAll(tasks);

        Console.WriteLine("   ✅ 所有并行任务完成!\n");

        // 后续步骤
        await workflow.Ship(order);
        await workflow.Complete(order);

        Console.WriteLine($"\n✅ 订单 {order.OrderId} 处理完成!\n");
    }

    /// <summary>
    /// 示例 4: 错误处理和自动重试
    /// </summary>
    static async Task Example4_ErrorHandlingAndRetry()
    {
        Console.WriteLine("【示例 4】错误处理和自动重试\n");

        var order = new Order
        {
            OrderId = "ORD-004",
            ProductName = "iPad Air",
            Quantity = 1,
            Price = 4599.00m,
            CustomerName = "赵六",
            ShippingAddress = "广州市天河区xxx路xxx号"
        };

        Console.WriteLine($"📦 订单: {order.OrderId}\n");

        var workflow = new OrderWorkflow();

        // 步骤 1: 库存检查
        await workflow.CheckInventory(order);

        // 步骤 2: 支付处理(模拟失败和重试)
        Console.WriteLine("   💳 处理支付...");

        int retryCount = 0;
        int maxRetries = 3;
        bool paymentSuccess = false;

        while (!paymentSuccess && retryCount < maxRetries)
        {
            try
            {
                // 模拟支付失败
                if (retryCount < 2)
                {
                    await Task.Delay(500);
                    throw new Exception("支付网关超时");
                }

                await workflow.ProcessPayment(order);
                paymentSuccess = true;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"   ❌ 支付失败: {ex.Message}");

                if (retryCount < maxRetries)
                {
                    Console.WriteLine($"   🔄 重试 ({retryCount}/{maxRetries})...");
                    await Task.Delay(1000);
                }
                else
                {
                    Console.WriteLine($"   ❌ 支付失败次数过多,订单取消\n");
                    order.Status = OrderStatus.Cancelled;
                    return;
                }
            }
        }

        Console.WriteLine($"   ✅ 支付成功(第 {retryCount + 1} 次尝试)\n");

        // 后续步骤
        await workflow.PrepareShipment(order);
        await workflow.Ship(order);
        await workflow.Complete(order);

        Console.WriteLine($"\n✅ 订单 {order.OrderId} 处理完成!\n");
    }
}

/// <summary>
/// 订单工作流 - 封装所有业务步骤
/// </summary>
public class OrderWorkflow
{
    private readonly InventoryService _inventory = new();
    private readonly PaymentService _payment = new();
    private readonly ShippingService _shipping = new();

    /// <summary>
    /// 步骤 1: 检查库存
    /// </summary>
    public async Task CheckInventory(Order order)
    {
        Console.WriteLine("   📦 检查库存...");
        await Task.Delay(300);

        var available = _inventory.CheckStock(order.ProductName, order.Quantity);

        if (!available)
        {
            order.Status = OrderStatus.OutOfStock;
            throw new Exception("库存不足");
        }

        _inventory.ReserveStock(order.ProductName, order.Quantity);
        order.Status = OrderStatus.InventoryChecked;
        Console.WriteLine($"   ✅ 库存充足,已预留 {order.Quantity} 件\n");
    }

    /// <summary>
    /// 步骤 2: 处理支付
    /// </summary>
    public async Task ProcessPayment(Order order)
    {
        Console.WriteLine("   💳 处理支付...");
        await Task.Delay(500);

        var totalAmount = order.Price * order.Quantity;
        var success = _payment.Charge(order.OrderId, totalAmount);

        if (!success)
        {
            order.Status = OrderStatus.PaymentFailed;
            throw new Exception("支付失败");
        }

        order.Status = OrderStatus.Paid;
        Console.WriteLine($"   ✅ 支付成功: ¥{totalAmount}\n");
    }

    /// <summary>
    /// 步骤 2.5: AI 风险评估
    /// </summary>
    public async Task<int> AssessRiskWithAI(Kernel kernel, Order order)
    {
        Console.WriteLine("   🤖 AI 风险评估中...");

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var prompt = $@"作为风险评估专家,请评估以下订单的风险等级(0-100):

订单信息:
- 商品: {order.ProductName}
- 数量: {order.Quantity}
- 金额: ¥{order.Price * order.Quantity}
- 客户: {order.CustomerName}

评估因素:
- 订单金额 (高额订单风险较高)
- 购买数量 (批量购买可能是黄牛)
- 商品类型 (高价值电子产品风险较高)

只返回0-100的数字,不要解释。";

        var history = new ChatHistory();
        history.AddUserMessage(prompt);

        var response = await chatService.GetChatMessageContentAsync(history);
        var scoreText = response.Content?.Trim() ?? "50";

        // 提取数字
        var riskScore = int.TryParse(new string(scoreText.Where(char.IsDigit).ToArray()), out var score)
            ? Math.Min(100, score)
            : 50;

        return riskScore;
    }

    /// <summary>
    /// 步骤 3: 人工审核请求
    /// </summary>
    public async Task RequestManualReview(Order order)
    {
        Console.WriteLine("   👤 提交人工审核请求...");
        await Task.Delay(200);
        order.Status = OrderStatus.PendingReview;
    }

    /// <summary>
    /// 步骤 4: 准备发货
    /// </summary>
    public async Task PrepareShipment(Order order)
    {
        Console.WriteLine("   📋 准备发货...");
        await Task.Delay(400);

        order.ShippingLabel = _shipping.GenerateShippingLabel(order);
        order.Status = OrderStatus.ReadyToShip;
        Console.WriteLine($"   ✅ 运单号: {order.ShippingLabel}\n");
    }

    /// <summary>
    /// 步骤 5: 发货
    /// </summary>
    public async Task Ship(Order order)
    {
        Console.WriteLine("   🚚 安排发货...");
        await Task.Delay(500);

        _shipping.Ship(order);
        order.Status = OrderStatus.Shipped;
        order.ShippedAt = DateTime.Now;
        Console.WriteLine($"   ✅ 已发货,预计 2-3 天送达\n");
    }

    /// <summary>
    /// 步骤 6: 完成订单
    /// </summary>
    public async Task Complete(Order order)
    {
        Console.WriteLine("   ✅ 订单完成");
        await Task.Delay(200);

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.Now;
    }
}

/// <summary>
/// 订单模型
/// </summary>
public class Order
{
    public string OrderId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string ShippingAddress { get; set; } = "";
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public string ShippingLabel { get; set; } = "";
    public string InvoiceNumber { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ShippedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus
{
    Created,
    InventoryChecked,
    Paid,
    PendingReview,
    ReadyToShip,
    Shipped,
    Completed,
    Cancelled,
    OutOfStock,
    PaymentFailed
}

/// <summary>
/// 库存服务 (模拟)
/// </summary>
public class InventoryService
{
    private readonly Dictionary<string, int> _stock = new()
    {
        ["智能手表 Pro"] = 100,
        ["iPhone 15 Pro Max"] = 50,
        ["MacBook Pro"] = 30,
        ["iPad Air"] = 80
    };

    public bool CheckStock(string product, int quantity)
    {
        return _stock.ContainsKey(product) && _stock[product] >= quantity;
    }

    public void ReserveStock(string product, int quantity)
    {
        if (_stock.ContainsKey(product))
        {
            _stock[product] -= quantity;
        }
    }
}

/// <summary>
/// 支付服务 (模拟)
/// </summary>
public class PaymentService
{
    public bool Charge(string orderId, decimal amount)
    {
        // 模拟支付处理
        return amount > 0;
    }
}

/// <summary>
/// 物流服务 (模拟)
/// </summary>
public class ShippingService
{
    private static int _labelCounter = 1000;

    public string GenerateShippingLabel(Order order)
    {
        return $"SF{_labelCounter++:D10}";
    }

    public void Ship(Order order)
    {
        // 模拟发货处理
    }
}
