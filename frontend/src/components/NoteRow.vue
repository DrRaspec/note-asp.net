<script setup lang="ts">
import { computed } from "vue";
import type { Note } from "../types";

const props = defineProps<{
  note: Note;
  active: boolean;
}>();

const snippet = computed(() => {
  if (!props.note.content.trim()) {
    return "No content yet.";
  }
  return props.note.content.replace(/\s+/g, " ").slice(0, 88);
});
</script>

<template>
  <article
    class="group rounded-2xl border p-3 text-left transition duration-200"
    :class="
      active
        ? 'border-app-accent bg-app-base shadow-soft'
        : 'border-app-border-soft bg-white hover:-translate-y-0.5 hover:border-app-accent hover:shadow-soft'
    "
  >
    <header class="flex items-start justify-between gap-2">
      <h3 class="line-clamp-1 text-sm font-semibold text-app-text">{{ note.title }}</h3>
      <time class="shrink-0 text-xs text-app-muted">{{ new Date(note.updatedAt).toLocaleDateString() }}</time>
    </header>
    <p class="mt-1 line-clamp-2 text-xs text-app-muted">{{ snippet }}</p>
    <footer class="mt-3 flex items-center justify-between text-xs text-app-muted">
      <span>Created {{ new Date(note.createdAt).toLocaleDateString() }}</span>
      <span>Updated {{ new Date(note.updatedAt).toLocaleDateString() }}</span>
    </footer>
  </article>
</template>
