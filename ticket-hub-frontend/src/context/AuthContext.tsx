import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import type { LoginRequest, RegisterRequest, UpdateProfileRequest, User } from '@/types/auth';
import { authApi } from '@/services/api/authApi';
import { tokenStorage, userStorage } from '@/utils/storage';

interface AuthContextValue {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isInitializing: boolean;
  login: (input: LoginRequest) => Promise<User>;
  register: (input: RegisterRequest) => Promise<User>;
  updateProfile: (input: UpdateProfileRequest) => Promise<User>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    const storedToken = tokenStorage.get();
    const storedUser = userStorage.get();
    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(storedUser);
    }
    setIsInitializing(false);
  }, []);

  const login = useCallback(async (input: LoginRequest): Promise<User> => {
    const result = await authApi.login(input);
    tokenStorage.set(result.token);
    userStorage.set(result.user);
    setToken(result.token);
    setUser(result.user);
    return result.user;
  }, []);

  const register = useCallback(async (input: RegisterRequest): Promise<User> => {
    const created = await authApi.register(input);
    // Backend returns the user but no token on register — sign in immediately.
    try {
      const result = await authApi.login({ email: input.email, password: input.password });
      tokenStorage.set(result.token);
      userStorage.set(result.user);
      setToken(result.token);
      setUser(result.user);
    } catch {
      // Registration succeeded but auto-login failed; user must sign in manually.
    }
    return created;
  }, []);

  const updateProfile = useCallback(async (input: UpdateProfileRequest): Promise<User> => {
    const updated = await authApi.updateProfile(input);
    userStorage.set(updated);
    setUser(updated);
    return updated;
  }, []);

  const logout = useCallback(() => {
    tokenStorage.clear();
    userStorage.clear();
    setToken(null);
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      isAuthenticated: !!token && !!user,
      isInitializing,
      login,
      register,
      updateProfile,
      logout,
    }),
    [user, token, isInitializing, login, register, updateProfile, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
