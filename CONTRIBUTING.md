# 贡献指南

感谢您考虑为 CatCat 项目做出贡献！

## 行为准则

本项目采用友好和包容的行为准则。参与本项目即表示您同意遵守此准则。

## 如何贡献

### 报告Bug

如果您发现了bug，请创建一个Issue并包含以下信息：

- **简短的描述性标题**
- **复现步骤**
- **预期行为**
- **实际行为**
- **环境信息**（操作系统、.NET版本、Node版本等）
- **截图**（如果适用）

### 提出新功能

如果您有新功能的想法：

1. 先搜索现有的Issues，确保没有重复
2. 创建新Issue，描述：
   - 功能的用例和价值
   - 可能的实现方案
   - 是否愿意自己实现

### 提交代码

#### 开发流程

1. **Fork项目**
   ```bash
   # 在GitHub上Fork项目
   git clone https://github.com/your-username/CatCat.git
   cd CatCat
   ```

2. **创建分支**
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   ```

3. **开发和测试**
   - 编写代码
   - 添加测试
   - 确保所有测试通过
   - 遵循代码规范

4. **提交更改**
   ```bash
   git add .
   git commit -m "feat: add new feature"
   ```

   提交信息格式：
   - `feat:` 新功能
   - `fix:` Bug修复
   - `docs:` 文档更新
   - `style:` 代码格式（不影响代码运行）
   - `refactor:` 重构
   - `test:` 添加测试
   - `chore:` 构建过程或辅助工具的变动

5. **推送到Fork**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **创建Pull Request**
   - 在GitHub上创建PR
   - 填写PR模板
   - 等待代码审查

#### 代码规范

**C# 代码**
- 遵循微软C#编码规范
- 使用async/await处理异步操作
- 适当添加注释和XML文档
- 变量和方法使用有意义的名称

**TypeScript/Vue代码**
- 使用ESLint推荐配置
- 组件使用PascalCase命名
- Props和events要有类型定义
- 添加必要的注释

**数据库**
- 使用下划线命名（snake_case）
- 为外键添加索引
- 添加合适的约束

#### 测试要求

- 新功能必须包含单元测试
- Bug修复需要添加回归测试
- 确保所有测试通过
- 测试覆盖率应保持在80%以上

```bash
# 运行后端测试
dotnet test

# 运行前端测试
npm run test
```

### 文档贡献

文档同样重要！您可以：

- 修正文档中的错误
- 改进现有文档的清晰度
- 添加示例和教程
- 翻译文档

## Pull Request流程

1. **确保PR只做一件事**
   - 一个PR解决一个问题
   - 如果是大功能，考虑拆分成多个PR

2. **更新文档**
   - API变更要更新API文档
   - 新功能要更新README

3. **测试**
   - 本地测试通过
   - CI/CD测试通过

4. **代码审查**
   - 响应审查意见
   - 进行必要的修改
   - 保持PR的可合并状态

5. **合并**
   - 至少一位维护者批准后合并
   - 使用Squash merge保持提交历史清晰

## 开发环境设置

详见 [QUICK_START.md](docs/QUICK_START.md)

## 问题和讨论

- **Bug报告**: 使用GitHub Issues
- **功能讨论**: 使用GitHub Discussions
- **技术问题**: 可以在Issues中提问

## 许可证

贡献的代码将采用与项目相同的MIT许可证。

## 联系方式

- **邮箱**: your-email@example.com
- **GitHub**: @your-username

---

再次感谢您的贡献！ 🐱

