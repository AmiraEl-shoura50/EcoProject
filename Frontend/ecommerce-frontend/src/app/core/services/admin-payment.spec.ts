import { TestBed } from '@angular/core/testing';

import { AdminPayment } from './admin-payment';

describe('AdminPayment', () => {
  let service: AdminPayment;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminPayment);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
