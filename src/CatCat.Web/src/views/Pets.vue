<template>
  <div class="pets-page">
    <van-nav-bar title="我的宠物" fixed placeholder />

    <div class="pets-list">
      <van-loading v-if="loading" class="loading" />
      <van-empty v-else-if="pets.length === 0" description="还没有添加宠物">
        <van-button type="primary" @click="addPet">添加宠物</van-button>
      </van-empty>

      <van-cell-group v-for="pet in pets" :key="pet.id" inset class="pet-item">
        <van-cell center>
          <template #icon>
            <van-image
              round
              width="60"
              height="60"
              :src="pet.avatar || 'https://via.placeholder.com/60/FFB6C1/FFFFFF?text=🐱'"
              class="pet-avatar"
            />
          </template>
          <template #title>
            <div class="pet-name">
              {{ pet.name }}
              <van-tag plain type="primary">{{ getPetType(pet.type) }}</van-tag>
            </div>
            <div class="pet-info">
              {{ pet.breed || '未知品种' }} · {{ pet.age }}岁 · {{ getGender(pet.gender) }}
            </div>
          </template>
          <template #right-icon>
            <van-button size="small" @click="editPet(pet)">编辑</van-button>
          </template>
        </van-cell>
        <van-cell v-if="pet.character" title="性格" :value="pet.character" />
        <van-cell v-if="pet.healthStatus" title="健康状况" :value="pet.healthStatus" />
      </van-cell-group>
    </div>

    <div class="add-button">
      <van-button type="primary" block icon="plus" @click="addPet">
        添加新宠物
      </van-button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getMyPets } from '@/api/pets'
import type { Pet } from '@/api/pets'
import { showToast } from 'vant'

const router = useRouter()
const loading = ref(false)
const pets = ref<Pet[]>([])

const getPetType = (type: number) => {
  return type === 0 ? '猫咪' : '狗狗'
}

const getGender = (gender: number) => {
  return gender === 0 ? '公' : '母'
}

const fetchPets = async () => {
  loading.value = true
  try {
    const res = await getMyPets()
    pets.value = res.data
  } catch (error: any) {
    showToast(error.message || 'Loading failed')
  } finally {
    loading.value = false
  }
}

const addPet = () => {
  router.push('/pets/add')
}

const editPet = (pet: Pet) => {
  router.push(`/pets/${pet.id}/edit`)
}

onMounted(() => {
  fetchPets()
})
</script>

<style scoped>
.pets-page {
  min-height: 100vh;
  background-color: #f7f8fa;
  padding-bottom: 80px;
}

.pets-list {
  padding: 16px;
  min-height: 400px;
}

.loading {
  padding: 60px 0;
  text-align: center;
}

.pet-item {
  margin-bottom: 16px;
}

.pet-avatar {
  margin-right: 12px;
}

.pet-name {
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 6px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.pet-info {
  font-size: 12px;
  color: #969799;
}

.add-button {
  position: fixed;
  bottom: 70px;
  left: 16px;
  right: 16px;
}
</style>

