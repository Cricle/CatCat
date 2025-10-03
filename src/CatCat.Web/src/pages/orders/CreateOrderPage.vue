<template>
  <h1 class="page-title">{{ t('orders.createOrder') }}</h1>

  <div class="max-w-4xl mx-auto">
    <VaStepper v-model="currentStep" :steps="steps" color="primary" class="mb-6">
      <template #step-button="{ setStep, index, isActive, isCompleted }">
        <VaButton
          :color="isCompleted ? 'success' : isActive ? 'primary' : 'secondary'"
          :disabled="index > currentStep"
          round
          @click="() => setStep(index)"
        >
          <VaIcon v-if="isCompleted" name="check" />
          <span v-else>{{ index + 1 }}</span>
        </VaButton>
      </template>
    </VaStepper>

    <VaCard>
      <VaCardContent>
        <!-- Step 1: Select Pet -->
        <div v-if="currentStep === 0" class="step-content">
          <h2 class="text-2xl font-bold mb-4">{{ t('orders.selectPet') }}</h2>

          <div v-if="loadingPets" class="flex justify-center py-8">
            <VaProgressCircle indeterminate />
          </div>

          <div v-else-if="pets.length === 0" class="text-center py-8">
            <VaIcon name="pets" size="4rem" color="secondary" />
            <p class="text-xl mt-4 text-secondary">{{ t('orders.noPets') }}</p>
            <VaButton class="mt-4" @click="$router.push('/pets')">
              {{ t('orders.addPetFirst') }}
            </VaButton>
          </div>

          <div v-else class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <VaCard
              v-for="pet in pets"
              :key="pet.id"
              :class="{ 'border-2 border-primary': orderForm.petId === pet.id }"
              class="cursor-pointer hover:shadow-md transition"
              @click="selectPet(pet)"
            >
              <VaCardContent>
                <div class="flex items-center gap-4">
                  <VaAvatar :src="pet.avatar" size="large" color="primary">
                    {{ pet.name?.charAt(0) }}
                  </VaAvatar>
                  <div class="flex-grow">
                    <h3 class="text-lg font-bold">{{ pet.name }}</h3>
                    <p class="text-sm text-secondary">
                      {{ getPetTypeText(pet.type) }} · {{ pet.age }}{{ t('dashboard.cards.yearsOld') }}
                      <span v-if="pet.breed"> · {{ pet.breed }}</span>
                    </p>
                  </div>
                  <VaIcon
                    v-if="orderForm.petId === pet.id"
                    name="check_circle"
                    color="success"
                    size="large"
                  />
                </div>
              </VaCardContent>
            </VaCard>
          </div>
        </div>

        <!-- Step 2: Select Package -->
        <div v-if="currentStep === 1" class="step-content">
          <h2 class="text-2xl font-bold mb-4">{{ t('orders.selectPackage') }}</h2>

          <div v-if="loadingPackages" class="flex justify-center py-8">
            <VaProgressCircle indeterminate />
          </div>

          <div v-else-if="packages.length === 0" class="text-center py-8">
            <VaIcon name="inventory_2" size="4rem" color="secondary" />
            <p class="text-xl mt-4 text-secondary">{{ t('packages.noPackages') }}</p>
          </div>

          <div v-else class="space-y-4">
            <VaCard
              v-for="pkg in packages"
              :key="pkg.id"
              :class="{ 'border-2 border-primary': orderForm.packageId === pkg.id }"
              class="cursor-pointer hover:shadow-md transition"
              @click="selectPackage(pkg)"
            >
              <VaCardContent>
                <div class="flex items-start justify-between">
                  <div class="flex-grow">
                    <h3 class="text-xl font-bold mb-2">{{ pkg.name }}</h3>
                    <p class="text-secondary mb-3">{{ pkg.description }}</p>
                    <div class="flex flex-wrap gap-2 mb-3">
                      <VaChip
                        v-for="service in pkg.servicesIncluded?.split(',') || []"
                        :key="service"
                        size="small"
                        color="info"
                        outline
                      >
                        {{ service.trim() }}
                      </VaChip>
                    </div>
                    <div class="flex gap-4 text-sm text-secondary">
                      <div class="flex items-center gap-1">
                        <VaIcon name="schedule" size="small" />
                        <span>{{ pkg.duration }}{{ t('packages.duration') }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="text-right ml-4">
                    <div class="text-2xl font-bold text-primary">¥{{ pkg.price }}</div>
                    <VaIcon
                      v-if="orderForm.packageId === pkg.id"
                      name="check_circle"
                      color="success"
                      size="large"
                      class="mt-2"
                    />
                  </div>
                </div>
              </VaCardContent>
            </VaCard>
          </div>
        </div>

        <!-- Step 3: Select Time and Address -->
        <div v-if="currentStep === 2" class="step-content">
          <h2 class="text-2xl font-bold mb-6">{{ t('orders.serviceDetails') }}</h2>

          <div class="space-y-4">
            <VaDateInput
              v-model="orderForm.serviceDate"
              :label="t('orders.serviceDate')"
              :min-date="new Date()"
              class="w-full"
            />

            <VaInput
              v-model="orderForm.serviceAddress"
              :label="t('orders.serviceAddress')"
              :placeholder="t('orders.serviceAddressPlaceholder')"
            />

            <VaTextarea
              v-model="orderForm.notes"
              :label="t('orders.notes')"
              :placeholder="t('orders.notesPlaceholder')"
              min-rows="4"
            />
          </div>
        </div>

        <!-- Step 4: Confirm -->
        <div v-if="currentStep === 3" class="step-content">
          <h2 class="text-2xl font-bold mb-6">{{ t('orders.confirmOrder') }}</h2>

          <div class="space-y-6">
            <div>
              <h3 class="font-semibold mb-2">{{ t('orders.petInfo') }}</h3>
              <VaCard color="backgroundBorder">
                <VaCardContent>
                  <div class="flex items-center gap-3">
                    <VaAvatar :src="selectedPet?.avatar" color="primary">
                      {{ selectedPet?.name?.charAt(0) }}
                    </VaAvatar>
                    <div>
                      <div class="font-semibold">{{ selectedPet?.name }}</div>
                      <div class="text-sm text-secondary">
                        {{ getPetTypeText(selectedPet?.type) }} · {{ selectedPet?.age }}{{ t('dashboard.cards.yearsOld') }}
                      </div>
                    </div>
                  </div>
                </VaCardContent>
              </VaCard>
            </div>

            <div>
              <h3 class="font-semibold mb-2">{{ t('orders.packageInfo') }}</h3>
              <VaCard color="backgroundBorder">
                <VaCardContent>
                  <div class="flex justify-between items-start">
                    <div>
                      <div class="font-semibold text-lg">{{ selectedPackage?.name }}</div>
                      <div class="text-secondary mt-1">{{ selectedPackage?.description }}</div>
                      <div class="text-sm text-secondary mt-2">
                        <VaIcon name="schedule" size="small" />
                        {{ selectedPackage?.duration }}{{ t('packages.duration') }}
                      </div>
                    </div>
                    <div class="text-2xl font-bold text-primary">¥{{ selectedPackage?.price }}</div>
                  </div>
                </VaCardContent>
              </VaCard>
            </div>

            <div>
              <h3 class="font-semibold mb-2">{{ t('orders.serviceInfo') }}</h3>
              <VaCard color="backgroundBorder">
                <VaCardContent>
                  <div class="space-y-2">
                    <div class="flex justify-between">
                      <span class="text-secondary">{{ t('orders.serviceDate') }}:</span>
                      <span class="font-semibold">{{ formatDate(orderForm.serviceDate) }}</span>
                    </div>
                    <div class="flex justify-between">
                      <span class="text-secondary">{{ t('orders.serviceAddress') }}:</span>
                      <span class="font-semibold">{{ orderForm.serviceAddress }}</span>
                    </div>
                    <div v-if="orderForm.notes">
                      <div class="text-secondary mb-1">{{ t('orders.notes') }}:</div>
                      <div class="font-semibold">{{ orderForm.notes }}</div>
                    </div>
                  </div>
                </VaCardContent>
              </VaCard>
            </div>

            <div>
              <h3 class="font-semibold mb-2">{{ t('orders.paymentSummary') }}</h3>
              <VaCard color="backgroundBorder">
                <VaCardContent>
                  <div class="space-y-2">
                    <div class="flex justify-between">
                      <span class="text-secondary">{{ t('orders.subtotal') }}:</span>
                      <span>¥{{ selectedPackage?.price }}</span>
                    </div>
                    <div class="flex justify-between text-xl font-bold">
                      <span>{{ t('orders.total') }}:</span>
                      <span class="text-primary">¥{{ selectedPackage?.price }}</span>
                    </div>
                  </div>
                </VaCardContent>
              </VaCard>
            </div>
          </div>
        </div>

        <!-- Navigation Buttons -->
        <div class="flex justify-between mt-8 pt-6 border-t">
          <VaButton v-if="currentStep > 0" preset="secondary" @click="prevStep">
            {{ t('common.back') }}
          </VaButton>
          <div v-else></div>

          <VaButton v-if="currentStep < 3" :disabled="!canProceed" @click="nextStep">
            {{ t('common.next') }}
          </VaButton>
          <VaButton v-else :loading="submitting" @click="submitOrder">
            {{ t('orders.submitOrder') }}
          </VaButton>
        </div>
      </VaCardContent>
    </VaCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useToast } from 'vuestic-ui'
import { petApi, packageApi, orderApi } from '../../services/catcat-api'
import type { Pet } from '../../types/catcat-types'

const { t } = useI18n()
const router = useRouter()
const { init: notify } = useToast()

const currentStep = ref(0)
const submitting = ref(false)
const loadingPets = ref(false)
const loadingPackages = ref(false)

const steps = [
  { label: t('orders.selectPet') },
  { label: t('orders.selectPackage') },
  { label: t('orders.serviceDetails') },
  { label: t('orders.confirmOrder') },
]

const pets = ref<Pet[]>([])
const packages = ref<any[]>([])

const orderForm = ref({
  petId: null as number | null,
  packageId: null as number | null,
  serviceDate: new Date(),
  serviceAddress: '',
  notes: '',
})

const selectedPet = computed(() => pets.value.find((p) => p.id === orderForm.value.petId))
const selectedPackage = computed(() => packages.value.find((p) => p.id === orderForm.value.packageId))

const canProceed = computed(() => {
  if (currentStep.value === 0) return orderForm.value.petId !== null
  if (currentStep.value === 1) return orderForm.value.packageId !== null
  if (currentStep.value === 2) return orderForm.value.serviceAddress.trim() !== ''
  return true
})

const loadPets = async () => {
  loadingPets.value = true
  try {
    const response = await petApi.getMyPets()
    pets.value = response.data || []
  } catch (error: any) {
    notify({ message: error.message || '加载宠物列表失败', color: 'danger' })
  } finally {
    loadingPets.value = false
  }
}

const loadPackages = async () => {
  loadingPackages.value = true
  try {
    const response = await packageApi.getPackages()
    packages.value = response.data || []
  } catch (error: any) {
    notify({ message: error.message || '加载套餐列表失败', color: 'danger' })
  } finally {
    loadingPackages.value = false
  }
}

const selectPet = (pet: Pet) => {
  orderForm.value.petId = pet.id
}

const selectPackage = (pkg: any) => {
  orderForm.value.packageId = pkg.id
}

const nextStep = () => {
  if (canProceed.value && currentStep.value < 3) {
    currentStep.value++
    if (currentStep.value === 1 && packages.value.length === 0) {
      loadPackages()
    }
  }
}

const prevStep = () => {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

const submitOrder = async () => {
  submitting.value = true
  try {
    await orderApi.createOrder({
      petId: orderForm.value.petId!,
      packageId: orderForm.value.packageId!,
      serviceDate: orderForm.value.serviceDate.toISOString(),
      serviceAddress: orderForm.value.serviceAddress,
      notes: orderForm.value.notes,
    })

    notify({ message: '订单创建成功！', color: 'success' })
    router.push('/orders')
  } catch (error: any) {
    notify({ message: error.message || '创建订单失败', color: 'danger' })
  } finally {
    submitting.value = false
  }
}

const getPetTypeText = (type: number) => {
  const map: Record<number, string> = {
    1: '猫咪',
    2: '狗狗',
    99: '其他',
  }
  return map[type] || '未知'
}

const formatDate = (date: Date) => {
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  })
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

.step-content {
  min-height: 400px;
}
</style>
