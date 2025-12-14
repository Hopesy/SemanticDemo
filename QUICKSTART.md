# 🚀 快速开始指南

## 第一步：配置 API 密钥

### 选项 A: 使用 OpenAI

1. 复制示例配置文件：
```bash
copy appsettings.openai.json appsettings.json
```

2. 编辑 `appsettings.json`，填入你的 API 密钥：
```json
{
  "type": "openai",
  "model": "gpt-4o-mini",
  "apikey": "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "orgId": ""
}
```

### 选项 B: 使用 Azure OpenAI

1. 复制示例配置文件：
```bash
copy appsettings.azure.json appsettings.json
```

2. 编辑 `appsettings.json`，填入你的配置：
```json
{
  "type": "azure",
  "model": "gpt-4o-mini",
  "endpoint": "https://your-resource.openai.azure.com/",
  "apikey": "your-azure-key",
  "orgId": ""
}
```

## 第二步：运行第一个示例

### 使用 Visual Studio

1. 双击打开 `SemanticDemo.sln`
2. 在解决方案资源管理器中，右键点击 `00-GettingStarted` 项目
3. 选择"设为启动项目"
4. 按 `F5` 运行

### 使用命令行

```bash
cd 00-GettingStarted
dotnet run
```

### 使用 VS Code

1. 打开 `SemanticDemo` 文件夹
2. 按 `F5`，选择 `00-GettingStarted` 项目
3. 或在终端运行：
```bash
cd 00-GettingStarted
dotnet run
```

## 第三步：探索其他示例

按照学习路径依次运行：

```bash
# 1. 快速入门
cd 00-GettingStarted && dotnet run

# 2. 内联语义函数（推荐）
cd ../03-SemanticFunctionInline && dotnet run

# 3. 聊天机器人
cd ../04-KernelArgumentsChat && dotnet run

# 4. 函数调用
cd ../05-FunctionCalling && dotnet run
```

## 常见问题

### ❌ 找不到配置文件

**错误信息**:
```
未找到配置文件 appsettings.json
```

**解决方法**:
确保在 `SemanticDemo` 根目录（与 `.sln` 文件同级）创建了 `appsettings.json`

### ❌ API 调用失败

**错误信息**:
```
401 Unauthorized
```

**解决方法**:
1. 检查 API 密钥是否正确
2. 确认 API 密钥有足够的配额
3. 对于 Azure OpenAI，确认部署名称正确

### ⚠️ 找不到插件目录

**警告信息**:
```
警告: 未找到 FunPlugin 目录
```

**解决方法**:
这不影响大部分示例运行。如果需要插件功能，可以：
1. 从 Semantic Kernel 仓库复制 `prompt_template_samples` 目录
2. 或者跳过插件演示，使用内联提示

## 下一步

- 阅读 [README.md](README.md) 了解详细的项目结构
- 查看每个项目的代码和注释
- 尝试修改提示和参数，观察不同的效果
- 探索更多 Semantic Kernel 功能

## 获取帮助

- 官方文档: https://learn.microsoft.com/semantic-kernel/
- GitHub 仓库: https://github.com/microsoft/semantic-kernel
- 示例代码: https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples

---

**祝学习愉快！** 🎉
