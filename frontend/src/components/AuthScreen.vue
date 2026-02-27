<script setup lang="ts">
type AuthMode = "login" | "register";

defineProps<{
  mode: AuthMode;
  name: string;
  email: string;
  password: string;
  error: string;
  loading: boolean;
}>();

defineEmits<{
  submit: [];
  toggleMode: [];
  "update:name": [value: string];
  "update:email": [value: string];
  "update:password": [value: string];
}>();
</script>

<template>
  <main class="auth-bg min-h-screen px-4 py-10 sm:px-6">
    <section class="mx-auto w-full max-w-md rounded-2xl border border-app-border bg-app-panel p-5 shadow-soft sm:p-7">
      <p class="text-xs font-semibold uppercase tracking-[0.16em] text-app-muted">Welcome</p>
      <h1 class="mt-2 text-2xl font-semibold text-app-text">Notes Application</h1>
      <p class="mt-1 text-sm text-app-muted">Sign in to manage your notes workspace.</p>

      <form class="mt-6 space-y-4" @submit.prevent="$emit('submit')">
        <label v-if="mode === 'register'" class="form-label">
          Name
          <input class="field mt-1" :value="name" required @input="$emit('update:name', ($event.target as HTMLInputElement).value)" />
        </label>

        <label class="form-label">
          Email
          <input
            class="field mt-1"
            type="email"
            :value="email"
            required
            @input="$emit('update:email', ($event.target as HTMLInputElement).value)"
          />
        </label>

        <label class="form-label">
          Password
          <input
            class="field mt-1"
            type="password"
            minlength="6"
            :value="password"
            required
            @input="$emit('update:password', ($event.target as HTMLInputElement).value)"
          />
        </label>

        <p v-if="error" class="text-sm font-medium text-rose-700">{{ error }}</p>

        <button class="btn btn-primary w-full justify-center" type="submit" :disabled="loading">
          {{ mode === "login" ? "Login" : "Register" }}
        </button>
      </form>

      <button class="mt-4 text-sm font-medium text-app-accent transition hover:text-app-accent-strong" type="button" @click="$emit('toggleMode')">
        {{ mode === "login" ? "Create an account" : "Already have an account?" }}
      </button>
    </section>
  </main>
</template>
