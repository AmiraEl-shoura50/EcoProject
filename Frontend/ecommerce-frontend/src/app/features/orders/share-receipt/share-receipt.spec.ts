import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShareReceipt } from './share-receipt';

describe('ShareReceipt', () => {
  let component: ShareReceipt;
  let fixture: ComponentFixture<ShareReceipt>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShareReceipt],
    }).compileComponents();

    fixture = TestBed.createComponent(ShareReceipt);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
