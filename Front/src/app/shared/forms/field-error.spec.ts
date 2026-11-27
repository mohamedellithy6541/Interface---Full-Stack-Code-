import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormControl, Validators } from '@angular/forms';

import { FieldError } from './field-error';

@Component({
  imports: [FieldError],
  template: `<app-field-error [control]="control" label="Full name" />`,
})
class Host {
  control = new FormControl('', Validators.required);
}

describe('FieldError', () => {
  async function render() {
    const fixture = TestBed.createComponent(Host);
    await fixture.whenStable();
    return fixture;
  }

  function errorText(element: HTMLElement): string | null {
    return element.querySelector('.field-error')?.textContent?.trim() ?? null;
  }

  it('stays hidden until the user touches the field', async () => {
    const fixture = await render();

    expect(errorText(fixture.nativeElement)).toBeNull();
  });

  it('shows the validation message once the field is touched', async () => {
    const fixture = await render();

    fixture.componentInstance.control.markAsTouched();
    fixture.changeDetectorRef.markForCheck();
    await fixture.whenStable();

    expect(errorText(fixture.nativeElement)).toBe('Full name is required.');
  });

  it('prefers the message sent by the server', async () => {
    const fixture = await render();

    fixture.componentInstance.control.setErrors({ server: 'Email already exists.' });
    fixture.componentInstance.control.markAsTouched();
    fixture.changeDetectorRef.markForCheck();
    await fixture.whenStable();

    expect(errorText(fixture.nativeElement)).toBe('Email already exists.');
  });
});
