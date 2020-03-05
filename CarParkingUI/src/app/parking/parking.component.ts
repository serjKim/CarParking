import { ChangeDetectionStrategy, Component, HostListener } from '@angular/core';

export const enum StatusType {
    Started,
    Completed,
}

@Component({
  selector: 'parking',
  templateUrl: './parking.component.html',
  styleUrls: ['./parking.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingComponent {
    public readonly startedStatus = StatusType.Started;
    public readonly completedStatus = StatusType.Completed;
    public selectedStatus = StatusType.Started;

    @HostListener('click')
    public onChangeStatus() {
        this.selectedStatus = this.selectedStatus === StatusType.Started
            ? StatusType.Completed
            : StatusType.Started;
    }

    public onComplete(e: MouseEvent) {
        e.stopPropagation();
    }
}
