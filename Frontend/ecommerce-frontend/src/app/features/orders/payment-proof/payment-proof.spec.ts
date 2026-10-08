import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PaymentProof } from './payment-proof';

describe('PaymentProof', () => {
  let component: PaymentProof;
  let fixture: ComponentFixture<PaymentProof>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PaymentProof],
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentProof);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
