<template>
  <VaCard gradient stripe stripe-color="success">
    <VaCardContent>
      <div class="flex items-center gap-4 text-white">
        <VaIcon name="pets" size="3rem" />
        <div class="flex-grow">
          <div class="text-3xl font-bold">{{ stats.total }}</div>
          <div class="text-sm opacity-80">{{ t('dashboard.cards.totalPets') }}</div>
        </div>
      </div>

      <div class="mt-4 space-y-2 text-white">
        <div class="flex justify-between items-center bg-white/20 rounded p-2">
          <div class="flex items-center gap-2">
            <VaIcon name="pets" size="small" />
            <span class="text-sm">{{ t('dashboard.cards.cats') }}</span>
          </div>
          <span class="font-semibold">{{ stats.cats }}</span>
        </div>
        <div class="flex justify-between items-center bg-white/20 rounded p-2">
          <div class="flex items-center gap-2">
            <VaIcon name="cruelty_free" size="small" />
            <span class="text-sm">{{ t('dashboard.cards.dogs') }}</span>
          </div>
          <span class="font-semibold">{{ stats.dogs }}</span>
        </div>
        <div class="flex justify-between items-center bg-white/20 rounded p-2">
          <div class="flex items-center gap-2">
            <VaIcon name="favorite" size="small" />
            <span class="text-sm">{{ t('dashboard.cards.avgAge') }}</span>
          </div>
          <span class="font-semibold">{{ stats.avgAge.toFixed(1) }} {{ t('dashboard.cards.years') }}</span>
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { petApi } from '../../../../services/catcat-api'

const { t } = useI18n()

const stats = ref({
  total: 0,
  cats: 0,
  dogs: 0,
  avgAge: 0,
})

const loadStats = async () => {
  try {
    const response = await petApi.getMyPets()
    const pets = response.data || []

    stats.value.total = pets.length
    stats.value.cats = pets.filter((p) => p.type === 1).length
    stats.value.dogs = pets.filter((p) => p.type === 2).length

    if (pets.length > 0) {
      const totalAge = pets.reduce((sum, p) => sum + (p.age || 0), 0)
      stats.value.avgAge = totalAge / pets.length
    }
  } catch (error) {
    console.error('Failed to load pet stats:', error)
  }
}

onMounted(() => {
  loadStats()
})
</script>

