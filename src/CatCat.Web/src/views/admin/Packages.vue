<template>
  <div class="admin-packages-page">
    <va-card>
      <va-card-title>
        <div class="page-header">
          <h1 class="va-h1">Package Management</h1>
          <va-button color="primary" @click="showCreateModal = true">
            <va-icon name="add" /> Create Package
          </va-button>
        </div>
      </va-card-title>

      <va-card-content>
        <!-- Packages Grid -->
        <div v-if="loading" class="va-row">
          <div v-for="i in 3" :key="i" class="flex xs12 md4">
            <va-skeleton height="200px" />
          </div>
        </div>

        <div v-else class="va-row">
          <div v-for="pkg in packages" :key="pkg.id" class="flex xs12 md4">
            <va-card hover class="package-card">
              <va-card-content>
                <div class="package-header">
                  <h3 class="va-h3">{{ pkg.name }}</h3>
                  <va-switch
                    v-model="pkg.isActive"
                    @update:modelValue="toggleStatus(pkg)"
                  />
                </div>
                <p class="va-text-secondary">{{ pkg.description }}</p>
                <va-divider />
                <div class="package-meta">
                  <va-chip size="small" color="primary">
                    ¥{{ pkg.price }}
                  </va-chip>
                  <va-chip size="small" color="info">
                    {{ pkg.duration }} min
                  </va-chip>
                </div>
                <va-divider />
                <div class="package-actions">
                  <va-button size="small" preset="plain" icon="edit" @click="editPackage(pkg)">
                    Edit
                  </va-button>
                  <va-button size="small" preset="plain" icon="delete" color="danger" @click="confirmDelete(pkg)">
                    Delete
                  </va-button>
                </div>
              </va-card-content>
            </va-card>
          </div>
        </div>
      </va-card-content>
    </va-card>

    <!-- Create/Edit Modal -->
    <va-modal
      v-model="showCreateModal"
      :title="isEditing ? 'Edit Package' : 'Create Package'"
      size="medium"
      ok-text="Save"
      @ok="savePackage"
    >
      <va-form ref="formRef">
        <va-input
          v-model="form.name"
          label="Package Name"
          :rules="[(v: string) => !!v || 'Name is required']"
        />
        <va-textarea
          v-model="form.description"
          label="Description"
          :rules="[(v: string) => !!v || 'Description is required']"
        />
        <va-input
          v-model.number="form.price"
          label="Price (¥)"
          type="number"
        />
        <va-input
          v-model.number="form.duration"
          label="Duration (minutes)"
          type="number"
        />
        <va-input
          v-model="form.iconUrl"
          label="Icon URL (optional)"
        />
        <va-textarea
          v-model="form.serviceItems"
          label="Service Items (separated by 、)"
          :rules="[(v: string) => !!v || 'Service items are required']"
        />
        <va-input
          v-model.number="form.sortOrder"
          label="Sort Order"
          type="number"
        />
      </va-form>
    </va-modal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import {
  getPackages,
  createPackage,
  updatePackage,
  deletePackage,
  togglePackageStatus,
  type ServicePackage
} from '@/api/admin'
import { useToast, useModal } from 'vuestic-ui'

const { init: notify } = useToast()
const { confirm } = useModal()

const loading = ref(false)
const packages = ref<ServicePackage[]>([])
const showCreateModal = ref(false)
const isEditing = ref(false)
const form = ref({
  id: 0,
  name: '',
  description: '',
  price: 0,
  duration: 0,
  iconUrl: '',
  serviceItems: '',
  isActive: true,
  sortOrder: 0
})

const fetchPackages = async () => {
  loading.value = true
  try {
    const res = await getPackages({ page: 1, pageSize: 100 })
    packages.value = res.data.items
  } catch (error: any) {
    notify({ message: error.message || 'Failed to load packages', color: 'danger' })
  } finally {
    loading.value = false
  }
}

const editPackage = (pkg: ServicePackage) => {
  form.value = {
    id: pkg.id,
    name: pkg.name,
    description: pkg.description,
    price: pkg.price,
    duration: pkg.duration,
    iconUrl: pkg.iconUrl || '',
    serviceItems: pkg.serviceItems,
    isActive: pkg.isActive,
    sortOrder: pkg.sortOrder
  }
  isEditing.value = true
  showCreateModal.value = true
}

const savePackage = async () => {
  try {
    if (isEditing.value) {
      await updatePackage(form.value.id, form.value)
      notify({ message: 'Package updated', color: 'success' })
    } else {
      await createPackage(form.value)
      notify({ message: 'Package created', color: 'success' })
    }
    showCreateModal.value = false
    resetForm()
    fetchPackages()
  } catch (error: any) {
    notify({ message: error.message || 'Save failed', color: 'danger' })
  }
}

const confirmDelete = async (pkg: ServicePackage) => {
  const agreed = await confirm({
    title: 'Delete Package',
    message: `Are you sure you want to delete "${pkg.name}"?`,
    okText: 'Delete',
    cancelText: 'Cancel'
  })

  if (agreed) {
    try {
      await deletePackage(pkg.id)
      notify({ message: 'Package deleted', color: 'success' })
      fetchPackages()
    } catch (error: any) {
      notify({ message: error.message || 'Delete failed', color: 'danger' })
    }
  }
}

const toggleStatus = async (pkg: ServicePackage) => {
  try {
    await togglePackageStatus(pkg.id, pkg.isActive)
    notify({ message: 'Status updated', color: 'success' })
  } catch (error: any) {
    notify({ message: error.message || 'Update failed', color: 'danger' })
    pkg.isActive = !pkg.isActive // Revert on error
  }
}

const resetForm = () => {
  form.value = {
    id: 0,
    name: '',
    description: '',
    price: 0,
    duration: 0,
    iconUrl: '',
    serviceItems: '',
    isActive: true,
    sortOrder: 0
  }
  isEditing.value = false
}

onMounted(() => {
  fetchPackages()
})
</script>

<style scoped>
.admin-packages-page {
  padding: var(--va-content-padding);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.package-card {
  width: 100%;
  height: 100%;
  margin-bottom: var(--va-content-padding);
}

.package-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.package-meta {
  display: flex;
  gap: 8px;
  margin: 12px 0;
}

.package-actions {
  display: flex;
  gap: 8px;
  justify-content: flex-end;
  margin-top: 12px;
}

@media (max-width: 768px) {
  .admin-packages-page {
    padding: 12px;
  }
}
</style>

