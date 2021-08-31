import { ChangeDetectionStrategy, Component, Self } from '@angular/core';
import { map } from 'rxjs/operators';
import { ParkingsFilter } from './parkings-filter';
import { ParkingsFilterRouter } from './parkings-filter-router';
import { SyncButtonsWithFilter } from './sync-buttons-with-filter';
import { TransitionButton } from './transition-buttons';

@Component({
    selector: 'parkings-filter',
    templateUrl: './parkings-filter.component.html',
    styleUrls: ['./parkings-filter.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        SyncButtonsWithFilter,
    ],
})
export class ParkingsFilterComponent {
    constructor(
        private readonly parkingsFilterRouter: ParkingsFilterRouter,
        @Self() syncButtonsWithFilter: SyncButtonsWithFilter,
    ) {
        syncButtonsWithFilter.register();
    }

    public getUpdatedFilter = (buttons: readonly TransitionButton[]) => {
        const transitionNames = buttons.map(x => x.transitionName);
        return this.parkingsFilterRouter.filter.pipe(
            map(currentFilter => currentFilter.setTransitionNames(new Set(transitionNames))),
        );
    }

    public onSelectedButtons(filter: ParkingsFilter) {
        this.parkingsFilterRouter.applyFilter(filter);
    }
}
