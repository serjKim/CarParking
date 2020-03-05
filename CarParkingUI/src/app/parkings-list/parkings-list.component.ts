import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'parkings-list',
    templateUrl: './parkings-list.component.html',
    styleUrls: ['./parkings-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListComponent {
    constructor() { }
}
