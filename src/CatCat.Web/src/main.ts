import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

// Vuestic UI - 现代化 Vue 3 UI 框架
import { createVuestic } from 'vuestic-ui'
import 'vuestic-ui/css'

// 自定义主题配置
const vuesticConfig = {
  colors: {
    variables: {
      // 主色调 - 紫色渐变
      primary: '#667eea',
      secondary: '#764ba2',
      success: '#07c160',
      info: '#1989fa',
      danger: '#ee0a24',
      warning: '#ff976a',
      // 背景色
      backgroundPrimary: '#ffffff',
      backgroundSecondary: '#f7f8fa',
      backgroundElement: '#ffffff',
      backgroundBorder: '#ebedf0',
      // 文本色
      textPrimary: '#323233',
      textInverted: '#ffffff',
    }
  },
  components: {
    // 全局组件配置
    VaButton: {
      round: true, // 默认圆角按钮
    },
    VaCard: {
      stripe: false,
      square: false,
    }
  }
}

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(createVuestic(vuesticConfig))

app.mount('#app')

