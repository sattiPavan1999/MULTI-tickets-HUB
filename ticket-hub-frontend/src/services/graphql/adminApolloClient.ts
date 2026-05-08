import { ApolloClient, InMemoryCache, createHttpLink, from } from '@apollo/client';
import { setContext } from '@apollo/client/link/context';
import { tokenStorage } from '@/utils/storage';

const httpLink = createHttpLink({
  uri: `${import.meta.env.VITE_API_URL ?? 'http://localhost:5000'}/graphql/admin`,
});

const authLink = setContext((_, { headers }) => {
  const token = tokenStorage.get();
  return {
    headers: {
      ...headers,
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  };
});

export const adminApolloClient = new ApolloClient({
  link: from([authLink, httpLink]),
  cache: new InMemoryCache(),
  defaultOptions: {
    watchQuery: { fetchPolicy: 'network-only' },
  },
});
