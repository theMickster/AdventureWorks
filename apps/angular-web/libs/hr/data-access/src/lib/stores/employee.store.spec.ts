import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import type { SearchResult } from '@adventureworks-web/shared/data-access';
import type { Employee } from '../models/employee.model';
import { EmployeeStore } from './employee.store';

const mockEnvironment = {
  production: false,
  api: {
    primary: { baseUrl: 'https://api.test.com', name: 'Test API' },
  },
};

const mockEmployee: Employee = {
  id: 1,
  firstName: 'John',
  lastName: 'Doe',
  middleName: null,
  title: null,
  suffix: null,
  jobTitle: 'Software Engineer',
  maritalStatus: 'S',
  gender: 'M',
  salariedFlag: true,
  organizationLevel: 2,
  nationalIdNumber: '123456789',
  loginId: 'adventure-works\\john',
  birthDate: '1990-01-01',
  hireDate: '2020-01-01',
  currentFlag: true,
  vacationHours: 40,
  sickLeaveHours: 24,
  emailAddress: 'john.doe@example.com',
  modifiedDate: '2026-01-01T00:00:00',
};

const mockSearchResult: SearchResult<Employee> = {
  pageNumber: 1,
  pageSize: 10,
  totalPages: 1,
  totalRecords: 1,
  hasPreviousPage: false,
  hasNextPage: false,
  results: [mockEmployee],
};

describe('EmployeeStore', () => {
  let store: InstanceType<typeof EmployeeStore>;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        EmployeeStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: ENVIRONMENT, useValue: mockEnvironment },
      ],
    });
    store = TestBed.inject(EmployeeStore);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should have idle initial state', () => {
    expect(store.entities()).toEqual([]);
    expect(store.requestStatus()).toBe('idle');
    expect(store.totalRecords()).toBe(0);
    expect(store.pageNumber()).toBe(1);
  });

  describe('loadPage', () => {
    it('should load entities and set pagination', () => {
      store.loadPage({ pageNumber: 1, pageSize: 10 });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/employees?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('GET');
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockEmployee]);
      expect(store.isLoaded()).toBe(true);
      expect(store.totalRecords()).toBe(1);
    });

    it('should handle empty results without error', () => {
      const emptyResult: SearchResult<Employee> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: [],
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/employees?pageNumber=1&pageSize=10');
      req.flush(emptyResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });

    it('should handle null results without error', () => {
      const nullResult: SearchResult<Employee> = {
        ...mockSearchResult,
        totalRecords: 0,
        totalPages: 0,
        results: null,
      };

      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/employees?pageNumber=1&pageSize=10');
      req.flush(nullResult);

      expect(store.entities()).toEqual([]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error state on failure', () => {
      store.loadPage({ pageNumber: 1, pageSize: 10 });
      const req = httpTesting.expectOne('https://api.test.com/v1/employees?pageNumber=1&pageSize=10');
      req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

      expect(store.hasError()).toBe(true);
      expect(store.error()).toBeTruthy();
    });
  });

  describe('search', () => {
    it('should search entities and set pagination', () => {
      store.search({ params: { pageNumber: 1, pageSize: 10 }, body: { firstName: 'John' } });

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/employees/search?pageNumber=1&pageSize=10');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ firstName: 'John' });
      req.flush(mockSearchResult);

      expect(store.entities()).toEqual([mockEmployee]);
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('loadById', () => {
    it('should load a single entity', () => {
      store.loadById(1);

      expect(store.isLoading()).toBe(true);

      const req = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(req.request.method).toBe('GET');
      req.flush(mockEmployee);

      expect(store.entities()).toEqual([mockEmployee]);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on failure', () => {
      store.loadById(999);
      const req = httpTesting.expectOne('https://api.test.com/v1/employees/999');
      req.flush('Not Found', { status: 404, statusText: 'Not Found' });

      expect(store.hasError()).toBe(true);
    });
  });

  describe('create', () => {
    it('should add created entity to store', () => {
      const createModel = {
        firstName: 'Jane',
        lastName: 'Smith',
        jobTitle: 'Designer',
        maritalStatus: 'S' as const,
        gender: 'F' as const,
        salariedFlag: true,
        nationalIdNumber: '987654321',
        loginId: 'adventure-works\\jane',
        birthDate: '1992-05-15',
        phone: { phoneNumber: '555-0200', phoneNumberTypeId: 1 },
        emailAddress: 'jane.smith@example.com',
        address: {
          addressLine1: '456 Oak Ave',
          city: 'Portland',
          stateProvince: { id: 2, name: 'Oregon', code: 'OR' },
          postalCode: '97201',
        },
        addressTypeId: 1,
      };

      store.create(createModel);

      const req = httpTesting.expectOne('https://api.test.com/v1/employees');
      expect(req.request.method).toBe('POST');
      req.flush({ ...mockEmployee, id: 2, firstName: 'Jane', lastName: 'Smith' });

      expect(store.entities().length).toBe(1);
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('update', () => {
    it('should update existing entity in store', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      // Then update
      const updateModel = {
        id: 1,
        firstName: 'John',
        lastName: 'Updated',
        maritalStatus: 'M' as const,
        gender: 'M' as const,
      };
      store.update({ id: 1, model: updateModel });

      const updateReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(updateReq.request.method).toBe('PUT');
      updateReq.flush({ ...mockEmployee, lastName: 'Updated', maritalStatus: 'M' });

      expect(store.entities()[0].lastName).toBe('Updated');
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('patch', () => {
    it('should patch existing entity in store', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      // Then patch
      const operations = [{ op: 'replace' as const, path: '/jobTitle', value: 'Senior Engineer' }];
      store.patch({ id: 1, operations });

      const patchReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(patchReq.request.method).toBe('PATCH');
      patchReq.flush({ ...mockEmployee, jobTitle: 'Senior Engineer' });

      expect(store.entities()[0].jobTitle).toBe('Senior Engineer');
      expect(store.isLoaded()).toBe(true);
    });
  });

  describe('hireEmployee', () => {
    it('should call lifecycle hire then reload the entity', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      // Hire
      const hireModel = {
        employeeId: 1,
        hireDate: '2026-03-01',
        departmentId: 1,
        shiftId: 1,
        initialPayRate: 25.0,
        payFrequency: 2,
      };
      store.hireEmployee({ id: 1, model: hireModel });

      // First: lifecycle POST
      const hireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/hire');
      expect(hireReq.request.method).toBe('POST');
      hireReq.flush({ businessEntityId: 1, message: 'Hired' });

      // Second: reload GET
      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(reloadReq.request.method).toBe('GET');
      reloadReq.flush({ ...mockEmployee, hireDate: '2026-03-01' });

      expect(store.entities()[0].hireDate).toBe('2026-03-01');
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on failure', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      store.hireEmployee({
        id: 1,
        model: {
          employeeId: 1,
          hireDate: '2026-03-01',
          departmentId: 1,
          shiftId: 1,
          initialPayRate: 25.0,
          payFrequency: 2,
        },
      });

      const hireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/hire');
      hireReq.flush('Bad Request', { status: 400, statusText: 'Bad Request' });

      expect(store.hasError()).toBe(true);
    });

    it('should set loaded state when POST succeeds but GET re-fetch fails', () => {
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      store.hireEmployee({
        id: 1,
        model: {
          employeeId: 1,
          hireDate: '2026-03-01',
          departmentId: 1,
          shiftId: 1,
          initialPayRate: 25.0,
          payFrequency: 2,
        },
      });

      const hireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/hire');
      hireReq.flush({ businessEntityId: 1, message: 'Hired' });

      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      reloadReq.flush('error', { status: 500, statusText: 'Server Error' });

      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });
  });

  describe('terminateEmployee', () => {
    it('should call lifecycle terminate then reload the entity', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      const terminateModel = {
        employeeId: 1,
        terminationDate: '2026-03-15',
        reason: 'Voluntary resignation',
        terminationType: 'Voluntary' as const,
        eligibleForRehire: true,
      };
      store.terminateEmployee({ id: 1, model: terminateModel });

      const terminateReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/terminate');
      expect(terminateReq.request.method).toBe('POST');
      terminateReq.flush({ message: 'Terminated' });

      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(reloadReq.request.method).toBe('GET');
      reloadReq.flush({ ...mockEmployee, currentFlag: false });

      expect(store.entities()[0].currentFlag).toBe(false);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on POST failure', () => {
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      store.terminateEmployee({
        id: 1,
        model: {
          employeeId: 1,
          terminationDate: '2026-03-15',
          reason: 'Voluntary resignation',
          terminationType: 'Voluntary' as const,
          eligibleForRehire: true,
        },
      });

      const terminateReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/terminate');
      terminateReq.flush('error', { status: 500, statusText: 'Server Error' });

      expect(store.hasError()).toBe(true);
    });

    it('should set loaded state when POST succeeds but GET re-fetch fails', () => {
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush(mockEmployee);

      store.terminateEmployee({
        id: 1,
        model: {
          employeeId: 1,
          terminationDate: '2026-03-15',
          reason: 'Voluntary resignation',
          terminationType: 'Voluntary' as const,
          eligibleForRehire: true,
        },
      });

      const terminateReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/terminate');
      terminateReq.flush({ message: 'Terminated' });

      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      reloadReq.flush('error', { status: 500, statusText: 'Server Error' });

      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });
  });

  describe('rehireEmployee', () => {
    it('should call lifecycle rehire then reload the entity', () => {
      // First load the entity
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush({ ...mockEmployee, currentFlag: false });

      const rehireModel = {
        employeeId: 1,
        rehireDate: '2026-04-01',
        departmentId: 2,
        shiftId: 1,
        payRate: 30.0,
        payFrequency: 2,
        restoreSeniority: true,
      };
      store.rehireEmployee({ id: 1, model: rehireModel });

      const rehireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/rehire');
      expect(rehireReq.request.method).toBe('POST');
      rehireReq.flush({ businessEntityId: 1, message: 'Rehired' });

      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      expect(reloadReq.request.method).toBe('GET');
      reloadReq.flush({ ...mockEmployee, currentFlag: true, hireDate: '2026-04-01' });

      expect(store.entities()[0].currentFlag).toBe(true);
      expect(store.isLoaded()).toBe(true);
    });

    it('should set error on POST failure', () => {
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush({ ...mockEmployee, currentFlag: false });

      store.rehireEmployee({
        id: 1,
        model: {
          employeeId: 1,
          rehireDate: '2026-04-01',
          departmentId: 2,
          shiftId: 1,
          payRate: 30.0,
          payFrequency: 2,
          restoreSeniority: true,
        },
      });

      const rehireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/rehire');
      rehireReq.flush('error', { status: 500, statusText: 'Server Error' });

      expect(store.hasError()).toBe(true);
    });

    it('should set loaded state when POST succeeds but GET re-fetch fails', () => {
      store.loadById(1);
      const loadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      loadReq.flush({ ...mockEmployee, currentFlag: false });

      store.rehireEmployee({
        id: 1,
        model: {
          employeeId: 1,
          rehireDate: '2026-04-01',
          departmentId: 2,
          shiftId: 1,
          payRate: 30.0,
          payFrequency: 2,
          restoreSeniority: true,
        },
      });

      const rehireReq = httpTesting.expectOne('https://api.test.com/v1/employees/1/lifecycle/rehire');
      rehireReq.flush({ businessEntityId: 1, message: 'Rehired' });

      const reloadReq = httpTesting.expectOne('https://api.test.com/v1/employees/1');
      reloadReq.flush('error', { status: 500, statusText: 'Server Error' });

      expect(store.isLoaded()).toBe(true);
      expect(store.hasError()).toBe(false);
    });
  });
});
