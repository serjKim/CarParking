import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NotNullProperty } from '../../../../extensions';
import { CompletedFirst, CompletedFree, Parking, ParkingType } from '../../../models';

@Component({
    selector: 'complete-info',
    templateUrl: './completed-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompletedInfoComponent {
    @Input()
    @NotNullProperty()
    public set parking(_: Parking) {}

    public get info(): CompletedFirst | CompletedFree | null {
        return this.parking.type === ParkingType.CompletedFirst || this.parking.type === ParkingType.CompletedFree
            ? this.parking
            : null;
    }
}
