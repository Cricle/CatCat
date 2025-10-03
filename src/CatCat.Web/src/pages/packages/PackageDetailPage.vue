<template>
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <div v-else-if="!servicePackage" class="text-center py-12">
    <VaIcon name="error_outline" size="4rem" color="danger" />
    <p class="text-xl mt-4">{{ t('packages.notFound') }}</p>
    <VaButton class="mt-4" @click="router.push('/packages')">
      {{ t('packages.backToList') }}
    </VaButton>
  </div>

  <div v-else class="max-w-6xl mx-auto">
    <!-- Back Button -->
    <VaButton preset="secondary" icon="arrow_back" class="mb-4" @click="router.back()">
      {{ t('common.back') }}
    </VaButton>

    <!-- Package Header -->
    <VaCard class="mb-6">
      <VaCardContent>
        <div class="flex flex-col md:flex-row gap-6">
          <!-- Package Info -->
          <div class="flex-grow">
            <div class="flex items-start justify-between mb-4">
              <div>
                <h1 class="text-3xl font-bold mb-2">{{ servicePackage.name }}</h1>
                <div class="flex gap-2 mb-3">
                  <VaBadge :text="servicePackage.category" color="primary" />
                  <VaBadge v-if="servicePackage.isPopular" text="🔥 热门套餐" color="warning" />
                  <VaBadge v-if="servicePackage.isActive" text="可预订" color="success" />
                </div>
              </div>
            </div>

            <p class="text-lg text-secondary mb-4">{{ servicePackage.description }}</p>

            <!-- Stats -->
            <div class="flex flex-wrap gap-6 mb-4">
              <div class="flex items-center gap-2">
                <VaIcon name="schedule" color="primary" />
                <div>
                  <div class="text-sm text-secondary">{{ t('packages.duration') }}</div>
                  <div class="font-semibold">{{ servicePackage.duration }} 分钟</div>
                </div>
              </div>

              <div class="flex items-center gap-2">
                <VaIcon name="star" color="warning" />
                <div>
                  <div class="text-sm text-secondary">{{ t('packages.rating') }}</div>
                  <div class="font-semibold">{{ servicePackage.rating || '5.0' }} / 5.0</div>
                </div>
              </div>

              <div class="flex items-center gap-2">
                <VaIcon name="shopping_cart" color="success" />
                <div>
                  <div class="text-sm text-secondary">{{ t('packages.orders') }}</div>
                  <div class="font-semibold">{{ servicePackage.orderCount || 0 }} 单</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Price and Action -->
          <div class="md:w-64">
            <VaCard gradient stripe stripe-color="primary">
              <VaCardContent>
                <div class="text-center text-white">
                  <div class="text-sm opacity-80 mb-1">{{ t('packages.price') }}</div>
                  <div class="text-4xl font-bold mb-1">¥{{ servicePackage.price }}</div>
                  <div class="text-sm opacity-80 mb-4">/ {{ servicePackage.duration }}分钟</div>
                  <VaButton block color="white" text-color="primary" @click="createOrder">
                    {{ t('packages.bookNow') }}
                  </VaButton>
                </div>
              </VaCardContent>
            </VaCard>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Services Included -->
    <VaCard class="mb-6">
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="task_alt" />
          <span>{{ t('packages.servicesIncluded') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div v-for="service in servicePackage.services" :key="service" class="flex items-center gap-3">
            <VaIcon name="check_circle" color="success" />
            <span class="text-lg">{{ service }}</span>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Service Details -->
    <VaCard class="mb-6">
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="info" />
          <span>{{ t('packages.details') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="prose max-w-none">
          <p v-if="package.details">{{ package.details }}</p>
          <p v-else class="text-secondary">{{ t('packages.noDetails') }}</p>
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Customer Reviews (Placeholder) -->
    <VaCard>
      <VaCardTitle>
        <div class="flex items-center gap-2">
          <VaIcon name="rate_review" />
          <span>{{ t('packages.customerReviews') }}</span>
        </div>
      </VaCardTitle>
      <VaCardContent>
        <div class="text-center py-8 text-secondary">
          <VaIcon name="reviews" size="3rem" color="secondary" />
          <p class="mt-2">{{ t('packages.noReviews') }}</p>
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
import { packageApi } from '../../services/catcat-api'
import type { ServicePackage } from '../../types/catcat-types'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const servicePackage = ref<ServicePackage | null>(null)

// Load package
const loadPackage = async () => {
  loading.value = true
  try {
    const id = Number(route.params.id)
    const response = await packageApi.getById(id)
    servicePackage.value = response.data
  } catch (error: any) {
    notify({
      message: error.message || '加载套餐详情失败',
      color: 'danger',
    })
  } finally {
    loading.value = false
  }
}

// Create order
const createOrder = () => {
  if (servicePackage.value) {
    router.push({ name: 'create-order', query: { packageId: servicePackage.value.id } })
  }
}

onMounted(() => {
  loadPackage()
})
</script>

<style scoped>
.prose {
  line-height: 1.75;
}
</style>

