<template>
  <div class="auth-container">
    <div v-if="mode === 'login'">
      <h2>Login</h2>
      <form @submit.prevent="handleLogin">
        <input v-model="loginForm.UserName" placeholder="Username" required />
        <input v-model="loginForm.Password" type="password" placeholder="Password" required />
        <button type="submit" :disabled="loading">{{ loading ? 'Logging in…' : 'Login' }}</button>
      </form>
      <p>Don't have an account? <a href="#" @click.prevent="mode = 'register'">Register</a></p>
    </div>
    <div v-else>
      <h2>Register</h2>
      <form @submit.prevent="handleRegister">
        <input v-model="registerForm.UserName" placeholder="Username" required />
        <input v-model="registerForm.Password" type="password" placeholder="Password" required />
        <select v-model="registerForm.companyId" required>
          <option value="" disabled>Select Company</option>
          <option v-for="company in companies" :key="company.id" :value="company.id">
            {{ company.name }}
          </option>
        </select>
        <button type="submit" :disabled="loading">{{ loading ? 'Registering…' : 'Register' }}</button>
      </form>
      <p>Already have an account? <a href="#" @click.prevent="mode = 'login'">Login</a></p>
    </div>
    <div v-if="error" class="error">{{ error }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useFetch } from '@vueuse/core'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const mode = ref<'login' | 'register'>('login')
const loginForm = ref({ UserName: '123', Password: '123' })
const registerForm = ref({ UserName: '', Password: '', companyId: '' })
const error = ref('')
const loading = ref(false)
const router = useRouter()
const authStore = useAuthStore()

const { data: companiesData, execute: loadCompanies } = useFetch('/api/Companies', {
  immediate: false,
}).json()

const companies = computed(() => (companiesData.value as any[]) ?? [])

watch(mode, (val) => {
  if (val === 'register' && !companiesData.value) {
    loadCompanies()
  }
})

async function handleLogin() {
  error.value = ''
  loading.value = true
  try {
    const ok = await authStore.login(loginForm.value.UserName, loginForm.value.Password)
    if (ok) {
      router.push('/')
    } else {
      error.value = 'Invalid username or password.'
    }
  } catch {
    error.value = 'Login failed. Please try again.'
  } finally {
    loading.value = false
  }
}

async function handleRegister() {
  error.value = ''
  loading.value = true
  try {
    const res = await fetch('/api/Auth/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        UserName: registerForm.value.UserName,
        Password: registerForm.value.Password,
        companyId: Number(registerForm.value.companyId),
      }),
    })
    if (res.ok) {
      registerForm.value = { UserName: '', Password: '', companyId: '' }
      mode.value = 'login'
    } else {
      error.value = 'Registration failed. Username may be taken.'
    }
  } catch {
    error.value = 'Registration failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-container {
  max-width: 400px;
  margin: 60px auto;
  padding: 2rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fff;
}
input,
select {
  display: block;
  width: 100%;
  margin-bottom: 1rem;
  padding: 0.5rem;
}
button {
  width: 100%;
  padding: 0.7rem;
  background: #42b983;
  color: #fff;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
}
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.error {
  color: red;
  margin-top: 1rem;
}
</style>
