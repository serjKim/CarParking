import { ChangeDetectionStrategy, Component, HostListener, Input, OnInit } from '@angular/core';
import { assertUnhandledType } from '../../util';
import { CompletedFirst, CompletedFree, Parking, ParkingType, StartedFree } from '../models';

enum InfoType {
    Started,
    Completed,
}

@Component({
    selector: 'parkings-list-item',
    templateUrl: './parkings-list-item.component.html',
    styleUrls: ['./parkings-list-item.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent implements OnInit {
    public readonly intoType = InfoType;
    public selectedInfo = InfoType.Started;
    public canSwitchInfo = false;

    @Input()
    public parking!: Parking;

    @HostListener('click')
    public onSwitchInfo() {
        if (!this.canSwitchInfo) {
            return;
        }

        this.selectedInfo = this.selectedInfo === InfoType.Started
            ? InfoType.Completed
            : InfoType.Started;
    }

    public ngOnInit() {
        switch (this.parking.type) {
            case ParkingType.StartedFree:
                this.selectedInfo = InfoType.Started;
                break;
            case ParkingType.CompletedFirst:
            case ParkingType.CompletedFree:
                this.selectedInfo = InfoType.Completed;
                this.canSwitchInfo = true;
                break;
            default:
                assertUnhandledType(this.parking);
        }
    }

    public isStartedInfo(prk: Parking): prk is StartedFree {
        return this.selectedInfo === InfoType.Started && prk.type === ParkingType.StartedFree;
    }

    public isCompletedInfo(prk: Parking): prk is CompletedFirst | CompletedFree {
        return this.selectedInfo === InfoType.Completed
            && (prk.type === ParkingType.CompletedFirst || prk.type === ParkingType.CompletedFree);
    }
}
