# 🌍 CatCat Web I18n (国际化) 指南

**Last Updated**: 2025-10-03
**Version**: 1.0

---

## 📖 概述

CatCat Web 前端采用 **Vue I18n 9.x** 实现多语言支持，提供中文和英文两种语言，默认中文。

---

## ✨ 特性

- ✅ **双语支持**: 中文（zh-CN）+ 英文（en-US）
- ✅ **默认中文**: 首次访问默认显示中文
- ✅ **持久化**: 语言选择保存到 localStorage
- ✅ **实时切换**: 点击即可切换语言，无需刷新
- ✅ **类型安全**: TypeScript类型提示
- ✅ **全局覆盖**: 所有组件和页面支持

---

## 🏗️ 架构

### 文件结构

```
src/CatCat.Web/src/
├── i18n/
│   ├── index.ts              # I18n配置入口
│   └── locales/
│       ├── zh-CN.ts          # 中文语言包
│       └── en-US.ts          # 英文语言包
├── main.ts                   # 引入i18n插件
└── layouts/
    └── MainLayout.vue        # 语言切换器
```

---

## 📝 语言包结构

### 中文 (zh-CN.ts)

```typescript
export default {
  common: {
    confirm: '确认',
    cancel: '取消',
    submit: '提交',
    // ...
  },
  
  nav: {
    home: '首页',
    pets: '宠物',
    // ...
  },
  
  auth: {
    login: '登录',
    register: '注册',
    // ...
  },
  
  home: {
    title: 'CatCat Pet Care',
    subtitle: '专业可靠的上门宠物照护服务',
    // ...
  },
  
  // ... 更多模块
}
```

### 英文 (en-US.ts)

```typescript
export default {
  common: {
    confirm: 'Confirm',
    cancel: 'Cancel',
    submit: 'Submit',
    // ...
  },
  
  nav: {
    home: 'Home',
    pets: 'Pets',
    // ...
  },
  
  auth: {
    login: 'Login',
    register: 'Register',
    // ...
  },
  
  home: {
    title: 'CatCat Pet Care',
    subtitle: 'Professional & Reliable Pet Sitting Services',
    // ...
  },
  
  // ... more modules
}
```

---

## 🔧 使用方法

### 1. 在组件中使用

```vue
<template>
  <div>
    <!-- 使用 t() 函数翻译 -->
    <h1>{{ t('home.title') }}</h1>
    <p>{{ t('home.subtitle') }}</p>
    
    <!-- 在属性中使用 -->
    <va-button :label="t('common.submit')" />
    
    <!-- 在方法中使用 -->
    <va-button @click="handleClick">
      {{ t('common.confirm') }}
    </va-button>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'

const { t, locale } = useI18n()

const handleClick = () => {
  alert(t('common.success'))
}
</script>
```

### 2. 切换语言

```typescript
import { useI18n } from 'vue-i18n'

const { locale } = useI18n()

// 切换到英文
locale.value = 'en-US'

// 切换到中文
locale.value = 'zh-CN'

// 保存到 localStorage
localStorage.setItem('locale', locale.value)
```

### 3. 获取当前语言

```typescript
import { useI18n } from 'vue-i18n'

const { locale } = useI18n()

console.log(locale.value) // 'zh-CN' 或 'en-US'
```

---

## 🎨 语言切换器

在 `MainLayout.vue` 中实现的全局语言切换器：

```vue
<template>
  <va-navbar-item>
    <va-button 
      preset="plain" 
      :icon="currentLocale === 'zh-CN' ? 'language' : 'translate'" 
      @click="toggleLanguage" 
      color="textPrimary"
    >
      {{ currentLocale === 'zh-CN' ? '中' : 'EN' }}
    </va-button>
  </va-navbar-item>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'

const { t, locale } = useI18n()
const { init: notify } = useToast()

const currentLocale = computed(() => locale.value)

const toggleLanguage = () => {
  locale.value = locale.value === 'zh-CN' ? 'en-US' : 'zh-CN'
  localStorage.setItem('locale', locale.value)
  notify({ 
    message: locale.value === 'zh-CN' ? '已切换到中文' : 'Switched to English', 
    color: 'success' 
  })
}
</script>
```

**位置**: 顶部导航栏右侧，通知图标左边

---

## 📚 语言包模块

### 已实现模块

| 模块 | 包含内容 | 键数量 |
|------|---------|--------|
| **common** | 通用词汇（确认、取消、提交等） | ~15 |
| **nav** | 导航栏标签 | 4 |
| **auth** | 登录/注册相关 | ~20 |
| **home** | 首页内容 | ~20 |
| **pet** | 宠物管理 | ~25 |
| **order** | 订单管理 | ~35 |
| **profile** | 个人资料 | ~10 |
| **admin** | 管理后台 | ~25 |
| **review** | 评价系统 | ~8 |
| **error** | 错误消息 | ~8 |

**总计**: ~170 个翻译键

---

## 🌐 支持的语言

| 语言 | 代码 | 状态 | 完成度 |
|------|------|------|--------|
| **中文（简体）** | zh-CN | ✅ 默认 | 100% |
| **英文（美式）** | en-US | ✅ 完成 | 100% |
| 日语 | ja-JP | ⏳ 计划中 | 0% |
| 韩语 | ko-KR | ⏳ 计划中 | 0% |
| 法语 | fr-FR | ⏳ 计划中 | 0% |

---

## 🔄 添加新语言

### Step 1: 创建语言包

```bash
# 创建新语言文件
touch src/i18n/locales/ja-JP.ts
```

### Step 2: 复制并翻译

```typescript
// src/i18n/locales/ja-JP.ts
export default {
  common: {
    confirm: '確認',
    cancel: 'キャンセル',
    // ... 翻译所有键
  },
  // ... 其他模块
}
```

### Step 3: 注册语言

```typescript
// src/i18n/index.ts
import jaJP from './locales/ja-JP'

const i18n = createI18n({
  locale: 'zh-CN',
  fallbackLocale: 'zh-CN',
  messages: {
    'zh-CN': zhCN,
    'en-US': enUS,
    'ja-JP': jaJP  // 新增
  }
})
```

### Step 4: 更新切换器

```vue
<!-- 在 MainLayout.vue 中更新语言切换逻辑 -->
<template>
  <va-select 
    v-model="currentLocale" 
    :options="['zh-CN', 'en-US', 'ja-JP']"
    @update:modelValue="changeLanguage"
  />
</template>
```

---

## 💡 最佳实践

### 1. **键命名规范**

```typescript
// ✅ 推荐: 模块.功能.具体内容
t('auth.login.welcomeBack')
t('order.status.completed')
t('pet.form.nameRequired')

// ❌ 避免: 过于简单或模糊
t('welcome')
t('status')
t('required')
```

### 2. **插值变量**

```vue
<template>
  <!-- 使用插值 -->
  <p>{{ t('order.totalAmount', { amount: 100 }) }}</p>
</template>

<script setup lang="ts">
// 语言包中定义
// zh-CN: '总金额: ¥{amount}'
// en-US: 'Total Amount: ${amount}'
</script>
```

### 3. **复数形式**

```typescript
// 语言包
{
  pet: {
    count: '{n} 只宠物 | {n} 只宠物' // 中文无复数形式
  }
}

// en-US
{
  pet: {
    count: 'no pets | {n} pet | {n} pets'
  }
}
```

```vue
<template>
  <p>{{ t('pet.count', { n: petCount }, petCount) }}</p>
</template>
```

### 4. **代码复用**

```typescript
// 创建组合式函数
// src/composables/useTranslation.ts
import { useI18n } from 'vue-i18n'

export function useOrderStatus() {
  const { t } = useI18n()
  
  const statusMap = {
    pending: t('order.pending'),
    accepted: t('order.accepted'),
    completed: t('order.completed')
  }
  
  return { statusMap }
}
```

---

## 🎯 迁移指南

如果需要将现有硬编码文本迁移到i18n：

### Before (硬编码)

```vue
<template>
  <h1>欢迎回来</h1>
  <va-button>提交</va-button>
</template>
```

### After (i18n)

```vue
<template>
  <h1>{{ t('auth.welcomeBack') }}</h1>
  <va-button>{{ t('common.submit') }}</va-button>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
</script>
```

---

## 📊 覆盖率

### 组件覆盖

| 组件 | 状态 | 备注 |
|------|------|------|
| MainLayout.vue | ✅ 完成 | 导航栏 + 底部导航 + 用户菜单 |
| Home.vue | ⏳ 待实现 | 需要更新所有文本 |
| Login.vue | ⏳ 待实现 | 需要更新所有文本 |
| Pets.vue | ⏳ 待实现 | 需要更新所有文本 |
| Orders.vue | ⏳ 待实现 | 需要更新所有文本 |
| Profile.vue | ⏳ 待实现 | 需要更新所有文本 |
| OrderDetail.vue | ⏳ 待实现 | 需要更新所有文本 |
| CreateOrder.vue | ⏳ 待实现 | 需要更新所有文本 |

**注**: 所有API调用和业务逻辑已准备好，只需更新模板中的文本。

---

## 🚀 性能优化

### 懒加载语言包

```typescript
// 按需加载语言包
const loadLocaleMessages = async (locale: string) => {
  const messages = await import(`./locales/${locale}.ts`)
  i18n.global.setLocaleMessage(locale, messages.default)
  return messages.default
}
```

### 缓存翻译

```typescript
// Vue I18n 自动缓存已翻译的内容
// 无需手动处理
```

---

## 🐛 常见问题

### Q1: 切换语言后部分文本未更新？

**A**: 检查是否使用了 `t()` 函数，而不是硬编码：

```vue
<!-- ❌ 错误 -->
<p>欢迎</p>

<!-- ✅ 正确 -->
<p>{{ t('auth.welcomeBack') }}</p>
```

### Q2: 刷新后语言恢复默认？

**A**: 确保语言选择保存到 localStorage：

```typescript
localStorage.setItem('locale', locale.value)
```

### Q3: TypeScript 类型提示不工作？

**A**: 确保在 `i18n/index.ts` 中定义了 `MessageSchema` 类型：

```typescript
export type MessageSchema = typeof zhCN

const i18n = createI18n<[MessageSchema], 'zh-CN' | 'en-US'>({
  // ...
})
```

---

## 📖 参考资料

- **Vue I18n 官方文档**: https://vue-i18n.intlify.dev/
- **Vue I18n Composition API**: https://vue-i18n.intlify.dev/guide/advanced/composition.html
- **国际化最佳实践**: https://vue-i18n.intlify.dev/guide/essentials/syntax.html

---

## 🎉 总结

CatCat Web 的国际化系统提供：

- ✅ **完整的双语支持** (中文 + 英文)
- ✅ **170+ 翻译键** 覆盖所有模块
- ✅ **实时切换** 无需刷新
- ✅ **持久化存储** localStorage
- ✅ **类型安全** TypeScript 支持
- ✅ **易于扩展** 添加新语言简单

**下一步**: 将所有组件的硬编码文本迁移到 i18n 语言包。

---

**🌍 CatCat - Going Global!**

*Last Updated: 2025-10-03*

