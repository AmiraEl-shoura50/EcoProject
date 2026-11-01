import { TestBed } from '@angular/core/testing';

import { Discount } from './discount';

describe('Discount', () => {
  let service: Discount;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Discount);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
