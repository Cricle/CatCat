<template>
  <div class="create-order-page">
    <van-nav-bar
      title="创建订单"
      left-arrow
      @click-left="$router.back()"
      fixed
      placeholder
    />

    <van-form @submit="onSubmit">
      <!-- 选择宠物 -->
      <van-cell-group title="宠物信息" inset>
        <van-field
          v-model="petName"
          label="选择宠物"
          placeholder="请选择宠物"
          readonly
          is-link
          @click="showPetPicker = true"
          :rules="[{ required: true, message: '请选择宠物' }]"
        />
      </van-cell-group>

      <!-- 服务套餐 -->
      <van-cell-group title="服务套餐" inset>
        <van-field
          v-model="packageName"
          label="服务套餐"
          placeholder="请选择服务套餐"
          readonly
          is-link
          @click="showPackagePicker = true"
          :rules="[{ required: true, message: '请选择服务套餐' }]"
        />
      </van-cell-group>

      <!-- 服务时间 -->
      <van-cell-group title="服务时间" inset>
        <van-field
          v-model="formData.serviceDate"
          label="服务日期"
          placeholder="请选择日期"
          readonly
          is-link
          @click="showDatePicker = true"
          :rules="[{ required: true, message: '请选择服务日期' }]"
        />
        <van-field
          v-model="formData.serviceTime"
          label="服务时间"
          placeholder="请选择时间"
          readonly
          is-link
          @click="showTimePicker = true"
          :rules="[{ required: true, message: '请选择服务时间' }]"
        />
      </van-cell-group>

      <!-- 服务地址 -->
      <van-cell-group title="服务地址" inset>
        <van-field
          v-model="formData.address"
          label="详细地址"
          placeholder="请输入详细地址"
          :rules="[{ required: true, message: '请输入详细地址' }]"
        />
        <van-field
          v-model="formData.addressDetail"
          label="门牌号等"
          placeholder="例如：3栋2单元501"
        />
      </van-cell-group>

      <!-- 备注 -->
      <van-cell-group title="备注" inset>
        <van-field
          v-model="formData.customerRemark"
          type="textarea"
          rows="3"
          placeholder="请输入备注信息，例如：宠物喜好、注意事项等"
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

const fetchData = async () => {
  try {
    const [petsRes, packagesRes] = await Promise.all([
      getMyPets(),
      getActivePackages()
    ])
    pets.value = petsRes
    packages.value = packagesRes

    // 如果从首页传递了套餐ID
    const packageId = route.query.packageId
    if (packageId) {
      selectedPackageId.value = Number(packageId)
      const pkg = packages.value.find(p => p.id === selectedPackageId.value)
      if (pkg) {
        packageName.value = pkg.name
      }
    }
  } catch (error: any) {
    showToast(error.message || '加载失败')
  }
}

const onPetConfirm = ({ selectedOptions }: any) => {
  const option = selectedOptions[0]
  selectedPetId.value = option.value
  petName.value = option.text
  showPetPicker.value = false
}

const onPackageConfirm = ({ selectedOptions }: any) => {
  const option = selectedOptions[0]
  selectedPackageId.value = option.value
  packageName.value = option.text.split(' - ')[0]
  showPackagePicker.value = false
}

const onDateConfirm = ({ selectedValues }: any) => {
  formData.serviceDate = selectedValues.join('-')
  showDatePicker.value = false
}

const onTimeConfirm = ({ selectedValues }: any) => {
  formData.serviceTime = selectedValues.join(':')
  showTimePicker.value = false
}

const onSubmit = async () => {
  if (!selectedPetId.value || !selectedPackageId.value) {
    showToast('请完整填写订单信息')
    return
  }

  loading.value = true
  try {
    const res = await createOrder({
      petId: selectedPetId.value,
      servicePackageId: selectedPackageId.value,
      serviceDate: formData.serviceDate,
      serviceTime: formData.serviceTime,
      address: formData.address,
      addressDetail: formData.addressDetail || undefined,
      customerRemark: formData.customerRemark || undefined
    })

    showSuccessToast('订单创建成功')
    setTimeout(() => {
      router.replace(`/orders/${res.orderId}`)
    }, 1500)
  } catch (error: any) {
    showToast(error.message || '订单创建失败')
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

:deep(.van-cell-group) {
  margin-bottom: 16px;
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
}
</style>

