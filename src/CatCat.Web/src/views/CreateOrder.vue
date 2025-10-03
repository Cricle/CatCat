<template>
  <div class="create-order-page">
    <!-- Page Header -->
    <div class="page-header">
      <va-button preset="plain" icon="arrow_back" @click="goBack">
        {{ t('common.back') }}
      </va-button>
      <h1 class="page-title">{{ t('order.createOrder') }}</h1>
    </div>

    <!-- Step Indicator -->
    <div class="step-indicator">
      <div
        v-for="(step, index) in steps"
        :key="step.key"
        class="step-item"
        :class="{ active: currentStep === index, completed: currentStep > index }"
      >
        <div class="step-circle">
          <va-icon v-if="currentStep > index" name="check" size="small" color="white" />
          <span v-else>{{ index + 1 }}</span>
        </div>
        <div class="step-label">{{ step.label }}</div>
        <div v-if="index < steps.length - 1" class="step-line"></div>
      </div>
    </div>

    <!-- Step Content -->
    <va-card class="step-content">
      <va-card-content>
        <!-- Step 1: Select Pet -->
        <div v-if="currentStep === 0" class="step-section">
          <h2 class="section-title">
            <va-icon name="pets" /> {{ t('order.selectPet') }}
          </h2>
          <p class="section-desc">{{ t('order.selectPetDesc') }}</p>

          <div v-if="pets.length === 0" class="empty-state">
            <va-icon name="pets" size="64px" color="secondary" />
            <p>{{ t('pet.noPets') }}</p>
            <va-button @click="$router.push('/pets')">
              {{ t('pet.addPet') }}
            </va-button>
          </div>

          <div v-else class="pets-grid">
            <div
              v-for="pet in pets"
              :key="pet.id"
              class="pet-option"
              :class="{ selected: orderForm.petId === pet.id }"
              @click="orderForm.petId = pet.id"
            >
              <div class="pet-avatar" :style="{ background: getPetGradient(pet.type) }">
                <va-icon name="pets" size="32px" color="white" />
              </div>
              <div class="pet-info">
                <h4>{{ pet.name }}</h4>
                <p>{{ pet.age }} {{ t('pet.years') }} · {{ pet.breed || t(`pet.${getPetTypeKey(pet.type)}`) }}</p>
              </div>
              <va-icon
                v-if="orderForm.petId === pet.id"
                name="check_circle"
                color="primary"
                size="large"
              />
            </div>
          </div>
        </div>

        <!-- Step 2: Select Package -->
        <div v-if="currentStep === 1" class="step-section">
          <h2 class="section-title">
            <va-icon name="business_center" /> {{ t('order.selectPackage') }}
          </h2>
          <p class="section-desc">{{ t('order.selectPackageDesc') }}</p>

          <div class="packages-grid">
            <div
              v-for="pkg in packages"
              :key="pkg.id"
              class="package-option"
              :class="{ selected: orderForm.servicePackageId === pkg.id, recommended: pkg.isRecommended }"
              @click="orderForm.servicePackageId = pkg.id"
            >
              <va-chip v-if="pkg.isRecommended" size="small" color="warning" class="recommended-tag">
                {{ t('common.recommended') }}
              </va-chip>
              <h3>{{ pkg.name }}</h3>
              <div class="package-price">
                <span class="price">¥{{ pkg.price }}</span>
                <span class="duration">/ {{ pkg.duration }}{{ t('common.minutes') }}</span>
              </div>
              <p class="package-desc">{{ pkg.description }}</p>
              <div class="package-features">
                <div v-for="feature in pkg.features" :key="feature" class="feature-item">
                  <va-icon name="check" size="small" color="success" />
                  <span>{{ feature }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Step 3: Set Date & Time -->
        <div v-if="currentStep === 2" class="step-section">
          <h2 class="section-title">
            <va-icon name="event" /> {{ t('order.setDateTime') }}
          </h2>
          <p class="section-desc">{{ t('order.setDateTimeDesc') }}</p>

          <div class="form-grid">
            <va-date-input
              v-model="orderForm.serviceDate"
              :label="t('order.serviceDate')"
              :min-date="new Date()"
              :rules="[(v) => !!v || t('order.serviceDateRequired')]"
            />

            <va-time-input
              v-model="orderForm.serviceTime"
              :label="t('order.serviceTime')"
              :rules="[(v) => !!v || t('order.serviceTimeRequired')]"
            />
          </div>

          <va-input
            v-model="orderForm.address"
            :label="t('order.address')"
            :placeholder="t('order.addressPlaceholder')"
            :rules="[(v) => !!v || t('order.addressRequired')]"
          >
            <template #prepend><va-icon name="location_on" /></template>
          </va-input>

          <va-textarea
            v-model="orderForm.addressDetail"
            :label="t('order.addressDetail')"
            :placeholder="t('order.addressDetailPlaceholder')"
            :min-rows="3"
          />

          <va-textarea
            v-model="orderForm.customerRemark"
            :label="t('order.remark')"
            :placeholder="t('order.remarkPlaceholder')"
            :min-rows="3"
          />
        </div>

        <!-- Step 4: Review & Confirm -->
        <div v-if="currentStep === 3" class="step-section">
          <h2 class="section-title">
            <va-icon name="preview" /> {{ t('order.reviewOrder') }}
          </h2>
          <p class="section-desc">{{ t('order.reviewOrderDesc') }}</p>

          <div class="review-card">
            <div class="review-item">
              <span class="review-label">{{ t('order.pet') }}</span>
              <span class="review-value">{{ getSelectedPetName() }}</span>
            </div>
            <va-divider />
            <div class="review-item">
              <span class="review-label">{{ t('order.package') }}</span>
              <span class="review-value">{{ getSelectedPackageName() }}</span>
            </div>
            <va-divider />
            <div class="review-item">
              <span class="review-label">{{ t('order.serviceDate') }}</span>
              <span class="review-value">{{ formatDate(orderForm.serviceDate) }}</span>
            </div>
            <va-divider />
            <div class="review-item">
              <span class="review-label">{{ t('order.serviceTime') }}</span>
              <span class="review-value">{{ formatTime(orderForm.serviceTime) }}</span>
            </div>
            <va-divider />
            <div class="review-item">
              <span class="review-label">{{ t('order.address') }}</span>
              <span class="review-value">{{ orderForm.address }}</span>
            </div>
            <va-divider />
            <div class="review-item total">
              <span class="review-label">{{ t('order.totalPrice') }}</span>
              <span class="review-value price">¥{{ getTotalPrice() }}</span>
            </div>
          </div>
        </div>
      </va-card-content>
    </va-card>

    <!-- Navigation Buttons -->
    <div class="nav-buttons">
      <va-button
        v-if="currentStep > 0"
        size="large"
        preset="secondary"
        @click="prevStep"
      >
        {{ t('common.previous') }}
      </va-button>
      <va-button
        v-if="currentStep < steps.length - 1"
        size="large"
        @click="nextStep"
        :disabled="!canProceed()"
      >
        {{ t('common.next') }}
      </va-button>
      <va-button
        v-if="currentStep === steps.length - 1"
        size="large"
        color="primary"
        :loading="submitting"
        @click="submitOrder"
      >
        {{ t('order.submitOrder') }}
      </va-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { getMyPets } from '@/api/pets'
import { createOrder } from '@/api/orders'

const { t } = useI18n()
const { init: notify } = useToast()
const router = useRouter()

const currentStep = ref(0)
const submitting = ref(false)
const pets = ref<any[]>([])
const packages = ref<any[]>([])

const steps = [
  { key: 'pet', label: t('order.selectPet') },
  { key: 'package', label: t('order.selectPackage') },
  { key: 'datetime', label: t('order.setDateTime') },
  { key: 'review', label: t('order.reviewOrder') }
]

const orderForm = ref({
  petId: 0,
  servicePackageId: 0,
  serviceDate: null as Date | null,
  serviceTime: new Date(),
  address: '',
  addressDetail: '',
  customerRemark: ''
})

const getPetGradient = (type: number) => {
  const gradients: Record<number, string> = {
    1: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
    2: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
    99: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)'
  }
  return gradients[type] || gradients[99]
}

const getPetTypeKey = (type: number) => {
  return type === 1 ? 'cat' : type === 2 ? 'dog' : 'other'
}

const fetchPets = async () => {
  try {
    const res = await getMyPets()
    pets.value = res.data
  } catch (error: any) {
    notify({ message: error.message || t('pet.failedToLoad'), color: 'danger' })
  }
}

const fetchPackages = async () => {
  try {
    // Mock packages data (will be replaced with real API)
    packages.value = [
      {
        id: 1,
        name: t('order.basicPackage'),
        price: 50,
        duration: 30,
        description: t('order.basicPackageDesc'),
        isRecommended: false,
        features: [t('order.feature1'), t('order.feature2'), t('order.feature3')]
      },
      {
        id: 2,
        name: t('order.standardPackage'),
        price: 80,
        duration: 60,
        description: t('order.standardPackageDesc'),
        isRecommended: true,
        features: [t('order.feature1'), t('order.feature2'), t('order.feature3'), t('order.feature4')]
      },
      {
        id: 3,
        name: t('order.premiumPackage'),
        price: 120,
        duration: 90,
        description: t('order.premiumPackageDesc'),
        isRecommended: false,
        features: [t('order.feature1'), t('order.feature2'), t('order.feature3'), t('order.feature4'), t('order.feature5')]
      }
    ]
  } catch (error: any) {
    notify({ message: error.message || t('order.failedToLoadPackages'), color: 'danger' })
  }
}

const canProceed = () => {
  switch (currentStep.value) {
    case 0:
      return orderForm.value.petId > 0
    case 1:
      return orderForm.value.servicePackageId > 0
    case 2:
      return orderForm.value.serviceDate && orderForm.value.serviceTime && orderForm.value.address
    case 3:
      return true
    default:
      return false
  }
}

const nextStep = () => {
  if (canProceed() && currentStep.value < steps.length - 1) {
    currentStep.value++
  }
}

const prevStep = () => {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

const goBack = () => {
  router.back()
}

const getSelectedPetName = () => {
  const pet = pets.value.find(p => p.id === orderForm.value.petId)
  return pet?.name || '-'
}

const getSelectedPackageName = () => {
  const pkg = packages.value.find(p => p.id === orderForm.value.servicePackageId)
  return pkg?.name || '-'
}

const getTotalPrice = () => {
  const pkg = packages.value.find(p => p.id === orderForm.value.servicePackageId)
  return pkg?.price || 0
}

const formatDate = (date: Date | null) => {
  if (!date) return '-'
  return new Date(date).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const formatTime = (time: Date) => {
  return time.toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const submitOrder = async () => {
  submitting.value = true
  try {
    await createOrder({
      petId: orderForm.value.petId,
      servicePackageId: orderForm.value.servicePackageId,
      serviceDate: orderForm.value.serviceDate?.toISOString().split('T')[0] || '',
      serviceTime: formatTime(orderForm.value.serviceTime),
      address: orderForm.value.address,
      addressDetail: orderForm.value.addressDetail,
      customerRemark: orderForm.value.customerRemark
    })
    notify({ message: t('order.createSuccess'), color: 'success' })
    router.push('/orders')
  } catch (error: any) {
    notify({ message: error.message, color: 'danger' })
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  fetchPets()
  fetchPackages()
})
</script>

<style scoped>
.create-order-page {
  padding: 24px;
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 32px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0;
}

/* Step Indicator */
.step-indicator {
  display: flex;
  justify-content: space-between;
  margin-bottom: 32px;
  position: relative;
}

.step-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  position: relative;
}

.step-circle {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: #e5e5e7;
  color: #6e6e73;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 18px;
  margin-bottom: 12px;
  transition: all 0.3s ease;
  z-index: 10;
}

.step-item.active .step-circle {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
  transform: scale(1.1);
}

.step-item.completed .step-circle {
  background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
  color: white;
}

.step-label {
  font-size: 14px;
  color: #6e6e73;
  text-align: center;
}

.step-item.active .step-label {
  color: #667eea;
  font-weight: 600;
}

.step-line {
  position: absolute;
  top: 24px;
  left: 50%;
  width: 100%;
  height: 2px;
  background: #e5e5e7;
  z-index: 1;
}

.step-item.completed .step-line {
  background: linear-gradient(to right, #43e97b, #38f9d7);
}

/* Step Content */
.step-content {
  margin-bottom: 24px;
  min-height: 400px;
}

.step-section {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.section-title {
  font-size: 24px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0 0 8px 0;
  display: flex;
  align-items: center;
  gap: 12px;
}

.section-desc {
  font-size: 14px;
  color: #6e6e73;
  margin: 0 0 24px 0;
}

/* Pets Grid */
.pets-grid {
  display: grid;
  gap: 16px;
}

.pet-option {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  border: 2px solid #e5e5e7;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.pet-option:hover {
  border-color: #667eea;
  transform: translateX(4px);
}

.pet-option.selected {
  border-color: #667eea;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
}

.pet-avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.pet-info {
  flex: 1;
}

.pet-info h4 {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
  margin: 0 0 4px 0;
}

.pet-info p {
  font-size: 14px;
  color: #6e6e73;
  margin: 0;
}

/* Packages Grid */
.packages-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
}

.package-option {
  position: relative;
  padding: 24px;
  border: 2px solid #e5e5e7;
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.package-option:hover {
  border-color: #667eea;
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.package-option.selected {
  border-color: #667eea;
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.05) 0%, rgba(118, 75, 162, 0.05) 100%);
}

.package-option.recommended {
  border-color: #ffa726;
}

.recommended-tag {
  position: absolute;
  top: 12px;
  right: 12px;
}

.package-option h3 {
  font-size: 20px;
  font-weight: 700;
  color: #1d1d1f;
  margin: 0 0 12px 0;
}

.package-price {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 16px;
}

.price {
  font-size: 32px;
  font-weight: 700;
  color: #667eea;
}

.duration {
  font-size: 14px;
  color: #6e6e73;
}

.package-desc {
  font-size: 14px;
  color: #6e6e73;
  margin: 0 0 16px 0;
}

.package-features {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #1d1d1f;
}

/* Form Grid */
.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin-bottom: 16px;
}

/* Review Card */
.review-card {
  background: #f5f5f7;
  border-radius: 12px;
  padding: 24px;
}

.review-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
}

.review-label {
  font-size: 14px;
  color: #6e6e73;
}

.review-value {
  font-size: 16px;
  font-weight: 500;
  color: #1d1d1f;
  text-align: right;
}

.review-item.total {
  padding-top: 24px;
}

.review-item.total .review-label {
  font-size: 18px;
  font-weight: 600;
  color: #1d1d1f;
}

.review-item.total .review-value.price {
  font-size: 32px;
  font-weight: 700;
  color: #667eea;
}

/* Navigation Buttons */
.nav-buttons {
  display: flex;
  justify-content: flex-end;
  gap: 16px;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 60px 40px;
}

.empty-state p {
  font-size: 16px;
  color: #6e6e73;
  margin: 16px 0 24px 0;
}

/* Responsive */
@media (max-width: 768px) {
  .create-order-page {
    padding: 16px;
  }

  .page-title {
    font-size: 24px;
  }

  .step-circle {
    width: 40px;
    height: 40px;
    font-size: 16px;
  }

  .step-label {
    font-size: 12px;
  }

  .packages-grid {
    grid-template-columns: 1fr;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .nav-buttons {
    flex-direction: column-reverse;
  }

  .nav-buttons button {
    width: 100%;
  }
}
</style>
