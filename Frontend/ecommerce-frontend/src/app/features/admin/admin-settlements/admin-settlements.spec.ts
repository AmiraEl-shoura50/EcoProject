import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminSettlements } from './admin-settlements';

describe('AdminSettlements', () => {
  let component: AdminSettlements;
  let fixture: ComponentFixture<AdminSettlements>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminSettlements],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminSettlements);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
