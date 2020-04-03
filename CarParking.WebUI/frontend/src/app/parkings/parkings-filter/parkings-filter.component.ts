import { ChangeDetectionStrategy, Component, Self } from '@angular/core';
import { FormControl } from '@angular/forms';
import { merge, Observable } from 'rxjs';
import { map, mergeMap, publishReplay, refCount, takeUntil, tap } from 'rxjs/operators';
import { NgDestroyer } from '../../util';
import { TransitionName } from '../models/transition';
import { ParkingsStorage } from '../parkings.storage';
import { TransitionsApi } from '../transitions.api';
import { ParkingsFilter } from './parking-filter';
import { ParkingsFilterStorage } from './parkings-filter.storage';

export class TransitionButton {
    public get buttonId(): string {
        return `${this.transitionName}-status-button`;
    }

    constructor(
        public readonly transitionName: TransitionName,
        public readonly control: FormControl,
        public readonly style: ButtonStyle,
    ) { }
}

interface ButtonStyle {
    readonly icon: string;
    readonly className: string;
}

type ButtonStyles = {
    readonly [key in TransitionName]: ButtonStyle;
};

@Component({
    selector: 'parkings-filter',
    templateUrl: './parkings-filter.component.html',
    styleUrls: ['./parkings-filter.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        NgDestroyer,
    ],
})
export class ParkingsFilterComponent {
    public readonly transitionButtons$: Observable<readonly TransitionButton[]>;
    private readonly buttonStyles: ButtonStyles = {
        'StartedFree': { icon: 'check_circle', className: 'started' },
        'CompletedFree': { icon: 'check_circle', className: 'completed' },
        'CompletedFirst': { icon: 'attach_money', className: 'payed' },
    };

    constructor(
        @Self() private readonly destroyer$: NgDestroyer,
        private readonly parkingsStorage: ParkingsStorage,
        private readonly parkingsFilterStorage: ParkingsFilterStorage,
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

        this.applyNewFilterOnValueChanges();
        this.loadStorage();
    }

    private applyNewFilterOnValueChanges() {
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

                        return this.parkingsFilterStorage.filter.pipe(
                            map(currentFilter => currentFilter.setTransitionNames(new Set(transitionNames))),
                        );
                    }),
                );
            }),
            takeUntil(this.destroyer$),
        ).subscribe(filter => {
            this.parkingsFilterStorage.applyFilter(filter);
        });
    }

    private loadStorage() {
        const filter$ = this.transitionButtons$.pipe(
            mergeMap(transitionButtons => {
                return this.parkingsFilterStorage.filter
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
