import { TestBed } from '@angular/core/testing';

import { AdminMessage } from './admin-message';

describe('AdminMessage', () => {
  let service: AdminMessage;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminMessage);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
