import { Injectable, Self } from '@angular/core';
import { FormControl } from '@angular/forms';
import { combineLatest, merge, Observable } from 'rxjs';
import { map, mergeMap, publishReplay, refCount, takeUntil } from 'rxjs/operators';
import { NgDestroyer } from '../../../extensions';
import { TransitionsApi } from '../../transitions.api';
import { ParkingsFilter } from '../parkings-filter';
import { ParkingsFilterRouter } from '../parkings-filter-router';
import { ButtonStyles, TransitionButton } from './transition-button';

@Injectable()
export class TransitionButtonsStorage {
    public readonly buttons: Observable<readonly TransitionButton[]>;
    public readonly selectedButtons: Observable<readonly TransitionButton[]>;

    private readonly buttonStyles: ButtonStyles = {
        'StartedFree': { icon: 'check_circle', className: 'started' },
        'CompletedFree': { icon: 'check_circle', className: 'completed' },
        'CompletedFirst': { icon: 'attach_money', className: 'payed' },
    };

    constructor(
        @Self() private readonly destroyer$: NgDestroyer,
        private readonly parkingsFilterRouter: ParkingsFilterRouter,
        private readonly transitionApi: TransitionsApi,
    ) {
        this.buttons = this.getButtons$();
        this.selectedButtons = this.getSelectedButtons$();
        this.syncWithFilter();
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

    private syncWithFilter() {
        combineLatest([
            this.parkingsFilterRouter.filter,
            this.buttons,
        ]).pipe(
            takeUntil(this.destroyer$),
        ).subscribe(this.setButtonValues);
    }

    private setButtonValues = ([filter, transitionButtons]: [ParkingsFilter, readonly TransitionButton[]]) => {
        for (const button of transitionButtons) {
            const selected = filter.transitionNames.has(button.transitionName);
            button.control.setValue(selected);
        }
    }
}
