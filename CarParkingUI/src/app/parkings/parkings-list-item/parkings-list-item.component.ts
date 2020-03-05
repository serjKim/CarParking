import { ChangeDetectionStrategy, Component, HostListener } from '@angular/core';

export const enum StatusType {
    Started,
    Completed,
}

@Component({
  selector: 'parkings-list-item',
  templateUrl: './parkings-list-item.component.html',
  styleUrls: ['./parkings-list-item.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingsListItemComponent {
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
