<template>
  <div class="admin-pets-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <h1 class="va-h1">Pet Management</h1>
          <va-button icon="refresh" @click="fetchPets">
            Refresh
          </va-button>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Filters -->
        <div class="va-row">
          <div class="flex xs12 sm6 md4">
            <va-input
              v-model.number="filters.userId"
              label="User ID"
              type="number"
              clearable
              @update:modelValue="fetchPets"
            />
          </div>
        </div>

        <!-- Data Table -->
        <va-data-table
          :items="pets"
          :columns="columns"
          :loading="loading"
          :per-page="pagination.pageSize"
          :current-page="pagination.page"
          @update:current-page="handlePageChange"
          striped
          hoverable
        >
          <template #cell(type)="{ rowData }">
            <va-chip size="small" :color="rowData.type === 1 ? 'primary' : 'info'">
              {{ rowData.type === 1 ? 'Cat' : 'Other' }}
            </va-chip>
          </template>

          <template #cell(gender)="{ rowData }">
            {{ rowData.gender === 1 ? 'Male' : 'Female' }}
          </template>

          <template #cell(createdAt)="{ rowData }">
            {{ formatDate(rowData.createdAt) }}
          </template>

          <template #cell(actions)="{ rowData }">
            <va-button
              size="small"
              preset="plain"
              icon="delete"
              color="danger"
              @click="confirmDelete(rowData)"
            />
          </template>
        </va-data-table>

        <!-- Pagination -->
        <div class="pagination-wrapper">
          <va-pagination
            v-model="pagination.page"
            :pages="totalPages"
            @update:modelValue="fetchPets"
          />
        </div>
      </va-card-content>
    </va-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getAllPets, deletePet, type Pet } from '@/api/admin'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()

const loading = ref(false)
const pets = ref<Pet[]>([])
const pagination = ref({ page: 1, pageSize: 20, total: 0 })
const filters = ref({ userId: null as number | null })

const columns = [
  { key: 'id', label: 'ID', sortable: true },
  { key: 'userId', label: 'User ID' },
  { key: 'name', label: 'Name' },
  { key: 'breed', label: 'Breed' },
  { key: 'age', label: 'Age' },
  { key: 'type', label: 'Type' },
  { key: 'gender', label: 'Gender' },
  { key: 'createdAt', label: 'Created' },
  { key: 'actions', label: 'Actions' }
]

const totalPages = computed(() => Math.ceil(pagination.value.total / pagination.value.pageSize))

const formatDate = (date: string) => {
  return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}

const fetchPets = async () => {
  loading.value = true
  try {
    const res = await getAllPets({
      page: pagination.value.page,
      pageSize: pagination.value.pageSize,
      userId: filters.value.userId || undefined
    })
    pets.value = res.data.items
    pagination.value.total = res.data.total
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load pets', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const handlePageChange = (page: number) => {
  pagination.value.page = page
  fetchPets()
}

const confirmDelete = async (pet: Pet) => {
  const agreed = await confirm({
    title: 'Delete Pet',
    message: `Are you sure you want to delete "${pet.name}"?`,
    okText: 'Delete',
    cancelText: 'Cancel'
  })

  if (agreed) {
    try {
      await deletePet(pet.id)
      notify({ message: 'Pet deleted', color: 'success' })
      fetchPets()
    } catch (error: any) {
      notify({ message: error.message || 'Delete failed', color: 'danger' })
    }
  }
}

onMounted(() => {
  fetchPets()
})
</script>

<style scoped>
.admin-pets-page {
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
  .admin-pets-page {
    padding: 12px;
  }
}
</style>

