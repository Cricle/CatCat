<template>
  <VaDataTable
    :items="pets"
    :columns="columns"
    :loading="loading"
    :per-page="pagination.perPage"
    :current-page="pagination.page"
    @update:current-page="(page) => (pagination.page = page)"
  >
    <template #cell(avatar)="{ rowData }">
      <VaAvatar :src="rowData.avatar || 'https://ui-avatars.com/api/?name=' + rowData.name" />
    </template>

    <template #cell(type)="{ rowData }">
      <VaChip :color="getPetTypeColor(rowData.type)" size="small">
        {{ getPetTypeName(rowData.type) }}
      </VaChip>
    </template>

    <template #cell(gender)="{ rowData }">
      <VaIcon :name="getGenderIcon(rowData.gender)" :color="getGenderColor(rowData.gender)" />
      {{ getGenderName(rowData.gender) }}
    </template>

    <template #cell(age)="{ rowData }">
      {{ rowData.age }} 岁
    </template>

    <template #cell(needsWaterRefill)="{ rowData }">
      <VaChip :color="rowData.needsWaterRefill ? 'primary' : 'secondary'" size="small">
        {{ rowData.needsWaterRefill ? '需要' : '不需要' }}
      </VaChip>
    </template>

    <template #cell(actions)="{ rowData }">
      <VaButton preset="plain" icon="edit" @click="$emit('edit-pet', rowData)" />
      <VaButton preset="plain" icon="delete" color="danger" @click="$emit('delete-pet', rowData)" />
    </template>
  </VaDataTable>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Pet, PetType, Gender } from '../../../types/catcat-types'

interface Props {
  pets: Pet[]
  loading?: boolean
  pagination: {
    page: number
    perPage: number
    total: number
  }
}

const props = defineProps<Props>()

defineEmits<{
  (e: 'edit-pet', pet: Pet): void
  (e: 'delete-pet', pet: Pet): void
}>()

const columns = [
  { key: 'avatar', label: '头像', sortable: false },
  { key: 'name', label: '名称', sortable: true },
  { key: 'type', label: '类型', sortable: true },
  { key: 'breed', label: '品种', sortable: false },
  { key: 'age', label: '年龄', sortable: true },
  { key: 'gender', label: '性别', sortable: true },
  { key: 'needsWaterRefill', label: '需要备水', sortable: false },
  { key: 'actions', label: '操作', width: 120, sortable: false },
]

const getPetTypeName = (type: PetType) => {
  const map: Record<PetType, string> = {
    1: '猫',
    2: '狗',
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

const getGenderName = (gender: Gender) => {
  const map: Record<Gender, string> = {
    0: '未知',
    1: '公',
    2: '母',
  }
  return map[gender] || '未知'
}

const getGenderIcon = (gender: Gender) => {
  const map: Record<Gender, string> = {
    0: 'help',
    1: 'male',
    2: 'female',
  }
  return map[gender] || 'help'
}

const getGenderColor = (gender: Gender) => {
  const map: Record<Gender, string> = {
    0: 'secondary',
    1: 'info',
    2: 'danger',
  }
  return map[gender] || 'secondary'
}
</script>

