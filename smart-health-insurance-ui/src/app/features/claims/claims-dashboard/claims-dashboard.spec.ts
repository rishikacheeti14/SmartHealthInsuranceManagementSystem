import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClaimsDashboardComponent } from './claims-dashboard';

describe('ClaimsDashboardComponent', () => {
  let component: ClaimsDashboardComponent;
  let fixture: ComponentFixture<ClaimsDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClaimsDashboardComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(ClaimsDashboardComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
