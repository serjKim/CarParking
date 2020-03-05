import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
    selector: 'create-parking-button',
    templateUrl: './create-parking-button.component.html',
    styleUrls: ['./create-parking-button.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateParkingButtonComponent {
    constructor() { }
}
