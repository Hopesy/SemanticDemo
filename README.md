# Semantic Kernel 学习项目

这是一个系统化的 Semantic Kernel 学习项目，包含从入门到进阶的完整示例代码。

## 📋 项目结构

```
SemanticDemo/
├── Common/                      # 共享代码和配置
│   ├── Settings.cs             # 统一的配置管理
│   └── PromptPlugins/          # 提示模板插件库（50+ 预定义插件）
├── Starts/                      # 入门教程（5个项目）
└── Concepts/                    # 核心概念（12个项目）
```

## 🚀 快速开始

### 1. 配置 API

在项目根目录创建 `appsettings.json`：

**OpenAI 配置（Chat + Embedding 同一服务）：**
```json
{
  "chatModel": {
    "model": "gpt-4o-mini",
    "endpoint": "",
    "apiKey": "your-openai-api-key",
    "orgId": ""
  },
  "embeddingModel": {
    "model": "text-embedding-ada-002",
    "endpoint": "",
    "apiKey": "your-openai-api-key",
    "orgId": "",
    "dimensions": 1536
  }
}
```

**智谱 AI 配置（推荐，Chat + Embedding 同一服务）：**
```json
{
  "chatModel": {
    "model": "glm-4-flash",
    "endpoint": "https://open.bigmodel.cn/api/paas/v4",
    "apiKey": "your-zhipu-api-key",
    "orgId": ""
  },
  "embeddingModel": {
    "model": "embedding-2",
    "endpoint": "https://open.bigmodel.cn/api/paas/v4",
    "apiKey": "your-zhipu-api-key",
    "orgId": "",
    "dimensions": 1024
  }
}
```

**DeepSeek 配置（Chat 用 DeepSeek，Embedding 用 OpenAI）：**
```json
{
  "chatModel": {
    "model": "deepseek-chat",
    "endpoint": "https://api.deepseek.com",
    "apiKey": "your-deepseek-api-key",
    "orgId": ""
  },
  "embeddingModel": {
    "model": "text-embedding-ada-002",
    "endpoint": "",
    "apiKey": "your-openai-api-key-for-embedding",
    "orgId": "",
    "dimensions": 1536
  }
}
```

更多配置示例请参考 `DEEPSEEK_CONFIG.md`。

### 2. 运行项目

```bash
# 编译所有项目
dotnet build

# 运行入门示例
cd Starts/GettingStarted
dotnet run

# 运行概念示例
cd Concepts/ChatCompletion
dotnet run
```

## 📚 学习路径

### 阶段 1：入门基础（Starts 目录）

按以下顺序学习，建议每个项目花费 15-30 分钟：

| 序号 | 项目名称 | 学习内容 | 关键概念 |
|------|---------|---------|---------|
| 1 | **GettingStarted** | 快速入门 | Kernel 创建、插件加载、基础调用 |
| 2 | **BasicKernelLoading** | Kernel 加载 | Kernel 配置、服务注册 |
| 3 | **FunctionCalling** | 函数调用 | 插件系统、自动函数调用、幻觉问题 |
| 4 | **SemanticFunctionInline** | 内联语义函数 | 提示模板、参数传递、CreateFunctionFromPrompt |
| 5 | **KernelArgumentsChat** | 参数化聊天 | KernelArguments、对话历史管理 |

**学习目标：** 掌握 Semantic Kernel 的基本使用方法，能够创建简单的 AI 应用。

---

### 阶段 2：核心概念（Concepts 目录）

#### 第一层：基础交互（必学）

| 序号 | 项目名称 | 学习内容 | 关键概念 | 难度 |
|------|---------|---------|---------|------|
| 1 | **ChatCompletion** | 聊天完成 | IChatCompletionService、ChatHistory、多轮对话 | ⭐ |
| 2 | **TextGeneration** | 文本生成 | Temperature、TopP、MaxTokens、StopSequences | ⭐ |
| 3 | **Streaming** | 流式输出 | 实时响应、StreamingChatMessageContent | ⭐⭐ |

**学习目标：** 理解 AI 模型的基本交互方式和参数控制。

---

#### 第二层：插件系统（核心）

| 序号 | 项目名称 | 学习内容 | 关键概念 | 难度 |
|------|---------|---------|---------|------|
| 4 | **Plugins** | 插件系统 | 原生插件、KernelFunction、插件导入 | ⭐⭐ |
| 5 | **FunctionCallingAdvanced** | 高级函数调用 | FunctionChoiceBehavior、Auto/Required/None | ⭐⭐⭐ |
| 6 | **PromptTemplates** | 提示模板 | Handlebars 模板、模板渲染、变量替换 | ⭐⭐ |

**学习目标：** 掌握插件开发和函数调用机制，能够扩展 AI 能力。

---

#### 第三层：高级特性（进阶）

| 序号 | 项目名称 | 学习内容 | 关键概念 | 难度 |
|------|---------|---------|---------|------|
| 7 | **DependencyInjection** | 依赖注入 | IServiceCollection、DI 容器、日志集成 | ⭐⭐ |
| 8 | **Filtering** | 过滤器 | IFunctionInvocationFilter、拦截器、重试机制 | ⭐⭐⭐ |

**学习目标：** 理解企业级应用开发模式，掌握高级架构技巧。

---

#### 第四层：智能应用（实战）

| 序号 | 项目名称 | 学习内容 | 关键概念 | 难度 |
|------|---------|---------|---------|------|
| 9 | **Memory** | 记忆系统 | Text Embedding、向量存储、语义搜索 | ⭐⭐⭐ |
| 10 | **RAG** | 检索增强生成 | 知识库、向量检索、上下文注入 | ⭐⭐⭐⭐ |
| 11 | **Search** | 搜索集成 | Web 搜索、多来源搜索、搜索增强对话 | ⭐⭐⭐ |
| 12 | **Agents** | AI 代理 | ChatCompletionAgent、多轮对话、带插件的 Agent | ⭐⭐⭐⭐ |

**学习目标：** 构建智能应用，实现 RAG、Agent 等高级 AI 模式。

---

## 🎯 推荐学习顺序

### 快速路径（2-3 天）
适合快速了解 Semantic Kernel 核心功能：

```
GettingStarted → FunctionCalling → ChatCompletion → Plugins → RAG
```

### 完整路径（1-2 周）
系统学习所有概念：

```
【入门】
GettingStarted → BasicKernelLoading → FunctionCalling
→ SemanticFunctionInline → KernelArgumentsChat

【基础交互】
ChatCompletion → TextGeneration → Streaming

【插件系统】
Plugins → FunctionCallingAdvanced → PromptTemplates

【高级特性】
DependencyInjection → Filtering

【智能应用】
Memory → RAG → Search → Agents
```

### 实战路径（针对特定场景）

**场景 1：构建聊天机器人**
```
GettingStarted → ChatCompletion → KernelArgumentsChat
→ Streaming → Plugins → Agents
```

**场景 2：构建知识库问答系统（RAG）**
```
GettingStarted → ChatCompletion → Memory → RAG → Search
```

**场景 3：构建企业级应用**
```
GettingStarted → DependencyInjection → Filtering
→ Plugins → FunctionCallingAdvanced
```

---

## 💡 学习建议

### 1. 循序渐进
- 不要跳过 Starts 目录，它们是理解后续概念的基础
- 按照推荐顺序学习，每个项目都有前置依赖

### 2. 动手实践
- 运行每个示例，观察输出结果
- 修改参数（Temperature、MaxTokens 等），观察变化
- 尝试修改提示词，理解提示工程

### 3. 理解原理
- 阅读代码注释，理解每行代码的作用
- 查看官方文档：https://learn.microsoft.com/semantic-kernel/
- 对比不同示例的实现方式

### 4. 项目实战
- 学完基础后，尝试构建自己的项目
- 从简单的聊天机器人开始
- 逐步添加插件、记忆、RAG 等功能

---

## 📖 核心概念速查

### Kernel
- **作用**：Semantic Kernel 的核心，管理 AI 服务和插件
- **创建**：`Settings.CreateKernelBuilder().Build()`
- **关键方法**：`InvokePromptAsync`、`InvokeAsync`

### Plugin（插件）
- **作用**：扩展 AI 能力，提供工具函数
- **类型**：原生插件（C# 类）、提示模板插件（文件）
- **注册**：`kernel.ImportPluginFromType<T>()`、`kernel.ImportPluginFromPromptDirectory()`

### Function Calling（函数调用）
- **作用**：让 AI 自动决定何时调用哪个函数
- **模式**：Auto（自动）、Required（必须）、None（禁用）
- **配置**：`FunctionChoiceBehavior.Auto()`

### ChatHistory（对话历史）
- **作用**：管理多轮对话的上下文
- **方法**：`AddUserMessage()`、`AddAssistantMessage()`、`AddSystemMessage()`

### Streaming（流式输出）
- **作用**：实时获取 AI 响应，提升用户体验
- **方法**：`GetStreamingChatMessageContentsAsync()`

### Memory（记忆）
- **作用**：存储和检索语义信息
- **核心**：Text Embedding（文本嵌入）、向量存储、相似度搜索

### RAG（检索增强生成）
- **作用**：结合知识库和 AI 生成，提供准确答案
- **流程**：检索相关文档 → 注入上下文 → AI 生成答案

### Agent（代理）
- **作用**：具有自主决策能力的 AI 助手
- **特点**：多轮对话、自动调用工具、任务规划

---

## 🛠️ 技术栈

- **.NET 8.0** - 运行时
- **Microsoft.SemanticKernel 1.23.0** - 核心框架
- **C# 12** - 编程语言

---

## 📦 项目特点

### 1. 统一配置管理
所有项目使用 `Settings.CreateKernelBuilder()` 统一创建 Kernel，支持：
- OpenAI (Chat + Embedding)
- 智谱 AI (Chat + Embedding) - 推荐
- DeepSeek (仅 Chat，Embedding 需配置其他服务)
- Ollama / LM Studio
- 任何兼容 OpenAI API 的服务

### 2. 丰富的插件库
`Common/PromptPlugins` 包含 50+ 预定义插件：
- **FunPlugin** - 娱乐（笑话、打油诗）
- **WriterPlugin** - 写作（邮件、翻译、改写）
- **SummarizePlugin** - 总结（摘要、主题提取）
- **ChatPlugin** - 对话机器人
- **CodingPlugin** - 代码生成
- 更多...

### 3. 完整的中文注释
每个项目都有详细的中文注释，帮助理解代码逻辑。

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

---

## 📄 许可证

MIT License

---

## 🔗 相关资源

- [Semantic Kernel 官方文档](https://learn.microsoft.com/semantic-kernel/)
- [Semantic Kernel GitHub](https://github.com/microsoft/semantic-kernel)
- [OpenAI API 文档](https://platform.openai.com/docs)
- [DeepSeek API 文档](https://platform.deepseek.com/docs)

---

**祝学习愉快！🎉**
