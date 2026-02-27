<script setup lang="ts">
import type { Note } from "../types";

defineProps<{
  selectedNoteId: number | null;
  selectedNote: Note | null;
  editorTitle: string;
  editorContent: string;
  loading: boolean;
  message: string;
  mobile: boolean;
}>();

defineEmits<{
  "update:title": [value: string];
  "update:content": [value: string];
  save: [];
  delete: [];
  close: [];
}>();
</script>

<template>
  <section class="rounded-2xl border border-app-border bg-app-panel p-4 shadow-soft sm:p-5">
    <header class="mb-4 flex items-center justify-between gap-2">
      <div>
        <p class="text-xs font-semibold uppercase tracking-[0.16em] text-app-muted">Editor</p>
        <h2 class="text-base font-semibold text-app-text">{{ selectedNoteId ? "Edit note" : "Create note" }}</h2>
      </div>
      <button v-if="mobile" class="btn btn-ghost lg:hidden" type="button" @click="$emit('close')">Back</button>
    </header>

    <div class="mb-3 flex items-center gap-2 rounded-2xl border border-app-border-soft bg-white/70 px-3 py-2">
      <button class="toolbar-btn" type="button" aria-label="Bold">B</button>
      <button class="toolbar-btn" type="button" aria-label="Italic"><span class="italic">I</span></button>
      <button class="toolbar-btn" type="button" aria-label="Heading">H</button>
      <div class="ml-auto text-xs text-app-muted">{{ loading ? "Saving..." : "Ready" }}</div>
    </div>

    <label class="form-label">
      Title
      <input
        class="field mt-1"
        placeholder="Note title"
        :value="editorTitle"
        @input="$emit('update:title', ($event.target as HTMLInputElement).value)"
      />
    </label>

    <label class="form-label mt-3">
      Content
      <textarea
        class="field mt-1 min-h-[280px] resize-y"
        placeholder="Write your note..."
        :value="editorContent"
        @input="$emit('update:content', ($event.target as HTMLTextAreaElement).value)"
      />
    </label>

    <p v-if="selectedNote" class="mt-2 text-xs text-app-muted">
      Updated {{ new Date(selectedNote.updatedAt).toLocaleString() }}
    </p>

    <div class="mt-4 flex flex-wrap items-center gap-2">
      <button class="btn btn-primary" type="button" :disabled="loading" @click="$emit('save')">
        {{ selectedNoteId ? "Update Note" : "Create Note" }}
      </button>
      <button v-if="selectedNoteId" class="btn btn-danger" type="button" :disabled="loading" @click="$emit('delete')">Delete</button>
    </div>

    <transition name="fade-slide">
      <p v-if="message" class="toast mt-3">{{ message }}</p>
    </transition>
  </section>
</template>
