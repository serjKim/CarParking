import { ChangeDetectionStrategy, Component, Self } from '@angular/core';
import { NgDestroyer } from '../../extensions';
import { TransitionButtonsStorage } from './transition-buttons.storage';

@Component({
    selector: 'parkings-filter',
    templateUrl: './parkings-filter.component.html',
    styleUrls: ['./parkings-filter.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    providers: [
        NgDestroyer,
        TransitionButtonsStorage,
    ],
})
export class ParkingsFilterComponent {
    public readonly transitionButtons$ = this.transitionButtonsStorage.transitionButtons;

    constructor(
        @Self() private readonly transitionButtonsStorage: TransitionButtonsStorage,
    ) { }
}
