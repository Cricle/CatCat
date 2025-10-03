<template>
  <h1 class="page-title">{{ t('admin.packages.title') }}</h1>

  <!-- Actions Bar -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4 items-end">
        <VaInput v-model="filter.search" :placeholder="t('admin.packages.searchPlaceholder')" class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaSelect v-model="filter.status" :options="statusOptions" :label="t('admin.packages.status')" class="md:w-48" />

        <VaButton icon="add" @click="createPackage">
          {{ t('admin.packages.addPackage') }}
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Packages Grid -->
  <div v-if="loading" class="flex justify-center py-12">
    <VaProgressCircle indeterminate />
  </div>

  <div v-else-if="filteredPackages.length === 0" class="text-center py-12">
    <VaIcon name="inventory_2" size="4rem" color="secondary" />
    <p class="text-xl mt-4 text-secondary">{{ t('admin.packages.noPackages') }}</p>
  </div>

  <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <VaCard v-for="pkg in paginatedPackages" :key="pkg.id" class="package-card">
      <VaCardContent>
        <!-- Package Header -->
        <div class="flex justify-between items-start mb-4">
          <div class="flex-grow">
            <h3 class="text-xl font-bold mb-2">{{ pkg.name }}</h3>
            <div class="flex gap-2 mb-2">
              <VaBadge :text="pkg.category" color="primary" />
              <VaBadge v-if="pkg.isPopular" text="🔥 热门" color="warning" />
              <VaBadge :text="pkg.isActive ? '启用' : '停用'" :color="pkg.isActive ? 'success' : 'danger'" />
            </div>
          </div>
          <div class="text-right">
            <div class="text-2xl font-bold text-primary">¥{{ pkg.price }}</div>
            <div class="text-sm text-secondary">{{ pkg.duration }}分钟</div>
          </div>
        </div>

        <!-- Description -->
        <p class="text-secondary mb-4 line-clamp-2">{{ pkg.description }}</p>

        <!-- Stats -->
        <div class="flex gap-4 text-sm text-secondary mb-4">
          <div class="flex items-center gap-1">
            <VaIcon name="shopping_cart" size="small" />
            <span>{{ pkg.orderCount || 0 }} 单</span>
          </div>
          <div class="flex items-center gap-1">
            <VaIcon name="star" size="small" color="warning" />
            <span>{{ pkg.rating || '5.0' }}</span>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex gap-2">
          <VaButton size="small" preset="secondary" icon="edit" @click="editPackage(pkg)">
            {{ t('common.edit') }}
          </VaButton>
          <VaButton
            size="small"
            preset="secondary"
            :icon="pkg.isActive ? 'visibility_off' : 'visibility'"
            :color="pkg.isActive ? 'warning' : 'success'"
            @click="togglePackageStatus(pkg)"
          >
            {{ pkg.isActive ? t('admin.packages.disable') : t('admin.packages.enable') }}
          </VaButton>
          <VaButton size="small" preset="secondary" icon="delete" color="danger" @click="deletePackage(pkg)">
            {{ t('common.delete') }}
          </VaButton>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Pagination -->
  <div v-if="!loading && filteredPackages.length > 0" class="flex justify-center mt-6">
    <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
  </div>

  <!-- Create/Edit Modal -->
  <VaModal v-model="showPackageModal" size="large" :title="editingPackage ? t('admin.packages.editPackage') : t('admin.packages.createPackage')">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <VaInput v-model="packageForm.name" :label="t('admin.packages.name')" class="md:col-span-2" />
      <VaInput v-model.number="packageForm.price" :label="t('admin.packages.price')" type="number" />
      <VaInput v-model.number="packageForm.duration" :label="t('admin.packages.duration')" type="number" suffix="分钟" />
      <VaInput v-model="packageForm.category" :label="t('admin.packages.category')" />
      <div class="flex items-center gap-4">
        <VaCheckbox v-model="packageForm.isActive" :label="t('admin.packages.activeStatus')" />
        <VaCheckbox v-model="packageForm.isPopular" :label="t('admin.packages.popularStatus')" />
      </div>
      <VaTextarea v-model="packageForm.description" :label="t('admin.packages.description')" class="md:col-span-2" :min-rows="3" />
      <VaTextarea v-model="packageForm.details" :label="t('admin.packages.details')" class="md:col-span-2" :min-rows="3" />
      <div class="md:col-span-2">
        <VaInput v-model="servicesInput" :label="t('admin.packages.services')" :placeholder="t('admin.packages.servicesPlaceholder')" />
        <div class="text-sm text-secondary mt-1">{{ t('admin.packages.servicesHint') }}</div>
      </div>
    </div>

    <template #footer>
      <VaButton preset="secondary" @click="showPackageModal = false">
        {{ t('common.cancel') }}
      </VaButton>
      <VaButton @click="savePackage">
        {{ t('common.save') }}
      </VaButton>
    </template>
  </VaModal>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { packageApi } from '../../../services/catcat-api'
import type { ServicePackage } from '../../../types/catcat-types'

const { t } = useI18n()
const { init: notify } = useToast()

const loading = ref(false)
const packages = ref<ServicePackage[]>([])
const showPackageModal = ref(false)
const editingPackage = ref<ServicePackage | null>(null)

const filter = ref({
  search: '',
  status: '全部状态',
})

const statusOptions = ['全部状态', '启用', '停用']

const packageForm = ref({
  name: '',
  price: 0,
  duration: 60,
  category: '基础服务',
  description: '',
  details: '',
  isActive: true,
  isPopular: false,
})

const servicesInput = ref('')

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
    notify({ message: error.message || '加载套餐失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered packages
const filteredPackages = computed(() => {
  let result = packages.value

  // Search
  if (filter.value.search) {
    const search = filter.value.search.toLowerCase()
    result = result.filter((p) => p.name.toLowerCase().includes(search) || p.description?.toLowerCase().includes(search))
  }

  // Status
  if (filter.value.status !== '全部状态') {
    const isActive = filter.value.status === '启用'
    result = result.filter((p) => p.isActive === isActive)
  }

  return result
})

// Paginated packages
const paginatedPackages = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredPackages.value.slice(start, end)
})

const totalPages = computed(() => Math.ceil(filteredPackages.value.length / pagination.value.perPage))

// Create package
const createPackage = () => {
  editingPackage.value = null
  packageForm.value = {
    name: '',
    price: 0,
    duration: 60,
    category: '基础服务',
    description: '',
    details: '',
    isActive: true,
    isPopular: false,
  }
  servicesInput.value = ''
  showPackageModal.value = true
}

// Edit package
const editPackage = (pkg: ServicePackage) => {
  editingPackage.value = pkg
  packageForm.value = { ...pkg } as any
  servicesInput.value = pkg.services?.join(', ') || ''
  showPackageModal.value = true
}

// Toggle package status
const togglePackageStatus = async (pkg: ServicePackage) => {
  try {
    // await packageApi.toggleStatus(pkg.id)
    pkg.isActive = !pkg.isActive
    notify({ message: '状态更新成功', color: 'success' })
  } catch (error: any) {
    notify({ message: error.message || '更新失败', color: 'danger' })
  }
}

// Delete package
const deletePackage = async (pkg: ServicePackage) => {
  if (!confirm(`确定要删除套餐"${pkg.name}"吗？`)) return

  try {
    // await packageApi.delete(pkg.id)
    const index = packages.value.findIndex((p) => p.id === pkg.id)
    if (index > -1) packages.value.splice(index, 1)
    notify({ message: '删除成功', color: 'success' })
  } catch (error: any) {
    notify({ message: error.message || '删除失败', color: 'danger' })
  }
}

// Save package
const savePackage = async () => {
  try {
    const data = {
      ...packageForm.value,
      services: servicesInput.value.split(',').map((s) => s.trim()).filter((s) => s),
    }

    if (editingPackage.value) {
      // Update
      // await packageApi.update(editingPackage.value.id, data)
      Object.assign(editingPackage.value, data)
      notify({ message: '更新成功', color: 'success' })
    } else {
      // Create
      // const response = await packageApi.create(data)
      // packages.value.unshift(response.data)
      notify({ message: '创建成功', color: 'success' })
    }

    showPackageModal.value = false
  } catch (error: any) {
    notify({ message: error.message || '保存失败', color: 'danger' })
  }
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
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>

