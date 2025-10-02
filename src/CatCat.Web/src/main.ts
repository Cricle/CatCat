import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

// Vuestic UI - Modern Vue 3 UI Framework
import { createVuestic } from 'vuestic-ui'
import 'vuestic-ui/styles/essential.css'
import 'vuestic-ui/styles/typography.css'

import './style.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(createVuestic({
  config: {
    colors: {
      variables: {
        primary: '#667eea',
        secondary: '#764ba2',
        success: '#07c160',
        info: '#1989fa',
        danger: '#ee0a24',
        warning: '#ff976a',
      }
    }
  }
}))

app.mount('#app')

