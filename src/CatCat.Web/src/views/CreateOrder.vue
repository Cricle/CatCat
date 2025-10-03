<template>
  <div class="create-order-page">
    <van-nav-bar
      title="Create Order"
      left-arrow
      @click-left="$router.back()"
      fixed
      placeholder
    />

    <!-- Progress Indicator -->
    <div class="progress-bar">
      <div class="progress-step" :class="{ active: step >= 1, completed: step > 1 }">
        <div class="step-circle">1</div>
        <span>Pet & Service</span>
      </div>
      <div class="progress-line" :class="{ active: step > 1 }"></div>
      <div class="progress-step" :class="{ active: step >= 2, completed: step > 2 }">
        <div class="step-circle">2</div>
        <span>Time & Location</span>
      </div>
      <div class="progress-line" :class="{ active: step > 2 }"></div>
      <div class="progress-step" :class="{ active: step >= 3 }">
        <div class="step-circle">3</div>
        <span>Confirm</span>
      </div>
    </div>

    <van-form @submit="onSubmit">
      <!-- Pet Selection -->
      <van-cell-group title="Pet Information" inset>
        <van-field
          v-model="petName"
          label="Select Pet"
          placeholder="Choose your pet"
          readonly
          is-link
          @click="openPetPicker"
          :rules="[{ required: true, message: 'Please select a pet' }]"
          :error-message="errors.pet"
        >
          <template #left-icon>
            <van-icon name="paw" />
          </template>
        </van-field>
        <div v-if="!pets.length && !loadingData" class="inline-tip">
          <van-icon name="info-o" />
          <span>No pets found. <a @click="$router.push('/pets')">Add a pet</a> first</span>
        </div>
      </van-cell-group>

      <!-- Service Package -->
      <van-cell-group title="Service Package" inset>
        <van-field
          v-model="packageName"
          label="Package"
          placeholder="Choose service package"
          readonly
          is-link
          @click="openPackagePicker"
          :rules="[{ required: true, message: 'Please select a package' }]"
          :error-message="errors.package"
        >
          <template #left-icon>
            <van-icon name="service-o" />
          </template>
        </van-field>
        <div v-if="selectedPackage" class="package-preview">
          <div class="preview-item">
            <span>Duration:</span>
            <strong>{{ selectedPackage.duration }} min</strong>
          </div>
          <div class="preview-item">
            <span>Price:</span>
            <strong class="price">¥{{ selectedPackage.price }}</strong>
          </div>
        </div>
      </van-cell-group>

      <!-- Service Time -->
      <van-cell-group title="Service Time" inset>
        <van-field
          v-model="formData.serviceDate"
          label="Date"
          placeholder="Select date"
          readonly
          is-link
          @click="showDatePicker = true"
          :rules="[{ required: true, message: 'Please select date' }]"
          :error-message="errors.date"
        >
          <template #left-icon>
            <van-icon name="calendar-o" />
          </template>
        </van-field>
        <van-field
          v-model="formData.serviceTime"
          label="Time"
          placeholder="Select time"
          readonly
          is-link
          @click="showTimePicker = true"
          :rules="[{ required: true, message: 'Please select time' }]"
          :error-message="errors.time"
        >
          <template #left-icon>
            <van-icon name="clock-o" />
          </template>
        </van-field>
      </van-cell-group>

      <!-- Service Address -->
      <van-cell-group title="Service Address" inset>
        <van-field
          v-model="formData.address"
          label="Address"
          placeholder="Enter detailed address"
          :rules="[{ required: true, message: 'Please enter address' }]"
          :error-message="errors.address"
          @blur="validateField('address')"
        >
          <template #left-icon>
            <van-icon name="location-o" />
          </template>
        </van-field>
        <van-field
          v-model="formData.addressDetail"
          label="Unit/Room"
          placeholder="e.g., Building 3, Unit 2, Room 501"
        />
      </van-cell-group>

      <!-- Remarks -->
      <van-cell-group title="Remarks (Optional)" inset>
        <van-field
          v-model="formData.customerRemark"
          type="textarea"
          rows="3"
          placeholder="Enter any special requests or pet preferences..."
          maxlength="200"
          show-word-limit
        />
      </van-cell-group>

      <!-- 价格 -->
      <van-cell-group inset>
        <van-cell title="服务费用">
          <template #value>
            <span class="price">¥{{ selectedPackage?.price || 0 }}</span>
          </template>
        </van-cell>
      </van-cell-group>

      <div class="submit-button">
        <van-button
          round
          block
          type="primary"
          native-type="submit"
          :loading="loading"
        >
          提交订单
        </van-button>
      </div>
    </van-form>

    <!-- 宠物选择器 -->
    <van-popup v-model:show="showPetPicker" position="bottom">
      <van-picker
        :columns="petColumns"
        @confirm="onPetConfirm"
        @cancel="showPetPicker = false"
      />
    </van-popup>

    <!-- 套餐选择器 -->
    <van-popup v-model:show="showPackagePicker" position="bottom">
      <van-picker
        :columns="packageColumns"
        @confirm="onPackageConfirm"
        @cancel="showPackagePicker = false"
      />
    </van-popup>

    <!-- 日期选择器 -->
    <van-popup v-model:show="showDatePicker" position="bottom">
      <van-date-picker
        v-model="currentDate"
        title="选择日期"
        :min-date="minDate"
        @confirm="onDateConfirm"
        @cancel="showDatePicker = false"
      />
    </van-popup>

    <!-- 时间选择器 -->
    <van-popup v-model:show="showTimePicker" position="bottom">
      <van-time-picker
        v-model="currentTime"
        title="选择时间"
        @confirm="onTimeConfirm"
        @cancel="showTimePicker = false"
      />
    </van-popup>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { getMyPets } from '@/api/pets'
import { getActivePackages } from '@/api/packages'
import { createOrder } from '@/api/orders'
import type { Pet } from '@/api/pets'
import type { ServicePackage } from '@/api/packages'
import { showToast, showSuccessToast } from 'vant'

const router = useRouter()
const route = useRoute()

const loading = ref(false)
const loadingData = ref(false)
const step = ref(1)
const pets = ref<Pet[]>([])
const packages = ref<ServicePackage[]>([])

const showPetPicker = ref(false)
const showPackagePicker = ref(false)
const showDatePicker = ref(false)
const showTimePicker = ref(false)

const selectedPetId = ref<number>()
const petName = ref('')
const selectedPackageId = ref<number>()
const packageName = ref('')

const errors = reactive({
  pet: '',
  package: '',
  date: '',
  time: '',
  address: ''
})

const minDate = new Date()
const currentDate = ref(['2024', '01', '01'])
const currentTime = ref(['09', '00'])

const formData = reactive({
  serviceDate: '',
  serviceTime: '',
  address: '',
  addressDetail: '',
  customerRemark: ''
})

const petColumns = computed(() =>
  pets.value.map(pet => ({
    text: `${pet.name} (${pet.breed || '未知品种'})`,
    value: pet.id
  }))
)

const packageColumns = computed(() =>
  packages.value.map(pkg => ({
    text: `${pkg.name} - ¥${pkg.price}`,
    value: pkg.id
  }))
)

const selectedPackage = computed(() =>
  packages.value.find(p => p.id === selectedPackageId.value)
)

const openPetPicker = () => {
  if (!pets.value.length) {
    showToast({
      message: 'Please add a pet first',
      icon: 'fail'
    })
    setTimeout(() => router.push('/pets'), 1500)
    return
  }
  showPetPicker.value = true
}

const openPackagePicker = () => {
  if (!packages.value.length) {
    showToast({
      message: 'No service packages available',
      icon: 'fail'
    })
    return
  }
  showPackagePicker.value = true
}

const validateField = (field: keyof typeof errors) => {
  errors[field] = ''
  if (field === 'address' && !formData.address) {
    errors.address = 'Address is required'
  }
}

const fetchData = async () => {
  loadingData.value = true
  try {
    const [petsRes, packagesRes] = await Promise.all([
      getMyPets(),
      getActivePackages()
    ])
    pets.value = petsRes.data
    packages.value = packagesRes.data

    const packageId = route.query.packageId
    if (packageId) {
      selectedPackageId.value = Number(packageId)
      const pkg = packages.value.find(p => p.id === selectedPackageId.value)
      if (pkg) {
        packageName.value = pkg.name
        step.value = 2
      }
    }
  } catch (error: any) {
    showToast({
      message: error.message || 'Loading failed',
      icon: 'fail'
    })
  } finally {
    loadingData.value = false
  }
}

const onPetConfirm = ({ selectedOptions }: any) => {
  const option = selectedOptions[0]
  selectedPetId.value = option.value
  petName.value = option.text
  errors.pet = ''
  showPetPicker.value = false
  if (selectedPackageId.value) step.value = 2
}

const onPackageConfirm = ({ selectedOptions }: any) => {
  const option = selectedOptions[0]
  selectedPackageId.value = option.value
  packageName.value = option.text.split(' - ')[0]
  errors.package = ''
  showPackagePicker.value = false
  if (selectedPetId.value) step.value = 2
}

const onDateConfirm = ({ selectedValues }: any) => {
  formData.serviceDate = selectedValues.join('-')
  errors.date = ''
  showDatePicker.value = false
  if (formData.serviceTime) step.value = 3
}

const onTimeConfirm = ({ selectedValues }: any) => {
  formData.serviceTime = selectedValues.join(':')
  errors.time = ''
  showTimePicker.value = false
  if (formData.serviceDate) step.value = 3
}

const onSubmit = async () => {
  // Validate all fields
  let hasError = false
  if (!selectedPetId.value) {
    errors.pet = 'Please select a pet'
    hasError = true
  }
  if (!selectedPackageId.value) {
    errors.package = 'Please select a package'
    hasError = true
  }
  if (!formData.serviceDate) {
    errors.date = 'Please select a date'
    hasError = true
  }
  if (!formData.serviceTime) {
    errors.time = 'Please select a time'
    hasError = true
  }
  if (!formData.address) {
    errors.address = 'Please enter an address'
    hasError = true
  }

  if (hasError) {
    showToast({
      message: 'Please complete all required fields',
      icon: 'fail'
    })
    return
  }

  loading.value = true
  try {
    const res = await createOrder({
      petId: selectedPetId.value!,
      servicePackageId: selectedPackageId.value!,
      serviceDate: formData.serviceDate,
      serviceTime: formData.serviceTime,
      address: formData.address,
      addressDetail: formData.addressDetail || undefined,
      customerRemark: formData.customerRemark || undefined
    })

    showSuccessToast({
      message: 'Order submitted successfully!',
      duration: 1500
    })
    
    setTimeout(() => {
      router.replace(`/orders/${res.data.orderId}`)
    }, 1500)
  } catch (error: any) {
    showToast({
      message: error.message || 'Order creation failed',
      icon: 'fail'
    })
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.create-order-page {
  min-height: 100vh;
  background-color: #f7f8fa;
  padding-bottom: 80px;
}

/* Progress Bar */
.progress-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px;
  background: white;
  margin-bottom: 12px;
}

.progress-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  flex: 0 0 auto;
  opacity: 0.4;
  transition: opacity 0.3s;
}

.progress-step.active {
  opacity: 1;
}

.step-circle {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #f0f0f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 14px;
  color: #999;
  transition: all 0.3s;
}

.progress-step.active .step-circle {
  background: #1989fa;
  color: white;
}

.progress-step.completed .step-circle {
  background: #07c160;
  color: white;
}

.progress-step span {
  font-size: 12px;
  color: #999;
  white-space: nowrap;
}

.progress-step.active span {
  color: #323233;
  font-weight: 500;
}

.progress-line {
  flex: 1;
  height: 2px;
  background: #f0f0f0;
  margin: 0 8px;
  transition: background 0.3s;
}

.progress-line.active {
  background: #1989fa;
}

:deep(.van-cell-group) {
  margin-bottom: 16px;
}

.inline-tip {
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #646566;
  background: #f7f8fa;
  border-radius: 4px;
  margin: -8px 16px 8px;
}

.inline-tip a {
  color: #1989fa;
  text-decoration: underline;
}

.package-preview {
  padding: 12px 16px;
  background: #f7f8fa;
  border-radius: 4px;
  margin: -8px 16px 8px;
}

.preview-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
  padding: 4px 0;
}

.preview-item span {
  color: #646566;
}

.preview-item strong {
  color: #323233;
}

.price {
  color: #ee0a24;
  font-weight: 600;
  font-size: 18px;
}

.submit-button {
  padding: 16px;
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: white;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
  z-index: 100;
}

/* Mobile Responsive */
@media (max-width: 640px) {
  .progress-step span {
    font-size: 10px;
  }
  
  .step-circle {
    width: 28px;
    height: 28px;
    font-size: 12px;
  }
}
</style>

