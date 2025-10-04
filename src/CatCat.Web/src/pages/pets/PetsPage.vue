<template>
  <div class="page-container">
    <div class="mb-6">
      <h1 class="page-title">{{ t('menu.pets') }}</h1>
      <p class="text-secondary">{{ t('pets.subtitle') }}</p>
    </div>

    <VaCard>
      <VaCardContent>
        <div class="flex flex-col md:flex-row gap-3 mb-4 justify-between">
          <VaInput v-model="filter" :placeholder="t('pets.searchPlaceholder')" class="flex-grow md:max-w-md">
            <template #prependInner>
              <VaIcon name="search" color="secondary" />
            </template>
          </VaInput>
          <div class="flex gap-2">
            <VaButtonToggle
              v-model="viewMode"
              :options="viewModeOptions"
              size="small"
              color="primary"
              border-color="primary"
            />
            <VaButton icon="add" @click="showAddModal = true">
              {{ t('pets.addPet') }}
            </VaButton>
          </div>
        </div>

        <!-- Loading Skeleton -->
        <LoadingSkeleton v-if="loading" type="grid" :count="8" />

        <!-- Empty State -->
        <EmptyState
          v-else-if="pets.length === 0"
          icon="pets"
          :title="t('emptyState.noPets')"
          :description="t('emptyState.noPetsDesc')"
          :action-text="t('pets.addPet')"
          @action="showAddModal = true"
        />

        <!-- Card View -->
        <div v-else-if="viewMode === 'card'" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          <PetCard
            v-for="pet in paginatedPets"
            :key="pet.id"
            :pet="pet"
            @edit-pet="editPet"
            @delete-pet="confirmDelete"
          />
        </div>

        <!-- Table View -->
        <PetsTable
          v-else
          v-model:sort-by="sortBy"
          v-model:sorting-order="sortingOrder"
          :pets="filteredPets"
          :loading="loading"
          :pagination="pagination"
          @edit-pet="editPet"
          @delete-pet="confirmDelete"
        />

        <!-- Pagination for Card View -->
        <div v-if="viewMode === 'card' && filteredPets.length > pagination.perPage" class="flex justify-center mt-6">
          <VaPagination
            v-model="pagination.page"
            :pages="Math.ceil(filteredPets.length / pagination.perPage)"
            :visible-pages="5"
            buttons-preset="secondary"
          />
        </div>
      </VaCardContent>
    </VaCard>

    <!-- Add/Edit Pet Modal -->
    <VaModal
      v-model="showAddModal"
      :title="t('pets.addPet')"
      size="large"
      @ok="savePet"
      @cancel="cancelEdit"
    >
      <PetForm v-model="editedPet" />
    </VaModal>

    <VaModal
      v-model="showEditModal"
      :title="t('pets.editPet')"
      size="large"
      @ok="savePet"
      @cancel="cancelEdit"
    >
      <PetForm v-model="editedPet" />
    </VaModal>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog
      v-model="showDeleteConfirm"
      :title="t('confirmDialog.deleteTitle')"
      :message="t('confirmDialog.deleteMessage')"
      :detail="deleteTarget ? `${t('pets.pet')}: ${deleteTarget.name}` : ''"
      icon="warning"
      icon-color="danger"
      confirm-color="danger"
      :confirm-text="t('confirmDialog.confirm')"
      :cancel-text="t('confirmDialog.cancel')"
      @confirm="handleDeleteConfirm"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vuestic-ui'
import { useI18n } from 'vue-i18n'
import { petApi } from '../../services/catcat-api'
import type { Pet } from '../../types/catcat-types'
import PetsTable from './widgets/PetsTable.vue'
import PetCard from './widgets/PetCard.vue'
import PetForm from './widgets/PetForm.vue'
import LoadingSkeleton from '../../components/LoadingSkeleton.vue'
import EmptyState from '../../components/EmptyState.vue'
import ConfirmDialog from '../../components/ConfirmDialog.vue'

const { init: notify } = useToast()
const { t } = useI18n()

const pets = ref<Pet[]>([])
const loading = ref(false)
const filter = ref('')
const sortBy = ref('name')
const sortingOrder = ref<'asc' | 'desc'>('asc')
const viewMode = ref<'card' | 'table'>('card')

const viewModeOptions = [
  { label: t('pets.cardView'), value: 'card', icon: 'grid_view' },
  { label: t('pets.tableView'), value: 'table', icon: 'view_list' },
]

const showAddModal = ref(false)
const showEditModal = ref(false)
const showDeleteConfirm = ref(false)
const editedPet = ref<Partial<Pet>>({})
const deleteTarget = ref<Pet | null>(null)

const pagination = ref({
  page: 1,
  perPage: 10,
  total: 0,
})

// Filtered and sorted pets
const filteredPets = computed(() => {
  let result = pets.value

  if (filter.value) {
    result = result.filter((pet) => pet.name.toLowerCase().includes(filter.value.toLowerCase()))
  }

  // Sort
  result = [...result].sort((a, b) => {
    const aVal = a[sortBy.value as keyof Pet]
    const bVal = b[sortBy.value as keyof Pet]

    if (aVal === bVal) return 0
    if (aVal == null) return 1
    if (bVal == null) return -1

    const result = aVal < bVal ? -1 : 1
    return sortingOrder.value === 'asc' ? result : -result
  })

  pagination.value.total = result.length
  return result
})

const paginatedPets = computed(() => {
  if (viewMode.value === 'table') return filteredPets.value

  const start = (pagination.value.page - 1) * pagination.value.perPage
  const end = start + pagination.value.perPage
  return filteredPets.value.slice(start, end)
})

// Load pets
const loadPets = async () => {
  loading.value = true
  try {
    const response = await petApi.getMyPets()
    pets.value = response.data
  } catch (error: any) {
    notify({ message: '加载宠物列表失败', color: 'danger' })
  } finally {
    loading.value = false
  }
}

// Edit pet
const editPet = (pet: Pet) => {
  editedPet.value = { ...pet }
  showEditModal.value = true
}

// Delete pet
// Confirm delete
const confirmDelete = (pet: Pet) => {
  deleteTarget.value = pet
  showDeleteConfirm.value = true
}

// Delete pet
const handleDeleteConfirm = async () => {
  if (!deleteTarget.value) return

  try {
    await petApi.deletePet(deleteTarget.value.id)
    notify({ message: t('pets.deleteSuccess'), color: 'success' })
    deleteTarget.value = null
    await loadPets()
  } catch (error: any) {
    notify({ message: t('pets.deleteFailed'), color: 'danger' })
  }
}

// Save pet
const savePet = async () => {
  try {
    if (editedPet.value.id) {
      await petApi.updatePet(editedPet.value.id, editedPet.value)
      notify({ message: '宠物信息已更新', color: 'success' })
    } else {
      await petApi.createPet(editedPet.value as Omit<Pet, 'id' | 'userId' | 'createdAt' | 'updatedAt'>)
      notify({ message: '宠物已添加', color: 'success' })
    }
    showAddModal.value = false
    showEditModal.value = false
    await loadPets()
  } catch (error: any) {
    notify({ message: '保存宠物信息失败', color: 'danger' })
  }
}

// Cancel edit
const cancelEdit = () => {
  editedPet.value = {}
  showAddModal.value = false
  showEditModal.value = false
}

onMounted(() => {
  loadPets()
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}
</style>

