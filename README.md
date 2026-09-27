# PromptRUN

PowerToys Run 的提示词片段插件：随时 Alt+Space 唤起，搜索你手工维护的提示词库，一键复制正文。

完全独立的项目，自带数据文件，不依赖任何其他应用。

## 功能

- **搜索 + 复制**：`pp <关键词>` 多关键词 AND 匹配（标题 / 标签 / 内容，不区分大小写），回车复制完整提示词到剪贴板并弹通知
- **智能排序**：收藏优先 → 使用次数 → 匹配权重（标题 > 标签 > 内容）→ 最近更新
- **模板变量标注**：内容含 `{{变量}}` 的条目会在副标题标注 "contains unfilled variables"，复制时原样保留
- **热重载**：用任意编辑器修改数据文件，无需重启即可生效（约 300ms 防抖）
- **使用计数**：复制成功自动 `useCount +1` 写回，排序越用越准
- **管理面板**：仅输入 `pp` 显示三项动作——打开数据文件夹 / 推送（Push）到 GitHub / 拉取（Pull）
- **GitHub 私库同步**：单文件同步 `prompts.json`；推送覆盖远端，拉取覆盖本地前自动备份为 `prompts.json.bak`

## 安装

1. 关闭 PowerToys（托盘图标右键退出，或任务管理器结束 `PowerToys.exe`）
2. 下载最新 Release 的 `PromptRun.zip`，解压到 `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\PromptRun\`
3. 重新打开 PowerToys

在 PowerToys Run 中输入 `pp `（或 Alt+Space 后输入），看到管理面板即安装成功。

## 修改触发词

默认触发词为 `pp`。在 PowerToys 设置 → PowerToys Run → PromptRUN → Action Keyword 中修改（PowerToys 原生支持）。

## 数据文件

位置（首次运行自动生成含示例的模板）：

```
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\PromptRun\prompts.json
```

格式（schema v1，直接用编辑器增删改）：

```json
{
  "version": 1,
  "prompts": [
    {
      "id": "任意唯一字符串",
      "title": "显示标题",
      "content": "复制到剪贴板的完整内容",
      "tags": ["搜索用标签"],
      "favorite": false,
      "useCount": 0,
      "createdAt": 1758888888888,
      "updatedAt": 1758888888888
    }
  ]
}
```

- 缺 `title` 或 `content` 的条目会被跳过；文件损坏时插件降级为空库并提示，不会崩溃
- 时间戳为 Unix 毫秒；`id` 建议用 GUID

## GitHub 同步配置（三步）

1. 在 GitHub 创建一个**私人仓库**
2. 创建 **fine-grained personal access token**（Settings → Developer settings → Fine-grained tokens）：
   - Repository access：**仅选择上一步的仓库**
   - Permissions：**Contents → Read and write**（其他全部不勾）
3. PowerToys 设置 → PowerToys Run → PromptRUN，填入 `GitHub repo (owner/name)` 与 token

之后在 PowerToys Run 输入 `pp` 即可 Push / Pull。多机同步：各机器装好插件并配置同一仓库和 token，分别用 Push / Pull 搬运数据。

> PAT 明文保存在 PowerToys 的本地设置中——务必使用最小权限的 fine-grained token。

## 从源码构建

```powershell
dotnet build -c Release   # 需要 .NET 10 SDK（PowerToys 0.97+ 对应 .NET 8/9/10 运行时）
dotnet test               # 运行单元测试
```

产物即 `src/PromptRun/bin/Release/net10.0-windows10.0.26100.0/`（分发只需 `PromptRun.dll` + `plugin.json` + `Images/`）。

## 排错

- **输入 `pp` 没有任何结果**：确认 PowerToys Run 设置里 PromptRUN 未被禁用；确认目录名与 zip 内结构一致（`plugin.json` 必须在 `PromptRun` 文件夹根部）
- **提示 "prompts.json is corrupt"**：检查 JSON 语法，或删除该文件重新生成模板
- **同步失败**：确认 token 未过期、仓库拼写正确、token 有该仓库 Contents 读写权限

## License

MIT
