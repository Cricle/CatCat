<template>
  <div class="pets-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <h1 class="va-h1">My Pets</h1>
          <va-button color="primary" @click="showAddDialog = true">
            <va-icon name="add" /> Add Pet
          </va-button>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Loading State -->
        <div v-if="loading" class="loading-container">
          <div class="va-row">
            <div v-for="i in 3" :key="i" class="flex xs12 sm6 md4">
              <va-skeleton height="200px" />
            </div>
          </div>
        </div>

        <!-- Empty State -->
        <va-card v-else-if="pets.length === 0" class="empty-state">
          <va-card-content>
            <div class="text-center">
              <va-icon name="pets" size="64px" color="secondary" />
              <h3 class="va-h3">No Pets Yet</h3>
              <p class="va-text-secondary">Add your first pet profile to get started</p>
              <va-button color="primary" @click="showAddDialog = true">
                <va-icon name="add" /> Add Pet
              </va-button>
            </div>
          </va-card-content>
        </va-card>

        <!-- Pet Grid -->
        <div v-else class="va-row">
          <div v-for="pet in pets" :key="pet.id" class="flex xs12 sm6 md4">
            <va-card hover class="pet-card" @click="editPet(pet)">
              <va-card-content>
                <div class="pet-avatar-wrapper">
                  <va-avatar size="80px" :src="pet.avatar || getDefaultAvatar(pet.gender)" />
                  <va-chip v-if="pet.breed" size="small" color="primary">{{ pet.breed }}</va-chip>
                </div>

                <h3 class="va-h3 pet-name">{{ pet.name }}</h3>

                <div class="pet-meta">
                  <va-chip size="small" outline>
                    <va-icon name="cake" size="small" /> {{ pet.age }} years
                  </va-chip>
                  <va-chip size="small" outline>
                    <va-icon :name="pet.gender === 1 ? 'male' : 'female'" size="small" />
                    {{ pet.gender === 1 ? 'Male' : 'Female' }}
                  </va-chip>
                </div>

                <p v-if="pet.healthStatus" class="pet-health va-text-secondary">
                  {{ pet.healthStatus }}
                </p>

                <va-divider />

                <div class="pet-actions">
                  <va-button size="small" preset="plain" icon="edit" @click.stop="editPet(pet)">
                    Edit
                  </va-button>
                  <va-button
                    size="small"
                    preset="plain"
                    icon="delete"
                    color="danger"
                    @click.stop="deletePetHandler(pet)"
                  >
                    Delete
                  </va-button>
                </div>
              </va-card-content>
            </va-card>
          </div>
        </div>
      </va-card-content>
    </va-card>

    <!-- Add/Edit Pet Modal -->
    <va-modal v-model="showAddDialog" :title="editingPet ? 'Edit Pet' : 'Add Pet'" size="medium">
      <va-form ref="formRef">
        <va-input
          v-model="form.name"
          label="Pet Name"
          :rules="[(v: string) => !!v || 'Name is required']"
        />
        <va-input v-model="form.breed" label="Breed" />
        <va-input v-model.number="form.age" label="Age (years)" type="number" />
        <va-select
          v-model="form.gender"
          label="Gender"
          :options="[
            { text: 'Male', value: 1 },
            { text: 'Female', value: 2 }
          ]"
        />
        <va-input v-model.number="form.weight" label="Weight (kg)" type="number" />
        <va-input v-model="form.avatar" label="Avatar URL" />
        <va-textarea v-model="form.character" label="Character" />
        <va-textarea v-model="form.dietaryHabits" label="Dietary Habits" />
        <va-textarea v-model="form.healthStatus" label="Health Status" />
        <va-textarea v-model="form.remarks" label="Remarks" />
      </va-form>

      <template #footer>
        <va-button preset="plain" @click="showAddDialog = false">Cancel</va-button>
        <va-button color="primary" @click="savePet">Save</va-button>
      </template>
    </va-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getMyPets, createPet, updatePet, deletePet as deletePetApi } from '@/api/pets'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()

const loading = ref(false)
const pets = ref<any[]>([])
const showAddDialog = ref(false)
const editingPet = ref<any>(null)
const form = ref({
  id: 0,
  name: '',
  breed: '',
  age: 1,
  type: 1,
  gender: 1,
  weight: 0,
  avatar: '',
  character: '',
  dietaryHabits: '',
  healthStatus: '',
  remarks: ''
})

const getDefaultAvatar = (gender: number) => {
  return gender === 1
    ? 'https://cdn.jsdelivr.net/npm/@vuestic/ui/dist/themes/icons/cat-male.svg'
    : 'https://cdn.jsdelivr.net/npm/@vuestic/ui/dist/themes/icons/cat-female.svg'
}

const fetchPets = async () => {
  loading.value = true
  try {
    const res = await getMyPets()
    pets.value = res.data
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load pets', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const editPet = (pet: any) => {
  editingPet.value = pet
  form.value = { ...pet }
  showAddDialog.value = true
}

const savePet = async () => {
  try {
    if (editingPet.value) {
      await updatePet(form.value.id, form.value)
      notify({ message: 'Pet updated successfully', color: 'success' })
    } else {
      await createPet(form.value)
      notify({ message: 'Pet added successfully', color: 'success' })
    }
    showAddDialog.value = false
    resetForm()
    fetchPets()
  } catch (error: any) {
    notify({ message: error.message || 'Failed to save pet', color: 'danger' })
  }
}

const deletePetHandler = async (pet: any) => {
  const agreed = await confirm({
    title: 'Delete Pet',
    message: `Are you sure you want to delete "${pet.name}"?`,
    okText: 'Delete',
    cancelText: 'Cancel'
  })

  if (agreed) {
    try {
      await deletePetApi(pet.id)
      notify({ message: 'Pet deleted successfully', color: 'success' })
      fetchPets()
    } catch (error: any) {
      notify({ message: error.message || 'Failed to delete pet', color: 'danger' })
    }
  }
}

const resetForm = () => {
  editingPet.value = null
  form.value = {
    id: 0,
    name: '',
    breed: '',
    age: 1,
    type: 1,
    gender: 1,
    weight: 0,
    avatar: '',
    character: '',
    dietaryHabits: '',
    healthStatus: '',
    remarks: ''
  }
}

onMounted(() => {
  fetchPets()
})
</script>

<style scoped>
.pets-page {
  padding: var(--va-content-padding);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.loading-container {
  padding: var(--va-content-padding) 0;
}

.empty-state {
  margin-top: var(--va-content-padding);
}

.text-center {
  text-align: center;
  padding: var(--va-content-padding);
}

.pet-card {
  width: 100%;
  height: 100%;
  margin-bottom: var(--va-content-padding);
  cursor: pointer;
  transition: transform 0.2s;
}

.pet-card:hover {
  transform: translateY(-4px);
}

.pet-avatar-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
}

.pet-name {
  text-align: center;
  margin: 12px 0;
}

.pet-meta {
  display: flex;
  justify-content: center;
  gap: 8px;
  flex-wrap: wrap;
  margin-bottom: 12px;
}

.pet-health {
  text-align: center;
  font-size: 14px;
  min-height: 40px;
  margin: 12px 0;
}

.pet-actions {
  display: flex;
  justify-content: center;
  gap: 8px;
  margin-top: 12px;
}

@media (max-width: 768px) {
  .pets-page {
    padding: 12px;
  }

  .pet-card {
    margin-bottom: 12px;
  }
}
</style>



