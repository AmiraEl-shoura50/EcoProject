import { TestBed } from '@angular/core/testing';

import { PaymentGroup } from './payment-group';

describe('PaymentGroup', () => {
  let service: PaymentGroup;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PaymentGroup);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
