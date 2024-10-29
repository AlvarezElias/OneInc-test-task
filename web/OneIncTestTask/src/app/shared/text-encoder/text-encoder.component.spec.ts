import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TextEncoderComponent } from './text-encoder.component';

describe('TextEncoderComponent', () => {
  let component: TextEncoderComponent;
  let fixture: ComponentFixture<TextEncoderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TextEncoderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TextEncoderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
