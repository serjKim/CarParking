import { ChangeDetectionStrategy, Component, Input, Output, Self } from '@angular/core';
import { Observable } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { NgDestroyer, NotNullProperty } from '../../../extensions';
import { TransitionButton } from './transition-button';
import { TransitionButtonsStorage } from './transition-buttons.storage';

@Component({
    selector: 'transition-buttons',
    templateUrl: './transition-buttons.component.html',
    styleUrls: ['./transition-buttons.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        NgDestroyer,
        TransitionButtonsStorage,
    ],
})
export class TransitionButtonsComponent<RMap> {
    @Input()
    @NotNullProperty()
    public set selectedButtonsArgsMap(_: (buttons: readonly TransitionButton[]) => Observable<RMap>) {}

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
        @Self() private readonly transitionButtonsStorage: TransitionButtonsStorage,
    ) { }
}
