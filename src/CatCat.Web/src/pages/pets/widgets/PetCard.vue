<template>
  <VaCard class="pet-card" @click="$emit('view-pet', pet)">
    <VaCardContent>
      <div class="pet-card-content">
        <!-- Pet Avatar -->
        <div class="pet-avatar-wrapper">
          <VaAvatar
            :src="pet.avatar || `https://ui-avatars.com/api/?name=${pet.name}&size=200`"
            size="large"
            class="pet-avatar"
          />
          <div :class="['status-badge', pet.gender === 1 ? 'status-male' : 'status-female']">
            <VaIcon :name="pet.gender === 1 ? 'male' : 'female'" size="small" />
          </div>
        </div>

        <!-- Pet Info -->
        <div class="pet-info">
          <h3 class="pet-name">{{ pet.name }}</h3>
          <div class="pet-meta">
            <VaChip :color="getPetTypeColor(pet.type)" size="small">
              {{ getPetTypeName(pet.type) }}
            </VaChip>
            <span class="pet-age">{{ pet.age }} {{ t('dashboard.cards.yearsOld') }}</span>
          </div>
          <p v-if="pet.breed" class="pet-breed">{{ pet.breed }}</p>
        </div>

        <!-- Quick Info -->
        <div class="pet-tags">
          <VaChip v-if="pet.needsWaterRefill" size="small" color="info" outline>
            <VaIcon name="water_drop" size="small" />
            {{ t('petCard.needsWater') }}
          </VaChip>
          <VaChip v-if="pet.healthStatus" size="small" color="success" outline>
            <VaIcon name="favorite" size="small" />
            {{ t('petCard.hasHealth') }}
          </VaChip>
        </div>

        <!-- Actions -->
        <div class="pet-actions">
          <VaButton preset="plain" icon="edit" size="small" @click.stop="$emit('edit-pet', pet)" />
          <VaButton preset="plain" icon="delete" color="danger" size="small" @click.stop="$emit('delete-pet', pet)" />
        </div>
      </div>
    </VaCardContent>
  </VaCard>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import type { Pet, PetType } from '../../../types/catcat-types'

interface Props {
  pet: Pet
}

defineProps<Props>()

defineEmits<{
  (e: 'view-pet', pet: Pet): void
  (e: 'edit-pet', pet: Pet): void
  (e: 'delete-pet', pet: Pet): void
}>()

const { t } = useI18n()

const getPetTypeName = (type: PetType) => {
  const map: Record<PetType, string> = {
    1: '猫咪',
    2: '狗狗',
    99: '其他',
  }
  return map[type] || '未知'
}

const getPetTypeColor = (type: PetType) => {
  const map: Record<PetType, string> = {
    1: 'primary',
    2: 'success',
    99: 'warning',
  }
  return map[type] || 'secondary'
}
</script>

<style scoped>
.pet-card {
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid var(--va-background-border);
}

.pet-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
  border-color: var(--va-primary);
}

.pet-card-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.pet-avatar-wrapper {
  position: relative;
  display: flex;
  justify-content: center;
  padding: 1rem 0;
}

.pet-avatar {
  width: 120px !important;
  height: 120px !important;
  font-size: 3rem;
  border: 3px solid var(--va-background-border);
}

.status-badge {
  position: absolute;
  bottom: 1rem;
  right: calc(50% - 60px - 0.5rem);
  width: 2rem;
  height: 2rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: white;
  border: 2px solid white;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.status-male {
  color: var(--va-info);
}

.status-female {
  color: var(--va-danger);
}

.pet-info {
  text-align: center;
}

.pet-name {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
  color: var(--va-text-primary);
}

.pet-meta {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.pet-age {
  font-size: 0.875rem;
  color: var(--va-text-secondary);
}

.pet-breed {
  font-size: 0.875rem;
  color: var(--va-text-secondary);
  margin: 0;
}

.pet-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: center;
}

.pet-actions {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--va-background-border);
}
</style>

