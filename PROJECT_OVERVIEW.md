# 📊 项目概览

## 解决方案结构

```
SemanticDemo.sln
├── Common (类库)                    # 共享配置和工具类
├── 00-GettingStarted (控制台)       # ✅ 已完成
├── 01-BasicKernelLoading (控制台)   # ⏳ 待实现
├── 03-SemanticFunctionInline (控制台) # ✅ 已完成
├── 04-KernelArgumentsChat (控制台)  # ✅ 已完成
└── 05-FunctionCalling (控制台)      # ⏳ 待实现
```

## 已实现的项目

### ✅ Common - 共享类库
**功能**:
- `Settings.cs`: 配置文件加载
- 自动向上查找 `appsettings.json`
- 支持 OpenAI 和 Azure OpenAI

**关键方法**:
```csharp
var (useAzureOpenAI, model, endpoint, apiKey, orgId) = Settings.LoadFromFile();
```

---

### ✅ 00-GettingStarted - 快速入门
**学习目标**: 5 分钟了解 Semantic Kernel 基本流程

**演示内容**:
1. 加载配置
2. 创建 Kernel
3. 加载插件（可选）
4. 直接调用提示

**代码亮点**:
- 完整的错误处理
- 友好的控制台输出
- 自动查找插件目录

**运行效果**:
```
=== Semantic Kernel 快速入门 ===

步骤 1: 加载配置...
使用 OpenAI 服务
模型: gpt-4o-mini

步骤 2: 创建 Kernel...
Kernel 创建成功!

额外演示: 直接调用提示...
问题: 用一句话介绍什么是 Semantic Kernel
回答: Semantic Kernel 是微软开发的开源 SDK...

✅ 快速入门完成!
```

---

### ✅ 03-SemanticFunctionInline - 内联语义函数
**学习目标**: 掌握提示模板和函数创建

**演示内容**:
1. **示例 1**: 文本总结
   - 使用 `CreateFunctionFromPrompt`
   - 配置执行设置 (Temperature, MaxTokens)

2. **示例 2**: 简化调用
   - 使用 `InvokePromptAsync`
   - 5 词总结演示

3. **示例 3**: 多参数函数
   - 语言翻译示例
   - `KernelArguments` 使用

4. **示例 4**: 创意生成
   - 诗歌创作
   - 主题和风格参数

**代码亮点**:
- 4 个完整的实用示例
- 清晰的注释和说明
- 渐进式学习曲线

**关键技术**:
```csharp
// 提示模板
string prompt = """
{{$input}}
请总结上面的内容。
""";

// 执行设置
var settings = new OpenAIPromptExecutionSettings
{
    MaxTokens = 2000,
    Temperature = 0.2
};

// 调用
var result = await kernel.InvokePromptAsync(prompt, new() { ["input"] = text });
```

---

### ✅ 04-KernelArgumentsChat - 聊天机器人
**学习目标**: 理解对话历史管理和上下文保持

**演示内容**:
1. **预设演示对话**
   - 4 轮自动对话
   - 展示上下文保持能力
   - 彩色控制台输出

2. **交互式聊天**
   - 用户可以自由输入
   - 实时对话
   - 输入 'exit' 退出

3. **历史记录显示**
   - 完整对话历史
   - 格式化输出

**代码亮点**:
- 真实的聊天体验
- 彩色输出（用户=绿色，机器人=青色）
- 完整的对话历史管理

**关键技术**:
```csharp
// 对话历史管理
var history = "";
var arguments = new KernelArguments
{
    ["history"] = history,
    ["userInput"] = userInput
};

// 更新历史
history += $"\n用户: {userInput}\nChatBot: {answer}\n";
arguments["history"] = history;
```

**运行效果**:
```
=== Kernel 参数聊天演示 ===

---开始对话---

用户: 你好，我想找一些关于中国历史的书籍推荐
ChatBot: 我推荐《中国通史》...

用户: 我对唐朝特别感兴趣，有什么推荐吗？
ChatBot: 针对唐朝，我推荐《唐朝那些事儿》...

---演示对话结束---

现在你可以自己与机器人对话了！

你: _
```

---

## 待实现的项目

### ⏳ 01-BasicKernelLoading
**计划内容**:
- Kernel Builder 详解
- 多后端配置
- 日志集成
- 服务选择器

### ⏳ 05-FunctionCalling
**计划内容**:
- 自动函数调用
- 插件系统
- ChatHistory 跟踪
- 实际业务场景演示

### ⏳ 02-RunningPromptsFromFile
**计划内容**:
- 从文件加载提示
- 插件目录结构
- config.json 配置

### ⏳ 06-VectorStoresAndEmbeddings
**计划内容**:
- 向量存储
- 嵌入生成
- 语义搜索
- RAG 基础

---

## 技术栈

- **.NET 8.0**: 目标框架
- **Microsoft.SemanticKernel 1.23.0**: 核心 SDK
- **C# 12**: 语言版本
- **控制台应用**: 项目类型

## 设计原则

1. **简单易懂**: 每个示例专注一个核心概念
2. **完整可运行**: 所有代码都可以直接运行
3. **中文注释**: 详细的中文说明和注释
4. **渐进式学习**: 从简单到复杂的学习路径
5. **实用性**: 展示真实的应用场景

## 代码规范

- ✅ 使用 `async/await` 异步编程
- ✅ 完整的异常处理
- ✅ 友好的控制台输出
- ✅ 清晰的代码注释
- ✅ 符合 C# 命名规范

## 下一步计划

1. 完善 `01-BasicKernelLoading` 项目
2. 实现 `05-FunctionCalling` 项目
3. 添加 `06-VectorStoresAndEmbeddings` 项目
4. 创建更多高级示例
5. 添加单元测试

---

**当前进度**: 4/7 项目已完成 (57%)
