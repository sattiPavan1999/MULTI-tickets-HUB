import { gql } from '@apollo/client';

export const GET_ADMIN_MOVIES = gql`
  query GetAdminMovies {
    movies {
      id
      title
      genre
      duration
      posterUrl
      isActive
      createdAt
    }
  }
`;

export const GET_ADMIN_TRAINS = gql`
  query GetAdminTrains {
    trains {
      id
      trainName
      trainNumber
      source
      destination
      departureTime
      arrivalTime
      price
      createdAt
    }
  }
`;

export const GET_ADMIN_USERS = gql`
  query GetAdminUsers {
    users {
      id
      email
      fullName
      role
      isActive
      createdAt
    }
  }
`;
