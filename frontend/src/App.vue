<script setup lang="ts">
import { computed, ref, watch } from "vue";
import AppShell from "./components/AppShell.vue";
import AuthScreen from "./components/AuthScreen.vue";
import BottomNav from "./components/BottomNav.vue";
import EditorPanel from "./components/EditorPanel.vue";
import FabButton from "./components/FabButton.vue";
import NoteList from "./components/NoteList.vue";
import Sidebar from "./components/Sidebar.vue";
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
type MobileTab = "notes" | "search" | "tags" | "settings";

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
const mobileTab = ref<MobileTab>("notes");
const mobileEditorOpen = ref(false);

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

function resetPagingFiltersAndSearch(): void {
  page.value = 1;
}

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
    mobileEditorOpen.value = true;
    mobileTab.value = "notes";
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
  mobileEditorOpen.value = true;
  mobileTab.value = "notes";
}

function handleMobileBackToList(): void {
  mobileEditorOpen.value = false;
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
  mobileTab.value = "notes";
  mobileEditorOpen.value = false;
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
  <AuthScreen
    v-if="!session"
    :mode="mode"
    :name="authName"
    :email="authEmail"
    :password="authPassword"
    :error="authError"
    :loading="loading"
    @submit="handleAuthSubmit"
    @toggle-mode="mode = mode === 'login' ? 'register' : 'login'"
    @update:name="authName = $event"
    @update:email="authEmail = $event"
    @update:password="authPassword = $event"
  />

  <AppShell v-else :session="session" @logout="handleLogout">
    <div class="grid gap-4 lg:grid-cols-[260px_minmax(330px,460px)_1fr] lg:gap-5">
      <Sidebar :session="session" :total-count="totalCount" />

      <section v-if="mobileTab === 'notes' || mobileTab === 'search'" class="space-y-4" :class="mobileEditorOpen ? 'hidden lg:block' : ''">
        <NoteList
          :notes="notes"
          :selected-note-id="selectedNoteId"
          :loading="loading"
          :page="page"
          :total-pages="totalPages"
          :total-count="totalCount"
          :search="search"
          :sort-by="sortBy"
          :sort-dir="sortDir"
          :page-size="pageSize"
          @select="handleSelectNote"
          @new="handleNewDraft"
          @prev="page = Math.max(1, page - 1)"
          @next="page = Math.min(Math.max(1, totalPages), page + 1)"
          @update:search="
            search = $event;
            resetPagingFiltersAndSearch();
          "
          @update:sort-by="
            sortBy = $event;
            resetPagingFiltersAndSearch();
          "
          @update:sort-dir="
            sortDir = $event;
            resetPagingFiltersAndSearch();
          "
          @update:page-size="
            pageSize = $event;
            resetPagingFiltersAndSearch();
          "
        />
      </section>

      <section v-if="mobileTab === 'tags'" class="rounded-2xl border border-app-border bg-app-panel p-5 shadow-soft lg:hidden">
        <h2 class="text-base font-semibold text-app-text">Tags</h2>
        <p class="mt-2 text-sm text-app-muted">Tag organization UI is ready. Data model can be plugged in later without API changes.</p>
      </section>

      <section v-if="mobileTab === 'settings'" class="rounded-2xl border border-app-border bg-app-panel p-5 shadow-soft lg:hidden">
        <h2 class="text-base font-semibold text-app-text">Settings</h2>
        <p class="mt-2 text-sm text-app-muted">Profile and app preferences screen placeholder for future settings.</p>
      </section>

      <section class="space-y-4" :class="mobileEditorOpen ? '' : 'hidden lg:block'">
        <EditorPanel
          :selected-note-id="selectedNoteId"
          :selected-note="selectedNote"
          :editor-title="editorTitle"
          :editor-content="editorContent"
          :loading="loading"
          :message="message"
          :mobile="true"
          @update:title="editorTitle = $event"
          @update:content="editorContent = $event"
          @save="selectedNoteId ? handleUpdateNote() : handleCreateNote()"
          @delete="handleDeleteNote"
          @close="handleMobileBackToList"
        />
      </section>
    </div>
    <FabButton @click="handleNewDraft" />
    <BottomNav :active-tab="mobileTab" @change="mobileTab = $event" />
  </AppShell>
</template>
