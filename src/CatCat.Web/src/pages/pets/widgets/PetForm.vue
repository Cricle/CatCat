<template>
  <VaForm ref="form">
    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
      <!-- Basic Information -->
      <div class="md:col-span-2">
        <h3 class="text-lg font-semibold mb-2">{{ t('petForm.basicInfo') }}</h3>
      </div>

      <VaInput v-model="modelValue.name" :label="t('petForm.name')" :placeholder="t('petForm.namePlaceholder')" required />

      <VaSelect
        v-model="modelValue.type"
        :label="t('petForm.type')"
        :options="petTypeOptions"
        text-by="text"
        value-by="value"
        required
      />

      <VaInput v-model="modelValue.breed" :label="t('petForm.breed')" :placeholder="t('petForm.breedPlaceholder')" />

      <VaInput v-model="modelValue.age" :label="t('petForm.age')" type="number" min="0" :placeholder="t('petForm.agePlaceholder')" required />

      <VaSelect
        v-model="modelValue.gender"
        :label="t('petForm.gender')"
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
        <h3 class="text-lg font-semibold mb-2">{{ t('petForm.serviceInfo') }}</h3>
        <p class="text-sm text-secondary mb-4">{{ t('petForm.serviceInfoHint') }}</p>
      </div>

      <!-- Food Location -->
      <div class="md:col-span-2">
        <label class="va-input-label mb-2">{{ t('petForm.foodLocation') }}</label>
        <div class="location-section">
          <ImageUploader v-model="modelValue.foodLocationImage" class="location-image" />
          <VaInput
            v-model="modelValue.foodLocationDesc"
            :placeholder="t('petForm.foodLocationPlaceholder')"
            class="flex-1"
          />
        </div>
      </div>

      <!-- Water Location -->
      <div class="md:col-span-2">
        <label class="va-input-label mb-2">{{ t('petForm.waterLocation') }}</label>
        <div class="location-section">
          <ImageUploader v-model="modelValue.waterLocationImage" class="location-image" />
          <VaInput
            v-model="modelValue.waterLocationDesc"
            :placeholder="t('petForm.waterLocationPlaceholder')"
            class="flex-1"
          />
        </div>
      </div>

      <!-- Litter Box Location -->
      <div class="md:col-span-2">
        <label class="va-input-label mb-2">{{ t('petForm.litterBoxLocation') }}</label>
        <div class="location-section">
          <ImageUploader v-model="modelValue.litterBoxLocationImage" class="location-image" />
          <VaInput
            v-model="modelValue.litterBoxLocationDesc"
            :placeholder="t('petForm.litterBoxLocationPlaceholder')"
            class="flex-1"
          />
        </div>
      </div>

      <!-- Cleaning Supplies Location -->
      <div class="md:col-span-2">
        <label class="va-input-label mb-2">{{ t('petForm.cleaningSuppliesLocation') }}</label>
        <div class="location-section">
          <ImageUploader v-model="modelValue.cleaningSuppliesImage" class="location-image" />
          <VaInput
            v-model="modelValue.cleaningSuppliesDesc"
            :placeholder="t('petForm.cleaningSuppliesLocationPlaceholder')"
            class="flex-1"
          />
        </div>
      </div>

      <div class="md:col-span-2">
        <VaCheckbox v-model="modelValue.needsWaterRefill" :label="t('petForm.needsWaterRefill')" />
      </div>

      <div class="md:col-span-2">
        <VaTextarea
          v-model="modelValue.specialInstructions"
          :label="t('petForm.specialInstructions')"
          :placeholder="t('petForm.specialInstructionsPlaceholder')"
          :max-rows="4"
        />
      </div>

      <!-- Health & Character -->
      <div class="md:col-span-2 mt-4">
        <h3 class="text-lg font-semibold mb-2">{{ t('petForm.healthCharacter') }}</h3>
      </div>

      <VaTextarea
        v-model="modelValue.character"
        :label="t('petForm.character')"
        :placeholder="t('petForm.characterPlaceholder')"
        :max-rows="3"
      />

      <VaTextarea
        v-model="modelValue.dietaryHabits"
        :label="t('petForm.dietaryHabits')"
        :placeholder="t('petForm.dietaryHabitsPlaceholder')"
        :max-rows="3"
      />

      <div class="md:col-span-2">
        <VaTextarea
          v-model="modelValue.healthStatus"
          :label="t('petForm.healthStatus')"
          :placeholder="t('petForm.healthStatusPlaceholder')"
          :max-rows="3"
        />
      </div>

      <div class="md:col-span-2">
        <VaTextarea v-model="modelValue.remarks" :label="t('petForm.remarks')" :placeholder="t('petForm.remarksPlaceholder')" :max-rows="3" />
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

<style scoped>
.location-section {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}

.location-image {
  width: 200px;
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .location-section {
    flex-direction: column;
  }
  
  .location-image {
    width: 100%;
  }
}
</style>

