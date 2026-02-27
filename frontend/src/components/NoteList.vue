<script setup lang="ts">
import NoteRow from "./NoteRow.vue";
import type { Note } from "../types";

defineProps<{
  notes: Note[];
  selectedNoteId: number | null;
  loading: boolean;
  page: number;
  totalPages: number;
  totalCount: number;
  search: string;
  sortBy: string;
  sortDir: string;
  pageSize: number;
}>();

defineEmits<{
  select: [id: number];
  new: [];
  prev: [];
  next: [];
  "update:search": [value: string];
  "update:sortBy": [value: string];
  "update:sortDir": [value: string];
  "update:pageSize": [value: number];
}>();
</script>

<template>
  <section class="rounded-2xl border border-app-border bg-app-panel p-3 shadow-soft sm:p-4">
    <header class="mb-3 flex items-center justify-between gap-2">
      <div>
        <h2 class="text-base font-semibold text-app-text">Notes</h2>
        <p class="text-xs text-app-muted">{{ totalCount }} total</p>
      </div>
      <button class="btn btn-primary" type="button" @click="$emit('new')">New Note</button>
    </header>

    <div class="grid gap-2 sm:grid-cols-2">
      <label class="form-label sm:col-span-2">
        Search
        <input
          :value="search"
          class="field mt-1"
          placeholder="Search title or content"
          type="search"
          @input="$emit('update:search', ($event.target as HTMLInputElement).value)"
        />
      </label>
      <label class="form-label">
        Sort By
        <select class="field mt-1" :value="sortBy" @change="$emit('update:sortBy', ($event.target as HTMLSelectElement).value)">
          <option value="updatedAt">Updated Date</option>
          <option value="createdAt">Created Date</option>
          <option value="title">Title</option>
        </select>
      </label>
      <label class="form-label">
        Direction
        <select class="field mt-1" :value="sortDir" @change="$emit('update:sortDir', ($event.target as HTMLSelectElement).value)">
          <option value="desc">Newest First</option>
          <option value="asc">Oldest First</option>
        </select>
      </label>
    </div>

    <div class="mt-3 space-y-2">
      <div v-if="loading && notes.length === 0" class="space-y-2">
        <div v-for="idx in 4" :key="idx" class="h-20 animate-pulse rounded-2xl bg-app-border-soft" />
      </div>

      <button
        v-for="note in notes"
        v-else
        :key="note.id"
        class="w-full text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-app-accent focus-visible:ring-offset-2"
        type="button"
        @click="$emit('select', note.id)"
      >
        <NoteRow :note="note" :active="note.id === selectedNoteId" />
      </button>

      <p v-if="!loading && notes.length === 0" class="rounded-2xl border border-dashed border-app-border p-6 text-center text-sm text-app-muted">
        No notes found. Create your first note to get started.
      </p>
    </div>

    <footer class="mt-4 flex flex-wrap items-center justify-between gap-2">
      <div class="flex items-center gap-2">
        <button class="btn btn-ghost" type="button" :disabled="page <= 1 || loading" @click="$emit('prev')">Prev</button>
        <button class="btn btn-ghost" type="button" :disabled="totalPages === 0 || page >= totalPages || loading" @click="$emit('next')">
          Next
        </button>
      </div>
      <p class="text-xs text-app-muted">Page {{ page }} / {{ Math.max(1, totalPages) }}</p>
      <select class="field w-[126px]" :value="pageSize" @change="$emit('update:pageSize', Number(($event.target as HTMLSelectElement).value))">
        <option :value="5">5 per page</option>
        <option :value="10">10 per page</option>
        <option :value="20">20 per page</option>
      </select>
    </footer>
  </section>
</template>
