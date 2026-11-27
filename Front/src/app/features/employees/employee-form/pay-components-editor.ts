import { Component, input } from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';

import { FieldError } from '../../../shared/forms/field-error';
import { PayComponentForm, createPayComponentForm } from './employee-form.model';

/** Edits a list of named monthly amounts (allowances or deductions). */
@Component({
  selector: 'app-pay-components-editor',
  imports: [ReactiveFormsModule, FieldError],
  template: `
    <fieldset class="editor">
      <legend class="editor__title">{{ title() }}</legend>
      <p class="muted editor__hint">{{ hint() }}</p>

      @for (row of items().controls; track row; let index = $index) {
        <div class="editor__row" [formGroup]="row">
          <div class="field">
            <label class="field__label" [for]="idPrefix() + '-name-' + index">Name</label>
            <input class="input" [id]="idPrefix() + '-name-' + index" formControlName="name" placeholder="e.g. Transport" />
            <app-field-error [control]="row.controls.name" label="Name" />
          </div>
          <div class="field">
            <label class="field__label" [for]="idPrefix() + '-amount-' + index">Monthly amount</label>
            <input class="input" [id]="idPrefix() + '-amount-' + index" formControlName="amount" type="number" min="0" step="0.01" />
            <app-field-error [control]="row.controls.amount" label="Amount" />
          </div>
          <button type="button" class="btn btn--ghost editor__remove" (click)="items().removeAt(index)"
                  [attr.aria-label]="'Remove ' + (row.controls.name.value || 'row')">Remove</button>
        </div>
      } @empty {
        <p class="muted editor__empty">None.</p>
      }

      <button type="button" class="btn btn--ghost" (click)="items().push(newRow())">{{ addLabel() }}</button>
    </fieldset>
  `,
  styles: `
    .editor { margin: 0; padding: 0; border: 0; display: flex; flex-direction: column; gap: var(--space-3); align-items: flex-start; }
    .editor__title { padding: 0; font-weight: 600; }
    .editor__hint, .editor__empty { margin: 0; font-size: 0.875rem; }
    .editor__row { display: grid; grid-template-columns: 2fr 1fr auto; gap: var(--space-3); align-items: start; width: 100%; }
    .editor__remove { margin-top: 1.6rem; }
    @media (max-width: 640px) { .editor__row { grid-template-columns: 1fr; } .editor__remove { margin-top: 0; } }
  `,
})
export class PayComponentsEditor {
  readonly items = input.required<FormArray<PayComponentForm>>();
  readonly title = input.required<string>();
  readonly hint = input('');
  readonly addLabel = input('Add');
  readonly idPrefix = input.required<string>();

  protected newRow(): PayComponentForm {
    return createPayComponentForm();
  }
}
