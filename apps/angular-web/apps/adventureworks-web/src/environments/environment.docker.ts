import { Environment } from '@adventureworks-web/shared/util';

export const environment: Environment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: '/api', name: 'AdventureWorks API' },
  },
};
