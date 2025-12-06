import { effect, Signal, signal, WritableSignal } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { NotificationService } from '@adventureworks-web/shared/util';

/**
 * Shared form-status/error-display machinery for the three lifecycle wizard modals
 * (Hire/Terminate/Rehire) — extracted after a code review flagged this logic as duplicated
 * near-verbatim across all three. Bridges `form.valueChanges` into the signal graph so
 * `getErrors`/`groupValid` re-evaluate on every value change and on every `markAllAsTouched()`
 * call (which mutates `AbstractControl.touched` without emitting through `valueChanges`).
 */
export class WizardFormErrors {
  readonly submitted = signal(false);
  private readonly _formStatus: Signal<unknown>;
  private readonly _touchTick = signal(0);

  constructor(form: FormGroup) {
    this._formStatus = toSignal(form.valueChanges, { initialValue: form.value });
  }

  /** Call after every markAllAsTouched()/markAsTouched() so `getErrors`/`groupValid` re-evaluate. */
  bumpTouchTick(): void {
    this._touchTick.update((tick) => tick + 1);
  }

  /** True once `group` is valid — reactive to every form value change. */
  groupValid(group: AbstractControl): boolean {
    this._formStatus();
    return group.valid;
  }

  /** Validation error messages for a control, shown after submit or touch; always includes any server error. */
  getErrors(ctrl: AbstractControl, msgs: Partial<Record<string, string>>): Record<string, string> | null {
    this._formStatus();
    this._touchTick();
    if (!ctrl.errors || (!this.submitted() && !ctrl.touched)) {
      return null;
    }
    const result: Record<string, string> = {};
    for (const [key, msg] of Object.entries(msgs)) {
      if (ctrl.errors[key]) {
        result[key] = msg ?? key;
      }
    }
    return Object.keys(result).length ? result : null;
  }
}

/** Converts an errors map to an iterable list for template @for loops. */
export function errorsToList(errors: Record<string, string> | null): Array<[string, string]> {
  return errors ? (Object.entries(errors) as Array<[string, string]>) : [];
}

/**
 * `InputFieldComponent`'s CVA emits `''` (not `null`) when a user clears a text/number input,
 * so a bare `!= null` check would let `Number('')` (0) slip through for an optional field that
 * was intentionally cleared back to empty. Treats both `null` and `''` as "not provided".
 */
export function toOptionalNumber(value: number | string | null | undefined): number | undefined {
  return value === null || value === '' ? undefined : Number(value);
}

/**
 * Wires the shared "fire-and-forget rxMethod + shared isLoading/hasError signals" completion-
 * detection pattern used by all three lifecycle wizard modals. Must be called from a component
 * constructor (a reactive context) since it registers an `effect()`. Safe only because each modal
 * is the sole active consumer of the store's shared request-status signals at a time — if a second
 * concurrent lifecycle action is ever introduced, this must move to a per-request result instead.
 */
export function watchLifecycleActionCompletion(options: {
  isSubmitting: WritableSignal<boolean>;
  isLoading: Signal<boolean>;
  hasError: Signal<boolean>;
  notificationService: NotificationService;
  errorMessage: string;
  onSuccess: () => void;
}): void {
  effect(() => {
    const loading = options.isLoading();
    if (options.isSubmitting() && !loading) {
      options.isSubmitting.set(false);
      if (options.hasError()) {
        options.notificationService.error(options.errorMessage);
      } else {
        options.onSuccess();
      }
    }
  });
}
