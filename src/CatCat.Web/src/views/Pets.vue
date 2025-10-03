<template>
  <div class="pets-page">
    <!-- Page Header -->
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">{{ t('pet.title') }}</h1>
        <va-button size="large" @click="showAddPetModal" icon="add">
          {{ t('pet.addPet') }}
        </va-button>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="pets-grid">
      <va-card v-for="i in 3" :key="i" class="pet-card">
        <va-card-content>
          <va-skeleton height="120px" width="120px" variant="circle" style="margin: 0 auto 16px" />
          <va-skeleton height="24px" width="70%" style="margin: 0 auto 8px" />
          <va-skeleton height="16px" width="100%" style="margin-top: 16px" />
        </va-card-content>
      </va-card>
    </div>

    <!-- Empty State -->
    <va-card v-else-if="!loading && pets.length === 0" class="empty-state">
      <va-card-content>
        <div class="empty-content">
          <va-icon name="pets" size="80px" color="secondary" />
          <h3>{{ t('pet.noPets') }}</h3>
          <p>{{ t('pet.addFirstPet') }}</p>
          <va-button size="large" @click="showAddPetModal" icon="add">
            {{ t('pet.addPet') }}
          </va-button>
        </div>
      </va-card-content>
    </va-card>

    <!-- Pets Grid -->
    <div v-else class="pets-grid">
      <va-card
        v-for="pet in pets"
        :key="pet.id"
        class="pet-card"
        @click="viewPetDetail(pet)"
      >
        <va-card-content>
          <!-- Avatar -->
          <div class="pet-avatar-wrapper">
            <div class="pet-avatar" :style="{ background: getGradientByType(pet.type) }">
              <va-icon v-if="!pet.avatar" name="pets" size="48px" color="white" />
              <img v-else :src="pet.avatar" :alt="pet.name" />
            </div>
          </div>

          <!-- Pet Info -->
          <h3 class="pet-name">{{ pet.name }}</h3>
          <div class="pet-meta">
            <va-chip size="small" :color="getGenderColor(pet.gender)">
              {{ t(`pet.${getGenderKey(pet.gender)}`) }}
            </va-chip>
            <span class="pet-age">{{ pet.age }} {{ t('pet.years') }}</span>
          </div>

          <div class="pet-details">
            <div class="detail-item">
              <va-icon name="category" size="small" />
              <span>{{ pet.breed || t(`pet.${getPetTypeKey(pet.type)}`) }}</span>
            </div>
            <div v-if="pet.character" class="detail-item">
              <va-icon name="mood" size="small" />
              <span>{{ pet.character }}</span>
            </div>
          </div>

          <!-- Service Info Tags -->
          <div v-if="hasServiceInfo(pet)" class="service-tags">
            <va-chip v-if="pet.foodLocation" size="small" outline color="success">
              <va-icon name="restaurant" size="small" /> {{ t('pet.foodLocation') }}
            </va-chip>
            <va-chip v-if="pet.waterLocation" size="small" outline color="info">
              <va-icon name="water_drop" size="small" /> {{ t('pet.waterLocation') }}
            </va-chip>
            <va-chip v-if="pet.needsWaterRefill" size="small" outline color="warning">
              <va-icon name="water" size="small" /> {{ t('pet.needsWaterRefill') }}
            </va-chip>
          </div>

          <va-divider style="margin: 16px 0" />

          <!-- Actions -->
          <div class="pet-actions">
            <va-button preset="plain" size="small" @click.stop="editPet(pet)" icon="edit">
              {{ t('common.edit') }}
            </va-button>
            <va-button preset="plain" size="small" @click.stop="confirmDelete(pet)" icon="delete" color="danger">
              {{ t('common.delete') }}
            </va-button>
          </div>
        </va-card-content>
      </va-card>
    </div>

    <!-- Pet Modal (Add/Edit) -->
    <va-modal
      v-model="showModal"
      size="large"
      :title="isEditMode ? t('pet.editPet') : t('pet.addPet')"
      @ok="savePet"
      @cancel="closeModal"
    >
      <va-form ref="formRef">
        <div class="modal-content">
          <!-- Basic Information -->
          <div class="form-section">
            <h4 class="section-title">{{ t('common.basic') }}</h4>
            
            <va-input
              v-model="formData.name"
              :label="t('pet.petName')"
              :placeholder="t('pet.petName')"
              :rules="[(v) => !!v || t('pet.petNameRequired')]"
            />

            <div class="form-row">
              <va-select
                v-model="formData.type"
                :label="t('pet.petType')"
                :options="petTypes"
                :rules="[(v) => !!v || t('pet.petTypeRequired')]"
              />
              <va-select
                v-model="formData.gender"
                :label="t('pet.gender')"
                :options="genderOptions"
                :rules="[(v) => !!v || t('pet.genderRequired')]"
              />
            </div>

            <div class="form-row">
              <va-input
                v-model="formData.breed"
                :label="t('pet.breed')"
                :placeholder="t('pet.breed')"
              />
              <va-input
                v-model.number="formData.age"
                type="number"
                :label="t('pet.age')"
                :placeholder="t('pet.age')"
              />
            </div>

            <va-textarea
              v-model="formData.character"
              :label="t('pet.character')"
              :placeholder="t('pet.character')"
            />

            <va-textarea
              v-model="formData.dietaryHabits"
              :label="t('pet.dietaryHabits')"
              :placeholder="t('pet.dietaryHabits')"
            />
          </div>

          <!-- Service Information (解决上门服务痛点) -->
          <div class="form-section">
            <h4 class="section-title">
              <va-icon name="home_repair_service" /> {{ t('pet.serviceInfo') }}
            </h4>
            <p class="section-description">帮助服务员快速找到所需物品</p>

            <va-input
              v-model="formData.foodLocation"
              :label="t('pet.foodLocation')"
              :placeholder="t('pet.foodLocationPlaceholder')"
            >
              <template #prepend><va-icon name="restaurant" /></template>
            </va-input>

            <va-input
              v-model="formData.waterLocation"
              :label="t('pet.waterLocation')"
              :placeholder="t('pet.waterLocationPlaceholder')"
            >
              <template #prepend><va-icon name="water_drop" /></template>
            </va-input>

            <va-input
              v-model="formData.litterBoxLocation"
              :label="t('pet.litterBoxLocation')"
              :placeholder="t('pet.litterBoxLocationPlaceholder')"
            >
              <template #prepend><va-icon name="cleaning_services" /></template>
            </va-input>

            <va-input
              v-model="formData.cleaningSuppliesLocation"
              :label="t('pet.cleaningSuppliesLocation')"
              :placeholder="t('pet.cleaningSuppliesPlaceholder')"
            >
              <template #prepend><va-icon name="cleaning_services" /></template>
            </va-input>

            <va-checkbox v-model="formData.needsWaterRefill">
              {{ t('pet.needsWaterRefill') }}
            </va-checkbox>

            <va-textarea
              v-model="formData.specialInstructions"
              :label="t('pet.specialInstructions')"
              :placeholder="t('pet.specialInstructionsPlaceholder')"
              :min-rows="3"
            />
          </div>

          <!-- Health & Other -->
          <div class="form-section">
            <h4 class="section-title">{{ t('pet.healthStatus') }}</h4>

            <va-textarea
              v-model="formData.healthStatus"
              :label="t('pet.healthStatus')"
              :placeholder="t('pet.healthStatus')"
            />

            <va-textarea
              v-model="formData.remarks"
              :label="t('pet.remarks')"
              :placeholder="t('pet.remarks')"
            />
          </div>
        </div>
      </va-form>
    </va-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToast, useModal } from 'vuestic-ui'
import { getMyPets, createPet, updatePet, deletePet } from '@/api/pets'

const { t } = useI18n()
const { init: notify } = useToast()
const { confirm } = useModal()

const loading = ref(false)
const showModal = ref(false)
const isEditMode = ref(false)
const formRef = ref()
const pets = ref<any[]>([])

const formData = ref({
  id: 0,
  name: '',
  type: 1,
  breed: '',
  age: 0,
  gender: 1,
  avatar: '',
  character: '',
  dietaryHabits: '',
  healthStatus: '',
  remarks: '',
  foodLocation: '',
  waterLocation: '',
  litterBoxLocation: '',
  cleaningSuppliesLocation: '',
  needsWaterRefill: false,
  specialInstructions: ''
})

const petTypes = [
  { text: t('pet.cat'), value: 1 },
  { text: t('pet.dog'), value: 2 },
  { text: t('pet.other'), value: 99 }
]

const genderOptions = [
  { text: t('pet.male'), value: 1 },
  { text: t('pet.female'), value: 2 }
]

const getGradientByType = (type: number) => {
  const gradients = {
    1: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', // Cat
    2: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)', // Dog
    99: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)' // Other
  }
  return gradients[type as keyof typeof gradients] || gradients[99]
}

const getGenderColor = (gender: number) => {
  return gender === 1 ? 'info' : gender === 2 ? 'danger' : 'secondary'
}

const getGenderKey = (gender: number) => {
  return gender === 1 ? 'male' : gender === 2 ? 'female' : 'other'
}

const getPetTypeKey = (type: number) => {
  return type === 1 ? 'cat' : type === 2 ? 'dog' : 'other'
}

const hasServiceInfo = (pet: any) => {
  return pet.foodLocation || pet.waterLocation || pet.needsWaterRefill
}

const fetchPets = async () => {
  loading.value = true
  try {
    const res = await getMyPets()
    pets.value = res.data
  } catch (error: any) {
    notify({ message: error.message || t('pet.failedToLoad'), color: 'danger' })
  } finally {
    loading.value = false
  }
}

const showAddPetModal = () => {
  isEditMode.value = false
  resetForm()
  showModal.value = true
}

const editPet = (pet: any) => {
  isEditMode.value = true
  formData.value = { ...pet }
  showModal.value = true
}

const viewPetDetail = (pet: any) => {
  editPet(pet)
}

const savePet = async () => {
  if (!formRef.value?.validate()) return

  try {
    if (isEditMode.value) {
      await updatePet(formData.value.id, formData.value)
      notify({ message: t('pet.updateSuccess'), color: 'success' })
    } else {
      await createPet(formData.value)
      notify({ message: t('pet.createSuccess'), color: 'success' })
    }
    closeModal()
    fetchPets()
  } catch (error: any) {
    notify({ message: error.message, color: 'danger' })
  }
}

const confirmDelete = async (pet: any) => {
  const agreed = await confirm({
    title: t('common.confirm'),
    message: t('pet.deleteConfirm'),
    okText: t('common.confirm'),
    cancelText: t('common.cancel')
  })

  if (agreed) {
    try {
      await deletePet(pet.id)
      notify({ message: t('pet.deleteSuccess'), color: 'success' })
      fetchPets()
    } catch (error: any) {
      notify({ message: error.message, color: 'danger' })
    }
  }
}

const closeModal = () => {
  showModal.value = false
  resetForm()
}

const resetForm = () => {
  formData.value = {
    id: 0,
    name: '',
    type: 1,
    breed: '',
    age: 0,
    gender: 1,
    avatar: '',
    character: '',
    dietaryHabits: '',
    healthStatus: '',
    remarks: '',
    foodLocation: '',
    waterLocation: '',
    litterBoxLocation: '',
    cleaningSuppliesLocation: '',
    needsWaterRefill: false,
    specialInstructions: ''
  }
}

onMounted(() => {
  fetchPets()
})
</script>

<style scoped>
.pets-page {
  padding: 24px;
  max-width: 1200px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 32px;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.page-title {
  font-size: 32px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0;
}

/* Pets Grid */
.pets-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 24px;
}

.pet-card {
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid #e5e5e7;
}

.pet-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: transparent;
}

.pet-avatar-wrapper {
  text-align: center;
  margin-bottom: 16px;
}

.pet-avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.pet-avatar img {
  width: 100%;
  height: 100%;
  border-radius: 50%;
  object-fit: cover;
}

.pet-name {
  font-size: 20px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0 0 12px 0;
  text-align: center;
}

.pet-meta {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
}

.pet-age {
  font-size: 14px;
  color: #6e6e73;
}

.pet-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 16px;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #6e6e73;
}

.service-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 8px;
}

.pet-actions {
  display: flex;
  justify-content: space-between;
}

/* Empty State */
.empty-state {
  margin-top: 60px;
}

.empty-content {
  text-align: center;
  padding: 60px 40px;
}

.empty-content h3 {
  font-size: 24px;
  margin: 24px 0 12px 0;
  color: #1d1d1f;
}

.empty-content p {
  font-size: 16px;
  color: #6e6e73;
  margin: 0 0 32px 0;
}

/* Modal */
.modal-content {
  max-height: 60vh;
  overflow-y: auto;
}

.form-section {
  margin-bottom: 32px;
}

.section-title {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0 0 16px 0;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-description {
  font-size: 14px;
  color: #6e6e73;
  margin: -8px 0 16px 0;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

/* Responsive */
@media (max-width: 1024px) {
  .pets-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .pets-page {
    padding: 16px;
  }

  .page-title {
    font-size: 24px;
  }

  .pets-grid {
    grid-template-columns: 1fr;
  }

  .form-row {
    grid-template-columns: 1fr;
  }

  .header-content {
    flex-direction: column;
    align-items: stretch;
    gap: 16px;
  }
}
</style>
