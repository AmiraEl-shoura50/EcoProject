import { TestBed } from '@angular/core/testing';

import { PwaInstall } from './pwa-install';

describe('PwaInstall', () => {
  let service: PwaInstall;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PwaInstall);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
