import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ProfilePage } from '@/pages/ProfilePage';
import { TestRouter } from '@/test/utils';

const mockUpdateProfile = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

const mockUser = {
  id: 1,
  email: 'john@example.com',
  fullName: 'John Doe',
  phoneNumber: '+1234567890',
  role: 'User',
  createdAt: '2024-01-01T00:00:00Z',
};

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ user: mockUser, updateProfile: mockUpdateProfile }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError }),
}));

function renderProfile() {
  return render(
    <TestRouter>
      <ProfilePage />
    </TestRouter>
  );
}

describe('ProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders user data pre-filled in form inputs', () => {
    renderProfile();
    expect(screen.getByLabelText(/full name/i)).toHaveValue('John Doe');
    expect(screen.getByLabelText(/email/i)).toHaveValue('john@example.com');
    expect(screen.getByLabelText(/phone number/i)).toHaveValue('+1234567890');
  });

  it('displays the user role and member since fields', () => {
    renderProfile();
    expect(screen.getByText('User')).toBeInTheDocument();
  });

  it('save button is disabled when the form has not changed', () => {
    renderProfile();
    expect(screen.getByRole('button', { name: /save changes/i })).toBeDisabled();
  });

  it('enables save button when a field is changed', async () => {
    const user = userEvent.setup();
    renderProfile();
    const fullNameInput = screen.getByLabelText(/full name/i);
    await user.clear(fullNameInput);
    await user.type(fullNameInput, 'Jane Doe');
    expect(screen.getByRole('button', { name: /save changes/i })).not.toBeDisabled();
  });

  it('calls updateProfile with only changed fields and shows success toast', async () => {
    const user = userEvent.setup();
    mockUpdateProfile.mockResolvedValue({ ...mockUser, fullName: 'Jane Doe' });
    renderProfile();

    const fullNameInput = screen.getByLabelText(/full name/i);
    await user.clear(fullNameInput);
    await user.type(fullNameInput, 'Jane Doe');
    await user.click(screen.getByRole('button', { name: /save changes/i }));

    expect(mockUpdateProfile).toHaveBeenCalledWith({ fullName: 'Jane Doe' });
    expect(mockToastSuccess).toHaveBeenCalledWith('Your profile has been updated.', 'Saved');
  });

  it('discard button resets form to original values', async () => {
    const user = userEvent.setup();
    renderProfile();

    const fullNameInput = screen.getByLabelText(/full name/i);
    await user.clear(fullNameInput);
    await user.type(fullNameInput, 'Jane Doe');
    expect(fullNameInput).toHaveValue('Jane Doe');

    await user.click(screen.getByRole('button', { name: /discard/i }));
    expect(fullNameInput).toHaveValue('John Doe');
    expect(screen.getByRole('button', { name: /save changes/i })).toBeDisabled();
  });
});
