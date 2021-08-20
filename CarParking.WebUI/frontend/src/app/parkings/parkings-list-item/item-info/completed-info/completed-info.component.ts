import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CompletedInfo } from './completed-info';

@Component({
    selector: 'complete-info',
    templateUrl: './completed-info.component.html',
    styleUrls: ['../item-info.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompletedInfoComponent {
    @Input()
    public info: Optional<CompletedInfo> = null;
}
