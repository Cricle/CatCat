<template>
  <h1 class="page-title">我的宠物</h1>

  <VaCard>
    <VaCardContent>
      <div class="flex flex-col md:flex-row gap-2 mb-2 justify-between">
        <div class="flex flex-col md:flex-row gap-2 flex-grow">
          <VaInput v-model="filter" placeholder="搜索宠物名称..." class="flex-grow">
            <template #prependInner>
              <VaIcon name="search" color="secondary" />
            </template>
          </VaInput>
          <VaButton @click="showAddModal = true">添加宠物</VaButton>
        </div>
      </div>

      <PetsTable
        v-model:sort-by="sortBy"
        v-model:sorting-order="sortingOrder"
        :pets="filteredPets"
        :loading="loading"
        :pagination="pagination"
        @edit-pet="editPet"
        @delete-pet="deletePet"
      />
    </VaCardContent>
  </VaCard>

  <!-- Add/Edit Pet Modal -->
  <VaModal
    v-model="showAddModal"
    title="添加宠物"
    size="large"
    @ok="savePet"
    @cancel="cancelEdit"
  >
    <PetForm v-model="editedPet" />
  </VaModal>

  <VaModal
    v-model="showEditModal"
    title="编辑宠物"
    size="large"
    @ok="savePet"
    @cancel="cancelEdit"
  >
    <PetForm v-model="editedPet" />
  </VaModal>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from 'vuestic-ui'
import { petApi } from '../../services/catcat-api'
import type { Pet } from '../../types/catcat-types'
import PetsTable from './widgets/PetsTable.vue'
import PetForm from './widgets/PetForm.vue'

const { init: notify } = useToast()

const pets = ref<Pet[]>([])
const loading = ref(false)
const filter = ref('')
const sortBy = ref('name')
const sortingOrder = ref<'asc' | 'desc'>('asc')

const showAddModal = ref(false)
const showEditModal = ref(false)
const editedPet = ref<Partial<Pet>>({})

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
const deletePet = async (pet: Pet) => {
  if (!confirm(`确定要删除宠物 "${pet.name}" 吗？`)) return

  try {
    await petApi.deletePet(pet.id)
    notify({ message: '宠物已删除', color: 'success' })
    await loadPets()
  } catch (error: any) {
    notify({ message: '删除宠物失败', color: 'danger' })
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

