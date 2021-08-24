import { ChangeDetectionStrategy, Component, Self } from '@angular/core';
import { NgDestroyer } from '../../extensions';
import { ParkingFilterStorage } from './parkings-filter.storage';

@Component({
    selector: 'parkings-filter',
    templateUrl: './parkings-filter.component.html',
    styleUrls: ['./parkings-filter.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        NgDestroyer,
        ParkingFilterStorage,
    ],
})
export class ParkingsFilterComponent {
    public readonly transitionButtons$ = this.parkingsFilterStorage.transitionButtons$;

    constructor(
        @Self() private readonly parkingsFilterStorage: ParkingFilterStorage,
    ) { }
}
