import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditSearchComponent } from './audit-search.component';

describe('AuditSearchComponent', () => {
  let component: AuditSearchComponent;
  let fixture: ComponentFixture<AuditSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AuditSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AuditSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
