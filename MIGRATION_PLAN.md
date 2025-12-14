# 📋 完整迁移计划

本文档列出了所有可以从 semantic-kernel 官方仓库迁移的案例，以及迁移优先级。

## 项目状态

### ✅ 已完成 (7个)

1. **Common** - 共享配置类库
2. **00-GettingStarted** - 快速入门
3. **03-SemanticFunctionInline** - 内联语义函数
4. **04-KernelArgumentsChat** - 聊天机器人
5. **Concepts.ChatCompletion** - ChatCompletion 核心概念
6. **Concepts.Streaming** - 流式输出
7. **Concepts.Plugins** - 插件系统

### 🚧 已创建项目框架 (8个)

以下项目已创建，但代码待实现：

1. **01-BasicKernelLoading** - Kernel 基础加载
2. **05-FunctionCalling** - 函数调用
3. **Concepts.FunctionCalling** - 函数调用核心概念
4. **Concepts.PromptTemplates** - 提示模板
5. **Concepts.TextGeneration** - 文本生成
6. **Concepts.Filtering** - 过滤器
7. **Concepts.RAG** - 检索增强生成
8. **Concepts.Memory** - 记忆系统
9. **Concepts.Agents** - 代理系统
10. **Concepts.Search** - 搜索功能
11. **Concepts.DependencyInjection** - 依赖注入

### ⏳ 待创建 (推荐)

#### 高优先级 (核心功能)

1. **Concepts.ImageGeneration** - 图像生成 (DALL-E)
   - 来源: `Concepts/TextToImage/`
   - 重要性: ⭐⭐⭐⭐
   - 依赖: DALL-E API

2. **Concepts.VectorStores** - 向量存储
   - 来源: `GettingStartedWithVectorStores/`
   - 重要性: ⭐⭐⭐⭐⭐
   - 依赖: 向量数据库连接器

3. **Concepts.Embeddings** - 嵌入向量
   - 来源: `Concepts/Memory/`
   - 重要性: ⭐⭐⭐⭐⭐
   - 依赖: Embedding API

4. **Concepts.Processes** - 流程框架
   - 来源: `GettingStartedWithProcesses/`
   - 重要性: ⭐⭐⭐⭐
   - 依赖: 无

#### 中优先级 (高级功能)

5. **Concepts.AudioToText** - 语音识别
   - 来源: `Concepts/AudioToText/`
   - 重要性: ⭐⭐⭐
   - 依赖: Whisper API

6. **Concepts.TextToAudio** - 文本转语音
   - 来源: `Concepts/TextToAudio/`
   - 重要性: ⭐⭐⭐
   - 依赖: TTS API

7. **Concepts.ImageToText** - 图像理解
   - 来源: `Concepts/ImageToText/`
   - 重要性: ⭐⭐⭐⭐
   - 依赖: Vision API

8. **Concepts.Caching** - 缓存策略
   - 来源: `Concepts/Caching/`
   - 重要性: ⭐⭐⭐
   - 依赖: 无

9. **Concepts.Optimization** - 性能优化
   - 来源: `Concepts/Optimization/`
   - 重要性: ⭐⭐⭐
   - 依赖: 无

#### 低优先级 (特定场景)

10. **Concepts.HuggingFace** - HuggingFace 集成
    - 来源: `Concepts/ChatCompletion/HuggingFace_*.cs`
    - 重要性: ⭐⭐
    - 依赖: HuggingFace API

11. **Concepts.Ollama** - 本地模型运行
    - 来源: `Concepts/ChatCompletion/Ollama_*.cs`
    - 重要性: ⭐⭐⭐
    - 依赖: Ollama 安装

12. **Concepts.Onnx** - ONNX Runtime
    - 来源: `Concepts/ChatCompletion/Onnx_*.cs`
    - 重要性: ⭐⭐
    - 依赖: ONNX Runtime

---

## 详细案例列表

### 📁 Concepts/ChatCompletion (50+ 文件)

#### ✅ 已迁移
- 基础对话
- 系统消息
- 多轮对话
- 执行设置

#### ⏳ 可选迁移
- `ChatHistoryAuthorName.cs` - 作者名称
- `ChatHistorySerialization.cs` - 历史序列化
- `Connectors_WithMultipleLLMs.cs` - 多模型切换
- `HybridCompletion_Fallback.cs` - 降级策略
- `OpenAI_StructuredOutputs.cs` - 结构化输出
- `OpenAI_UsingLogitBias.cs` - Logit Bias
- `OpenAI_ChatCompletionWithVision.cs` - 视觉理解
- `OpenAI_ChatCompletionWithAudio.cs` - 音频处理
- `OpenAI_ChatCompletionWithFile.cs` - 文件处理
- `OpenAI_ChatCompletionWithReasoning.cs` - 推理模式

### 📁 Concepts/FunctionCalling (8 文件)

#### 核心案例
- `FunctionCalling.cs` - 函数调用基础 ⭐⭐⭐⭐⭐
- `FunctionCalling_ReturnMetadata.cs` - 返回元数据 ⭐⭐⭐
- `FunctionCalling_SharedState.cs` - 共享状态 ⭐⭐⭐
- `MultipleFunctionsVsParameters.cs` - 多函数 vs 多参数 ⭐⭐⭐⭐
- `ContextDependentAdvertising.cs` - 上下文相关 ⭐⭐⭐

### 📁 Concepts/Plugins (20+ 文件)

#### 核心案例
- `ApiManifestBasedPlugins.cs` - API Manifest 插件 ⭐⭐⭐⭐
- `CreatePluginFromOpenApiSpec_Github.cs` - OpenAPI 插件 ⭐⭐⭐⭐⭐
- `GroundednessChecks.cs` - 真实性检查 ⭐⭐⭐
- `ImportPluginFromGrpc.cs` - gRPC 插件 ⭐⭐⭐
- `ImportPluginFromOpenAI.cs` - OpenAI 插件 ⭐⭐⭐⭐

### 📁 Concepts/PromptTemplates (10+ 文件)

#### 核心案例
- `ChatCompletionPrompts.cs` - 聊天提示 ⭐⭐⭐⭐⭐
- `HandlebarsPrompts.cs` - Handlebars 模板 ⭐⭐⭐⭐
- `LiquidPrompts.cs` - Liquid 模板 ⭐⭐⭐
- `MultiplePromptTemplates.cs` - 多模板 ⭐⭐⭐
- `PromptFunctionsWithChatGPT.cs` - ChatGPT 提示 ⭐⭐⭐⭐

### 📁 Concepts/Memory (15+ 文件)

#### 核心案例
- `TextChunkerUsage.cs` - 文本分块 ⭐⭐⭐⭐⭐
- `TextMemoryPlugin_GeminiEmbedding.cs` - Gemini 嵌入 ⭐⭐⭐
- `TextMemoryPlugin_MultipleMemoryStore.cs` - 多存储 ⭐⭐⭐⭐
- `VectorStore_Langchain_Interop.cs` - Langchain 互操作 ⭐⭐⭐

### 📁 Concepts/RAG (10+ 文件)

#### 核心案例
- `WithFunctionCalling.cs` - 函数调用 RAG ⭐⭐⭐⭐⭐
- `WithPlugins.cs` - 插件 RAG ⭐⭐⭐⭐
- `WithTextSearch.cs` - 文本搜索 RAG ⭐⭐⭐⭐⭐

### 📁 Concepts/Agents (30+ 文件)

#### 核心案例
- `ChatCompletion_Agent.cs` - 聊天代理 ⭐⭐⭐⭐⭐
- `ChatCompletion_Streaming.cs` - 流式代理 ⭐⭐⭐⭐
- `OpenAIAssistant_Agent.cs` - OpenAI 助手 ⭐⭐⭐⭐
- `AgentCollaboration.cs` - 代理协作 ⭐⭐⭐⭐⭐
- `AgentAuthoring.cs` - 代理创作 ⭐⭐⭐⭐

### 📁 Concepts/TextGeneration (5 文件)

#### 核心案例
- `OpenAI_TextGeneration.cs` - 文本生成 ⭐⭐⭐⭐
- `OpenAI_TextGenerationStreaming.cs` - 流式生成 ⭐⭐⭐⭐

### 📁 Concepts/Search (10+ 文件)

#### 核心案例
- `BingTextSearch.cs` - Bing 搜索 ⭐⭐⭐⭐
- `GoogleTextSearch.cs` - Google 搜索 ⭐⭐⭐
- `MyAISearch.cs` - 自定义搜索 ⭐⭐⭐⭐

### 📁 Concepts/Filtering (10+ 文件)

#### 核心案例
- `AutoFunctionInvocationFiltering.cs` - 函数调用过滤 ⭐⭐⭐⭐
- `FunctionInvocationFiltering.cs` - 函数过滤 ⭐⭐⭐⭐
- `PromptRenderFiltering.cs` - 提示渲染过滤 ⭐⭐⭐
- `RetryWithFilters.cs` - 重试过滤 ⭐⭐⭐⭐

### 📁 Concepts/DependencyInjection (5 文件)

#### 核心案例
- `HttpClient_Registration.cs` - HttpClient 注册 ⭐⭐⭐⭐
- `Kernel_Building.cs` - Kernel 构建 ⭐⭐⭐⭐⭐
- `Kernel_Injecting.cs` - Kernel 注入 ⭐⭐⭐⭐

---

## 迁移指南

### 快速迁移步骤

1. **选择案例**: 从上面的列表中选择要迁移的案例
2. **创建项目**: 如果项目不存在，创建新的控制台项目
3. **复制代码**: 从原始文件复制核心代码
4. **改写代码**:
   - 移除测试框架依赖 (xUnit, ITestOutputHelper)
   - 转换为 Main 方法
   - 使用 Settings.LoadFromFile() 加载配置
   - 添加中文注释
   - 添加友好的控制台输出
5. **测试运行**: 确保代码可以正常运行
6. **更新文档**: 在 README.md 中添加说明

### 代码改写模板

```csharp
using Microsoft.SemanticKernel;
using Common;

namespace Concepts.YourTopic;

/// <summary>
/// [主题] 演示
/// [详细说明]
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== [主题] 演示 ===\n");

        try
        {
            // 加载配置
            var (useAzureOpenAI, model, azureEndpoint, apiKey, orgId) = Settings.LoadFromFile();

            // 创建 Kernel
            var builder = Kernel.CreateBuilder();
            if (useAzureOpenAI)
            {
                builder.AddAzureOpenAIChatCompletion(model, azureEndpoint, apiKey);
            }
            else
            {
                builder.AddOpenAIChatCompletion(model, apiKey, orgId);
            }
            var kernel = builder.Build();

            // ===== 示例 1 =====
            await Example1(kernel);

            // ===== 示例 2 =====
            await Example2(kernel);

            Console.WriteLine("\n✅ 所有示例完成!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ 发生错误: {ex.Message}");
        }

        Console.WriteLine("\n按任意键退出...");
        Console.ReadKey();
    }

    static async Task Example1(Kernel kernel)
    {
        Console.WriteLine("【示例 1】[标题]\n");
        // 实现代码
    }
}
```

---

## 推荐学习路径

### 阶段 1: 基础 (1-2周)
1. ✅ 00-GettingStarted
2. ✅ 03-SemanticFunctionInline
3. ✅ Concepts.ChatCompletion
4. ✅ Concepts.Streaming
5. ✅ 04-KernelArgumentsChat
6. ✅ Concepts.Plugins

### 阶段 2: 进阶 (2-3周)
7. ⏳ Concepts.FunctionCalling
8. ⏳ Concepts.PromptTemplates
9. ⏳ Concepts.TextGeneration
10. ⏳ Concepts.Filtering
11. ⏳ Concepts.DependencyInjection

### 阶段 3: 高级 (3-4周)
12. ⏳ Concepts.Memory
13. ⏳ Concepts.Embeddings
14. ⏳ Concepts.VectorStores
15. ⏳ Concepts.RAG
16. ⏳ Concepts.Search

### 阶段 4: 专家 (4+周)
17. ⏳ Concepts.Agents
18. ⏳ Concepts.Processes
19. ⏳ Concepts.ImageGeneration
20. ⏳ Concepts.AudioToText

---

## 贡献

如果你完成了某个案例的迁移，欢迎提交 PR！

**最后更新**: 2025-12-13
