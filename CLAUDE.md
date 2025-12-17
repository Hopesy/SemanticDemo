# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

这是一个 **Semantic Kernel 学习项目**,包含从入门到进阶的完整示例代码。项目采用 .NET 8.0 和 Microsoft.SemanticKernel 1.68.0 构建,展示了如何使用 Semantic Kernel 框架构建 AI 应用。该项目是从官方项目`C:\Users\zhouh\Desktop\semantic-kernel\dotnet\samples`移植过来的，API用法你参考他的案例

## 架构设计

### 项目结构

项目采用三层组织结构:

1. **Common/** - 共享基础设施层
   - `Settings.cs`: 统一的 AI 服务配置管理(支持 OpenAI、DeepSeek 等兼容 OpenAI API 的服务)
   - `PromptPlugins/`: 50+ 预定义的提示模板插件库,按功能分类(FunPlugin、WriterPlugin、SummarizePlugin、ChatPlugin、CodingPlugin 等)
   - `Plugins/`: 原生 C# 插件(如果存在)

2. **Starts/** - 入门教程项目(5个)
   - GettingStarted: Kernel 创建与基础调用
   - BasicKernelLoading: Kernel 配置与服务注册
   - FunctionCalling: 插件系统与自动函数调用
   - SemanticFunctionInline: 内联语义函数与提示模板
   - KernelArgumentsChat: 参数化聊天与对话历史管理

3. **Concepts/** - 核心概念项目(12个)
   - **基础交互**: ChatCompletion、TextGeneration、Streaming
   - **插件系统**: Plugins、FunctionCallingAdvanced、PromptTemplates
   - **高级特性**: DependencyInjection、Filtering
   - **智能应用**: Memory、RAG、Search、Agents

4. **Advances/** - 高级应用示例
   - OrderProcessWorkflow: 工作流示例

### 统一配置系统

所有项目通过 `Common/Settings.cs` 统一管理 AI 服务配置:

- **配置文件位置**: `Common/appsettings.json` (项目引用后会复制到各自的输出目录)
- **支持的服务**: OpenAI、智谱 AI、DeepSeek、Ollama、LM Studio 等所有兼容 OpenAI API 的服务
- **配置格式**:
  ```json
  {
    "chatModel": {
      "model": "Chat 模型名称",
      "endpoint": "Chat 服务端点(可选)",
      "apiKey": "Chat 服务 API 密钥",
      "orgId": "组织 ID(可选)"
    },
    "embeddingModel": {
      "model": "Embedding 模型名称",
      "endpoint": "Embedding 服务端点(可选)",
      "apiKey": "Embedding 服务 API 密钥",
      "orgId": "组织 ID(可选)",
      "dimensions": 向量维度(可选，默认1536)
    }
  }
  ```

- **配置说明**:
  - `chatModel` 节点（必需）: 聊天模型配置
  - `embeddingModel` 节点（可选）: 向量模型配置
  - 如果不配置 `embeddingModel` 节点，将使用 `chatModel` 的配置
  - 支持 Chat 和 Embedding 使用不同的服务（如 Chat 用 DeepSeek，Embedding 用 OpenAI）

- **核心方法**:
  - `Settings.CreateKernelBuilder()`: 创建配置好的 Kernel Builder (仅聊天完成)
  - `Settings.CreateKernelBuilderWithEmbedding()`: 创建包含 Embedding 服务的 Kernel Builder (用于 Memory/RAG)
  - `Settings.CreateEmbeddingGenerator()`: 创建最新的 IEmbeddingGenerator API

### 插件系统

项目包含两类插件:

1. **提示模板插件** (存储在 `Common/PromptPlugins/`)
   - 结构: `{PluginName}/{FunctionName}/config.json` + `skprompt.txt`
   - 主要插件分类:
     - **FunPlugin**: 娱乐功能(笑话、打油诗、借口)
     - **WriterPlugin**: 写作助手(邮件生成、翻译、改写、头脑风暴等)
     - **SummarizePlugin**: 文本摘要(总结、主题提取)
     - **ChatPlugin**: 对话机器人
     - **CodingPlugin**: 代码生成(Python、DOS 脚本、实体类等)
     - **QAPlugin**: 问答系统
     - **ClassificationPlugin**: 文本分类
     - **GroundingPlugin**: 实体提取与验证
     - **IntentDetectionPlugin**: 意图识别
     - **CalendarPlugin**: 日历事件
   - 加载方式: `kernel.ImportPluginFromPromptDirectory(Path.Combine(repoFiles, "PromptPlugins", "PluginName"))`

2. **原生 C# 插件**
   - 使用 `[KernelFunction]` 特性标注方法
   - 加载方式: `kernel.ImportPluginFromType<T>()`

## 常用开发命令

### 构建与运行

```bash
# 构建整个解决方案
dotnet build

# 构建特定项目
dotnet build Starts/GettingStarted/GettingStarted.csproj

# 清理构建输出
dotnet clean

# 还原 NuGet 包
dotnet restore

# 运行特定项目 (在项目目录下)
cd Starts/GettingStarted
dotnet run

# 或从根目录运行
dotnet run --project Starts/GettingStarted/GettingStarted.csproj

# 运行测试 (如果有)
dotnet test
```

### 管理解决方案

```bash
# 列出解决方案中的所有项目
dotnet sln list

# 添加项目到解决方案
dotnet sln add <ProjectPath>

# 从解决方案移除项目
dotnet sln remove <ProjectPath>
```

### NuGet 包管理

```bash
# 列出项目中的所有包
dotnet list package

# 列出过时的包
dotnet list package --outdated

# 添加包到项目
dotnet add package <PackageName>

# 更新包到最新版本
dotnet add package <PackageName> --version <Version>
```

## 关键依赖项

- **Microsoft.SemanticKernel**: 1.68.0 - 核心框架
- **Microsoft.SemanticKernel.Agents.Core**: 1.23.0-alpha - Agent 功能(仅 Agents 项目)
- **Microsoft.Extensions.AI**: 10.1.1 - 最新的 AI 抽象层
- **OpenAI**: 2.8.0 - OpenAI 客户端

所有示例项目都引用 `Common` 项目以获得统一的配置管理。

## 开发注意事项

### 配置文件管理

1. **首次使用**: 在 `Common/` 目录下创建 `appsettings.json` 文件,填入你的 API 配置
2. **配置查找逻辑**: `Settings.cs` 会自动向上查找 `Common/appsettings.json`,无需在每个项目中单独配置
3. **不要提交敏感信息**: `appsettings.json` 应包含在 `.gitignore` 中

### 学习路径建议

1. **入门路径** (按顺序):
   - GettingStarted → BasicKernelLoading → FunctionCalling → SemanticFunctionInline → KernelArgumentsChat

2. **核心概念** (分层学习):
   - 第一层: ChatCompletion → TextGeneration → Streaming
   - 第二层: Plugins → FunctionCallingAdvanced → PromptTemplates
   - 第三层: DependencyInjection → Filtering
   - 第四层: Memory → RAG → Search → Agents

### 调试技巧

1. **查看完整配置路径**: 在 `Settings.FindConfigFile()` 中添加 Console.WriteLine 查看配置文件查找过程
2. **查看 Kernel 日志**: 使用 `builder.Services.AddLogging()` 启用日志
3. **查看函数调用**: 在 FunctionChoiceBehavior.Auto() 模式下,可以通过响应元数据查看 AI 选择了哪些函数

### 实验性功能警告

部分项目使用了实验性 API,已通过 `<NoWarn>` 标签抑制警告:
- `SKEXP0001`: 实验性 Kernel 功能
- `SKEXP0010`: 实验性服务配置
- `SKEXP0110`: 实验性 Agent 功能

这些警告在生产环境中需要重新评估。

## 扩展与定制

### 添加新的提示模板插件

1. 在 `Common/PromptPlugins/` 下创建新的插件目录
2. 创建函数子目录,包含 `config.json` 和 `skprompt.txt`
3. 在示例项目中使用 `kernel.ImportPluginFromPromptDirectory()` 加载

### 添加新的原生插件

1. 在 `Common/Plugins/` 下创建新的 C# 类
2. 使用 `[KernelFunction]` 标注公共方法
3. 在示例项目中使用 `kernel.ImportPluginFromType<T>()` 加载

### 添加新的示例项目

1. 在 `Starts/` 或 `Concepts/` 下创建新项目
2. 添加对 `Common/Common.csproj` 的项目引用
3. 添加 `Microsoft.SemanticKernel` NuGet 包引用
4. 使用 `dotnet sln add` 将项目添加到解决方案

## 故障排查

### 配置文件未找到

**错误**: `FileNotFoundException: 未找到配置文件 appsettings.json`

**解决**:
1. 确认 `Common/appsettings.json` 文件存在
2. 确认文件 Build Action 设置为 `Content` 且 `CopyToOutputDirectory` 设置为 `Always` (在 Common.csproj 中已配置)
3. 运行 `dotnet clean` 后重新 `dotnet build`

### API 调用失败

**错误**: `HttpRequestException` 或 `401 Unauthorized`

**解决**:
1. 检查 `appsettings.json` 中的 `apikey` 是否正确
2. 对于 DeepSeek 等第三方服务,确认 `endpoint` 字段配置正确
3. 确认 `type` 字段设置为 `openai`

### 函数调用失败

**错误**: AI 未能正确调用插件函数

**解决**:
1. 确认插件已正确导入到 Kernel
2. 确认函数描述清晰(在 `config.json` 的 `description` 字段或 `[KernelFunction]` 的 `Description` 参数)
3. 确认使用了 `FunctionChoiceBehavior.Auto()` 启用自动函数调用
4. 尝试使用更强大的模型(如 GPT-4)以提高函数调用准确性
