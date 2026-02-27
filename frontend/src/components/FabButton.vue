<script setup lang="ts">
defineEmits<{
  click: [];
}>();
</script>

<template>
  <button
    aria-label="Create new note"
    class="fixed bottom-20 right-5 z-50 rounded-full bg-app-accent px-5 py-4 text-sm font-semibold text-white shadow-soft transition hover:scale-[1.03] hover:bg-app-accent-strong focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-app-accent focus-visible:ring-offset-2 lg:hidden"
    type="button"
    @click="$emit('click')"
  >
    + New
  </button>
</template>
