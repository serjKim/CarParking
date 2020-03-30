import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'parkings',
    templateUrl: './parkings.component.html',
    styleUrls: ['./parkings.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsComponent { }
