import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminPaymentMethods } from './admin-payment-methods';

describe('AdminPaymentMethods', () => {
  let component: AdminPaymentMethods;
  let fixture: ComponentFixture<AdminPaymentMethods>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminPaymentMethods],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPaymentMethods);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
