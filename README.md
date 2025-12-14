# Semantic Kernel 学习演示项目

这是一个系统化的 Semantic Kernel 学习项目，将官方 Jupyter Notebook 教程转换为独立的控制台应用程序，方便直接运行和调试。

## 📊 项目完成度

| 类别 | 完成度 | 状态 |
|------|--------|------|
| **Common (共享类库)** | 1/1 | ✅ 100% |
| **Starts (基础教程)** | 5/5 | ✅ 100% |
| **Concepts (核心概念)** | 12/12 | ✅ 100% |
| **总计** | **18/18** | **✅ 100%** |

**编译状态**: ✅ 所有项目编译通过，0 警告 0 错误

---

## 📁 项目结构

```
SemanticDemo/
├── Common/                          # 共享类库
│   ├── Settings.cs                  # 配置加载类
│   └── Common.csproj
│
├── Starts/                          # 基础教程 (5个项目)
│   ├── GettingStarted/              # ✅ 快速入门
│   ├── BasicKernelLoading/          # ✅ Kernel 基础加载
│   ├── SemanticFunctionInline/      # ✅ 内联语义函数
│   ├── KernelArgumentsChat/         # ✅ 聊天机器人
│   └── FunctionCalling/             # ✅ 函数调用
│
├── Concepts/                        # 核心概念 (12个项目)
│   ├── ChatCompletion/              # ✅ 聊天完成
│   ├── Streaming/                   # ✅ 流式输出
│   ├── Plugins/                     # ✅ 插件系统
│   ├── PromptTemplates/             # ✅ 提示模板
│   ├── TextGeneration/              # ✅ 文本生成
│   ├── DependencyInjection/         # ✅ 依赖注入
│   ├── Filtering/                   # ✅ 过滤器
│   ├── FunctionCallingAdvanced/     # ✅ 高级函数调用
│   ├── Memory/                      # ✅ 记忆系统
│   ├── RAG/                         # ✅ 检索增强生成
│   ├── Agents/                      # ✅ AI 代理
│   └── Search/                      # ✅ 搜索功能
│
├── appsettings.json                 # AI 服务配置（需要创建）
├── appsettings.openai.json          # OpenAI 配置示例
├── appsettings.azure.json           # Azure OpenAI 配置示例
└── SemanticDemo.sln                 # 解决方案文件
```

---

## 🚀 快速开始

### 1. 环境要求

- **.NET 8 SDK** 或更高版本
- **Visual Studio 2022** 或 **VS Code** 或 **Rider**
- **OpenAI API 密钥** 或 **Azure OpenAI 服务**

### 2. 配置 API 密钥

在解决方案根目录创建 `appsettings.json` 文件：

#### 使用 OpenAI

```json
{
  "type": "openai",
  "model": "gpt-4o-mini",
  "apikey": "你的-OpenAI-API-密钥",
  "orgId": ""
}
```

#### 使用 Azure OpenAI

```json
{
  "type": "azure",
  "model": "你的部署名称",
  "endpoint": "https://你的资源名称.openai.azure.com/",
  "apikey": "你的-Azure-OpenAI-密钥"
}
```

**⚠️ 重要**:
- 将 `appsettings.json` 添加到 `.gitignore`，避免泄露 API 密钥
- 示例配置文件已生成：`appsettings.openai.json` 和 `appsettings.azure.json`

### 3. 运行项目

#### 方式 1: 使用 Visual Studio
1. 打开 `SemanticDemo.sln`
2. 右键点击要运行的项目 → 设置为启动项目
3. 按 F5 运行

#### 方式 2: 使用命令行
```bash
# 构建所有项目
dotnet build

# 运行特定项目
cd Starts\GettingStarted
dotnet run
```

#### 方式 3: 使用 VS Code
1. 打开解决方案文件夹
2. 按 F5 选择要运行的项目
3. 或在终端中使用 `dotnet run`

---

## 📚 完整学习路径

### 🎯 阶段 1: 基础入门（1-2周）

建议按以下顺序学习基础教程，掌握 Semantic Kernel 的核心概念。

#### 1. GettingStarted - 快速入门 ⭐
**学习目标**: 5分钟了解 Semantic Kernel 的基本使用流程

**核心内容**:
- 加载配置文件
- 创建 Kernel 实例
- 加载和运行插件
- 直接调用提示

**运行**:
```bash
cd Starts\GettingStarted
dotnet run
```

---

#### 2. SemanticFunctionInline - 内联语义函数 ⭐
**学习目标**: 学习如何在代码中直接定义和使用语义函数

**核心内容**:
- 提示模板语法 (`{{$input}}`)
- 创建函数 (`CreateFunctionFromPrompt`)
- 简化调用 (`InvokePromptAsync`)
- 多参数函数
- 执行设置 (Temperature, MaxTokens)

**示例场景**:
- 文本总结
- 语言翻译
- 创意生成（诗歌创作）

**关键代码**:
```csharp
string prompt = """
{{$input}}
请总结上面的内容。
""";

var result = await kernel.InvokePromptAsync(prompt, new() { ["input"] = text });
```

---

#### 3. BasicKernelLoading - Kernel 基础加载
**学习目标**: 深入理解 Kernel 的配置和初始化

**核心内容**:
- Kernel Builder 模式
- 基础提示调用
- 模板化提示（使用参数）
- 流式调用（打字机效果）
- 执行设置（MaxTokens, Temperature）
- JSON 格式输出

**关键代码**:
```csharp
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(model, apiKey);
var kernel = builder.Build();
```

---

#### 4. KernelArgumentsChat - 聊天机器人
**学习目标**: 使用 Kernel 参数管理对话历史

**核心内容**:
- KernelArguments 使用
- 对话历史管理
- 上下文保持
- 交互式聊天循环

**特色功能**:
- 预设演示对话（4轮自动对话）
- 实时交互聊天
- 彩色控制台输出
- 完整历史记录显示

---

#### 5. FunctionCalling - 函数调用 ⭐
**学习目标**: 理解 AI 如何自动调用你的函数

**核心内容**:
- AI 幻觉演示（无插件时）
- 直接调用插件（模板语法）
- 自动函数调用 (`FunctionChoiceBehavior.Auto()`)
- 复杂场景（多步骤计算）

**应用场景**:
- 业务流程自动化
- 工具调用
- 代码生成

**包含插件**:
- TimeInformation: 获取当前时间
- MathOperations: 数学运算

---

### 🎯 阶段 2: 核心概念（2-3周）

掌握 Semantic Kernel 的核心功能和常用模式。

#### 6. ChatCompletion - 聊天完成 ⭐
**学习目标**: 深入理解 ChatCompletionService 的使用

**核心内容**:
- 基础聊天
- 多轮对话（上下文保持）
- 系统消息（定义 AI 角色）
- 流式聊天

**示例场景**:
- 简单问答
- C# 编程导师
- 委托概念解释

**运行**:
```bash
cd Concepts\ChatCompletion
dotnet run
```

---

#### 7. Streaming - 流式输出 ⭐
**学习目标**: 掌握实时流式接收 AI 响应

**核心内容**:
- 基础流式输出
- 流式聊天
- 流式输出with元数据（统计数据块）
- 打字机效果（20ms 延迟）

**特色功能**:
- 实时显示 AI 响应
- 模拟打字机效果
- 响应长度统计

**关键代码**:
```csharp
await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(history))
{
    Console.Write(chunk.Content);
    await Task.Delay(20); // 打字机效果
}
```

---

#### 8. Plugins - 插件系统 ⭐
**学习目标**: 学习如何创建和使用原生函数插件

**核心内容**:
- 创建和导入原生插件
- 内联函数插件
- 列出所有插件和函数
- AI 自动调用插件

**示例插件**:
- **MathPlugin**: 加减乘除运算
- **TextPlugin**: 大小写转换、长度计算、反转、连接

**关键代码**:
```csharp
[KernelFunction, Description("两个数相加")]
public int Add(
    [Description("第一个数")] int a,
    [Description("第二个数")] int b)
{
    return a + b;
}
```

---

#### 9. PromptTemplates - 提示模板 ⭐
**学习目标**: 使用不同的模板语法创建动态提示

**核心内容**:
- 基础模板语法 (`{{$variable}}`)
- 在模板中调用插件函数
- Handlebars 模板（支持循环和条件）
- 模板渲染预览

**示例场景**:
- 个性化问候
- 时间查询
- 客服对话（使用 Handlebars）
- 营销文案生成

**关键代码**:
```csharp
string template = """
你好，{{$name}}！
欢迎来到 {{$company}}。
今天是 {{$date}}，祝你有美好的一天！
""";
```

---

#### 10. TextGeneration - 文本生成
**学习目标**: 控制 AI 文本生成的参数和行为

**核心内容**:
- 控制输出长度（MaxTokens）
- 控制创造性（Temperature: 0.0-2.0）
- Top P 采样（TopP: 0.0-1.0）
- 停止序列（StopSequences）

**示例对比**:
- 短输出 vs 长输出
- 保守模式 vs 创造模式
- 确定模式 vs 多样模式

---

### 🎯 阶段 3: 高级特性（3-4周）

学习企业级应用所需的高级功能。

#### 11. DependencyInjection - 依赖注入 ⭐
**学习目标**: 使用 DI 容器管理 Kernel 和服务

**核心内容**:
- 基础 DI 集成（ServiceCollection）
- 注入自定义服务
- 日志集成（Microsoft.Extensions.Logging）

**示例服务**:
- IGreetingService: 个性化问候服务
- GreetingPlugin: 使用 DI 的插件

**关键代码**:
```csharp
var services = new ServiceCollection();
services.AddSingleton<IGreetingService, GreetingService>();
services.AddKernel()
    .AddOpenAIChatCompletion(model, apiKey);
```

---

#### 12. Filtering - 过滤器 ⭐
**学习目标**: 使用过滤器拦截和修改函数调用

**核心内容**:
- 函数调用过滤器（LoggingFilter）
- 提示渲染过滤器（PromptLoggingFilter）
- 自动重试过滤器（RetryFilter）

**应用场景**:
- 日志记录
- 性能监控
- 错误处理
- 自动重试（指数退避）

**关键代码**:
```csharp
public async Task OnFunctionInvocationAsync(
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, Task> next)
{
    Console.WriteLine($"调用前: {context.Function.Name}");
    await next(context);
    Console.WriteLine($"调用后: {context.Result}");
}
```

---

#### 13. FunctionCallingAdvanced - 高级函数调用
**学习目标**: 掌握函数调用的高级特性和控制

**核心内容**:
- 自动函数调用（AI 自动决定）
- 必须调用函数（强制 AI 调用）
- 禁用函数调用（AI 不调用任何函数）
- 多步骤函数调用（AI 连续调用多个函数）

**示例插件**:
- WeatherPlugin: 天气查询
- CurrencyPlugin: 货币转换

---

### 🎯 阶段 4: 专家级（4+周）

掌握 AI 应用的高级模式和架构。

#### 14. Memory - 记忆系统 ⭐
**学习目标**: 使用嵌入和向量存储实现语义记忆

**核心内容**:
- 文本嵌入生成（将文本转换为向量）
- 语义记忆存储（概念演示）
- 语义搜索（概念演示）

**核心概念**:
- 向量嵌入的生成和使用
- 语义相似度计算
- 知识库的构建和检索

**运行**:
```bash
cd Concepts\Memory
dotnet run
```

---

#### 15. RAG - 检索增强生成 ⭐⭐
**学习目标**: 结合向量搜索和 AI 生成实现 RAG

**核心内容**:
- 基础 RAG 流程（检索 → 生成）
- 带上下文的对话
- 多文档 RAG

**RAG 四步骤**:
1. 构建知识库
2. 用户提问
3. 检索相关信息
4. 生成答案

**应用场景**:
- 产品客服（基于产品手册）
- 企业知识库问答
- 技术文档助手

**运行**:
```bash
cd Concepts\RAG
dotnet run
```

---

#### 16. Agents - AI 代理系统 ⭐⭐
**学习目标**: 创建和使用 AI Agent

**核心内容**:
- 创建基础 Agent
- 带插件的 Agent（天气、计算器）
- 多轮对话 Agent（带记忆）

**Agent 特性**:
- 自定义指令（Instructions）
- 插件集成
- 对话历史管理
- 流式响应

**关键代码**:
```csharp
ChatCompletionAgent agent = new()
{
    Name = "助手",
    Instructions = "你是一位友好的助手，用简洁的语言回答问题。",
    Kernel = kernel
};

await foreach (var response in agent.InvokeAsync(history))
{
    Console.Write(response.Content);
}
```

**运行**:
```bash
cd Concepts\Agents
dotnet run
```

---

#### 17. Search - 搜索功能
**学习目标**: 集成 Web 搜索功能

**核心内容**:
- 模拟搜索插件
- 搜索增强的对话
- 多来源搜索（Web、新闻、文档）

**示例插件**:
- MockSearchPlugin: Web 搜索
- MockNewsSearchPlugin: 新闻搜索
- MockDocSearchPlugin: 文档搜索

**应用场景**:
- 实时信息查询
- 新闻聚合
- 企业文档搜索

**运行**:
```bash
cd Concepts\Search
dotnet run
```

---

## 🎓 学习建议

### 推荐学习顺序

1. **第1周**: 完成阶段1（基础入门）
   - 重点: GettingStarted → SemanticFunctionInline → FunctionCalling

2. **第2-3周**: 完成阶段2（核心概念）
   - 重点: ChatCompletion → Streaming → Plugins → PromptTemplates

3. **第4-5周**: 完成阶段3（高级特性）
   - 重点: DependencyInjection → Filtering → FunctionCallingAdvanced

4. **第6周+**: 完成阶段4（专家级）
   - 重点: Memory → RAG → Agents

### 学习技巧

1. **动手实践**: 每个项目都可以直接运行，建议修改代码进行实验
2. **理解概念**: Memory 和 RAG 使用了概念演示，重点理解原理
3. **循序渐进**: 按照推荐顺序学习，不要跳跃
4. **做笔记**: 记录关键概念和代码片段
5. **实际应用**: 尝试将学到的知识应用到实际项目中

---

## 🔧 常见问题

### Q1: 找不到配置文件
**错误**: `未找到配置文件 appsettings.json`

**解决**:
1. 确保在解决方案根目录创建了 `appsettings.json`
2. 检查文件名拼写是否正确
3. 参考 `appsettings.openai.json` 或 `appsettings.azure.json` 示例

### Q2: API 调用失败
**错误**: `401 Unauthorized` 或 `403 Forbidden`

**解决**:
1. 检查 API 密钥是否正确
2. 确认 API 密钥有足够的配额
3. 对于 Azure OpenAI，确认部署名称和端点正确

### Q3: 编译错误
**错误**: 缺少 NuGet 包

**解决**:
```bash
# 还原所有包
dotnet restore

# 重新构建
dotnet build
```

### Q4: 运行时错误
**错误**: 找不到某个类型或方法

**解决**:
```bash
# 清理并重新构建
dotnet clean
dotnet build
```

---

## 📖 扩展学习

### 推荐资源

1. **官方文档**: https://learn.microsoft.com/semantic-kernel/
2. **GitHub 仓库**: https://github.com/microsoft/semantic-kernel
3. **示例代码**: https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples
4. **DevBlogs**: https://devblogs.microsoft.com/semantic-kernel/

### 进阶主题

- **向量存储和嵌入** (Vector Stores)
- **多代理系统** (Multi-Agent Systems)
- **流程编排** (Process Framework)
- **插件开发** (Plugin Development)
- **OpenAPI 插件** (OpenAPI Plugins)
- **Azure AI Search 集成**

---

## 📊 项目统计

| 指标 | 数值 |
|------|------|
| 总项目数 | 18 个 |
| 总代码行数 | 约 3000+ 行 |
| C# 源文件 | 18 个 |
| 插件类 | 15+ 个 |
| 示例方法 | 50+ 个 |
| 编译状态 | ✅ 100% 成功 |

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

如果你发现任何问题或有改进建议，请：
1. 提交 Issue 描述问题
2. Fork 项目并创建分支
3. 提交 Pull Request

---

## 📄 许可证

本项目基于 MIT 许可证开源。

---

## 🙏 致谢

- 感谢 Microsoft Semantic Kernel 团队提供优秀的框架
- 本项目基于官方 Jupyter Notebook 教程改编
- 感谢所有贡献者的支持

---

**开始你的 Semantic Kernel 学习之旅吧！** 🚀

💡 **提示**: 建议从 `GettingStarted` 项目开始，按照学习路径循序渐进。每个项目都包含详细的中文注释和完整的示例代码。
