import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrdersPage } from './orders-page';

describe('OrdersPage', () => {
  let component: OrdersPage;
  let fixture: ComponentFixture<OrdersPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrdersPage],
    }).compileComponents();

    fixture = TestBed.createComponent(OrdersPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
