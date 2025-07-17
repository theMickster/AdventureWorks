import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: true,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: '/api', name: 'AdventureWorks API' },
  },
};
