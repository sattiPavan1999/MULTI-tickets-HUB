import { ApolloProvider } from '@apollo/client/react';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '@/context/AuthContext';
import { ToastProvider } from '@/context/ToastContext';
import { ToastViewport } from '@/components/ui/Toast';
import { ErrorBoundary } from '@/components/ErrorBoundary';
import { AppRoutes } from '@/routes/AppRoutes';
import { apolloClient } from '@/services/graphql/apolloClient';

export function App() {
  return (
    <ApolloProvider client={apolloClient}>
      <ErrorBoundary>
        <ToastProvider>
          <AuthProvider>
            <BrowserRouter>
              <AppRoutes />
              <ToastViewport />
            </BrowserRouter>
          </AuthProvider>
        </ToastProvider>
      </ErrorBoundary>
    </ApolloProvider>
  );
}
