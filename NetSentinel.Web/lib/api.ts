const BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5149";

function getToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("ns_token");
}

async function request<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string> ?? {}),
  };

  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${BASE}${path}`, {
    ...options,
    headers,
  });

  const text = await res.text();

  let data;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!res.ok) {
    throw new Error(
      (data && data.message) ||
      (data && data.error) ||
      `HTTP ${res.status}`
    );
  }

  return data;
}

// POST /api/auth/login
export async function apiLogin(username: string, password: string) {
  return request<{ token: string; message: string }>("/api/auth/login", {
    method: "POST",
    body: JSON.stringify({ username, password }),
  });
}

// POST /api/auth/users
export async function apiRegisterUser(data: {
  name: string;
  username: string;
  email: string;
  password: string;
  department: string;
  roleId: number;
}) {
  return request<{ message: string }>("/api/auth/users", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

// GET /api/manager/device/list
export async function apiGetDevices() {
  return request<import("@/types").Device[]>("/api/manager/device/list");
}