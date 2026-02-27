<script setup lang="ts">
import type { UserSession } from "../types";

defineProps<{
  session: UserSession;
}>();

defineEmits<{
  logout: [];
}>();
</script>

<template>
  <main class="min-h-screen bg-app-base px-4 pb-24 pt-5 text-app-text sm:px-6 lg:px-8 lg:pb-8">
    <section class="mx-auto max-w-[1500px]">
      <header
        class="mb-4 flex items-center justify-between rounded-2xl border border-app-border bg-app-panel px-4 py-3 shadow-soft backdrop-blur lg:mb-6"
      >
        <div>
          <p class="text-xs font-semibold uppercase tracking-[0.18em] text-app-muted">Workspace</p>
          <h1 class="text-lg font-semibold sm:text-xl">Notes Application</h1>
        </div>
        <div class="flex items-center gap-2">
          <p class="hidden text-sm text-app-muted md:block">{{ session.name }} - {{ session.email }}</p>
          <button class="btn btn-secondary" type="button" @click="$emit('logout')">Logout</button>
        </div>
      </header>
      <slot />
    </section>
  </main>
</template>
