import { FormControl } from '@angular/forms';

import { currentIsoMonth, greaterThan, maxDecimalPlaces, notBlank, notInFuture, todayIsoDate } from './validators';

describe('form validators', () => {
  it('formats today and the current month in local time', () => {
    const date = new Date(2026, 2, 5); // 5 March 2026, local time

    expect(todayIsoDate(date)).toBe('2026-03-05');
    expect(currentIsoMonth(date)).toBe('2026-03');
  });

  describe('notInFuture', () => {
    const validator = notInFuture(() => '2026-03-15');

    it('allows today and past dates', () => {
      expect(validator(new FormControl('2026-03-15'))).toBeNull();
      expect(validator(new FormControl('2020-01-01'))).toBeNull();
    });

    it('rejects tomorrow', () => {
      expect(validator(new FormControl('2026-03-16'))).toEqual({ futureDate: true });
    });

    it('leaves empty values to Validators.required', () => {
      expect(validator(new FormControl(''))).toBeNull();
    });
  });

  describe('greaterThan(0)', () => {
    const validator = greaterThan(0);

    it.each([0, -5])('rejects %s', (value) => {
      expect(validator(new FormControl(value))).toEqual({ greaterThan: { limit: 0 } });
    });

    it('accepts the smallest positive amount', () => {
      expect(validator(new FormControl(0.01))).toBeNull();
    });
  });

  describe('maxDecimalPlaces(2)', () => {
    const validator = maxDecimalPlaces(2);

    it.each([100, 100.5, 100.25])('accepts %s', (value) => {
      expect(validator(new FormControl(value))).toBeNull();
    });

    it('rejects three decimals', () => {
      expect(validator(new FormControl(100.005))).toEqual({ decimalPlaces: { places: 2 } });
    });
  });

  describe('notBlank', () => {
    it('treats whitespace-only text as missing', () => {
      expect(notBlank(new FormControl('   '))).toEqual({ required: true });
      expect(notBlank(new FormControl('Sara'))).toBeNull();
    });
  });
});
