import axios from "axios";
import type { Note, PagedResult, UserSession } from "./types";

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim() || "https://localhost:7287/api";

const api = axios.create({
  baseURL: apiBaseUrl
});

const refreshApi = axios.create({
  baseURL: apiBaseUrl
});

let currentSession: UserSession | null = null;
let onSessionUpdate: ((session: UserSession) => void) | null = null;
let onAuthFailure: (() => void) | null = null;
let refreshPromise: Promise<UserSession> | null = null;

type RetriableConfig = {
  _retry?: boolean;
  headers?: Record<string, string>;
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const status = error?.response?.status as number | undefined;
    const original = (error?.config ?? {}) as RetriableConfig;

    if (status !== 401 || original._retry || !currentSession?.refreshToken) {
      return Promise.reject(error);
    }

    original._retry = true;

    try {
      if (!refreshPromise) {
        refreshPromise = refreshSession(currentSession.refreshToken);
      }
      const refreshed = await refreshPromise;
      refreshPromise = null;
      setAuthContext(refreshed, onSessionUpdate, onAuthFailure);
      original.headers = original.headers ?? {};
      original.headers.Authorization = `Bearer ${refreshed.token}`;
      return api.request(original);
    } catch (refreshError) {
      refreshPromise = null;
      clearAuthContext();
      onAuthFailure?.();
      return Promise.reject(refreshError);
    }
  }
);

export function setAuthContext(
  session: UserSession | null,
  sessionUpdate: ((session: UserSession) => void) | null,
  authFailure: (() => void) | null
): void {
  currentSession = session;
  onSessionUpdate = sessionUpdate;
  onAuthFailure = authFailure;

  if (session?.token) {
    api.defaults.headers.common.Authorization = `Bearer ${session.token}`;
  } else {
    delete api.defaults.headers.common.Authorization;
  }
}

export function clearAuthContext(): void {
  setAuthContext(null, null, null);
}

export async function register(name: string, email: string, password: string): Promise<UserSession> {
  const { data } = await api.post<UserSession>("/auth/register", { name, email, password });
  return data;
}

export async function login(email: string, password: string): Promise<UserSession> {
  const { data } = await api.post<UserSession>("/auth/login", { email, password });
  return data;
}

export async function refreshSession(refreshToken: string): Promise<UserSession> {
  const { data } = await refreshApi.post<UserSession>("/auth/refresh", { refreshToken });
  return data;
}

export async function logout(): Promise<void> {
  await api.post("/auth/logout");
}

export async function getNotes(
  search: string,
  sortBy: string,
  sortDir: string,
  page: number,
  pageSize: number
): Promise<PagedResult<Note>> {
  const { data } = await api.get<PagedResult<Note>>("/notes", {
    params: { search: search || undefined, sortBy, sortDir, page, pageSize }
  });
  return data;
}

export async function getNote(id: number): Promise<Note> {
  const { data } = await api.get<Note>(`/notes/${id}`);
  return data;
}

export async function createNote(title: string, content: string): Promise<Note> {
  const { data } = await api.post<Note>("/notes", { title, content });
  return data;
}

export async function updateNote(id: number, title: string, content: string): Promise<Note> {
  const { data } = await api.put<Note>(`/notes/${id}`, { title, content });
  return data;
}

export async function deleteNote(id: number): Promise<void> {
  await api.delete(`/notes/${id}`);
}
