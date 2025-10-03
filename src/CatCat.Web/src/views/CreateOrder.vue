<template>
  <div class="create-order-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <va-button preset="plain" icon="arrow_back" @click="$router.back()" />
          <h1 class="va-h1">Create Order</h1>
          <div></div>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Progress Steps -->
        <va-stepper v-model="currentStep" :steps="steps" :controls-hidden="true" />

        <!-- Step 1: Pet & Service -->
        <div v-show="currentStep === 0" class="step-content">
          <h3 class="va-h3">Select Pet & Service</h3>

          <!-- Loading -->
          <div v-if="loading" class="loading-state">
            <va-skeleton height="60px" class="mb-3" />
            <va-skeleton height="60px" class="mb-3" />
          </div>

          <!-- Pet Selection -->
          <va-select
            v-else
            v-model="formData.petId"
            label="Select Pet"
            :options="pets"
            text-by="name"
            value-by="id"
            class="mb-4"
          >
            <template #prepend>
              <va-icon name="pets" />
            </template>
          </va-select>

          <va-alert v-if="!pets.length && !loading" color="info" class="mb-4">
            No pets found. <router-link to="/pets">Add a pet</router-link> first.
          </va-alert>

          <!-- Package Selection -->
          <va-select
            v-model="formData.packageId"
            label="Select Service Package"
            :options="packages"
            text-by="name"
            value-by="id"
          >
            <template #prepend>
              <va-icon name="category" />
            </template>
          </va-select>

          <!-- Package Preview -->
          <va-card v-if="selectedPackage" class="package-preview mt-4" outlined>
            <va-card-content>
              <div class="preview-row">
                <span>Duration:</span>
                <strong>{{ selectedPackage.duration }} min</strong>
              </div>
              <div class="preview-row">
                <span>Price:</span>
                <strong class="price">¥{{ selectedPackage.price }}</strong>
              </div>
              <div class="preview-row">
                <span>Items:</span>
                <span>{{ selectedPackage.serviceItems }}</span>
              </div>
            </va-card-content>
          </va-card>
        </div>

        <!-- Step 2: Time & Location -->
        <div v-show="currentStep === 1" class="step-content">
          <h3 class="va-h3">Schedule Service</h3>

          <va-date-input
            v-model="formData.serviceDate"
            label="Service Date"
            class="mb-4"
          >
            <template #prepend>
              <va-icon name="calendar_today" />
            </template>
          </va-date-input>

          <va-time-input
            v-model="formData.serviceTime"
            label="Service Time"
            class="mb-4"
          >
            <template #prepend>
              <va-icon name="schedule" />
            </template>
          </va-time-input>

          <va-input
            v-model="formData.address"
            label="Service Address"
          >
            <template #prepend>
              <va-icon name="location_on" />
            </template>
          </va-input>
        </div>

        <!-- Step 3: Confirm -->
        <div v-show="currentStep === 2" class="step-content">
          <h3 class="va-h3">Confirm Order</h3>

          <va-card outlined class="summary-card">
            <va-card-content>
              <div class="summary-row">
                <va-icon name="pets" color="primary" />
                <span class="label">Pet:</span>
                <strong>{{ selectedPet?.name }}</strong>
              </div>
              <va-divider />
              <div class="summary-row">
                <va-icon name="category" color="primary" />
                <span class="label">Package:</span>
                <strong>{{ selectedPackage?.name }}</strong>
              </div>
              <va-divider />
              <div class="summary-row">
                <va-icon name="calendar_today" color="primary" />
                <span class="label">Date:</span>
                <strong>{{ formatDate(formData.serviceDate) }}</strong>
              </div>
              <va-divider />
              <div class="summary-row">
                <va-icon name="schedule" color="primary" />
                <span class="label">Time:</span>
                <strong>{{ formatTime(formData.serviceTime) }}</strong>
              </div>
              <va-divider />
              <div class="summary-row">
                <va-icon name="location_on" color="primary" />
                <span class="label">Address:</span>
                <strong>{{ formData.address }}</strong>
              </div>
              <va-divider />
              <div class="summary-row total-row">
                <va-icon name="payments" color="success" />
                <span class="label">Total:</span>
                <strong class="price">¥{{ selectedPackage?.price }}</strong>
              </div>
            </va-card-content>
          </va-card>

          <va-textarea
            v-model="formData.notes"
            label="Additional Notes (Optional)"
            placeholder="Any special requirements..."
            :max-rows="4"
            class="mt-4"
          />
        </div>

        <!-- Actions -->
        <div class="actions-bar">
          <va-button v-if="currentStep > 0" preset="secondary" @click="prevStep">
            <va-icon name="arrow_back" /> Back
          </va-button>
          <va-spacer />
          <va-button
            v-if="currentStep < 2"
            color="primary"
            :disabled="!canProceed"
            @click="nextStep"
          >
            Next <va-icon name="arrow_forward" />
          </va-button>
          <va-button
            v-else
            color="success"
            :disabled="!canProceed || submitting"
            :loading="submitting"
            @click="submitOrder"
          >
            <va-icon name="check" /> Confirm Order
          </va-button>
        </div>
      </va-card-content>
    </va-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyPets } from '@/api/pets'
import { getServicePackages } from '@/api/packages'
import { createOrder } from '@/api/orders'
import { useToast } from 'vuestic-ui'

const { init: notify } = useToast()
const router = useRouter()

const currentStep = ref(0)
const loading = ref(false)
const submitting = ref(false)

const pets = ref<any[]>([])
const packages = ref<any[]>([])

const formData = ref({
  petId: null as number | null,
  packageId: null as number | null,
  serviceDate: undefined as Date | undefined,
  serviceTime: undefined as Date | undefined,
  address: '',
  notes: ''
})

const steps = [
  { label: 'Pet & Service' },
  { label: 'Time & Location' },
  { label: 'Confirm' }
]

const selectedPet = computed(() => pets.value.find(p => p.id === formData.value.petId))
const selectedPackage = computed(() => packages.value.find(p => p.id === formData.value.packageId))

const canProceed = computed(() => {
  if (currentStep.value === 0) {
    return formData.value.petId && formData.value.packageId
  }
  if (currentStep.value === 1) {
    return formData.value.serviceDate && formData.value.serviceTime && formData.value.address
  }
  return true
})

const formatDate = (date: Date | undefined) => {
  if (!date) return ''
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const formatTime = (time: Date | undefined) => {
  if (!time) return ''
  return new Date(time).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const fetchData = async () => {
  loading.value = true
  try {
    const [petsRes, packagesRes] = await Promise.all([
      getMyPets(),
      getServicePackages()
    ])
    pets.value = petsRes.data
    packages.value = packagesRes.data
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load data', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const nextStep = () => {
  if (canProceed.value && currentStep.value < 2) {
    currentStep.value++
  }
}

const prevStep = () => {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

const submitOrder = async () => {
  if (!canProceed.value) return

  submitting.value = true
  try {
    const serviceDate = formData.value.serviceDate 
      ? new Date(formData.value.serviceDate).toISOString().split('T')[0]
      : ''
    
    const serviceTime = formData.value.serviceTime
      ? new Date(formData.value.serviceTime).toTimeString().slice(0, 5)
      : ''

    await createOrder({
      petId: formData.value.petId!,
      servicePackageId: formData.value.packageId!,
      serviceDate,
      serviceTime,
      address: formData.value.address,
      customerRemark: formData.value.notes
    })

    notify({ message: 'Order created successfully! Processing...', color: 'success' })
    router.push('/orders')
  } catch (error: any) {
    notify({ message: error.message || 'Failed to create order', color: 'danger' })
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.create-order-page {
  padding: var(--va-content-padding);
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.page-header h1 {
  margin: 0;
  flex: 1;
  text-align: center;
}

.step-content {
  margin: 24px 0;
  min-height: 300px;
}

.step-content h3 {
  margin-bottom: 24px;
}

.loading-state {
  margin: 24px 0;
}

.package-preview {
  margin-top: 16px;
}

.preview-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.preview-row:last-child {
  margin-bottom: 0;
}

.price {
  color: var(--va-success);
  font-size: 18px;
  font-weight: 700;
}

.summary-card {
  margin-bottom: 16px;
}

.summary-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
}

.summary-row .label {
  min-width: 80px;
  color: var(--va-text-secondary);
}

.summary-row strong {
  flex: 1;
  text-align: right;
}

.total-row {
  font-size: 18px;
}

.total-row .price {
  font-size: 24px;
}

.actions-bar {
  display: flex;
  gap: 12px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid var(--va-background-border);
}

@media (max-width: 768px) {
  .create-order-page {
    padding: 12px;
  }

  .page-header h1 {
    font-size: 20px;
  }

  .step-content {
    min-height: 250px;
  }

  .summary-row {
    font-size: 14px;
  }

  .total-row {
    font-size: 16px;
  }

  .total-row .price {
    font-size: 20px;
  }
}
</style>
