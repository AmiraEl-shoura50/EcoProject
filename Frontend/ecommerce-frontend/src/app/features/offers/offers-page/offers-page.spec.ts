import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OffersPage } from './offers-page';

describe('OffersPage', () => {
  let component: OffersPage;
  let fixture: ComponentFixture<OffersPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OffersPage],
    }).compileComponents();

    fixture = TestBed.createComponent(OffersPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
