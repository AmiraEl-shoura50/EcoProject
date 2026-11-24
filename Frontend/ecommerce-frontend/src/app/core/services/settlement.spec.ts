import { TestBed } from '@angular/core/testing';

import { Settlement } from './settlement';

describe('Settlement', () => {
  let service: Settlement;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Settlement);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
