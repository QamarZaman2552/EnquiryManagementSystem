import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Enquires } from './enquires';

describe('Enquires', () => {
  let component: Enquires;
  let fixture: ComponentFixture<Enquires>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Enquires],
    }).compileComponents();

    fixture = TestBed.createComponent(Enquires);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
