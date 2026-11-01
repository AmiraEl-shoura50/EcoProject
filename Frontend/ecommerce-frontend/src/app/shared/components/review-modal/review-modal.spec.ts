import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReviewModal } from './review-modal';

describe('ReviewModal', () => {
  let component: ReviewModal;
  let fixture: ComponentFixture<ReviewModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReviewModal],
    }).compileComponents();

    fixture = TestBed.createComponent(ReviewModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
