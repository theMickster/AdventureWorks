import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ENVIRONMENT } from '@adventureworks-web/shared/util';
import { HrApiService } from './hr-api.service';
import type { Employee } from '../models/employee.model';
import type { EmployeeCreate } from '../models/employee-create.model';
import type { EmployeeUpdate } from '../models/employee-update.model';
import type { EmployeeSearchBody } from '../models/employee-search.model';
import type {
  EmployeeHire,
  EmployeeTerminate,
  EmployeeRehire,
  EmployeeLifecycleStatus,
} from '../models/employee-lifecycle.model';
import type { JsonPatchOperation } from '../models/json-patch.model';
import type { SearchResult } from '@adventureworks-web/shared/data-access';

const mockEnvironment = {
  production: false,
  defaultLocale: 'en',
  api: {
    primary: { baseUrl: 'https://localhost:44369/api', name: 'Test API' },
  },
};

const BASE_URL = 'https://localhost:44369/api';

const mockEmployee: Employee = {
  id: 1,
  firstName: 'Ken',
  lastName: 'Sánchez',
  middleName: 'J',
  title: 'Mr.',
  suffix: null,
  jobTitle: 'Chief Executive Officer',
  maritalStatus: 'S',
  gender: 'M',
  salariedFlag: true,
  organizationLevel: 0,
  nationalIdNumber: '295847284',
  loginId: 'adventure-works\\ken0',
  birthDate: '1969-01-29',
  hireDate: '2009-01-14',
  currentFlag: true,
  vacationHours: 99,
  sickLeaveHours: 69,
  emailAddress: 'ken0@adventure-works.com',
  modifiedDate: '2024-01-01T00:00:00',
};

describe('HrApiService', () => {
  let service: HrApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ENVIRONMENT, useValue: mockEnvironment }],
    });
    service = TestBed.inject(HrApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be injectable', () => {
    expect(service).toBeTruthy();
  });

  describe('CRUD', () => {
    it('should GET a single employee by id', () => {
      service.getEmployee(1).subscribe((result) => {
        expect(result).toEqual(mockEmployee);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockEmployee);
    });

    it('should GET employees without params', () => {
      const mockData: SearchResult<Employee> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [mockEmployee],
      };

      service.getEmployees().subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should GET employees with params', () => {
      const mockData: SearchResult<Employee> = {
        pageNumber: 2,
        pageSize: 20,
        totalPages: 3,
        hasPreviousPage: true,
        hasNextPage: true,
        totalRecords: 50,
        results: [],
      };

      service.getEmployees({ pageNumber: 2, pageSize: 20, orderBy: 'lastName' }).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees?pageNumber=2&pageSize=20&orderBy=lastName`);
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });

    it('should POST to search employees with query params', () => {
      const body: EmployeeSearchBody = { lastName: 'Sánchez' };
      const mockData: SearchResult<Employee> = {
        pageNumber: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
        totalRecords: 1,
        results: [mockEmployee],
      };

      service.searchEmployees({ pageNumber: 1, pageSize: 10 }, body).subscribe((result) => {
        expect(result).toEqual(mockData);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/search?pageNumber=1&pageSize=10`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockData);
    });

    it('should POST to create an employee', () => {
      const body: EmployeeCreate = {
        firstName: 'Jane',
        lastName: 'Doe',
        jobTitle: 'Design Engineer',
        maritalStatus: 'S',
        gender: 'F',
        salariedFlag: true,
        nationalIdNumber: '123456789',
        loginId: 'adventure-works\\jane0',
        birthDate: '1990-01-15',
        phone: { phoneNumber: '555-0100', phoneNumberTypeId: 1 },
        emailAddress: 'jane0@adventure-works.com',
        address: {
          addressLine1: '123 Main St',
          city: 'Seattle',
          stateProvince: { id: 79, name: 'Washington', code: 'WA' },
          postalCode: '98101',
        },
        addressTypeId: 2,
      };

      service.createEmployee(body).subscribe((result) => {
        expect(result.id).toBe(300);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush({ ...mockEmployee, id: 300, firstName: 'Jane', lastName: 'Doe' });
    });

    it('should PUT to update an employee', () => {
      const body: EmployeeUpdate = {
        id: 1,
        firstName: 'Ken',
        lastName: 'Sánchez',
        maritalStatus: 'M',
        gender: 'M',
      };

      service.updateEmployee(1, body).subscribe((result) => {
        expect(result).toEqual(mockEmployee);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(body);
      req.flush(mockEmployee);
    });

    it('should PATCH an employee with JSON Patch operations', () => {
      const operations: JsonPatchOperation[] = [{ op: 'replace', path: '/jobTitle', value: 'Senior Engineer' }];

      service.patchEmployee(1, operations).subscribe((result) => {
        expect(result).toEqual(mockEmployee);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual(operations);
      req.flush(mockEmployee);
    });
  });

  describe('Lifecycle', () => {
    it('should POST to hire an employee', () => {
      const body: EmployeeHire = {
        employeeId: 1,
        hireDate: '2024-06-01',
        departmentId: 1,
        shiftId: 1,
        initialPayRate: 25.0,
        payFrequency: 2,
      };
      const mockResponse = { businessEntityId: 1, message: 'Employee hired successfully' };

      service.hireEmployee(1, body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1/lifecycle/hire`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should POST to terminate an employee', () => {
      const body: EmployeeTerminate = {
        employeeId: 1,
        terminationDate: '2024-12-31',
        reason: 'Resignation',
        terminationType: 'Voluntary',
        eligibleForRehire: true,
      };
      const mockResponse = { message: 'Employee terminated successfully' };

      service.terminateEmployee(1, body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1/lifecycle/terminate`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should POST to rehire an employee', () => {
      const body: EmployeeRehire = {
        employeeId: 1,
        rehireDate: '2025-03-01',
        departmentId: 2,
        shiftId: 1,
        payRate: 30.0,
        payFrequency: 2,
        restoreSeniority: true,
      };
      const mockResponse = { businessEntityId: 1, message: 'Employee rehired successfully' };

      service.rehireEmployee(1, body).subscribe((result) => {
        expect(result).toEqual(mockResponse);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1/lifecycle/rehire`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(body);
      req.flush(mockResponse);
    });

    it('should GET lifecycle status for an employee', () => {
      const mockStatus: EmployeeLifecycleStatus = {
        employeeId: 1,
        fullName: 'Ken J Sánchez',
        employmentStatus: 'Active',
        hireDate: '2009-01-14',
        terminationDate: null,
        daysEmployed: 5543,
        currentDepartment: 'Executive',
        currentShift: 'Day',
        departmentStartDate: '2009-01-14',
        currentPayRate: 125.5,
        payRateEffectiveDate: '2009-01-14',
        vacationHoursBalance: 99,
        sickLeaveHoursBalance: 69,
        eligibleForRehire: true,
        rehireCount: null,
      };

      service.getLifecycleStatus(1).subscribe((result) => {
        expect(result).toEqual(mockStatus);
      });

      const req = httpTesting.expectOne(`${BASE_URL}/v1/employees/1/lifecycle/status`);
      expect(req.request.method).toBe('GET');
      req.flush(mockStatus);
    });
  });
});
