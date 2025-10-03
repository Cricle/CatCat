<template>
  <div class="admin-users-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <h1 class="va-h1">User Management</h1>
          <va-button icon="refresh" @click="fetchUsers">
            Refresh
          </va-button>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Filters -->
        <div class="va-row">
          <div class="flex xs12 sm4 md3">
            <va-select
              v-model="filters.role"
              :options="roleOptions"
              label="Role"
              clearable
              @update:modelValue="fetchUsers"
            />
          </div>
          <div class="flex xs12 sm4 md3">
            <va-select
              v-model="filters.status"
              :options="statusOptions"
              label="Status"
              clearable
              @update:modelValue="fetchUsers"
            />
          </div>
        </div>

        <!-- Data Table -->
        <va-data-table
          :items="users"
          :columns="columns"
          :loading="loading"
          :per-page="pagination.pageSize"
          :current-page="pagination.page"
          @update:current-page="handlePageChange"
          striped
          hoverable
        >
          <template #cell(role)="{ rowData }">
            <va-badge :text="getRoleText(rowData.role)" :color="getRoleColor(rowData.role)" />
          </template>

          <template #cell(status)="{ rowData }">
            <va-badge :text="getStatusText(rowData.status)" :color="getStatusColor(rowData.status)" />
          </template>

          <template #cell(createdAt)="{ rowData }">
            {{ formatDate(rowData.createdAt) }}
          </template>

          <template #cell(actions)="{ rowData }">
            <va-button-group>
              <va-button size="small" preset="plain" icon="edit" @click="editUser(rowData)" />
              <va-button size="small" preset="plain" icon="vpn_key" @click="changeRole(rowData)" />
            </va-button-group>
          </template>
        </va-data-table>

        <!-- Pagination -->
        <div class="pagination-wrapper">
          <va-pagination
            v-model="pagination.page"
            :pages="totalPages"
            @update:modelValue="fetchUsers"
          />
        </div>
      </va-card-content>
    </va-card>

    <!-- Edit User Modal -->
    <va-modal
      v-model="showEditModal"
      title="Edit User Status"
      size="small"
      ok-text="Save"
      @ok="saveUserStatus"
    >
      <va-select
        v-model="editForm.status"
        :options="statusOptions"
        label="Status"
      />
    </va-modal>

    <!-- Change Role Modal -->
    <va-modal
      v-model="showRoleModal"
      title="Change User Role"
      size="small"
      ok-text="Save"
      @ok="saveUserRole"
    >
      <va-select
        v-model="editForm.role"
        :options="roleOptions"
        label="Role"
      />
    </va-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getUsers, updateUserStatus, updateUserRole, type User } from '@/api/admin'
import { useToast } from 'vuestic-ui'

const { init: notify } = useToast()

const loading = ref(false)
const users = ref<User[]>([])
const pagination = ref({ page: 1, pageSize: 20, total: 0 })
const filters = ref({ role: null as number | null, status: null as number | null })
const showEditModal = ref(false)
const showRoleModal = ref(false)
const editForm = ref({ id: 0, status: 0, role: 1 })

const roleOptions = [
  { text: 'Customer', value: 1 },
  { text: 'Service Provider', value: 2 },
  { text: 'Admin', value: 99 }
]

const statusOptions = [
  { text: 'Pending', value: 0 },
  { text: 'Active', value: 1 },
  { text: 'Suspended', value: 2 },
  { text: 'Banned', value: 3 }
]

const columns = [
  { key: 'id', label: 'ID', sortable: true },
  { key: 'phone', label: 'Phone' },
  { key: 'nickName', label: 'Name' },
  { key: 'role', label: 'Role' },
  { key: 'status', label: 'Status' },
  { key: 'createdAt', label: 'Created' },
  { key: 'actions', label: 'Actions' }
]

const totalPages = computed(() => Math.ceil(pagination.value.total / pagination.value.pageSize))

const getRoleText = (role: number) => {
  const option = roleOptions.find(o => o.value === role)
  return option?.text || 'Unknown'
}

const getRoleColor = (role: number) => {
  const colors: Record<number, string> = { 1: 'info', 2: 'success', 99: 'danger' }
  return colors[role] || 'secondary'
}

const getStatusText = (status: number) => {
  const option = statusOptions.find(o => o.value === status)
  return option?.text || 'Unknown'
}

const getStatusColor = (status: number) => {
  const colors: Record<number, string> = { 0: 'warning', 1: 'success', 2: 'danger', 3: 'danger' }
  return colors[status] || 'secondary'
}

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

const fetchUsers = async () => {
  loading.value = true
  try {
    const res = await getUsers({
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      role: filters.value.role || undefined,
      status: filters.value.status || undefined
    })
    users.value = res.data.items
    pagination.value.total = res.data.total
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load users', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const handlePageChange = (page: number) => {
  pagination.value.page = page
  fetchUsers()
}

const editUser = (user: User) => {
  editForm.value = { id: user.id, status: user.status, role: user.role }
  showEditModal.value = true
}

const changeRole = (user: User) => {
  editForm.value = { id: user.id, status: user.status, role: user.role }
  showRoleModal.value = true
}

const saveUserStatus = async () => {
  try {
    await updateUserStatus(editForm.value.id, editForm.value.status)
    notify({ message: 'User status updated', color: 'success' })
    fetchUsers()
  } catch (error: any) {
    notify({ message: error.message || 'Update failed', color: 'danger' })
  }
}

const saveUserRole = async () => {
  try {
    await updateUserRole(editForm.value.id, editForm.value.role)
    notify({ message: 'User role updated', color: 'success' })
    fetchUsers()
  } catch (error: any) {
    notify({ message: error.message || 'Update failed', color: 'danger' })
  }
}

onMounted(() => {
  fetchUsers()
})
</script>

<style scoped>
.admin-users-page {
  padding: var(--va-content-padding);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.pagination-wrapper {
  display: flex;
  justify-content: center;
  margin-top: var(--va-content-padding);
}

@media (max-width: 768px) {
  .admin-users-page {
    padding: 12px;
  }
}
</style>

