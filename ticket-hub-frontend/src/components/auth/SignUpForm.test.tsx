import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Routes, Route } from 'react-router-dom';
import { SignUpForm } from '@/components/auth/SignUpForm';
import { TestRouter } from '@/test/utils';

const mockRegister = vi.hoisted(() => vi.fn());
const mockToastSuccess = vi.hoisted(() => vi.fn());
const mockToastError = vi.hoisted(() => vi.fn());

vi.mock('@/hooks/useAuth', () => ({
  useAuth: () => ({ register: mockRegister }),
}));

vi.mock('@/hooks/useToast', () => ({
  useToast: () => ({ success: mockToastSuccess, error: mockToastError }),
}));

function renderSignUpForm() {
  return render(
    <TestRouter initialEntries={['/auth']}>
      <Routes>
        <Route path="/auth" element={<SignUpForm />} />
        <Route path="/dashboard" element={<div>Dashboard page</div>} />
      </Routes>
    </TestRouter>
  );
}

describe('SignUpForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders all form fields', () => {
    renderSignUpForm();
    expect(screen.getByLabelText(/full name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/phone number/i)).toBeInTheDocument();
    expect(screen.getByLabelText('Password')).toBeInTheDocument();
    expect(screen.getByLabelText(/confirm password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /create account/i })).toBeInTheDocument();
  });

  it('shows validation errors when submitted with empty fields', async () => {
    const user = userEvent.setup();
    renderSignUpForm();
    await user.click(screen.getByRole('button', { name: /create account/i }));

    expect(await screen.findByText('Full name is required')).toBeInTheDocument();
    expect(screen.getByText('Email is required')).toBeInTheDocument();
    expect(screen.getByText('Phone number is required')).toBeInTheDocument();
    expect(screen.getByText('Password is required')).toBeInTheDocument();
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it('shows confirm password mismatch error', async () => {
    const user = userEvent.setup();
    renderSignUpForm();

    await user.type(screen.getByLabelText(/full name/i), 'John Doe');
    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.type(screen.getByLabelText(/phone number/i), '+1234567890');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.type(screen.getByLabelText(/confirm password/i), 'different123');
    await user.click(screen.getByRole('button', { name: /create account/i }));

    expect(await screen.findByText('Passwords do not match')).toBeInTheDocument();
    expect(mockRegister).not.toHaveBeenCalled();
  });

  it('calls register and navigates to dashboard on success', async () => {
    const user = userEvent.setup();
    const mockUser = {
      id: 1,
      email: 'john@example.com',
      fullName: 'John Doe',
      phoneNumber: '+1234567890',
      role: 'User',
      createdAt: '2024-01-01T00:00:00Z',
    };
    mockRegister.mockResolvedValue(mockUser);
    renderSignUpForm();

    await user.type(screen.getByLabelText(/full name/i), 'John Doe');
    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.type(screen.getByLabelText(/phone number/i), '+1234567890');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.type(screen.getByLabelText(/confirm password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /create account/i }));

    expect(mockRegister).toHaveBeenCalledWith({
      email: 'john@example.com',
      password: 'password123',
      fullName: 'John Doe',
      phoneNumber: '+1234567890',
    });
    expect(await screen.findByText('Dashboard page')).toBeInTheDocument();
    expect(mockToastSuccess).toHaveBeenCalledWith('Your account is ready. Redirecting…', 'Account created');
  });

  it('shows error toast when register fails', async () => {
    const user = userEvent.setup();
    mockRegister.mockRejectedValue(new Error('Email already registered'));
    renderSignUpForm();

    await user.type(screen.getByLabelText(/full name/i), 'John Doe');
    await user.type(screen.getByLabelText(/email/i), 'john@example.com');
    await user.type(screen.getByLabelText(/phone number/i), '+1234567890');
    await user.type(screen.getByLabelText('Password'), 'password123');
    await user.type(screen.getByLabelText(/confirm password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /create account/i }));

    expect(await screen.findByRole('button', { name: /create account/i })).not.toBeDisabled();
    expect(mockToastError).toHaveBeenCalledWith(
      'Unable to create account. Please try again.',
      'Sign up failed'
    );
  });
});
