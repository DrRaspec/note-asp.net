export type UserSession = {
  userId: number;
  name: string;
  email: string;
  token: string;
  refreshToken: string;
};

export type Note = {
  id: number;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
};
