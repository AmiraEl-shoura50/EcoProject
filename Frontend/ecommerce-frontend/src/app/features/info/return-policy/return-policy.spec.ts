import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReturnPolicy } from './return-policy';

describe('ReturnPolicy', () => {
  let component: ReturnPolicy;
  let fixture: ComponentFixture<ReturnPolicy>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReturnPolicy],
    }).compileComponents();

    fixture = TestBed.createComponent(ReturnPolicy);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
