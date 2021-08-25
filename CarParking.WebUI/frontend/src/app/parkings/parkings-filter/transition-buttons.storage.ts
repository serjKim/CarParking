import { Injectable, Self } from '@angular/core';
import { FormControl } from '@angular/forms';
import { merge, Observable } from 'rxjs';
import { map, mergeMap, publishReplay, refCount, takeUntil, tap } from 'rxjs/operators';
import { NgDestroyer } from '../../extensions';
import { ParkingsStorage } from '../parkings.storage';
import { TransitionsApi } from '../transitions.api';
import { ParkingsFilter } from './parking-filter';
import { ParkingsFilterRouter } from './parkings-filter-router';
import { ButtonStyles, TransitionButton } from './transition-button';

@Injectable()
export class TransitionButtonsStorage {
    public readonly transitionButtons$: Observable<readonly TransitionButton[]>;

    private readonly buttonStyles: ButtonStyles = {
        'StartedFree': { icon: 'check_circle', className: 'started' },
        'CompletedFree': { icon: 'check_circle', className: 'completed' },
        'CompletedFirst': { icon: 'attach_money', className: 'payed' },
    };

    constructor(
        @Self() private readonly destroyer$: NgDestroyer,
        private readonly parkingsStorage: ParkingsStorage,
        private readonly parkingsFilterRouter: ParkingsFilterRouter,
        transitionApi: TransitionsApi,
    ) {
        this.transitionButtons$ = transitionApi.getTransitions().pipe(
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

        this.applyNewFilterOnButtonClicks();
        this.loadStorage();
    }

    private applyNewFilterOnButtonClicks() {
        const buttonValues$ = this.transitionButtons$.pipe(
            map(transitionButtons => transitionButtons.map<Observable<void>>(btn => btn.control.valueChanges)),
        );

        buttonValues$.pipe(
            mergeMap(buttonValues => {
                return merge(...buttonValues).pipe(
                    mergeMap(() => this.transitionButtons$),
                    mergeMap(transitionButtons => {
                        const transitionNames = transitionButtons
                            .filter(x => !!x.control.value)
                            .map(x => x.transitionName);

                        return this.parkingsFilterRouter.filter.pipe(
                            map(currentFilter => currentFilter.setTransitionNames(new Set(transitionNames))),
                        );
                    }),
                );
            }),
            takeUntil(this.destroyer$),
        ).subscribe(filter => {
            this.parkingsFilterRouter.applyFilter(filter);
        });
    }

    private loadStorage() {
        const filter$ = this.transitionButtons$.pipe(
            mergeMap(transitionButtons => {
                return this.parkingsFilterRouter.filter
                    .pipe(
                        tap(this.setButtonValues(transitionButtons)),
                    );
            }),
        );

        this.parkingsStorage.loadStorage(filter$);
    }

    private setButtonValues(transitionButtons: readonly TransitionButton[]) {
        return (filter: ParkingsFilter) => {
            for (const button of transitionButtons) {
                const selected = filter.transitionNames.has(button.transitionName);
                button.control.setValue(selected);
            }
        };
    }
}
