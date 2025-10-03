<template>
  <div class="flex items-center justify-between">
    <p>{{ t('settings.language') }}</p>
    <div class="w-40">
      <VaSelect v-model="model" :options="options" />
    </div>
  </div>
</template>
<script lang="ts" setup>
import { computed } from 'vue'

import { useI18n } from 'vue-i18n'

type LanguageMap = Record<string, string>

const { locale, t } = useI18n()

const languages: LanguageMap = {
  english: 'English (英语)',
  spanish: 'Español (西班牙语)',
  brazilian_portuguese: 'Português (葡萄牙语)',
  simplified_chinese: '简体中文',
  persian: 'فارسی (波斯语)',
}

const languageCodes: LanguageMap = {
  gb: languages.english,
  es: languages.spanish,
  br: languages.brazilian_portuguese,
  cn: languages.simplified_chinese,
  ir: languages.persian,
}

const languageName: LanguageMap = Object.fromEntries(Object.entries(languageCodes).map(([key, value]) => [value, key]))

const options = Object.values(languageCodes)

const model = computed({
  get() {
    return languageCodes[locale.value]
  },
  set(value) {
    locale.value = languageName[value]
  },
})
</script>
