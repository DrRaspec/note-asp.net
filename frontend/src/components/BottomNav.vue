<script setup lang="ts">
type MobileTab = "notes" | "search" | "tags" | "settings";

defineProps<{
  activeTab: MobileTab;
}>();

defineEmits<{
  change: [tab: MobileTab];
}>();

const tabs: Array<{ id: MobileTab; label: string }> = [
  { id: "notes", label: "Notes" },
  { id: "search", label: "Search" },
  { id: "tags", label: "Tags" },
  { id: "settings", label: "Settings" }
];
</script>

<template>
  <nav class="fixed inset-x-0 bottom-0 z-40 border-t border-app-border bg-app-panel p-2 backdrop-blur lg:hidden">
    <ul class="mx-auto grid max-w-xl grid-cols-4 gap-2">
      <li v-for="tab in tabs" :key="tab.id">
        <button
          class="w-full rounded-xl px-2 py-2 text-xs font-medium transition focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-app-accent focus-visible:ring-offset-2"
          :class="activeTab === tab.id ? 'bg-app-accent text-white shadow-soft' : 'text-app-muted hover:bg-app-border-soft'"
          type="button"
          @click="$emit('change', tab.id)"
        >
          {{ tab.label }}
        </button>
      </li>
    </ul>
  </nav>
</template>
