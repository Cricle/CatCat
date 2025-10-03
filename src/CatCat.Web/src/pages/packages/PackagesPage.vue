<template>
  <h1 class="page-title">{{ t('packages.title') }}</h1>

  <!-- Filters -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4">
        <VaInput v-model="filter.search" :placeholder="t('packages.searchPlaceholder')" class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaSelect
          v-model="filter.sort"
          :options="sortOptions"
          :label="t('packages.sortBy')"
          class="md:w-48"
        />
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Loading -->
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <!-- Empty State -->
  <div v-else-if="filteredPackages.length === 0" class="text-center py-12">
    <VaIcon name="inventory_2" size="4rem" color="secondary" />
    <p class="text-xl mt-4 text-secondary">{{ t('packages.noPackages') }}</p>
  </div>

  <!-- Packages Grid -->
  <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <VaCard
      v-for="pkg in filteredPackages"
      :key="pkg.id"
      class="package-card cursor-pointer hover:shadow-lg transition-shadow"
      @click="goToDetail(pkg.id)"
    >
      <VaCardContent>
        <!-- Package Header -->
        <div class="flex justify-between items-start mb-4">
          <div>
            <h3 class="text-2xl font-bold mb-2">{{ pkg.name }}</h3>
            <div class="flex items-center gap-2">
              <VaBadge :text="pkg.category" color="primary" />
              <VaBadge v-if="pkg.isPopular" text="🔥 热门" color="warning" />
            </div>
          </div>
          <div class="text-right">
            <div class="text-3xl font-bold text-primary">¥{{ pkg.price }}</div>
            <div class="text-sm text-secondary">/ {{ pkg.duration }}分钟</div>
          </div>
        </div>

        <!-- Package Description -->
        <p class="text-secondary mb-4 line-clamp-2">{{ pkg.description }}</p>

        <!-- Services Included -->
        <div class="mb-4">
          <div class="text-sm font-semibold mb-2">{{ t('packages.includes') }}:</div>
          <div class="flex flex-wrap gap-2">
            <VaChip
              v-for="service in pkg.services?.slice(0, 4)"
              :key="service"
              size="small"
              color="success"
              outline
            >
              {{ service }}
            </VaChip>
            <VaChip v-if="pkg.services && pkg.services.length > 4" size="small" color="info" outline>
              +{{ pkg.services.length - 4 }} 更多
            </VaChip>
          </div>
        </div>

        <!-- Stats -->
        <div class="flex gap-4 text-sm text-secondary mb-4">
          <div class="flex items-center gap-1">
            <VaIcon name="schedule" size="small" />
            <span>{{ pkg.duration }}分钟</span>
          </div>
          <div class="flex items-center gap-1">
            <VaIcon name="star" size="small" color="warning" />
            <span>{{ pkg.rating || '5.0' }} ({{ pkg.orderCount || 0 }})</span>
          </div>
        </div>

        <!-- Action Button -->
        <VaButton block @click.stop="createOrder(pkg.id)">
          {{ t('packages.bookNow') }}
        </VaButton>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Pagination -->
  <div v-if="!loading && filteredPackages.length > 0" class="flex justify-center mt-6">
    <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { packageApi } from '../../services/catcat-api'
import type { ServicePackage } from '../../types/catcat-types'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const loading = ref(false)
const packages = ref<ServicePackage[]>([])

const filter = ref({
  search: '',
  sort: '推荐排序',
})

const sortOptions = ['推荐排序', '价格从低到高', '价格从高到低', '评分最高', '最受欢迎']

const pagination = ref({
  page: 1,
  perPage: 9,
})

// Load packages
const loadPackages = async () => {
  loading.value = true
  try {
    const response = await packageApi.getAll({ page: 1, pageSize: 100 })
    packages.value = response.data.items || []
  } catch (error: any) {
    notify({
      message: error.message || '加载套餐失败',
      color: 'danger',
    })
  } finally {
    loading.value = false
  }
}

// Filtered packages
const filteredPackages = computed(() => {
  let result = packages.value

  // Search filter
  if (filter.value.search) {
    const searchLower = filter.value.search.toLowerCase()
    result = result.filter(
      (pkg) =>
        pkg.name.toLowerCase().includes(searchLower) ||
        pkg.description?.toLowerCase().includes(searchLower),
    )
  }

  // Sort
  if (filter.value.sort === '价格从低到高') {
    result = [...result].sort((a, b) => a.price - b.price)
  } else if (filter.value.sort === '价格从高到低') {
    result = [...result].sort((a, b) => b.price - a.price)
  } else if (filter.value.sort === '评分最高') {
    result = [...result].sort((a, b) => (b.rating || 0) - (a.rating || 0))
  } else if (filter.value.sort === '最受欢迎') {
    result = [...result].sort((a, b) => (b.orderCount || 0) - (a.orderCount || 0))
  }

  // Pagination
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return result.slice(start, end)
})

// Total pages
const totalPages = computed(() => {
  return Math.ceil(packages.value.length / pagination.value.perPage)
})

// Go to detail
const goToDetail = (id: number) => {
  router.push(`/packages/${id}`)
}

// Create order
const createOrder = (packageId: number) => {
  router.push({ name: 'create-order', query: { packageId } })
}

onMounted(() => {
  loadPackages()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.package-card {
  transition: all 0.3s ease;
}

.package-card:hover {
  transform: translateY(-4px);
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>

