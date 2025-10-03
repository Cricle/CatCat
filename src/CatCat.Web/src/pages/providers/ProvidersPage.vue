<template>
  <h1 class="page-title">{{ t('providers.title') }}</h1>

  <!-- Filters -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4">
        <VaInput v-model="filter.search" :placeholder="t('providers.searchPlaceholder')" class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaSelect v-model="filter.sort" :options="sortOptions" :label="t('providers.sortBy')" class="md:w-48" />
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Loading -->
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <!-- Empty -->
  <div v-else-if="filteredProviders.length === 0" class="text-center py-12">
    <VaIcon name="person_off" size="4rem" color="secondary" />
    <p class="text-xl mt-4 text-secondary">{{ t('providers.noProviders') }}</p>
  </div>

  <!-- Providers Grid -->
  <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <VaCard
      v-for="provider in paginatedProviders"
      :key="provider.id"
      class="provider-card cursor-pointer hover:shadow-lg transition-shadow"
      @click="goToDetail(provider.id)"
    >
      <VaCardContent>
        <!-- Header -->
        <div class="flex items-start gap-4 mb-4">
          <VaAvatar :src="provider.avatar" size="large" color="primary">
            {{ provider.name?.charAt(0) }}
          </VaAvatar>
          <div class="flex-grow">
            <h3 class="text-xl font-bold mb-1">{{ provider.name }}</h3>
            <div class="flex items-center gap-2 mb-2">
              <VaRating :model-value="provider.rating || 5" readonly size="small" />
              <span class="text-sm text-secondary">({{ provider.orderCount || 0 }})</span>
            </div>
            <div class="flex gap-2">
              <VaBadge v-if="provider.isCertified" text="✓ 认证" color="success" />
              <VaBadge v-if="provider.isRecommended" text="推荐" color="primary" />
            </div>
          </div>
        </div>

        <!-- Introduction -->
        <p class="text-secondary mb-4 line-clamp-2">{{ provider.introduction || '这位服务人员很勤劳，暂时还没有介绍' }}</p>

        <!-- Stats -->
        <div class="grid grid-cols-3 gap-2 mb-4">
          <div class="text-center bg-gray-50 dark:bg-gray-800 rounded p-2">
            <div class="font-semibold">{{ provider.orderCount || 0 }}</div>
            <div class="text-xs text-secondary">{{ t('providers.totalOrders') }}</div>
          </div>
          <div class="text-center bg-gray-50 dark:bg-gray-800 rounded p-2">
            <div class="font-semibold">{{ provider.rating || '5.0' }}</div>
            <div class="text-xs text-secondary">{{ t('providers.rating') }}</div>
          </div>
          <div class="text-center bg-gray-50 dark:bg-gray-800 rounded p-2">
            <div class="font-semibold">{{ provider.yearsOfService || '1' }}</div>
            <div class="text-xs text-secondary">{{ t('providers.years') }}</div>
          </div>
        </div>

        <!-- Specialties -->
        <div v-if="provider.specialties" class="mb-4">
          <div class="text-sm font-semibold mb-2">{{ t('providers.specialties') }}:</div>
          <div class="flex flex-wrap gap-2">
            <VaChip v-for="specialty in provider.specialties.slice(0, 3)" :key="specialty" size="small" color="info" outline>
              {{ specialty }}
            </VaChip>
          </div>
        </div>

        <!-- Action -->
        <VaButton block @click.stop="selectProvider(provider)">
          {{ t('providers.viewProfile') }}
        </VaButton>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Pagination -->
  <div v-if="!loading && filteredProviders.length > 0" class="flex justify-center mt-6">
    <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { adminApi } from '../../services/catcat-api'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const providers = ref<any[]>([])

const filter = ref({
  search: '',
  sort: '推荐排序',
})

const sortOptions = ['推荐排序', '评分最高', '订单最多', '新加入']

const pagination = ref({
  page: 1,
  perPage: 9,
})

const loadProviders = async () => {
  loading.value = true
  try {
    const response = await adminApi.getUsers({ page: 1, pageSize: 100 })
    providers.value = (response.data.items || []).filter((u: any) => u.role === 2).map((u: any) => ({
      ...u,
      rating: 4.5 + Math.random() * 0.5,
      orderCount: Math.floor(Math.random() * 100),
      yearsOfService: Math.floor(Math.random() * 5) + 1,
      isCertified: Math.random() > 0.3,
      isRecommended: Math.random() > 0.7,
      introduction: '经验丰富的宠物护理专家，擅长猫咪护理和行为训练',
      specialties: ['喂食', '清洁', '陪玩', '护理'],
    }))
  } catch (error: any) {
    notify({ message: error.message || '加载服务人员失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const filteredProviders = computed(() => {
  let result = providers.value

  if (filter.value.search) {
    const search = filter.value.search.toLowerCase()
    result = result.filter((p) => p.name?.toLowerCase().includes(search) || p.introduction?.toLowerCase().includes(search))
  }

  if (filter.value.sort === '评分最高') {
    result = [...result].sort((a, b) => (b.rating || 0) - (a.rating || 0))
  } else if (filter.value.sort === '订单最多') {
    result = [...result].sort((a, b) => (b.orderCount || 0) - (a.orderCount || 0))
  } else if (filter.value.sort === '新加入') {
    result = [...result].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  }

  return result
})

const paginatedProviders = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredProviders.value.slice(start, end)
})

const totalPages = computed(() => Math.ceil(filteredProviders.value.length / pagination.value.perPage))

const goToDetail = (id: number) => {
  router.push(`/providers/${id}`)
}

const selectProvider = (provider: any) => {
  router.push(`/providers/${provider.id}`)
}

onMounted(() => {
  loadProviders()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.provider-card {
  transition: all 0.3s ease;
}

.provider-card:hover {
  transform: translateY(-4px);
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>

