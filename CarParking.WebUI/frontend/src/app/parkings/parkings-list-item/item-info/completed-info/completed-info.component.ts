import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { notNullOrFail } from '../../../../extensions';
import { CompletedFirst, CompletedFree, Parking, ParkingType } from '../../../models';

@Component({
    selector: 'complete-info',
    templateUrl: './completed-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompletedInfoComponent {
    private currentParking: CompletedFirst | CompletedFree | null = null;

    @Input()
    public set parking(prk: Parking) {
        if (prk.type !== ParkingType.CompletedFirst && prk.type !== ParkingType.CompletedFree) {
            throw new Error('Expected completed.');
        }
    }

    public get info(): CompletedFirst | CompletedFree {
        return notNullOrFail(this.currentParking);
    }
}
