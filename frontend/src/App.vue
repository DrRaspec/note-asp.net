<script setup lang="ts">
import { computed, ref, watch } from "vue";
import {
  clearAuthContext,
  createNote,
  deleteNote,
  getNote,
  getNotes,
  login,
  logout,
  register,
  setAuthContext,
  updateNote
} from "./api";
import type { Note, UserSession } from "./types";

const SESSION_KEY = "notes-app-session";

type AuthMode = "login" | "register";

const session = ref<UserSession | null>(loadSession());
const mode = ref<AuthMode>("login");
const authName = ref("");
const authEmail = ref("");
const authPassword = ref("");
const authError = ref("");
const notes = ref<Note[]>([]);
const selectedNoteId = ref<number | null>(null);
const editorTitle = ref("");
const editorContent = ref("");
const search = ref("");
const sortBy = ref("updatedAt");
const sortDir = ref("desc");
const page = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = ref(0);
const message = ref("");
const loading = ref(false);

const selectedNote = computed(() => notes.value.find((note) => note.id === selectedNoteId.value) ?? null);

watch(
  session,
  (value) => {
    if (!value) {
      clearAuthContext();
      return;
    }

    setAuthContext(
      value,
      (nextSession) => {
        persistSession(nextSession);
        session.value = nextSession;
      },
      () => {
        clearSession();
        session.value = null;
      }
    );
  },
  { immediate: true }
);

watch(
  [session, search, sortBy, sortDir, page, pageSize],
  () => {
    if (!session.value) {
      return;
    }

    void refreshNotes();
  },
  { immediate: true }
);

async function handleAuthSubmit(): Promise<void> {
  authError.value = "";
  message.value = "";
  loading.value = true;

  try {
    const result =
      mode.value === "login"
        ? await login(authEmail.value, authPassword.value)
        : await register(authName.value, authEmail.value, authPassword.value);

    persistSession(result);
    session.value = result;
    authPassword.value = "";
    authName.value = "";
    message.value = "Authenticated successfully.";
  } catch {
    authError.value = mode.value === "login" ? "Invalid credentials." : "Registration failed.";
  } finally {
    loading.value = false;
  }
}

async function refreshNotes(): Promise<void> {
  if (!session.value) {
    return;
  }

  loading.value = true;
  try {
    const result = await getNotes(search.value, sortBy.value, sortDir.value, page.value, pageSize.value);
    if (result.totalPages > 0 && page.value > result.totalPages) {
      page.value = result.totalPages;
      return;
    }
    if (result.totalPages === 0 && page.value !== 1) {
      page.value = 1;
      return;
    }

    notes.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;

    if (
      result.items.length > 0 &&
      (selectedNoteId.value === null || !result.items.some((note) => note.id === selectedNoteId.value))
    ) {
      selectedNoteId.value = result.items[0].id;
    }

    if (result.items.length === 0) {
      selectedNoteId.value = null;
      editorTitle.value = "";
      editorContent.value = "";
    }
  } finally {
    loading.value = false;
  }
}

async function handleSelectNote(noteId: number): Promise<void> {
  message.value = "";
  loading.value = true;
  try {
    const note = await getNote(noteId);
    selectedNoteId.value = note.id;
    editorTitle.value = note.title;
    editorContent.value = note.content;
  } finally {
    loading.value = false;
  }
}

async function handleCreateNote(): Promise<void> {
  message.value = "";
  if (!editorTitle.value.trim()) {
    message.value = "Title is required.";
    return;
  }

  loading.value = true;
  try {
    const created = await createNote(editorTitle.value, editorContent.value);
    page.value = 1;
    const result = await getNotes(search.value, sortBy.value, sortDir.value, 1, pageSize.value);
    notes.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;
    selectedNoteId.value = created.id;
    message.value = "Note created.";
  } finally {
    loading.value = false;
  }
}

async function handleUpdateNote(): Promise<void> {
  message.value = "";
  if (!selectedNoteId.value) {
    message.value = "Select a note first.";
    return;
  }
  if (!editorTitle.value.trim()) {
    message.value = "Title is required.";
    return;
  }

  loading.value = true;
  try {
    await updateNote(selectedNoteId.value, editorTitle.value, editorContent.value);
    await refreshNotes();
    message.value = "Note updated.";
  } finally {
    loading.value = false;
  }
}

async function handleDeleteNote(): Promise<void> {
  message.value = "";
  if (!selectedNoteId.value) {
    message.value = "Select a note first.";
    return;
  }

  loading.value = true;
  try {
    await deleteNote(selectedNoteId.value);
    await refreshNotes();
    message.value = "Note deleted.";
  } finally {
    loading.value = false;
  }
}

function handleNewDraft(): void {
  selectedNoteId.value = null;
  editorTitle.value = "";
  editorContent.value = "";
  message.value = "New draft.";
}

async function handleLogout(): Promise<void> {
  try {
    await logout();
  } catch {
    // ignore logout call failures; local session is still cleared
  }

  clearSession();
  clearAuthContext();
  session.value = null;
  notes.value = [];
  selectedNoteId.value = null;
  editorTitle.value = "";
  editorContent.value = "";
  message.value = "";
  page.value = 1;
  totalCount.value = 0;
  totalPages.value = 0;
}

function loadSession(): UserSession | null {
  try {
    const raw = localStorage.getItem(SESSION_KEY);
    return raw ? (JSON.parse(raw) as UserSession) : null;
  } catch {
    return null;
  }
}

function persistSession(value: UserSession): void {
  localStorage.setItem(SESSION_KEY, JSON.stringify(value));
}

function clearSession(): void {
  localStorage.removeItem(SESSION_KEY);
}
</script>

<template>
  <main v-if="!session" class="min-h-screen bg-slate-100 p-6">
    <section class="mx-auto mt-8 w-full max-w-md rounded-xl bg-white p-6 shadow-sm">
      <h1 class="text-2xl font-semibold text-slate-900">Notes Application</h1>
      <p class="mt-1 text-sm text-slate-500">Login or register to manage your notes.</p>
      <form class="mt-6 space-y-4" @submit.prevent="handleAuthSubmit">
        <label v-if="mode === 'register'" class="block text-sm text-slate-700">
          Name
          <input
            v-model="authName"
            class="mt-1 w-full rounded-md border border-slate-300 px-3 py-2"
            required
          />
        </label>
        <label class="block text-sm text-slate-700">
          Email
          <input
            v-model="authEmail"
            class="mt-1 w-full rounded-md border border-slate-300 px-3 py-2"
            type="email"
            required
          />
        </label>
        <label class="block text-sm text-slate-700">
          Password
          <input
            v-model="authPassword"
            class="mt-1 w-full rounded-md border border-slate-300 px-3 py-2"
            type="password"
            minlength="6"
            required
          />
        </label>
        <p v-if="authError" class="text-sm text-red-600">{{ authError }}</p>
        <button class="w-full rounded-md bg-blue-600 px-3 py-2 font-medium text-white disabled:opacity-50" type="submit" :disabled="loading">
          {{ mode === "login" ? "Login" : "Register" }}
        </button>
      </form>
      <button
        class="mt-3 text-sm text-blue-700"
        type="button"
        @click="mode = mode === 'login' ? 'register' : 'login'"
      >
        {{ mode === "login" ? "Create an account" : "Already have an account?" }}
      </button>
    </section>
  </main>

  <main v-else class="min-h-screen bg-slate-100 p-4 md:p-6">
    <section class="mx-auto max-w-6xl rounded-xl bg-white p-4 shadow-sm md:p-6">
      <header class="mb-4 flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 pb-4">
        <div>
          <h1 class="text-2xl font-semibold text-slate-900">Notes Application</h1>
          <p class="text-sm text-slate-600">Signed in as {{ session.name }} ({{ session.email }})</p>
        </div>
        <button class="rounded-md bg-slate-800 px-3 py-2 text-sm text-white" @click="handleLogout">Logout</button>
      </header>

      <div class="mb-4 grid gap-3 md:grid-cols-4">
        <input
          v-model="search"
          class="rounded-md border border-slate-300 px-3 py-2"
          placeholder="Search notes..."
          @input="page = 1"
        />
        <select v-model="sortBy" class="rounded-md border border-slate-300 px-3 py-2" @change="page = 1">
          <option value="updatedAt">Sort by updated date</option>
          <option value="createdAt">Sort by created date</option>
          <option value="title">Sort by title</option>
        </select>
        <select v-model="sortDir" class="rounded-md border border-slate-300 px-3 py-2" @change="page = 1">
          <option value="desc">Descending</option>
          <option value="asc">Ascending</option>
        </select>
        <select v-model.number="pageSize" class="rounded-md border border-slate-300 px-3 py-2" @change="page = 1">
          <option :value="5">5 per page</option>
          <option :value="10">10 per page</option>
          <option :value="20">20 per page</option>
        </select>
      </div>

      <div class="grid gap-4 md:grid-cols-[320px_1fr]">
        <aside class="rounded-lg border border-slate-200">
          <div class="flex items-center justify-between border-b border-slate-200 p-3">
            <h2 class="font-medium text-slate-800">Notes ({{ totalCount }})</h2>
            <button class="rounded bg-blue-600 px-2 py-1 text-xs text-white" @click="handleNewDraft">New</button>
          </div>
          <ul class="max-h-[500px] overflow-auto">
            <li v-for="note in notes" :key="note.id">
              <button
                class="w-full border-b border-slate-100 px-3 py-3 text-left hover:bg-slate-50"
                :class="note.id === selectedNoteId ? 'bg-blue-50' : ''"
                @click="handleSelectNote(note.id)"
              >
                <p class="truncate font-medium text-slate-800">{{ note.title }}</p>
                <p class="mt-1 text-xs text-slate-500">Created {{ new Date(note.createdAt).toLocaleString() }}</p>
              </button>
            </li>
            <li v-if="notes.length === 0" class="p-3 text-sm text-slate-500">No notes found.</li>
          </ul>
          <div class="flex items-center justify-between border-t border-slate-200 p-3 text-xs text-slate-600">
            <button
              class="rounded border border-slate-300 px-2 py-1 disabled:opacity-50"
              :disabled="page <= 1 || loading"
              @click="page = Math.max(1, page - 1)"
            >
              Prev
            </button>
            <span>Page {{ page }} of {{ Math.max(1, totalPages) }}</span>
            <button
              class="rounded border border-slate-300 px-2 py-1 disabled:opacity-50"
              :disabled="totalPages === 0 || page >= totalPages || loading"
              @click="page = Math.min(Math.max(1, totalPages), page + 1)"
            >
              Next
            </button>
          </div>
        </aside>

        <section class="rounded-lg border border-slate-200 p-4">
          <label class="block text-sm text-slate-700">
            Title
            <input
              v-model="editorTitle"
              class="mt-1 w-full rounded-md border border-slate-300 px-3 py-2"
              placeholder="Note title"
            />
          </label>
          <label class="mt-3 block text-sm text-slate-700">
            Content
            <textarea
              v-model="editorContent"
              class="mt-1 h-64 w-full rounded-md border border-slate-300 px-3 py-2"
              placeholder="Write your note..."
            />
          </label>
          <p v-if="selectedNote" class="mt-2 text-xs text-slate-500">
            Updated {{ new Date(selectedNote.updatedAt).toLocaleString() }}
          </p>
          <div class="mt-4 flex flex-wrap gap-2">
            <button
              class="rounded-md bg-green-600 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
              :disabled="loading"
              @click="selectedNoteId ? handleUpdateNote() : handleCreateNote()"
            >
              {{ selectedNoteId ? "Update note" : "Create note" }}
            </button>
            <button
              v-if="selectedNoteId"
              class="rounded-md bg-red-600 px-3 py-2 text-sm font-medium text-white disabled:opacity-50"
              :disabled="loading"
              @click="handleDeleteNote"
            >
              Delete note
            </button>
          </div>
          <p v-if="message" class="mt-3 text-sm text-blue-700">{{ message }}</p>
        </section>
      </div>
    </section>
  </main>
</template>
