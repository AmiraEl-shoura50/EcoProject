import { TestBed } from '@angular/core/testing';

import { AdminProduct } from './admin-product';

describe('AdminProduct', () => {
  let service: AdminProduct;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminProduct);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
