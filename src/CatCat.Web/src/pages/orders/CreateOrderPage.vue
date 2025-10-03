<template>
  <h1 class="page-title">创建订单</h1>

  <VaCard>
    <VaCardContent>
      <!-- Step Indicator -->
      <div class="mb-6">
        <VaStepper v-model="currentStep" :steps="steps" color="primary" />
      </div>

      <!-- Step 1: Select Pet -->
      <div v-if="currentStep === 0">
        <h2 class="text-xl font-semibold mb-4">选择宠物</h2>

        <div v-if="loadingPets" class="flex justify-center py-8">
          <VaProgressCircle indeterminate />
        </div>

        <div v-else-if="pets.length === 0" class="text-center py-8">
          <VaIcon name="pets" size="large" color="secondary" />
          <p class="text-secondary mt-2">暂无宠物，请先添加宠物</p>
          <VaButton class="mt-4" to="/pets">去添加宠物</VaButton>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <VaCard
            v-for="pet in pets"
            :key="pet.id"
            :class="['pet-card', { selected: orderForm.petId === pet.id }]"
            @click="selectPet(pet)"
          >
            <VaCardContent>
              <div class="flex items-center gap-3">
                <VaAvatar :src="pet.avatarUrl || '/default-pet.png'" size="large" />
                <div class="flex-grow">
                  <div class="font-semibold">{{ pet.name }}</div>
                  <div class="text-sm text-secondary">{{ pet.type }} · {{ pet.age }}岁</div>
                </div>
                <VaIcon v-if="orderForm.petId === pet.id" name="check_circle" color="success" size="large" />
              </div>
            </VaCardContent>
          </VaCard>
        </div>
      </div>

      <!-- Step 2: Select Package -->
      <div v-if="currentStep === 1">
        <h2 class="text-xl font-semibold mb-4">选择服务套餐</h2>

        <div v-if="loadingPackages" class="flex justify-center py-8">
          <VaProgressCircle indeterminate />
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          <VaCard
            v-for="pkg in packages"
            :key="pkg.id"
            :class="['package-card', { selected: orderForm.packageId === pkg.id }]"
            @click="selectPackage(pkg)"
          >
            <VaCardContent>
              <div class="flex justify-between items-start mb-3">
                <div>
                  <h3 class="font-semibold text-lg">{{ pkg.name }}</h3>
                  <VaChip size="small" :color="pkg.isActive ? 'success' : 'danger'" class="mt-1">
                    {{ pkg.isActive ? '可用' : '暂停' }}
                  </VaChip>
                </div>
                <div class="text-right">
                  <div class="text-2xl font-bold text-primary">¥{{ pkg.price }}</div>
                  <div class="text-sm text-secondary">{{ pkg.duration }}天</div>
                </div>
              </div>

              <p class="text-sm text-secondary mb-3">{{ pkg.description }}</p>

              <div class="space-y-1 text-sm">
                <div class="flex items-center gap-2">
                  <VaIcon name="event" size="small" />
                  <span>{{ pkg.visitsPerDay }}次/天</span>
                </div>
                <div class="flex items-center gap-2">
                  <VaIcon name="schedule" size="small" />
                  <span>{{ pkg.minutesPerVisit }}分钟/次</span>
                </div>
              </div>

              <VaIcon
                v-if="orderForm.packageId === pkg.id"
                name="check_circle"
                color="success"
                size="large"
                class="absolute top-2 right-2"
              />
            </VaCardContent>
          </VaCard>
        </div>
      </div>

      <!-- Step 3: Schedule & Address -->
      <div v-if="currentStep === 2">
        <h2 class="text-xl font-semibold mb-4">服务时间与地址</h2>

        <div class="max-w-2xl space-y-4">
          <VaInput v-model="orderForm.serviceDate" type="date" label="服务日期" required class="w-full" />

          <VaInput v-model="orderForm.serviceTime" type="time" label="服务时间" required class="w-full" />

          <VaInput v-model="orderForm.address" label="服务地址" placeholder="请输入详细地址" required class="w-full">
            <template #prepend>
              <VaIcon name="location_on" />
            </template>
          </VaInput>

          <VaTextarea
            v-model="orderForm.notes"
            label="备注说明"
            placeholder="有任何特殊要求请在这里说明..."
            :min-rows="3"
            class="w-full"
          />
        </div>
      </div>

      <!-- Step 4: Review -->
      <div v-if="currentStep === 3">
        <h2 class="text-xl font-semibold mb-4">确认订单</h2>

        <div class="max-w-2xl space-y-4">
          <!-- Pet Info -->
          <VaCard color="background-border">
            <VaCardContent>
              <h3 class="font-semibold mb-2 flex items-center gap-2">
                <VaIcon name="pets" />
                宠物信息
              </h3>
              <div v-if="selectedPet" class="flex items-center gap-3">
                <VaAvatar :src="selectedPet.avatarUrl || '/default-pet.png'" />
                <div>
                  <div class="font-semibold">{{ selectedPet.name }}</div>
                  <div class="text-sm text-secondary">{{ selectedPet.type }} · {{ selectedPet.age }}岁</div>
                </div>
              </div>
            </VaCardContent>
          </VaCard>

          <!-- Package Info -->
          <VaCard color="background-border">
            <VaCardContent>
              <h3 class="font-semibold mb-2 flex items-center gap-2">
                <VaIcon name="business_center" />
                服务套餐
              </h3>
              <div v-if="selectedPackage">
                <div class="flex justify-between items-start">
                  <div>
                    <div class="font-semibold">{{ selectedPackage.name }}</div>
                    <div class="text-sm text-secondary">{{ selectedPackage.description }}</div>
                  </div>
                  <div class="text-xl font-bold text-primary">¥{{ selectedPackage.price }}</div>
                </div>
                <div class="mt-2 flex gap-4 text-sm">
                  <span>{{ selectedPackage.duration }}天</span>
                  <span>{{ selectedPackage.visitsPerDay }}次/天</span>
                  <span>{{ selectedPackage.minutesPerVisit }}分钟/次</span>
                </div>
              </div>
            </VaCardContent>
          </VaCard>

          <!-- Schedule Info -->
          <VaCard color="background-border">
            <VaCardContent>
              <h3 class="font-semibold mb-2 flex items-center gap-2">
                <VaIcon name="event" />
                服务时间
              </h3>
              <div class="space-y-1">
                <div class="flex items-center gap-2">
                  <VaIcon name="calendar_today" size="small" />
                  <span>{{ orderForm.serviceDate }}</span>
                </div>
                <div class="flex items-center gap-2">
                  <VaIcon name="schedule" size="small" />
                  <span>{{ orderForm.serviceTime }}</span>
                </div>
              </div>
            </VaCardContent>
          </VaCard>

          <!-- Address Info -->
          <VaCard color="background-border">
            <VaCardContent>
              <h3 class="font-semibold mb-2 flex items-center gap-2">
                <VaIcon name="location_on" />
                服务地址
              </h3>
              <p>{{ orderForm.address }}</p>
              <p v-if="orderForm.notes" class="text-sm text-secondary mt-2">备注: {{ orderForm.notes }}</p>
            </VaCardContent>
          </VaCard>

          <!-- Total -->
          <VaCard color="primary">
            <VaCardContent>
              <div class="flex justify-between items-center text-white">
                <span class="text-lg font-semibold">订单总额</span>
                <span class="text-3xl font-bold">¥{{ selectedPackage?.price || 0 }}</span>
              </div>
            </VaCardContent>
          </VaCard>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex justify-between mt-6">
        <VaButton v-if="currentStep > 0" preset="secondary" @click="previousStep">上一步</VaButton>
        <div v-else></div>

        <div class="flex gap-2">
          <VaButton preset="secondary" @click="cancel">取消</VaButton>
          <VaButton v-if="currentStep < steps.length - 1" :disabled="!canProceed" @click="nextStep">
            下一步
          </VaButton>
          <VaButton v-else color="primary" :loading="submitting" :disabled="!canSubmit" @click="submitOrder">
            提交订单
          </VaButton>
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useToast } from 'vuestic-ui'
import { petApi, packageApi, orderApi } from '../../services/catcat-api'
import type { Pet, ServicePackage } from '../../types/catcat-types'

const router = useRouter()
const { init: notify } = useToast()

const currentStep = ref(0)
const steps = [
  { label: '选择宠物' },
  { label: '选择套餐' },
  { label: '时间地址' },
  { label: '确认订单' },
]

const pets = ref<Pet[]>([])
const packages = ref<ServicePackage[]>([])
const loadingPets = ref(false)
const loadingPackages = ref(false)
const submitting = ref(false)

const orderForm = ref({
  petId: '',
  packageId: '',
  serviceDate: '',
  serviceTime: '10:00',
  address: '',
  notes: '',
})

// Selected items
const selectedPet = computed(() => pets.value.find((p) => p.id === orderForm.value.petId))
const selectedPackage = computed(() => packages.value.find((p) => p.id === orderForm.value.packageId))

// Can proceed checks
const canProceed = computed(() => {
  switch (currentStep.value) {
    case 0:
      return !!orderForm.value.petId
    case 1:
      return !!orderForm.value.packageId
    case 2:
      return !!orderForm.value.serviceDate && !!orderForm.value.serviceTime && !!orderForm.value.address
    default:
      return true
  }
})

const canSubmit = computed(() => {
  return (
    orderForm.value.petId &&
    orderForm.value.packageId &&
    orderForm.value.serviceDate &&
    orderForm.value.serviceTime &&
    orderForm.value.address
  )
})

// Load data
const loadPets = async () => {
  loadingPets.value = true
  try {
    const response = await petApi.getMyPets()
    pets.value = response.data || []
  } catch (error: any) {
    notify({ message: '加载宠物列表失败', color: 'danger' })
  } finally {
    loadingPets.value = false
  }
}

const loadPackages = async () => {
  loadingPackages.value = true
  try {
    const response = await packageApi.getAll({ page: 1, pageSize: 100 })
    packages.value = (response.data.items || []).filter((p: ServicePackage) => p.isActive)
  } catch (error: any) {
    notify({ message: '加载套餐列表失败', color: 'danger' })
  } finally {
    loadingPackages.value = false
  }
}

// Select handlers
const selectPet = (pet: Pet) => {
  orderForm.value.petId = pet.id
}

const selectPackage = (pkg: ServicePackage) => {
  orderForm.value.packageId = pkg.id
}

// Navigation
const nextStep = () => {
  if (currentStep.value < steps.length - 1) {
    currentStep.value++

    // Load data when entering step
    if (currentStep.value === 1 && packages.value.length === 0) {
      loadPackages()
    }
  }
}

const previousStep = () => {
  if (currentStep.value > 0) {
    currentStep.value--
  }
}

const cancel = () => {
  router.push('/orders')
}

// Submit
const submitOrder = async () => {
  if (!canSubmit.value) return

  submitting.value = true
  try {
    const createData = {
      petId: orderForm.value.petId,
      packageId: orderForm.value.packageId,
      serviceDate: orderForm.value.serviceDate,
      serviceTime: orderForm.value.serviceTime,
      address: orderForm.value.address,
      notes: orderForm.value.notes,
    }

    const response = await orderApi.create(createData)
    notify({ message: '订单创建成功！', color: 'success' })
    router.push(`/orders/${response.data.id}`)
  } catch (error: any) {
    notify({ message: error.response?.data?.message || '创建订单失败', color: 'danger' })
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  loadPets()

  // Set default service date to tomorrow
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  orderForm.value.serviceDate = tomorrow.toISOString().split('T')[0]
})
</script>

<style scoped>
.page-title {
  font-size: 2rem;
  font-weight: 600;
  margin-bottom: 1.5rem;
}

.pet-card,
.package-card {
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
}

.pet-card:hover,
.package-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.pet-card.selected,
.package-card.selected {
  border: 2px solid var(--va-primary);
  box-shadow: 0 4px 12px rgba(var(--va-primary-rgb), 0.3);
}
</style>

