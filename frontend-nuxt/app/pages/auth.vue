<script setup lang="ts">
definePageMeta({
  layout: 'auth',
})

interface AuthResponse {
  token: string
  role: string
  userName: string
  companyName: string
}

const config = useRuntimeConfig()
const username = ref('')
const password = ref('')
const errorMessage = ref('')
const isSubmitting = ref(false)
const auth = useState<AuthResponse | null>('auth', () => null)

async function handleSubmit() {
  errorMessage.value = ''

  if (!username.value || !password.value) {
    errorMessage.value = 'Enter your username and password.'
    return
  }

  isSubmitting.value = true

  try {
    auth.value = await $fetch<AuthResponse>('/api/Auth/login', {
      baseURL: config.public.apiBase,
      method: 'POST',
      credentials: 'include',
      body: {
        UserName: username.value,
        Password: password.value,
      },
    })

    await navigateTo('/')
  } catch {
    errorMessage.value = 'Invalid username or password.'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<template>
  <section class="login">
    <header>
      <h1>Sign in</h1>
      <p>Enter your details to access the reporting application.</p>
    </header>

    <form @submit.prevent="handleSubmit">
      <div class="field">
        <label for="username">Username</label>
        <input
          id="username"
          v-model.trim="username"
          name="username"
          type="text"
          autocomplete="username"
          required
        >
      </div>

      <div class="field">
        <label for="password">Password</label>
        <input
          id="password"
          v-model="password"
          name="password"
          type="password"
          autocomplete="current-password"
          required
        >
      </div>

      <p v-if="errorMessage" class="error" role="alert">
        {{ errorMessage }}
      </p>

      <button type="submit" :disabled="isSubmitting">
        {{ isSubmitting ? 'Signing in...' : 'Sign in' }}
      </button>
    </form>
  </section>
</template>

<style scoped>
.login {
  width: min(100%, 22rem);
}

header {
  margin-bottom: 1.5rem;
}

h1 {
  margin: 0;
}

header p {
  margin: 0.5rem 0 0;
  color: #666;
}

form,
.field {
  display: grid;
  gap: 1rem;
}

.field {
  gap: 0.375rem;
}

label {
  font-weight: 600;
}

input {
  box-sizing: border-box;
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #bbb;
  border-radius: 0.375rem;
  font: inherit;
}

input:focus-visible {
  border-color: #008f5d;
  outline: 2px solid rgb(0 220 130 / 25%);
}

button {
  padding: 0.75rem 1rem;
  border: 0;
  border-radius: 0.375rem;
  background: #008f5d;
  color: white;
  font: inherit;
  font-weight: 700;
  cursor: pointer;
}

button:disabled {
  cursor: wait;
  opacity: 0.65;
}

.error {
  margin: 0;
  color: #b42318;
}
</style>
