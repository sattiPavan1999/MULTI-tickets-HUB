import { useState, useMemo } from 'react';
import { ApolloProvider, useQuery } from '@apollo/client/react';
import { adminApolloClient } from '@/services/graphql/adminApolloClient';
import { GET_ADMIN_USERS } from '@/services/graphql/adminQueries';
import { adminApi } from '@/services/api/adminApi';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Spinner } from '@/components/ui/Spinner';
import { useToast } from '@/hooks/useToast';
import { useAuth } from '@/hooks/useAuth';

interface UserRow {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
  createdAt?: string | null;
}

function AdminUsersContent() {
  const { data, loading, refetch } = useQuery<{ users: UserRow[] }>(GET_ADMIN_USERS);
  const { user: currentUser } = useAuth();
  const toast = useToast();
  const [search, setSearch] = useState('');
  const [confirm, setConfirm] = useState<null | { userId: number; email: string; isActive: boolean }>(null);

  const users: UserRow[] = (data?.users ?? []).filter((u) => u.id !== currentUser?.id);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return users;
    return users.filter(
      (u) => u.email.toLowerCase().includes(q) || String(u.id).includes(q) || u.fullName.toLowerCase().includes(q)
    );
  }, [users, search]);

  const handleToggle = async () => {
    if (!confirm) return;
    try {
      await adminApi.toggleUserStatus(confirm.userId);
      await refetch();
      toast.success(`User ${confirm.isActive ? 'deactivated' : 'activated'} successfully`);
    } catch {
      toast.error('Failed to update user status');
    } finally {
      setConfirm(null);
    }
  };

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 py-10">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h1 className="font-serif text-3xl text-white">User Moderation</h1>
          <p className="mt-1 text-sm text-white/50">{users.length} users</p>
        </div>
        <div className="w-full sm:w-72">
          <Input
            placeholder="Search by email, name or ID…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center py-20"><Spinner size="lg" /></div>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-white/10">
          <table className="w-full text-sm text-white">
            <thead className="bg-ink-800/80 text-white/50 text-xs uppercase tracking-widest">
              <tr>
                <th className="px-4 py-3 text-left">ID</th>
                <th className="px-4 py-3 text-left">Email</th>
                <th className="px-4 py-3 text-left">Name</th>
                <th className="px-4 py-3 text-left">Role</th>
                <th className="px-4 py-3 text-left">Status</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {filtered.map((u) => (
                <tr key={u.id} className="hover:bg-white/[0.02]">
                  <td className="px-4 py-3 text-white/40 font-mono">#{u.id}</td>
                  <td className="px-4 py-3">{u.email}</td>
                  <td className="px-4 py-3 text-white/60">{u.fullName}</td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${u.role === 'Admin' ? 'bg-accent-500/10 text-accent-300' : 'bg-white/5 text-white/50'}`}>
                      {u.role}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <span className={`rounded-full px-2 py-0.5 text-[11px] font-semibold ${u.isActive ? 'bg-teal-400/10 text-teal-200' : 'bg-white/5 text-white/30'}`}>
                      {u.isActive ? 'Active' : 'Deactivated'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => setConfirm({ userId: u.id, email: u.email, isActive: u.isActive })}
                    >
                      {u.isActive ? 'Deactivate' : 'Activate'}
                    </Button>
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr><td colSpan={6} className="px-4 py-10 text-center text-white/30">No users found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}

      {confirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="w-full max-w-sm rounded-2xl border border-white/10 bg-ink-800 p-8 shadow-2xl">
            <h2 className="mb-2 font-serif text-xl text-white">Confirm Action</h2>
            <p className="mb-6 text-sm text-white/60">
              {confirm.isActive ? 'Deactivate' : 'Activate'} account <span className="text-white">{confirm.email}</span>?
              {confirm.isActive && ' They will not be able to log in.'}
            </p>
            <div className="flex gap-3">
              <Button onClick={handleToggle}>{confirm.isActive ? 'Deactivate' : 'Activate'}</Button>
              <Button variant="secondary" onClick={() => setConfirm(null)}>Cancel</Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export function AdminUsersPage() {
  return (
    <ApolloProvider client={adminApolloClient}>
      <AdminUsersContent />
    </ApolloProvider>
  );
}
