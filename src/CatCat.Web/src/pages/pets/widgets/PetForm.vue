<template>
  <VaForm ref="form">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <!-- Basic Information -->
      <div class="md:col-span-2">
        <h3 class="text-lg font-semibold mb-2">基础信息</h3>
      </div>

      <VaInput v-model="modelValue.name" label="宠物名称" placeholder="请输入宠物名称" required />

      <VaSelect
        v-model="modelValue.type"
        label="宠物类型"
        :options="petTypeOptions"
        text-by="text"
        value-by="value"
        required
      />

      <VaInput v-model="modelValue.breed" label="品种" placeholder="请输入品种" />

      <VaInput v-model="modelValue.age" label="年龄（岁）" type="number" min="0" placeholder="请输入年龄" required />

      <VaSelect
        v-model="modelValue.gender"
        label="性别"
        :options="genderOptions"
        text-by="text"
        value-by="value"
        required
      />

      <!-- Avatar Upload -->
      <div class="md:col-span-2">
        <label class="va-input-label">{{ t('petForm.avatar') }}</label>
        <ImageUploader v-model="modelValue.avatar" />
      </div>

      <!-- Service Information -->
      <div class="md:col-span-2 mt-4">
        <h3 class="text-lg font-semibold mb-2">服务信息</h3>
        <p class="text-sm text-gray-500 mb-4">帮助服务人员快速找到所需物品</p>
      </div>

      <VaInput
        v-model="modelValue.foodLocation"
        label="猫粮位置"
        placeholder="例如：厨房橱柜第二层"
      />

      <VaInput
        v-model="modelValue.waterLocation"
        label="水盆位置"
        placeholder="例如：客厅电视柜旁边"
      />

      <VaInput
        v-model="modelValue.litterBoxLocation"
        label="猫砂盆位置"
        placeholder="例如：卫生间角落"
      />

      <VaInput
        v-model="modelValue.cleaningSuppliesLocation"
        label="清洁用品位置"
        placeholder="例如：阳台储物柜（扫把、猫屎袋等）"
      />

      <div class="md:col-span-2">
        <VaCheckbox v-model="modelValue.needsWaterRefill" label="需要备水" />
      </div>

      <div class="md:col-span-2">
        <VaTextarea
          v-model="modelValue.specialInstructions"
          label="特殊说明"
          placeholder="例如：猫粮每次半碗、水要换新的"
          :max-rows="4"
        />
      </div>

      <!-- Health & Character -->
      <div class="md:col-span-2 mt-4">
        <h3 class="text-lg font-semibold mb-2">健康与性格</h3>
      </div>

      <VaTextarea
        v-model="modelValue.character"
        label="性格"
        placeholder="例如：活泼好动、胆小怕生"
        :max-rows="3"
      />

      <VaTextarea
        v-model="modelValue.dietaryHabits"
        label="饮食习惯"
        placeholder="例如：喜欢吃罐头、不喜欢鱼肉"
        :max-rows="3"
      />

      <div class="md:col-span-2">
        <VaTextarea
          v-model="modelValue.healthStatus"
          label="健康状况"
          placeholder="例如：已绝育、定期驱虫"
          :max-rows="3"
        />
      </div>

      <div class="md:col-span-2">
        <VaTextarea v-model="modelValue.remarks" label="备注" placeholder="其他需要说明的信息" :max-rows="3" />
      </div>
    </div>
  </VaForm>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import type { Pet } from '../../../types/catcat-types'
import ImageUploader from '../../../components/ImageUploader.vue'

const { t } = useI18n()

interface Props {
  modelValue: Partial<Pet>
}

const props = defineProps<Props>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: Partial<Pet>): void
}>()

const petTypeOptions = [
  { value: 1, text: '猫' },
  { value: 2, text: '狗' },
  { value: 99, text: '其他' },
]

const genderOptions = [
  { value: 0, text: '未知' },
  { value: 1, text: '公' },
  { value: 2, text: '母' },
]
</script>

