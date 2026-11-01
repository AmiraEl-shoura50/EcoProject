import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellerOrders } from './seller-orders';

describe('SellerOrders', () => {
  let component: SellerOrders;
  let fixture: ComponentFixture<SellerOrders>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SellerOrders],
    }).compileComponents();

    fixture = TestBed.createComponent(SellerOrders);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
