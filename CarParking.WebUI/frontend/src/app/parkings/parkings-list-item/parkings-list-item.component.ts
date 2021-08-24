import { ChangeDetectionStrategy, Component, HostListener, Input } from '@angular/core';
import { notNullOrFail } from '../../extensions';
import { Parking } from '../models';
import { InfoType, SelectedInfo, selectedInfoByType } from './selected-info';

@Component({
    selector: 'parkings-list-item',
    templateUrl: './parkings-list-item.component.html',
    styleUrls: ['./parkings-list-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent {
    public readonly intoType = InfoType;
    public selectedInfo = SelectedInfo.started();
    private currentParking: Parking | null = null;

    @Input()
    public set parking(prk: Parking) {
        this.currentParking = prk;
        this.selectedInfo = selectedInfoByType[prk.type]();
    }

    public get parking(): Parking {
        return notNullOrFail(this.currentParking);
    }

    @HostListener('click')
    public onSwitchInfo() {
        this.selectedInfo.trySwitch();
    }
}
