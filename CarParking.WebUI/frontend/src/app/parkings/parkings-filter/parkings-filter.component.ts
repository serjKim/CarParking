import { ChangeDetectionStrategy, Component, Self } from '@angular/core';
import { FormControl } from '@angular/forms';
import { merge, Observable } from 'rxjs';
import { map, mergeMap, takeUntil, tap } from 'rxjs/operators';
import { NgDestroyer } from '../../util';
import { PARKING_TYPE_KEYS, ParkingTypeKey } from '../models/parking';
import { ParkingsStorage } from '../parkings.storage';
import { ParkingsFilter } from './parking-filter';
import { ParkingsFilterStorage } from './parkings-filter.storage';

export class ParkingTypeButton {
    public get buttonName(): string {
        return `${this.parkingTypeKey}-status-button`;
    }

    constructor(
        public readonly parkingTypeKey: ParkingTypeKey,
        public readonly control: FormControl,
        public readonly style: ButtonStyle,
    ) { }
}

interface ButtonStyle {
    readonly icon: string;
    readonly className: string;
}

type ButtonStyles = {
    readonly [key in ParkingTypeKey]: ButtonStyle;
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
    public readonly typeButtons: readonly ParkingTypeButton[];
    private readonly buttonStyles: ButtonStyles = {
        StartedFree: { icon: 'check_circle', className: 'started' },
        CompletedFree: { icon: 'check_circle', className: 'completed' },
        CompletedFirst: { icon: 'attach_money', className: 'payed' },
    };

    constructor(
        @Self() private readonly destroyer$: NgDestroyer,
        private readonly parkingsStorage: ParkingsStorage,
        private readonly parkingsFilterStorage: ParkingsFilterStorage,
    ) {
        this.typeButtons = Array
            .from(PARKING_TYPE_KEYS)
            .map(parkingType =>
                new ParkingTypeButton(
                    parkingType,
                    new FormControl(false),
                    this.buttonStyles[parkingType]));

        this.applyNewFilterOnValueChanges();
        this.loadStorage();
    }

    private applyNewFilterOnValueChanges() {
        const buttonValues$ = this.typeButtons.map<Observable<void>>(btn => btn.control.valueChanges);

        merge(...buttonValues$).pipe(
            mergeMap(() => {
                const parkingTypeKeys = this.typeButtons
                    .filter(x => !!x.control.value)
                    .map(x => x.parkingTypeKey);

                return this.parkingsFilterStorage.filter.pipe(
                    map(currentFilter => currentFilter.setParkingTypes(new Set(parkingTypeKeys))),
                );
            }),
            takeUntil(this.destroyer$),
        ).subscribe(newFilter => {
            this.parkingsFilterStorage.applyFilter(newFilter);
        });
    }

    private loadStorage() {
        const filter$ = this.parkingsFilterStorage.filter
            .pipe(
                tap(this.setButtonValues),
            );

        this.parkingsStorage.loadStorage(filter$);
    }

    private setButtonValues = (filter: ParkingsFilter) => {
        for (const button of this.typeButtons) {
            const selected = filter.parkingTypeKeys.has(button.parkingTypeKey);
            button.control.setValue(selected);
        }
    }
}
