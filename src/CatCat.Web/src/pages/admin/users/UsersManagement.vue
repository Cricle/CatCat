<template>
  <h1 class="page-title">{{ t('admin.users.title') }}</h1>

  <!-- Filters and Actions -->
  <VaCard class="mb-6">
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-4 items-end">
        <VaInput v-model="filter.search" :placeholder="t('admin.users.searchPlaceholder')" class="flex-grow">
          <template #prependInner>
            <VaIcon name="search" />
          </template>
        </VaInput>

        <VaSelect
          v-model="filter.role"
          :options="roleOptions"
          :label="t('admin.users.filterByRole')"
          class="md:w-48"
        />

        <VaSelect v-model="filter.status" :options="statusOptions" :label="t('admin.users.status')" class="md:w-48" />

        <VaButton icon="add" @click="showCreateModal = true">
          {{ t('admin.users.addUser') }}
        </VaButton>
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Stats Cards -->
  <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
    <VaCard>
      <VaCardContent>
        <div class="flex items-center gap-4">
          <VaIcon name="people" size="2.5rem" color="primary" />
          <div>
            <div class="text-2xl font-bold">{{ stats.total }}</div>
            <div class="text-sm text-secondary">{{ t('admin.users.totalUsers') }}</div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard>
      <VaCardContent>
        <div class="flex items-center gap-4">
          <VaIcon name="person" size="2.5rem" color="success" />
          <div>
            <div class="text-2xl font-bold">{{ stats.customers }}</div>
            <div class="text-sm text-secondary">{{ t('admin.users.customers') }}</div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard>
      <VaCardContent>
        <div class="flex items-center gap-4">
          <VaIcon name="work" size="2.5rem" color="warning" />
          <div>
            <div class="text-2xl font-bold">{{ stats.providers }}</div>
            <div class="text-sm text-secondary">{{ t('admin.users.providers') }}</div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>

    <VaCard>
      <VaCardContent>
        <div class="flex items-center gap-4">
          <VaIcon name="admin_panel_settings" size="2.5rem" color="danger" />
          <div>
            <div class="text-2xl font-bold">{{ stats.admins }}</div>
            <div class="text-sm text-secondary">{{ t('admin.users.admins') }}</div>
          </div>
        </div>
      </VaCardContent>
    </VaCard>
  </div>

  <!-- Users Table -->
  <VaCard>
    <VaCardContent>
      <div v-if="loading" class="flex justify-center py-8">
        <VaProgressCircle indeterminate />
      </div>

      <div v-else-if="filteredUsers.length === 0" class="text-center py-8 text-secondary">
        {{ t('admin.users.noUsers') }}
      </div>

      <VaDataTable v-else :items="paginatedUsers" :columns="columns">
        <template #cell(avatar)="{ rowData }">
          <VaAvatar :src="rowData.avatar" :color="rowData.avatar ? undefined : 'primary'">
            {{ rowData.name?.charAt(0) }}
          </VaAvatar>
        </template>

        <template #cell(name)="{ rowData }">
          <div>
            <div class="font-semibold">{{ rowData.name }}</div>
            <div class="text-sm text-secondary">{{ rowData.phone }}</div>
          </div>
        </template>

        <template #cell(role)="{ rowData }">
          <VaChip :color="getRoleColor(rowData.role)" size="small">
            {{ getRoleText(rowData.role) }}
          </VaChip>
        </template>

        <template #cell(isActive)="{ rowData }">
          <VaBadge :text="rowData.isActive ? t('admin.users.active') : t('admin.users.inactive')" :color="rowData.isActive ? 'success' : 'danger'" />
        </template>

        <template #cell(createdAt)="{ rowData }">
          {{ formatDate(rowData.createdAt) }}
        </template>

        <template #cell(actions)="{ rowData }">
          <div class="flex gap-2">
            <VaButton preset="secondary" size="small" icon="edit" @click="editUser(rowData)">
              {{ t('common.edit') }}
            </VaButton>
            <VaButton
              preset="secondary"
              size="small"
              :icon="rowData.isActive ? 'block' : 'check_circle'"
              :color="rowData.isActive ? 'danger' : 'success'"
              @click="toggleUserStatus(rowData)"
            >
              {{ rowData.isActive ? t('admin.users.disable') : t('admin.users.enable') }}
            </VaButton>
          </div>
        </template>
      </VaDataTable>

      <!-- Pagination -->
      <div v-if="!loading && filteredUsers.length > 0" class="flex justify-center mt-4">
        <VaPagination v-model="pagination.page" :pages="totalPages" :visible-pages="5" buttons-preset="secondary" />
      </div>
    </VaCardContent>
  </VaCard>

  <!-- Create/Edit Modal -->
  <VaModal v-model="showCreateModal" size="medium" :title="editingUser ? t('admin.users.editUser') : t('admin.users.createUser')">
    <div class="flex flex-col gap-4">
      <VaInput v-model="userForm.name" :label="t('admin.users.name')" :placeholder="t('admin.users.namePlaceholder')" />
      <VaInput v-model="userForm.phone" :label="t('admin.users.phone')" :placeholder="t('admin.users.phonePlaceholder')" />
      <VaInput v-model="userForm.email" :label="t('admin.users.email')" type="email" :placeholder="t('admin.users.emailPlaceholder')" />
      <VaSelect v-model="userForm.role" :label="t('admin.users.role')" :options="roleSelectOptions" />
      <VaCheckbox v-model="userForm.isActive" :label="t('admin.users.activeStatus')" />
    </div>

    <template #footer>
      <VaButton preset="secondary" @click="showCreateModal = false">
        {{ t('common.cancel') }}
      </VaButton>
      <VaButton @click="saveUser">
        {{ t('common.save') }}
      </VaButton>
    </template>
  </VaModal>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { adminApi } from '../../../services/catcat-api'

const { t } = useI18n()
const { init: notify } = useToast()

const loading = ref(false)
const users = ref<any[]>([])
const showCreateModal = ref(false)
const editingUser = ref<any>(null)

const filter = ref({
  search: '',
  role: '全部角色',
  status: '全部状态',
})

const roleOptions = ['全部角色', '普通用户', '服务人员', '管理员']
const statusOptions = ['全部状态', '启用', '禁用']

const roleSelectOptions = [
  { text: '普通用户', value: 1 },
  { text: '服务人员', value: 2 },
  { text: '管理员', value: 99 },
]

const userForm = ref({
  name: '',
  phone: '',
  email: '',
  role: 1,
  isActive: true,
})

const stats = ref({
  total: 0,
  customers: 0,
  providers: 0,
  admins: 0,
})

const columns = [
  { key: 'avatar', label: '', sortable: false },
  { key: 'name', label: t('admin.users.name'), sortable: true },
  { key: 'role', label: t('admin.users.role'), sortable: true },
  { key: 'isActive', label: t('admin.users.status'), sortable: true },
  { key: 'createdAt', label: t('admin.users.joinedAt'), sortable: true },
  { key: 'actions', label: t('admin.users.actions'), sortable: false },
]

const pagination = ref({
  page: 1,
  perPage: 10,
})

// Load users
const loadUsers = async () => {
  loading.value = true
  try {
    const response = await adminApi.getUsers({ page: 1, pageSize: 100 })
    users.value = response.data.items || []

    // Calculate stats
    stats.value.total = users.value.length
    stats.value.customers = users.value.filter((u) => u.role === 1).length
    stats.value.providers = users.value.filter((u) => u.role === 2).length
    stats.value.admins = users.value.filter((u) => u.role === 99).length
  } catch (error: any) {
    notify({ message: error.message || '加载用户失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Filtered users
const filteredUsers = computed(() => {
  let result = users.value

  // Search
  if (filter.value.search) {
    const search = filter.value.search.toLowerCase()
    result = result.filter(
      (u) => u.name?.toLowerCase().includes(search) || u.phone?.includes(search) || u.email?.toLowerCase().includes(search),
    )
  }

  // Role filter
  if (filter.value.role !== '全部角色') {
    const roleMap: Record<string, number> = { 普通用户: 1, 服务人员: 2, 管理员: 99 }
    result = result.filter((u) => u.role === roleMap[filter.value.role])
  }

  // Status filter
  if (filter.value.status !== '全部状态') {
    const isActive = filter.value.status === '启用'
    result = result.filter((u) => u.isActive === isActive)
  }

  return result
})

// Paginated users
const paginatedUsers = computed(() => {
  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredUsers.value.slice(start, end)
})

const totalPages = computed(() => Math.ceil(filteredUsers.value.length / pagination.value.perPage))

// Get role text
const getRoleText = (role: number) => {
  const map: Record<number, string> = { 1: '普通用户', 2: '服务人员', 99: '管理员' }
  return map[role] || '未知'
}

// Get role color
const getRoleColor = (role: number) => {
  const map: Record<number, string> = { 1: 'info', 2: 'warning', 99: 'danger' }
  return map[role] || 'secondary'
}

// Format date
const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleDateString('zh-CN')
}

// Edit user
const editUser = (user: any) => {
  editingUser.value = user
  userForm.value = { ...user }
  showCreateModal.value = true
}

// Toggle user status
const toggleUserStatus = async (user: any) => {
  try {
    // await adminApi.toggleUserStatus(user.id)
    user.isActive = !user.isActive
    notify({ message: '状态更新成功', color: 'success' })
  } catch (error: any) {
    notify({ message: error.message || '更新失败', color: 'danger' })
  }
}

// Save user
const saveUser = async () => {
  try {
    if (editingUser.value) {
      // Update
      // await adminApi.updateUser(editingUser.value.id, userForm.value)
      Object.assign(editingUser.value, userForm.value)
      notify({ message: '更新成功', color: 'success' })
    } else {
      // Create
      // const response = await adminApi.createUser(userForm.value)
      // users.value.unshift(response.data)
      notify({ message: '创建成功', color: 'success' })
    }
    showCreateModal.value = false
    editingUser.value = null
    userForm.value = { name: '', phone: '', email: '', role: 1, isActive: true }
  } catch (error: any) {
    notify({ message: error.message || '保存失败', color: 'danger' })
  }
}

onMounted(() => {
  loadUsers()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}
</style>

