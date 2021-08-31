import { ChangeDetectionStrategy, Component, Input, Output } from '@angular/core';
import { Observable } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { TransitionButton } from './transition-button';
import { TransitionButtonsStorage } from './transition-buttons.storage';

@Component({
    selector: 'transition-buttons',
    templateUrl: './transition-buttons.component.html',
    styleUrls: ['./transition-buttons.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransitionButtonsComponent<RMap> {
    @Input()
    public selectedButtonsArgsMap: ((buttons: readonly TransitionButton[]) => Observable<RMap>) | null = null;

    @Output()
    public selectedButtons = this.transitionButtonsStorage.selectedButtons.pipe(
        filter(() => !!this.selectedButtonsArgsMap),
        switchMap(x => {
            if (!this.selectedButtonsArgsMap) {
                throw new Error('Expected mapping.');
            }
            return this.selectedButtonsArgsMap(x);
        }),
    );

    public readonly transitionButtons$ = this.transitionButtonsStorage.buttons;

    constructor(
        private readonly transitionButtonsStorage: TransitionButtonsStorage,
    ) { }
}
