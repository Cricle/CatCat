<template>
  <div class="pets-page">
    <van-nav-bar title="My Pets" fixed placeholder />

    <van-pull-refresh v-model="refreshing" @refresh="onRefresh">
      <div class="pets-content">
        <!-- Skeleton Loading -->
        <template v-if="loading && !refreshing">
          <van-skeleton title avatar :row="2" class="skeleton-item" v-for="i in 3" :key="i" />
        </template>

        <!-- Empty State -->
        <van-empty 
          v-else-if="pets.length === 0" 
          image="search"
          description="No pets yet"
        >
          <van-button type="primary" round @click="addPet">
            <van-icon name="plus" />
            Add Pet
          </van-button>
        </van-empty>

        <!-- Pet List -->
        <div v-else class="pets-grid">
          <div
            v-for="pet in pets"
            :key="pet.id"
            class="pet-card"
            @click="editPet(pet)"
          >
            <div class="pet-avatar-wrapper">
              <van-image
                round
                width="80"
                height="80"
                :src="pet.avatar || getDefaultAvatar(pet.gender)"
                fit="cover"
              />
              <van-tag 
                v-if="pet.breed" 
                class="pet-breed-tag"
                type="primary"
              >
                {{ pet.breed }}
              </van-tag>
            </div>

            <div class="pet-info">
              <h3 class="pet-name">{{ pet.name }}</h3>
              <div class="pet-meta">
                <van-tag plain>{{ pet.age }} years</van-tag>
                <van-tag plain>{{ getGender(pet.gender) }}</van-tag>
              </div>
              <p v-if="pet.healthStatus" class="pet-health">
                {{ pet.healthStatus }}
              </p>
            </div>

            <van-icon name="arrow" class="arrow-icon" />
          </div>
        </div>
      </div>
    </van-pull-refresh>

    <!-- Floating Add Button -->
    <van-button
      class="fab-button"
      type="primary"
      icon="plus"
      round
      @click="addPet"
    >
      Add Pet
    </van-button>
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
const refreshing = ref(false)
const pets = ref<Pet[]>([])

const getGender = (gender: number) => {
  return gender === 0 ? 'Male' : 'Female'
}

const getDefaultAvatar = (gender: number) => {
  return gender === 0 
    ? 'https://via.placeholder.com/80/87CEEB/FFFFFF?text=🐱'
    : 'https://via.placeholder.com/80/FFB6C1/FFFFFF?text=🐱'
}

const fetchPets = async () => {
  loading.value = true
  try {
    const res = await getMyPets()
    pets.value = res.data
  } catch (error: any) {
    showToast({
      message: error.message || 'Failed to load pets',
      icon: 'fail'
    })
  } finally {
    loading.value = false
    refreshing.value = false
  }
}

const onRefresh = () => {
  refreshing.value = true
  fetchPets()
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
  background: var(--gray-50);
  padding-bottom: 90px;
}

.pets-content {
  padding: 12px;
  min-height: 400px;
}

.skeleton-item {
  margin-bottom: 12px;
  padding: 16px;
  background: white;
  border-radius: var(--radius);
}

.pets-grid {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.pet-card {
  background: white;
  border-radius: var(--radius);
  padding: 16px;
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: all var(--transition);
  border: 1px solid var(--gray-200);
  position: relative;
}

.pet-card:active {
  transform: scale(0.98);
  border-color: var(--primary);
}

.pet-avatar-wrapper {
  position: relative;
  flex-shrink: 0;
}

.pet-breed-tag {
  position: absolute;
  bottom: -4px;
  right: -4px;
}

.pet-info {
  flex: 1;
  min-width: 0;
}

.pet-name {
  font-size: 18px;
  font-weight: 600;
  color: var(--gray-900);
  margin: 0 0 8px 0;
}

.pet-meta {
  display: flex;
  gap: 6px;
  margin-bottom: 6px;
}

.pet-health {
  font-size: 13px;
  color: var(--gray-600);
  margin: 6px 0 0 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.arrow-icon {
  color: var(--gray-400);
  font-size: 16px;
  flex-shrink: 0;
}

.fab-button {
  position: fixed;
  bottom: 80px;
  right: 20px;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
  z-index: 10;
}
</style>
