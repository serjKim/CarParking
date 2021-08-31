import { Injectable } from '@angular/core';
import { FormControl } from '@angular/forms';
import { merge, Observable } from 'rxjs';
import { map, mergeMap, publishReplay, refCount } from 'rxjs/operators';
import { TransitionsApi } from '../../transitions.api';
import { ButtonStyles, TransitionButton } from './transition-button';

@Injectable({ providedIn: 'root' })
export class TransitionButtonsStorage {
    public readonly buttons: Observable<readonly TransitionButton[]>;
    public readonly selectedButtons: Observable<readonly TransitionButton[]>;

    private readonly buttonStyles: ButtonStyles = {
        'StartedFree': { icon: 'check_circle', className: 'started' },
        'CompletedFree': { icon: 'check_circle', className: 'completed' },
        'CompletedFirst': { icon: 'attach_money', className: 'payed' },
    };

    constructor(
        private readonly transitionApi: TransitionsApi,
    ) {
        this.buttons = this.getButtons$();
        this.selectedButtons = this.getSelectedButtons$();
    }

    private getButtons$() {
        return this.transitionApi.getTransitions().pipe(
            map(transitions => {
                return transitions.map(transition =>
                    new TransitionButton(
                        transition.name,
                        new FormControl(false),
                        this.buttonStyles[transition.name]));
            }),
            publishReplay(1),
            refCount(),
        );
    }

    private getSelectedButtons$() {
        const buttonValues$ = this.buttons.pipe(
            map(transitionButtons => transitionButtons.map<Observable<void>>(btn => btn.control.valueChanges)),
        );
        return buttonValues$.pipe(
            mergeMap(buttonValues => {
                return merge(...buttonValues).pipe(
                    mergeMap(() => this.buttons),
                    map(transitionButtons => {
                        const selectedButtons = transitionButtons
                            .filter(x => !!x.control.value)
                            .map(x => x);
                        return selectedButtons;
                    }),
                );
            }),
        );
    }
}
