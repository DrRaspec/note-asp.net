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
      <p class="text-xs font-semibold uppercase tracking-[0.14em] text-app-muted">Quick Filters</p>
      <ul class="mt-3 space-y-2 text-sm">
        <li class="flex items-center justify-between rounded-xl bg-app-base px-3 py-2 text-app-text">
          <span>All Notes</span>
          <span class="rounded-full bg-white px-2 py-0.5 text-xs text-app-muted">{{ totalCount }}</span>
        </li>
        <li class="rounded-xl px-3 py-2 text-app-muted">Recently Updated</li>
        <li class="rounded-xl px-3 py-2 text-app-muted">Created This Week</li>
      </ul>
    </section>

    <section class="rounded-2xl border border-app-border-soft bg-white/70 p-4">
      <p class="text-xs font-semibold uppercase tracking-[0.14em] text-app-muted">Tags</p>
      <div class="mt-3 flex flex-wrap gap-2">
        <span class="tag-chip">Work</span>
        <span class="tag-chip">Study</span>
        <span class="tag-chip">Ideas</span>
      </div>
      <p class="mt-3 text-xs text-app-muted">Tag management can be added without changing note logic.</p>
    </section>
  </aside>
</template>
