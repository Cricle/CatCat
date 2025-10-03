<template>
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <div v-else-if="!provider" class="text-center py-12">
    <VaIcon name="error_outline" size="4rem" color="danger" />
    <p class="text-xl mt-4">{{ t('providers.notFound') }}</p>
    <VaButton class="mt-4" @click="router.push('/providers')">
      {{ t('providers.backToList') }}
    </VaButton>
  </div>

  <div v-else class="max-w-4xl mx-auto">
    <VaButton preset="secondary" icon="arrow_back" class="mb-4" @click="router.back()">
      {{ t('common.back') }}
    </VaButton>

    <!-- Profile Header -->
    <VaCard class="mb-6">
      <VaCardContent>
        <div class="flex flex-col md:flex-row gap-6">
          <VaAvatar :src="provider.avatar" size="96px" color="primary">
            {{ provider.name?.charAt(0) }}
          </VaAvatar>

          <div class="flex-grow">
            <div class="flex items-start justify-between mb-2">
              <div>
                <h1 class="text-3xl font-bold mb-2">{{ provider.name }}</h1>
                <div class="flex gap-2 mb-3">
                  <VaBadge v-if="provider.isCertified" text="✓ 已认证" color="success" />
                  <VaBadge v-if="provider.isRecommended" text="推荐服务人员" color="primary" />
                  <VaBadge v-if="provider.isActive" text="在线" color="success" />
                </div>
              </div>
            </div>

            <div class="flex items-center gap-4 mb-4">
              <div class="flex items-center gap-2">
                <VaRating :model-value="provider.rating || 5" readonly />
                <span class="font-semibold">{{ provider.rating || '5.0' }}</span>
              </div>
              <div class="text-secondary">
                {{ provider.orderCount || 0 }} {{ t('providers.ordersCompleted') }}
              </div>
            </div>

            <p class="text-secondary">{{ provider.introduction || '暂无介绍' }}</p>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Stats -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <VaCard>
        <VaCardContent>
          <div class="text-center">
            <div class="text-3xl font-bold text-primary">{{ provider.orderCount || 0 }}</div>
            <div class="text-sm text-secondary mt-1">{{ t('providers.totalOrders') }}</div>
          </div>
        </VaCardContent>
      </VaCard>

      <VaCard>
        <VaCardContent>
          <div class="text-center">
            <div class="text-3xl font-bold text-success">{{ provider.rating || '5.0' }}</div>
            <div class="text-sm text-secondary mt-1">{{ t('providers.rating') }}</div>
          </div>
        </VaCardContent>
      </VaCard>

      <VaCard>
        <VaCardContent>
          <div class="text-center">
            <div class="text-3xl font-bold text-warning">{{ provider.yearsOfService || '1' }}</div>
            <div class="text-sm text-secondary mt-1">{{ t('providers.yearsOfService') }}</div>
          </div>
        </VaCardContent>
      </VaCard>

      <VaCard>
        <VaCardContent>
          <div class="text-center">
            <div class="text-3xl font-bold text-info">98%</div>
            <div class="text-sm text-secondary mt-1">{{ t('providers.responseRate') }}</div>
          </div>
        </VaCardContent>
      </VaCard>
    </div>

    <!-- Specialties -->
    <VaCard class="mb-6">
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="star" />
          <span>{{ t('providers.specialtiesTitle') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="flex flex-wrap gap-3">
          <VaChip v-for="specialty in provider.specialties" :key="specialty" color="primary">
            {{ specialty }}
          </VaChip>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Service History -->
    <VaCard class="mb-6">
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="history" />
          <span>{{ t('providers.serviceHistory') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="text-center py-8 text-secondary">
          <VaIcon name="receipt_long" size="3rem" color="secondary" />
          <p class="mt-2">{{ t('providers.noHistory') }}</p>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Reviews -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="rate_review" />
          <span>{{ t('providers.reviews') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="text-center py-8 text-secondary">
          <VaIcon name="reviews" size="3rem" color="secondary" />
          <p class="mt-2">{{ t('providers.noReviews') }}</p>
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { adminApi } from '../../services/catcat-api'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const provider = ref<any>(null)

const loadProvider = async () => {
  loading.value = true
  try {
    const id = Number(route.params.id)
    const response = await adminApi.getUsers({ page: 1, pageSize: 100 })
    const user = response.data.items?.find((u: any) => u.id === id && u.role === 2)

    if (user) {
      provider.value = {
        ...user,
        rating: 4.5 + Math.random() * 0.5,
        orderCount: Math.floor(Math.random() * 100),
        yearsOfService: Math.floor(Math.random() * 5) + 1,
        isCertified: true,
        isRecommended: Math.random() > 0.5,
        introduction: '经验丰富的宠物护理专家，擅长猫咪护理和行为训练。有5年以上的宠物护理经验，深受客户信赖。',
        specialties: ['喂食', '清洁', '陪玩', '护理', '行为训练'],
      }
    }
  } catch (error: any) {
    notify({ message: error.message || '加载服务人员信息失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadProvider()
})
</script>

