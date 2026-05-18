import { AbstractControl, ValidationErrors } from '@angular/forms';

const ALPHA_TEXT_PATTERN = /^(?!.*[\s'-]{2})[\p{L}]+(?:[ '-][\p{L}]+)*$/u;

/**
 * Validates letter-based text inputs.
 * Rules:
 * - Allows Unicode letters (including accented characters)
 * - Allows spaces, hyphens, and apostrophes as separators
 * - Rejects repeated separators (no double spaces, double hyphens, etc.)
 * - Rejects digits
 */
export function validAlphaText(control: AbstractControl): ValidationErrors | null {
  if (!control.value) {
    return null; // Let Validators.required handle empty values
  }
  const valid = ALPHA_TEXT_PATTERN.test(control.value);
  return valid ? null : { invalidAlphaText: true };
}
