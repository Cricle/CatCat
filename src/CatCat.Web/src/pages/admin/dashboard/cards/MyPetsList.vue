<template>
  <VaCard>
    <VaCardTitle>
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <VaIcon name="pets" />
          <span>{{ t('dashboard.cards.myPets') }}</span>
        </div>
        <VaButton preset="secondary" size="small" @click="router.push('/pets')">
          {{ t('dashboard.cards.viewAll') }}
        </VaButton>
      </div>
    </VaCardTitle>
    <VaCardContent>
      <div v-if="loading" class="flex justify-center py-4">
        <VaProgressCircle indeterminate size="small" />
      </div>

      <div v-else-if="pets.length === 0" class="text-center py-8 text-secondary">
        <VaIcon name="pets" size="3rem" color="secondary" />
        <p class="mt-2">{{ t('dashboard.cards.noPets') }}</p>
        <VaButton class="mt-3" size="small" @click="router.push('/pets')">
          {{ t('dashboard.cards.addPet') }}
        </VaButton>
      </div>

      <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <div
          v-for="pet in pets"
          :key="pet.id"
          class="flex items-center gap-3 p-3 rounded border border-gray-200 dark:border-gray-700 hover:border-primary cursor-pointer transition"
          @click="router.push('/pets')"
        >
          <VaAvatar :src="pet.avatar" color="primary" size="large">
            {{ pet.name?.charAt(0) }}
          </VaAvatar>
          <div class="flex-grow">
            <div class="font-semibold">{{ pet.name }}</div>
            <div class="text-sm text-secondary">
              {{ getPetTypeText(pet.type) }} · {{ pet.age }}{{ t('dashboard.cards.yearsOld') }}
            </div>
            <div v-if="pet.breed" class="text-xs text-secondary">{{ pet.breed }}</div>
          </div>
          <VaChip :color="pet.gender === 1 ? 'info' : 'danger'" size="small">
            {{ pet.gender === 1 ? '♂' : '♀' }}
          </VaChip>
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { petApi } from '../../../../services/catcat-api'
import type { Pet } from '../../../../types/catcat-types'

const { t } = useI18n()
const router = useRouter()

const loading = ref(false)
const pets = ref<Pet[]>([])

const loadPets = async () => {
  loading.value = true
  try {
    const response = await petApi.getMyPets()
    pets.value = response.data?.slice(0, 4) || []
  } catch (error) {
    console.error('Failed to load pets:', error)
  } finally {
    loading.value = false
  }
}

const getPetTypeText = (type: number) => {
  const map: Record<number, string> = {
    1: '猫咪',
    2: '狗狗',
    99: '其他',
  }
  return map[type] || '未知'
}

onMounted(() => {
  loadPets()
})
</script>

