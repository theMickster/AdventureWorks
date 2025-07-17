import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:5001/api', name: 'AdventureWorks API' },
  },
};
