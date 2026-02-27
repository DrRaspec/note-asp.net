<script setup lang="ts">
import type { UserSession } from "../types";

defineProps<{
  session: UserSession;
  totalCount: number;
}>();
</script>

<template>
  <aside class="hidden rounded-2xl border border-app-border bg-app-panel p-4 shadow-soft lg:flex lg:flex-col lg:gap-4">
    <section class="rounded-2xl border border-app-border-soft bg-white/70 p-4">
      <p class="text-xs font-semibold uppercase tracking-[0.14em] text-app-muted">Profile</p>
      <h2 class="mt-2 text-base font-semibold text-app-text">{{ session.name }}</h2>
      <p class="text-sm text-app-muted">{{ session.email }}</p>
    </section>

    <section class="rounded-2xl border border-app-border-soft bg-white/70 p-4">
      <p class="text-xs font-semibold uppercase tracking-[0.14em] text-app-muted">Overview</p>
      <div class="mt-3 rounded-xl bg-app-base px-3 py-3">
        <p class="text-sm font-semibold text-app-text">{{ totalCount }} notes</p>
        <p class="mt-1 text-xs text-app-muted">Use search and sort in the notes panel to find your content quickly.</p>
      </div>
    </section>
  </aside>
</template>
